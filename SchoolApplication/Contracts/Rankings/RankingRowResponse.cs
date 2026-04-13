namespace SchoolApplication.Contracts.Rankings;

public sealed record RankingRowResponse(
    int StudentId,
    string StudentName,
    decimal TotalScore,
    int Rank,
    int TieSize);
