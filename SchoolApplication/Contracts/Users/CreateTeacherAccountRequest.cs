namespace SchoolApplication.Contracts.Users;

/// <summary>Convenience payload for creating a teacher login (same as <see cref="CreateUserRequest"/> with role Teacher).</summary>
public sealed record CreateTeacherAccountRequest(string UserName, string? Email, string Password);
