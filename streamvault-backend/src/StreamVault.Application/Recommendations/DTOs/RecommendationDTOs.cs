using System.ComponentModel.DataAnnotations;

namespace StreamVault.Application.Recommendations.DTOs;

public class RecommendedVideoDto
{
    public Guid VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public int ViewCount { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public UserDto Creator { get; set; } = null!;
    public double Score { get; set; } // Recommendation score
    public string Reason { get; set; } = string.Empty; // Why this was recommended
    public List<string> RecommendationReasons { get; set; } = new();
    public bool IsWatched { get; set; }
    public int WatchProgress { get; set; } // Percentage watched
    public bool IsLiked { get; set; }
    public bool IsDisliked { get; set; }
    public bool IsInWatchLater { get; set; }
    public DateTimeOffset? LastWatchedAt { get; set; }
    public int WatchTimeSeconds { get; set; }
    public double EngagementScore { get; set; }
    public double TrendingScore { get; set; }
    public double PersonalizationScore { get; set; }
}

public class UserWatchHistoryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid VideoId { get; set; }
    public int WatchedSeconds { get; set; }
    public int TotalSeconds { get; set; }
    public double WatchPercentage { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? LastWatchedAt { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsLiked { get; set; }
    public bool IsDisliked { get; set; }
    public bool IsShared { get; set; }
    public bool IsAddedToWatchLater { get; set; }
    public int WatchSessions { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class UserInteractionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid VideoId { get; set; }
    public string InteractionType { get; set; } = string.Empty; // like, dislike, share, comment, subscribe, etc.
    public DateTimeOffset Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public double Weight { get; set; } // Interaction weight for recommendations
}

public class RecommendationProfileDto
{
    public Guid UserId { get; set; }
    public Dictionary<string, double> CategoryPreferences { get; set; } = new();
    public Dictionary<string, double> TagPreferences { get; set; } = new();
    public List<Guid> PreferredCreators { get; set; } = new();
    public List<Guid> BlockedCreators { get; set; } = new();
    public List<Guid> BlockedVideos { get; set; } = new();
    public double AverageWatchTime { get; set; }
    public double AverageSessionLength { get; set; }
    public List<string> PreferredVideoLengths { get; set; } = new();
    public List<string> PreferredUploadTimes { get; set; } = new();
    public double EngagementRate { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}

public class RecommendationAnalyticsDto
{
    public Guid VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TotalRecommendations { get; set; }
    public int ClickThroughRate { get; set; }
    public int ViewsFromRecommendations { get; set; }
    public double ConversionRate { get; set; }
    public double AverageWatchTime { get; set; }
    public Dictionary<string, int> RecommendationSources { get; set; } = new();
    public List<string> TopReasons { get; set; } = new();
    public DateOnly Date { get; set; }
}

public class TrendingVideoDto
{
    public Guid VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ShareCount { get; set; }
    public double TrendingScore { get; set; }
    public double GrowthRate { get; set; } // Views growth rate
    public int Rank { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public UserDto Creator { get; set; } = null!;
    public DateTimeOffset PublishedAt { get; set; }
    public TimeSpan TimeSincePublished { get; set; }
}

public class RecommendationFeedbackDto
{
    public Guid UserId { get; set; }
    public Guid VideoId { get; set; }
    public bool IsRelevant { get; set; }
    public string? Reason { get; set; }
    public List<string> SelectedReasons { get; set; } = new();
    public string FeedbackType { get; set; } = string.Empty; // not_interested, recommend_more, etc.
    public DateTimeOffset Timestamp { get; set; }
}

public class PersonalizedInsightDto
{
    public string InsightType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Guid> RelatedVideos { get; set; } = new();
    public List<Guid> RelatedCreators { get; set; } = new();
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTimeOffset GeneratedAt { get; set; }
}

public class RecommendationSettingsDto
{
    public bool EnablePersonalization { get; set; } = true;
    public bool EnableTrending { get; set; } = true;
    public bool EnableSimilarContent { get; set; } = true;
    public bool EnableCreatorRecommendations { get; set; } = true;
    public bool EnableWatchHistory { get; set; } = true;
    public List<string> BlockedCategories { get; set; } = new();
    public List<string> BlockedTags { get; set; } = new();
    public List<Guid> BlockedCreators { get; set; } = new();
    public int MaxRecommendationsPerSession { get; set; } = 50;
    public double DiversityThreshold { get; set; } = 0.3;
    public string RecommendationAlgorithm { get; set; } = "hybrid";
}

public class RecommendationRequestDto
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string Algorithm { get; set; } = "hybrid";
    public int Limit { get; set; } = 20;
    public List<string> Filters { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
    public string? SessionId { get; set; }
    public string? Source { get; set; } // homepage, watch_page, search, etc.
}

public class RecommendationResponseDto
{
    public List<RecommendedVideoDto> Videos { get; set; } = new();
    public string Algorithm { get; set; } = string.Empty;
    public int TotalResults { get; set; }
    public bool HasMore { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTimeOffset GeneratedAt { get; set; }
}
