using Microsoft.EntityFrameworkCore;
using SchoolApplication.Contracts.Marks;
using SchoolApplication.Contracts.Rankings;
using SchoolApplication.Exceptions;
using SchoolApplication.Models;

namespace SchoolApplication.Services;

public sealed class MePortalService : IMePortalService
{
    private readonly SchoolAssessmentContext _db;

    public MePortalService(SchoolAssessmentContext db) => _db = db;

    public async Task<IReadOnlyList<StudentMarkRowResponse>> GetMyMarksAsync(int userId, int? examId, CancellationToken cancellationToken = default)
    {
        var student = await _db.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        if (student is null)
            throw new NotFoundException("No student profile is linked to your account.");

        var query = _db.Marks
            .AsNoTracking()
            .Where(m => m.StudentId == student.StudentId);
        if (examId is { } eid)
            query = query.Where(m => m.ExamId == eid);

        return await query
            .OrderBy(m => m.ExamId)
            .ThenBy(m => m.SubjectId)
            .Join(_db.Subjects.AsNoTracking(), m => m.SubjectId, s => s.SubjectId, (m, s) => new { m, s })
            .Join(_db.Exams.AsNoTracking(), x => x.m.ExamId, e => e.ExamId, (x, e) => new StudentMarkRowResponse(
                x.m.SubjectId,
                x.s.Name,
                x.m.ExamId,
                e.DisplayName,
                x.m.Score,
                x.m.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<RankingRowResponse?> GetMySectionRankingAsync(int userId, int examId, CancellationToken cancellationToken = default)
    {
        var student = await _db.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        if (student is null)
            throw new NotFoundException("No student profile is linked to your account.");

        var snap = await _db.RankingSnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.ExamId == examId && s.Scope == "Section" && s.ScopeId == student.SectionId,
                cancellationToken);

        if (snap is null)
            return null;

        var row = await _db.RankingRows
            .AsNoTracking()
            .Where(r => r.SnapshotId == snap.SnapshotId && r.StudentId == student.StudentId)
            .Select(r => new RankingRowResponse(student.StudentId, student.FullName, r.TotalScore, r.Rank, r.TieSize ?? 1))
            .FirstOrDefaultAsync(cancellationToken);

        return row;
    }
}
