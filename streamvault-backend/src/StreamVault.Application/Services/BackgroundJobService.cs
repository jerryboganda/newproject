using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Services;

public class BackgroundJobService : IBackgroundJobService
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly ILogger<BackgroundJobService> _logger;

    public BackgroundJobService(StreamVaultDbContext dbContext, ILogger<BackgroundJobService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task EnqueueVideoProcessingAsync(Guid videoId, string jobType, Dictionary<string, object>? metadata = null)
    {
        var job = new VideoProcessingJob
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            JobType = jobType.ToLower() switch
            {
                "thumbnail" => ProcessingJobType.ThumbnailGeneration,
                "transcode" => ProcessingJobType.Transcoding,
                "caption" => ProcessingJobType.CaptionGeneration,
                "analysis" => ProcessingJobType.Analysis,
                _ => ProcessingJobType.Transcoding
            },
            Status = ProcessingJobStatus.Pending,
            Metadata = metadata,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.VideoProcessingJobs.Add(job);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Enqueued video processing job {JobId} for video {VideoId}", job.Id, videoId);

        // TODO: In production, this would enqueue to a real queue system like RabbitMQ, Azure Service Bus, or AWS SQS
        // For now, we'll simulate processing
        _ = Task.Run(() => ProcessJobAsync(job.Id));
    }

    public async Task EnqueueThumbnailGenerationAsync(Guid videoId)
    {
        await EnqueueVideoProcessingAsync(videoId, "thumbnail", new Dictionary<string, object>
        {
            ["outputFormat"] = "jpg",
            ["quality"] = 90
        });
    }

    public async Task EnqueueVideoTranscodingAsync(Guid videoId, string outputFormat = "mp4")
    {
        await EnqueueVideoProcessingAsync(videoId, "transcode", new Dictionary<string, object>
        {
            ["outputFormat"] = outputFormat,
            ["quality"] = "high"
        });
    }

    public async Task EnqueueVideoAnalysisAsync(Guid videoId)
    {
        await EnqueueVideoProcessingAsync(videoId, "analysis", new Dictionary<string, object>
        {
            ["extractMetadata"] = true,
            ["generateTranscript"] = false
        });
    }

    private async Task ProcessJobAsync(Guid jobId)
    {
        var job = await _dbContext.VideoProcessingJobs.FindAsync(jobId);
        if (job == null) return;

        try
        {
            job.Status = ProcessingJobStatus.Processing;
            job.StartedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();

            // Simulate processing time
            await Task.Delay(Random.Shared.Next(5000, 15000));

            // Update progress
            job.ProgressPercentage = 50;
            await _dbContext.SaveChangesAsync();

            // Simulate more processing
            await Task.Delay(Random.Shared.Next(5000, 10000));

            // Complete the job
            job.Status = ProcessingJobStatus.Completed;
            job.ProgressPercentage = 100;
            job.CompletedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();

            // Update video status if all jobs are complete
            var video = await _dbContext.Videos.FindAsync(job.VideoId);
            if (video != null)
            {
                var allJobs = await _dbContext.VideoProcessingJobs
                    .Where(j => j.VideoId == job.VideoId)
                    .ToListAsync();

                if (allJobs.All(j => j.Status == ProcessingJobStatus.Completed))
                {
                    video.Status = VideoStatus.Processed;
                    video.UpdatedAt = DateTimeOffset.UtcNow;
                    await _dbContext.SaveChangesAsync();
                }
            }

            _logger.LogInformation("Completed video processing job {JobId}", jobId);
        }
        catch (Exception ex)
        {
            job.Status = ProcessingJobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogError(ex, "Failed to process video processing job {JobId}", jobId);
        }
    }
}
