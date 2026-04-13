namespace SchoolApplication.Contracts.Users;

public sealed record UserResponse(
    int UserId,
    string UserName,
    string? Email,
    bool IsActive,
    IReadOnlyList<string> RoleNames,
    DateTime? CreatedAtUtc);
