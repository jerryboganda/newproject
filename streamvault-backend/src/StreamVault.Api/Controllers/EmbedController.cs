using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Embed;
using StreamVault.Application.Embed.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class EmbedController : ControllerBase
{
    private readonly IEmbedService _embedService;

    public EmbedController(IEmbedService embedService)
    {
        _embedService = embedService;
    }

    [HttpGet("video/{videoId}/config")]
    public async Task<ActionResult<EmbedConfigDto>> GetEmbedConfig(Guid videoId, [FromQuery] EmbedOptionsDto options)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var config = await _embedService.GetEmbedConfigAsync(videoId, tenantId, options);
            return Ok(config);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}/code")]
    public async Task<ActionResult<string>> GenerateEmbedCode(Guid videoId, [FromQuery] EmbedOptionsDto options)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var embedCode = await _embedService.GenerateEmbedCodeAsync(videoId, tenantId, options);
            return Ok(embedCode);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}/analytics")]
    public async Task<ActionResult<List<EmbedAnalyticsDto>>> GetEmbedAnalytics(
        Guid videoId, 
        [FromQuery] DateTimeOffset? startDate = null, 
        [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var analytics = await _embedService.GetEmbedAnalyticsAsync(videoId, tenantId, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

// Public embed controller (no authorization required)
[ApiController]
[Route("api/v1/public/[controller]")]
public class PublicEmbedController : ControllerBase
{
    private readonly IEmbedService _embedService;

    public PublicEmbedController(IEmbedService embedService)
    {
        _embedService = embedService;
    }

    [HttpGet("video/{videoId}/config")]
    [AllowAnonymous]
    public async Task<ActionResult<EmbedConfigDto>> GetPublicEmbedConfig(Guid videoId, [FromQuery] EmbedOptionsDto options)
    {
        try
        {
            // For public embed, we need to verify the video is public
            // This is a simplified implementation
            var tenantId = Guid.Empty; // You might extract this from subdomain or other means
            var config = await _embedService.GetEmbedConfigAsync(videoId, tenantId, options);
            return Ok(config);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
