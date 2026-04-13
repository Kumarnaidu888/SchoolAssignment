using Microsoft.EntityFrameworkCore;
using SchoolApplication.Contracts.Auth;
using SchoolApplication.Exceptions;
using SchoolApplication.Models;

namespace SchoolApplication.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly SchoolAssessmentContext _db;
    private readonly ITokenIssuer _tokens;
    private readonly ILogger<AuthService> _logger;
    private readonly Microsoft.Extensions.Options.IOptions<SchoolApplication.Options.JwtOptions> _jwtOptions;

    public AuthService(
        SchoolAssessmentContext db,
        ITokenIssuer tokens,
        ILogger<AuthService> logger,
        Microsoft.Extensions.Options.IOptions<SchoolApplication.Options.JwtOptions> jwtOptions)
    {
        _db = db;
        _tokens = tokens;
        _logger = logger;
        _jwtOptions = jwtOptions;
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalized = request.UserName.Trim().ToUpperInvariant();
        var user = await _db.AppUsers
            .Include(u => u.Roles)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.NormalizedUserName == normalized, cancellationToken);

        if (user is null || user.IsActive != true)
        {
            _logger.LogWarning("Login failed for user {UserName}: not found or inactive", request.UserName);
            throw new AuthenticationException("Invalid username or password.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed for user {UserName}: bad password", request.UserName);
            throw new AuthenticationException("Invalid username or password.");
        }

        var roles = user.Roles.Select(r => r.RoleName).ToList();
        var (access, accessExpires) = _tokens.CreateAccessToken(user, roles);
        var refreshPlain = _tokens.CreateRefreshTokenValue();
        var refreshHash = _tokens.HashRefreshToken(refreshPlain);
        var refreshExpires = DateTime.UtcNow.AddDays(_jwtOptions.Value.RefreshTokenDays);

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.UserId,
            TokenHash = refreshHash,
            ExpiresAtUtc = refreshExpires
        });
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} logged in", user.UserId);

        return new TokenResponse(
            access,
            (int)(accessExpires - DateTime.UtcNow).TotalSeconds,
            refreshPlain,
            refreshExpires);
    }

    public async Task<TokenResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var hash = _tokens.HashRefreshToken(request.RefreshToken);
        var existing = await _db.RefreshTokens
            .Include(t => t.User)
            .ThenInclude(u => u.Roles)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (existing is null || existing.RevokedAtUtc != null || existing.ExpiresAtUtc < DateTime.UtcNow)
            throw new AuthenticationException("Invalid or expired refresh token.");

        var user = existing.User;
        if (user.IsActive != true)
            throw new AuthenticationException("User is inactive.");

        existing.RevokedAtUtc = DateTime.UtcNow;

        var roles = user.Roles.Select(r => r.RoleName).ToList();
        var (access, accessExpires) = _tokens.CreateAccessToken(user, roles);
        var refreshPlain = _tokens.CreateRefreshTokenValue();
        var newHash = _tokens.HashRefreshToken(refreshPlain);
        var refreshExpires = DateTime.UtcNow.AddDays(_jwtOptions.Value.RefreshTokenDays);

        var newRow = new RefreshToken
        {
            UserId = user.UserId,
            TokenHash = newHash,
            ExpiresAtUtc = refreshExpires,
            ReplacedById = null
        };
        _db.RefreshTokens.Add(newRow);
        await _db.SaveChangesAsync(cancellationToken);

        existing.ReplacedById = newRow.RefreshTokenId;
        await _db.SaveChangesAsync(cancellationToken);

        return new TokenResponse(
            access,
            (int)(accessExpires - DateTime.UtcNow).TotalSeconds,
            refreshPlain,
            refreshExpires);
    }

    public async Task LogoutAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var hash = _tokens.HashRefreshToken(request.RefreshToken);
        var existing = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
        if (existing is null)
            return;

        existing.RevokedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Refresh token revoked for user {UserId}", existing.UserId);
    }
}
