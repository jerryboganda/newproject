using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.SEO;
using StreamVault.Application.SEO.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class VideoSEOController : ControllerBase
{
    private readonly IVideoSEOService _seoService;

    public VideoSEOController(IVideoSEOService seoService)
    {
        _seoService = seoService;
    }

    [HttpGet("video/{videoId}")]
    public async Task<ActionResult<VideoSEODto>> GetVideoSEO(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var seo = await _seoService.GetVideoSEOAsync(videoId, userId, tenantId);
            return Ok(seo);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("video/{videoId}")]
    public async Task<ActionResult<VideoSEODto>> UpdateVideoSEO(Guid videoId, [FromBody] UpdateVideoSEORequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var seo = await _seoService.UpdateVideoSEOAsync(videoId, request, userId, tenantId);
            return Ok(seo);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("video/{videoId}/generate")]
    public async Task<ActionResult<VideoSEODto>> GenerateSEO(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var seo = await _seoService.GenerateSEOAsync(videoId, userId, tenantId);
            return Ok(seo);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("sitemap")]
    public async Task<ActionResult<string>> GenerateSitemap()
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var sitemap = await _seoService.GenerateSitemapAsync(tenantId);
            return Content(sitemap, "application/xml");
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("robots.txt")]
    public async Task<ActionResult<string>> GenerateRobotsTxt()
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var robotsTxt = await _seoService.GenerateRobotsTxtAsync(tenantId);
            return Content(robotsTxt, "text/plain");
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}/keywords")]
    public async Task<ActionResult<List<VideoSearchKeywordDto>>> GetSearchKeywords(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var keywords = await _seoService.GetSearchKeywordsAsync(videoId, userId, tenantId);
            return Ok(keywords);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}/backlinks")]
    public async Task<ActionResult<List<VideoBacklinkDto>>> GetBacklinks(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var backlinks = await _seoService.GetBacklinksAsync(videoId, userId, tenantId);
            return Ok(backlinks);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}/score")]
    public async Task<ActionResult<SEOScoreDto>> CalculateSEOScore(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var score = await _seoService.CalculateSEOScoreAsync(videoId, userId, tenantId);
            return Ok(score);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}/recommendations")]
    public async Task<ActionResult<List<SEORecommendationDto>>> GetSEORecommendations(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var recommendations = await _seoService.GetSEORecommendationsAsync(videoId, userId, tenantId);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("video/{videoId}/submit")]
    public async Task<ActionResult<bool>> SubmitToSearchEngines(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var result = await _seoService.SubmitToSearchEnginesAsync(videoId, userId, tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("video/{videoId}/analytics")]
    public async Task<ActionResult<SEOAnalyticsDto>> GetSEOAnalytics(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            var analytics = await _seoService.GetSEOAnalyticsAsync(videoId, userId, tenantId);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("video/{videoId}/index")]
    public async Task<IActionResult> IndexVideo(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");
            
            await _seoService.IndexVideoAsync(videoId, userId, tenantId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
