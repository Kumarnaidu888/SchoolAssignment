using SchoolApplication.Contracts.Reference;

namespace SchoolApplication.Services;

public interface IReferenceDataService
{
    Task<IReadOnlyList<SubjectResponse>> GetSubjectsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamResponse>> GetExamsAsync(CancellationToken cancellationToken = default);
}
