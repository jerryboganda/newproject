using StreamVault.Application.Recommendations.DTOs;

namespace StreamVault.Application.Recommendations;

public interface IVideoRecommendationService
{
    Task<List<RecommendedVideoDto>> GetRecommendationsForUserAsync(Guid userId, Guid tenantId, int limit = 20, string? algorithm = null);
    Task<List<RecommendedVideoDto>> GetSimilarVideosAsync(Guid videoId, Guid userId, Guid tenantId, int limit = 10);
    Task<List<RecommendedVideoDto>> GetTrendingVideosAsync(Guid tenantId, int limit = 20, string? category = null);
    Task<List<RecommendedVideoDto>> GetPersonalizedFeedAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20);
    Task<List<RecommendedVideoDto>> GetContinueWatchingAsync(Guid userId, Guid tenantId, int limit = 10);
    Task<List<RecommendedVideoDto>> GetWatchAgainAsync(Guid userId, Guid tenantId, int limit = 10);
    Task<List<RecommendedVideoDto>> GetRecommendedForYouAsync(Guid userId, Guid tenantId, int limit = 20);
    Task<List<RecommendedVideoDto>> GetPopularInCategoryAsync(Guid categoryId, Guid userId, Guid tenantId, int limit = 20);
    Task<List<RecommendedVideoDto>> GetRecommendedFromCreatorsAsync(List<Guid> creatorIds, Guid userId, Guid tenantId, int limit = 20);
    Task<bool> UpdateWatchHistoryAsync(Guid userId, Guid videoId, Guid tenantId, int watchedSeconds, int totalSeconds);
    Task<bool> RecordUserInteractionAsync(Guid userId, Guid videoId, Guid tenantId, string interactionType, Dictionary<string, object>? metadata = null);
    Task<List<RecommendedVideoDto>> GetSearchRecommendationsAsync(Guid userId, Guid tenantId, string query, int limit = 10);
}
