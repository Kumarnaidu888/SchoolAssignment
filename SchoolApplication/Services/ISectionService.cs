using SchoolApplication.Contracts.Sections;

namespace SchoolApplication.Services;

public interface ISectionService
{
    Task<IReadOnlyList<SectionResponse>> GetByClassIdAsync(int classId, CancellationToken cancellationToken = default);
    Task<SectionResponse> GetByIdAsync(int sectionId, CancellationToken cancellationToken = default);
    Task<SectionResponse> CreateAsync(int classId, CreateSectionRequest request, CancellationToken cancellationToken = default);
    Task<SectionResponse> UpdateAsync(int sectionId, UpdateSectionRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int sectionId, CancellationToken cancellationToken = default);
}
