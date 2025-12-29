using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Services
{
    public interface IAnalyticsService
    {
        Task TrackVideoViewAsync(Guid videoId, Guid userId, string? sessionId = null, string? userAgent = null, string? ipAddress = null, string? country = null);
        Task TrackVideoEngagementAsync(Guid videoId, Guid userId, EngagementType type, Dictionary<string, object>? metadata = null);
        Task<VideoAnalytics> GetVideoAnalyticsAsync(Guid videoId, DateTime? startDate = null, DateTime? endDate = null);
        Task<TenantAnalytics> GetTenantAnalyticsAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<ViewSession>> GetVideoViewSessionsAsync(Guid videoId, int limit = 100);
        Task<GeographyAnalytics> GetGeographyAnalyticsAsync(Guid videoId, DateTime? startDate = null, DateTime? endDate = null);
        Task<EngagementReport> GetEngagementReportAsync(Guid videoId, DateTime? startDate = null, DateTime? endDate = null);
        Task<UsageReport> GenerateUsageReportAsync(Guid tenantId, DateTime startDate, DateTime endDate);
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly StreamVaultDbContext _context;
        private readonly ITenantContext _tenantContext;

        public AnalyticsService(StreamVaultDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task TrackVideoViewAsync(Guid videoId, Guid userId, string? sessionId = null, string? userAgent = null, string? ipAddress = null, string? country = null)
        {
            var video = await _context.Videos
                .Include(v => v.Tenant)
                .FirstOrDefaultAsync(v => v.Id == videoId);

            if (video == null)
                throw new ArgumentException("Video not found", nameof(videoId));

            // Check if this is a unique view in the last 30 minutes
            var thirtyMinutesAgo = DateTime.UtcNow.AddMinutes(-30);
            var recentView = await _context.VideoViews
                .AnyAsync(vv => vv.VideoId == videoId && 
                               vv.UserId == userId && 
                               vv.ViewedAt > thirtyMinutesAgo);

            if (!recentView)
            {
                video.ViewCount++;
            }

            // Create view record
            var view = new VideoView
            {
                Id = Guid.NewGuid(),
                VideoId = videoId,
                UserId = userId,
                TenantId = video.TenantId,
                SessionId = sessionId ?? Guid.NewGuid().ToString(),
                UserAgent = userAgent,
                IpAddress = ipAddress,
                Country = country,
                ViewedAt = DateTime.UtcNow,
                IsUniqueView = !recentView
            };

            _context.VideoViews.Add(view);
            await _context.SaveChangesAsync();
        }

        public async Task TrackVideoEngagementAsync(Guid videoId, Guid userId, EngagementType type, Dictionary<string, object>? metadata = null)
        {
            var video = await _context.Videos
                .Include(v => v.Tenant)
                .FirstOrDefaultAsync(v => v.Id == videoId);

            if (video == null)
                throw new ArgumentException("Video not found", nameof(videoId));

            var engagement = new VideoEngagement
            {
                Id = Guid.NewGuid(),
                VideoId = videoId,
                UserId = userId,
                TenantId = video.TenantId,
                Type = type,
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow
            };

            _context.VideoEngagements.Add(engagement);
            await _context.SaveChangesAsync();
        }

        public async Task<VideoAnalytics> GetVideoAnalyticsAsync(Guid videoId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var video = await _context.Videos
                .Include(v => v.Views)
                .Include(v => v.Engagements)
                .FirstOrDefaultAsync(v => v.Id == videoId);

            if (video == null)
                throw new ArgumentException("Video not found", nameof(videoId));

            var query = _context.VideoViews.Where(vv => vv.VideoId == videoId);
            var engagementQuery = _context.VideoEngagements.Where(ve => ve.VideoId == videoId);

            if (startDate.HasValue)
            {
                query = query.Where(vv => vv.ViewedAt >= startDate.Value);
                engagementQuery = engagementQuery.Where(ve => ve.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(vv => vv.ViewedAt <= endDate.Value);
                engagementQuery = engagementQuery.Where(ve => ve.CreatedAt <= endDate.Value);
            }

            var views = await query.ToListAsync();
            var engagements = await engagementQuery.ToListAsync();

            var dailyViews = views
                .GroupBy(v => v.ViewedAt.Date)
                .Select(g => new DailyStat
                {
                    Date = g.Key,
                    Count = g.Count(v => v.IsUniqueView),
                    TotalViews = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            var engagementBreakdown = engagements
                .GroupBy(e => e.Type)
                .Select(g => new EngagementStat
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .ToList();

            return new VideoAnalytics
            {
                VideoId = videoId,
                TotalViews = video.ViewCount,
                UniqueViews = views.Count(v => v.IsUniqueView),
                AverageWatchTime = CalculateAverageWatchTime(views),
                EngagementRate = CalculateEngagementRate(views.Count, engagements.Count),
                DailyViews = dailyViews,
                EngagementBreakdown = engagementBreakdown,
                TopCountries = views
                    .Where(v => !string.IsNullOrEmpty(v.Country))
                    .GroupBy(v => v.Country)
                    .Select(g => new CountryStat
                    {
                        Country = g.Key,
                        Views = g.Count()
                    })
                    .OrderByDescending(c => c.Views)
                    .Take(10)
                    .ToList()
            };
        }

        public async Task<TenantAnalytics> GetTenantAnalyticsAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var videos = await _context.Videos
                .Include(v => v.Views)
                .Include(v => v.Engagements)
                .Where(v => v.TenantId == tenantId)
                .ToListAsync();

            var videoIds = videos.Select(v => v.Id).ToList();

            var viewQuery = _context.VideoViews.Where(vv => videoIds.Contains(vv.VideoId));
            var engagementQuery = _context.VideoEngagements.Where(ve => videoIds.Contains(ve.VideoId));

            if (startDate.HasValue)
            {
                viewQuery = viewQuery.Where(vv => vv.ViewedAt >= startDate.Value);
                engagementQuery = engagementQuery.Where(ve => ve.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                viewQuery = viewQuery.Where(vv => vv.ViewedAt <= endDate.Value);
                engagementQuery = engagementQuery.Where(ve => ve.CreatedAt <= endDate.Value);
            }

            var views = await viewQuery.ToListAsync();
            var engagements = await engagementQuery.ToListAsync();

            return new TenantAnalytics
            {
                TenantId = tenantId,
                TotalVideos = videos.Count,
                TotalViews = videos.Sum(v => v.ViewCount),
                UniqueViews = views.Count(v => v.IsUniqueView),
                TotalStorageUsed = videos.Sum(v => v.FileSize),
                TopVideos = videos
                    .OrderByDescending(v => v.ViewCount)
                    .Take(10)
                    .Select(v => new VideoStat
                    {
                        VideoId = v.Id,
                        Title = v.Title,
                        Views = v.ViewCount
                    })
                    .ToList(),
                DailyStats = views
                    .GroupBy(v => v.ViewedAt.Date)
                    .Select(g => new DailyStat
                    {
                        Date = g.Key,
                        Count = g.Count(),
                        TotalViews = g.Sum(v => v.Video.ViewCount)
                    })
                    .OrderBy(d => d.Date)
                    .ToList()
            };
        }

        public async Task<IEnumerable<ViewSession>> GetVideoViewSessionsAsync(Guid videoId, int limit = 100)
        {
            return await _context.VideoViews
                .Where(vv => vv.VideoId == videoId)
                .OrderByDescending(vv => vv.ViewedAt)
                .Take(limit)
                .Select(vv => new ViewSession
                {
                    Id = vv.Id,
                    UserId = vv.UserId,
                    SessionId = vv.SessionId,
                    ViewedAt = vv.ViewedAt,
                    Duration = vv.Duration,
                    Country = vv.Country,
                    IsUniqueView = vv.IsUniqueView
                })
                .ToListAsync();
        }

        public async Task<GeographyAnalytics> GetGeographyAnalyticsAsync(Guid videoId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.VideoViews.Where(vv => vv.VideoId == videoId);

            if (startDate.HasValue)
                query = query.Where(vv => vv.ViewedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(vv => vv.ViewedAt <= endDate.Value);

            var views = await query.ToListAsync();

            var countryViews = views
                .Where(v => !string.IsNullOrEmpty(v.Country))
                .GroupBy(v => v.Country)
                .Select(g => new CountryStat
                {
                    Country = g.Key,
                    Views = g.Count(),
                    UniqueViews = g.Count(v => v.IsUniqueView)
                })
                .OrderByDescending(c => c.Views)
                .ToList();

            return new GeographyAnalytics
            {
                VideoId = videoId,
                CountryBreakdown = countryViews,
                TotalCountries = countryViews.Count,
                TopCountry = countryViews.FirstOrDefault()
            };
        }

        public async Task<EngagementReport> GetEngagementReportAsync(Guid videoId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.VideoEngagements.Where(ve => ve.VideoId == videoId);

            if (startDate.HasValue)
                query = query.Where(ve => ve.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(ve => ve.CreatedAt <= endDate.Value);

            var engagements = await query.ToListAsync();

            return new EngagementReport
            {
                VideoId = videoId,
                TotalEngagements = engagements.Count,
                Likes = engagements.Count(e => e.Type == EngagementType.Like),
                Dislikes = engagements.Count(e => e.Type == EngagementType.Dislike),
                Comments = engagements.Count(e => e.Type == EngagementType.Comment),
                Shares = engagements.Count(e => e.Type == EngagementType.Share),
                Downloads = engagements.Count(e => e.Type == EngagementType.Download),
                AverageEngagementPerView = CalculateAverageEngagementPerView(videoId, engagements.Count)
            };
        }

        public async Task<UsageReport> GenerateUsageReportAsync(Guid tenantId, DateTime startDate, DateTime endDate)
        {
            var videos = await _context.Videos
                .Include(v => v.Views)
                .Where(v => v.TenantId == tenantId && 
                           v.CreatedAt >= startDate && 
                           v.CreatedAt <= endDate)
                .ToListAsync();

            var videoIds = videos.Select(v => v.Id).ToList();
            var views = await _context.VideoViews
                .Where(vv => videoIds.Contains(vv.VideoId) && 
                             vv.ViewedAt >= startDate && 
                             vv.ViewedAt <= endDate)
                .ToListAsync();

            return new UsageReport
            {
                TenantId = tenantId,
                Period = new DateRange { Start = startDate, End = endDate },
                VideosUploaded = videos.Count,
                TotalStorageUsed = videos.Sum(v => v.FileSize),
                BandwidthUsed = views.Sum(v => v.Video.FileSize), // Approximate
                TotalViews = views.Count,
                UniqueViewers = views.Select(v => v.UserId).Distinct().Count(),
                AverageVideoDuration = videos.Average(v => v.Duration),
                TopPerformingVideos = videos
                    .OrderByDescending(v => v.ViewCount)
                    .Take(5)
                    .Select(v => new VideoStat
                    {
                        VideoId = v.Id,
                        Title = v.Title,
                        Views = v.ViewCount
                    })
                    .ToList()
            };
        }

        private double CalculateAverageWatchTime(IEnumerable<VideoView> views)
        {
            return views.Where(v => v.Duration.HasValue).Average(v => v.Duration?.TotalSeconds ?? 0);
        }

        private double CalculateEngagementRate(int totalViews, int totalEngagements)
        {
            return totalViews > 0 ? (double)totalEngagements / totalViews * 100 : 0;
        }

        private double CalculateAverageEngagementPerView(Guid videoId, int totalEngagements)
        {
            // This would typically involve more complex logic
            return totalEngagements;
        }
    }

    // Analytics DTOs
    public class VideoAnalytics
    {
        public Guid VideoId { get; set; }
        public int TotalViews { get; set; }
        public int UniqueViews { get; set; }
        public double AverageWatchTime { get; set; }
        public double EngagementRate { get; set; }
        public List<DailyStat> DailyViews { get; set; } = new();
        public List<EngagementStat> EngagementBreakdown { get; set; } = new();
        public List<CountryStat> TopCountries { get; set; } = new();
    }

    public class TenantAnalytics
    {
        public Guid TenantId { get; set; }
        public int TotalVideos { get; set; }
        public int TotalViews { get; set; }
        public int UniqueViews { get; set; }
        public long TotalStorageUsed { get; set; }
        public List<VideoStat> TopVideos { get; set; } = new();
        public List<DailyStat> DailyStats { get; set; } = new();
    }

    public class GeographyAnalytics
    {
        public Guid VideoId { get; set; }
        public List<CountryStat> CountryBreakdown { get; set; } = new();
        public int TotalCountries { get; set; }
        public CountryStat? TopCountry { get; set; }
    }

    public class EngagementReport
    {
        public Guid VideoId { get; set; }
        public int TotalEngagements { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public int Comments { get; set; }
        public int Shares { get; set; }
        public int Downloads { get; set; }
        public double AverageEngagementPerView { get; set; }
    }

    public class UsageReport
    {
        public Guid TenantId { get; set; }
        public DateRange Period { get; set; } = new();
        public int VideosUploaded { get; set; }
        public long TotalStorageUsed { get; set; }
        public long BandwidthUsed { get; set; }
        public int TotalViews { get; set; }
        public int UniqueViewers { get; set; }
        public TimeSpan AverageVideoDuration { get; set; }
        public List<VideoStat> TopPerformingVideos { get; set; } = new();
    }

    public class ViewSession
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string SessionId { get; set; } = null!;
        public DateTime ViewedAt { get; set; }
        public TimeSpan? Duration { get; set; }
        public string? Country { get; set; }
        public bool IsUniqueView { get; set; }
    }

    public class DailyStat
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public int TotalViews { get; set; }
    }

    public class EngagementStat
    {
        public EngagementType Type { get; set; }
        public int Count { get; set; }
    }

    public class CountryStat
    {
        public string Country { get; set; } = null!;
        public int Views { get; set; }
        public int UniqueViews { get; set; }
    }

    public class VideoStat
    {
        public Guid VideoId { get; set; }
        public string Title { get; set; } = null!;
        public int Views { get; set; }
    }

    public class DateRange
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public enum EngagementType
    {
        Like,
        Dislike,
        Comment,
        Share,
        Download,
        Bookmark,
        Report
    }
}
