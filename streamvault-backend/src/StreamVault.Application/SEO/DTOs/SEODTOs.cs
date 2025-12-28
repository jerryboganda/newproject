using System.ComponentModel.DataAnnotations;
using StreamVault.Domain.Entities;

namespace StreamVault.Application.SEO.DTOs;

public class UpdateVideoSEORequest
{
    [MaxLength(60)]
    public string? SEOTitle { get; set; }

    [MaxLength(160)]
    public string? SEODescription { get; set; }

    [MaxLength(255)]
    public string? CanonicalUrl { get; set; }

    public List<string> Keywords { get; set; } = new();

    public string? SchemaType { get; set; }

    public string? SchemaData { get; set; }

    public bool? EnableIndexing { get; set; }

    public bool? EnableSitemap { get; set; }

    public string? OpenGraphTitle { get; set; }

    public string? OpenGraphDescription { get; set; }

    public string? OpenGraphImage { get; set; }

    public string? TwitterCard { get; set; }

    public string? TwitterTitle { get; set; }

    public string? TwitterDescription { get; set; }

    public string? TwitterImage { get; set; }

    public string? CustomMetaTags { get; set; }

    public string? AltText { get; set; }

    public string? Language { get; set; }

    public string? Region { get; set; }
}

public class VideoSEODto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public string? SEOTitle { get; set; }
    public string? SEODescription { get; set; }
    public string? CanonicalUrl { get; set; }
    public List<string> Keywords { get; set; } = new();
    public string? SchemaType { get; set; }
    public string? SchemaData { get; set; }
    public bool EnableIndexing { get; set; }
    public bool EnableSitemap { get; set; }
    public string? OpenGraphTitle { get; set; }
    public string? OpenGraphDescription { get; set; }
    public string? OpenGraphImage { get; set; }
    public string? TwitterCard { get; set; }
    public string? TwitterTitle { get; set; }
    public string? TwitterDescription { get; set; }
    public string? TwitterImage { get; set; }
    public string? CustomMetaTags { get; set; }
    public string? AltText { get; set; }
    public string? TranscriptText { get; set; }
    public List<VideoTag> Tags { get; set; } = new();
    public string? Language { get; set; }
    public string? Region { get; set; }
    public DateTimeOffset LastIndexed { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class VideoSearchKeywordDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public int SearchVolume { get; set; }
    public double RelevanceScore { get; set; }
    public int Position { get; set; }
    public int Clicks { get; set; }
    public int Impressions { get; set; }
    public double CTR { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}

public class VideoBacklinkDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public string SourceTitle { get; set; } = string.Empty;
    public string? AnchorText { get; set; }
    public LinkType LinkType { get; set; }
    public int DomainAuthority { get; set; }
    public bool IsDoFollow { get; set; }
    public DateTimeOffset DiscoveredAt { get; set; }
    public DateTimeOffset? LastChecked { get; set; }
    public bool IsActive { get; set; }
}

public class SEOScoreDto
{
    public Guid VideoId { get; set; }
    public int OverallScore { get; set; } // 0-100
    public int TitleScore { get; set; }
    public int DescriptionScore { get; set; }
    public int KeywordsScore { get; set; }
    public int TechnicalScore { get; set; }
    public int ContentScore { get; set; }
    public int BacklinksScore { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
}

public class SEORecommendationDto
{
    public string Type { get; set; } = string.Empty; // title, description, keywords, technical, content, backlinks
    public string Priority { get; set; } = string.Empty; // high, medium, low
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Action { get; set; }
    public int ImpactScore { get; set; } // 1-10
}

public class SEOAnalyticsDto
{
    public Guid VideoId { get; set; }
    public int TotalImpressions { get; set; }
    public int TotalClicks { get; set; }
    public double AverageCTR { get; set; }
    public double AveragePosition { get; set; }
    public int TotalBacklinks { get; set; }
    public int ReferringDomains { get; set; }
    public double DomainAuthority { get; set; }
    public List<KeywordPerformanceDto> TopKeywords { get; set; } = new();
    public List<TrafficSourceDto> TrafficSources { get; set; } = new();
}

public class KeywordPerformanceDto
{
    public string Keyword { get; set; } = string.Empty;
    public int Impressions { get; set; }
    public int Clicks { get; set; }
    public double CTR { get; set; }
    public double AveragePosition { get; set; }
}

public class TrafficSourceDto
{
    public string Source { get; set; } = string.Empty; // google, youtube, social, direct, referral
    public int Visits { get; set; }
    public double Percentage { get; set; }
}

public class SitemapEntry
{
    public string Url { get; set; } = string.Empty;
    public DateTimeOffset LastModified { get; set; }
    public string ChangeFrequency { get; set; } = "weekly";
    public string Priority { get; set; } = "0.8";
}

public class RobotsTxtEntry
{
    public string UserAgent { get; set; } = "*";
    public List<string> Allow { get; set; } = new();
    public List<string> Disallow { get; set; } = new();
    public string? Sitemap { get; set; }
}
