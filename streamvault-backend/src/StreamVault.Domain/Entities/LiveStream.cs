using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class LiveStream
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required, MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string? StreamKey { get; set; }

    public string? PlaybackUrl { get; set; }

    public string? IngestUrl { get; set; }

    public LiveStreamStatus Status { get; set; } = LiveStreamStatus.Scheduled;

    public DateTimeOffset? ScheduledAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? EndedAt { get; set; }

    public int ConcurrentViewers { get; set; } = 0;

    public int TotalViews { get; set; } = 0;

    public bool IsPublic { get; set; } = true;

    public bool AllowChat { get; set; } = true;

    public bool AllowReactions { get; set; } = true;

    public bool RecordStream { get; set; } = false;

    public Guid? RecordedVideoId { get; set; }

    public string? Category { get; set; }

    public List<string> Tags { get; set; } = new();

    public int MaxDurationMinutes { get; set; } = 240; // 4 hours default

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Video? RecordedVideo { get; set; }
    public List<LiveStreamViewer> Viewers { get; set; } = new();
    public List<LiveStreamChatMessage> ChatMessages { get; set; } = new();
}

public class LiveStreamViewer
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid LiveStreamId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LeftAt { get; set; }

    // Navigation properties
    public LiveStream LiveStream { get; set; } = null!;
    public User User { get; set; } = null!;
}

public class LiveStreamChatMessage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid LiveStreamId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required, MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    public ChatMessageType Type { get; set; } = ChatMessageType.Message;

    public string? Metadata { get; set; } // For reactions, donations, etc.

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public LiveStream LiveStream { get; set; } = null!;
    public User User { get; set; } = null!;
}

public enum LiveStreamStatus
{
    Scheduled,
    Live,
    Ended,
    Cancelled
}

public enum ChatMessageType
{
    Message,
    Reaction,
    Donation,
    System,
    Moderation
}
