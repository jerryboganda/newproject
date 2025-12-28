using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Xml.Linq;
using StreamVault.Application.SEO.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.SEO;

public class VideoSEOService : IVideoSEOService
{
    private readonly StreamVaultDbContext _dbContext;

    public VideoSEOService(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VideoSEODto> GetVideoSEOAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var seo = await _dbContext.VideoSEOs
            .FirstOrDefaultAsync(vs => vs.VideoId == videoId);

        if (seo == null)
        {
            // Create default SEO if not exists
            seo = await CreateDefaultSEOAsync(videoId, video);
        }

        return MapToDto(seo);
    }

    public async Task<VideoSEODto> UpdateVideoSEOAsync(Guid videoId, UpdateVideoSEORequest request, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var seo = await _dbContext.VideoSEOs
            .FirstOrDefaultAsync(vs => vs.VideoId == videoId);

        if (seo == null)
        {
            seo = await CreateDefaultSEOAsync(videoId, video);
        }

        // Update SEO properties
        if (request.SEOTitle != null)
            seo.SEOTitle = request.SEOTitle;

        if (request.SEODescription != null)
            seo.SEODescription = request.SEODescription;

        if (request.CanonicalUrl != null)
            seo.CanonicalUrl = request.CanonicalUrl;

        if (request.Keywords != null)
            seo.Keywords = request.Keywords;

        if (request.SchemaType != null)
            seo.SchemaType = request.SchemaType;

        if (request.SchemaData != null)
            seo.SchemaData = request.SchemaData;

        if (request.EnableIndexing.HasValue)
            seo.EnableIndexing = request.EnableIndexing.Value;

        if (request.EnableSitemap.HasValue)
            seo.EnableSitemap = request.EnableSitemap.Value;

        if (request.OpenGraphTitle != null)
            seo.OpenGraphTitle = request.OpenGraphTitle;

        if (request.OpenGraphDescription != null)
            seo.OpenGraphDescription = request.OpenGraphDescription;

        if (request.OpenGraphImage != null)
            seo.OpenGraphImage = request.OpenGraphImage;

        if (request.TwitterCard != null)
            seo.TwitterCard = request.TwitterCard;

        if (request.TwitterTitle != null)
            seo.TwitterTitle = request.TwitterTitle;

        if (request.TwitterDescription != null)
            seo.TwitterDescription = request.TwitterDescription;

        if (request.TwitterImage != null)
            seo.TwitterImage = request.TwitterImage;

        if (request.CustomMetaTags != null)
            seo.CustomMetaTags = request.CustomMetaTags;

        if (request.AltText != null)
            seo.AltText = request.AltText;

        if (request.Language != null)
            seo.Language = request.Language;

        if (request.Region != null)
            seo.Region = request.Region;

        seo.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(seo);
    }

    public async Task<VideoSEODto> GenerateSEOAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .Include(v => v.VideoTags)
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var seo = await _dbContext.VideoSEOs
            .FirstOrDefaultAsync(vs => vs.VideoId == videoId);

        if (seo == null)
        {
            seo = await CreateDefaultSEOAsync(videoId, video);
        }

        // Generate SEO optimized content
        seo.SEOTitle = GenerateOptimizedTitle(video);
        seo.SEODescription = GenerateOptimizedDescription(video);
        seo.Keywords = GenerateKeywords(video);
        seo.SchemaType = "VideoObject";
        seo.SchemaData = GenerateVideoSchema(video);
        seo.OpenGraphTitle = video.Title;
        seo.OpenGraphDescription = video.Description?.Length > 160 
            ? video.Description?.Substring(0, 157) + "..."
            : video.Description;
        seo.OpenGraphImage = video.ThumbnailPath;
        seo.TwitterCard = "summary_large_image";
        seo.TwitterTitle = video.Title;
        seo.TwitterDescription = video.Description?.Length > 160 
            ? video.Description?.Substring(0, 157) + "..."
            : video.Description;
        seo.TwitterImage = video.ThumbnailPath;
        seo.AltText = GenerateAltText(video);
        seo.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(seo);
    }

    public async Task<string> GenerateSitemapAsync(Guid tenantId)
    {
        var videos = await _dbContext.Videos
            .Include(v => v.SEO)
            .Where(v => v.TenantId == tenantId && v.IsPublic && (v.SEO == null || v.SEO.EnableSitemap))
            .ToListAsync();

        var sitemap = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement(XName.Get("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                videos.Select(video => new XElement("url",
                    new XElement("loc", $"https://streamvault.com/videos/{video.Id}"),
                    new XElement("lastmod", video.UpdatedAt.ToString("yyyy-MM-dd")),
                    new XElement("changefreq", "weekly"),
                    new XElement("priority", "0.8")
                ))
            )
        );

        return sitemap.ToString();
    }

    public async Task<string> GenerateRobotsTxtAsync(Guid tenantId)
    {
        var robotsTxt = @"User-agent: *
Allow: /
Disallow: /api/
Disallow: /admin/
Disallow: /account/
Disallow: /upload/

Sitemap: https://streamvault.com/sitemap.xml";

        return robotsTxt;
    }

    public async Task<List<VideoSearchKeywordDto>> GetSearchKeywordsAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var keywords = await _dbContext.VideoSearchKeywords
            .Where(vsk => vsk.VideoId == videoId)
            .OrderByDescending(vsk => vsk.RelevanceScore)
            .ToListAsync();

        return keywords.Select(MapKeywordToDto).ToList();
    }

    public async Task<List<VideoBacklinkDto>> GetBacklinksAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var backlinks = await _dbContext.VideoBacklinks
            .Where(vb => vb.VideoId == videoId)
            .OrderByDescending(vb => vb.DomainAuthority)
            .ToListAsync();

        return backlinks.Select(MapBacklinkToDto).ToList();
    }

    public async Task<SEOScoreDto> CalculateSEOScoreAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .Include(v => v.SEO)
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var seo = video.SEO ?? await CreateDefaultSEOAsync(videoId, video);

        var score = new SEOScoreDto
        {
            VideoId = videoId
        };

        // Title score (30 points)
        score.TitleScore = CalculateTitleScore(seo);
        score.Strengths.AddRange(GetTitleStrengths(seo));
        score.Weaknesses.AddRange(GetTitleWeaknesses(seo));

        // Description score (20 points)
        score.DescriptionScore = CalculateDescriptionScore(seo);
        score.Strengths.AddRange(GetDescriptionStrengths(seo));
        score.Weaknesses.AddRange(GetDescriptionWeaknesses(seo));

        // Keywords score (15 points)
        score.KeywordsScore = CalculateKeywordsScore(seo);
        score.Strengths.AddRange(GetKeywordsStrengths(seo));
        score.Weaknesses.AddRange(GetKeywordsWeaknesses(seo));

        // Technical score (20 points)
        score.TechnicalScore = CalculateTechnicalScore(seo);
        score.Strengths.AddRange(GetTechnicalStrengths(seo));
        score.Weaknesses.AddRange(GetTechnicalWeaknesses(seo));

        // Content score (10 points)
        score.ContentScore = CalculateContentScore(video, seo);
        score.Strengths.AddRange(GetContentStrengths(video, seo));
        score.Weaknesses.AddRange(GetContentWeaknesses(video, seo));

        // Backlinks score (5 points)
        score.BacklinksScore = await CalculateBacklinksScore(videoId);
        score.Strengths.AddRange(await GetBacklinksStrengths(videoId));
        score.Weaknesses.AddRange(await GetBacklinksWeaknesses(videoId));

        score.OverallScore = score.TitleScore + score.DescriptionScore + score.KeywordsScore + 
                              score.TechnicalScore + score.ContentScore + score.BacklinksScore;

        return score;
    }

    public async Task<List<SEORecommendationDto>> GetSEORecommendationsAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .Include(v => v.SEO)
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var seo = video.SEO ?? await CreateDefaultSEOAsync(videoId, video);
        var recommendations = new List<SEORecommendationDto>();

        // Title recommendations
        if (string.IsNullOrEmpty(seo.SEOTitle) || seo.SEOTitle.Length < 30)
        {
            recommendations.Add(new SEORecommendationDto
            {
                Type = "title",
                Priority = "high",
                Title = "Optimize your video title",
                Description = "Your title should be between 30-60 characters and include relevant keywords",
                Action = "Update SEO title with target keywords",
                ImpactScore = 9
            });
        }

        // Description recommendations
        if (string.IsNullOrEmpty(seo.SEODescription) || seo.SEODescription.Length < 120)
        {
            recommendations.Add(new SEORecommendationDto
            {
                Type = "description",
                Priority = "high",
                Title = "Improve video description",
                Description = "Add a compelling description between 120-160 characters",
                Action = "Write a detailed description with keywords",
                ImpactScore = 8
            });
        }

        // Keywords recommendations
        if (seo.Keywords.Count < 5)
        {
            recommendations.Add(new SEORecommendationDto
            {
                Type = "keywords",
                Priority = "medium",
                Title = "Add more keywords",
                Description = "Include at least 5-10 relevant keywords",
                Action = "Research and add target keywords",
                ImpactScore = 7
            });
        }

        // Schema recommendations
        if (string.IsNullOrEmpty(seo.SchemaData))
        {
            recommendations.Add(new SEORecommendationDto
            {
                Type = "technical",
                Priority = "medium",
                Title = "Add structured data",
                Description = "Implement VideoObject schema for better search visibility",
                Action = "Generate and add schema markup",
                ImpactScore = 6
            });
        }

        // Alt text recommendations
        if (string.IsNullOrEmpty(seo.AltText))
        {
            recommendations.Add(new SEORecommendationDto
            {
                Type = "content",
                Priority = "low",
                Title = "Add alt text for thumbnail",
                Description = "Describe your thumbnail for accessibility and SEO",
                Action = "Write descriptive alt text",
                ImpactScore = 4
            });
        }

        return recommendations.OrderByDescending(r => r.ImpactScore).ToList();
    }

    public async Task<bool> SubmitToSearchEnginesAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .Include(v => v.SEO)
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        // In production, this would integrate with Google Search Console API,
        // Bing Webmaster Tools API, etc.
        // For now, we'll just mark the video as submitted
        if (video.SEO == null)
        {
            video.SEO = await CreateDefaultSEOAsync(videoId, video);
        }

        video.SEO.LastIndexed = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<SEOAnalyticsDto> GetSEOAnalyticsAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var keywords = await _dbContext.VideoSearchKeywords
            .Where(vsk => vsk.VideoId == videoId)
            .ToListAsync();

        var backlinks = await _dbContext.VideoBacklinks
            .Where(vb => vb.VideoId == videoId && vb.IsActive)
            .ToListAsync();

        var analytics = new SEOAnalyticsDto
        {
            VideoId = videoId,
            TotalImpressions = keywords.Sum(k => k.Impressions),
            TotalClicks = keywords.Sum(k => k.Clicks),
            AverageCTR = keywords.Any() ? keywords.Average(k => k.CTR) : 0,
            AveragePosition = keywords.Any() ? keywords.Average(k => k.Position) : 0,
            TotalBacklinks = backlinks.Count,
            ReferringDomains = backlinks.Select(b => b.SourceUrl).Distinct().Count(),
            DomainAuthority = backlinks.Any() ? backlinks.Average(b => b.DomainAuthority) : 0,
            TopKeywords = keywords
                .OrderByDescending(k => k.Impressions)
                .Take(10)
                .Select(k => new KeywordPerformanceDto
                {
                    Keyword = k.Keyword,
                    Impressions = k.Impressions,
                    Clicks = k.Clicks,
                    CTR = k.CTR,
                    AveragePosition = k.Position
                })
                .ToList(),
            TrafficSources = GetTrafficSources(videoId)
        };

        return analytics;
    }

    public async Task IndexVideoAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .Include(v => v.SEO)
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        // Generate or update SEO
        if (video.SEO == null)
        {
            video.SEO = await CreateDefaultSEOAsync(videoId, video);
        }

        // Update transcript text for search indexing
        var transcript = await _dbContext.VideoTranscripts
            .FirstOrDefaultAsync(vt => vt.VideoId == videoId);

        if (transcript != null)
        {
            video.SEO.TranscriptText = transcript.Text;
        }

        video.SEO.LastIndexed = DateTimeOffset.UtcNow;
        video.SEO.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    private async Task<VideoSEO> CreateDefaultSEOAsync(Guid videoId, Video video)
    {
        var seo = new VideoSEO
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            SEOTitle = video.Title,
            SEODescription = video.Description,
            EnableIndexing = true,
            EnableSitemap = true,
            Keywords = video.VideoTags.Select(t => t.Tag).ToList(),
            Language = "en",
            Region = "US",
            LastIndexed = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.VideoSEOs.Add(seo);
        await _dbContext.SaveChangesAsync();

        return seo;
    }

    private static VideoSEODto MapToDto(VideoSEO seo)
    {
        return new VideoSEODto
        {
            Id = seo.Id,
            VideoId = seo.VideoId,
            SEOTitle = seo.SEOTitle,
            SEODescription = seo.SEODescription,
            CanonicalUrl = seo.CanonicalUrl,
            Keywords = seo.Keywords,
            SchemaType = seo.SchemaType,
            SchemaData = seo.SchemaData,
            EnableIndexing = seo.EnableIndexing,
            EnableSitemap = seo.EnableSitemap,
            OpenGraphTitle = seo.OpenGraphTitle,
            OpenGraphDescription = seo.OpenGraphDescription,
            OpenGraphImage = seo.OpenGraphImage,
            TwitterCard = seo.TwitterCard,
            TwitterTitle = seo.TwitterTitle,
            TwitterDescription = seo.TwitterDescription,
            TwitterImage = seo.TwitterImage,
            CustomMetaTags = seo.CustomMetaTags,
            AltText = seo.AltText,
            TranscriptText = seo.TranscriptText,
            Tags = seo.Tags,
            Language = seo.Language,
            Region = seo.Region,
            LastIndexed = seo.LastIndexed,
            CreatedAt = seo.CreatedAt,
            UpdatedAt = seo.UpdatedAt
        };
    }

    private static VideoSearchKeywordDto MapKeywordToDto(VideoSearchKeyword keyword)
    {
        return new VideoSearchKeywordDto
        {
            Id = keyword.Id,
            VideoId = keyword.VideoId,
            Keyword = keyword.Keyword,
            SearchVolume = keyword.SearchVolume,
            RelevanceScore = keyword.RelevanceScore,
            Position = keyword.Position,
            Clicks = keyword.Clicks,
            Impressions = keyword.Impressions,
            CTR = keyword.CTR,
            LastUpdated = keyword.LastUpdated
        };
    }

    private static VideoBacklinkDto MapBacklinkToDto(VideoBacklink backlink)
    {
        return new VideoBacklinkDto
        {
            Id = backlink.Id,
            VideoId = backlink.VideoId,
            SourceUrl = backlink.SourceUrl,
            SourceTitle = backlink.SourceTitle,
            AnchorText = backlink.AnchorText,
            LinkType = backlink.LinkType,
            DomainAuthority = backlink.DomainAuthority,
            IsDoFollow = backlink.IsDoFollow,
            DiscoveredAt = backlink.DiscoveredAt,
            LastChecked = backlink.LastChecked,
            IsActive = backlink.IsActive
        };
    }

    private static string GenerateOptimizedTitle(Video video)
    {
        // Generate SEO-optimized title
        var title = video.Title;
        if (title.Length > 60)
        {
            title = title.Substring(0, 57) + "...";
        }
        return title;
    }

    private static string GenerateOptimizedDescription(Video video)
    {
        // Generate SEO-optimized description
        var description = video.Description ?? "";
        if (string.IsNullOrEmpty(description))
        {
            description = $"Watch {video.Title} on StreamVault. " +
                         $"{video.VideoTags.Select(t => t.Tag).Take(3).Aggregate((a, b) => a + ", " + b)} and more.";
        }
        else if (description.Length > 160)
        {
            description = description.Substring(0, 157) + "...";
        }
        return description;
    }

    private static List<string> GenerateKeywords(Video video)
    {
        // Generate keywords from tags and title
        var keywords = video.VideoTags.Select(t => t.Tag).ToList();
        
        // Extract words from title
        var titleWords = video.Title.Split(' ')
            .Where(w => w.Length > 3)
            .Select(w => w.ToLower())
            .ToList();
        
        keywords.AddRange(titleWords);
        
        return keywords.Distinct().Take(10).ToList();
    }

    private static string GenerateVideoSchema(Video video)
    {
        var schema = new
        {
            @context = "https://schema.org",
            @type = "VideoObject",
            name = video.Title,
            description = video.Description,
            thumbnailUrl = video.ThumbnailPath,
            uploadDate = video.CreatedAt.ToString("yyyy-MM-dd"),
            duration = $"PT{video.DurationSeconds}S"
        };

        return JsonSerializer.Serialize(schema);
    }

    private static string GenerateAltText(Video video)
    {
        return $"Video thumbnail for {video.Title}";
    }

    private static int CalculateTitleScore(VideoSEO seo)
    {
        int score = 0;
        if (!string.IsNullOrEmpty(seo.SEOTitle))
        {
            if (seo.SEOTitle.Length >= 30 && seo.SEOTitle.Length <= 60) score += 15;
            else if (seo.SEOTitle.Length >= 20 && seo.SEOTitle.Length <= 70) score += 10;
            else score += 5;

            if (seo.Keywords.Any(k => seo.SEOTitle.Contains(k, StringComparison.OrdinalIgnoreCase))) score += 15;
        }
        return Math.Min(score, 30);
    }

    private static int CalculateDescriptionScore(VideoSEO seo)
    {
        int score = 0;
        if (!string.IsNullOrEmpty(seo.SEODescription))
        {
            if (seo.SEODescription.Length >= 120 && seo.SEODescription.Length <= 160) score += 10;
            else if (seo.SEODescription.Length >= 100 && seo.SEODescription.Length <= 180) score += 7;
            else score += 3;

            if (seo.Keywords.Any(k => seo.SEODescription.Contains(k, StringComparison.OrdinalIgnoreCase))) score += 10;
        }
        return Math.Min(score, 20);
    }

    private static int CalculateKeywordsScore(VideoSEO seo)
    {
        int score = 0;
        if (seo.Keywords.Count >= 5) score += 5;
        if (seo.Keywords.Count >= 10) score += 5;
        if (seo.Keywords.Count <= 20) score += 5; // Not too many
        
        return Math.Min(score, 15);
    }

    private static int CalculateTechnicalScore(VideoSEO seo)
    {
        int score = 0;
        if (!string.IsNullOrEmpty(seo.SchemaData)) score += 5;
        if (seo.EnableIndexing) score += 5;
        if (seo.EnableSitemap) score += 5;
        if (!string.IsNullOrEmpty(seo.CanonicalUrl)) score += 5;
        
        return Math.Min(score, 20);
    }

    private static int CalculateContentScore(Video video, VideoSEO seo)
    {
        int score = 0;
        if (!string.IsNullOrEmpty(video.Description)) score += 3;
        if (video.VideoTags.Any()) score += 3;
        if (!string.IsNullOrEmpty(seo.AltText)) score += 2;
        if (!string.IsNullOrEmpty(seo.TranscriptText)) score += 2;
        
        return Math.Min(score, 10);
    }

    private async Task<int> CalculateBacklinksScore(Guid videoId)
    {
        var backlinkCount = await _dbContext.VideoBacklinks
            .CountAsync(vb => vb.VideoId == videoId && vb.IsActive);

        if (backlinkCount >= 10) return 5;
        if (backlinkCount >= 5) return 3;
        if (backlinkCount >= 1) return 1;
        return 0;
    }

    private static List<string> GetTitleStrengths(VideoSEO seo)
    {
        var strengths = new List<string>();
        if (!string.IsNullOrEmpty(seo.SEOTitle) && seo.SEOTitle.Length >= 30 && seo.SEOTitle.Length <= 60)
            strengths.Add("Title has optimal length");
        if (seo.Keywords.Any(k => !string.IsNullOrEmpty(seo.SEOTitle) && seo.SEOTitle.Contains(k, StringComparison.OrdinalIgnoreCase)))
            strengths.Add("Title contains relevant keywords");
        return strengths;
    }

    private static List<string> GetTitleWeaknesses(VideoSEO seo)
    {
        var weaknesses = new List<string>();
        if (string.IsNullOrEmpty(seo.SEOTitle))
            weaknesses.Add("Missing SEO title");
        else if (seo.SEOTitle.Length < 30)
            weaknesses.Add("Title is too short");
        else if (seo.SEOTitle.Length > 60)
            weaknesses.Add("Title is too long");
        return weaknesses;
    }

    private static List<string> GetDescriptionStrengths(VideoSEO seo)
    {
        var strengths = new List<string>();
        if (!string.IsNullOrEmpty(seo.SEODescription) && seo.SEODescription.Length >= 120 && seo.SEODescription.Length <= 160)
            strengths.Add("Description has optimal length");
        return strengths;
    }

    private static List<string> GetDescriptionWeaknesses(VideoSEO seo)
    {
        var weaknesses = new List<string>();
        if (string.IsNullOrEmpty(seo.SEODescription))
            weaknesses.Add("Missing SEO description");
        else if (seo.SEODescription.Length < 120)
            weaknesses.Add("Description is too short");
        return weaknesses;
    }

    private static List<string> GetKeywordsStrengths(VideoSEO seo)
    {
        var strengths = new List<string>();
        if (seo.Keywords.Count >= 10)
            strengths.Add("Good number of keywords");
        return strengths;
    }

    private static List<string> GetKeywordsWeaknesses(VideoSEO seo)
    {
        var weaknesses = new List<string>();
        if (seo.Keywords.Count < 5)
            weaknesses.Add("Not enough keywords");
        return weaknesses;
    }

    private static List<string> GetTechnicalStrengths(VideoSEO seo)
    {
        var strengths = new List<string>();
        if (!string.IsNullOrEmpty(seo.SchemaData))
            strengths.Add("Structured data implemented");
        if (seo.EnableIndexing)
            strengths.Add("Search engine indexing enabled");
        return strengths;
    }

    private static List<string> GetTechnicalWeaknesses(VideoSEO seo)
    {
        var weaknesses = new List<string>();
        if (string.IsNullOrEmpty(seo.SchemaData))
            weaknesses.Add("Missing structured data");
        if (string.IsNullOrEmpty(seo.CanonicalUrl))
            weaknesses.Add("Missing canonical URL");
        return weaknesses;
    }

    private static List<string> GetContentStrengths(Video video, VideoSEO seo)
    {
        var strengths = new List<string>();
        if (video.VideoTags.Any())
            strengths.Add("Video has tags");
        if (!string.IsNullOrEmpty(seo.TranscriptText))
            strengths.Add("Video has transcript");
        return strengths;
    }

    private static List<string> GetContentWeaknesses(Video video, VideoSEO seo)
    {
        var weaknesses = new List<string>();
        if (!video.VideoTags.Any())
            weaknesses.Add("No video tags");
        if (string.IsNullOrEmpty(seo.AltText))
            weaknesses.Add("Missing alt text for thumbnail");
        return weaknesses;
    }

    private async Task<List<string>> GetBacklinksStrengths(Guid videoId)
    {
        var strengths = new List<string>();
        var backlinkCount = await _dbContext.VideoBacklinks
            .CountAsync(vb => vb.VideoId == videoId && vb.IsActive);
        
        if (backlinkCount >= 10)
            strengths.Add("Good number of backlinks");
        
        return strengths;
    }

    private async Task<List<string>> GetBacklinksWeaknesses(Guid videoId)
    {
        var weaknesses = new List<string>();
        var backlinkCount = await _dbContext.VideoBacklinks
            .CountAsync(vb => vb.VideoId == videoId && vb.IsActive);
        
        if (backlinkCount == 0)
            weaknesses.Add("No backlinks found");
        
        return weaknesses;
    }

    private static List<TrafficSourceDto> GetTrafficSources(Guid videoId)
    {
        // In production, this would come from analytics data
        return new List<TrafficSourceDto>
        {
            new TrafficSourceDto { Source = "google", Visits = 150, Percentage = 45.5 },
            new TrafficSourceDto { Source = "youtube", Visits = 100, Percentage = 30.3 },
            new TrafficSourceDto { Source = "social", Visits = 50, Percentage = 15.2 },
            new TrafficSourceDto { Source = "direct", Visits = 30, Percentage = 9.0 }
        };
    }
}
