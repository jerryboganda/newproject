using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.Recommendations;
using StreamVault.Application.Recommendations.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class VideoRecommendationController : ControllerBase
{
    private readonly IVideoRecommendationService _recommendationService;

    public VideoRecommendationController(IVideoRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet("for-you")]
    public async Task<ActionResult<List<RecommendedVideoDto>>> GetRecommendationsForUser(
        [FromQuery] int limit = 20,
        [FromQuery] string? algorithm = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var recommendations = await _recommendationService.GetRecommendationsForUserAsync(userId, tenantId, limit, algorithm);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("similar/{videoId}")]
    public async Task<ActionResult<List<RecommendedVideoDto>>> GetSimilarVideos(
        Guid videoId,
        [FromQuery] int limit = 10)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var similarVideos = await _recommendationService.GetSimilarVideosAsync(videoId, userId, tenantId, limit);
            return Ok(similarVideos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("trending")]
    public async Task<ActionResult<List<RecommendedVideoDto>>> GetTrendingVideos(
        [FromQuery] int limit = 20,
        [FromQuery] string? category = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var trendingVideos = await _recommendationService.GetTrendingVideosAsync(tenantId, limit, category);
            return Ok(trendingVideos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("feed")]
    public async Task<ActionResult<List<RecommendedVideoDto>>> GetPersonalizedFeed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var feed = await _recommendationService.GetPersonalizedFeedAsync(userId, tenantId, page, pageSize);
            return Ok(feed);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("continue-watching")]
    public async Task<ActionResult<List<RecommendedVideoDto>>> GetContinueWatching(
        [FromQuery] int limit = 10)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var continueWatching = await _recommendationService.GetContinueWatchingAsync(userId, tenantId, limit);
            return Ok(continueWatching);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("watch-again")]
    public async Task<ActionResult<List<RecommendedVideoDto>>> GetWatchAgain(
        [FromQuery] int limit = 10)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var watchAgain = await _recommendationService.GetWatchAgainAsync(userId, tenantId, limit);
            return Ok(watchAgain);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("recommended-for-you")]
    public async Task<ActionResult<List<RecommendedVideoDto>>> GetRecommendedForYou(
        [FromQuery] int limit = 20)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var recommendations = await _recommendationService.GetRecommendedForYouAsync(userId, tenantId, limit);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("category/{categoryId}/popular")]
    public async Task<ActionResult<List<RecommendedVideoDto>>> GetPopularInCategory(
        Guid categoryId,
        [FromQuery] int limit = 20)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var popularVideos = await _recommendationService.GetPopularInCategoryAsync(categoryId, userId, tenantId, limit);
            return Ok(popularVideos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("from-creators")]
    public async Task<ActionResult<List<RecommendedVideoDto>>> GetRecommendedFromCreators(
        [FromBody] List<Guid> creatorIds,
        [FromQuery] int limit = 20)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var recommendations = await _recommendationService.GetRecommendedFromCreatorsAsync(creatorIds, userId, tenantId, limit);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("watch-history")]
    public async Task<ActionResult<bool>> UpdateWatchHistory(
        [FromBody] UpdateWatchHistoryRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _recommendationService.UpdateWatchHistoryAsync(
                userId, 
                request.VideoId, 
                tenantId, 
                request.WatchedSeconds, 
                request.TotalSeconds);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("interaction")]
    public async Task<ActionResult<bool>> RecordUserInteraction(
        [FromBody] RecordInteractionRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _recommendationService.RecordUserInteractionAsync(
                userId, 
                request.VideoId, 
                tenantId, 
                request.InteractionType, 
                request.Metadata);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<RecommendedVideoDto>>> GetSearchRecommendations(
        [FromQuery] string query,
        [FromQuery] int limit = 10)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var recommendations = await _recommendationService.GetSearchRecommendationsAsync(userId, tenantId, query, limit);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class UpdateWatchHistoryRequest
{
    public Guid VideoId { get; set; }
    public int WatchedSeconds { get; set; }
    public int TotalSeconds { get; set; }
}

public class RecordInteractionRequest
{
    public Guid VideoId { get; set; }
    public string InteractionType { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}
