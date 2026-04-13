namespace SchoolApplication.Contracts.Teachers;

public sealed record ReplaceTeacherSectionsRequest(IReadOnlyList<int> SectionIds);
