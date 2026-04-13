using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApplication.Contracts.Teachers;
using SchoolApplication.Security;
using SchoolApplication.Services;

namespace SchoolApplication.Controllers;

/// <summary>Assign teachers to sections. Create the teacher account first with <c>POST /api/users/teachers</c> (or <c>POST /api/users</c> with role Teacher).</summary>
[ApiController]
[Route("api/teachers")]
[Produces("application/json")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class TeacherAssignmentsController : ControllerBase
{
    private readonly ITeacherAssignmentService _service;

    public TeacherAssignmentsController(ITeacherAssignmentService service) => _service = service;

    /// <summary>Get all section assignments for a teacher user.</summary>
    [HttpGet("{teacherUserId:int}/sections")]
    [ProducesResponseType(typeof(IReadOnlyList<TeacherSectionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TeacherSectionResponse>>> GetAssignments(int teacherUserId, CancellationToken cancellationToken) =>
        Ok(await _service.GetAssignmentsForTeacherAsync(teacherUserId, cancellationToken));

    /// <summary>Replace section assignments for a teacher (must have Teacher role).</summary>
    [HttpPut("{teacherUserId:int}/sections")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReplaceAssignments(int teacherUserId, [FromBody] ReplaceTeacherSectionsRequest request, CancellationToken cancellationToken)
    {
        await _service.ReplaceAssignmentsAsync(teacherUserId, request.SectionIds, cancellationToken);
        return NoContent();
    }
}
