using Microsoft.EntityFrameworkCore;
using SchoolApplication.Contracts.Teachers;
using SchoolApplication.Exceptions;
using SchoolApplication.Models;
using SchoolApplication.Security;

namespace SchoolApplication.Services;

public sealed class TeacherAssignmentService : ITeacherAssignmentService
{
    private readonly SchoolAssessmentContext _db;
    private readonly ILogger<TeacherAssignmentService> _logger;

    public TeacherAssignmentService(SchoolAssessmentContext db, ILogger<TeacherAssignmentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TeacherSectionResponse>> GetAssignmentsForTeacherAsync(int teacherUserId, CancellationToken cancellationToken = default)
    {
        if (!await _db.AppUsers.AnyAsync(u => u.UserId == teacherUserId, cancellationToken))
            throw new NotFoundException($"User with id {teacherUserId} was not found.");

        return await _db.TeacherSections
            .AsNoTracking()
            .Where(t => t.TeacherUserId == teacherUserId)
            .Select(t => new TeacherSectionResponse(t.TeacherUserId, t.SectionId, t.AssignedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceAssignmentsAsync(int teacherUserId, IReadOnlyList<int> sectionIds, CancellationToken cancellationToken = default)
    {
        var user = await _db.AppUsers
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserId == teacherUserId, cancellationToken);
        if (user is null)
            throw new NotFoundException($"User with id {teacherUserId} was not found.");

        if (!user.Roles.Any(r => r.RoleName == AppRoles.Teacher))
            throw new ConflictException("User must have the Teacher role to be assigned to sections.");

        var distinct = sectionIds.Distinct().ToList();
        foreach (var sid in distinct)
        {
            if (!await _db.Sections.AnyAsync(s => s.SectionId == sid, cancellationToken))
                throw new NotFoundException($"Section with id {sid} was not found.");
        }

        var existing = await _db.TeacherSections.Where(t => t.TeacherUserId == teacherUserId).ToListAsync(cancellationToken);
        _db.TeacherSections.RemoveRange(existing);

        foreach (var sid in distinct)
        {
            _db.TeacherSections.Add(new TeacherSection { TeacherUserId = teacherUserId, SectionId = sid });
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Replaced teacher {TeacherId} section assignments: {Count} sections", teacherUserId, distinct.Count);
    }
}
