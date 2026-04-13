using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApplication.Contracts.Rankings;
using SchoolApplication.Security;
using SchoolApplication.Services;

namespace SchoolApplication.Controllers;

[ApiController]
[Route("api/rankings")]
[Produces("application/json")]
[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Teacher}")]
public sealed class RankingsController : ControllerBase
{
    private readonly IRankingQueryService _rankings;

    public RankingsController(IRankingQueryService rankings) => _rankings = rankings;

    /// <summary>Rankings within a section for an exam.</summary>
    [HttpGet("sections/{sectionId:int}")]
    [ProducesResponseType(typeof(RankingSnapshotResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RankingSnapshotResponse>> GetBySection(int sectionId, [FromQuery] int examId, CancellationToken cancellationToken) =>
        Ok(await _rankings.GetSectionRankingAsync(sectionId, examId, cancellationToken));

    /// <summary>Rankings across a class (all sections) for an exam.</summary>
    [HttpGet("classes/{classId:int}")]
    [ProducesResponseType(typeof(RankingSnapshotResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RankingSnapshotResponse>> GetByClass(int classId, [FromQuery] int examId, CancellationToken cancellationToken) =>
        Ok(await _rankings.GetClassRankingAsync(classId, examId, cancellationToken));

    /// <summary>Top N students for a scope (Section or Class) and exam.</summary>
    [HttpGet("top")]
    [ProducesResponseType(typeof(IReadOnlyList<RankingRowResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RankingRowResponse>>> GetTop(
        [FromQuery] int examId,
        [FromQuery] string scope,
        [FromQuery] int scopeId,
        [FromQuery] int n = 10,
        CancellationToken cancellationToken = default) =>
        Ok(await _rankings.GetTopNAsync(scope, scopeId, examId, n, cancellationToken));
}
