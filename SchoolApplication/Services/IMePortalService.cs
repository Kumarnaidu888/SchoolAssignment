using SchoolApplication.Contracts.Marks;
using SchoolApplication.Contracts.Rankings;

namespace SchoolApplication.Services;

public interface IMePortalService
{
    Task<IReadOnlyList<StudentMarkRowResponse>> GetMyMarksAsync(int userId, int? examId, CancellationToken cancellationToken = default);
    Task<RankingRowResponse?> GetMySectionRankingAsync(int userId, int examId, CancellationToken cancellationToken = default);
}
