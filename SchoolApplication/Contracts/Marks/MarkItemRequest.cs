namespace SchoolApplication.Contracts.Marks;

public sealed record MarkItemRequest(int StudentId, int SubjectId, decimal? Score);
