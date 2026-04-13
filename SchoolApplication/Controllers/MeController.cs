using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApplication.Contracts.Marks;
using SchoolApplication.Contracts.Me;
using SchoolApplication.Contracts.Rankings;
using SchoolApplication.Exceptions;
using SchoolApplication.Security;
using SchoolApplication.Services;

namespace SchoolApplication.Controllers;

[ApiController]
[Route("api/me")]
[Produces("application/json")]
[Authorize]
public sealed class MeController : ControllerBase
{
    private readonly ICurrentUserProfileService _profile;
    private readonly IMePortalService _portal;
    private readonly ICurrentUser _currentUser;

    public MeController(
        ICurrentUserProfileService profile,
        IMePortalService portal,
        ICurrentUser currentUser)
    {
        _profile = profile;
        _portal = portal;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Profile for the current JWT (Admin, Teacher, or Student). Check <c>studentPortal</c>: if null, the account is not linked to a <c>Students</c> row, so
    /// <c>GET /api/me/marks</c> returns 404 until an admin uses <c>PUT /api/students/&#123;id&#125;/linked-user</c> (or full student PUT with <c>userId</c>).
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(CurrentUserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CurrentUserProfileResponse>> GetProfile(CancellationToken cancellationToken)
    {
        var uid = _currentUser.UserId ?? throw new ForbiddenException("User id missing from token.");
        return Ok(await _profile.GetProfileAsync(uid, cancellationToken));
    }

    /// <summary>Student marks. **404** if the account has no <c>Students.UserId</c> link yet.</summary>
    [HttpGet("marks")]
    [Authorize(Roles = AppRoles.Student)]
    [ProducesResponseType(typeof(IReadOnlyList<StudentMarkRowResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<StudentMarkRowResponse>>> GetMarks([FromQuery] int? examId, CancellationToken cancellationToken)
    {
        var uid = _currentUser.UserId!.Value;
        return Ok(await _portal.GetMyMarksAsync(uid, examId, cancellationToken));
    }

    /// <summary>Your rank within your section for an exam. **404** if not linked to a student; **204** if no ranking snapshot yet.</summary>
    [HttpGet("rankings")]
    [Authorize(Roles = AppRoles.Student)]
    [ProducesResponseType(typeof(RankingRowResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<RankingRowResponse>> GetRanking([FromQuery] int examId, CancellationToken cancellationToken)
    {
        var uid = _currentUser.UserId!.Value;
        var row = await _portal.GetMySectionRankingAsync(uid, examId, cancellationToken);
        if (row is null)
            return NoContent();
        return Ok(row);
    }
}
