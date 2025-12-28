using StreamVault.Application.SEO.DTOs;
using StreamVault.Domain.Entities;

namespace StreamVault.Application.SEO;

public interface IVideoSEOService
{
    Task<VideoSEODto> GetVideoSEOAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<VideoSEODto> UpdateVideoSEOAsync(Guid videoId, UpdateVideoSEORequest request, Guid userId, Guid tenantId);
    Task<VideoSEODto> GenerateSEOAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<string> GenerateSitemapAsync(Guid tenantId);
    Task<string> GenerateRobotsTxtAsync(Guid tenantId);
    Task<List<VideoSearchKeywordDto>> GetSearchKeywordsAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<List<VideoBacklinkDto>> GetBacklinksAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<SEOScoreDto> CalculateSEOScoreAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<List<SEORecommendationDto>> GetSEORecommendationsAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<bool> SubmitToSearchEnginesAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<SEOAnalyticsDto> GetSEOAnalyticsAsync(Guid videoId, Guid userId, Guid tenantId);
    Task IndexVideoAsync(Guid videoId, Guid userId, Guid tenantId);
}
