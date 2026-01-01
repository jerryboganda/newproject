using Microsoft.AspNetCore.Mvc;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow });
}
