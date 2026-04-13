using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApplication.Contracts.Reference;
using SchoolApplication.Services;

namespace SchoolApplication.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public sealed class SubjectsController : ControllerBase
{
    private readonly IReferenceDataService _referenceData;

    public SubjectsController(IReferenceDataService referenceData) => _referenceData = referenceData;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SubjectResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SubjectResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _referenceData.GetSubjectsAsync(cancellationToken));
}
