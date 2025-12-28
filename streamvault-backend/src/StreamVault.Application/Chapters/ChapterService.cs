using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Chapters.DTOs;
using StreamVault.Application.Services;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Chapters;

public class ChapterService : IChapterService
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly IStorageService _storageService;

    public ChapterService(StreamVaultDbContext dbContext, IStorageService storageService)
    {
        _dbContext = dbContext;
        _storageService = storageService;
    }

    public async Task<List<ChapterDto>> GetChaptersAsync(Guid videoId, Guid tenantId)
    {
        // Verify video exists and belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var chapters = await _dbContext.VideoChapters
            .Where(vc => vc.VideoId == videoId)
            .OrderBy(vc => vc.SortOrder)
            .ThenBy(vc => vc.StartTimeSeconds)
            .ToListAsync();

        var chapterDtos = new List<ChapterDto>();
        foreach (var chapter in chapters)
        {
            var thumbnailUrl = chapter.ThumbnailPath != null 
                ? await _storageService.GeneratePresignedUrlAsync(chapter.ThumbnailPath, TimeSpan.FromHours(1))
                : null;

            chapterDtos.Add(new ChapterDto
            {
                Id = chapter.Id,
                VideoId = chapter.VideoId,
                StartTimeSeconds = chapter.StartTimeSeconds,
                EndTimeSeconds = chapter.EndTimeSeconds,
                Title = chapter.Title,
                Description = chapter.Description,
                ThumbnailUrl = thumbnailUrl,
                SortOrder = chapter.SortOrder,
                CreatedAt = chapter.CreatedAt
            });
        }

        return chapterDtos;
    }

    public async Task<ChapterDto?> GetChapterAsync(Guid chapterId, Guid videoId, Guid tenantId)
    {
        // Verify video exists and belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var chapter = await _dbContext.VideoChapters
            .FirstOrDefaultAsync(vc => vc.Id == chapterId && vc.VideoId == videoId);

        if (chapter == null)
            return null;

        var thumbnailUrl = chapter.ThumbnailPath != null 
            ? await _storageService.GeneratePresignedUrlAsync(chapter.ThumbnailPath, TimeSpan.FromHours(1))
            : null;

        return new ChapterDto
        {
            Id = chapter.Id,
            VideoId = chapter.VideoId,
            StartTimeSeconds = chapter.StartTimeSeconds,
            EndTimeSeconds = chapter.EndTimeSeconds,
            Title = chapter.Title,
            Description = chapter.Description,
            ThumbnailUrl = thumbnailUrl,
            SortOrder = chapter.SortOrder,
            CreatedAt = chapter.CreatedAt
        };
    }

    public async Task<ChapterDto> CreateChapterAsync(CreateChapterRequest request, Guid userId, Guid tenantId)
    {
        // Verify video exists and belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == request.VideoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        // Validate time range
        if (request.StartTimeSeconds < 0 || request.EndTimeSeconds <= request.StartTimeSeconds)
            throw new Exception("Invalid time range");

        // Check for overlapping chapters
        var overlappingChapter = await _dbContext.VideoChapters
            .AnyAsync(vc => vc.VideoId == request.VideoId &&
                           ((vc.StartTimeSeconds <= request.StartTimeSeconds && vc.EndTimeSeconds > request.StartTimeSeconds) ||
                            (vc.StartTimeSeconds < request.EndTimeSeconds && vc.EndTimeSeconds >= request.EndTimeSeconds)));

        if (overlappingChapter)
            throw new Exception("Chapter time range overlaps with existing chapter");

        var chapter = new VideoChapter
        {
            Id = Guid.NewGuid(),
            VideoId = request.VideoId,
            StartTimeSeconds = request.StartTimeSeconds,
            EndTimeSeconds = request.EndTimeSeconds,
            Title = request.Title,
            Description = request.Description,
            ThumbnailPath = request.Thumbnail,
            SortOrder = request.SortOrder,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.VideoChapters.Add(chapter);
        await _dbContext.SaveChangesAsync();

        return await GetChapterAsync(chapter.Id, request.VideoId, tenantId);
    }

    public async Task<ChapterDto> UpdateChapterAsync(Guid chapterId, UpdateChapterRequest request, Guid userId, Guid tenantId)
    {
        var chapter = await _dbContext.VideoChapters
            .Include(vc => vc.Video)
            .FirstOrDefaultAsync(vc => vc.Id == chapterId);

        if (chapter == null || chapter.Video.TenantId != tenantId)
            throw new Exception("Chapter not found");

        if (request.StartTimeSeconds.HasValue)
            chapter.StartTimeSeconds = request.StartTimeSeconds.Value;

        if (request.EndTimeSeconds.HasValue)
            chapter.EndTimeSeconds = request.EndTimeSeconds.Value;

        if (request.Title != null)
            chapter.Title = request.Title;

        if (request.Description != null)
            chapter.Description = request.Description;

        if (request.Thumbnail != null)
            chapter.ThumbnailPath = request.Thumbnail;

        if (request.SortOrder.HasValue)
            chapter.SortOrder = request.SortOrder.Value;

        chapter.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return await GetChapterAsync(chapterId, chapter.VideoId, tenantId);
    }

    public async Task DeleteChapterAsync(Guid chapterId, Guid userId, Guid tenantId)
    {
        var chapter = await _dbContext.VideoChapters
            .Include(vc => vc.Video)
            .FirstOrDefaultAsync(vc => vc.Id == chapterId);

        if (chapter == null || chapter.Video.TenantId != tenantId)
            throw new Exception("Chapter not found");

        _dbContext.VideoChapters.Remove(chapter);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<ChapterDto>> ReorderChaptersAsync(Guid videoId, List<Guid> chapterIds, Guid userId, Guid tenantId)
    {
        // Verify video exists and belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var chapters = await _dbContext.VideoChapters
            .Where(vc => vc.VideoId == videoId && chapterIds.Contains(vc.Id))
            .ToListAsync();

        for (int i = 0; i < chapterIds.Count; i++)
        {
            var chapter = chapters.FirstOrDefault(c => c.Id == chapterIds[i]);
            if (chapter != null)
            {
                chapter.SortOrder = i;
                chapter.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync();

        return await GetChaptersAsync(videoId, tenantId);
    }
}
