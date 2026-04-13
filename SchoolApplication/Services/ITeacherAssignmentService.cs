using SchoolApplication.Contracts.Teachers;

namespace SchoolApplication.Services;

public interface ITeacherAssignmentService
{
    Task<IReadOnlyList<TeacherSectionResponse>> GetAssignmentsForTeacherAsync(int teacherUserId, CancellationToken cancellationToken = default);
    Task ReplaceAssignmentsAsync(int teacherUserId, IReadOnlyList<int> sectionIds, CancellationToken cancellationToken = default);
}
