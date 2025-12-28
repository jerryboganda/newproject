using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Videos;
using StreamVault.Application.Videos.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class VideoController : ControllerBase
{
    private readonly IVideoService _videoService;

    public VideoController(IVideoService videoService)
    {
        _videoService = videoService;
    }

    [HttpPost("upload/initiate")]
    public async Task<ActionResult<VideoUploadResponse>> InitiateUpload([FromBody] VideoUploadRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var response = await _videoService.InitiateUploadAsync(request, userId, tenantId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("upload/chunk")]
    public async Task<ActionResult<ChunkUploadResponse>> GetChunkUploadUrl([FromBody] ChunkUploadRequest request)
    {
        try
        {
            var response = await _videoService.GetChunkUploadUrlAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("upload/complete")]
    public async Task<IActionResult> CompleteUpload([FromBody] CompleteUploadRequest request)
    {
        try
        {
            await _videoService.CompleteUploadAsync(request.VideoId, request.UploadToken);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<VideoDto>> GetVideo(Guid id)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var video = await _videoService.GetVideoAsync(id, tenantId);
            
            if (video == null)
                return NotFound();

            // Increment view count for public videos
            if (video.IsPublic)
            {
                await _videoService.IncrementViewCountAsync(id, tenantId);
            }

            return Ok(video);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<VideoListResponse>> GetVideos([FromQuery] VideoListRequest request)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var response = await _videoService.GetVideosAsync(request, tenantId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VideoDto>> UpdateVideo(Guid id, [FromBody] VideoUpdateRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var video = await _videoService.UpdateVideoAsync(id, request, userId, tenantId);
            return Ok(video);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVideo(Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _videoService.DeleteVideoAsync(id, userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class CompleteUploadRequest
{
    public string VideoId { get; set; } = string.Empty;
    public string UploadToken { get; set; } = string.Empty;
}
