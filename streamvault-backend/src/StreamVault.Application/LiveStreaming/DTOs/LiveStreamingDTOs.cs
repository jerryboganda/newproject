using System.ComponentModel.DataAnnotations;
using StreamVault.Domain.Entities;
using StreamVault.Application.Auth.DTOs;

namespace StreamVault.Application.LiveStreaming.DTOs;

public class CreateLiveStreamRequest
{
    [Required, MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public string? ThumbnailUrl { get; set; }

    public DateTimeOffset? ScheduledAt { get; set; }

    public bool IsPublic { get; set; } = true;

    public bool AllowChat { get; set; } = true;

    public bool AllowReactions { get; set; } = true;

    public bool RecordStream { get; set; } = false;

    public string? Category { get; set; }

    public List<string> Tags { get; set; } = new();

    public int MaxDurationMinutes { get; set; } = 240;
    
    public StreamQuality MaxQuality { get; set; } = StreamQuality.High1080p;
    
    public bool IsPrivate { get; set; } = false;
    
    public List<Guid> AllowedViewerIds { get; set; } = new();
    
    public bool RequiresPassword { get; set; } = false;
    
    public string? Password { get; set; }
    
    public bool EnableDVR { get; set; } = false;
    
    public int MaxViewers { get; set; } = 0; // 0 = unlimited
}

public class UpdateLiveStreamRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? ThumbnailUrl { get; set; }

    public DateTimeOffset? ScheduledAt { get; set; }

    public bool? IsPublic { get; set; }

    public bool? AllowChat { get; set; }

    public bool? AllowReactions { get; set; }

    public bool? RecordStream { get; set; }

    public string? Category { get; set; }

    public List<string>? Tags { get; set; }

    public int? MaxDurationMinutes { get; set; }
}

public class LiveStreamDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? PlaybackUrl { get; set; }
    public LiveStreamStatus Status { get; set; }
    public DateTimeOffset? ScheduledAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public int ConcurrentViewers { get; set; }
    public int TotalViews { get; set; }
    public bool IsPublic { get; set; }
    public bool AllowChat { get; set; }
    public bool AllowReactions { get; set; }
    public bool RecordStream { get; set; }
    public Guid? RecordedVideoId { get; set; }
    public string? Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class StreamAccessDto
{
    public string StreamKey { get; set; } = string.Empty;
    public string IngestUrl { get; set; } = string.Empty;
    public string PlaybackUrl { get; set; } = string.Empty;
}

public class SendChatMessageRequest
{
    [Required, MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    public ChatMessageType Type { get; set; } = ChatMessageType.Message;

    public string? Metadata { get; set; }
}

public class LiveStreamChatMessageDto
{
    public Guid Id { get; set; }
    public Guid LiveStreamId { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public ChatMessageType Type { get; set; }
    public string? Metadata { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class LiveStreamStatsDto
{
    public Guid StreamId { get; set; }
    public int ConcurrentViewers { get; set; }
    public int TotalViews { get; set; }
    public TimeSpan? Duration { get; set; }
    public int ChatMessagesCount { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
}

// Enhanced live streaming DTOs
public class LiveStreamViewerDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public DateTimeOffset JoinedAt { get; set; }
    public TimeSpan WatchDuration { get; set; }
    public bool IsModerator { get; set; }
    public bool IsSubscriber { get; set; }
}

public class LiveStreamAnalyticsDto
{
    public Guid StreamId { get; set; }
    public int PeakViewers { get; set; }
    public int AverageViewers { get; set; }
    public int TotalViews { get; set; }
    public TimeSpan TotalWatchTime { get; set; }
    public double AverageWatchDuration { get; set; }
    public List<ViewerCountTimelineDto> ViewerTimeline { get; set; } = new();
    public Dictionary<string, int> CountryBreakdown { get; set; } = new();
    public Dictionary<string, int> DeviceBreakdown { get; set; } = new();
    public int ChatMessagesSent { get; set; }
    public int ReactionsSent { get; set; }
    public double EngagementRate { get; set; }
}

public class ViewerCountTimelineDto
{
    public DateTimeOffset Timestamp { get; set; }
    public int ViewerCount { get; set; }
}

public class LiveStreamRecordingDto
{
    public Guid Id { get; set; }
    public Guid StreamId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public long FileSizeBytes { get; set; }
    public string Format { get; set; } = string.Empty;
    public DateTimeOffset RecordedAt { get; set; }
    public bool IsProcessing { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class StreamSettingsDto
{
    public StreamQuality MaxQuality { get; set; } = StreamQuality.High1080p;
    public bool AllowChat { get; set; } = true;
    public bool AllowReactions { get; set; } = true;
    public bool EnableDVR { get; set; } = false;
    public int MaxViewers { get; set; } = 0;
    public bool IsPrivate { get; set; } = false;
    public bool RequiresPassword { get; set; } = false;
    public string? Password { get; set; }
    public List<Guid> Moderators { get; set; } = new();
    public List<string> BlockedWords { get; set; } = new();
    public bool SlowMode { get; set; } = false;
    public int SlowModeDelaySeconds { get; set; } = 5;
    public bool SubscribersOnly { get; set; } = false;
    public bool FollowersOnly { get; set; } = false;
    public int MinFollowTimeMinutes { get; set; } = 0;
}

public class CreatePollRequest
{
    [Required, MaxLength(200)]
    public string Question { get; set; } = string.Empty;
    
    [Required, MinLength(2), MaxLength(5)]
    public List<string> Options { get; set; } = new();
    
    public int DurationMinutes { get; set; } = 5;
    
    public bool AllowMultipleChoice { get; set; } = false;
}

public class StreamPollDto
{
    public Guid Id { get; set; }
    public Guid StreamId { get; set; }
    public string Question { get; set; } = string.Empty;
    public List<PollOptionDto> Options { get; set; } = new();
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public bool IsActive { get; set; }
    public int TotalVotes { get; set; }
}

public class PollOptionDto
{
    public string Text { get; set; } = string.Empty;
    public int Votes { get; set; }
    public double Percentage { get; set; }
}

public enum StreamQuality
{
    Low240p,
    Medium360p,
    Medium480p,
    High720p,
    High1080p,
    Ultra1440p,
    Ultra4K
}
