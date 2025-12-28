using System.ComponentModel.DataAnnotations;

namespace StreamVault.Application.AI.DTOs;

public class RecommendationRequest
{
    public int Count { get; set; } = 10;
    public List<string> ExcludedCategories { get; set; } = new();
    public List<string> PreferredCategories { get; set; } = new();
    public List<string> ExcludedTags { get; set; } = new();
    public List<string> PreferredTags { get; set; } = new();
    public int? MinDuration { get; set; }
    public int? MaxDuration { get; set; }
    public DateTime? PublishedAfter { get; set; }
    public DateTime? PublishedBefore { get; set; }
    public int? MinViews { get; set; }
    public int? MaxViews { get; set; }
    public double? MinRating { get; set; }
    public RecommendationAlgorithm Algorithm { get; set; } = RecommendationAlgorithm.Hybrid;
    public bool IncludeWatched { get; set; } = false;
    public bool IncludeLiked { get; set; } = true;
    public bool IncludeDisliked { get; set; } = false;
    public string? Context { get; set; } // home, search, video_end, etc.
    public Dictionary<string, object>? CustomParameters { get; set; }
}

public class RecommendedVideoDto
{
    public Guid VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string ChannelAvatar { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public int ViewCount { get; set; }
    public DateTime PublishedAt { get; set; }
    public double Rating { get; set; }
    public int LikeCount { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public double RecommendationScore { get; set; }
    public List<string> RecommendationReasons { get; set; } = new();
    public RecommendationSource Source { get; set; }
    public bool IsWatched { get; set; }
    public bool IsLiked { get; set; }
    public bool IsDisliked { get; set; }
    public TimeSpan? WatchProgress { get; set; }
    public bool IsNew { get; set; }
    public bool IsTrending { get; set; }
    public bool IsLive { get; set; }
    public bool IsPremium { get; set; }
    public string? Language { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class TrendingRequest
{
    public TrendingPeriod Period { get; set; } = TrendingPeriod.Today;
    public string? Category { get; set; }
    public string? Country { get; set; }
    public string? Language { get; set; }
    public int Count { get; set; } = 10;
    public bool IncludeWatched { get; set; } = false;
    public TrendingAlgorithm Algorithm { get; set; } = TrendingAlgorithm.Engagement;
}

public class SimilarityRequest
{
    public int Count { get; set; } = 10;
    public SimilarityType Type { get; set; } = SimilarityType.Content;
    public double MinSimilarity { get; set; } = 0.5;
    public bool IncludeSameChannel { get; set; } = false;
    public bool IncludeWatched { get; set; } = false;
}

public class TimeBasedRequest
{
    public TimeOfDay TimeOfDay { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan? Duration { get; set; }
    public int Count { get; set; } = 10;
}

public class LocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Radius { get; set; } // in kilometers
    public string? Country { get; set; }
    public string? City { get; set; }
    public int Count { get; set; } = 10;
}

public class DeviceRequest
{
    public DeviceType DeviceType { get; set; }
    public string? Os { get; set; }
    public string? Browser { get; set; }
    public bool IsMobile { get; set; }
    public bool IsTablet { get; set; }
    public int Count { get; set; } = 10;
}

public class MoodRequest
{
    public MoodType Mood { get; set; }
    public List<string> Activities { get; set; } = new();
    public TimeSpan? AvailableTime { get; set; }
    public int Count { get; set; } = 10;
}

public class HybridRequest
{
    public double CollaborativeWeight { get; set; } = 0.4;
    public double ContentWeight { get; set; } = 0.3;
    public double PopularityWeight { get; set; } = 0.2;
    public double DiversityWeight { get; set; } = 0.1;
    public int Count { get; set; } = 10;
}

public class RealTimeRequest
{
    public List<string> RecentVideos { get; set; } = new();
    public List<string> RecentSearches { get; set; } = new();
    public TimeSpan SessionDuration { get; set; }
    public int ClickCount { get; set; }
    public int Count { get; set; } = 10;
}

public class UserInteractionDto
{
    public Guid VideoId { get; set; }
    public InteractionType Type { get; set; }
    public TimeSpan? WatchDuration { get; set; }
    public double? CompletionRate { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Context { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}

public class NewUserRequest
{
    public List<string> SelectedCategories { get; set; } = new();
    public List<string> SelectedChannels { get; set; } = new();
    public string? ReferralSource { get; set; }
    public string? Campaign { get; set; }
    public int Count { get; set; } = 10;
}

public class NewVideoRequest
{
    public List<string> Categories { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public string? ChannelId { get; set; }
    public int Count { get; set; } = 10;
}

public class AnalyticsRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string> Metrics { get; set; } = new();
    public string? Granularity { get; set; } // hourly, daily, weekly, monthly
}

public class RecommendationAnalyticsDto
{
    public Guid UserId { get; set; }
    public int TotalRecommendations { get; set; }
    public int ClickedRecommendations { get; set; }
    public int WatchedRecommendations { get; set; }
    public int LikedRecommendations { get; set; }
    public double ClickThroughRate { get; set; }
    public double WatchRate { get; set; }
    public double LikeRate { get; set; }
    public double AverageWatchTime { get; set; }
    public List<RecommendationPerformanceDto> PerformanceByAlgorithm { get; set; } = new();
    public List<RecommendationTimelineDto> Timeline { get; set; } = new();
}

public class RecommendationPerformanceDto
{
    public RecommendationAlgorithm Algorithm { get; set; }
    public int Impressions { get; set; }
    public int Clicks { get; set; }
    public int Watches { get; set; }
    public double ClickThroughRate { get; set; }
    public double WatchRate { get; set; }
    public double AverageScore { get; set; }
}

public class RecommendationTimelineDto
{
    public DateTime Date { get; set; }
    public int Recommendations { get; set; }
    public int Clicks { get; set; }
    public int Watches { get; set; }
    public double CTR { get; set; }
}

public class MetricsRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<RecommendationAlgorithm> Algorithms { get; set; } = new();
    public List<string> Metrics { get; set; } = new();
}

public class RecommendationMetricsDto
{
    public RecommendationAlgorithm Algorithm { get; set; }
    public int TotalUsers { get; set; }
    public int TotalRecommendations { get; set; }
    public double AverageCTR { get; set; }
    public double AverageWatchRate { get; set; }
    public double AverageEngagement { get; set; }
    public double Coverage { get; set; }
    public double Diversity { get; set; }
    public double Novelty { get; set; }
    public double Serendipity { get; set; }
}

public class ABTestResultDto
{
    public Guid TestId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public List<ABTestVariantResultDto> Variants { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public ABTestStatus Status { get; set; }
    public string? WinningVariant { get; set; }
    public double Confidence { get; set; }
}

public class ABTestVariantResultDto
{
    public string Variant { get; set; } = string.Empty;
    public int Users { get; set; }
    public int Impressions { get; set; }
    public int Clicks { get; set; }
    public int Conversions { get; set; }
    public double CTR { get; set; }
    public double ConversionRate { get; set; }
    public double StatisticalSignificance { get; set; }
}

public class FeedbackRequest
{
    [Required]
    public Guid VideoId { get; set; }
    
    [Required]
    public FeedbackType Type { get; set; }
    
    public int? Rating { get; set; }
    
    [MaxLength(500)]
    public string? Comment { get; set; }
    
    public List<string>? Reasons { get; set; }
    
    public string? Context { get; set; }
    
    public bool IsHelpful { get; set; }
}

public class FeedbackDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public FeedbackType Type { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public List<string> Reasons { get; set; } = new();
    public string? Context { get; set; }
    public bool IsHelpful { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ModelUpdateRequest
{
    public List<Guid> VideoIds { get; set; } = new();
    public UpdateType Type { get; set; }
    public bool ForceUpdate { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

public class CreateABTestRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public List<ABTestVariantDto> Variants { get; set; } = new();
    
    public double TrafficSplit { get; set; } = 1.0;
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public int MinSampleSize { get; set; } = 1000;
    
    public double ConfidenceLevel { get; set; } = 0.95;
    
    public string? TargetMetric { get; set; }
}

public class ABTestVariantDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public Dictionary<string, object> Configuration { get; set; } = new();
    
    public double TrafficWeight { get; set; } = 0.5;
}

public class ABTestVariantDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsControl { get; set; }
    public double TrafficWeight { get; set; }
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class ExplanationDto
{
    public string Type { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> Factors { get; set; } = new();
    public Dictionary<string, object>? Details { get; set; }
}

public class ExplanationModelRequest
{
    public List<ExplanationType> Types { get; set; } = new();
    public double ConfidenceThreshold { get; set; } = 0.7;
    public int MaxFactors { get; set; } = 5;
}

public class DiversityRequest
{
    public double DiversityWeight { get; set; } = 0.3;
    public List<string> DiversityDimensions { get; set; } = new();
    public int MinCategories { get; set; } = 3;
    public int MinChannels { get; set; } = 2;
    public int Count { get; set; } = 10;
}

public class SerendipityRequest
{
    public double SerendipityWeight { get; set; } = 0.2;
    public double NoveltyThreshold { get; set; } = 0.5;
    public int MaxSimilarity { get; set; } = 3;
    public int Count { get; set; } = 10;
}

public class BanditRequest
{
    public BanditAlgorithm Algorithm { get; set; } = BanditAlgorithm.UCB1;
    public double ExplorationRate { get; set; } = 0.1;
    public int Count { get; set; } = 10;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class BanditUpdateRequest
{
    public Guid ArmId { get; set; }
    public double Reward { get; set; }
    public bool UpdateModel { get; set; } = true;
}

public class DeepLearningRequest
{
    public string ModelName { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public int Count { get; set; } = 10;
    public double Threshold { get; set; } = 0.5;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class ModelTrainingRequest
{
    [Required]
    public string ModelName { get; set; } = string.Empty;
    
    public List<string> Features { get; set; } = new();
    
    public int Epochs { get; set; } = 100;
    
    public double LearningRate { get; set; } = 0.001;
    
    public double ValidationSplit { get; set; } = 0.2;
    
    public Dictionary<string, object>? Hyperparameters { get; set; }
}

public class ModelPerformanceDto
{
    public Guid ModelId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public double AUC { get; set; }
    public DateTime TrainedAt { get; set; }
    public int TrainingSamples { get; set; }
    public int ValidationSamples { get; set; }
}

public class FilterRequest
{
    public List<string> AllowedCategories { get; set; } = new();
    public List<string> BlockedCategories { get; set; } = new();
    public List<string> AllowedTags { get; set; } = new();
    public List<string> BlockedTags { get; set; } = new();
    public List<Guid> AllowedChannels { get; set; } = new();
    public List<Guid> BlockedChannels { get; set; } = new();
    public int? MinDuration { get; set; }
    public int? MaxDuration { get; set; }
    public int? MinRating { get; set; }
    public bool ExcludeWatched { get; set; } = true;
    public bool ExcludeDisliked { get; set; } = true;
    public int Count { get; set; } = 10;
}

public class FilterPreferencesDto
{
    public List<string> PreferredCategories { get; set; } = new();
    public List<string> BlockedCategories { get; set; } = new();
    public List<string> PreferredTags { get; set; } = new();
    public List<string> BlockedTags { get; set; } = new();
    public List<Guid> PreferredChannels { get; set; } = new();
    public List<Guid> BlockedChannels { get; set; } = new();
    public int? PreferredMinDuration { get; set; }
    public int? PreferredMaxDuration { get; set; }
    public double? PreferredMinRating { get; set; }
    public bool ExcludeWatched { get; set; } = true;
    public bool ExcludeDisliked { get; set; } = true;
}

public class CacheRequest
{
    public string CacheKey { get; set; } = string.Empty;
    public List<RecommendedVideoDto> Recommendations { get; set; } = new();
    public TimeSpan? Expiry { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class CrossPlatformRequest
{
    public List<string> Platforms { get; set; } = new();
    public bool SyncWatchHistory { get; set; } = true;
    public bool SyncPreferences { get; set; } = true;
    public int Count { get; set; } = 10;
}

public class SyncRequest
{
    public List<string> Platforms { get; set; } = new();
    public bool FullSync { get; set; } = false;
    public DateTime? LastSync { get; set; }
}

public class QualityRequest
{
    public List<QualityMetric> Metrics { get; set; } = new();
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class RecommendationQualityDto
{
    public double Accuracy { get; set; }
    public double Diversity { get; set; }
    public double Novelty { get; set; }
    public double Serendipity { get; set; }
    public double Coverage { get; set; }
    public double Freshness { get; set; }
    public double Relevance { get; set; }
    public double Engagement { get; set; }
    public List<QualityMetricDto> MetricBreakdown { get; set; } = new();
}

public class QualityImprovementRequest
{
    public List<QualityMetric> TargetMetrics { get; set; } = new();
    public double TargetImprovement { get; set; }
    public int RetrainingDays { get; set; } = 7;
}

public class PersonalizationMetricsDto
{
    public Guid UserId { get; set; }
    public double PersonalizationScore { get; set; }
    public double PreferenceAlignment { get; set; }
    public double BehaviorPrediction { get; set; }
    public double SatisfactionScore { get; set; }
    public List<PersonalizationMetricDto> Metrics { get; set; } = new();
}

public class PersonalizationUpdateRequest
{
    public List<string> UpdatedPreferences { get; set; } = new();
    public List<UserInteractionDto> RecentInteractions { get; set; } = new();
    public bool ForceRetraining { get; set; }
}

// Helper DTOs
public class QualityMetricDto
{
    public QualityMetric Metric { get; set; }
    public double Value { get; set; }
    public double Target { get; set; }
    public double Improvement { get; set; }
}

public class PersonalizationMetricDto
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public double Weight { get; set; }
}

// Enums
public enum RecommendationAlgorithm
{
    Collaborative,
    ContentBased,
    Hybrid,
    DeepLearning,
    Bandit,
    Popular,
    Trending,
    Similar,
    Personalized,
    Contextual
}

public enum RecommendationSource
{
    CollaborativeFiltering,
    ContentBased,
    Popular,
    Trending,
    Similar,
    UserHistory,
    Contextual,
    RealTime,
    Bandit,
    DeepLearning
}

public enum TrendingPeriod
{
    Hour,
    Today,
    Week,
    Month,
    Year,
    AllTime
}

public enum TrendingAlgorithm
{
    Views,
    Engagement,
    Growth,
    Velocity,
    Acceleration
}

public enum SimilarityType
{
    Content,
    Tags,
    Categories,
    Channel,
    Description,
    Transcript,
    Visual,
    Audio
}

public enum TimeOfDay
{
    Morning,
    Afternoon,
    Evening,
    Night,
    LateNight
}

public enum DeviceType
{
    Desktop,
    Mobile,
    Tablet,
    TV,
    Console,
    SmartWatch
}

public enum MoodType
{
    Happy,
    Sad,
    Energetic,
    Relaxed,
    Focused,
    Creative,
    Educational,
    Entertaining
}

public enum InteractionType
{
    View,
    Like,
    Dislike,
    Comment,
    Share,
    Save,
    Download,
    Complete,
    Skip,
    Pause,
    Seek
}

public enum FeedbackType
{
    Positive,
    Negative,
    Neutral,
    NotRelevant,
    Inappropriate,
    Duplicate,
    QualityIssue
}

public enum UpdateType
{
    Incremental,
    Full,
    RealTime,
    Scheduled
}

public enum ABTestStatus
{
    Draft,
    Running,
    Paused,
    Completed,
    Analyzed
}

public enum ExplanationType
{
    ContentSimilarity,
    UserBehavior,
    Popular,
    Trending,
    Collaborative,
    Contextual,
    Personalized
}

public enum BanditAlgorithm
{
    EpsilonGreedy,
    UCB1,
    ThompsonSampling,
    LinUCB,
    EXP3
}

public enum QualityMetric
{
    Accuracy,
    Precision,
    Recall,
    F1Score,
    Diversity,
    Novelty,
    Serendipity,
    Coverage,
    Freshness,
    Relevance,
    Engagement,
    Satisfaction
}
