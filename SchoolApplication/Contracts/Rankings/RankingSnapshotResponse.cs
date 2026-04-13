namespace SchoolApplication.Contracts.Rankings;

public sealed record RankingSnapshotResponse(
    long SnapshotId,
    int ExamId,
    string Scope,
    int ScopeId,
    DateTime? ComputedAtUtc,
    IReadOnlyList<RankingRowResponse> Rows);
