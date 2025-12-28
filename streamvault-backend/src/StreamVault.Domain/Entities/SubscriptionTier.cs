using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class SubscriptionTier
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "USD";

    public BillingCycle BillingCycle { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; } = 0;

    public string? StripePriceId { get; set; }

    public List<TierFeature> Features { get; set; } = new();

    public List<TierLimit> Limits { get; set; } = new();

    public List<UserSubscription> Subscriptions { get; set; } = new();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class TierFeature
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SubscriptionTierId { get; set; }

    [Required, MaxLength(200)]
    public string FeatureName { get; set; } = string.Empty;

    public bool IsIncluded { get; set; } = true;

    public string? Description { get; set; }

    public int SortOrder { get; set; } = 0;

    // Navigation properties
    public SubscriptionTier SubscriptionTier { get; set; } = null!;
}

public class TierLimit
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SubscriptionTierId { get; set; }

    [Required, MaxLength(100)]
    public string LimitType { get; set; } = string.Empty; // video_upload, storage, bandwidth, etc.

    public int? LimitValue { get; set; }

    public string? Unit { get; set; } // GB, videos, hours, etc.

    // Navigation properties
    public SubscriptionTier SubscriptionTier { get; set; } = null!;
}

public class UserSubscription
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid SubscriptionTierId { get; set; }

    public string? StripeSubscriptionId { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CurrentPeriodStart { get; set; }

    public DateTimeOffset? CurrentPeriodEnd { get; set; }

    public DateTimeOffset? CancelAtPeriodEnd { get; set; }

    public DateTimeOffset? CanceledAt { get; set; }

    public DateTimeOffset? EndedAt { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "USD";

    public BillingCycle BillingCycle { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public SubscriptionTier SubscriptionTier { get; set; } = null!;
}

public enum BillingCycle
{
    Monthly,
    Quarterly,
    Yearly
}

public enum SubscriptionStatus
{
    Active,
    Trialing,
    PastDue,
    Canceled,
    Unpaid,
    Incomplete,
    IncompleteExpired
}
