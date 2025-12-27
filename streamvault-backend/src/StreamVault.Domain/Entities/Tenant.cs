using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StreamVault.Domain.Entities;

public class Tenant
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Slug { get; set; } = string.Empty; // For subdomain

    public TenantStatus Status { get; set; } = TenantStatus.Trial;

    public DatabaseType DatabaseType { get; set; } = DatabaseType.Shared;

    public string? DedicatedDbConn { get; set; } // Encrypted

    public string? BunnyLibraryId { get; set; } // Encrypted

    public string? BunnyApiKey { get; set; } // Encrypted

    public string? BunnyPullZoneId { get; set; }

    public string? BunnyCdnHostname { get; set; }

    public DateTimeOffset? TrialEndsAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? UpdatedAt { get; set; }

    public Dictionary<string, object>? Settings { get; set; } // JSONB

    // Navigation
    public TenantBranding? Branding { get; set; }

    public ICollection<TenantSubscription> Subscriptions { get; set; } = new List<TenantSubscription>();

    public ICollection<User> Users { get; set; } = new List<User>();
}

public enum TenantStatus
{
    Trial,
    Active,
    Suspended,
    Cancelled
}

public enum DatabaseType
{
    Shared,
    Dedicated
}
