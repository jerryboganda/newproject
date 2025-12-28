using StreamVault.Application.Analytics.DTOs;

namespace StreamVault.Application.Analytics;

public interface IRevenueAnalyticsService
{
    Task<RevenueOverviewDto> GetRevenueOverviewAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
    Task<List<RevenueTrendDto>> GetRevenueTrendsAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, string? period = null);
    Task<List<TopEarningVideoDto>> GetTopEarningVideosAsync(Guid userId, Guid tenantId, int limit = 10, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
    Task<RevenueBySourceDto> GetRevenueBySourceAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
    Task<RevenueByCountryDto> GetRevenueByCountryAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
    Task<SubscriberAnalyticsDto> GetSubscriberAnalyticsAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
    Task<RevenueForecastDto> GetRevenueForecastAsync(Guid userId, Guid tenantId, int months = 6);
    Task<List<MonthlyRevenueReportDto>> GetMonthlyRevenueReportAsync(Guid userId, Guid tenantId, int months = 12);
    Task<RevenueMetricsDto> GetRevenueMetricsAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
}
