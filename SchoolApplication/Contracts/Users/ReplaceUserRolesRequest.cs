namespace SchoolApplication.Contracts.Users;

public sealed record ReplaceUserRolesRequest(IReadOnlyList<string> RoleNames);
