using System.ComponentModel.DataAnnotations;
using StreamVault.Domain.Interfaces;

namespace StreamVault.Domain.Entities;

public class VideoAnalyticsHourlyAggregate : ITenantEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public Guid VideoId { get; set; }

    [Required]
    public DateTimeOffset BucketStartUtc { get; set; }

    public int Views { get; set; }

    public int UniqueViewers { get; set; }

    public double WatchTimeSeconds { get; set; }

    public int Completes { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Video Video { get; set; } = null!;
}

public class VideoAnalyticsDailyAggregate : ITenantEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public Guid VideoId { get; set; }

    [Required]
    public DateOnly DateUtc { get; set; }

    public int Views { get; set; }

    public int UniqueViewers { get; set; }

    public double WatchTimeSeconds { get; set; }

    public int Completes { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Video Video { get; set; } = null!;
}

public class VideoAnalyticsDailyCountryAggregate : ITenantEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public Guid VideoId { get; set; }

    [Required]
    public DateOnly DateUtc { get; set; }

    [Required, MaxLength(2)]
    public string CountryCode { get; set; } = "XX";

    public int Views { get; set; }

    public int UniqueViewers { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Video Video { get; set; } = null!;
}
