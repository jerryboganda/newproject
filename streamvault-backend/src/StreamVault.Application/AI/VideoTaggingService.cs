using Microsoft.EntityFrameworkCore;
using StreamVault.Application.AI.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;
using System.Text.Json;

namespace StreamVault.Application.AI;

public class VideoTaggingService : IVideoTaggingService
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly ILogger<VideoTaggingService> _logger;
    private readonly HttpClient _httpClient;

    public VideoTaggingService(StreamVaultDbContext dbContext, ILogger<VideoTaggingService> logger, HttpClient httpClient)
    {
        _dbContext = dbContext;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<List<GeneratedTagDto>> GenerateTagsAsync(Guid videoId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .Include(v => v.VideoTags)
                .ThenInclude(vt => vt.Tag)
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var allTags = new List<GeneratedTagDto>();

        // Generate tags from different sources
        var transcriptTags = await GenerateTagsFromTranscriptAsync(videoId);
        var thumbnailTags = await GenerateTagsFromThumbnailAsync(videoId, tenantId);
        var audioTags = await GenerateTagsFromAudioAsync(videoId, tenantId);
        var metadataTags = await GenerateTagsFromMetadataAsync(new VideoMetadataDto
        {
            Title = video.Title,
            Description = video.Description ?? "",
            ExistingTags = video.VideoTags.Select(vt => vt.Tag.Name).ToList(),
            DurationSeconds = video.DurationSeconds
        });

        allTags.AddRange(transcriptTags);
        allTags.AddRange(thumbnailTags);
        allTags.AddRange(audioTags);
        allTags.AddRange(metadataTags);

        // Remove duplicates and sort by confidence
        var uniqueTags = allTags
            .GroupBy(t => t.Tag.ToLower())
            .Select(g => new GeneratedTagDto
            {
                Tag = g.First().Tag,
                Confidence = g.Max(t => t.Confidence),
                Source = string.Join(", ", g.Select(t => t.Source).Distinct()),
                Type = g.First().Type,
                RelevanceScore = g.Max(t => t.RelevanceScore),
                Frequency = g.Sum(t => t.Frequency),
                Context = g.SelectMany(t => t.Context).Distinct().ToList(),
                Metadata = g.First().Metadata
            })
            .OrderByDescending(t => t.Confidence)
            .Take(20)
            .ToList();

        return uniqueTags;
    }

    public async Task<List<GeneratedTagDto>> GenerateTagsFromTranscriptAsync(string transcript)
    {
        // In production, this would use NLP/AI services like:
        // - OpenAI GPT for keyword extraction
        // - Azure Cognitive Services for text analytics
        // - spaCy or NLTK for NLP processing
        
        var tags = new List<GeneratedTagDto>();

        if (string.IsNullOrEmpty(transcript))
            return tags;

        // Simulate AI processing
        var words = transcript.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var wordFrequency = words
            .GroupBy(w => w.ToLower())
            .Where(g => g.Count() > 2) // Words appearing more than twice
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToList();

        foreach (var word in wordFrequency)
        {
            tags.Add(new GeneratedTagDto
            {
                Tag = word.Key,
                Confidence = Math.Min(0.9, word.Count() * 0.1),
                Source = "transcript",
                Type = "keyword",
                RelevanceScore = word.Count(),
                Frequency = word.Count(),
                Context = new List<string> { "spoken" }
            });
        }

        // Extract potential topics (simplified)
        var topics = ExtractTopicsFromText(transcript);
        foreach (var topic in topics)
        {
            tags.Add(new GeneratedTagDto
            {
                Tag = topic,
                Confidence = 0.7,
                Source = "transcript",
                Type = "topic",
                RelevanceScore = 5,
                Frequency = 1,
                Context = new List<string> { "topic" }
            });
        }

        return tags;
    }

    public async Task<List<GeneratedTagDto>> GenerateTagsFromThumbnailAsync(Guid videoId, Guid tenantId)
    {
        // In production, this would use:
        // - Computer Vision APIs (Azure, AWS, Google)
        // - Custom ML models for object detection
        // - Scene recognition services
        
        var tags = new List<GeneratedTagDto>();

        // Simulate image analysis
        var simulatedObjects = new[]
        {
            new { Object = "person", Confidence = 0.85 },
            new { Object = "outdoor", Confidence = 0.75 },
            new { Object = "technology", Confidence = 0.60 },
            new { Object = "computer", Confidence = 0.70 },
            new { Object = "office", Confidence = 0.65 }
        };

        foreach (var obj in simulatedObjects)
        {
            tags.Add(new GeneratedTagDto
            {
                Tag = obj.Object,
                Confidence = obj.Confidence,
                Source = "thumbnail",
                Type = "object",
                RelevanceScore = (int)(obj.Confidence * 10),
                Frequency = 1,
                Context = new List<string> { "visual" }
            });
        }

        return tags;
    }

    public async Task<List<GeneratedTagDto>> GenerateTagsFromAudioAsync(Guid videoId, Guid tenantId)
    {
        // In production, this would use:
        // - Speech-to-text services
        // - Audio analysis APIs
        // - Music recognition services
        
        var tags = new List<GeneratedTagDto>();

        // Simulate audio analysis
        var simulatedAudioFeatures = new[]
        {
            new { Feature = "speech", Confidence = 0.90, Type = "content" },
            new { Feature = "music", Confidence = 0.30, Type = "background" },
            new { Feature = "english", Confidence = 0.95, Type = "language" },
            new { Feature = "tutorial", Confidence = 0.70, Type = "format" },
            new { Feature = "educational", Confidence = 0.75, Type = "content" }
        };

        foreach (var feature in simulatedAudioFeatures)
        {
            tags.Add(new GeneratedTagDto
            {
                Tag = feature.Feature,
                Confidence = feature.Confidence,
                Source = "audio",
                Type = feature.Type,
                RelevanceScore = (int)(feature.Confidence * 10),
                Frequency = 1,
                Context = new List<string> { "audio" }
            });
        }

        return tags;
    }

    public async Task<List<GeneratedTagDto>> GenerateTagsFromMetadataAsync(VideoMetadataDto metadata)
    {
        var tags = new List<GeneratedTagDto>();

        // Extract tags from title
        var titleWords = ExtractKeywordsFromText(metadata.Title);
        foreach (var word in titleWords.Take(5))
        {
            tags.Add(new GeneratedTagDto
            {
                Tag = word,
                Confidence = 0.8,
                Source = "metadata",
                Type = "title",
                RelevanceScore = 8,
                Frequency = 1,
                Context = new List<string> { "title" }
            });
        }

        // Extract tags from description
        if (!string.IsNullOrEmpty(metadata.Description))
        {
            var descWords = ExtractKeywordsFromText(metadata.Description);
            foreach (var word in descWords.Take(10))
            {
                tags.Add(new GeneratedTagDto
                {
                    Tag = word,
                    Confidence = 0.6,
                    Source = "metadata",
                    Type = "description",
                    RelevanceScore = 5,
                    Frequency = 1,
                    Context = new List<string> { "description" }
                });
            }
        }

        // Add duration-based tags
        if (metadata.DurationSeconds > 0)
        {
            var durationMinutes = metadata.DurationSeconds / 60;
            if (durationMinutes < 5)
            {
                tags.Add(new GeneratedTagDto { Tag = "short", Confidence = 0.9, Source = "metadata", Type = "duration", RelevanceScore = 9 });
            }
            else if (durationMinutes > 20)
            {
                tags.Add(new GeneratedTagDto { Tag = "long-form", Confidence = 0.9, Source = "metadata", Type = "duration", RelevanceScore = 9 });
            }
        }

        return tags;
    }

    public async Task<List<GeneratedCategoryDto>> SuggestCategoriesAsync(Guid videoId, Guid tenantId)
    {
        var tags = await GenerateTagsAsync(videoId, tenantId);
        var video = await _dbContext.Videos
            .Include(v => v.Category)
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        // Get all available categories
        var categories = await _dbContext.Categories
            .Where(c => c.TenantId == tenantId)
            .ToListAsync();

        var suggestions = new List<GeneratedCategoryDto>();

        // Simple category matching based on tags
        foreach (var category in categories)
        {
            var matchingTags = tags.Where(t => 
                t.Tag.Contains(category.Name, StringComparison.OrdinalIgnoreCase) ||
                category.Name.Contains(t.Tag, StringComparison.OrdinalIgnoreCase)).ToList();

            if (matchingTags.Any())
            {
                var confidence = matchingTags.Max(t => t.Confidence);
                suggestions.Add(new GeneratedCategoryDto
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    Confidence = confidence,
                    Reason = $"Based on {matchingTags.Count} matching tags",
                    SupportingTags = matchingTags.Select(t => t.Tag).ToList()
                });
            }
        }

        return suggestions.OrderByDescending(s => s.Confidence).Take(3).ToList();
    }

    public async Task<VideoContentAnalysisDto> AnalyzeVideoContentAsync(Guid videoId, Guid tenantId)
    {
        var tags = await GenerateTagsAsync(videoId, tenantId);
        var categories = await SuggestCategoriesAsync(videoId, tenantId);

        var video = await _dbContext.Videos
            .Include(v => v.User)
            .Include(v => v.Category)
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        // Simulate comprehensive analysis
        var analysis = new VideoContentAnalysisDto
        {
            VideoId = videoId,
            Title = video.Title,
            GeneratedTags = tags,
            SuggestedCategories = categories,
            DetectedObjects = GenerateSimulatedObjects(),
            DetectedPersons = GenerateSimulatedPersons(),
            DetectedLocations = GenerateSimulatedLocations(),
            DetectedText = GenerateSimulatedText(),
            EmotionAnalysis = GenerateSimulatedEmotionAnalysis(),
            SceneAnalysis = GenerateSimulatedSceneAnalysis(),
            AudioAnalysis = GenerateSimulatedAudioAnalysis(),
            KeyTopics = tags.Where(t => t.Type == "topic").Select(t => t.Tag).Take(5).ToList(),
            Summary = GenerateVideoSummary(video.Title, video.Description, tags),
            ContentQualityScore = CalculateContentQualityScore(video, tags),
            AnalysisMetadata = new Dictionary<string, object>
            {
                ["analysisVersion"] = "1.0",
                ["processingTime"] = "2.5s"
            },
            AnalyzedAt = DateTimeOffset.UtcNow
        };

        return analysis;
    }

    public async Task<bool> ApplyGeneratedTagsAsync(Guid videoId, List<Guid> tagIds, Guid tenantId)
    {
        var video = await _dbContext.Videos
            .Include(v => v.VideoTags)
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        // Get tags to add
        var tagsToAdd = await _dbContext.Tags
            .Where(t => tagIds.Contains(t.Id) && t.TenantId == tenantId)
            .ToListAsync();

        foreach (var tag in tagsToAdd)
        {
            if (!video.VideoTags.Any(vt => vt.TagId == tag.Id))
            {
                video.VideoTags.Add(new VideoTag
                {
                    Id = Guid.NewGuid(),
                    VideoId = videoId,
                    TagId = tag.Id,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<List<VideoInsightDto>> GenerateVideoInsightsAsync(Guid videoId, Guid tenantId)
    {
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var insights = new List<VideoInsightDto>();

        // Generate various insights
        if (video.DurationSeconds > 1800) // 30 minutes
        {
            insights.Add(new VideoInsightDto
            {
                InsightType = "engagement",
                Title = "Long-form content opportunity",
                Description = "This video is longer than 30 minutes. Consider adding chapters to improve viewer retention.",
                Timestamp = TimeSpan.FromMinutes(5),
                Confidence = 0.8,
                RelatedTags = new List<string> { "long-form", "chapters" },
                Priority = InsightPriority.Medium
            });
        }

        if (video.Title.Length > 100)
        {
            insights.Add(new VideoInsightDto
            {
                InsightType = "seo",
                Title = "Title optimization needed",
                Description = "Your title is quite long. Consider shortening it for better visibility.",
                Confidence = 0.9,
                RelatedTags = new List<string> { "seo", "title-optimization" },
                Priority = InsightPriority.High
            });
        }

        if (string.IsNullOrEmpty(video.Description))
        {
            insights.Add(new VideoInsightDto
            {
                InsightType = "content",
                Title = "Add video description",
                Description = "Adding a detailed description can improve SEO and provide more context to viewers.",
                Confidence = 0.95,
                RelatedTags = new List<string> { "description", "seo" },
                Priority = InsightPriority.High
            });
        }

        return insights;
    }

    public async Task<List<TrendingTagDto>> GetTrendingTagsAsync(Guid tenantId, int limit = 50)
    {
        // Get tags used in recent videos
        var recentTags = await _dbContext.VideoTags
            .Include(vt => vt.Tag)
            .Include(vt => vt.Video)
            .Where(vt => vt.Video.TenantId == tenantId && 
                        vt.Video.CreatedAt >= DateTimeOffset.UtcNow.AddDays(-30))
            .GroupBy(vt => vt.Tag.Name)
            .Select(g => new { Tag = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(limit)
            .ToListAsync();

        var trendingTags = new List<TrendingTagDto>();
        foreach (var item in recentTags)
        {
            // Calculate growth rate (simplified)
            var growthRate = CalculateTagGrowthRate(item.Tag, tenantId);
            
            trendingTags.Add(new TrendingTagDto
            {
                Tag = item.Tag,
                UsageCount = item.Count,
                GrowthRate = growthRate,
                RelatedVideos = new List<string>(), // Would populate with actual video IDs
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Category = "general"
            });
        }

        return trendingTags.OrderByDescending(t => t.GrowthRate).ToList();
    }

    public async Task<List<TagSuggestionDto>> GetTagSuggestionsAsync(Guid videoId, Guid tenantId, string? query = null)
    {
        var video = await _dbContext.Videos
            .Include(v => v.VideoTags)
                .ThenInclude(vt => vt.Tag)
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var existingTagNames = video.VideoTags.Select(vt => vt.Tag.Name).ToHashSet();

        // Get all available tags
        var allTags = await _dbContext.Tags
            .Where(t => t.TenantId == tenantId)
            .ToListAsync();

        var suggestions = new List<TagSuggestionDto>();

        foreach (var tag in allTags)
        {
            if (!string.IsNullOrEmpty(query) && !tag.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                continue;

            var relevanceScore = CalculateTagRelevance(tag, video);
            
            suggestions.Add(new TagSuggestionDto
            {
                Tag = tag.Name,
                RelevanceScore = relevanceScore,
                Reason = GetTagSuggestionReason(tag, video),
                UsageCount = await _dbContext.VideoTags.CountAsync(vt => vt.TagId == tag.Id),
                IsExisting = existingTagNames.Contains(tag.Name),
                TagId = tag.Id
            });
        }

        return suggestions
            .Where(s => s.RelevanceScore > 0.3)
            .OrderByDescending(s => s.RelevanceScore)
            .Take(20)
            .ToList();
    }

    private List<string> ExtractTopicsFromText(string text)
    {
        // Simplified topic extraction
        // In production, use NLP libraries or AI services
        var topics = new List<string>();
        
        var topicKeywords = new[]
        {
            "tutorial", "review", "how-to", "guide", "tips", "tricks",
            "technology", "programming", "design", "marketing", "business",
            "education", "science", "health", "fitness", "cooking"
        };

        foreach (var keyword in topicKeywords)
        {
            if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                topics.Add(keyword);
            }
        }

        return topics;
    }

    private List<string> ExtractKeywordsFromText(string text)
    {
        // Simple keyword extraction
        var stopWords = new[] { "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by", "is", "are", "was", "were" };
        
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3 && !stopWords.Contains(w.ToLower()))
            .Select(w => w.Trim().ToLower())
            .Distinct()
            .ToList();
    }

    private List<DetectedObjectDto> GenerateSimulatedObjects()
    {
        return new List<DetectedObjectDto>
        {
            new() { ObjectName = "person", Confidence = 0.85, X = 100, Y = 100, Width = 200, Height = 300, Timestamp = TimeSpan.FromSeconds(10) },
            new() { ObjectName = "laptop", Confidence = 0.75, X = 300, Y = 200, Width = 150, Height = 100, Timestamp = TimeSpan.FromSeconds(15) },
            new() { ObjectName = "book", Confidence = 0.60, X = 450, Y = 150, Width = 80, Height = 120, Timestamp = TimeSpan.FromSeconds(20) }
        };
    }

    private List<DetectedPersonDto> GenerateSimulatedPersons()
    {
        return new List<DetectedPersonDto>
        {
            new() { PersonId = "person_1", Confidence = 0.90, X = 100, Y = 100, Width = 200, Height = 300, IsSpeaker = true, ScreenTimePercentage = 75.5 },
            new() { PersonId = "person_2", Confidence = 0.70, X = 400, Y = 100, Width = 180, Height = 280, IsSpeaker = false, ScreenTimePercentage = 15.2 }
        };
    }

    private List<DetectedLocationDto> GenerateSimulatedLocations()
    {
        return new List<DetectedLocationDto>
        {
            new() { LocationName = "office", LocationType = "indoor", Confidence = 0.80, Timestamp = TimeSpan.FromSeconds(5) },
            new() { LocationName = "meeting room", LocationType = "indoor", Confidence = 0.65, Timestamp = TimeSpan.FromSeconds(30) }
        };
    }

    private List<DetectedTextDto> GenerateSimulatedText()
    {
        return new List<DetectedTextDto>
        {
            new() { Text = "Tutorial", Language = "en", Confidence = 0.95, X = 50, Y = 50, Type = TextType.Title },
            new() { Text = "Step 1", Language = "en", Confidence = 0.85, X = 100, Y = 400, Type = TextType.OnScreen }
        };
    }

    private VideoEmotionAnalysisDto GenerateSimulatedEmotionAnalysis()
    {
        return new VideoEmotionAnalysisDto
        {
            DominantEmotion = "positive",
            SentimentScore = 0.75,
            SentimentLabel = "positive",
            EmotionScores = new Dictionary<string, double>
            {
                ["happy"] = 0.6,
                ["neutral"] = 0.3,
                ["excited"] = 0.4,
                ["calm"] = 0.2
            },
            EmotionTimeline = new List<EmotionTimelineDto>
            {
                new() { Timestamp = TimeSpan.FromSeconds(0), Emotion = "neutral", Intensity = 0.5 },
                new() { Timestamp = TimeSpan.FromSeconds(30), Emotion = "happy", Intensity = 0.7 },
                new() { Timestamp = TimeSpan.FromSeconds(60), Emotion = "excited", Intensity = 0.8 }
            }
        };
    }

    private VideoSceneAnalysisDto GenerateSimulatedSceneAnalysis()
    {
        return new VideoSceneAnalysisDto
        {
            SceneCount = 3,
            AverageSceneLength = 120,
            SceneComplexity = "medium",
            SceneTypes = new List<string> { "intro", "main", "outro" },
            Scenes = new List<SceneDto>
            {
                new() { SceneNumber = 1, StartTime = TimeSpan.Zero, EndTime = TimeSpan.FromSeconds(30), SceneType = "intro" },
                new() { SceneNumber = 2, StartTime = TimeSpan.FromSeconds(30), EndTime = TimeSpan.FromSeconds(270), SceneType = "main" },
                new() { SceneNumber = 3, StartTime = TimeSpan.FromSeconds(270), EndTime = TimeSpan.FromSeconds(300), SceneType = "outro" }
            }
        };
    }

    private VideoAudioAnalysisDto GenerateSimulatedAudioAnalysis()
    {
        return new VideoAudioAnalysisDto
        {
            HasSpeech = true,
            DetectedLanguage = "en",
            HasMusic = false,
            HasSoundEffects = true,
            AudioQuality = 0.85,
            AverageVolume = 0.7,
            Keywords = new List<string> { "tutorial", "guide", "step", "process" },
            SpeechSegments = new List<SpeechSegmentDto>
            {
                new() { StartTime = TimeSpan.FromSeconds(5), EndTime = TimeSpan.FromSeconds(295), SpeakerId = "speaker_1", Transcription = "Welcome to this tutorial...", Confidence = 0.95 }
            }
        };
    }

    private string GenerateVideoSummary(string title, string? description, List<GeneratedTagDto> tags)
    {
        var keyTags = tags.Take(5).Select(t => t.Tag);
        return $"This video titled '{title}' covers topics including {string.Join(", ", keyTags)}.";
    }

    private double CalculateContentQualityScore(Video video, List<GeneratedTagDto> tags)
    {
        var score = 0.5; // Base score

        // Title quality
        if (video.Title.Length >= 10 && video.Title.Length <= 100)
            score += 0.1;

        // Description quality
        if (!string.IsNullOrEmpty(video.Description) && video.Description.Length > 50)
            score += 0.1;

        // Tag quality
        if (tags.Count >= 5 && tags.Count <= 20)
            score += 0.1;

        // Tag confidence
        var avgConfidence = tags.Any() ? tags.Average(t => t.Confidence) : 0;
        score += avgConfidence * 0.2;

        return Math.Min(1.0, score);
    }

    private double CalculateTagGrowthRate(string tagName, Guid tenantId)
    {
        // Simplified growth rate calculation
        // In production, compare usage over different time periods
        return new Random().NextDouble() * 100; // Random growth between 0-100%
    }

    private double CalculateTagRelevance(Tag tag, Video video)
    {
        var relevance = 0.0;

        // Check if tag matches title
        if (video.Title.Contains(tag.Name, StringComparison.OrdinalIgnoreCase))
            relevance += 0.5;

        // Check if tag matches description
        if (!string.IsNullOrEmpty(video.Description) && 
            video.Description.Contains(tag.Name, StringComparison.OrdinalIgnoreCase))
            relevance += 0.3;

        // Check tag popularity
        var usageCount = _dbContext.VideoTags.CountAsync(vt => vt.TagId == tag.Id).Result;
        relevance += Math.Min(0.2, usageCount * 0.01);

        return Math.Min(1.0, relevance);
    }

    private string GetTagSuggestionReason(Tag tag, Video video)
    {
        if (video.Title.Contains(tag.Name, StringComparison.OrdinalIgnoreCase))
            return "Found in title";
        
        if (!string.IsNullOrEmpty(video.Description) && 
            video.Description.Contains(tag.Name, StringComparison.OrdinalIgnoreCase))
            return "Found in description";
        
        return "Popular tag";
    }
}
