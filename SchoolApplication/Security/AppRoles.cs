namespace SchoolApplication.Security;

/// <summary>Must match <c>auth.Roles.RoleName</c> values in the database.</summary>
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";
}
