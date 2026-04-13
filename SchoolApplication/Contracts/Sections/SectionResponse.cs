namespace SchoolApplication.Contracts.Sections;

public sealed record SectionResponse(int SectionId, int ClassId, string Name, DateTime? CreatedAtUtc);
