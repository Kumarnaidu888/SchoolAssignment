using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Domain;
using SchoolApplication.Models;
using SchoolApplication.Processing;

namespace SchoolApplication.Services.Processing;

public sealed class MarkJobProcessor : IMarkJobProcessor
{
    private readonly SchoolAssessmentContext _db;
    private readonly ILogger<MarkJobProcessor> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public MarkJobProcessor(SchoolAssessmentContext db, ILogger<MarkJobProcessor> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task ProcessNextAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var job = await _db.MarkProcessingJobs
            .Where(j => j.Status == "Pending" && j.NextAttemptAtUtc <= now &&
                        (j.LockedUntilUtc == null || j.LockedUntilUtc < now))
            .OrderBy(j => j.JobId)
            .FirstOrDefaultAsync(cancellationToken);

        if (job is null)
            return;

        job.Status = "Processing";
        job.LockedUntilUtc = now.AddMinutes(5);
        var attemptNo = (job.RetryCount ?? 0) + 1;
        await _db.JobAttemptLogs.AddAsync(new JobAttemptLog
        {
            JobId = job.JobId,
            AttemptNumber = attemptNo,
            Outcome = "Started"
        }, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            await ProcessPayloadInTransactionAsync(job, cancellationToken);

            job.Status = "Completed";
            job.LockedUntilUtc = null;
            job.LastError = null;

            await _db.JobAttemptLogs.AddAsync(new JobAttemptLog
            {
                JobId = job.JobId,
                AttemptNumber = attemptNo,
                Outcome = "Succeeded"
            }, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Marks job {JobId} completed", job.JobId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Marks job {JobId} attempt {Attempt} failed", job.JobId, attemptNo);

            job.RetryCount = attemptNo;
            job.LastError = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
            job.LockedUntilUtc = null;

            var maxRetries = job.MaxRetries ?? 3;
            if (attemptNo >= maxRetries)
            {
                job.Status = "DeadLetter";
                job.NextAttemptAtUtc = null;
            }
            else
            {
                job.Status = "Pending";
                var delaySeconds = Math.Min(300, (int)Math.Pow(2, attemptNo));
                job.NextAttemptAtUtc = DateTime.UtcNow.AddSeconds(delaySeconds);
            }

            await _db.JobAttemptLogs.AddAsync(new JobAttemptLog
            {
                JobId = job.JobId,
                AttemptNumber = attemptNo,
                Outcome = attemptNo >= maxRetries ? "Failed" : "RetryScheduled"
            }, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task ProcessPayloadInTransactionAsync(MarkProcessingJob job, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<MarkSubmissionPayload>(job.PayloadJson, JsonOptions)
            ?? throw new InvalidOperationException("Invalid job payload.");

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var line in payload.Marks)
            {
                var mark = await _db.Marks.FirstOrDefaultAsync(
                    m => m.StudentId == line.StudentId && m.SubjectId == line.SubjectId && m.ExamId == payload.ExamId,
                    cancellationToken);

                if (mark is null)
                {
                    mark = new Mark
                    {
                        StudentId = line.StudentId,
                        SubjectId = line.SubjectId,
                        ExamId = payload.ExamId,
                        Score = line.Score,
                        EnteredByUserId = payload.SubmittedByUserId,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };
                    _db.Marks.Add(mark);
                }
                else
                {
                    mark.Score = line.Score;
                    mark.EnteredByUserId = payload.SubmittedByUserId;
                    mark.UpdatedAtUtc = DateTime.UtcNow;
                }
            }

            await _db.SaveChangesAsync(cancellationToken);

            var affectedStudentIds = payload.Marks.Select(m => m.StudentId).Distinct().ToList();
            var sectionIds = await _db.Students
                .Where(s => affectedStudentIds.Contains(s.StudentId))
                .Select(s => s.SectionId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var classIds = await _db.Sections
                .Where(s => sectionIds.Contains(s.SectionId))
                .Select(s => s.ClassId)
                .Distinct()
                .ToListAsync(cancellationToken);

            foreach (var sectionId in sectionIds)
                await RecomputeRankingAsync(payload.ExamId, "Section", sectionId, cancellationToken);

            foreach (var classId in classIds)
                await RecomputeRankingAsync(payload.ExamId, "Class", classId, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task RecomputeRankingAsync(int examId, string scope, int scopeId, CancellationToken cancellationToken)
    {
        List<int> studentIdsInScope;
        if (scope == "Section")
        {
            studentIdsInScope = await _db.Students
                .AsNoTracking()
                .Where(s => s.SectionId == scopeId)
                .Select(s => s.StudentId)
                .ToListAsync(cancellationToken);
        }
        else
        {
            studentIdsInScope = await _db.Students
                .AsNoTracking()
                .Where(s => s.Section.ClassId == scopeId)
                .Select(s => s.StudentId)
                .ToListAsync(cancellationToken);
        }

        var markSums = await _db.Marks
            .AsNoTracking()
            .Where(m => m.ExamId == examId && studentIdsInScope.Contains(m.StudentId))
            .GroupBy(m => m.StudentId)
            .Select(g => new { StudentId = g.Key, Total = g.Sum(x => x.Score ?? 0m) })
            .ToDictionaryAsync(x => x.StudentId, x => x.Total, cancellationToken);

        var totals = studentIdsInScope
            .Select(sid => (StudentId: sid, TotalScore: markSums.GetValueOrDefault(sid, 0m)))
            .ToList();

        var ranked = RankingCalculator.ApplyCompetitionRanking(totals);

        var snap = await _db.RankingSnapshots
            .Include(s => s.RankingRows)
            .FirstOrDefaultAsync(
                s => s.ExamId == examId && s.Scope == scope && s.ScopeId == scopeId,
                cancellationToken);

        if (snap is null)
        {
            snap = new RankingSnapshot
            {
                ExamId = examId,
                Scope = scope,
                ScopeId = scopeId,
                ComputedAtUtc = DateTime.UtcNow
            };
            _db.RankingSnapshots.Add(snap);
            await _db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            _db.RankingRows.RemoveRange(snap.RankingRows);
            snap.ComputedAtUtc = DateTime.UtcNow;
        }

        foreach (var r in ranked)
        {
            _db.RankingRows.Add(new RankingRow
            {
                SnapshotId = snap.SnapshotId,
                StudentId = r.StudentId,
                TotalScore = r.TotalScore,
                Rank = r.Rank,
                TieSize = r.TieSize
            });
        }
    }
}
