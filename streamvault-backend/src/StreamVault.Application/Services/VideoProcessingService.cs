using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Services
{
    public interface IVideoProcessingService
    {
        Task<EncodingJob> CreateEncodingJobAsync(Guid videoId, EncodingJobType jobType);
        Task UpdateEncodingJobStatusAsync(Guid jobId, EncodingJobStatus status, int? progress = null, string? errorMessage = null);
        Task<EncodingJob?> GetEncodingJobAsync(Guid jobId);
        Task<IEnumerable<EncodingJob>> GetVideoEncodingJobsAsync(Guid videoId);
        Task<IEnumerable<EncodingJob>> GetPendingJobsAsync(int limit = 100);
        Task<VideoProcessingStatus> GetVideoProcessingStatusAsync(Guid videoId);
    }

    public class VideoProcessingService : IVideoProcessingService
    {
        private readonly StreamVaultDbContext _context;
        private readonly IBunnyCDNService _bunnyCDNService;

        public VideoProcessingService(StreamVaultDbContext context, IBunnyCDNService bunnyCDNService)
        {
            _context = context;
            _bunnyCDNService = bunnyCDNService;
        }

        public async Task<EncodingJob> CreateEncodingJobAsync(Guid videoId, EncodingJobType jobType)
        {
            var video = await _context.Videos
                .Include(v => v.Tenant)
                .FirstOrDefaultAsync(v => v.Id == videoId);

            if (video == null)
                throw new ArgumentException("Video not found", nameof(videoId));

            var job = new EncodingJob
            {
                Id = Guid.NewGuid(),
                VideoId = videoId,
                TenantId = video.TenantId,
                JobType = jobType,
                Status = EncodingJobStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.EncodingJobs.Add(job);
            await _context.SaveChangesAsync();

            // Update video status
            video.Status = VideoStatus.Processing;
            video.ProcessingStartedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return job;
        }

        public async Task UpdateEncodingJobStatusAsync(Guid jobId, EncodingJobStatus status, int? progress = null, string? errorMessage = null)
        {
            var job = await _context.EncodingJobs
                .Include(j => j.Video)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null)
                throw new ArgumentException("Encoding job not found", nameof(jobId));

            job.Status = status;
            job.Progress = progress;
            job.ErrorMessage = errorMessage;
            job.UpdatedAt = DateTime.UtcNow;

            if (status == EncodingJobStatus.Processing && !job.StartedAt.HasValue)
            {
                job.StartedAt = DateTime.UtcNow;
            }
            else if (status == EncodingJobStatus.Completed)
            {
                job.CompletedAt = DateTime.UtcNow;
                job.Video.Status = VideoStatus.Ready;
                job.Video.PublishedAt = DateTime.UtcNow;
            }
            else if (status == EncodingJobStatus.Failed)
            {
                job.FailedAt = DateTime.UtcNow;
                job.Video.Status = VideoStatus.Failed;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<EncodingJob?> GetEncodingJobAsync(Guid jobId)
        {
            return await _context.EncodingJobs
                .Include(j => j.Video)
                .FirstOrDefaultAsync(j => j.Id == jobId);
        }

        public async Task<IEnumerable<EncodingJob>> GetVideoEncodingJobsAsync(Guid videoId)
        {
            return await _context.EncodingJobs
                .Where(j => j.VideoId == videoId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<EncodingJob>> GetPendingJobsAsync(int limit = 100)
        {
            return await _context.EncodingJobs
                .Where(j => j.Status == EncodingJobStatus.Pending || j.Status == EncodingJobStatus.Processing)
                .OrderBy(j => j.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<VideoProcessingStatus> GetVideoProcessingStatusAsync(Guid videoId)
        {
            var video = await _context.Videos
                .Include(v => v.EncodingJobs)
                .FirstOrDefaultAsync(v => v.Id == videoId);

            if (video == null)
                throw new ArgumentException("Video not found", nameof(videoId));

            var latestJob = video.EncodingJobs.OrderByDescending(j => j.CreatedAt).FirstOrDefault();

            return new VideoProcessingStatus
            {
                VideoId = videoId,
                Status = video.Status,
                Progress = latestJob?.Progress ?? 0,
                CurrentJobType = latestJob?.JobType,
                StartedAt = video.ProcessingStartedAt,
                EstimatedCompletion = CalculateEstimatedCompletion(latestJob),
                EncodingJobs = video.EncodingJobs.Select(j => new EncodingJobInfo
                {
                    Id = j.Id,
                    JobType = j.JobType,
                    Status = j.Status,
                    Progress = j.Progress,
                    CreatedAt = j.CreatedAt,
                    StartedAt = j.StartedAt,
                    CompletedAt = j.CompletedAt,
                    ErrorMessage = j.ErrorMessage
                }).ToList()
            };
        }

        private DateTime? CalculateEstimatedCompletion(EncodingJob? job)
        {
            if (job?.Status != EncodingJobStatus.Processing || !job.StartedAt.HasValue)
                return null;

            var averageProcessingTime = TimeSpan.FromMinutes(10); // Configurable
            var elapsed = DateTime.UtcNow - job.StartedAt.Value;
            var remaining = averageProcessingTime - elapsed;

            return remaining > TimeSpan.Zero ? DateTime.UtcNow + remaining : DateTime.UtcNow.AddMinutes(1);
        }
    }

    public class VideoProcessingStatus
    {
        public Guid VideoId { get; set; }
        public VideoStatus Status { get; set; }
        public int Progress { get; set; }
        public EncodingJobType? CurrentJobType { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EstimatedCompletion { get; set; }
        public List<EncodingJobInfo> EncodingJobs { get; set; } = new();
    }

    public class EncodingJobInfo
    {
        public Guid Id { get; set; }
        public EncodingJobType JobType { get; set; }
        public EncodingJobStatus Status { get; set; }
        public int? Progress { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public enum EncodingJobType
    {
        Transcoding,
        ThumbnailGeneration,
        CaptionGeneration,
        ChapterGeneration,
        QualityConversion
    }

    public enum EncodingJobStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled
    }
}
