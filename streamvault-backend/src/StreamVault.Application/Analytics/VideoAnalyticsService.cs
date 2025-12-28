using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using StreamVault.Application.Analytics.DTOs;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Analytics;

public class VideoAnalyticsDashboardService : IVideoAnalyticsDashboardService
{
    private readonly StreamVaultDbContext _dbContext;

    public VideoAnalyticsDashboardService(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task TrackEventAsync(TrackEventRequest request, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == request.VideoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var analytics = new VideoAnalytics
        {
            Id = Guid.NewGuid(),
            VideoId = request.VideoId,
            UserId = userId,
            EventType = request.EventType,
            PositionSeconds = request.PositionSeconds,
            DeviceType = request.DeviceType,
            Browser = request.Browser,
            OS = request.OS,
            Country = request.Country,
            City = request.City,
            Referrer = request.Referrer,
            UTMSource = request.UTMSource,
            UTMMedium = request.UTMMedium,
            UTMCampaign = request.UTMCampaign,
            SessionId = request.SessionId,
            Metadata = request.Metadata,
            Timestamp = DateTimeOffset.UtcNow
        };

        _dbContext.VideoAnalytics.Add(analytics);
        await _dbContext.SaveChangesAsync();

        // Update video view count if this is a view event
        if (request.EventType == AnalyticsEventType.View)
        {
            video.ViewCount++;
            await _dbContext.SaveChangesAsync();
        }

        // Schedule summary update (in production, this would be a background job)
        _ = Task.Run(() => UpdateVideoAnalyticsSummaryAsync(request.VideoId, DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    public async Task<VideoAnalyticsDto> GetVideoAnalyticsAsync(Guid videoId, Guid userId, Guid tenantId, AnalyticsFilter? filter = null)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var query = _dbContext.VideoAnalytics
            .Include(va => va.User)
            .Where(va => va.VideoId == videoId);

        if (filter != null)
        {
            if (filter.StartDate.HasValue)
                query = query.Where(va => va.Timestamp >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(va => va.Timestamp <= filter.EndDate.Value);

            if (filter.EventType.HasValue)
                query = query.Where(va => va.EventType == filter.EventType.Value);

            if (!string.IsNullOrEmpty(filter.Country))
                query = query.Where(va => va.Country == filter.Country);

            if (!string.IsNullOrEmpty(filter.DeviceType))
                query = query.Where(va => va.DeviceType == filter.DeviceType);

            if (!string.IsNullOrEmpty(filter.Browser))
                query = query.Where(va => va.Browser == filter.Browser);
        }

        var analytics = await query
            .OrderByDescending(va => va.Timestamp)
            .FirstOrDefaultAsync();

        if (analytics == null)
            throw new Exception("No analytics data found");

        return MapToDto(analytics);
    }

    public async Task<List<VideoAnalyticsDto>> GetVideoAnalyticsListAsync(Guid videoId, Guid userId, Guid tenantId, AnalyticsFilter filter)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var query = _dbContext.VideoAnalytics
            .Include(va => va.User)
            .Where(va => va.VideoId == videoId);

        if (filter.StartDate.HasValue)
            query = query.Where(va => va.Timestamp >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(va => va.Timestamp <= filter.EndDate.Value);

        if (filter.EventType.HasValue)
            query = query.Where(va => va.EventType == filter.EventType.Value);

        if (!string.IsNullOrEmpty(filter.Country))
            query = query.Where(va => va.Country == filter.Country);

        if (!string.IsNullOrEmpty(filter.DeviceType))
            query = query.Where(va => va.DeviceType == filter.DeviceType);

        if (!string.IsNullOrEmpty(filter.Browser))
            query = query.Where(va => va.Browser == filter.Browser);

        var page = filter.Page ?? 1;
        var pageSize = filter.PageSize ?? 50;

        var analytics = await query
            .OrderByDescending(va => va.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return analytics.Select(MapToDto).ToList();
    }

    public async Task<AnalyticsOverviewDto> GetAnalyticsOverviewAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var start = startDate ?? DateTimeOffset.UtcNow.AddDays(-30);
        var end = endDate ?? DateTimeOffset.UtcNow;

        var videos = await _dbContext.Videos
            .Where(v => v.UserId == userId && v.TenantId == tenantId)
            .ToListAsync();

        var videoIds = videos.Select(v => v.Id).ToList();

        // Get analytics data
        var analytics = await _dbContext.VideoAnalytics
            .Where(va => videoIds.Contains(va.VideoId) && va.Timestamp >= start && va.Timestamp <= end)
            .ToListAsync();

        // Calculate overview metrics
        var totalViews = analytics.Count(a => a.EventType == AnalyticsEventType.View);
        var uniqueViewers = analytics.Select(a => a.UserId).Distinct().Count();
        var watchTimeEvents = analytics.Where(a => a.EventType == AnalyticsEventType.Complete || a.EventType == AnalyticsEventType.Exit);
        var totalWatchTimeMinutes = watchTimeEvents.Sum(a => a.PositionSeconds ?? 0) / 60.0;
        var averageWatchTimeMinutes = totalViews > 0 ? totalWatchTimeMinutes / totalViews : 0;

        // Get engagement metrics
        var likes = analytics.Count(a => a.EventType == AnalyticsEventType.Like);
        var dislikes = analytics.Count(a => a.EventType == AnalyticsEventType.Dislike);
        var comments = analytics.Count(a => a.EventType == AnalyticsEventType.Comment);
        var shares = analytics.Count(a => a.EventType == AnalyticsEventType.Share);

        // Group views by date
        var viewsByDate = analytics
            .Where(a => a.EventType == AnalyticsEventType.View)
            .GroupBy(a => DateOnly.FromDateTime(a.Timestamp.DateTime))
            .Select(g => new ViewsByDateDto { Date = g.Key, Views = g.Count() })
            .OrderBy(x => x.Date)
            .ToList();

        // Get top videos
        var topVideos = analytics
            .Where(a => a.EventType == AnalyticsEventType.View)
            .GroupBy(a => a.VideoId)
            .Select(g => new
            {
                VideoId = g.Key,
                Views = g.Count(),
                CompletionRate = CalculateCompletionRate(g.Key)
            })
            .OrderByDescending(x => x.Views)
            .Take(5)
            .Join(videos, x => x.VideoId, v => v.Id, (x, v) => new TopVideoDto
            {
                VideoId = x.VideoId,
                Title = v.Title,
                Views = x.Views,
                CompletionRate = x.CompletionRate
            })
            .ToList();

        // Device breakdown
        var deviceBreakdown = analytics
            .Where(a => !string.IsNullOrEmpty(a.DeviceType))
            .GroupBy(a => a.DeviceType!)
            .ToDictionary(g => g.Key, g => g.Count());

        // Country breakdown
        var countryBreakdown = analytics
            .Where(a => !string.IsNullOrEmpty(a.Country))
            .GroupBy(a => a.Country!)
            .ToDictionary(g => g.Key, g => g.Count());

        return new AnalyticsOverviewDto
        {
            TotalVideos = videos.Count,
            TotalViews = totalViews,
            UniqueViewers = uniqueViewers,
            TotalWatchTimeMinutes = totalWatchTimeMinutes,
            AverageWatchTimeMinutes = averageWatchTimeMinutes,
            AverageCompletionRate = CalculateAverageCompletionRate(videoIds),
            TotalLikes = likes,
            TotalComments = comments,
            TotalShares = shares,
            ViewsByDate = viewsByDate,
            TopVideos = topVideos,
            DeviceBreakdown = deviceBreakdown,
            CountryBreakdown = countryBreakdown
        };
    }

    public async Task<List<PopularVideoDto>> GetPopularVideosAsync(Guid userId, Guid tenantId, int? limit = null, DateTimeOffset? startDate = null)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var start = startDate ?? DateTimeOffset.UtcNow.AddDays(-30);
        var take = limit ?? 10;

        var videos = await _dbContext.Videos
            .Where(v => v.UserId == userId && v.TenantId == tenantId)
            .ToListAsync();

        var videoIds = videos.Select(v => v.Id).ToList();

        var analytics = await _dbContext.VideoAnalytics
            .Where(va => videoIds.Contains(va.VideoId) && va.Timestamp >= start)
            .ToListAsync();

        var popularVideos = analytics
            .Where(a => a.EventType == AnalyticsEventType.View)
            .GroupBy(a => a.VideoId)
            .Select(g => new
            {
                VideoId = g.Key,
                Views = g.Count(),
                Likes = analytics.Count(a => a.VideoId == g.Key && a.EventType == AnalyticsEventType.Like),
                Comments = analytics.Count(a => a.VideoId == g.Key && a.EventType == AnalyticsEventType.Comment),
                CompletionRate = CalculateCompletionRate(g.Key)
            })
            .OrderByDescending(x => x.Views)
            .Take(take)
            .Join(videos, x => x.VideoId, v => v.Id, (x, v) => new PopularVideoDto
            {
                VideoId = x.VideoId,
                Title = v.Title,
                ThumbnailUrl = v.ThumbnailPath ?? string.Empty,
                Views = x.Views,
                Likes = x.Likes,
                Comments = x.Comments,
                CompletionRate = x.CompletionRate,
                CreatedAt = v.CreatedAt
            })
            .ToList();

        return popularVideos;
    }

    public async Task<AnalyticsExportDto> ExportAnalyticsAsync(Guid videoId, Guid userId, Guid tenantId, AnalyticsFilter filter)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var analytics = await GetVideoAnalyticsListAsync(videoId, userId, tenantId, filter);

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Timestamp,EventType,PositionSeconds,DeviceType,Browser,OS,Country,City,Referrer,SessionId");

        foreach (var item in analytics)
        {
            csv.AppendLine($"{item.Timestamp:yyyy-MM-dd HH:mm:ss},{item.EventType},{item.PositionSeconds},{item.DeviceType},{item.Browser},{item.OS},{item.Country},{item.City},{item.Referrer},{item.SessionId}");
        }

        var data = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"video_analytics_{videoId}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

        return new AnalyticsExportDto
        {
            FileName = fileName,
            ContentType = "text/csv",
            Data = data
        };
    }

    public async Task<ViewerRetentionDto> GetViewerRetentionAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        if (video.DurationSeconds <= 0)
            throw new Exception("Video duration is not available");

        var analytics = await _dbContext.VideoAnalytics
            .Where(va => va.VideoId == videoId && va.EventType == AnalyticsEventType.Exit)
            .Select(va => new { va.PositionSeconds, va.UserId })
            .ToListAsync();

        var retentionByPercentage = new Dictionary<int, double>();
        var totalViews = await _dbContext.VideoAnalytics
            .CountAsync(va => va.VideoId == videoId && va.EventType == AnalyticsEventType.View);

        for (int i = 10; i <= 100; i += 10)
        {
            var positionThreshold = (video.DurationSeconds * i) / 100;
            var viewersAtThisPoint = analytics.Count(a => a.PositionSeconds >= positionThreshold);
            var retentionPercentage = totalViews > 0 ? (viewersAtThisPoint * 100.0) / totalViews : 0;
            retentionByPercentage[i] = retentionPercentage;
        }

        // Calculate viewers at specific positions (every minute)
        var viewersAtPosition = new Dictionary<int, int>();
        for (int i = 0; i <= video.DurationSeconds; i += 60)
        {
            var viewersAtMinute = analytics.Count(a => a.PositionSeconds >= i);
            viewersAtPosition[i] = viewersAtMinute;
        }

        var averageRetention = retentionByPercentage.Values.DefaultIfEmpty(0).Average();

        return new ViewerRetentionDto
        {
            VideoId = videoId,
            RetentionByPercentage = retentionByPercentage,
            AverageRetention = averageRetention,
            ViewersAtPosition = viewersAtPosition
        };
    }

    public async Task<GeographicAnalyticsDto> GetGeographicAnalyticsAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var analytics = await _dbContext.VideoAnalytics
            .Where(va => va.VideoId == videoId)
            .ToListAsync();

        var viewsByCountry = analytics
            .Where(a => !string.IsNullOrEmpty(a.Country))
            .GroupBy(a => a.Country!)
            .ToDictionary(g => g.Key, g => g.Count());

        var viewsByCity = analytics
            .Where(a => !string.IsNullOrEmpty(a.City))
            .GroupBy(a => a.City!)
            .ToDictionary(g => g.Key, g => g.Count());

        var totalViews = analytics.Count;
        var topCountries = viewsByCountry
            .Select(kvp => new CountryStatsDto
            {
                Country = kvp.Key,
                Views = kvp.Value,
                Percentage = totalViews > 0 ? (kvp.Value * 100.0) / totalViews : 0,
                AverageWatchTimeMinutes = CalculateAverageWatchTimeForCountry(videoId, kvp.Key)
            })
            .OrderByDescending(c => c.Views)
            .Take(10)
            .ToList();

        return new GeographicAnalyticsDto
        {
            VideoId = videoId,
            ViewsByCountry = viewsByCountry,
            ViewsByCity = viewsByCity,
            TopCountries = topCountries
        };
    }

    public async Task<DeviceAnalyticsDto> GetDeviceAnalyticsAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var analytics = await _dbContext.VideoAnalytics
            .Where(va => va.VideoId == videoId)
            .ToListAsync();

        var viewsByDeviceType = analytics
            .Where(a => !string.IsNullOrEmpty(a.DeviceType))
            .GroupBy(a => a.DeviceType!)
            .ToDictionary(g => g.Key, g => g.Count());

        var viewsByBrowser = analytics
            .Where(a => !string.IsNullOrEmpty(a.Browser))
            .GroupBy(a => a.Browser!)
            .ToDictionary(g => g.Key, g => g.Count());

        var viewsByOS = analytics
            .Where(a => !string.IsNullOrEmpty(a.OS))
            .GroupBy(a => a.OS!)
            .ToDictionary(g => g.Key, g => g.Count());

        var totalViews = analytics.Count;
        var topDevices = viewsByDeviceType
            .Select(kvp => new DeviceStatsDto
            {
                DeviceType = kvp.Key,
                Views = kvp.Value,
                Percentage = totalViews > 0 ? (kvp.Value * 100.0) / totalViews : 0,
                AverageWatchTimeMinutes = CalculateAverageWatchTimeForDevice(videoId, kvp.Key),
                CompletionRate = CalculateCompletionRateForDevice(videoId, kvp.Key)
            })
            .OrderByDescending(d => d.Views)
            .ToList();

        return new DeviceAnalyticsDto
        {
            VideoId = videoId,
            ViewsByDeviceType = viewsByDeviceType,
            ViewsByBrowser = viewsByBrowser,
            ViewsByOS = viewsByOS,
            TopDevices = topDevices
        };
    }

    public async Task<EngagementAnalyticsDto> GetEngagementAnalyticsAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var analytics = await _dbContext.VideoAnalytics
            .Where(va => va.VideoId == videoId)
            .ToListAsync();

        var likes = analytics.Count(a => a.EventType == AnalyticsEventType.Like);
        var dislikes = analytics.Count(a => a.EventType == AnalyticsEventType.Dislike);
        var comments = analytics.Count(a => a.EventType == AnalyticsEventType.Comment);
        var shares = analytics.Count(a => a.EventType == AnalyticsEventType.Share);
        var downloads = analytics.Count(a => a.EventType == AnalyticsEventType.Download);

        var totalViews = analytics.Count(a => a.EventType == AnalyticsEventType.View);
        var likeRatio = (likes + dislikes) > 0 ? (likes * 100.0) / (likes + dislikes) : 0;
        var commentRatio = totalViews > 0 ? (comments * 100.0) / totalViews : 0;
        var shareRatio = totalViews > 0 ? (shares * 100.0) / totalViews : 0;

        // Engagement over time
        var engagementOverTime = analytics
            .Where(a => a.EventType == AnalyticsEventType.Like || 
                       a.EventType == AnalyticsEventType.Comment || 
                       a.EventType == AnalyticsEventType.Share)
            .GroupBy(a => DateOnly.FromDateTime(a.Timestamp.DateTime))
            .Select(g => new EngagementEventDto
            {
                Date = g.Key,
                Likes = g.Count(a => a.EventType == AnalyticsEventType.Like),
                Comments = g.Count(a => a.EventType == AnalyticsEventType.Comment),
                Shares = g.Count(a => a.EventType == AnalyticsEventType.Share)
            })
            .OrderBy(x => x.Date)
            .ToList();

        return new EngagementAnalyticsDto
        {
            VideoId = videoId,
            Likes = likes,
            Dislikes = dislikes,
            Comments = comments,
            Shares = shares,
            Downloads = downloads,
            LikeRatio = likeRatio,
            CommentRatio = commentRatio,
            ShareRatio = shareRatio,
            EngagementOverTime = engagementOverTime
        };
    }

    public async Task UpdateVideoAnalyticsSummaryAsync(Guid videoId, DateOnly date)
    {
        var existingSummary = await _dbContext.VideoAnalyticsSummaries
            .FirstOrDefaultAsync(vas => vas.VideoId == videoId && vas.Date == date);

        var analytics = await _dbContext.VideoAnalytics
            .Where(va => va.VideoId == videoId && 
                        DateOnly.FromDateTime(va.Timestamp.DateTime) == date)
            .ToListAsync();

        var totalViews = analytics.Count(a => a.EventType == AnalyticsEventType.View);
        var uniqueViewers = analytics.Select(a => a.UserId).Distinct().Count();
        var watchTimeEvents = analytics.Where(a => a.EventType == AnalyticsEventType.Complete || a.EventType == AnalyticsEventType.Exit);
        var totalWatchTimeSeconds = watchTimeEvents.Sum(a => a.PositionSeconds ?? 0);
        var averageWatchTimeSeconds = totalViews > 0 ? totalWatchTimeSeconds / totalViews : 0;

        var likes = analytics.Count(a => a.EventType == AnalyticsEventType.Like);
        var dislikes = analytics.Count(a => a.EventType == AnalyticsEventType.Dislike);
        var comments = analytics.Count(a => a.EventType == AnalyticsEventType.Comment);
        var shares = analytics.Count(a => a.EventType == AnalyticsEventType.Share);
        var downloads = analytics.Count(a => a.EventType == AnalyticsEventType.Download);

        var deviceBreakdown = analytics
            .Where(a => !string.IsNullOrEmpty(a.DeviceType))
            .GroupBy(a => a.DeviceType!)
            .ToDictionary(g => g.Key, g => g.Count());

        var countryBreakdown = analytics
            .Where(a => !string.IsNullOrEmpty(a.Country))
            .GroupBy(a => a.Country!)
            .ToDictionary(g => g.Key, g => g.Count());

        // Calculate viewer retention at each 10% interval
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId);

        var viewerRetention = new Dictionary<int, int>();
        if (video != null && video.DurationSeconds > 0)
        {
            for (int i = 10; i <= 100; i += 10)
            {
                var positionThreshold = (video.DurationSeconds * i) / 100;
                var viewersAtThisPoint = watchTimeEvents.Count(a => a.PositionSeconds >= positionThreshold);
                viewerRetention[i] = viewersAtThisPoint;
            }
        }

        if (existingSummary == null)
        {
            existingSummary = new VideoAnalyticsSummary
            {
                Id = Guid.NewGuid(),
                VideoId = videoId,
                Date = date
            };
            _dbContext.VideoAnalyticsSummaries.Add(existingSummary);
        }

        existingSummary.TotalViews = totalViews;
        existingSummary.UniqueViewers = uniqueViewers;
        existingSummary.TotalWatchTimeSeconds = totalWatchTimeSeconds;
        existingSummary.AverageWatchTimeSeconds = averageWatchTimeSeconds;
        existingSummary.CompletionRate = CalculateCompletionRate(videoId);
        existingSummary.Likes = likes;
        existingSummary.Dislikes = dislikes;
        existingSummary.Comments = comments;
        existingSummary.Shares = shares;
        existingSummary.Downloads = downloads;
        existingSummary.DeviceBreakdown = deviceBreakdown;
        existingSummary.CountryBreakdown = countryBreakdown;
        existingSummary.ViewerRetention = viewerRetention;
        existingSummary.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    private static VideoAnalyticsDto MapToDto(VideoAnalytics analytics)
    {
        return new VideoAnalyticsDto
        {
            Id = analytics.Id,
            VideoId = analytics.VideoId,
            UserId = analytics.UserId,
            EventType = analytics.EventType,
            PositionSeconds = analytics.PositionSeconds,
            DeviceType = analytics.DeviceType,
            Browser = analytics.Browser,
            OS = analytics.OS,
            Country = analytics.Country,
            City = analytics.City,
            Referrer = analytics.Referrer,
            UTMSource = analytics.UTMSource,
            UTMMedium = analytics.UTMMedium,
            UTMCampaign = analytics.UTMCampaign,
            SessionId = analytics.SessionId,
            Timestamp = analytics.Timestamp,
            Metadata = analytics.Metadata,
            User = new UserDto
            {
                Id = analytics.User.Id,
                Email = analytics.User.Email,
                FirstName = analytics.User.FirstName,
                LastName = analytics.User.LastName,
                AvatarUrl = analytics.User.AvatarUrl
            }
        };
    }

    private double CalculateCompletionRate(Guid videoId)
    {
        // This is a simplified calculation
        // In production, you'd use more sophisticated methods
        return 75.0; // Placeholder
    }

    private double CalculateAverageCompletionRate(List<Guid> videoIds)
    {
        // Simplified average
        return 70.0; // Placeholder
    }

    private double CalculateAverageWatchTimeForCountry(Guid videoId, string country)
    {
        // Simplified calculation
        return 5.5; // Placeholder in minutes
    }

    private double CalculateAverageWatchTimeForDevice(Guid videoId, string deviceType)
    {
        // Simplified calculation
        return 6.0; // Placeholder in minutes
    }

    private double CalculateCompletionRateForDevice(Guid videoId, string deviceType)
    {
        // Simplified calculation
        return 80.0; // Placeholder
    }
}
