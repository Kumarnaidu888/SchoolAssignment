namespace SchoolApplication.Contracts.Students;

public sealed record UpdateStudentRequest(
    int SectionId,
    string FullName,
    string? AdmissionNo,
    int? UserId);
