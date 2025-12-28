using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamVault.Application.SocialMedia;
using StreamVault.Application.SocialMedia.DTOs;
using System.Security.Claims;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SocialMediaController : ControllerBase
{
    private readonly ISocialMediaService _socialMediaService;

    public SocialMediaController(ISocialMediaService socialMediaService)
    {
        _socialMediaService = socialMediaService;
    }

    // Social Account Management
    [HttpPost("accounts/connect")]
    public async Task<ActionResult<bool>> ConnectSocialAccount([FromBody] ConnectSocialAccountRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _socialMediaService.ConnectSocialAccountAsync(userId, tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("accounts/{platform}/disconnect")]
    public async Task<ActionResult<bool>> DisconnectSocialAccount(string platform)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _socialMediaService.DisconnectSocialAccountAsync(userId, tenantId, platform);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("accounts")]
    public async Task<ActionResult<List<SocialAccountDto>>> GetUserSocialAccounts()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var accounts = await _socialMediaService.GetUserSocialAccountsAsync(userId, tenantId);
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Social Media Posts
    [HttpPost("posts")]
    public async Task<ActionResult<bool>> PostToSocialMedia([FromBody] PostToSocialMediaRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _socialMediaService.PostToSocialMediaAsync(userId, tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("posts")]
    public async Task<ActionResult<List<SocialPostDto>>> GetSocialPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var posts = await _socialMediaService.GetSocialPostsAsync(userId, tenantId, page, pageSize);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Video Sharing
    [HttpPost("videos/{videoId}/share")]
    public async Task<ActionResult<bool>> ShareVideo(Guid videoId, [FromBody] ShareVideoRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _socialMediaService.ShareVideoAsync(videoId, userId, tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("videos/shared")]
    public async Task<ActionResult<List<SharedVideoDto>>> GetSharedVideos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var videos = await _socialMediaService.GetSharedVideosAsync(userId, tenantId, page, pageSize);
            return Ok(videos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("videos/{videoId}/analytics")]
    public async Task<ActionResult<SocialAnalyticsDto>> GetSocialAnalytics(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var analytics = await _socialMediaService.GetSocialAnalyticsAsync(videoId, userId, tenantId);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Authentication
    [HttpGet("auth/{platform}/url")]
    public async Task<ActionResult<string>> GetAuthUrl(string platform, [FromQuery] string redirectUri)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var authUrl = await _socialMediaService.GetAuthUrlAsync(platform, tenantId, redirectUri);
            return Ok(authUrl);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("auth/{platform}/callback")]
    public async Task<ActionResult<SocialAuthResultDto>> AuthenticateCallback(
        string platform,
        [FromQuery] string code,
        [FromQuery] string state)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _socialMediaService.AuthenticateAsync(platform, code, state, tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("auth/{platform}/refresh")]
    public async Task<ActionResult<bool>> RefreshToken(string platform)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _socialMediaService.RefreshTokenAsync(userId, tenantId, platform);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Content Synchronization
    [HttpPost("sync/{platform}")]
    public async Task<ActionResult<bool>> SyncVideosFromSocial(string platform)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _socialMediaService.SyncVideosFromSocialAsync(userId, tenantId, platform);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("import")]
    public async Task<ActionResult<bool>> ImportSocialVideos([FromBody] ImportSocialVideosRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _socialMediaService.ImportSocialVideosAsync(userId, tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("imported")]
    public async Task<ActionResult<List<ImportedVideoDto>>> GetImportedVideos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var videos = await _socialMediaService.GetImportedVideosAsync(userId, tenantId, page, pageSize);
            return Ok(videos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Analytics
    [HttpGet("analytics")]
    public async Task<ActionResult<SocialMediaAnalyticsDto>> GetSocialMediaAnalytics(
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var analytics = await _socialMediaService.GetSocialMediaAnalyticsAsync(userId, tenantId, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("videos/{videoId}/engagement")]
    public async Task<ActionResult<List<SocialEngagementDto>>> GetEngagementMetrics(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var metrics = await _socialMediaService.GetEngagementMetricsAsync(videoId, userId, tenantId);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("trends/{platform}")]
    public async Task<ActionResult<SocialTrendsDto>> GetSocialTrends(string platform)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var trends = await _socialMediaService.GetSocialTrendsAsync(tenantId, platform);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Scheduled Posts
    [HttpPost("schedule")]
    public async Task<ActionResult<Guid>> SchedulePost([FromBody] SchedulePostRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var postId = await _socialMediaService.SchedulePostAsync(userId, tenantId, request);
            return Ok(postId);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("schedule/{postId}")]
    public async Task<ActionResult<bool>> UpdateScheduledPost(Guid postId, [FromBody] UpdateScheduleRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _socialMediaService.UpdateScheduledPostAsync(postId, userId, tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("schedule/{postId}")]
    public async Task<ActionResult<bool>> CancelScheduledPost(Guid postId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _socialMediaService.CancelScheduledPostAsync(postId, userId, tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("schedule")]
    public async Task<ActionResult<List<ScheduledPostDto>>> GetScheduledPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var posts = await _socialMediaService.GetScheduledPostsAsync(userId, tenantId, page, pageSize);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Comments
    [HttpGet("videos/{videoId}/comments/{platform}")]
    public async Task<ActionResult<List<SocialCommentDto>>> GetSocialComments(Guid videoId, string platform)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var comments = await _socialMediaService.GetSocialCommentsAsync(videoId, userId, tenantId, platform);
            return Ok(comments);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("videos/{videoId}/comments/reply")]
    public async Task<ActionResult<bool>> ReplyToSocialComment(Guid videoId, [FromBody] ReplyToCommentRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _socialMediaService.ReplyToSocialCommentAsync(videoId, userId, tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("comments/{commentId}/moderate")]
    public async Task<ActionResult<bool>> ModerateSocialComment(string commentId, [FromBody] ModerateCommentRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _socialMediaService.ModerateSocialCommentAsync(Guid.Parse(commentId), userId, tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Hashtags
    [HttpPost("hashtags/track")]
    public async Task<ActionResult<bool>> TrackHashtag([FromBody] TrackHashtagRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _socialMediaService.TrackHashtagAsync(userId, tenantId, request.Platform, request.Hashtag);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("hashtags/{platform}/analytics")]
    public async Task<ActionResult<List<HashtagAnalyticsDto>>> GetHashtagAnalytics(string platform)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var analytics = await _socialMediaService.GetHashtagAnalyticsAsync(userId, tenantId, platform);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("hashtags/{platform}/trending")]
    public async Task<ActionResult<List<TrendingHashtagDto>>> GetTrendingHashtags(string platform)
    {
        try
        {
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var hashtags = await _socialMediaService.GetTrendingHashtagsAsync(tenantId, platform);
            return Ok(hashtags);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Mentions
    [HttpGet("mentions/{platform}")]
    public async Task<ActionResult<List<SocialMentionDto>>> GetMentions(
        string platform,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var mentions = await _socialMediaService.GetMentionsAsync(userId, tenantId, platform, page, pageSize);
            return Ok(mentions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("mentions/{mentionId}/respond")]
    public async Task<ActionResult<bool>> RespondToMention(string mentionId, [FromBody] RespondToMentionRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var result = await _socialMediaService.RespondToMentionAsync(Guid.Parse(mentionId), userId, tenantId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Sentiment Analysis
    [HttpGet("videos/{videoId}/sentiment")]
    public async Task<ActionResult<SocialSentimentDto>> GetSentimentAnalysis(Guid videoId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var tenantId = Guid.Parse(User.FindFirst("tenant_id")?.Value ?? "");

            var sentiment = await _socialMediaService.GetSentimentAnalysisAsync(videoId, userId, tenantId);
            return Ok(sentiment);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class TrackHashtagRequest
{
    [Required]
    public string Platform { get; set; } = string.Empty;
    
    [Required]
    public string Hashtag { get; set; } = string.Empty;
}
