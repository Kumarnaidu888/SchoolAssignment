using Microsoft.EntityFrameworkCore;
using SchoolApplication.Contracts.Marks;
using SchoolApplication.Exceptions;
using SchoolApplication.Models;
using SchoolApplication.Security;

namespace SchoolApplication.Services;

public sealed class SectionMarkReadService : ISectionMarkReadService
{
    private readonly SchoolAssessmentContext _db;
    private readonly ICurrentUser _currentUser;

    public SectionMarkReadService(SchoolAssessmentContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<SectionStudentMarksResponse>> GetSectionMarksAsync(int sectionId, int? examId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not { } uid)
            throw new ForbiddenException("Authentication required.");

        if (!_currentUser.IsInRole(AppRoles.Admin))
        {
            if (!_currentUser.IsInRole(AppRoles.Teacher))
                throw new ForbiddenException("Only administrators and teachers can view section marks.");

            var assigned = await _db.TeacherSections.AnyAsync(t => t.TeacherUserId == uid && t.SectionId == sectionId, cancellationToken);
            if (!assigned)
                throw new ForbiddenException("You are not assigned to this section.");
        }

        if (!await _db.Sections.AnyAsync(s => s.SectionId == sectionId, cancellationToken))
            throw new NotFoundException($"Section with id {sectionId} was not found.");

        var students = await _db.Students
            .AsNoTracking()
            .Where(s => s.SectionId == sectionId)
            .OrderBy(s => s.FullName)
            .Select(s => new { s.StudentId, s.FullName })
            .ToListAsync(cancellationToken);

        var result = new List<SectionStudentMarksResponse>();
        foreach (var st in students)
        {
            var q = _db.Marks.AsNoTracking().Where(m => m.StudentId == st.StudentId);
            if (examId is { } e)
                q = q.Where(m => m.ExamId == e);

            var marks = await q
                .OrderBy(m => m.ExamId)
                .ThenBy(m => m.SubjectId)
                .Join(_db.Subjects.AsNoTracking(), m => m.SubjectId, sub => sub.SubjectId, (m, sub) => new { m, sub })
                .Join(_db.Exams.AsNoTracking(), x => x.m.ExamId, ex => ex.ExamId, (x, ex) => new StudentMarkRowResponse(
                    x.m.SubjectId,
                    x.sub.Name,
                    x.m.ExamId,
                    ex.DisplayName,
                    x.m.Score,
                    x.m.UpdatedAtUtc))
                .ToListAsync(cancellationToken);

            result.Add(new SectionStudentMarksResponse(st.StudentId, st.FullName, marks));
        }

        return result;
    }
}
