using Microsoft.EntityFrameworkCore;
using SchoolApplication.Contracts.Sections;
using SchoolApplication.Exceptions;
using SchoolApplication.Models;

namespace SchoolApplication.Services;

public sealed class SectionService : ISectionService
{
    private readonly SchoolAssessmentContext _db;
    private readonly ILogger<SectionService> _logger;

    public SectionService(SchoolAssessmentContext db, ILogger<SectionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SectionResponse>> GetByClassIdAsync(int classId, CancellationToken cancellationToken = default)
    {
        if (!await _db.Classes.AnyAsync(c => c.ClassId == classId, cancellationToken))
        {
            _logger.LogWarning("Class {ClassId} not found when listing sections", classId);
            throw new NotFoundException($"Class with id {classId} was not found.");
        }

        var list = await _db.Sections
            .AsNoTracking()
            .Where(s => s.ClassId == classId)
            .OrderBy(s => s.Name)
            .Select(s => new SectionResponse(s.SectionId, s.ClassId, s.Name, s.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Listed {Count} sections for class {ClassId}", list.Count, classId);
        return list;
    }

    public async Task<SectionResponse> GetByIdAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        var row = await _db.Sections
            .AsNoTracking()
            .Where(s => s.SectionId == sectionId)
            .Select(s => new SectionResponse(s.SectionId, s.ClassId, s.Name, s.CreatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            _logger.LogWarning("Section {SectionId} not found", sectionId);
            throw new NotFoundException($"Section with id {sectionId} was not found.");
        }

        return row;
    }

    public async Task<SectionResponse> CreateAsync(int classId, CreateSectionRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _db.Classes.AnyAsync(c => c.ClassId == classId, cancellationToken))
        {
            _logger.LogWarning("Class {ClassId} not found when creating section", classId);
            throw new NotFoundException($"Class with id {classId} was not found.");
        }

        var entity = new Section
        {
            ClassId = classId,
            Name = request.Name.Trim()
        };

        _db.Sections.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        await _db.Entry(entity).ReloadAsync(cancellationToken);

        _logger.LogInformation("Created section {SectionId} for class {ClassId}", entity.SectionId, classId);
        return new SectionResponse(entity.SectionId, entity.ClassId, entity.Name, entity.CreatedAtUtc);
    }

    public async Task<SectionResponse> UpdateAsync(int sectionId, UpdateSectionRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Sections.FirstOrDefaultAsync(s => s.SectionId == sectionId, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Section {SectionId} not found for update", sectionId);
            throw new NotFoundException($"Section with id {sectionId} was not found.");
        }

        entity.Name = request.Name.Trim();
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated section {SectionId}", sectionId);
        return new SectionResponse(entity.SectionId, entity.ClassId, entity.Name, entity.CreatedAtUtc);
    }

    public async Task DeleteAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Sections.FirstOrDefaultAsync(s => s.SectionId == sectionId, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Section {SectionId} not found for delete", sectionId);
            throw new NotFoundException($"Section with id {sectionId} was not found.");
        }

        _db.Sections.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted section {SectionId}", sectionId);
    }
}
