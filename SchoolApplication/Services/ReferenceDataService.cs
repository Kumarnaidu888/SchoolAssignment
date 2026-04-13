using Microsoft.EntityFrameworkCore;
using SchoolApplication.Contracts.Reference;
using SchoolApplication.Models;

namespace SchoolApplication.Services;

public sealed class ReferenceDataService : IReferenceDataService
{
    private readonly SchoolAssessmentContext _db;

    public ReferenceDataService(SchoolAssessmentContext db) => _db = db;

    public async Task<IReadOnlyList<SubjectResponse>> GetSubjectsAsync(CancellationToken cancellationToken = default) =>
        await _db.Subjects
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SubjectResponse(s.SubjectId, s.Name, s.Code))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ExamResponse>> GetExamsAsync(CancellationToken cancellationToken = default) =>
        await _db.Exams
            .AsNoTracking()
            .OrderBy(e => e.ExamId)
            .Select(e => new ExamResponse(e.ExamId, e.ExamType, e.DisplayName))
            .ToListAsync(cancellationToken);
}
