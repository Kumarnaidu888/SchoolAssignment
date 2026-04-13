namespace SchoolApplication.Contracts.Students;

public sealed record StudentResponse(
    int StudentId,
    int SectionId,
    string FullName,
    string? AdmissionNo,
    int? UserId,
    DateTime? CreatedAtUtc);
