using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class VideoSEO
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VideoId { get; set; }

    [MaxLength(60)]
    public string? SEOTitle { get; set; }

    [MaxLength(160)]
    public string? SEODescription { get; set; }

    [MaxLength(255)]
    public string? CanonicalUrl { get; set; }

    public List<string> Keywords { get; set; } = new();

    public string? SchemaType { get; set; }

    public string? SchemaData { get; set; } // JSON string for structured data

    public bool EnableIndexing { get; set; } = true;

    public bool EnableSitemap { get; set; } = true;

    public string? OpenGraphTitle { get; set; }

    public string? OpenGraphDescription { get; set; }

    public string? OpenGraphImage { get; set; }

    public string? TwitterCard { get; set; }

    public string? TwitterTitle { get; set; }

    public string? TwitterDescription { get; set; }

    public string? TwitterImage { get; set; }

    public string? CustomMetaTags { get; set; } // JSON string for custom meta tags

    public string? AltText { get; set; } // For thumbnail

    public string? TranscriptText { get; set; } // For search indexing

    public List<VideoTag> Tags { get; set; } = new();

    public string? Language { get; set; }

    public string? Region { get; set; }

    public DateTimeOffset LastIndexed { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public Video Video { get; set; } = null!;
}

public class VideoSearchKeyword
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VideoId { get; set; }

    [Required, MaxLength(100)]
    public string Keyword { get; set; } = string.Empty;

    public int SearchVolume { get; set; }

    public double RelevanceScore { get; set; }

    public int Position { get; set; }

    public int Clicks { get; set; }

    public int Impressions { get; set; }

    public double CTR { get; set; } // Click-through rate

    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public Video Video { get; set; } = null!;
}

public class VideoBacklink
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VideoId { get; set; }

    [Required, MaxLength(500)]
    public string SourceUrl { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string SourceTitle { get; set; } = string.Empty;

    public string? AnchorText { get; set; }

    public LinkType LinkType { get; set; }

    public int DomainAuthority { get; set; }

    public bool IsDoFollow { get; set; } = true;

    public DateTimeOffset DiscoveredAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastChecked { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Video Video { get; set; } = null!;
}

public enum LinkType
{
    Internal,
    External,
    Social,
    Directory,
    Forum,
    Blog,
    News,
    Video,
    Other
}
