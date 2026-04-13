using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApplication.Contracts.Sections;
using SchoolApplication.Security;
using SchoolApplication.Services;

namespace SchoolApplication.Controllers;

[ApiController]
[Route("api")]
[Produces("application/json")]
[Authorize]
public sealed class SectionsController : ControllerBase
{
    private readonly ISectionService _sectionService;

    public SectionsController(ISectionService sectionService) => _sectionService = sectionService;

    /// <summary>List sections for a class.</summary>
    [HttpGet("classes/{classId:int}/sections")]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Teacher}")]
    [ProducesResponseType(typeof(IReadOnlyList<SectionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<SectionResponse>>> GetByClass(int classId, CancellationToken cancellationToken) =>
        Ok(await _sectionService.GetByClassIdAsync(classId, cancellationToken));

    /// <summary>Create a section in a class.</summary>
    [HttpPost("classes/{classId:int}/sections")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(SectionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SectionResponse>> Create(int classId, [FromBody] CreateSectionRequest request, CancellationToken cancellationToken)
    {
        var created = await _sectionService.CreateAsync(classId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { sectionId = created.SectionId }, created);
    }

    /// <summary>Get a section by id.</summary>
    [HttpGet("sections/{sectionId:int}")]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Teacher}")]
    [ProducesResponseType(typeof(SectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SectionResponse>> GetById(int sectionId, CancellationToken cancellationToken) =>
        Ok(await _sectionService.GetByIdAsync(sectionId, cancellationToken));

    /// <summary>Update a section.</summary>
    [HttpPut("sections/{sectionId:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(SectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SectionResponse>> Update(int sectionId, [FromBody] UpdateSectionRequest request, CancellationToken cancellationToken) =>
        Ok(await _sectionService.UpdateAsync(sectionId, request, cancellationToken));

    /// <summary>Delete a section.</summary>
    [HttpDelete("sections/{sectionId:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int sectionId, CancellationToken cancellationToken)
    {
        await _sectionService.DeleteAsync(sectionId, cancellationToken);
        return NoContent();
    }
}
