using Microsoft.EntityFrameworkCore;
using SchoolApplication.Contracts.Users;
using SchoolApplication.Exceptions;
using SchoolApplication.Models;
using SchoolApplication.Security;

namespace SchoolApplication.Services;

public sealed class UserManagementService : IUserManagementService
{
    private static readonly HashSet<string> AllowedRoles =
    [
        AppRoles.Admin,
        AppRoles.Teacher,
        AppRoles.Student
    ];

    private readonly SchoolAssessmentContext _db;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(SchoolAssessmentContext db, ILogger<UserManagementService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.AppUsers
            .AsNoTracking()
            .Include(u => u.Roles)
            .OrderBy(u => u.UserName)
            .ToListAsync(cancellationToken);

        return list
            .Select(u => new UserResponse(
                u.UserId,
                u.UserName,
                u.Email,
                u.IsActive == true,
                u.Roles.Select(r => r.RoleName).OrderBy(n => n).ToList(),
                u.CreatedAtUtc))
            .ToList();
    }

    public async Task<UserResponse> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.AppUsers
            .AsNoTracking()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (entity is null)
            throw new NotFoundException($"User with id {userId} was not found.");

        return new UserResponse(
            entity.UserId,
            entity.UserName,
            entity.Email,
            entity.IsActive == true,
            entity.Roles.Select(r => r.RoleName).OrderBy(n => n).ToList(),
            entity.CreatedAtUtc);
    }

    public Task<UserResponse> CreateTeacherAccountAsync(CreateTeacherAccountRequest request, CancellationToken cancellationToken = default) =>
        CreateUserAsync(
            new CreateUserRequest(request.UserName, request.Email, request.Password, [AppRoles.Teacher]),
            cancellationToken);

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRoleNames(request.Roles);

        var normalized = request.UserName.Trim().ToUpperInvariant();
        if (await _db.AppUsers.AnyAsync(u => u.NormalizedUserName == normalized, cancellationToken))
            throw new ConflictException($"Username '{request.UserName.Trim()}' is already taken.");

        var roles = await LoadRolesAsync(request.Roles, cancellationToken);

        var entity = new AppUser
        {
            UserName = request.UserName.Trim(),
            NormalizedUserName = normalized,
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true
        };

        foreach (var role in roles)
            entity.Roles.Add(role);

        _db.AppUsers.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        await _db.Entry(entity).ReloadAsync(cancellationToken);

        _logger.LogInformation("Created user {UserId} ({UserName}) with roles: {Roles}",
            entity.UserId, entity.UserName, string.Join(", ", request.Roles));

        return await GetByIdAsync(entity.UserId, cancellationToken);
    }

    public async Task<UserResponse> ReplaceRolesAsync(int userId, ReplaceUserRolesRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRoleNames(request.RoleNames);

        var user = await _db.AppUsers
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user is null)
            throw new NotFoundException($"User with id {userId} was not found.");

        var roles = await LoadRolesAsync(request.RoleNames, cancellationToken);

        user.Roles.Clear();
        foreach (var role in roles)
            user.Roles.Add(role);

        user.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated roles for user {UserId}", userId);
        return await GetByIdAsync(userId, cancellationToken);
    }

    private static void ValidateRoleNames(IReadOnlyList<string> roleNames)
    {
        if (roleNames.Count == 0)
            throw new ConflictException("At least one role is required.");

        foreach (var name in roleNames)
        {
            if (string.IsNullOrWhiteSpace(name) || !AllowedRoles.Contains(name.Trim()))
                throw new ConflictException($"Role '{name}' is not allowed. Use: {string.Join(", ", AllowedRoles)}.");
        }
    }

    private async Task<List<Role>> LoadRolesAsync(IReadOnlyList<string> roleNames, CancellationToken cancellationToken)
    {
        var distinct = roleNames.Select(r => r.Trim()).Distinct(StringComparer.Ordinal).ToList();
        var roles = await _db.Roles.Where(r => distinct.Contains(r.RoleName)).ToListAsync(cancellationToken);
        if (roles.Count != distinct.Count)
        {
            var missing = distinct.Except(roles.Select(r => r.RoleName), StringComparer.Ordinal).ToList();
            throw new NotFoundException($"Unknown role(s): {string.Join(", ", missing)}. Ensure auth.Roles is seeded.");
        }

        return roles;
    }
}
