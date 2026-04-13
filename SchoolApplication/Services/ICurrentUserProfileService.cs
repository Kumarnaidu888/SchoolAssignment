using SchoolApplication.Contracts.Me;

namespace SchoolApplication.Services;

public interface ICurrentUserProfileService
{
    Task<CurrentUserProfileResponse> GetProfileAsync(int userId, CancellationToken cancellationToken = default);
}
