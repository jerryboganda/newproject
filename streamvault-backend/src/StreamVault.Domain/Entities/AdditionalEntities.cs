using System;
using System.Collections.Generic;
using StreamVault.Domain.Interfaces;

namespace StreamVault.Domain.Entities
{
    /// <summary>
    /// Represents a team invitation
    /// </summary>
    public class TeamInvitation : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid InviterId { get; set; }
        public Guid InviteeId { get; set; }
        public string InviteeEmail { get; set; } = string.Empty;
        public Guid TenantId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string Status { get; set; } = string.Empty; // Pending, Accepted, Declined, Expired
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RespondedAt { get; set; }

        // Navigation properties
        public virtual User Inviter { get; set; } = null!;
        public virtual User? Invitee { get; set; }
        public virtual Tenant Tenant { get; set; } = null!;
    }

    /// <summary>
    /// Represents a playlist video relationship
    /// </summary>
    public class PlaylistVideo : ITenantEntity
    {
        public Guid PlaylistId { get; set; }
        public Guid VideoId { get; set; }
        public DateTime AddedAt { get; set; }
        public int SortOrder { get; set; }
        public Guid TenantId { get; set; }

        // Navigation properties
        public virtual Playlist Playlist { get; set; } = null!;
        public virtual Video Video { get; set; } = null!;
    }

    /// <summary>
    /// Represents a video view
    /// </summary>
    public class VideoView : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid VideoId { get; set; }
        public Guid? UserId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Referrer { get; set; }
        public DateTime ViewedAt { get; set; }
        public Guid TenantId { get; set; }

        // Navigation properties
        public virtual Video Video { get; set; } = null!;
        public virtual User? User { get; set; }
    }

    /// <summary>
    /// Represents a video tag
    /// </summary>
    public class VideoTag : ITenantEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid TenantId { get; set; }

        // Navigation properties
        public virtual ICollection<VideoVideoTag> VideoTags { get; set; } = new List<VideoVideoTag>();
    }

    /// <summary>
    /// Represents a video tag relationship
    /// </summary>
    public class VideoVideoTag : ITenantEntity
    {
        public Guid VideoId { get; set; }
        public Guid TagId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid TenantId { get; set; }

        // Navigation properties
        public virtual Video Video { get; set; } = null!;
        public virtual VideoTag Tag { get; set; } = null!;
    }

    /// <summary>
    /// Represents a video comment
    /// </summary>
    public class Comment : ITenantEntity
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid VideoId { get; set; }
        public Guid UserId { get; set; }
        public Guid? ParentId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid TenantId { get; set; }

        // Navigation properties
        public virtual Video Video { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual Comment? Parent { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }

    /// <summary>
    /// Represents a video like
    /// </summary>
    public class VideoLike : ITenantEntity
    {
        public Guid VideoId { get; set; }
        public Guid UserId { get; set; }
        public DateTime LikedAt { get; set; }
        public Guid TenantId { get; set; }

        // Navigation properties
        public virtual Video Video { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
    /// <summary>
    /// Represents a tenant invoice
    /// </summary>
    public class TenantInvoice : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string StripeInvoiceId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // draft, open, paid, void, etc.
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime DueDate { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? InvoiceUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual Tenant Tenant { get; set; } = null!;
    }

    /// <summary>
    /// Represents a webhook event
    /// </summary>
    public class WebhookEvent
    {
        public Guid Id { get; set; }
        public string Source { get; set; } = string.Empty; // stripe, bunny, etc.
        public string EventType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public bool Processed { get; set; }
        public string? ProcessingError { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

    /// <summary>
    /// Represents an audit log entry
    /// </summary>
    public class AuditLog : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public Dictionary<string, object>? OldValues { get; set; }
        public Dictionary<string, object>? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid TenantId { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }

    /// <summary>
    /// Represents a notification
    /// </summary>
    public class Notification : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public Guid TenantId { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }

    /// <summary>
    /// Represents an API key
    /// </summary>
    public class ApiKey : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KeyHash { get; set; } = string.Empty;
        public string KeyPrefix { get; set; } = string.Empty;
        public string[] Scopes { get; set; } = Array.Empty<string>();
        public DateTime ExpiresAt { get; set; }
        public DateTime LastUsedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid TenantId { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
