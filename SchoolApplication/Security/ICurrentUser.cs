namespace SchoolApplication.Security;

public interface ICurrentUser
{
    int? UserId { get; }
    string? UserName { get; }
    IReadOnlyList<string> RoleNames { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
