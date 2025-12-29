using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
}

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        return Ok(new {
            token = "mock-jwt-token-" + Guid.NewGuid(),
            user = new {
                id = Guid.NewGuid(),
                email = request.Email,
                role = request.Email.Contains("superadmin") ? "SuperAdmin" : "Admin",
                tenantId = Guid.NewGuid()
            }
        });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    [HttpGet("stats")]
    public IActionResult GetStats() => Ok(new {
        totalVideos = 0,
        totalViews = 0,
        totalUsers = 1,
        totalStorage = 0
    });
}

[ApiController]
[Route("api/[controller]")]
public class VideosController : ControllerBase
{
    [HttpGet]
    public IActionResult GetVideos() => Ok(new List<object>());

    [HttpGet("{id}")]
    public IActionResult GetVideo(string id) => Ok(new { 
        id = id, 
        title = "Sample Video",
        description = "This is a sample video",
        thumbnailUrl = "",
        videoUrl = "",
        duration = 0,
        viewCount = 0,
        createdAt = DateTime.UtcNow
    });
}

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetSettings() => Ok(new {
        bunnyStream = new {
            apiKey = "",
            libraryId = "",
            pullZoneId = ""
        },
        stripe = new {
            publishableKey = "",
            secretKey = ""
        }
    });

    [HttpPut]
    public IActionResult UpdateSettings([FromBody] object settings) => Ok(new { 
        success = true, 
        message = "Settings updated successfully" 
    });
}
