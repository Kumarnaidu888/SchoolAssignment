namespace SchoolApplication.Domain;

public static class RankingCalculator
{
    /// <summary>Competition ranking: ties share rank; next rank skips (e.g. 1, 2, 2, 4).</summary>
    public static IReadOnlyList<RankedScore> ApplyCompetitionRanking(IReadOnlyList<(int StudentId, decimal TotalScore)> totals)
    {
        if (totals.Count == 0)
            return [];

        var ordered = totals
            .OrderByDescending(x => x.TotalScore)
            .ThenBy(x => x.StudentId)
            .ToList();

        var result = new List<RankedScore>(ordered.Count);
        var position = 1;
        var i = 0;
        while (i < ordered.Count)
        {
            var score = ordered[i].TotalScore;
            var tieCount = 1;
            while (i + tieCount < ordered.Count && ordered[i + tieCount].TotalScore == score)
                tieCount++;

            for (var j = 0; j < tieCount; j++)
            {
                result.Add(new RankedScore(
                    ordered[i + j].StudentId,
                    ordered[i + j].TotalScore,
                    Rank: position,
                    TieSize: tieCount));
            }

            position += tieCount;
            i += tieCount;
        }

        return result;
    }

    public sealed record RankedScore(int StudentId, decimal TotalScore, int Rank, int TieSize);
}
