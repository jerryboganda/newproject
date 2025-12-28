using System.ComponentModel.DataAnnotations;
using StreamVault.Domain.Entities;
using StreamVault.Application.Auth.DTOs;

namespace StreamVault.Application.Monetization.DTOs;

public class CreateSubscriptionTierRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    public string Currency { get; set; } = "USD";

    [Required]
    public BillingCycle BillingCycle { get; set; }

    public int SortOrder { get; set; } = 0;

    public List<CreateTierFeatureRequest> Features { get; set; } = new();

    public List<CreateTierLimitRequest> Limits { get; set; } = new();
}

public class UpdateSubscriptionTierRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? Currency { get; set; }

    public BillingCycle? BillingCycle { get; set; }

    public bool? IsActive { get; set; }

    public int? SortOrder { get; set; }

    public List<UpdateTierFeatureRequest>? Features { get; set; }

    public List<UpdateTierLimitRequest>? Limits { get; set; }
}

public class CreateTierFeatureRequest
{
    [Required, MaxLength(200)]
    public string FeatureName { get; set; } = string.Empty;

    public bool IsIncluded { get; set; } = true;

    public string? Description { get; set; }

    public int SortOrder { get; set; } = 0;
}

public class UpdateTierFeatureRequest
{
    [Required]
    public Guid Id { get; set; }

    public string? FeatureName { get; set; }

    public bool? IsIncluded { get; set; }

    public string? Description { get; set; }

    public int? SortOrder { get; set; }
}

public class CreateTierLimitRequest
{
    [Required, MaxLength(100)]
    public string LimitType { get; set; } = string.Empty;

    public int? LimitValue { get; set; }

    public string? Unit { get; set; }
}

public class UpdateTierLimitRequest
{
    [Required]
    public Guid Id { get; set; }

    public string? LimitType { get; set; }

    public int? LimitValue { get; set; }

    public string? Unit { get; set; }
}

public class CreateSubscriptionRequest
{
    public string PaymentMethodId { get; set; } = string.Empty;
    public string? CouponCode { get; set; }
}

public class SubscriptionTierDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public BillingCycle BillingCycle { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public string? StripePriceId { get; set; }
    public List<TierFeatureDto> Features { get; set; } = new();
    public List<TierLimitDto> Limits { get; set; } = new();
    public int SubscriberCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class TierFeatureDto
{
    public Guid Id { get; set; }
    public string FeatureName { get; set; } = string.Empty;
    public bool IsIncluded { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}

public class TierLimitDto
{
    public Guid Id { get; set; }
    public string LimitType { get; set; } = string.Empty;
    public int? LimitValue { get; set; }
    public string? Unit { get; set; }
}

public class UserSubscriptionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SubscriptionTierId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? CurrentPeriodStart { get; set; }
    public DateTimeOffset? CurrentPeriodEnd { get; set; }
    public DateTimeOffset? CancelAtPeriodEnd { get; set; }
    public DateTimeOffset? CanceledAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public BillingCycle BillingCycle { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public SubscriptionTierDto SubscriptionTier { get; set; } = null!;
    public UserDto User { get; set; } = null!;
}

public class SubscriptionAnalyticsDto
{
    public Guid TierId { get; set; }
    public string TierName { get; set; } = string.Empty;
    public int ActiveSubscriptions { get; set; }
    public int NewSubscriptions { get; set; }
    public int CanceledSubscriptions { get; set; }
    public decimal Revenue { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
    public double ChurnRate { get; set; }
    public List<SubscriptionTrendDto> Trends { get; set; } = new();
}

public class SubscriptionTrendDto
{
    public DateOnly Date { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int NewSubscriptions { get; set; }
    public int CanceledSubscriptions { get; set; }
    public decimal DailyRevenue { get; set; }
}

public class SubscriptionComparisonDto
{
    public List<TierComparisonDto> Tiers { get; set; } = new();
    public List<string> AllFeatures { get; set; } = new();
}

public class TierComparisonDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public Dictionary<string, bool> FeatureAvailability { get; set; } = new();
    public Dictionary<string, int?> LimitValues { get; set; } = new();
}
