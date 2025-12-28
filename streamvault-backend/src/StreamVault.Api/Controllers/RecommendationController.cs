using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Recommendations;
using StreamVault.Application.Videos.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet("recommended")]
    public async Task<ActionResult<List<VideoListDto>>> GetRecommendedVideos([FromQuery] int limit = 10)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var videos = await _recommendationService.GetRecommendedVideosAsync(userId, tenantId, limit);
            return Ok(videos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("similar/{videoId}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<VideoListDto>>> GetSimilarVideos(Guid videoId, [FromQuery] int limit = 10)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var videos = await _recommendationService.GetSimilarVideosAsync(videoId, tenantId, limit);
            return Ok(videos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("trending")]
    [AllowAnonymous]
    public async Task<ActionResult<List<VideoListDto>>> GetTrendingVideos([FromQuery] int limit = 10)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var videos = await _recommendationService.GetTrendingVideosAsync(tenantId, limit);
            return Ok(videos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("popular")]
    [AllowAnonymous]
    public async Task<ActionResult<List<VideoListDto>>> GetPopularVideos([FromQuery] int limit = 10)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            var videos = await _recommendationService.GetPopularVideosAsync(tenantId, limit);
            return Ok(videos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("continue-watching")]
    public async Task<ActionResult<List<VideoListDto>>> GetContinueWatching([FromQuery] int limit = 10)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var videos = await _recommendationService.GetContinueWatchingAsync(userId, tenantId, limit);
            return Ok(videos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
