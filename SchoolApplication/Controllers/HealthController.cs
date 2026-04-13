using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SchoolApplication.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class HealthController : ControllerBase
{
    /// <summary>Anonymous probe so Swagger/browser can confirm the API is reachable (no JWT).</summary>
    [HttpGet]
    public IActionResult Get() =>
        Ok(new { status = "ok", timeUtc = DateTime.UtcNow });
}
