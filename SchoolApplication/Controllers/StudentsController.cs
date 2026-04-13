using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApplication.Contracts.Students;
using SchoolApplication.Security;
using SchoolApplication.Services;

namespace SchoolApplication.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService) => _studentService = studentService;

    [HttpGet("by-section/{sectionId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<StudentResponse>>> GetBySection(int sectionId, CancellationToken cancellationToken) =>
        Ok(await _studentService.GetBySectionIdAsync(sectionId, cancellationToken));

    [HttpGet("{studentId:int}")]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentResponse>> GetById(int studentId, CancellationToken cancellationToken) =>
        Ok(await _studentService.GetByIdAsync(studentId, cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<StudentResponse>> Create([FromBody] CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var created = await _studentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { studentId = created.StudentId }, created);
    }

    [HttpPut("{studentId:int}")]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StudentResponse>> Update(int studentId, [FromBody] UpdateStudentRequest request, CancellationToken cancellationToken) =>
        Ok(await _studentService.UpdateAsync(studentId, request, cancellationToken));

    /// <summary>
    /// Link a login user to this student so they can use <c>GET /api/me/marks</c> and <c>/api/me/rankings</c>.
    /// The user must already exist with the <b>Student</b> role (see <c>POST /api/users</c>). Alternatively use full <c>PUT /api/students/&#123;id&#125;</c> with <c>userId</c>.
    /// </summary>
    [HttpPut("{studentId:int}/linked-user")]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StudentResponse>> LinkUser(int studentId, [FromBody] LinkStudentUserRequest request, CancellationToken cancellationToken) =>
        Ok(await _studentService.LinkStudentUserAsync(studentId, request, cancellationToken));

    [HttpDelete("{studentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int studentId, CancellationToken cancellationToken)
    {
        await _studentService.DeleteAsync(studentId, cancellationToken);
        return NoContent();
    }
}
