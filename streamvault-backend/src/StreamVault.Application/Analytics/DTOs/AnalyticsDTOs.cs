using System.ComponentModel.DataAnnotations;
using StreamVault.Domain.Entities;
using StreamVault.Application.Auth.DTOs;

namespace StreamVault.Application.Analytics.DTOs;

public class TrackEventRequest
{
    [Required]
    public Guid VideoId { get; set; }

    [Required]
    public AnalyticsEventType EventType { get; set; }

    public double? PositionSeconds { get; set; }

    public string? DeviceType { get; set; }

    public string? Browser { get; set; }

    public string? OS { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? Referrer { get; set; }

    public string? UTMSource { get; set; }

    public string? UTMMedium { get; set; }

    public string? UTMCampaign { get; set; }

    public string? SessionId { get; set; }

    public string? Metadata { get; set; }
}

public class AnalyticsFilter
{
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public AnalyticsEventType? EventType { get; set; }
    public string? Country { get; set; }
    public string? DeviceType { get; set; }
    public string? Browser { get; set; }
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 50;
}

public class VideoAnalyticsDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public Guid UserId { get; set; }
    public AnalyticsEventType EventType { get; set; }
    public double? PositionSeconds { get; set; }
    public string? DeviceType { get; set; }
    public string? Browser { get; set; }
    public string? OS { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Referrer { get; set; }
    public string? UTMSource { get; set; }
    public string? UTMMedium { get; set; }
    public string? UTMCampaign { get; set; }
    public string? SessionId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? Metadata { get; set; }
    public UserDto User { get; set; } = null!;
}

public class AnalyticsOverviewDto
{
    public int TotalVideos { get; set; }
    public int TotalViews { get; set; }
    public int UniqueViewers { get; set; }
    public double TotalWatchTimeMinutes { get; set; }
    public double AverageWatchTimeMinutes { get; set; }
    public double AverageCompletionRate { get; set; }
    public int TotalLikes { get; set; }
    public int TotalComments { get; set; }
    public int TotalShares { get; set; }
    public List<ViewsByDateDto> ViewsByDate { get; set; } = new();
    public List<TopVideoDto> TopVideos { get; set; } = new();
    public Dictionary<string, int> DeviceBreakdown { get; set; } = new();
    public Dictionary<string, int> CountryBreakdown { get; set; } = new();
}

public class ViewsByDateDto
{
    public DateOnly Date { get; set; }
    public int Views { get; set; }
}

public class TopVideoDto
{
    public Guid VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Views { get; set; }
    public double CompletionRate { get; set; }
}

public class PopularVideoDto
{
    public Guid VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int Views { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
    public double CompletionRate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class AnalyticsExportDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "text/csv";
    public byte[] Data { get; set; } = Array.Empty<byte>();
}

public class ViewerRetentionDto
{
    public Guid VideoId { get; set; }
    public Dictionary<int, double> RetentionByPercentage { get; set; } = new(); // Key: percentage (10, 20, 30...), Value: retention percentage
    public double AverageRetention { get; set; }
    public Dictionary<int, int> ViewersAtPosition { get; set; } = new(); // Key: position in seconds, Value: number of viewers
}

public class GeographicAnalyticsDto
{
    public Guid VideoId { get; set; }
    public Dictionary<string, int> ViewsByCountry { get; set; } = new();
    public Dictionary<string, int> ViewsByCity { get; set; } = new();
    public List<CountryStatsDto> TopCountries { get; set; } = new();
}

public class CountryStatsDto
{
    public string Country { get; set; } = string.Empty;
    public int Views { get; set; }
    public double Percentage { get; set; }
    public double AverageWatchTimeMinutes { get; set; }
}

public class DeviceAnalyticsDto
{
    public Guid VideoId { get; set; }
    public Dictionary<string, int> ViewsByDeviceType { get; set; } = new();
    public Dictionary<string, int> ViewsByBrowser { get; set; } = new();
    public Dictionary<string, int> ViewsByOS { get; set; } = new();
    public List<DeviceStatsDto> TopDevices { get; set; } = new();
}

public class DeviceStatsDto
{
    public string DeviceType { get; set; } = string.Empty;
    public int Views { get; set; }
    public double Percentage { get; set; }
    public double AverageWatchTimeMinutes { get; set; }
    public double CompletionRate { get; set; }
}

public class EngagementAnalyticsDto
{
    public Guid VideoId { get; set; }
    public int Likes { get; set; }
    public int Dislikes { get; set; }
    public int Comments { get; set; }
    public int Shares { get; set; }
    public int Downloads { get; set; }
    public double LikeRatio { get; set; } // Likes / (Likes + Dislikes)
    public double CommentRatio { get; set; } // Comments / Views
    public double ShareRatio { get; set; } // Shares / Views
    public List<EngagementEventDto> EngagementOverTime { get; set; } = new();
}

public class EngagementEventDto
{
    public DateOnly Date { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
    public int Shares { get; set; }
}
