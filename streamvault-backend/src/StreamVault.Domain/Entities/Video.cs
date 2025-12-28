using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class Video
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    public Guid? CategoryId { get; set; }

    public string? OriginalFileName { get; set; }

    [Required, MaxLength(500)]
    public string StoragePath { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string? ThumbnailPath { get; set; }

    public long FileSizeBytes { get; set; }

    public string? MimeType { get; set; }

    public int DurationSeconds { get; set; }

    public VideoStatus Status { get; set; } = VideoStatus.Uploading;

    public bool IsPublic { get; set; } = false;

    public int ViewCount { get; set; } = 0;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? PublishedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public VideoCategory? Category { get; set; }
    public ICollection<VideoTag> VideoTags { get; set; } = new List<VideoTag>();
    public ICollection<VideoProcessingJob> ProcessingJobs { get; set; } = new List<VideoProcessingJob>();
    public VideoSEO? SEO { get; set; }
    public VideoMonetization? Monetization { get; set; }
    public List<VideoPurchase> Purchases { get; set; } = new();
    public List<VideoRental> Rentals { get; set; } = new();
    public List<AdRevenue> AdRevenues { get; set; } = new();
}

public enum VideoStatus
{
    Uploading,
    Uploaded,
    Processing,
    Processed,
    Failed,
    Deleted
}
