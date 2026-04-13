namespace SchoolApplication.Contracts.Auth;

public sealed record TokenResponse(
    string AccessToken,
    int ExpiresInSeconds,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    string TokenType = "Bearer");
