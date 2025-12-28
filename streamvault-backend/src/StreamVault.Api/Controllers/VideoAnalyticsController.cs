using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Analytics;
using StreamVault.Application.Analytics.DTOs;
using StreamVault.Domain.Entities;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class VideoAnalyticsController : ControllerBase
{
    private readonly IVideoAnalyticsDashboardService _analyticsService;

    public VideoAnalyticsController(IVideoAnalyticsDashboardService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpPost("track")]
    public async Task<IActionResult> TrackEvent([FromBody] TrackEventRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _analyticsService.TrackEventAsync(request, userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}")]
    public async Task<ActionResult<VideoAnalyticsDto>> GetVideoAnalytics(Guid videoId, [FromQuery] AnalyticsFilter? filter = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var analytics = await _analyticsService.GetVideoAnalyticsAsync(videoId, userId, tenantId, filter);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}/list")]
    public async Task<ActionResult<List<VideoAnalyticsDto>>> GetVideoAnalyticsList(Guid videoId, [FromQuery] AnalyticsFilter filter)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var analytics = await _analyticsService.GetVideoAnalyticsListAsync(videoId, userId, tenantId, filter);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("overview")]
    public async Task<ActionResult<AnalyticsOverviewDto>> GetAnalyticsOverview([FromQuery] DateTimeOffset? startDate = null, [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var overview = await _analyticsService.GetAnalyticsOverviewAsync(userId, tenantId, startDate, endDate);
            return Ok(overview);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("popular-videos")]
    public async Task<ActionResult<List<PopularVideoDto>>> GetPopularVideos([FromQuery] int? limit = null, [FromQuery] DateTimeOffset? startDate = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var videos = await _analyticsService.GetPopularVideosAsync(userId, tenantId, limit, startDate);
            return Ok(videos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}/export")]
    public async Task<ActionResult<AnalyticsExportDto>> ExportAnalytics(Guid videoId, [FromQuery] AnalyticsFilter filter)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var export = await _analyticsService.ExportAnalyticsAsync(videoId, userId, tenantId, filter);
            
            return File(export.Data, export.ContentType, export.FileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}/retention")]
    public async Task<ActionResult<ViewerRetentionDto>> GetViewerRetention(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var retention = await _analyticsService.GetViewerRetentionAsync(videoId, userId, tenantId);
            return Ok(retention);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}/geographic")]
    public async Task<ActionResult<GeographicAnalyticsDto>> GetGeographicAnalytics(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var geographic = await _analyticsService.GetGeographicAnalyticsAsync(videoId, userId, tenantId);
            return Ok(geographic);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}/device")]
    public async Task<ActionResult<DeviceAnalyticsDto>> GetDeviceAnalytics(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var device = await _analyticsService.GetDeviceAnalyticsAsync(videoId, userId, tenantId);
            return Ok(device);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}/engagement")]
    public async Task<ActionResult<EngagementAnalyticsDto>> GetEngagementAnalytics(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var engagement = await _analyticsService.GetEngagementAnalyticsAsync(videoId, userId, tenantId);
            return Ok(engagement);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
