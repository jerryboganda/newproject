using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Services
{
    public interface IChapterService
    {
        Task<Chapter> CreateChapterAsync(Guid videoId, string title, TimeSpan startTime, TimeSpan? endTime = null, string? description = null, string? thumbnailUrl = null);
        Task UpdateChapterAsync(Guid chapterId, string? title = null, TimeSpan? startTime = null, TimeSpan? endTime = null, string? description = null, string? thumbnailUrl = null);
        Task DeleteChapterAsync(Guid chapterId);
        Task<Chapter?> GetChapterAsync(Guid chapterId);
        Task<IEnumerable<Chapter>> GetVideoChaptersAsync(Guid videoId);
        Task ReorderChaptersAsync(Guid videoId, List<Guid> chapterIds);
        Task<Chapter> AutoGenerateChaptersAsync(Guid videoId);
        Task<byte[]> ExportChaptersAsync(Guid videoId, ChapterExportFormat format);
        Task ImportChaptersAsync(Guid videoId, byte[] data, ChapterExportFormat format);
    }

    public class ChapterService : IChapterService
    {
        private readonly StreamVaultDbContext _context;
        private readonly IVideoProcessingService _videoProcessingService;

        public ChapterService(StreamVaultDbContext context, IVideoProcessingService videoProcessingService)
        {
            _context = context;
            _videoProcessingService = videoProcessingService;
        }

        public async Task<Chapter> CreateChapterAsync(Guid videoId, string title, TimeSpan startTime, TimeSpan? endTime = null, string? description = null, string? thumbnailUrl = null)
        {
            var video = await _context.Videos
                .Include(v => v.Tenant)
                .FirstOrDefaultAsync(v => v.Id == videoId);

            if (video == null)
                throw new ArgumentException("Video not found", nameof(videoId));

            // Validate time range
            if (startTime < TimeSpan.Zero || (endTime.HasValue && endTime.Value > video.Duration))
                throw new ArgumentException("Invalid time range");

            if (endTime.HasValue && startTime >= endTime.Value)
                throw new ArgumentException("Start time must be before end time");

            var chapter = new Chapter
            {
                Id = Guid.NewGuid(),
                VideoId = videoId,
                TenantId = video.TenantId,
                Title = title,
                StartTime = startTime,
                EndTime = endTime,
                Description = description,
                ThumbnailUrl = thumbnailUrl,
                Order = await GetNextChapterOrderAsync(videoId),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Chapters.Add(chapter);
            await _context.SaveChangesAsync();

            return chapter;
        }

        public async Task UpdateChapterAsync(Guid chapterId, string? title = null, TimeSpan? startTime = null, TimeSpan? endTime = null, string? description = null, string? thumbnailUrl = null)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Video)
                .FirstOrDefaultAsync(c => c.Id == chapterId);

            if (chapter == null)
                throw new ArgumentException("Chapter not found", nameof(chapterId));

            if (title != null) chapter.Title = title;
            if (startTime.HasValue) chapter.StartTime = startTime.Value;
            if (endTime.HasValue) chapter.EndTime = endTime.Value;
            if (description != null) chapter.Description = description;
            if (thumbnailUrl != null) chapter.ThumbnailUrl = thumbnailUrl;

            chapter.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteChapterAsync(Guid chapterId)
        {
            var chapter = await _context.Chapters.FindAsync(chapterId);
            if (chapter == null)
                throw new ArgumentException("Chapter not found", nameof(chapterId));

            _context.Chapters.Remove(chapter);
            await _context.SaveChangesAsync();

            // Reorder remaining chapters
            await ReorderChaptersAfterDeletionAsync(chapter.VideoId);
        }

        public async Task<Chapter?> GetChapterAsync(Guid chapterId)
        {
            return await _context.Chapters
                .Include(c => c.Video)
                .FirstOrDefaultAsync(c => c.Id == chapterId);
        }

        public async Task<IEnumerable<Chapter>> GetVideoChaptersAsync(Guid videoId)
        {
            return await _context.Chapters
                .Where(c => c.VideoId == videoId)
                .OrderBy(c => c.Order)
                .ThenBy(c => c.StartTime)
                .ToListAsync();
        }

        public async Task ReorderChaptersAsync(Guid videoId, List<Guid> chapterIds)
        {
            var chapters = await _context.Chapters
                .Where(c => c.VideoId == videoId && chapterIds.Contains(c.Id))
                .ToListAsync();

            for (int i = 0; i < chapterIds.Count; i++)
            {
                var chapter = chapters.FirstOrDefault(c => c.Id == chapterIds[i]);
                if (chapter != null)
                {
                    chapter.Order = i + 1;
                    chapter.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Chapter> AutoGenerateChaptersAsync(Guid videoId)
        {
            // Create encoding job for chapter generation
            var job = await _videoProcessingService.CreateEncodingJobAsync(videoId, EncodingJobType.ChapterGeneration);
            
            // In a real implementation, this would trigger an AI/ML service
            // For now, we'll return a placeholder
            return new Chapter
            {
                Id = Guid.NewGuid(),
                VideoId = videoId,
                Title = "Auto-generated Chapter",
                StartTime = TimeSpan.Zero,
                Order = 1,
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<byte[]> ExportChaptersAsync(Guid videoId, ChapterExportFormat format)
        {
            var chapters = await GetVideoChaptersAsync(videoId);
            
            return format switch
            {
                ChapterExportFormat.Json => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(chapters),
                ChapterExportFormat.Csv => ExportToCsv(chapters),
                ChapterExportFormat.Txt => ExportToText(chapters),
                _ => throw new ArgumentException("Unsupported format", nameof(format))
            };
        }

        public async Task ImportChaptersAsync(Guid videoId, byte[] data, ChapterExportFormat format)
        {
            var video = await _context.Videos.FindAsync(videoId);
            if (video == null)
                throw new ArgumentException("Video not found", nameof(videoId));

            List<Chapter> chapters = format switch
            {
                ChapterExportFormat.Json => System.Text.Json.JsonSerializer.Deserialize<List<Chapter>>(data) ?? new List<Chapter>(),
                ChapterExportFormat.Csv => ImportFromCsv(data, videoId),
                ChapterExportFormat.Txt => ImportFromText(data, videoId),
                _ => throw new ArgumentException("Unsupported format", nameof(format))
            };

            // Clear existing chapters
            var existingChapters = await _context.Chapters.Where(c => c.VideoId == videoId).ToListAsync();
            _context.Chapters.RemoveRange(existingChapters);

            // Add new chapters
            foreach (var chapter in chapters)
            {
                chapter.Id = Guid.NewGuid();
                chapter.VideoId = videoId;
                chapter.TenantId = video.TenantId;
                chapter.CreatedAt = DateTime.UtcNow;
                chapter.UpdatedAt = DateTime.UtcNow;
                _context.Chapters.Add(chapter);
            }

            await _context.SaveChangesAsync();
        }

        private async Task<int> GetNextChapterOrderAsync(Guid videoId)
        {
            var lastChapter = await _context.Chapters
                .Where(c => c.VideoId == videoId)
                .OrderByDescending(c => c.Order)
                .FirstOrDefaultAsync();

            return (lastChapter?.Order ?? 0) + 1;
        }

        private async Task ReorderChaptersAfterDeletionAsync(Guid videoId)
        {
            var chapters = await _context.Chapters
                .Where(c => c.VideoId == videoId)
                .OrderBy(c => c.Order)
                .ToListAsync();

            for (int i = 0; i < chapters.Count; i++)
            {
                chapters[i].Order = i + 1;
                chapters[i].UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        private byte[] ExportToCsv(IEnumerable<Chapter> chapters)
        {
            var csv = "Title,StartTime,EndTime,Description\n";
            foreach (var chapter in chapters)
            {
                csv += $"\"{chapter.Title}\",{chapter.StartTime},{chapter.EndTime},\"{chapter.Description}\"\n";
            }
            return System.Text.Encoding.UTF8.GetBytes(csv);
        }

        private byte[] ExportToText(IEnumerable<Chapter> chapters)
        {
            var text = "";
            foreach (var chapter in chapters)
            {
                text += $"{chapter.StartTime:hh\\:mm\\:ss} - {chapter.EndTime:hh\\:mm\\:ss}: {chapter.Title}\n";
                if (!string.IsNullOrEmpty(chapter.Description))
                {
                    text += $"  {chapter.Description}\n";
                }
                text += "\n";
            }
            return System.Text.Encoding.UTF8.GetBytes(text);
        }

        private List<Chapter> ImportFromCsv(byte[] data, Guid videoId)
        {
            var chapters = new List<Chapter>();
            var lines = System.Text.Encoding.UTF8.GetString(data).Split('\n');
            
            foreach (var line in lines.Skip(1)) // Skip header
            {
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    chapters.Add(new Chapter
                    {
                        Title = parts[0].Trim('"'),
                        StartTime = TimeSpan.Parse(parts[1]),
                        EndTime = TimeSpan.Parse(parts[2]),
                        Description = parts.Length > 3 ? parts[3].Trim('"') : null
                    });
                }
            }
            
            return chapters;
        }

        private List<Chapter> ImportFromText(byte[] data, Guid videoId)
        {
            var chapters = new List<Chapter>();
            var lines = System.Text.Encoding.UTF8.GetString(data).Split('\n');
            
            foreach (var line in lines)
            {
                if (line.Contains(':') && line.Contains('-'))
                {
                    var parts = line.Split(new[] { " - ", ": " }, StringSplitOptions.None);
                    if (parts.Length >= 3)
                    {
                        chapters.Add(new Chapter
                        {
                            StartTime = TimeSpan.Parse(parts[0]),
                            EndTime = TimeSpan.Parse(parts[1]),
                            Title = parts[2]
                        });
                    }
                }
            }
            
            return chapters;
        }
    }

    public enum ChapterExportFormat
    {
        Json,
        Csv,
        Txt
    }
}
