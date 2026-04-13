namespace SchoolApplication.Contracts.Students;

/// <summary>Admin: attach a login user (must have Student role) to an existing student record for <c>/api/me/marks</c> and <c>/api/me/rankings</c>.</summary>
public sealed record LinkStudentUserRequest(int UserId);
