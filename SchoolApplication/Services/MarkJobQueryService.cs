using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Contracts.Jobs;
using SchoolApplication.Exceptions;
using SchoolApplication.Models;
using SchoolApplication.Processing;

namespace SchoolApplication.Services;

public sealed class MarkJobQueryService : IMarkJobQueryService
{
    private readonly SchoolAssessmentContext _db;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public MarkJobQueryService(SchoolAssessmentContext db) => _db = db;

    public async Task<JobStatusResponse> GetJobAsync(long jobId, int requestingUserId, bool requesterIsAdmin, CancellationToken cancellationToken = default)
    {
        var job = await _db.MarkProcessingJobs.AsNoTracking().FirstOrDefaultAsync(j => j.JobId == jobId, cancellationToken);
        if (job is null)
            throw new NotFoundException($"Job with id {jobId} was not found.");

        if (!requesterIsAdmin)
        {
            var payload = JsonSerializer.Deserialize<MarkSubmissionPayload>(job.PayloadJson, JsonOptions);
            if (payload?.SubmittedByUserId != requestingUserId)
                throw new ForbiddenException("You can only view jobs you submitted.");
        }

        return new JobStatusResponse(
            job.JobId,
            job.Status!,
            job.RetryCount ?? 0,
            job.MaxRetries,
            job.NextAttemptAtUtc,
            job.LastError,
            job.CreatedAtUtc);
    }
}
