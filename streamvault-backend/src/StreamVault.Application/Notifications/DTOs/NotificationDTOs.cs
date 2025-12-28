using System.ComponentModel.DataAnnotations;
using StreamVault.Domain.Entities;

namespace StreamVault.Application.Notifications.DTOs;

public class CreateNotificationRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public string? ActionUrl { get; set; }

    public NotificationType Type { get; set; } = NotificationType.Info;
    
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
    
    public Dictionary<string, object>? Metadata { get; set; }
    
    public bool SendRealTime { get; set; } = true;
    
    public DateTimeOffset? ScheduledAt { get; set; }
}

public class RealTimeNotificationRequest
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public string Event { get; set; } = string.Empty;
    
    [Required]
    public Dictionary<string, object> Data { get; set; } = new();
    
    public string? Message { get; set; }
    
    public bool Broadcast { get; set; } = false;
}

public class UpdateNotificationRequest
{
    public string? Title { get; set; }

    public string? Message { get; set; }

    public string? ActionUrl { get; set; }

    public NotificationType? Type { get; set; }
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset ReadAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public NotificationChannel Channel { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class NotificationCountDto
{
    public int Total { get; set; }
    public int Unread { get; set; }
}

public class NotificationPreferencesDto
{
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool SMSNotifications { get; set; } = false;
    public bool InAppNotifications { get; set; } = true;
    public List<string> EnabledTypes { get; set; } = new();
    public List<string> DisabledTypes { get; set; } = new();
    public bool DoNotDisturb { get; set; } = false;
    public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;
    public TimeOnly QuietHoursStart { get; set; } = new TimeOnly(22, 0);
    public TimeOnly QuietHoursEnd { get; set; } = new TimeOnly(8, 0);
}

public class NotificationTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string TitleTemplate { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
    public Dictionary<string, object> DefaultVariables { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class CreateNotificationTemplateRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    public string TitleTemplate { get; set; } = string.Empty;
    
    [Required]
    public string MessageTemplate { get; set; } = string.Empty;
    
    public Dictionary<string, object>? DefaultVariables { get; set; }
}

public class PushNotificationRequest
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Body { get; set; } = string.Empty;
    
    public string? Icon { get; set; }
    
    public string? Image { get; set; }
    
    public string? ClickAction { get; set; }
    
    public Dictionary<string, string>? Data { get; set; }
    
    public int? Badge { get; set; }
    
    public string? Sound { get; set; }
}

public class EmailNotificationRequest
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    public string HtmlBody { get; set; } = string.Empty;
    
    public string? TextBody { get; set; }
    
    public string? FromEmail { get; set; }
    
    public string? FromName { get; set; }
    
    public List<EmailAttachmentDto>? Attachments { get; set; }
}

public class EmailAttachmentDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
}

public class SMSNotificationRequest
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public string? FromNumber { get; set; }
}

public class NotificationAnalyticsDto
{
    public DateOnly Date { get; set; }
    public int TotalSent { get; set; }
    public int Delivered { get; set; }
    public int Read { get; set; }
    public int Clicked { get; set; }
    public double DeliveryRate { get; set; }
    public double ReadRate { get; set; }
    public double ClickRate { get; set; }
    public Dictionary<string, int> TypeBreakdown { get; set; } = new();
    public Dictionary<string, int> ChannelBreakdown { get; set; } = new();
}

public class NotificationHubConnectionDto
{
    public Guid UserId { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public DateTimeOffset ConnectedAt { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
}

public enum NotificationChannel
{
    InApp,
    Email,
    Push,
    SMS,
    Webhook
}

public class BulkNotificationRequest
{
    [Required]
    public List<Guid> UserIds { get; set; } = new();
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public string? ActionUrl { get; set; }
    
    public NotificationType Type { get; set; } = NotificationType.Info;
    
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
    
    public bool SendRealTime { get; set; } = true;
    
    public Dictionary<string, object>? Metadata { get; set; }
}
