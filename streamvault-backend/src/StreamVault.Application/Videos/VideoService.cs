using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Application.Services;
using StreamVault.Application.Videos.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;
using System.Security.Cryptography;

namespace StreamVault.Application.Videos;

public class VideoService : IVideoService
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly IStorageService _storageService;
    private readonly IBackgroundJobService _backgroundJobService;

    public VideoService(
        StreamVaultDbContext dbContext, 
        IStorageService storageService,
        IBackgroundJobService backgroundJobService)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _backgroundJobService = backgroundJobService;
    }

    public async Task<VideoUploadResponse> InitiateUploadAsync(VideoUploadRequest request, Guid userId, Guid tenantId)
    {
        var videoId = Guid.NewGuid();
        var uploadToken = GenerateUploadToken();
        var fileKey = GenerateFileKey(videoId, request.Title);

        var video = new Video
        {
            Id = videoId,
            Title = request.Title,
            Description = request.Description,
            UserId = userId,
            TenantId = tenantId,
            OriginalFileName = request.Title,
            StoragePath = fileKey,
            Status = VideoStatus.Uploading,
            IsPublic = request.IsPublic
        };

        _dbContext.Videos.Add(video);

        // Add tags if provided
        if (request.Tags?.Any() == true)
        {
            foreach (var tag in request.Tags)
            {
                video.VideoTags.Add(new VideoTag { Tag = tag });
            }
        }

        await _dbContext.SaveChangesAsync();

        // Generate upload URL for the first chunk
        var uploadUrl = await _storageService.GenerateUploadUrlAsync($"{fileKey}.part0", "video/*", 0);

        return new VideoUploadResponse
        {
            UploadUrl = uploadUrl,
            VideoId = videoId.ToString(),
            UploadToken = uploadToken
        };
    }

    public async Task<ChunkUploadResponse> GetChunkUploadUrlAsync(ChunkUploadRequest request)
    {
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id.ToString() == request.VideoId && v.Status == VideoStatus.Uploading);

        if (video == null)
            throw new Exception("Video not found or not in uploading state");

        // TODO: Validate upload token
        var chunkKey = $"{video.StoragePath}.part{request.ChunkIndex}";
        var uploadUrl = await _storageService.GenerateUploadUrlAsync(chunkKey, "video/*", 0);

        return new ChunkUploadResponse
        {
            UploadUrl = uploadUrl,
            IsComplete = request.ChunkIndex == request.TotalChunks - 1
        };
    }

    public async Task CompleteUploadAsync(string videoId, string uploadToken)
    {
        var video = await _dbContext.Videos
            .Include(v => v.VideoTags)
            .FirstOrDefaultAsync(v => v.Id.ToString() == videoId);

        if (video == null)
            throw new Exception("Video not found");

        // TODO: Validate upload token
        // TODO: Merge chunks in S3
        // TODO: Get file size and duration
        // TODO: Generate thumbnail

        video.Status = VideoStatus.Uploaded;
        video.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        // Queue video processing jobs
        await _backgroundJobService.EnqueueThumbnailGenerationAsync(video.Id);
        await _backgroundJobService.EnqueueVideoTranscodingAsync(video.Id);
        await _backgroundJobService.EnqueueVideoAnalysisAsync(video.Id);
    }

    public async Task<VideoDto?> GetVideoAsync(Guid id, Guid tenantId)
    {
        var video = await _dbContext.Videos
            .Include(v => v.User)
            .Include(v => v.VideoTags)
            .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId);

        if (video == null)
            return null;

        var videoUrl = await _storageService.GeneratePresignedUrlAsync(video.StoragePath, TimeSpan.FromHours(1));
        var thumbnailUrl = video.ThumbnailPath != null 
            ? await _storageService.GeneratePresignedUrlAsync(video.ThumbnailPath, TimeSpan.FromHours(1))
            : null;

        return new VideoDto
        {
            Id = video.Id,
            Title = video.Title,
            Description = video.Description,
            OriginalFileName = video.OriginalFileName,
            ThumbnailUrl = thumbnailUrl,
            VideoUrl = videoUrl,
            FileSizeBytes = video.FileSizeBytes,
            MimeType = video.MimeType,
            DurationSeconds = video.DurationSeconds,
            Status = video.Status,
            IsPublic = video.IsPublic,
            ViewCount = video.ViewCount,
            CreatedAt = video.CreatedAt,
            PublishedAt = video.PublishedAt,
            User = new UserDto
            {
                Id = video.User.Id,
                Email = video.User.Email,
                FirstName = video.User.FirstName,
                LastName = video.User.LastName
            },
            Tags = video.VideoTags.Select(vt => vt.Tag).ToList()
        };
    }

    public async Task<VideoListResponse> GetVideosAsync(VideoListRequest request, Guid tenantId)
    {
        var query = _dbContext.Videos
            .Include(v => v.User)
            .Include(v => v.VideoTags)
            .Where(v => v.TenantId == tenantId && v.Status != VideoStatus.Deleted);

        // Apply filters
        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(v => v.Title.Contains(request.Search) || 
                                    (v.Description != null && v.Description.Contains(request.Search)));
        }

        if (!string.IsNullOrEmpty(request.Tag))
        {
            query = query.Where(v => v.VideoTags.Any(vt => vt.Tag == request.Tag));
        }

        if (request.Tags?.Any() == true)
        {
            query = query.Where(v => v.VideoTags.Any(vt => request.Tags.Contains(vt.Tag)));
        }

        if (request.IsPublic.HasValue)
        {
            query = query.Where(v => v.IsPublic == request.IsPublic.Value);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(v => v.UserId == request.UserId.Value);
        }

        if (request.MinDuration.HasValue)
        {
            query = query.Where(v => v.DurationSeconds >= request.MinDuration.Value);
        }

        if (request.MaxDuration.HasValue)
        {
            query = query.Where(v => v.DurationSeconds <= request.MaxDuration.Value);
        }

        if (request.UploadedAfter.HasValue)
        {
            query = query.Where(v => v.CreatedAt >= request.UploadedAfter.Value);
        }

        if (request.UploadedBefore.HasValue)
        {
            query = query.Where(v => v.CreatedAt <= request.UploadedBefore.Value);
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "title" => request.SortOrder?.ToLower() == "asc" 
                ? query.OrderBy(v => v.Title) 
                : query.OrderByDescending(v => v.Title),
            "views" => request.SortOrder?.ToLower() == "asc" 
                ? query.OrderBy(v => v.ViewCount) 
                : query.OrderByDescending(v => v.ViewCount),
            "duration" => request.SortOrder?.ToLower() == "asc" 
                ? query.OrderBy(v => v.DurationSeconds) 
                : query.OrderByDescending(v => v.DurationSeconds),
            _ => request.SortOrder?.ToLower() == "asc" 
                ? query.OrderBy(v => v.CreatedAt) 
                : query.OrderByDescending(v => v.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var videos = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var videoDtos = new List<VideoDto>();
        foreach (var video in videos)
        {
            var videoUrl = await _storageService.GeneratePresignedUrlAsync(video.StoragePath, TimeSpan.FromHours(1));
            var thumbnailUrl = video.ThumbnailPath != null 
                ? await _storageService.GeneratePresignedUrlAsync(video.ThumbnailPath, TimeSpan.FromHours(1))
                : null;

            videoDtos.Add(new VideoDto
            {
                Id = video.Id,
                Title = video.Title,
                Description = video.Description,
                OriginalFileName = video.OriginalFileName,
                ThumbnailUrl = thumbnailUrl,
                VideoUrl = videoUrl,
                FileSizeBytes = video.FileSizeBytes,
                MimeType = video.MimeType,
                DurationSeconds = video.DurationSeconds,
                Status = video.Status,
                IsPublic = video.IsPublic,
                ViewCount = video.ViewCount,
                CreatedAt = video.CreatedAt,
                PublishedAt = video.PublishedAt,
                User = new UserDto
                {
                    Id = video.User.Id,
                    Email = video.User.Email,
                    FirstName = video.User.FirstName,
                    LastName = video.User.LastName
                },
                Tags = video.VideoTags.Select(vt => vt.Tag).ToList()
            });
        }

        return new VideoListResponse
        {
            Videos = videoDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
    }

    public async Task<VideoDto> UpdateVideoAsync(Guid id, VideoUpdateRequest request, Guid userId, Guid tenantId)
    {
        var video = await _dbContext.Videos
            .Include(v => v.VideoTags)
            .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId && v.UserId == userId);

        if (video == null)
            throw new Exception("Video not found");

        if (!string.IsNullOrEmpty(request.Title))
            video.Title = request.Title;

        if (request.Description != null)
            video.Description = request.Description;

        if (request.IsPublic.HasValue)
            video.IsPublic = request.IsPublic.Value;

        // Update tags
        if (request.Tags != null)
        {
            video.VideoTags.Clear();
            foreach (var tag in request.Tags)
            {
                video.VideoTags.Add(new VideoTag { Tag = tag });
            }
        }

        video.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return await GetVideoAsync(id, tenantId) ?? throw new Exception("Failed to retrieve updated video");
    }

    public async Task DeleteVideoAsync(Guid id, Guid userId, Guid tenantId)
    {
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId && v.UserId == userId);

        if (video == null)
            throw new Exception("Video not found");

        video.Status = VideoStatus.Deleted;
        video.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        // TODO: Delete files from storage
    }

    public async Task IncrementViewCountAsync(Guid id, Guid tenantId)
    {
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId && v.IsPublic);

        if (video != null)
        {
            video.ViewCount++;
            await _dbContext.SaveChangesAsync();
        }
    }

    private string GenerateUploadToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    private string GenerateFileKey(Guid videoId, string title)
    {
        var sanitizedTitle = string.Join("", title.Split(Path.GetInvalidFileNameChars()));
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return $"videos/{videoId}/{sanitizedTitle}_{timestamp}";
    }
}
