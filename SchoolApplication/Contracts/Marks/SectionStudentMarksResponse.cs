namespace SchoolApplication.Contracts.Marks;

public sealed record SectionStudentMarksResponse(
    int StudentId,
    string StudentName,
    IReadOnlyList<StudentMarkRowResponse> Marks);
