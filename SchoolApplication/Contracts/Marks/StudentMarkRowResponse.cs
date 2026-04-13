namespace SchoolApplication.Contracts.Marks;

public sealed record StudentMarkRowResponse(
    int SubjectId,
    string SubjectName,
    int ExamId,
    string ExamDisplayName,
    decimal? Score,
    DateTime? UpdatedAtUtc);
