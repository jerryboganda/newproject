using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.AI;
using StreamVault.Application.AI.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AITaggingController : ControllerBase
{
    private readonly IVideoTaggingService _taggingService;

    public AITaggingController(IVideoTaggingService taggingService)
    {
        _taggingService = taggingService;
    }

    [HttpPost("videos/{videoId}/generate-tags")]
    public async Task<ActionResult<List<GeneratedTagDto>>> GenerateTags(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var tags = await _taggingService.GenerateTagsAsync(videoId, tenantId);
            return Ok(tags);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("generate-tags/transcript")]
    public async Task<ActionResult<List<GeneratedTagDto>>> GenerateTagsFromTranscript([FromBody] GenerateTagsFromTranscriptRequest request)
    {
        try
        {
            var tags = await _taggingService.GenerateTagsFromTranscriptAsync(request.Transcript);
            return Ok(tags);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("videos/{videoId}/generate-tags/thumbnail")]
    public async Task<ActionResult<List<GeneratedTagDto>>> GenerateTagsFromThumbnail(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var tags = await _taggingService.GenerateTagsFromThumbnailAsync(videoId, tenantId);
            return Ok(tags);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("videos/{videoId}/generate-tags/audio")]
    public async Task<ActionResult<List<GeneratedTagDto>>> GenerateTagsFromAudio(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var tags = await _taggingService.GenerateTagsFromAudioAsync(videoId, tenantId);
            return Ok(tags);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("generate-tags/metadata")]
    public async Task<ActionResult<List<GeneratedTagDto>>> GenerateTagsFromMetadata([FromBody] VideoMetadataDto metadata)
    {
        try
        {
            var tags = await _taggingService.GenerateTagsFromMetadataAsync(metadata);
            return Ok(tags);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("videos/{videoId}/suggest-categories")]
    public async Task<ActionResult<List<GeneratedCategoryDto>>> SuggestCategories(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var categories = await _taggingService.SuggestCategoriesAsync(videoId, tenantId);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("videos/{videoId}/analyze")]
    public async Task<ActionResult<VideoContentAnalysisDto>> AnalyzeVideoContent(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var analysis = await _taggingService.AnalyzeVideoContentAsync(videoId, tenantId);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("videos/{videoId}/apply-tags")]
    public async Task<ActionResult<bool>> ApplyGeneratedTags(Guid videoId, [FromBody] ApplyTagsRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _taggingService.ApplyGeneratedTagsAsync(videoId, request.TagIds, tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("videos/{videoId}/insights")]
    public async Task<ActionResult<List<VideoInsightDto>>> GenerateVideoInsights(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var insights = await _taggingService.GenerateVideoInsightsAsync(videoId, tenantId);
            return Ok(insights);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("trending-tags")]
    public async Task<ActionResult<List<TrendingTagDto>>> GetTrendingTags([FromQuery] int limit = 50)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var trendingTags = await _taggingService.GetTrendingTagsAsync(tenantId, limit);
            return Ok(trendingTags);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("videos/{videoId}/tag-suggestions")]
    public async Task<ActionResult<List<TagSuggestionDto>>> GetTagSuggestions(
        Guid videoId, 
        [FromQuery] string? query = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var suggestions = await _taggingService.GetTagSuggestionsAsync(videoId, tenantId, query);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("batch/generate-tags")]
    public async Task<ActionResult<BatchTagGenerationResult>> BatchGenerateTags([FromBody] BatchTagGenerationRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = new BatchTagGenerationResult
            {
                TotalVideos = request.VideoIds.Count,
                ProcessedVideos = 0,
                FailedVideos = 0,
                Results = new Dictionary<Guid, List<GeneratedTagDto>>(),
                Errors = new List<string>()
            };

            foreach (var videoId in request.VideoIds)
            {
                try
                {
                    var tags = await _taggingService.GenerateTagsAsync(videoId, tenantId);
                    
                    // Filter by confidence threshold
                    var filteredTags = tags
                        .Where(t => t.Confidence >= request.ConfidenceThreshold)
                        .Take(request.MaxTagsPerVideo)
                        .ToList();

                    result.Results[videoId] = filteredTags;
                    result.ProcessedVideos++;

                    // Auto-apply if requested
                    if (request.AutoApply && filteredTags.Any())
                    {
                        // This would need to create/get tag IDs first
                        // For now, just store in results
                    }
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

public class GenerateTagsFromTranscriptRequest
{
    public string Transcript { get; set; } = string.Empty;
}

public class ApplyTagsRequest
{
    public List<Guid> TagIds { get; set; } = new();
}
