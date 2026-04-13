using SchoolApplication.Contracts.Classes;

namespace SchoolApplication.Services;

public interface IClassService
{
    Task<IReadOnlyList<ClassResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClassResponse> GetByIdAsync(int classId, CancellationToken cancellationToken = default);
    Task<ClassResponse> CreateAsync(CreateClassRequest request, CancellationToken cancellationToken = default);
    Task<ClassResponse> UpdateAsync(int classId, UpdateClassRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int classId, CancellationToken cancellationToken = default);
}
