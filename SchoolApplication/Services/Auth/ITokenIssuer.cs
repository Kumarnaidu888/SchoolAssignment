using SchoolApplication.Models;

namespace SchoolApplication.Services.Auth;

public interface ITokenIssuer
{
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(AppUser user, IReadOnlyList<string> roleNames);
    string CreateRefreshTokenValue();
    byte[] HashRefreshToken(string refreshToken);
}
