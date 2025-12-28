using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class Notification
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public string? ActionUrl { get; set; }

    public NotificationType Type { get; set; } = NotificationType.Info;

    public bool IsRead { get; set; } = false;

    public DateTimeOffset ReadAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error,
    VideoUploaded,
    VideoProcessed,
    CommentReceived,
    SubscriptionRenewed,
    PaymentReceived,
    SystemUpdate
}
