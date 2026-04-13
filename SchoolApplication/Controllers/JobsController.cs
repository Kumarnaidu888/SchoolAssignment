using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApplication.Contracts.Jobs;
using SchoolApplication.Security;
using SchoolApplication.Exceptions;
using SchoolApplication.Services;

namespace SchoolApplication.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Teacher}")]
public sealed class JobsController : ControllerBase
{
    private readonly IMarkJobQueryService _jobs;
    private readonly ICurrentUser _currentUser;

    public JobsController(IMarkJobQueryService jobs, ICurrentUser currentUser)
    {
        _jobs = jobs;
        _currentUser = currentUser;
    }

    [HttpGet("{jobId:long}")]
    [ProducesResponseType(typeof(JobStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<JobStatusResponse>> GetJob(long jobId, CancellationToken cancellationToken)
    {
        var uid = _currentUser.UserId ?? throw new ForbiddenException("User id missing from token.");
        var isAdmin = _currentUser.IsInRole(AppRoles.Admin);
        return Ok(await _jobs.GetJobAsync(jobId, uid, isAdmin, cancellationToken));
    }
}
