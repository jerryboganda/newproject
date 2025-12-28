using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.VideoSettings;
using StreamVault.Application.VideoSettings.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class VideoSettingsController : ControllerBase
{
    private readonly IVideoSettingsService _videoSettingsService;

    public VideoSettingsController(IVideoSettingsService videoSettingsService)
    {
        _videoSettingsService = videoSettingsService;
    }

    [HttpGet("video/{videoId}")]
    public async Task<ActionResult<VideoSettingsDto>> GetSettings(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var settings = await _videoSettingsService.GetSettingsAsync(videoId, userId, tenantId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("video/{videoId}")]
    public async Task<ActionResult<VideoSettingsDto>> UpdateSettings(Guid videoId, [FromBody] UpdateVideoSettingsRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var settings = await _videoSettingsService.UpdateSettingsAsync(videoId, request, userId, tenantId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("video/{videoId}/playback-speed")]
    public async Task<ActionResult<VideoSettingsDto>> UpdatePlaybackSpeed(Guid videoId, [FromBody] PlaybackSpeedRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var settings = await _videoSettingsService.UpdatePlaybackSpeedAsync(videoId, request.Speed, userId, tenantId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("video/{videoId}/volume")]
    public async Task<ActionResult<VideoSettingsDto>> UpdateVolume(Guid videoId, [FromBody] VolumeRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var settings = await _videoSettingsService.UpdateVolumeAsync(videoId, request.Volume, userId, tenantId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("video/{videoId}/position")]
    public async Task<ActionResult<VideoSettingsDto>> UpdatePosition(Guid videoId, [FromBody] PositionRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var settings = await _videoSettingsService.UpdatePositionAsync(videoId, request.PositionSeconds, userId, tenantId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("video/{videoId}")]
    public async Task<IActionResult> ResetSettings(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _videoSettingsService.ResetSettingsAsync(videoId, userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
