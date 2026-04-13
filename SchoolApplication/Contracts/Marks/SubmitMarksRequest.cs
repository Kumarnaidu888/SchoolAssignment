namespace SchoolApplication.Contracts.Marks;

public sealed record SubmitMarksRequest(int ExamId, IReadOnlyList<MarkItemRequest> Marks);
