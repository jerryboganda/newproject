using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Moderation;
using StreamVault.Application.Moderation.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ModerationController : ControllerBase
{
    private readonly IVideoModerationService _moderationService;

    public ModerationController(IVideoModerationService moderationService)
    {
        _moderationService = moderationService;
    }

    [HttpPost("videos/{videoId}/moderate")]
    public async Task<ActionResult<ModerationResultDto>> ModerateVideo(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _moderationService.ModerateVideoAsync(videoId, tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("videos/{videoId}/flags")]
    public async Task<ActionResult<List<ModerationFlagDto>>> GetVideoFlags(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var flags = await _moderationService.GetVideoFlagsAsync(videoId, tenantId);
            return Ok(flags);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("videos/{videoId}/approve")]
    public async Task<ActionResult<bool>> ApproveVideo(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _moderationService.ApproveVideoAsync(videoId, tenantId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("videos/{videoId}/reject")]
    public async Task<ActionResult<bool>> RejectVideo(Guid videoId, [FromBody] RejectVideoRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _moderationService.RejectVideoAsync(videoId, tenantId, userId, request.Reason);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("videos/{videoId}/report")]
    public async Task<ActionResult<bool>> ReportVideo(Guid videoId, [FromBody] ReportVideoRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _moderationService.ReportVideoAsync(videoId, userId, tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("reported-videos")]
    public async Task<ActionResult<List<ReportedVideoDto>>> GetReportedVideos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var reportedVideos = await _moderationService.GetReportedVideosAsync(tenantId, page, pageSize);
            return Ok(reportedVideos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("pending-review")]
    public async Task<ActionResult<List<PendingModerationDto>>> GetPendingModeration(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var pendingVideos = await _moderationService.GetPendingModerationAsync(tenantId, page, pageSize);
            return Ok(pendingVideos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ModerationStatsDto>> GetModerationStats(
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var stats = await _moderationService.GetModerationStatsAsync(tenantId, startDate, endDate);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("settings")]
    public async Task<ActionResult<bool>> UpdateModerationSettings([FromBody] ModerationSettingsDto settings)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _moderationService.UpdateModerationSettingsAsync(tenantId, settings);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("settings")]
    public async Task<ActionResult<ModerationSettingsDto>> GetModerationSettings()
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var settings = await _moderationService.GetModerationSettingsAsync(tenantId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("videos/{videoId}/history")]
    public async Task<ActionResult<List<ModerationActionDto>>> GetModerationHistory(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var history = await _moderationService.GetModerationHistoryAsync(videoId, tenantId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("videos/{videoId}/appeal")]
    public async Task<ActionResult<bool>> AppealModerationDecision(Guid videoId, [FromBody] AppealRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _moderationService.AppealModerationDecisionAsync(videoId, userId, tenantId, request.Reason);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("auto-moderated")]
    public async Task<ActionResult<List<AutoModeratedContentDto>>> GetAutoModeratedContent(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var content = await _moderationService.GetAutoModeratedContentAsync(tenantId, page, pageSize);
            return Ok(content);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("batch/approve")]
    public async Task<ActionResult<BatchModerationResult>> BatchApproveVideos([FromBody] BatchModerationRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = new BatchModerationResult
            {
                TotalVideos = request.VideoIds.Count,
                ProcessedVideos = 0,
                FailedVideos = 0,
                SuccessfulVideoIds = new List<Guid>(),
                Errors = new List<string>()
            };

            foreach (var videoId in request.VideoIds)
            {
                try
                {
                    await _moderationService.ApproveVideoAsync(videoId, tenantId, userId);
                    result.SuccessfulVideoIds.Add(videoId);
                    result.ProcessedVideos++;
                }
                catch (Exception ex)
                {
                    result.FailedVideos++;
                    result.Errors.Add($"Video {videoId}: {ex.Message}");
                }
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("batch/reject")]
    public async Task<ActionResult<BatchModerationResult>> BatchRejectVideos([FromBody] BatchModerationRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = new BatchModerationResult
            {
                TotalVideos = request.VideoIds.Count,
                ProcessedVideos = 0,
                FailedVideos = 0,
                SuccessfulVideoIds = new List<Guid>(),
                Errors = new List<string>()
            };

            foreach (var videoId in request.VideoIds)
            {
                try
                {
                    await _moderationService.RejectVideoAsync(videoId, tenantId, userId, request.Reason);
                    result.SuccessfulVideoIds.Add(videoId);
                    result.ProcessedVideos++;
                }
                catch (Exception ex)
                {
                    result.FailedVideos++;
                    result.Errors.Add($"Video {videoId}: {ex.Message}");
                }
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class RejectVideoRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class AppealRequest
{
    public string Reason { get; set; } = string.Empty;
}
