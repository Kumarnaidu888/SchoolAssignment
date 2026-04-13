using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApplication.Contracts.Reference;
using SchoolApplication.Services;

namespace SchoolApplication.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public sealed class ExamsController : ControllerBase
{
    private readonly IReferenceDataService _referenceData;

    public ExamsController(IReferenceDataService referenceData) => _referenceData = referenceData;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ExamResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ExamResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _referenceData.GetExamsAsync(cancellationToken));
}
