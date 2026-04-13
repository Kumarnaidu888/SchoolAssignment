using SchoolApplication.Contracts.Marks;

namespace SchoolApplication.Services;

public interface IMarkSubmissionService
{
    Task<MarkSubmissionAcceptedResponse> SubmitAsync(
        string idempotencyKey,
        SubmitMarksRequest request,
        int submittedByUserId,
        bool submitterIsAdmin,
        CancellationToken cancellationToken = default);
}
