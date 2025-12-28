using System.ComponentModel.DataAnnotations;
using StreamVault.Application.Auth.DTOs;

namespace StreamVault.Application.Comments.DTOs;

public class CreateCommentRequest
{
    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
    
    public Guid? ParentCommentId { get; set; }
    
    public TimeSpan? Timestamp { get; set; } // For timestamped comments
    
    public List<string> Mentions { get; set; } = new();
    
    public List<string> Hashtags { get; set; } = new();
    
    public bool IsSpoiler { get; set; } = false;
    
    public string? Language { get; set; }
}

public class UpdateCommentRequest
{
    [MaxLength(2000)]
    public string? Content { get; set; }
    
    public bool? IsSpoiler { get; set; }
    
    public string? Language { get; set; }
}

public class VideoCommentDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public TimeSpan? Timestamp { get; set; }
    public List<string> Mentions { get; set; } = new();
    public List<string> Hashtags { get; set; } = new();
    public bool IsSpoiler { get; set; }
    public bool IsEdited { get; set; }
    public bool IsPinned { get; set; }
    public bool IsHighlighted { get; set; }
    public string? HighlightReason { get; set; }
    public string Language { get; set; } = string.Empty;
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    public int ReplyCount { get; set; }
    public UserReaction? UserReaction { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public UserDto User { get; set; } = null!;
    public List<VideoCommentDto> Replies { get; set; } = new();
    public CommentStatus Status { get; set; }
}

public class CommentReactionDto
{
    public Guid CommentId { get; set; }
    public Dictionary<string, int> ReactionCounts { get; set; } = new();
    public UserReaction? UserReaction { get; set; }
}

public class ReportCommentRequest
{
    [Required]
    public string Reason { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public string Category { get; set; } = string.Empty;
}

public class ReportedCommentDto
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset ReportedAt { get; set; }
    public ReportStatus Status { get; set; }
    public int ReportCount { get; set; }
}

public class SearchCommentsRequest
{
    public string? Query { get; set; }
    
    public List<string> Authors { get; set; } = new();
    
    public List<string> Hashtags { get; set; } = new();
    
    public DateTimeOffset? StartDate { get; set; }
    
    public DateTimeOffset? EndDate { get; set; }
    
    public bool IncludeReplies { get; set; } = true;
    
    public CommentSortOption SortBy { get; set; } = CommentSortOption.Newest;
    
    public bool? HasReplies { get; set; }
    
    public int? MinLikes { get; set; }
}

public class CommentAnalyticsDto
{
    public Guid VideoId { get; set; }
    public int TotalComments { get; set; }
    public int TopLevelComments { get; set; }
    public int ReplyComments { get; set; }
    public int UniqueCommenters { get; set; }
    public double AverageCommentsPerUser { get; set; }
    public List<CommentCountTimelineDto> CommentTimeline { get; set; } = new();
    public List<TopCommenterDto> TopCommenters { get; set; } = new();
    public List<PopularCommentDto> PopularComments { get; set; } = new();
    public Dictionary<string, int> HashtagCounts { get; set; } = new();
    public Dictionary<string, int> MentionCounts { get; set; } = new();
}

public class CommentCountTimelineDto
{
    public DateOnly Date { get; set; }
    public int CommentCount { get; set; }
}

public class TopCommenterDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int CommentCount { get; set; }
    public int TotalLikes { get; set; }
    public double EngagementRate { get; set; }
}

public class PopularCommentDto
{
    public Guid CommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Likes { get; set; }
    public int Replies { get; set; }
    public double EngagementScore { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class CommentEngagementDto
{
    public Guid VideoId { get; set; }
    public int TotalLikes { get; set; }
    public int TotalDislikes { get; set; }
    public double LikeRatio { get; set; }
    public List<EngagementHourDto> HourlyEngagement { get; set; } = new();
    public List<EngagementLocationDto> EngagementByLocation { get; set; } = new();
    public double AverageEngagementTime { get; set; } // in seconds
}

public class EngagementHourDto
{
    public int Hour { get; set; }
    public int Likes { get; set; }
    public int Dislikes { get; set; }
}

public class EngagementLocationDto
{
    public string Location { get; set; } = string.Empty;
    public int EngagementCount { get; set; }
}

public class CommentSentimentDto
{
    public Guid CommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public double SentimentScore { get; set; } // -1 to 1
    public SentimentLabel Sentiment { get; set; }
    public double Confidence { get; set; }
    public List<string> KeyPhrases { get; set; } = new();
    public List<string> Emotions { get; set; } = new();
}

public class CommentSettingsDto
{
    public Guid VideoId { get; set; }
    public bool AllowComments { get; set; } = true;
    public bool AllowReplies { get; set; } = true;
    public bool AllowMentions { get; set; } = true;
    public bool AllowHashtags { get; set; } = true;
    public bool ModerateComments { get; set; } = false;
    public bool AutoModerate { get; set; } = false;
    public List<string> BlockedWords { get; set; } = new();
    public List<string> BlockedUsers { get; set; } = new();
    public int MaxCommentLength { get; set; } = 2000;
    public int MaxReplyDepth { get; set; } = 10;
    public bool RequireVerification { get; set; } = false;
    public bool EnableTimestamps { get; set; } = true;
    public CommentSortOption DefaultSort { get; set; } = CommentSortOption.Newest;
}

public class UpdateCommentSettingsRequest
{
    public bool? AllowComments { get; set; }
    public bool? AllowReplies { get; set; }
    public bool? AllowMentions { get; set; }
    public bool? AllowHashtags { get; set; }
    public bool? ModerateComments { get; set; }
    public bool? AutoModerate { get; set; }
    public List<string>? BlockedWords { get; set; }
    public List<string>? BlockedUsers { get; set; }
    public int? MaxCommentLength { get; set; }
    public int? MaxReplyDepth { get; set; }
    public bool? RequireVerification { get; set; }
    public bool? EnableTimestamps { get; set; }
    public CommentSortOption? DefaultSort { get; set; }
}

public class CommentThreadDto
{
    public Guid RootCommentId { get; set; }
    public VideoCommentDto RootComment { get; set; } = null!;
    public List<VideoCommentDto> AllReplies { get; set; } = new();
    public int TotalReplies { get; set; }
    public int MaxDepth { get; set; }
    public bool IsCollapsed { get; set; }
    public ThreadViewMode ViewMode { get; set; }
}

public class CommentNotificationDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public string VideoTitle { get; set; } = string.Empty;
    public Guid CommentId { get; set; }
    public string CommentContent { get; set; } = string.Empty;
    public Guid TriggerUserId { get; set; }
    public string TriggerUserName { get; set; } = string.Empty;
    public string TriggerUserAvatar { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? ActionUrl { get; set; }
}

public class ImportCommentsRequest
{
    [Required]
    public string Format { get; set; } = string.Empty; // json, csv, youtube
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public bool ReplaceExisting { get; set; } = false;
    
    public bool PreserveTimestamps { get; set; } = true;
    
    public bool ImportAuthors { get; set; } = true;
    
    public string? DefaultAuthor { get; set; }
}

public class CommentExportDto
{
    public Guid VideoId { get; set; }
    public string VideoTitle { get; set; } = string.Empty;
    public List<ExportedCommentDto> Comments { get; set; } = new();
    public int TotalComments { get; set; }
    public DateTimeOffset ExportedAt { get; set; }
    public string Format { get; set; } = string.Empty;
}

public class ExportedCommentDto
{
    public Guid Id { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public TimeSpan? Timestamp { get; set; }
    public int Likes { get; set; }
    public int Replies { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? Language { get; set; }
}

// Enums
public enum CommentSortOption
{
    Newest,
    Oldest,
    Top,
    NewestFirst,
    Controversial
}

public enum UserReaction
{
    None,
    Like,
    Dislike
}

public enum CommentStatus
{
    Active,
    Hidden,
    Deleted,
    Reported,
    Pending
}

public enum ReportStatus
{
    Pending,
    Reviewed,
    Resolved,
    Dismissed
}

public enum SentimentLabel
{
    Positive,
    Negative,
    Neutral
}

public enum NotificationType
{
    Reply,
    Mention,
    Like,
    Pin,
    Highlight
}

public enum ThreadViewMode
{
    Linear,
    Threaded,
    Nested
}

public enum CommentExportFormat
{
    Json,
    Csv,
    Xml,
    YouTube
}
