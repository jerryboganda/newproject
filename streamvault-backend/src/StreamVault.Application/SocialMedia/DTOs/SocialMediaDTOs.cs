using System.ComponentModel.DataAnnotations;

namespace StreamVault.Application.SocialMedia.DTOs;

public class ConnectSocialAccountRequest
{
    [Required]
    public string Platform { get; set; } = string.Empty; // facebook, twitter, instagram, youtube, tiktok, linkedin
    
    [Required]
    public string AccessToken { get; set; } = string.Empty;
    
    public string? RefreshToken { get; set; }
    
    public DateTimeOffset? TokenExpiresAt { get; set; }
    
    public string? UserId { get; set; } // Platform-specific user ID
    
    public string? Username { get; set; }
    
    public Dictionary<string, object>? AdditionalData { get; set; }
}

public class SocialAccountDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string PlatformUserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset ConnectedAt { get; set; }
    public DateTimeOffset? LastSyncAt { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public int PostCount { get; set; }
}

public class PostToSocialMediaRequest
{
    [Required]
    public List<string> Platforms { get; set; } = new();
    
    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
    
    public string? VideoUrl { get; set; }
    
    public List<string> MediaUrls { get; set; } = new();
    
    public List<string> Hashtags { get; set; } = new();
    
    public List<string> Mentions { get; set; } = new();
    
    public bool SchedulePost { get; set; } = false;
    
    public DateTimeOffset? ScheduledAt { get; set; }
    
    public Dictionary<string, object>? PlatformSpecificSettings { get; set; }
}

public class SocialPostDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string PlatformPostId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public List<string> MediaUrls { get; set; } = new();
    public List<string> Hashtags { get; set; } = new();
    public string PostUrl { get; set; } = string.Empty;
    public DateTimeOffset PostedAt { get; set; }
    public int Likes { get; set; }
    public int Shares { get; set; }
    public int Comments { get; set; }
    public int Views { get; set; }
    public double EngagementRate { get; set; }
    public PostStatus Status { get; set; }
}

public class ShareVideoRequest
{
    [Required]
    public List<string> Platforms { get; set; } = new();
    
    [Required, MaxLength(500)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    public string? ThumbnailUrl { get; set; }
    
    public List<string> Hashtags { get; set; } = new();
    
    public List<string> Mentions { get; set; } = new();
    
    public bool AddWatermark { get; set; } = true;
    
    public bool IncludeCallToAction { get; set; } = false;
    
    public string? CallToActionText { get; set; }
    
    public string? CallToActionUrl { get; set; }
    
    public bool SchedulePost { get; set; } = false;
    
    public DateTimeOffset? ScheduledAt { get; set; }
    
    public Dictionary<string, object>? PlatformSpecificSettings { get; set; }
}

public class SharedVideoDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public string VideoTitle { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string PostUrl { get; set; } = string.Empty;
    public DateTimeOffset SharedAt { get; set; }
    public int Likes { get; set; }
    public int Shares { get; set; }
    public int Comments { get; set; }
    public int Views { get; set; }
    public ShareStatus Status { get; set; }
}

public class SocialAnalyticsDto
{
    public Guid VideoId { get; set; }
    public Dictionary<string, SocialPlatformStats> PlatformStats { get; set; } = new();
    public int TotalLikes { get; set; }
    public int TotalShares { get; set; }
    public int TotalComments { get; set; }
    public int TotalViews { get; set; }
    public double OverallEngagementRate { get; set; }
    public List<EngagementTimelineDto> EngagementTimeline { get; set; } = new();
    public List<PlatformDemographicsDto> Demographics { get; set; } = new();
}

public class SocialPlatformStats
{
    public string Platform { get; set; } = string.Empty;
    public int Likes { get; set; }
    public int Shares { get; set; }
    public int Comments { get; set; }
    public int Views { get; set; }
    public double EngagementRate { get; set; }
    public double Reach { get; set; }
    public double Impressions { get; set; }
}

public class EngagementTimelineDto
{
    public DateTimeOffset Date { get; set; }
    public int Likes { get; set; }
    public int Shares { get; set; }
    public int Comments { get; set; }
    public int Views { get; set; }
}

public class PlatformDemographicsDto
{
    public string Platform { get; set; } = string.Empty;
    public Dictionary<string, double> AgeGroups { get; set; } = new();
    public Dictionary<string, double> Genders { get; set; } = new();
    public Dictionary<string, double> Countries { get; set; } = new();
    public Dictionary<string, double> Languages { get; set; } = new();
}

public class SocialAuthResultDto
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset? TokenExpiresAt { get; set; }
    public string? PlatformUserId { get; set; }
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ImportSocialVideosRequest
{
    [Required]
    public string Platform { get; set; } = string.Empty;
    
    public List<string> VideoIds { get; set; } = new();
    
    public bool ImportAll { get; set; } = false;
    
    public DateTimeOffset? SinceDate { get; set; }
    
    public bool ImportPrivateVideos { get; set; } = false;
    
    public bool ImportAnalytics { get; set; } = true;
    
    public string? DestinationFolder { get; set; }
}

public class ImportedVideoDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public string OriginalPlatform { get; set; } = string.Empty;
    public string OriginalVideoId { get; set; } = string.Empty;
    public string OriginalUrl { get; set; } = string.Empty;
    public DateTimeOffset ImportedAt { get; set; }
    public int OriginalViews { get; set; }
    public int OriginalLikes { get; set; }
    public int OriginalComments { get; set; }
    public ImportStatus Status { get; set; }
}

public class SocialMediaAnalyticsDto
{
    public Dictionary<string, SocialPlatformAnalytics> Platforms { get; set; } = new();
    public int TotalPosts { get; set; }
    public int TotalEngagement { get; set; }
    public double AverageEngagementRate { get; set; }
    public int FollowerGrowth { get; set; }
    public List<PostPerformanceDto> TopPosts { get; set; } = new();
    public List<HashtagPerformanceDto> TopHashtags { get; set; } = new();
}

public class SocialPlatformAnalytics
{
    public string Platform { get; set; } = string.Empty;
    public int Posts { get; set; }
    public int Followers { get; set; }
    public int Following { get; set; }
    public double EngagementRate { get; set; }
    public int Reach { get; set; }
    public int Impressions { get; set; }
    public double GrowthRate { get; set; }
}

public class PostPerformanceDto
{
    public Guid PostId { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Engagement { get; set; }
    public double EngagementRate { get; set; }
    public DateTimeOffset PostedAt { get; set; }
}

public class HashtagPerformanceDto
{
    public string Hashtag { get; set; } = string.Empty;
    public int Uses { get; set; }
    public int Reach { get; set; }
    public double EngagementRate { get; set; }
    public List<string> Platforms { get; set; } = new();
}

public class SocialEngagementDto
{
    public string Platform { get; set; } = string.Empty;
    public int Likes { get; set; }
    public int Comments { get; set; }
    public int Shares { get; set; }
    public int Saves { get; set; }
    public int Clicks { get; set; }
    public double EngagementRate { get; set; }
    public List<EngagementHourDto> HourlyBreakdown { get; set; } = new();
}

public class EngagementHourDto
{
    public int Hour { get; set; }
    public int Engagement { get; set; }
}

public class SocialTrendsDto
{
    public List<TrendingTopicDto> TrendingTopics { get; set; } = new();
    public List<TrendingHashtagDto> TrendingHashtags { get; set; } = new();
    public List<ViralContentDto> ViralContent { get; set; } = new();
    public DateTimeOffset UpdatedAt { get; set; }
}

public class TrendingTopicDto
{
    public string Topic { get; set; } = string.Empty;
    public int Mentions { get; set; }
    public double Growth { get; set; }
    public string Sentiment { get; set; } = string.Empty;
}

public class TrendingHashtagDto
{
    public string Hashtag { get; set; } = string.Empty;
    public int Posts { get; set; }
    public int Reach { get; set; }
    public double Growth { get; set; }
}

public class ViralContentDto
{
    public string ContentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Shares { get; set; }
    public int Engagement { get; set; }
    public double ViralityScore { get; set; }
}

public class SchedulePostRequest
{
    [Required]
    public List<string> Platforms { get; set; } = new();
    
    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    public DateTimeOffset ScheduledAt { get; set; }
    
    public string? VideoUrl { get; set; }
    
    public List<string> MediaUrls { get; set; } = new();
    
    public List<string> Hashtags { get; set; } = new();
    
    public bool RecurringPost { get; set; } = false;
    
    public string? RecurrencePattern { get; set; } // daily, weekly, monthly
    
    public DateTimeOffset? RecurrenceEnd { get; set; }
}

public class UpdateScheduleRequest
{
    public DateTimeOffset? ScheduledAt { get; set; }
    
    public string? Content { get; set; }
    
    public List<string>? Hashtags { get; set; }
    
    public bool? RecurringPost { get; set; }
    
    public string? RecurrencePattern { get; set; }
    
    public DateTimeOffset? RecurrenceEnd { get; set; }
}

public class ScheduledPostDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<string> Platforms { get; set; } = new();
    public string Content { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public bool RecurringPost { get; set; }
    public string? RecurrencePattern { get; set; }
    public DateTimeOffset? RecurrenceEnd { get; set; }
    public ScheduledPostStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SocialCommentDto
{
    public string Id { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string PostId { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset PostedAt { get; set; }
    public int Likes { get; set; }
    public List<SocialCommentDto> Replies { get; set; } = new();
    public CommentStatus Status { get; set; }
}

public class ReplyToCommentRequest
{
    [Required]
    public string CommentId { get; set; } = string.Empty;
    
    [Required]
    public string Platform { get; set; } = string.Empty;
    
    [Required, MaxLength(1000)]
    public string Reply { get; set; } = string.Empty;
}

public class ModerateCommentRequest
{
    [Required]
    public string CommentId { get; set; } = string.Empty;
    
    [Required]
    public string Platform { get; set; } = string.Empty;
    
    [Required]
    public CommentAction Action { get; set; }
    
    public string? Reason { get; set; }
}

public class HashtagAnalyticsDto
{
    public string Hashtag { get; set; } = string.Empty;
    public int Uses { get; set; }
    public int Reach { get; set; }
    public int Engagement { get; set; }
    public double EngagementRate { get; set; }
    public List<HashtagMetricDto> DailyMetrics { get; set; } = new();
}

public class HashtagMetricDto
{
    public DateOnly Date { get; set; }
    public int Uses { get; set; }
    public int Reach { get; set; }
}

public class SocialMentionDto
{
    public string Id { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public string MentionUrl { get; set; } = string.Empty;
    public DateTimeOffset MentionedAt { get; set; }
    public MentionType Type { get; set; }
    public MentionStatus Status { get; set; }
    public double SentimentScore { get; set; }
}

public class RespondToMentionRequest
{
    [Required]
    public string MentionId { get; set; } = string.Empty;
    
    [Required]
    public string Platform { get; set; } = string.Empty;
    
    [Required, MaxLength(500)]
    public string Response { get; set; } = string.Empty;
}

public class SocialSentimentDto
{
    public double OverallScore { get; set; } // -1 to 1 (negative to positive)
    public SentimentBreakdown Breakdown { get; set; } = new();
    public List<SentimentTimelineDto> Timeline { get; set; } = new();
    public List<KeyPhraseDto> KeyPhrases { get; set; } = new();
}

public class SentimentBreakdown
{
    public double Positive { get; set; }
    public double Negative { get; set; }
    public double Neutral { get; set; }
}

public class SentimentTimelineDto
{
    public DateTimeOffset Date { get; set; }
    public double Score { get; set; }
    public string Sentiment { get; set; } = string.Empty;
}

public class KeyPhraseDto
{
    public string Phrase { get; set; } = string.Empty;
    public int Mentions { get; set; }
    public double Sentiment { get; set; }
}

// Enums
public enum PostStatus
{
    Draft,
    Scheduled,
    Posted,
    Failed,
    Deleted
}

public enum ShareStatus
{
    Pending,
    Shared,
    Failed,
    Removed
}

public enum ImportStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Partial
}

public enum ScheduledPostStatus
{
    Pending,
    Posted,
    Failed,
    Cancelled,
    Paused
}

public enum CommentStatus
{
    Active,
    Hidden,
    Deleted,
    Reported
}

public enum CommentAction
{
    Hide,
    Delete,
    Report,
    Approve
}

public enum MentionType
{
    Direct,
    Retweet,
    Quote,
    Reply
}

public enum MentionStatus
{
    New,
    Read,
    Responded,
    Archived
}
