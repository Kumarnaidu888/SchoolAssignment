using SchoolApplication.Contracts.Marks;

namespace SchoolApplication.Services;

public interface ISectionMarkReadService
{
    Task<IReadOnlyList<SectionStudentMarksResponse>> GetSectionMarksAsync(int sectionId, int? examId, CancellationToken cancellationToken = default);
}
