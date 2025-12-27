using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StreamVault.Domain.Entities;

public class TenantSubscription
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, ForeignKey("Tenant")]
    public Guid TenantId { get; set; }

    [Required, ForeignKey("Plan")]
    public Guid PlanId { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trialing;

    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;

    [MaxLength(100)]
    public string? StripeSubscriptionId { get; set; }

    [MaxLength(100)]
    public string? StripeCustomerId { get; set; }

    public DateTimeOffset? CurrentPeriodStart { get; set; }

    public DateTimeOffset? CurrentPeriodEnd { get; set; }

    public DateTimeOffset? CancelAt { get; set; }

    public DateTimeOffset? TrialEnd { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;

    public SubscriptionPlan Plan { get; set; } = null!;
}

public enum SubscriptionStatus
{
    Trialing,
    Active,
    PastDue,
    Cancelled,
    Paused
}

public enum BillingCycle
{
    Monthly,
    Yearly,
    Custom
}
