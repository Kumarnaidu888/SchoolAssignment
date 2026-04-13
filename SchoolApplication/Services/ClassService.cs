using Microsoft.EntityFrameworkCore;
using SchoolApplication.Contracts.Classes;
using SchoolApplication.Exceptions;
using SchoolApplication.Models;

namespace SchoolApplication.Services;

public sealed class ClassService : IClassService
{
    private readonly SchoolAssessmentContext _db;
    private readonly ILogger<ClassService> _logger;

    public ClassService(SchoolAssessmentContext db, ILogger<ClassService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ClassResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.Classes
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new ClassResponse(c.ClassId, c.Name, c.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Listed {Count} classes", list.Count);
        return list;
    }

    public async Task<ClassResponse> GetByIdAsync(int classId, CancellationToken cancellationToken = default)
    {
        var row = await _db.Classes
            .AsNoTracking()
            .Where(c => c.ClassId == classId)
            .Select(c => new ClassResponse(c.ClassId, c.Name, c.CreatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            _logger.LogWarning("Class {ClassId} not found", classId);
            throw new NotFoundException($"Class with id {classId} was not found.");
        }

        return row;
    }

    public async Task<ClassResponse> CreateAsync(CreateClassRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Class
        {
            Name = request.Name.Trim()
        };

        _db.Classes.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        await _db.Entry(entity).ReloadAsync(cancellationToken);

        _logger.LogInformation("Created class {ClassId} with name {Name}", entity.ClassId, entity.Name);
        return new ClassResponse(entity.ClassId, entity.Name, entity.CreatedAtUtc);
    }

    public async Task<ClassResponse> UpdateAsync(int classId, UpdateClassRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Classes.FirstOrDefaultAsync(c => c.ClassId == classId, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Class {ClassId} not found for update", classId);
            throw new NotFoundException($"Class with id {classId} was not found.");
        }

        entity.Name = request.Name.Trim();
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated class {ClassId}", classId);
        return new ClassResponse(entity.ClassId, entity.Name, entity.CreatedAtUtc);
    }

    public async Task DeleteAsync(int classId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Classes.FirstOrDefaultAsync(c => c.ClassId == classId, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Class {ClassId} not found for delete", classId);
            throw new NotFoundException($"Class with id {classId} was not found.");
        }

        _db.Classes.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted class {ClassId}", classId);
    }
}
