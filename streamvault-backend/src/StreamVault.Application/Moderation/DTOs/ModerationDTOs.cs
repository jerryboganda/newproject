using System.ComponentModel.DataAnnotations;

namespace StreamVault.Application.Moderation.DTOs;

public class ModerationResultDto
{
    public Guid VideoId { get; set; }
    public ModerationStatus Status { get; set; }
    public double ConfidenceScore { get; set; }
    public List<ModerationFlagDto> Flags { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public bool RequiresHumanReview { get; set; }
    public DateTimeOffset ModeratedAt { get; set; }
    public string? ModeratorId { get; set; }
    public string? ModeratorNotes { get; set; }
    public List<string> DetectedIssues { get; set; } = new();
    public Dictionary<string, double> CategoryScores { get; set; } = new();
}

public class ModerationFlagDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public string Type { get; set; } = string.Empty; // violence, adult_content, hate_speech, copyright, spam, etc.
    public string Severity { get; set; } = string.Empty; // low, medium, high, critical
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public TimeSpan? Timestamp { get; set; }
    public string? Evidence { get; set; }
    public bool IsAutoDetected { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsResolved { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
}

public enum ModerationStatus
{
    Pending,
    Approved,
    Rejected,
    UnderReview,
    Flagged,
    AutoApproved,
    AutoRejected
}

public class ReportVideoRequest
{
    [Required]
    public string Reason { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public string Category { get; set; } = string.Empty; // violence, harassment, copyright, spam, etc.
    
    public TimeSpan? Timestamp { get; set; }
    
    public Dictionary<string, object>? Evidence { get; set; }
}

public class ReportedVideoDto
{
    public Guid VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public int ReportCount { get; set; }
    public List<VideoReportDto> Reports { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public ModerationStatus Status { get; set; }
    public bool IsUnderReview { get; set; }
    public string? ReviewerId { get; set; }
    public double SeverityScore { get; set; }
}

public class VideoReportDto
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TimeSpan? Timestamp { get; set; }
    public DateTimeOffset ReportedAt { get; set; }
    public bool IsReviewed { get; set; }
    public ReportStatus Status { get; set; }
}

public enum ReportStatus
{
    Pending,
    UnderReview,
    Resolved,
    Dismissed,
    ActionTaken
}

public class PendingModerationDto
{
    public Guid VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public string CreatorEmail { get; set; } = string.Empty;
    public DateTimeOffset UploadedAt { get; set; }
    public int DurationSeconds { get; set; }
    public double PriorityScore { get; set; }
    public List<string> FlagReasons { get; set; } = new();
    public int ReportCount { get; set; }
    public bool IsAutoFlagged { get; set; }
    public string QueueType { get; set; } = string.Empty; // standard, priority, escalated
}

public class ModerationStatsDto
{
    public int TotalVideosModerated { get; set; }
    public int AutoApproved { get; set; }
    public int AutoRejected { get; set; }
    public int ManuallyReviewed { get; set; }
    public int PendingReview { get; set; }
    public Dictionary<string, int> FlagCategories { get; set; } = new();
    public double AverageReviewTime { get; set; }
    public double AccuracyRate { get; set; }
    public List<DailyModerationStatsDto> DailyStats { get; set; } = new();
    public List<ModeratorPerformanceDto> ModeratorPerformance { get; set; } = new();
}

public class DailyModerationStatsDto
{
    public DateOnly Date { get; set; }
    public int VideosProcessed { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Escalated { get; set; }
    public double AverageProcessingTime { get; set; }
}

public class ModeratorPerformanceDto
{
    public Guid ModeratorId { get; set; }
    public string ModeratorName { get; set; } = string.Empty;
    public int VideosReviewed { get; set; }
    public int ActionsTaken { get; set; }
    public double AverageTimePerVideo { get; set; }
    public double AccuracyRate { get; set; }
    public int AppealsReceived { get; set; }
    public int AppealsUpheld { get; set; }
}

public class ModerationSettingsDto
{
    public bool EnableAutoModeration { get; set; } = true;
    public double AutoApprovalThreshold { get; set; } = 0.9;
    public double AutoRejectionThreshold { get; set; } = 0.8;
    public List<string> RestrictedCategories { get; set; } = new();
    public List<string> BannedWords { get; set; } = new();
    public List<string> SuspiciousPatterns { get; set; } = new();
    public bool RequireManualReviewForNewCreators { get; set; } = true;
    public int NewCreatorThreshold { get; set; } = 5; // Number of videos before auto-approval
    public bool EnableCopyrightDetection { get; set; } = true;
    public bool EnableAdultContentDetection { get; set; } = true;
    public bool EnableViolenceDetection { get; set; } = true;
    public bool EnableHateSpeechDetection { get; set; } = true;
    public int MaxVideoLength { get; set; } = 7200; // 2 hours in seconds
    public List<string> TrustedCreatorIds { get; set; } = new();
    public bool EnableAppealsProcess { get; set; } = true;
    public int AppealReviewTimeHours { get; set; } = 48;
}

public class ModerationActionDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public Guid ModeratorId { get; set; }
    public string ModeratorName { get; set; } = string.Empty;
    public ModerationAction Action { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<string> FlagsResolved { get; set; } = new();
    public DateTimeOffset Timestamp { get; set; }
    public bool IsAutomated { get; set; }
    public string? AutomationRule { get; set; }
}

public enum ModerationAction
{
    Approved,
    Rejected,
    Flagged,
    Escalated,
    AgeRestricted,
    Demonetized,
    Removed,
    WarningIssued
}

public class AutoModeratedContentDto
{
    public Guid VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public string DetectionType { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset DetectedAt { get; set; }
    public bool IsUnderReview { get; set; }
    public string? ReviewerId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ContentDetectionResultDto
{
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<DetectionDetailDto> Details { get; set; } = new();
    public bool RequiresAction { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
}

public class DetectionDetailDto
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public TimeSpan? Timestamp { get; set; }
    public string? Evidence { get; set; }
}

public class ModerationQueueDto
{
    public string QueueName { get; set; } = string.Empty;
    public int PendingCount { get; set; }
    public int InProgressCount { get; set; }
    public int AverageWaitTime { get; set; }
    public List<PendingModerationDto> Videos { get; set; } = new();
}

public class AppealDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string OriginalDecision { get; set; } = string.Empty;
    public string AppealReason { get; set; } = string.Empty;
    public DateTimeOffset AppealedAt { get; set; }
    public AppealStatus Status { get; set; }
    public string? ReviewerId { get; set; }
    public string? ReviewerNotes { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string FinalDecision { get; set; } = string.Empty;
}

public enum AppealStatus
{
    Pending,
    UnderReview,
    Approved,
    Rejected,
    Escalated
}

public class ModerationRuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Conditions { get; set; } = new();
    public string Action { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastTriggered { get; set; }
    public int TriggerCount { get; set; }
}

public class BatchModerationRequest
{
    public List<Guid> VideoIds { get; set; } = new();
    public ModerationAction Action { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool ApplyToAll { get; set; }
}

public class BatchModerationResult
{
    public int TotalVideos { get; set; }
    public int ProcessedVideos { get; set; }
    public int FailedVideos { get; set; }
    public List<Guid> SuccessfulVideoIds { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
