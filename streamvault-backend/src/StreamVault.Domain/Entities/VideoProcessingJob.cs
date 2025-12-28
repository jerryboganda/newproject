using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace StreamVault.Domain.Entities;

public class VideoProcessingJob
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VideoId { get; set; }

    public ProcessingJobType JobType { get; set; }

    public ProcessingJobStatus Status { get; set; } = ProcessingJobStatus.Pending;

    public int ProgressPercentage { get; set; } = 0;

    public string? ErrorMessage { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    // Navigation properties
    public Video Video { get; set; } = null!;
}

public enum ProcessingJobType
{
    ThumbnailGeneration,
    Transcoding,
    CaptionGeneration,
    Analysis
}

public enum ProcessingJobStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}
