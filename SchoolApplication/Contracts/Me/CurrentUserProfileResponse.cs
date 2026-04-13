namespace SchoolApplication.Contracts.Me;

/// <summary>Server-side profile for any authenticated user. Student portal fields are null when no <c>Students.UserId</c> link exists.</summary>
public sealed record CurrentUserProfileResponse(
    int UserId,
    string UserName,
    string? Email,
    bool IsActive,
    IReadOnlyList<string> RoleNames,
    StudentPortalLinkSummary? StudentPortal);

/// <summary>Present when this user account is linked to a <c>Students</c> row (<c>Students.UserId</c>).</summary>
public sealed record StudentPortalLinkSummary(
    int StudentId,
    string FullName,
    int SectionId,
    string SectionName,
    int ClassId,
    string ClassName);
