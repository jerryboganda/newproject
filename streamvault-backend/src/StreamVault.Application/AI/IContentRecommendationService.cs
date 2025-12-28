using StreamVault.Application.AI.DTOs;

namespace StreamVault.Application.AI;

public interface IContentRecommendationService
{
    // Personalized Recommendations
    Task<List<RecommendedVideoDto>> GetPersonalizedRecommendationsAsync(Guid userId, Guid tenantId, RecommendationRequest request);
    Task<List<RecommendedVideoDto>> GetTrendingRecommendationsAsync(Guid userId, Guid tenantId, TrendingRequest request);
    Task<List<RecommendedVideoDto>> GetSimilarVideosAsync(Guid videoId, Guid userId, Guid tenantId, SimilarityRequest request);
    Task<List<RecommendedVideoDto>> GetContinueWatchingAsync(Guid userId, Guid tenantId, int count = 10);
    Task<List<RecommendedVideoDto>> GetBecauseYouWatchedAsync(Guid videoId, Guid userId, Guid tenantId, int count = 10);
    
    // Contextual Recommendations
    Task<List<RecommendedVideoDto>> GetTimeBasedRecommendationsAsync(Guid userId, Guid tenantId, TimeBasedRequest request);
    Task<List<RecommendedVideoDto>> GetLocationBasedRecommendationsAsync(Guid userId, Guid tenantId, LocationRequest request);
    Task<List<RecommendedVideoDto>> GetDeviceBasedRecommendationsAsync(Guid userId, Guid tenantId, DeviceRequest request);
    Task<List<RecommendedVideoDto>> GetMoodBasedRecommendationsAsync(Guid userId, Guid tenantId, MoodRequest request);
    
    // Collaborative Filtering
    Task<List<RecommendedVideoDto>> GetUserBasedRecommendationsAsync(Guid userId, Guid tenantId, int count = 10);
    Task<List<RecommendedVideoDto>> GetItemBasedRecommendationsAsync(Guid videoId, Guid userId, Guid tenantId, int count = 10);
    Task<List<RecommendedVideoDto>> GetHybridRecommendationsAsync(Guid userId, Guid tenantId, HybridRequest request);
    
    // Content-Based Filtering
    Task<List<RecommendedVideoDto>> GetGenreBasedRecommendationsAsync(Guid userId, Guid tenantId, List<string> genres, int count = 10);
    Task<List<RecommendedVideoDto>> GetTagBasedRecommendationsAsync(Guid userId, Guid tenantId, List<string> tags, int count = 10);
    Task<List<RecommendedVideoDto>> GetTopicBasedRecommendationsAsync(Guid userId, Guid tenantId, string topic, int count = 10);
    
    // Real-time Recommendations
    Task<List<RecommendedVideoDto>> GetRealTimeRecommendationsAsync(Guid userId, Guid tenantId, RealTimeRequest request);
    Task<bool> UpdateRealTimeModelAsync(Guid userId, Guid tenantId, UserInteractionDto interaction);
    Task<List<RecommendedVideoDto>> GetSessionBasedRecommendationsAsync(Guid sessionId, Guid userId, Guid tenantId);
    
    // Cold Start Problem
    Task<List<RecommendedVideoDto>> GetNewUserRecommendationsAsync(Guid userId, Guid tenantId, NewUserRequest request);
    Task<List<RecommendedVideoDto>> GetNewVideoRecommendationsAsync(Guid videoId, Guid tenantId, NewVideoRequest request);
    
    // Recommendation Analytics
    Task<RecommendationAnalyticsDto> GetRecommendationAnalyticsAsync(Guid userId, Guid tenantId, AnalyticsRequest request);
    Task<List<RecommendationMetricsDto>> GetRecommendationMetricsAsync(Guid tenantId, MetricsRequest request);
    Task<ABTestResultDto> GetABTestResultsAsync(Guid testId, Guid tenantId);
    
    // Feedback Loop
    Task<bool> RecordFeedbackAsync(Guid userId, Guid tenantId, FeedbackRequest request);
    Task<bool> UpdateRecommendationModelAsync(Guid userId, Guid tenantId, ModelUpdateRequest request);
    Task<List<FeedbackDto>> GetFeedbackHistoryAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20);
    
    // A/B Testing
    Task<Guid> CreateABTestAsync(Guid tenantId, CreateABTestRequest request);
    Task<bool> AssignUserToTestAsync(Guid userId, Guid testId, Guid tenantId);
    Task<ABTestVariantDto> GetUserTestVariantAsync(Guid userId, Guid testId, Guid tenantId);
    
    // Recommendation Explanations
    Task<List<ExplanationDto>> GetRecommendationExplanationsAsync(Guid userId, Guid videoId, Guid tenantId);
    Task<bool> UpdateExplanationModelAsync(Guid tenantId, ExplanationModelRequest request);
    
    // Diversity and Serendipity
    Task<List<RecommendedVideoDto>> GetDiverseRecommendationsAsync(Guid userId, Guid tenantId, DiversityRequest request);
    Task<List<RecommendedVideoDto>> GetSerendipitousRecommendationsAsync(Guid userId, Guid tenantId, SerendipityRequest request);
    
    // Multi-Arm Bandit
    Task<List<RecommendedVideoDto>> GetBanditRecommendationsAsync(Guid userId, Guid tenantId, BanditRequest request);
    Task<bool> UpdateBanditModelAsync(Guid userId, Guid tenantId, BanditUpdateRequest request);
    
    // Deep Learning Models
    Task<List<RecommendedVideoDto>> GetDeepLearningRecommendationsAsync(Guid userId, Guid tenantId, DeepLearningRequest request);
    Task<bool> TrainDeepLearningModelAsync(Guid tenantId, ModelTrainingRequest request);
    Task<ModelPerformanceDto> GetModelPerformanceAsync(Guid modelId, Guid tenantId);
    
    // Recommendation Filters
    Task<List<RecommendedVideoDto>> GetFilteredRecommendationsAsync(Guid userId, Guid tenantId, FilterRequest request);
    Task<bool> UpdateFilterPreferencesAsync(Guid userId, Guid tenantId, FilterPreferencesDto preferences);
    
    // Recommendation Caching
    Task<bool> CacheRecommendationsAsync(Guid userId, Guid tenantId, CacheRequest request);
    Task<List<RecommendedVideoDto>> GetCachedRecommendationsAsync(Guid userId, Guid tenantId, string cacheKey);
    Task<bool> InvalidateCacheAsync(Guid userId, Guid tenantId, string pattern);
    
    // Cross-Platform Recommendations
    Task<List<RecommendedVideoDto>> GetCrossPlatformRecommendationsAsync(Guid userId, Guid tenantId, CrossPlatformRequest request);
    Task<bool> SyncRecommendationModelsAsync(Guid tenantId, SyncRequest request);
    
    // Recommendation Quality
    Task<RecommendationQualityDto> GetRecommendationQualityAsync(Guid userId, Guid tenantId, QualityRequest request);
    Task<bool> ImproveRecommendationQualityAsync(Guid tenantId, QualityImprovementRequest request);
    
    // Personalization Metrics
    Task<PersonalizationMetricsDto> GetPersonalizationMetricsAsync(Guid userId, Guid tenantId);
    Task<bool> UpdatePersonalizationModelAsync(Guid userId, Guid tenantId, PersonalizationUpdateRequest request);
}
