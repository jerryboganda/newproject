using System.ComponentModel.DataAnnotations;
using StreamVault.Domain.Interfaces;

namespace StreamVault.Domain.Entities;

public class VideoAnalytics : ITenantEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VideoId { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    public Guid? UserId { get; set; }

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

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    public string? Metadata { get; set; } // JSON string for additional data

    // Navigation properties
    public Video Video { get; set; } = null!;
    public User? User { get; set; }
}

public class VideoAnalyticsSummary
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VideoId { get; set; }

    public DateOnly Date { get; set; }

    public int TotalViews { get; set; } = 0;

    public int UniqueViewers { get; set; } = 0;

    public double TotalWatchTimeSeconds { get; set; } = 0;

    public double AverageWatchTimeSeconds { get; set; } = 0;

    public double CompletionRate { get; set; } = 0; // Percentage

    public int Likes { get; set; } = 0;

    public int Dislikes { get; set; } = 0;

    public int Comments { get; set; } = 0;

    public int Shares { get; set; } = 0;

    public int Downloads { get; set; } = 0;

    public Dictionary<string, int> DeviceBreakdown { get; set; } = new();

    public Dictionary<string, int> CountryBreakdown { get; set; } = new();

    public Dictionary<int, int> ViewerRetention { get; set; } = new(); // Percentage of viewers at each 10% interval

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public Video Video { get; set; } = null!;
}

public enum AnalyticsEventType
{
    View,
    Play,
    Pause,
    Seek,
    Complete,
    Like,
    Dislike,
    Comment,
    Share,
    Download,
    Embed,
    ThumbnailClick,
    QualityChange,
    SpeedChange,
    Fullscreen,
    Exit
}
