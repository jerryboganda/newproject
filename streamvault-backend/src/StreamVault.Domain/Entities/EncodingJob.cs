using System;
using StreamVault.Domain.Interfaces;

namespace StreamVault.Domain.Entities
{
    public class EncodingJob : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid VideoId { get; set; }
        public Video Video { get; set; } = null!;
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public EncodingJobType JobType { get; set; }
        public EncodingJobStatus Status { get; set; }
        public int? Progress { get; set; }
        public string? ErrorMessage { get; set; }
        public string? OutputPath { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public enum EncodingJobType
    {
        Transcoding = 1,
        ThumbnailGeneration = 2,
        CaptionGeneration = 3,
        ChapterGeneration = 4,
        QualityConversion = 5,
        AudioExtraction = 6,
        Watermarking = 7
    }

    public enum EncodingJobStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4,
        Retrying = 5
    }
}
