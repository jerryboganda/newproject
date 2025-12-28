namespace StreamVault.Application.Services;

public interface IVideoAnalyticsService
{
    Task TrackViewAsync(Guid videoId, Guid userId, string? sessionId);
    Task TrackEngagementAsync(Guid videoId, Guid userId, EngagementType type, Dictionary<string, object>? metadata = null);
    Task<VideoAnalytics> GetVideoAnalyticsAsync(Guid videoId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
    Task<TenantAnalytics> GetTenantAnalyticsAsync(Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
}

public class VideoAnalytics
{
    public Guid VideoId { get; set; }
    public long TotalViews { get; set; }
    public long UniqueViews { get; set; }
    public double AverageWatchTime { get; set; }
    public long Likes { get; set; }
    public long Dislikes { get; set; }
    public long Comments { get; set; }
    public long Shares { get; set; }
    public Dictionary<string, long> ViewsByDate { get; set; } = new();
    public Dictionary<string, double> WatchTimeDistribution { get; set; } = new();
}

public class TenantAnalytics
{
    public Guid TenantId { get; set; }
    public long TotalVideos { get; set; }
    public long TotalViews { get; set; }
    public long TotalUsers { get; set; }
    public double TotalWatchTime { get; set; }
    public List<TopVideo> TopVideos { get; set; } = new();
    public Dictionary<string, long> ViewsByDate { get; set; } = new();
}

public class TopVideo
{
    public Guid VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public long Views { get; set; }
    public double EngagementRate { get; set; }
}

public enum EngagementType
{
    Like,
    Dislike,
    Comment,
    Share,
    Download,
    Favorite
}
