using SchoolApplication.Contracts.Students;

namespace SchoolApplication.Services;

public interface IStudentService
{
    Task<IReadOnlyList<StudentResponse>> GetBySectionIdAsync(int sectionId, CancellationToken cancellationToken = default);
    Task<StudentResponse> GetByIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<StudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);
    Task<StudentResponse> UpdateAsync(int studentId, UpdateStudentRequest request, CancellationToken cancellationToken = default);
    Task<StudentResponse> LinkStudentUserAsync(int studentId, LinkStudentUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int studentId, CancellationToken cancellationToken = default);
}
