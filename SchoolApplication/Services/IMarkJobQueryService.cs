using SchoolApplication.Contracts.Jobs;

namespace SchoolApplication.Services;

public interface IMarkJobQueryService
{
    Task<JobStatusResponse> GetJobAsync(long jobId, int requestingUserId, bool requesterIsAdmin, CancellationToken cancellationToken = default);
}
