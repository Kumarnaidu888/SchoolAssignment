namespace SchoolApplication.Contracts.Marks;

public sealed record MarkSubmissionAcceptedResponse(long JobId, string Status, string Message);
