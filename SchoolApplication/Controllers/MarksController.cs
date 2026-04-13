using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApplication.Contracts.Marks;
using SchoolApplication.Exceptions;
using SchoolApplication.Security;
using SchoolApplication.Services;

namespace SchoolApplication.Controllers;

[ApiController]
[Route("api/marks")]
[Produces("application/json")]
[Authorize]
public sealed class MarksController : ControllerBase
{
    private readonly IMarkSubmissionService _submission;
    private readonly ISectionMarkReadService _sectionMarks;
    private readonly ICurrentUser _currentUser;

    public MarksController(
        IMarkSubmissionService submission,
        ISectionMarkReadService sectionMarks,
        ICurrentUser currentUser)
    {
        _submission = submission;
        _sectionMarks = sectionMarks;
        _currentUser = currentUser;
    }

    /// <summary>Submit marks (async). Send header <c>Idempotency-Key</c> (required). Teachers may only include students in assigned sections.</summary>
    [HttpPost("submissions")]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Teacher}")]
    [ProducesResponseType(typeof(MarkSubmissionAcceptedResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MarkSubmissionAcceptedResponse>> Submit(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromBody] SubmitMarksRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest(new ProblemDetails { Title = "Missing Idempotency-Key", Detail = "Send a unique Idempotency-Key header per submission intent." });

        var uid = _currentUser.UserId ?? throw new ForbiddenException("User id missing from token.");
        var isAdmin = _currentUser.IsInRole(AppRoles.Admin);
        var result = await _submission.SubmitAsync(idempotencyKey.Trim(), request, uid, isAdmin, cancellationToken);
        return Accepted($"/api/jobs/{result.JobId}", result);
    }

    /// <summary>View marks for all students in a section (admin or assigned teacher).</summary>
    [HttpGet("sections/{sectionId:int}")]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Teacher}")]
    [ProducesResponseType(typeof(IReadOnlyList<SectionStudentMarksResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SectionStudentMarksResponse>>> GetSectionMarks(
        int sectionId,
        [FromQuery] int? examId,
        CancellationToken cancellationToken) =>
        Ok(await _sectionMarks.GetSectionMarksAsync(sectionId, examId, cancellationToken));
}
