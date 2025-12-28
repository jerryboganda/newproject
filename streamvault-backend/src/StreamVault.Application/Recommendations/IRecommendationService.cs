using StreamVault.Application.Videos.DTOs;
using StreamVault.Application.Auth.DTOs;

namespace StreamVault.Application.Recommendations;

public interface IRecommendationService
{
    Task<List<VideoListDto>> GetRecommendedVideosAsync(Guid userId, Guid tenantId, int limit = 10);
    Task<List<VideoListDto>> GetSimilarVideosAsync(Guid videoId, Guid tenantId, int limit = 10);
    Task<List<VideoListDto>> GetTrendingVideosAsync(Guid tenantId, int limit = 10);
    Task<List<VideoListDto>> GetPopularVideosAsync(Guid tenantId, int limit = 10);
    Task<List<VideoListDto>> GetContinueWatchingAsync(Guid userId, Guid tenantId, int limit = 10);
}
