using SchoolApplication.Contracts.Rankings;

namespace SchoolApplication.Services;

public interface IRankingQueryService
{
    Task<RankingSnapshotResponse> GetSectionRankingAsync(int sectionId, int examId, CancellationToken cancellationToken = default);
    Task<RankingSnapshotResponse> GetClassRankingAsync(int classId, int examId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RankingRowResponse>> GetTopNAsync(string scope, int scopeId, int examId, int n, CancellationToken cancellationToken = default);
}
