using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Thumbnails;
using StreamVault.Application.Thumbnails.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ThumbnailController : ControllerBase
{
    private readonly IThumbnailGeneratorService _thumbnailGeneratorService;

    public ThumbnailController(IThumbnailGeneratorService thumbnailGeneratorService)
    {
        _thumbnailGeneratorService = thumbnailGeneratorService;
    }

    [HttpPost("video/{videoId}/generate")]
    public async Task<ActionResult<List<VideoThumbnailDto>>> GenerateThumbnails(Guid videoId, [FromBody] GenerateThumbnailsRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var thumbnails = await _thumbnailGeneratorService.GenerateThumbnailsAsync(videoId, request, userId, tenantId);
            return Ok(thumbnails);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("video/{videoId}/position")]
    public async Task<ActionResult<VideoThumbnailDto>> GenerateThumbnailAtPosition(Guid videoId, [FromBody] GenerateThumbnailAtPositionRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var thumbnail = await _thumbnailGeneratorService.GenerateThumbnailAtPositionAsync(videoId, request.PositionSeconds, request.Options, userId, tenantId);
            return Ok(thumbnail);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("video/{videoId}/upload")]
    public async Task<ActionResult<VideoThumbnailDto>> UploadCustomThumbnail(Guid videoId, [FromForm] UploadThumbnailRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var thumbnail = await _thumbnailGeneratorService.UploadCustomThumbnailAsync(videoId, request, userId, tenantId);
            return Ok(thumbnail);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}")]
    public async Task<ActionResult<List<VideoThumbnailDto>>> GetThumbnails(Guid videoId)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var thumbnails = await _thumbnailGeneratorService.GetThumbnailsAsync(videoId, tenantId);
            return Ok(thumbnails);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("video/{videoId}/default/{thumbnailId}")]
    public async Task<IActionResult> SetDefaultThumbnail(Guid videoId, Guid thumbnailId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _thumbnailGeneratorService.SetDefaultThumbnailAsync(videoId, thumbnailId, userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{thumbnailId}")]
    public async Task<IActionResult> DeleteThumbnail(Guid thumbnailId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _thumbnailGeneratorService.DeleteThumbnailAsync(thumbnailId, userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("video/{videoId}/sprite")]
    public async Task<ActionResult<string>> GenerateSpriteSheet(Guid videoId, [FromBody] SpriteSheetRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var spriteSheetUrl = await _thumbnailGeneratorService.GenerateSpriteSheetAsync(videoId, request, userId, tenantId);
            return Ok(new { spriteSheetUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class GenerateThumbnailAtPositionRequest
{
    public int PositionSeconds { get; set; }
    public ThumbnailOptions Options { get; set; } = new();
}
