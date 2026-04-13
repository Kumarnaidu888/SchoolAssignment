using SchoolApplication.Contracts.Users;

namespace SchoolApplication.Services;

public interface IUserManagementService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserResponse> GetByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> CreateTeacherAccountAsync(CreateTeacherAccountRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> ReplaceRolesAsync(int userId, ReplaceUserRolesRequest request, CancellationToken cancellationToken = default);
}
