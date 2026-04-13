using Microsoft.EntityFrameworkCore;
using SchoolApplication.Contracts.Students;
using SchoolApplication.Exceptions;
using SchoolApplication.Models;
using SchoolApplication.Security;

namespace SchoolApplication.Services;

public sealed class StudentService : IStudentService
{
    private readonly SchoolAssessmentContext _db;
    private readonly ILogger<StudentService> _logger;

    public StudentService(SchoolAssessmentContext db, ILogger<StudentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<StudentResponse>> GetBySectionIdAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        if (!await _db.Sections.AnyAsync(s => s.SectionId == sectionId, cancellationToken))
            throw new NotFoundException($"Section with id {sectionId} was not found.");

        return await _db.Students
            .AsNoTracking()
            .Where(s => s.SectionId == sectionId)
            .OrderBy(s => s.FullName)
            .Select(s => new StudentResponse(s.StudentId, s.SectionId, s.FullName, s.AdmissionNo, s.UserId, s.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentResponse> GetByIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var row = await _db.Students
            .AsNoTracking()
            .Where(s => s.StudentId == studentId)
            .Select(s => new StudentResponse(s.StudentId, s.SectionId, s.FullName, s.AdmissionNo, s.UserId, s.CreatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            throw new NotFoundException($"Student with id {studentId} was not found.");

        return row;
    }

    public async Task<StudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _db.Sections.AnyAsync(s => s.SectionId == request.SectionId, cancellationToken))
            throw new NotFoundException($"Section with id {request.SectionId} was not found.");

        if (request.UserId is { } uid && await _db.Students.AnyAsync(s => s.UserId == uid, cancellationToken))
            throw new ConflictException("Another student is already linked to this user account.");

        var entity = new Student
        {
            SectionId = request.SectionId,
            FullName = request.FullName.Trim(),
            AdmissionNo = string.IsNullOrWhiteSpace(request.AdmissionNo) ? null : request.AdmissionNo.Trim(),
            UserId = request.UserId
        };

        _db.Students.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        await _db.Entry(entity).ReloadAsync(cancellationToken);

        _logger.LogInformation("Created student {StudentId}", entity.StudentId);
        return new StudentResponse(entity.StudentId, entity.SectionId, entity.FullName, entity.AdmissionNo, entity.UserId, entity.CreatedAtUtc);
    }

    public async Task<StudentResponse> UpdateAsync(int studentId, UpdateStudentRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Students.FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);
        if (entity is null)
            throw new NotFoundException($"Student with id {studentId} was not found.");

        if (!await _db.Sections.AnyAsync(s => s.SectionId == request.SectionId, cancellationToken))
            throw new NotFoundException($"Section with id {request.SectionId} was not found.");

        if (request.UserId is { } uid)
        {
            var taken = await _db.Students.AnyAsync(s => s.UserId == uid && s.StudentId != studentId, cancellationToken);
            if (taken)
                throw new ConflictException("Another student is already linked to this user account.");
        }

        entity.SectionId = request.SectionId;
        entity.FullName = request.FullName.Trim();
        entity.AdmissionNo = string.IsNullOrWhiteSpace(request.AdmissionNo) ? null : request.AdmissionNo.Trim();
        entity.UserId = request.UserId;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated student {StudentId}", studentId);
        return new StudentResponse(entity.StudentId, entity.SectionId, entity.FullName, entity.AdmissionNo, entity.UserId, entity.CreatedAtUtc);
    }

    public async Task<StudentResponse> LinkStudentUserAsync(int studentId, LinkStudentUserRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Students.FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);
        if (entity is null)
            throw new NotFoundException($"Student with id {studentId} was not found.");

        var user = await _db.AppUsers
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);
        if (user is null)
            throw new NotFoundException($"User with id {request.UserId} was not found.");

        if (!user.Roles.Any(r => r.RoleName == AppRoles.Student))
            throw new ConflictException("The user must have the Student role. Create the account with POST /api/users (roles containing Student) first.");

        if (await _db.Students.AnyAsync(s => s.UserId == request.UserId && s.StudentId != studentId, cancellationToken))
            throw new ConflictException("That user account is already linked to a different student.");

        entity.UserId = request.UserId;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Linked student {StudentId} to user {UserId}", studentId, request.UserId);
        return await GetByIdAsync(studentId, cancellationToken);
    }

    public async Task DeleteAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Students.FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);
        if (entity is null)
            throw new NotFoundException($"Student with id {studentId} was not found.");

        _db.Students.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted student {StudentId}", studentId);
    }
}
