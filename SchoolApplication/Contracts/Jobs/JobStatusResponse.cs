namespace SchoolApplication.Contracts.Jobs;

public sealed record JobStatusResponse(
    long JobId,
    string Status,
    int RetryCount,
    int? MaxRetries,
    DateTime? NextAttemptAtUtc,
    string? LastError,
    DateTime? CreatedAtUtc);
