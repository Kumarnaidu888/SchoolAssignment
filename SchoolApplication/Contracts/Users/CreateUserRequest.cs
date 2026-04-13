namespace SchoolApplication.Contracts.Users;

/// <summary>Create a login account with one or more roles (Admin, Teacher, Student).</summary>
public sealed record CreateUserRequest(string UserName, string? Email, string Password, IReadOnlyList<string> Roles);
