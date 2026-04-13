using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApplication.Contracts.Users;
using SchoolApplication.Security;
using SchoolApplication.Services;

namespace SchoolApplication.Controllers;

/// <summary>Admin-only user accounts (teachers, other admins, app logins with Student role before linking to a student row).</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class UsersController : ControllerBase
{
    private readonly IUserManagementService _users;

    public UsersController(IUserManagementService users) => _users = users;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> List(CancellationToken cancellationToken) =>
        Ok(await _users.GetAllAsync(cancellationToken));

    [HttpGet("{userId:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(int userId, CancellationToken cancellationToken) =>
        Ok(await _users.GetByIdAsync(userId, cancellationToken));

    /// <summary>Create a user with explicit roles (e.g. <c>["Teacher"]</c> or <c>["Admin","Teacher"]</c>).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var created = await _users.CreateUserAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { userId = created.UserId }, created);
    }

    /// <summary>Create a teacher login (same as POST /api/users with role Teacher). Then assign sections via PUT /api/teachers/&#123;userId&#125;/sections.</summary>
    [HttpPost("teachers")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> CreateTeacher([FromBody] CreateTeacherAccountRequest request, CancellationToken cancellationToken)
    {
        var created = await _users.CreateTeacherAccountAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { userId = created.UserId }, created);
    }

    /// <summary>Replace all roles for a user (e.g. promote/demote).</summary>
    [HttpPut("{userId:int}/roles")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> ReplaceRoles(int userId, [FromBody] ReplaceUserRolesRequest request, CancellationToken cancellationToken) =>
        Ok(await _users.ReplaceRolesAsync(userId, request, cancellationToken));
}
