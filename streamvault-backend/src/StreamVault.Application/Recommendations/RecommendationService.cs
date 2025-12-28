using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Services;
using StreamVault.Application.Videos.DTOs;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Recommendations;

public class RecommendationService : IRecommendationService
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly IStorageService _storageService;

    public RecommendationService(StreamVaultDbContext dbContext, IStorageService storageService)
    {
        _dbContext = dbContext;
        _storageService = storageService;
    }

    public async Task<List<VideoListDto>> GetRecommendedVideosAsync(Guid userId, Guid tenantId, int limit = 10)
    {
        // Get user's viewing history
        var viewedVideos = await _dbContext.Videos
            .Where(v => v.TenantId == tenantId && v.ViewCount > 0)
            .Select(v => v.CategoryId)
            .ToListAsync();

        // Get videos from same categories
        var recommendedVideos = await _dbContext.Videos
            .Include(v => v.User)
            .Include(v => v.Category)
            .Where(v => v.TenantId == tenantId 
                    && v.Status == VideoStatus.Processed 
                    && v.IsPublic
                    && viewedVideos.Contains(v.CategoryId))
            .OrderByDescending(v => v.ViewCount)
            .ThenByDescending(v => v.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return await ConvertToVideoListDtos(recommendedVideos);
    }

    public async Task<List<VideoListDto>> GetSimilarVideosAsync(Guid videoId, Guid tenantId, int limit = 10)
    {
        var video = await _dbContext.Videos
            .Include(v => v.Category)
            .Include(v => v.VideoTags)
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            return new List<VideoListDto>();

        var similarVideos = await _dbContext.Videos
            .Include(v => v.User)
            .Include(v => v.Category)
            .Include(v => v.VideoTags)
            .Where(v => v.TenantId == tenantId 
                    && v.Id != videoId
                    && v.Status == VideoStatus.Processed 
                    && v.IsPublic
                    && (v.CategoryId == video.CategoryId || 
                        v.VideoTags.Any(vt => video.VideoTags.Any(pt => pt.Tag == vt.Tag))))
            .OrderByDescending(v => v.ViewCount)
            .Take(limit)
            .ToListAsync();

        return await ConvertToVideoListDtos(similarVideos);
    }

    public async Task<List<VideoListDto>> GetTrendingVideosAsync(Guid tenantId, int limit = 10)
    {
        var sevenDaysAgo = DateTimeOffset.UtcNow.AddDays(-7);
        
        var trendingVideos = await _dbContext.Videos
            .Include(v => v.User)
            .Include(v => v.Category)
            .Where(v => v.TenantId == tenantId 
                    && v.Status == VideoStatus.Processed 
                    && v.IsPublic
                    && v.CreatedAt >= sevenDaysAgo)
            .OrderByDescending(v => v.ViewCount)
            .ThenByDescending(v => v.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return await ConvertToVideoListDtos(trendingVideos);
    }

    public async Task<List<VideoListDto>> GetPopularVideosAsync(Guid tenantId, int limit = 10)
    {
        var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);
        
        var popularVideos = await _dbContext.Videos
            .Include(v => v.User)
            .Include(v => v.Category)
            .Where(v => v.TenantId == tenantId 
                    && v.Status == VideoStatus.Processed 
                    && v.IsPublic
                    && v.CreatedAt >= thirtyDaysAgo)
            .OrderByDescending(v => v.ViewCount)
            .Take(limit)
            .ToListAsync();

        return await ConvertToVideoListDtos(popularVideos);
    }

    public async Task<List<VideoListDto>> GetContinueWatchingAsync(Guid userId, Guid tenantId, int limit = 10)
    {
        // Get videos the user has watched but not completed
        // This is a simplified implementation - in production, you'd track watch progress
        var watchedVideos = await _dbContext.Videos
            .Include(v => v.User)
            .Include(v => v.Category)
            .Where(v => v.TenantId == tenantId 
                    && v.UserId != userId // Not user's own videos
                    && v.Status == VideoStatus.Processed 
                    && v.IsPublic
                    && v.ViewCount > 0)
            .OrderByDescending(v => v.UpdatedAt)
            .Take(limit * 2) // Get more to filter
            .ToListAsync();

        // Return a subset
        var continueWatching = watchedVideos.Take(limit).ToList();
        
        return await ConvertToVideoListDtos(continueWatching);
    }

    private async Task<List<VideoListDto>> ConvertToVideoListDtos(List<Video> videos)
    {
        var videoDtos = new List<VideoListDto>();

        foreach (var video in videos)
        {
            var videoUrl = await _storageService.GeneratePresignedUrlAsync(video.StoragePath, TimeSpan.FromHours(1));
            var thumbnailUrl = video.ThumbnailPath != null 
                ? await _storageService.GeneratePresignedUrlAsync(video.ThumbnailPath, TimeSpan.FromHours(1))
                : null;

            videoDtos.Add(new VideoListDto
            {
                Id = video.Id,
                Title = video.Title,
                Description = video.Description,
                ThumbnailUrl = thumbnailUrl,
                VideoUrl = videoUrl,
                DurationSeconds = video.DurationSeconds,
                ViewCount = video.ViewCount,
                CreatedAt = video.CreatedAt,
                User = new UserDto
                {
                    Id = video.User.Id,
                    Email = video.User.Email,
                    FirstName = video.User.FirstName,
                    LastName = video.User.LastName
                },
                Tags = video.VideoTags.Select(vt => vt.Tag).ToList(),
                Category = video.Category?.Name,
                IsPublic = video.IsPublic
            });
        }

        return videoDtos;
    }
}
