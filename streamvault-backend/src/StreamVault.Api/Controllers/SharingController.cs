using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Sharing;
using StreamVault.Application.Sharing.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SharingController : ControllerBase
{
    private readonly ISharingService _sharingService;

    public SharingController(ISharingService sharingService)
    {
        _sharingService = sharingService;
    }

    [HttpPost]
    public async Task<ActionResult<ShareLinkDto>> GenerateShareLink([FromBody] CreateShareLinkRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var shareLink = await _sharingService.GenerateShareLinkAsync(request.VideoId, request, userId, tenantId);
            return Ok(shareLink);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}")]
    public async Task<ActionResult<List<ShareLinkDto>>> GetVideoShareLinks(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var shareLinks = await _sharingService.GetVideoShareLinksAsync(videoId, userId, tenantId);
            return Ok(shareLinks);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{shareId}/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<ShareLinkDto>> GetShareLink(Guid shareId, string token)
    {
        try
        {
            var shareLink = await _sharingService.GetShareLinkAsync(shareId, token);
            
            if (shareLink == null)
                return NotFound();

            return Ok(shareLink);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{shareId}")]
    public async Task<IActionResult> UpdateShareLink(Guid shareId, [FromBody] UpdateShareLinkRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _sharingService.UpdateShareLinkAsync(shareId, request, userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{shareId}")]
    public async Task<IActionResult> DeleteShareLink(Guid shareId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _sharingService.DeleteShareLinkAsync(shareId, userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("shared/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<VideoDto>> GetSharedVideo(string token)
    {
        try
        {
            var video = await _sharingService.GetSharedVideoAsync(token);
            
            if (video == null)
                return NotFound();

            return Ok(video);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
