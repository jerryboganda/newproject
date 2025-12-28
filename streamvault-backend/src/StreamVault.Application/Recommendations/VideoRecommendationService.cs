using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Recommendations.DTOs;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;
using System.Collections.Immutable;

namespace StreamVault.Application.Recommendations;

public class VideoRecommendationService : IVideoRecommendationService
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly ILogger<VideoRecommendationService> _logger;

    public VideoRecommendationService(StreamVaultDbContext dbContext, ILogger<VideoRecommendationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<RecommendedVideoDto>> GetRecommendationsForUserAsync(Guid userId, Guid tenantId, int limit = 20, string? algorithm = null)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        // Get user's watch history and preferences
        var watchHistory = await _dbContext.VideoAnalytics
            .Include(va => va.Video)
                .ThenInclude(v => v.User)
            .Include(va => va.Video)
                .ThenInclude(v => v.Category)
            .Where(va => va.UserId == userId)
            .OrderByDescending(va => va.Timestamp)
            .Take(100)
            .ToListAsync();

        // Get user's liked videos
        var likedVideos = await _dbContext.VideoAnalytics
            .Where(va => va.UserId == userId && va.IsLiked)
            .Select(va => va.VideoId)
            .ToListAsync();

        // Get user's preferred categories
        var preferredCategories = watchHistory
            .GroupBy(va => va.Video.Category?.Name)
            .Where(g => g.Key != null)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(5)
            .ToList();

        // Get recommendations based on algorithm
        var recommendations = algorithm?.ToLower() switch
        {
            "collaborative" => await GetCollaborativeRecommendations(userId, tenantId, limit, watchHistory, likedVideos),
            "content" => await GetContentBasedRecommendations(userId, tenantId, limit, watchHistory, likedVideos),
            "trending" => await GetTrendingRecommendations(userId, tenantId, limit),
            _ => await GetHybridRecommendations(userId, tenantId, limit, watchHistory, likedVideos, preferredCategories)
        };

        return recommendations;
    }

    public async Task<List<RecommendedVideoDto>> GetSimilarVideosAsync(Guid videoId, Guid userId, Guid tenantId, int limit = 10)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        // Get the source video
        var sourceVideo = await _dbContext.Videos
            .Include(v => v.Category)
            .Include(v => v.VideoTags)
                .ThenInclude(vt => vt.Tag)
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.Id == videoId);

        if (sourceVideo == null)
            throw new Exception("Video not found");

        // Find similar videos based on:
        // 1. Same category
        // 2. Similar tags
        // 3. Same creator
        // 4. Similar duration
        // 5. Similar title/description keywords

        var sourceTags = sourceVideo.VideoTags.Select(vt => vt.Tag.Name).ToHashSet();
        var sourceCategory = sourceVideo.Category?.Name;
        var sourceCreatorId = sourceVideo.UserId;

        var similarVideos = await _dbContext.Videos
            .Include(v => v.Category)
            .Include(v => v.VideoTags)
                .ThenInclude(vt => vt.Tag)
            .Include(v => v.User)
            .Where(v => v.Id != videoId && 
                        v.IsPublic && 
                        v.Status == VideoStatus.Processed &&
                        v.TenantId == tenantId &&
                        (v.CategoryId == sourceVideo.CategoryId ||
                         v.UserId == sourceCreatorId ||
                         v.VideoTags.Any(vt => sourceTags.Contains(vt.Tag.Name))))
            .ToListAsync();

        // Calculate similarity scores
        var scoredVideos = similarVideos.Select(v =>
        {
            double score = 0;
            var reasons = new List<string>();

            // Category match (30% weight)
            if (v.CategoryId == sourceVideo.CategoryId)
            {
                score += 0.3;
                reasons.Add("Same category");
            }

            // Creator match (25% weight)
            if (v.UserId == sourceCreatorId)
            {
                score += 0.25;
                reasons.Add("Same creator");
            }

            // Tag similarity (30% weight)
            var videoTags = v.VideoTags.Select(vt => vt.Tag.Name).ToHashSet();
            var commonTags = sourceTags.Intersect(videoTags).Count();
            if (commonTags > 0)
            {
                var tagScore = (double)commonTags / Math.Max(sourceTags.Count, videoTags.Count) * 0.3;
                score += tagScore;
                reasons.Add($"{commonTags} common tags");
            }

            // Duration similarity (15% weight)
            var durationDiff = Math.Abs(v.DurationSeconds - sourceVideo.DurationSeconds);
            var durationSimilarity = 1 - (durationDiff / (double)Math.Max(v.DurationSeconds, sourceVideo.DurationSeconds));
            if (durationSimilarity > 0.8)
            {
                score += durationSimilarity * 0.15;
                reasons.Add("Similar duration");
            }

            return new { Video = v, Score = score, Reasons = reasons };
        })
        .OrderByDescending(x => x.Score)
        .Take(limit)
        .ToList();

        // Convert to DTOs
        var result = new List<RecommendedVideoDto>();
        foreach (var item in scoredVideos)
        {
            var video = item.Video;
            var watchHistory = await _dbContext.VideoAnalytics
                .FirstOrDefaultAsync(va => va.UserId == userId && va.VideoId == video.Id);

            result.Add(new RecommendedVideoDto
            {
                VideoId = video.Id,
                Title = video.Title,
                Description = video.Description ?? "",
                ThumbnailUrl = video.ThumbnailPath ?? "",
                VideoUrl = video.VideoUrl,
                DurationSeconds = video.DurationSeconds,
                ViewCount = video.ViewCount,
                PublishedAt = video.PublishedAt ?? video.CreatedAt,
                CategoryName = video.Category?.Name ?? "",
                Tags = video.VideoTags.Select(vt => vt.Tag.Name).ToList(),
                Creator = new UserDto
                {
                    Id = video.User.Id,
                    Email = video.User.Email,
                    FirstName = video.User.FirstName,
                    LastName = video.User.LastName,
                    AvatarUrl = video.User.AvatarUrl
                },
                Score = item.Score,
                Reason = string.Join(", ", item.Reasons.Take(2)),
                RecommendationReasons = item.Reasons,
                IsWatched = watchHistory != null,
                WatchProgress = watchHistory?.WatchPercentage ?? 0,
                IsLiked = watchHistory?.IsLiked ?? false,
                IsDisliked = watchHistory?.IsDisliked ?? false,
                IsInWatchLater = false, // TODO: Implement watch later feature
                LastWatchedAt = watchHistory?.LastWatchedAt,
                WatchTimeSeconds = watchHistory?.WatchedSeconds ?? 0,
                EngagementScore = CalculateEngagementScore(video),
                TrendingScore = await CalculateTrendingScore(video),
                PersonalizationScore = item.Score
            });
        }

        return result;
    }

    public async Task<List<RecommendedVideoDto>> GetTrendingVideosAsync(Guid tenantId, int limit = 20, string? category = null)
    {
        var query = _dbContext.Videos
            .Include(v => v.Category)
            .Include(v => v.VideoTags)
                .ThenInclude(vt => vt.Tag)
            .Include(v => v.User)
            .Where(v => v.IsPublic && 
                        v.Status == VideoStatus.Processed &&
                        v.TenantId == tenantId &&
                        v.PublishedAt.HasValue &&
                        v.PublishedAt >= DateTimeOffset.UtcNow.AddDays(-7));

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(v => v.Category != null && v.Category.Name == category);
        }

        var videos = await query.ToListAsync();

        // Calculate trending scores based on:
        // 1. Recent view velocity
        // 2. Engagement rate
        // 3. Growth rate
        // 4. Freshness

        var trendingVideos = videos.Select(v =>
        {
            var score = CalculateTrendingScore(v).Result;
            return new { Video = v, Score = score };
        })
        .OrderByDescending(x => x.Score)
        .Take(limit)
        .ToList();

        return trendingVideos.Select(v => MapToRecommendedVideo(v.Video, v.Score, "Trending")).ToList();
    }

    public async Task<List<RecommendedVideoDto>> GetPersonalizedFeedAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20)
    {
        // Get mix of different recommendation types
        var tasks = new[]
        {
            GetRecommendationsForUserAsync(userId, tenantId, pageSize / 3, "collaborative"),
            GetTrendingVideosAsync(tenantId, pageSize / 3),
            GetContinueWatchingAsync(userId, tenantId, pageSize / 3)
        };

        var results = await Task.WhenAll(tasks);
        var combined = results.SelectMany(r => r).ToList();

        // Shuffle and paginate
        var random = new Random();
        var shuffled = combined.OrderBy(x => random.Next()).ToList();

        return shuffled.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    }

    public async Task<List<RecommendedVideoDto>> GetContinueWatchingAsync(Guid userId, Guid tenantId, int limit = 10)
    {
        // Get partially watched videos
        var watchHistory = await _dbContext.VideoAnalytics
            .Include(va => va.Video)
                .ThenInclude(v => v.Category)
            .Include(va => va.Video)
                .ThenInclude(v => v.User)
            .Include(va => va.Video)
                .ThenInclude(v => v.VideoTags)
                    .ThenInclude(vt => vt.Tag)
            .Where(va => va.UserId == userId && 
                        va.WatchPercentage > 0 && 
                        va.WatchPercentage < 90 &&
                        va.Video.IsPublic &&
                        va.Video.Status == VideoStatus.Processed &&
                        va.Video.TenantId == tenantId)
            .OrderByDescending(va => va.LastWatchedAt)
            .Take(limit * 2) // Get more to filter
            .ToListAsync();

        return watchHistory.Take(limit).Select(va => new RecommendedVideoDto
        {
            VideoId = va.Video.Id,
            Title = va.Video.Title,
            Description = va.Video.Description ?? "",
            ThumbnailUrl = va.Video.ThumbnailPath ?? "",
            VideoUrl = va.Video.VideoUrl,
            DurationSeconds = va.Video.DurationSeconds,
            ViewCount = va.Video.ViewCount,
            PublishedAt = va.Video.PublishedAt ?? va.Video.CreatedAt,
            CategoryName = va.Video.Category?.Name ?? "",
            Tags = va.Video.VideoTags.Select(vt => vt.Tag.Name).ToList(),
            Creator = new UserDto
            {
                Id = va.Video.User.Id,
                Email = va.Video.User.Email,
                FirstName = va.Video.User.FirstName,
                LastName = va.Video.User.LastName,
                AvatarUrl = va.Video.User.AvatarUrl
            },
            Score = 0.9, // High score for continue watching
            Reason = "Continue watching",
            RecommendationReasons = new List<string> { "You watched this recently" },
            IsWatched = true,
            WatchProgress = va.WatchPercentage,
            IsLiked = va.IsLiked,
            IsDisliked = va.IsDisliked,
            IsInWatchLater = false,
            LastWatchedAt = va.LastWatchedAt,
            WatchTimeSeconds = va.WatchedSeconds,
            EngagementScore = CalculateEngagementScore(va.Video),
            TrendingScore = CalculateTrendingScore(va.Video).Result,
            PersonalizationScore = 0.9
        }).ToList();
    }

    public async Task<List<RecommendedVideoDto>> GetWatchAgainAsync(Guid userId, Guid tenantId, int limit = 10)
    {
        // Get fully watched videos that were liked
        var watchHistory = await _dbContext.VideoAnalytics
            .Include(va => va.Video)
                .ThenInclude(v => v.Category)
            .Include(va => va.Video)
                .ThenInclude(v => v.User)
            .Include(va => va.Video)
                .ThenInclude(v => v.VideoTags)
                    .ThenInclude(vt => vt.Tag)
            .Where(va => va.UserId == userId && 
                        va.WatchPercentage >= 90 &&
                        (va.IsLiked || va.WatchSessions > 1) &&
                        va.Video.IsPublic &&
                        va.Video.Status == VideoStatus.Processed &&
                        va.Video.TenantId == tenantId)
            .OrderByDescending(va => va.LastWatchedAt)
            .Take(limit)
            .ToListAsync();

        return watchHistory.Select(va => new RecommendedVideoDto
        {
            VideoId = va.Video.Id,
            Title = va.Video.Title,
            Description = va.Video.Description ?? "",
            ThumbnailUrl = va.Video.ThumbnailPath ?? "",
            VideoUrl = va.Video.VideoUrl,
            DurationSeconds = va.Video.DurationSeconds,
            ViewCount = va.Video.ViewCount,
            PublishedAt = va.Video.PublishedAt ?? va.Video.CreatedAt,
            CategoryName = va.Video.Category?.Name ?? "",
            Tags = va.Video.VideoTags.Select(vt => vt.Tag.Name).ToList(),
            Creator = new UserDto
            {
                Id = va.Video.User.Id,
                Email = va.Video.User.Email,
                FirstName = va.Video.User.FirstName,
                LastName = va.Video.User.LastName,
                AvatarUrl = va.Video.User.AvatarUrl
            },
            Score = 0.8,
            Reason = "Watch again",
            RecommendationReasons = new List<string> { va.IsLiked ? "You liked this video" : "You watched this multiple times" },
            IsWatched = true,
            WatchProgress = 100,
            IsLiked = va.IsLiked,
            IsDisliked = va.IsDisliked,
            IsInWatchLater = false,
            LastWatchedAt = va.LastWatchedAt,
            WatchTimeSeconds = va.WatchedSeconds,
            EngagementScore = CalculateEngagementScore(va.Video),
            TrendingScore = CalculateTrendingScore(va.Video).Result,
            PersonalizationScore = 0.8
        }).ToList();
    }

    public async Task<List<RecommendedVideoDto>> GetRecommendedForYouAsync(Guid userId, Guid tenantId, int limit = 20)
    {
        return await GetRecommendationsForUserAsync(userId, tenantId, limit, "hybrid");
    }

    public async Task<List<RecommendedVideoDto>> GetPopularInCategoryAsync(Guid categoryId, Guid userId, Guid tenantId, int limit = 20)
    {
        var videos = await _dbContext.Videos
            .Include(v => v.Category)
            .Include(v => v.VideoTags)
                .ThenInclude(vt => vt.Tag)
            .Include(v => v.User)
            .Where(v => v.CategoryId == categoryId && 
                        v.IsPublic && 
                        v.Status == VideoStatus.Processed &&
                        v.TenantId == tenantId)
            .OrderByDescending(v => v.ViewCount)
            .Take(limit)
            .ToListAsync();

        return videos.Select(v => MapToRecommendedVideo(v, 0.7, "Popular in category")).ToList();
    }

    public async Task<List<RecommendedVideoDto>> GetRecommendedFromCreatorsAsync(List<Guid> creatorIds, Guid userId, Guid tenantId, int limit = 20)
    {
        var videos = await _dbContext.Videos
            .Include(v => v.Category)
            .Include(v => v.VideoTags)
                .ThenInclude(vt => vt.Tag)
            .Include(v => v.User)
            .Where(v => creatorIds.Contains(v.UserId) && 
                        v.IsPublic && 
                        v.Status == VideoStatus.Processed &&
                        v.TenantId == tenantId)
            .OrderByDescending(v => v.PublishedAt)
            .Take(limit)
            .ToListAsync();

        return videos.Select(v => MapToRecommendedVideo(v, 0.75, "From creators you follow")).ToList();
    }

    public async Task<bool> UpdateWatchHistoryAsync(Guid userId, Guid videoId, Guid tenantId, int watchedSeconds, int totalSeconds)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        // Get or create watch history
        var watchHistory = await _dbContext.VideoAnalytics
            .FirstOrDefaultAsync(va => va.UserId == userId && va.VideoId == videoId);

        if (watchHistory == null)
        {
            watchHistory = new VideoAnalytic
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                VideoId = videoId,
                WatchedSeconds = watchedSeconds,
                TotalSeconds = totalSeconds,
                WatchPercentage = (double)watchedSeconds / totalSeconds * 100,
                StartedAt = DateTimeOffset.UtcNow,
                LastWatchedAt = DateTimeOffset.UtcNow,
                IsCompleted = watchedSeconds >= totalSeconds,
                WatchSessions = 1
            };
            _dbContext.VideoAnalytics.Add(watchHistory);
        }
        else
        {
            watchHistory.WatchedSeconds = Math.Max(watchHistory.WatchedSeconds, watchedSeconds);
            watchHistory.TotalSeconds = totalSeconds;
            watchHistory.WatchPercentage = (double)watchHistory.WatchedSeconds / totalSeconds * 100;
            watchHistory.LastWatchedAt = DateTimeOffset.UtcNow;
            watchHistory.IsCompleted = watchedSeconds >= totalSeconds;
            watchHistory.WatchSessions += 1;
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RecordUserInteractionAsync(Guid userId, Guid videoId, Guid tenantId, string interactionType, Dictionary<string, object>? metadata = null)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        // Update video analytics based on interaction
        var analytics = await _dbContext.VideoAnalytics
            .FirstOrDefaultAsync(va => va.UserId == userId && va.VideoId == videoId);

        if (analytics != null)
        {
            switch (interactionType.ToLower())
            {
                case "like":
                    analytics.IsLiked = true;
                    analytics.IsDisliked = false;
                    break;
                case "dislike":
                    analytics.IsLiked = false;
                    analytics.IsDisliked = true;
                    break;
                case "share":
                    analytics.IsShared = true;
                    break;
                case "comment":
                    analytics.HasCommented = true;
                    break;
            }
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<List<RecommendedVideoDto>> GetSearchRecommendationsAsync(Guid userId, Guid tenantId, string query, int limit = 10)
    {
        // Get videos matching the search query
        var videos = await _dbContext.Videos
            .Include(v => v.Category)
            .Include(v => v.VideoTags)
                .ThenInclude(vt => vt.Tag)
            .Include(v => v.User)
            .Where(v => v.IsPublic && 
                        v.Status == VideoStatus.Processed &&
                        v.TenantId == tenantId &&
                        (v.Title.Contains(query) || 
                         (v.Description != null && v.Description.Contains(query))))
            .OrderByDescending(v => v.ViewCount)
            .Take(limit)
            .ToListAsync();

        return videos.Select(v => MapToRecommendedVideo(v, 0.6, $"Search result for '{query}'")).ToList();
    }

    private async Task<List<RecommendedVideoDto>> GetCollaborativeRecommendations(Guid userId, Guid tenantId, int limit, List<VideoAnalytic> watchHistory, List<Guid> likedVideos)
    {
        // Find users with similar watch history
        var similarUsers = await _dbContext.VideoAnalytics
            .Where(va => va.UserId != userId && 
                        likedVideos.Contains(va.VideoId))
            .GroupBy(va => va.UserId)
            .Where(g => g.Count() >= 3) // At least 3 common liked videos
            .Select(g => new { UserId = g.Key, CommonVideos = g.Count() })
            .OrderByDescending(g => g.CommonVideos)
            .Take(10)
            .ToListAsync();

        var similarUserIds = similarUsers.Select(u => u.UserId).ToList();

        // Get videos watched by similar users that current user hasn't watched
        var recommendations = await _dbContext.VideoAnalytics
            .Include(va => va.Video)
                .ThenInclude(v => v.Category)
            .Include(va => va.Video)
                .ThenInclude(v => v.User)
            .Include(va => va.Video)
                .ThenInclude(v => v.VideoTags)
                    .ThenInclude(vt => vt.Tag)
            .Where(va => similarUserIds.Contains(va.UserId) &&
                        !watchHistory.Any(wh => wh.VideoId == va.VideoId) &&
                        va.Video.IsPublic &&
                        va.Video.Status == VideoStatus.Processed &&
                        va.Video.TenantId == tenantId)
            .GroupBy(va => va.VideoId)
            .Select(g => new { Video = g.First().Video, Score = g.Count() })
            .OrderByDescending(g => g.Score)
            .Take(limit)
            .ToListAsync();

        return recommendations.Select(r => MapToRecommendedVideo(r.Video, r.Score * 0.1, "Users like you also watched")).ToList();
    }

    private async Task<List<RecommendedVideoDto>> GetContentBasedRecommendations(Guid userId, Guid tenantId, int limit, List<VideoAnalytic> watchHistory, List<Guid> likedVideos)
    {
        // Get user's preferred tags and categories
        var preferredTags = watchHistory
            .SelectMany(va => va.Video.VideoTags)
            .GroupBy(vt => vt.Tag.Name)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => g.Key)
            .ToList();

        var preferredCategories = watchHistory
            .Where(va => va.Video.Category != null)
            .GroupBy(va => va.Video.Category!.Name)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToList();

        // Find videos with preferred tags/categories
        var recommendations = await _dbContext.Videos
            .Include(v => v.Category)
            .Include(v => v.VideoTags)
                .ThenInclude(vt => vt.Tag)
            .Include(v => v.User)
            .Where(v => v.IsPublic && 
                        v.Status == VideoStatus.Processed &&
                        v.TenantId == tenantId &&
                        !watchHistory.Any(wh => wh.VideoId == v.Id) &&
                        (v.VideoTags.Any(vt => preferredTags.Contains(vt.Tag.Name)) ||
                         (v.Category != null && preferredCategories.Contains(v.Category.Name))))
            .OrderByDescending(v => v.ViewCount)
            .Take(limit)
            .ToListAsync();

        return recommendations.Select(v => MapToRecommendedVideo(v, 0.7, "Based on your interests")).ToList();
    }

    private async Task<List<RecommendedVideoDto>> GetTrendingRecommendations(Guid userId, Guid tenantId, int limit)
    {
        return await GetTrendingVideosAsync(tenantId, limit);
    }

    private async Task<List<RecommendedVideoDto>> GetHybridRecommendations(Guid userId, Guid tenantId, int limit, List<VideoAnalytic> watchHistory, List<Guid> likedVideos, List<dynamic> preferredCategories)
    {
        // Combine multiple recommendation strategies
        var collaborative = await GetCollaborativeRecommendations(userId, tenantId, limit / 3, watchHistory, likedVideos);
        var contentBased = await GetContentBasedRecommendations(userId, tenantId, limit / 3, watchHistory, likedVideos);
        var trending = await GetTrendingRecommendations(userId, tenantId, limit / 3);

        // Merge and deduplicate
        var allRecommendations = collaborative
            .Concat(contentBased)
            .Concat(trending)
            .GroupBy(r => r.VideoId)
            .Select(g => g.First())
            .OrderByDescending(r => r.Score)
            .Take(limit)
            .ToList();

        return allRecommendations;
    }

    private RecommendedVideoDto MapToRecommendedVideo(Video video, double score, string reason)
    {
        return new RecommendedVideoDto
        {
            VideoId = video.Id,
            Title = video.Title,
            Description = video.Description ?? "",
            ThumbnailUrl = video.ThumbnailPath ?? "",
            VideoUrl = video.VideoUrl,
            DurationSeconds = video.DurationSeconds,
            ViewCount = video.ViewCount,
            PublishedAt = video.PublishedAt ?? video.CreatedAt,
            CategoryName = video.Category?.Name ?? "",
            Tags = video.VideoTags.Select(vt => vt.Tag.Name).ToList(),
            Creator = new UserDto
            {
                Id = video.User.Id,
                Email = video.User.Email,
                FirstName = video.User.FirstName,
                LastName = video.User.LastName,
                AvatarUrl = video.User.AvatarUrl
            },
            Score = score,
            Reason = reason,
            RecommendationReasons = new List<string> { reason },
            IsWatched = false,
            WatchProgress = 0,
            IsLiked = false,
            IsDisliked = false,
            IsInWatchLater = false,
            EngagementScore = CalculateEngagementScore(video),
            TrendingScore = CalculateTrendingScore(video).Result,
            PersonalizationScore = score
        };
    }

    private double CalculateEngagementScore(Video video)
    {
        // Simple engagement score calculation
        // In production, this would be more sophisticated
        var likeCount = _dbContext.VideoAnalytics.Count(va => va.VideoId == video.Id && va.IsLiked);
        var commentCount = _dbContext.VideoAnalytics.Count(va => va.VideoId == video.Id && va.HasCommented);
        var shareCount = _dbContext.VideoAnalytics.Count(va => va.VideoId == video.Id && va.IsShared);

        var totalEngagements = likeCount + commentCount + shareCount;
        return video.ViewCount > 0 ? (double)totalEngagements / video.ViewCount : 0;
    }

    private async Task<double> CalculateTrendingScore(Video video)
    {
        // Calculate trending score based on recent views and engagement
        var recentViews = await _dbContext.VideoAnalytics
            .CountAsync(va => va.VideoId == video.Id && va.Timestamp >= DateTimeOffset.UtcNow.AddDays(-7));

        var totalViews = video.ViewCount;
        var ageInDays = (DateTimeOffset.UtcNow - (video.PublishedAt ?? video.CreatedAt)).TotalDays;

        // Trending score considers velocity, freshness, and total popularity
        var velocityScore = ageInDays > 0 ? recentViews / ageInDays : 0;
        var freshnessScore = Math.Max(0, 1 - (ageInDays / 30)); // Decay over 30 days
        var popularityScore = Math.Log10(Math.Max(1, totalViews));

        return (velocityScore * 0.4) + (freshnessScore * 0.3) + (popularityScore * 0.3);
    }
}
