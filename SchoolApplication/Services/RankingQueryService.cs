using Microsoft.EntityFrameworkCore;
using SchoolApplication.Contracts.Rankings;
using SchoolApplication.Exceptions;
using SchoolApplication.Models;
using SchoolApplication.Security;

namespace SchoolApplication.Services;

public sealed class RankingQueryService : IRankingQueryService
{
    private readonly SchoolAssessmentContext _db;
    private readonly ICurrentUser _currentUser;

    public RankingQueryService(SchoolAssessmentContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RankingSnapshotResponse> GetSectionRankingAsync(int sectionId, int examId, CancellationToken cancellationToken = default)
    {
        await EnsureSectionAccessAsync(sectionId, cancellationToken);

        if (!await _db.Sections.AnyAsync(s => s.SectionId == sectionId, cancellationToken))
            throw new NotFoundException($"Section with id {sectionId} was not found.");

        return await LoadSnapshotAsync("Section", sectionId, examId, cancellationToken);
    }

    public async Task<RankingSnapshotResponse> GetClassRankingAsync(int classId, int examId, CancellationToken cancellationToken = default)
    {
        await EnsureClassAccessAsync(classId, cancellationToken);

        if (!await _db.Classes.AnyAsync(c => c.ClassId == classId, cancellationToken))
            throw new NotFoundException($"Class with id {classId} was not found.");

        return await LoadSnapshotAsync("Class", classId, examId, cancellationToken);
    }

    public async Task<IReadOnlyList<RankingRowResponse>> GetTopNAsync(string scope, int scopeId, int examId, int n, CancellationToken cancellationToken = default)
    {
        if (n <= 0 || n > 500)
            throw new ConflictException("Parameter n must be between 1 and 500.");

        var scopeNorm = scope.Equals("Section", StringComparison.OrdinalIgnoreCase)
            ? "Section"
            : scope.Equals("Class", StringComparison.OrdinalIgnoreCase)
                ? "Class"
                : throw new ConflictException("Scope must be 'Section' or 'Class'.");

        if (scopeNorm == "Section")
            await EnsureSectionAccessAsync(scopeId, cancellationToken);
        else
            await EnsureClassAccessAsync(scopeId, cancellationToken);

        var snap = await _db.RankingSnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.ExamId == examId && s.Scope == scopeNorm && s.ScopeId == scopeId,
                cancellationToken);

        if (snap is null)
            return [];

        return await _db.RankingRows
            .AsNoTracking()
            .Where(r => r.SnapshotId == snap.SnapshotId)
            .OrderBy(r => r.Rank)
            .ThenBy(r => r.StudentId)
            .Take(n)
            .Join(_db.Students.AsNoTracking(),
                row => row.StudentId,
                st => st.StudentId,
                (row, st) => new RankingRowResponse(row.StudentId, st.FullName, row.TotalScore, row.Rank, row.TieSize ?? 1))
            .ToListAsync(cancellationToken);
    }

    private async Task EnsureSectionAccessAsync(int sectionId, CancellationToken cancellationToken)
    {
        if (_currentUser.IsInRole(AppRoles.Admin))
            return;

        if (_currentUser.UserId is not { } uid)
            throw new ForbiddenException("Authentication required.");

        if (_currentUser.IsInRole(AppRoles.Teacher))
        {
            var ok = await _db.TeacherSections.AnyAsync(t => t.TeacherUserId == uid && t.SectionId == sectionId, cancellationToken);
            if (!ok)
                throw new ForbiddenException("You are not assigned to this section.");
            return;
        }

        throw new ForbiddenException("You are not allowed to view this ranking.");
    }

    private async Task EnsureClassAccessAsync(int classId, CancellationToken cancellationToken)
    {
        if (_currentUser.IsInRole(AppRoles.Admin))
            return;

        if (_currentUser.UserId is not { } uid)
            throw new ForbiddenException("Authentication required.");

        if (_currentUser.IsInRole(AppRoles.Teacher))
        {
            var ok = await _db.TeacherSections
                .AnyAsync(t => t.TeacherUserId == uid && t.Section.ClassId == classId, cancellationToken);
            if (!ok)
                throw new ForbiddenException("You are not assigned to any section in this class.");
            return;
        }

        throw new ForbiddenException("You are not allowed to view this ranking.");
    }

    private async Task<RankingSnapshotResponse> LoadSnapshotAsync(string scope, int scopeId, int examId, CancellationToken cancellationToken)
    {
        var snap = await _db.RankingSnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.ExamId == examId && s.Scope == scope && s.ScopeId == scopeId,
                cancellationToken);

        if (snap is null)
        {
            return new RankingSnapshotResponse(0, examId, scope, scopeId, null, []);
        }

        var rows = await _db.RankingRows
            .AsNoTracking()
            .Where(r => r.SnapshotId == snap.SnapshotId)
            .OrderBy(r => r.Rank)
            .ThenBy(r => r.StudentId)
            .Join(_db.Students.AsNoTracking(),
                row => row.StudentId,
                st => st.StudentId,
                (row, st) => new RankingRowResponse(row.StudentId, st.FullName, row.TotalScore, row.Rank, row.TieSize ?? 1))
            .ToListAsync(cancellationToken);

        return new RankingSnapshotResponse(snap.SnapshotId, examId, scope, scopeId, snap.ComputedAtUtc, rows);
    }
}
