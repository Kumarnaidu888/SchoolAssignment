using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApplication.Contracts.Classes;
using SchoolApplication.Security;
using SchoolApplication.Services;

namespace SchoolApplication.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public sealed class ClassesController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassesController(IClassService classService) => _classService = classService;

    /// <summary>List all classes.</summary>
    [HttpGet]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Teacher}")]
    [ProducesResponseType(typeof(IReadOnlyList<ClassResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClassResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _classService.GetAllAsync(cancellationToken));

    /// <summary>Get a class by id.</summary>
    [HttpGet("{classId:int}")]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Teacher}")]
    [ProducesResponseType(typeof(ClassResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClassResponse>> GetById(int classId, CancellationToken cancellationToken) =>
        Ok(await _classService.GetByIdAsync(classId, cancellationToken));

    /// <summary>Create a class.</summary>
    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(ClassResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClassResponse>> Create([FromBody] CreateClassRequest request, CancellationToken cancellationToken)
    {
        var created = await _classService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { classId = created.ClassId }, created);
    }

    /// <summary>Update a class.</summary>
    [HttpPut("{classId:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(ClassResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClassResponse>> Update(int classId, [FromBody] UpdateClassRequest request, CancellationToken cancellationToken) =>
        Ok(await _classService.UpdateAsync(classId, request, cancellationToken));

    /// <summary>Delete a class.</summary>
    [HttpDelete("{classId:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int classId, CancellationToken cancellationToken)
    {
        await _classService.DeleteAsync(classId, cancellationToken);
        return NoContent();
    }
}
