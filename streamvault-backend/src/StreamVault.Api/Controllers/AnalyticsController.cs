using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Services;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IVideoAnalyticsService _analyticsService;

    public AnalyticsController(IVideoAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpPost("videos/{videoId}/track-view")]
    public async Task<IActionResult> TrackView(Guid videoId, [FromBody] TrackViewRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            await _analyticsService.TrackViewAsync(videoId, userId, request.SessionId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("videos/{videoId}/engage")]
    public async Task<IActionResult> TrackEngagement(Guid videoId, [FromBody] TrackEngagementRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            await _analyticsService.TrackEngagementAsync(videoId, userId, request.Type, request.Metadata);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("videos/{videoId}")]
    public async Task<ActionResult<VideoAnalytics>> GetVideoAnalytics(Guid videoId, [FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
    {
        try
        {
            var analytics = await _analyticsService.GetVideoAnalyticsAsync(videoId, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("tenant")]
    public async Task<ActionResult<TenantAnalytics>> GetTenantAnalytics([FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var analytics = await _analyticsService.GetTenantAnalyticsAsync(tenantId, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class TrackViewRequest
{
    public string? SessionId { get; set; }
}

public class TrackEngagementRequest
{
    public EngagementType Type { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
