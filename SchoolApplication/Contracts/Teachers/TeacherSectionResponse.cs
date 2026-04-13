namespace SchoolApplication.Contracts.Teachers;

public sealed record TeacherSectionResponse(int TeacherUserId, int SectionId, DateTime? AssignedAtUtc);
