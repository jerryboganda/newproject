using StreamVault.Application.SocialMedia.DTOs;

namespace StreamVault.Application.SocialMedia;

public interface ISocialMediaService
{
    // Social Media Platform Integration
    Task<bool> ConnectSocialAccountAsync(Guid userId, Guid tenantId, ConnectSocialAccountRequest request);
    Task<bool> DisconnectSocialAccountAsync(Guid userId, Guid tenantId, string platform);
    Task<List<SocialAccountDto>> GetUserSocialAccountsAsync(Guid userId, Guid tenantId);
    Task<bool> PostToSocialMediaAsync(Guid userId, Guid tenantId, PostToSocialMediaRequest request);
    Task<List<SocialPostDto>> GetSocialPostsAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20);
    
    // Video Sharing to Social Media
    Task<bool> ShareVideoAsync(Guid videoId, Guid userId, Guid tenantId, ShareVideoRequest request);
    Task<List<SharedVideoDto>> GetSharedVideosAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20);
    Task<SocialAnalyticsDto> GetSocialAnalyticsAsync(Guid videoId, Guid userId, Guid tenantId);
    
    // Social Media Authentication
    Task<string> GetAuthUrlAsync(string platform, Guid tenantId, string redirectUri);
    Task<SocialAuthResultDto> AuthenticateAsync(string platform, string code, string state, Guid tenantId);
    Task<bool> RefreshTokenAsync(Guid userId, Guid tenantId, string platform);
    
    // Content Synchronization
    Task<bool> SyncVideosFromSocialAsync(Guid userId, Guid tenantId, string platform);
    Task<bool> ImportSocialVideosAsync(Guid userId, Guid tenantId, ImportSocialVideosRequest request);
    Task<List<ImportedVideoDto>> GetImportedVideosAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20);
    
    // Social Media Analytics
    Task<SocialMediaAnalyticsDto> GetSocialMediaAnalyticsAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
    Task<List<SocialEngagementDto>> GetEngagementMetricsAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<SocialTrendsDto> GetSocialTrendsAsync(Guid tenantId, string platform);
    
    // Scheduled Posts
    Task<Guid> SchedulePostAsync(Guid userId, Guid tenantId, SchedulePostRequest request);
    Task<bool> UpdateScheduledPostAsync(Guid postId, Guid userId, Guid tenantId, UpdateScheduleRequest request);
    Task<bool> CancelScheduledPostAsync(Guid postId, Guid userId, Guid tenantId);
    Task<List<ScheduledPostDto>> GetScheduledPostsAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20);
    
    // Social Media Comments
    Task<List<SocialCommentDto>> GetSocialCommentsAsync(Guid videoId, Guid userId, Guid tenantId, string platform);
    Task<bool> ReplyToSocialCommentAsync(Guid videoId, Guid userId, Guid tenantId, ReplyToCommentRequest request);
    Task<bool> ModerateSocialCommentAsync(Guid commentId, Guid userId, Guid tenantId, ModerateCommentRequest request);
    
    // Hashtag Management
    Task<bool> TrackHashtagAsync(Guid userId, Guid tenantId, string platform, string hashtag);
    Task<List<HashtagAnalyticsDto>> GetHashtagAnalyticsAsync(Guid userId, Guid tenantId, string platform);
    Task<List<TrendingHashtagDto>> GetTrendingHashtagsAsync(Guid tenantId, string platform);
    
    // Social Media Monitoring
    Task<List<SocialMentionDto>> GetMentionsAsync(Guid userId, Guid tenantId, string platform, int page = 1, int pageSize = 20);
    Task<bool> RespondToMentionAsync(Guid mentionId, Guid userId, Guid tenantId, RespondToMentionRequest request);
    Task<SocialSentimentDto> GetSentimentAnalysisAsync(Guid videoId, Guid userId, Guid tenantId);
}
