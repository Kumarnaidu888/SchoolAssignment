using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Contracts.Marks;
using SchoolApplication.Exceptions;
using SchoolApplication.Models;
using SchoolApplication.Processing;
namespace SchoolApplication.Services;

public sealed class MarkSubmissionService : IMarkSubmissionService
{
    private readonly SchoolAssessmentContext _db;
    private readonly ILogger<MarkSubmissionService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public MarkSubmissionService(SchoolAssessmentContext db, ILogger<MarkSubmissionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<MarkSubmissionAcceptedResponse> SubmitAsync(
        string idempotencyKey,
        SubmitMarksRequest request,
        int submittedByUserId,
        bool submitterIsAdmin,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.MarkProcessingJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.IdempotencyKey == idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            _logger.LogInformation("Idempotent marks submission hit for key {Key}, job {JobId}", idempotencyKey, existing.JobId);
            return new MarkSubmissionAcceptedResponse(existing.JobId, existing.Status!, "Duplicate request; returning existing job.");
        }

        if (!await _db.Exams.AnyAsync(e => e.ExamId == request.ExamId, cancellationToken))
            throw new NotFoundException($"Exam with id {request.ExamId} was not found.");

        var studentIds = request.Marks.Select(m => m.StudentId).Distinct().ToList();
        var students = await _db.Students
            .AsNoTracking()
            .Where(s => studentIds.Contains(s.StudentId))
            .Select(s => new { s.StudentId, s.SectionId })
            .ToListAsync(cancellationToken);

        if (students.Count != studentIds.Count)
            throw new NotFoundException("One or more students were not found.");

        if (!submitterIsAdmin)
        {
            var allowedSections = (await _db.TeacherSections
                .AsNoTracking()
                .Where(t => t.TeacherUserId == submittedByUserId)
                .Select(t => t.SectionId)
                .ToListAsync(cancellationToken)).ToHashSet();

            foreach (var s in students)
            {
                if (!allowedSections.Contains(s.SectionId))
                    throw new ForbiddenException($"You are not assigned to the section for student {s.StudentId}.");
            }
        }

        var subjectIds = request.Marks.Select(m => m.SubjectId).Distinct().ToList();
        if (subjectIds.Count != await _db.Subjects.CountAsync(s => subjectIds.Contains(s.SubjectId), cancellationToken))
            throw new NotFoundException("One or more subjects were not found.");

        var payload = new MarkSubmissionPayload
        {
            ExamId = request.ExamId,
            SubmittedByUserId = submittedByUserId,
            Marks = request.Marks
                .Select(m => new MarkSubmissionPayload.MarkLinePayload
                {
                    StudentId = m.StudentId,
                    SubjectId = m.SubjectId,
                    Score = m.Score
                })
                .ToList()
        };

        var job = new MarkProcessingJob
        {
            IdempotencyKey = idempotencyKey,
            Status = "Pending",
            PayloadJson = JsonSerializer.Serialize(payload, JsonOptions),
            RetryCount = 0,
            MaxRetries = 3,
            NextAttemptAtUtc = DateTime.UtcNow
        };

        _db.MarkProcessingJobs.Add(job);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Enqueued marks job {JobId} for exam {ExamId} with {Count} lines (user {UserId})",
            job.JobId, request.ExamId, request.Marks.Count, submittedByUserId);

        return new MarkSubmissionAcceptedResponse(job.JobId, job.Status!, "Marks queued for asynchronous processing.");
    }
}
