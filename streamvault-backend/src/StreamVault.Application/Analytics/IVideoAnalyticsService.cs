using StreamVault.Application.Analytics.DTOs;
using StreamVault.Domain.Entities;

namespace StreamVault.Application.Analytics;

public interface IVideoAnalyticsDashboardService
{
    Task TrackEventAsync(TrackEventRequest request, Guid userId, Guid tenantId);
    Task<VideoAnalyticsDto> GetVideoAnalyticsAsync(Guid videoId, Guid userId, Guid tenantId, AnalyticsFilter? filter = null);
    Task<List<VideoAnalyticsDto>> GetVideoAnalyticsListAsync(Guid videoId, Guid userId, Guid tenantId, AnalyticsFilter filter);
    Task<AnalyticsOverviewDto> GetAnalyticsOverviewAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
    Task<List<PopularVideoDto>> GetPopularVideosAsync(Guid userId, Guid tenantId, int? limit = null, DateTimeOffset? startDate = null);
    Task<AnalyticsExportDto> ExportAnalyticsAsync(Guid videoId, Guid userId, Guid tenantId, AnalyticsFilter filter);
    Task<ViewerRetentionDto> GetViewerRetentionAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<GeographicAnalyticsDto> GetGeographicAnalyticsAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<DeviceAnalyticsDto> GetDeviceAnalyticsAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<EngagementAnalyticsDto> GetEngagementAnalyticsAsync(Guid videoId, Guid userId, Guid tenantId);
    Task UpdateVideoAnalyticsSummaryAsync(Guid videoId, DateOnly date);
}
