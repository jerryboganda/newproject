using System.ComponentModel.DataAnnotations;

namespace StreamVault.Application.Subscriptions.DTOs;

public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string BillingInterval { get; set; } = string.Empty; // monthly, yearly
    public Dictionary<string, object> Features { get; set; } = new();
    public Dictionary<string, long> Limits { get; set; } = new();
    public bool IsActive { get; set; }
    public string StripePriceId { get; set; } = string.Empty;
}

public class SubscriptionDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // active, canceled, past_due, etc.
    public DateTimeOffset CurrentPeriodStart { get; set; }
    public DateTimeOffset CurrentPeriodEnd { get; set; }
    public DateTimeOffset? CanceledAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public bool AutoRenew { get; set; }
    public Dictionary<string, long> Usage { get; set; } = new();
    public Dictionary<string, long> Limits { get; set; } = new();
}

public class SubscribeRequest
{
    [Required]
    public Guid PlanId { get; set; }
    
    public string PaymentMethodId { get; set; } = string.Empty;
    
    public bool AutoRenew { get; set; } = true;
    
    public string BillingCycle { get; set; } = "monthly"; // monthly or yearly
}

public class UsageDto
{
    public string Feature { get; set; } = string.Empty;
    public long Used { get; set; }
    public long Limit { get; set; }
    public double PercentageUsed => Limit > 0 ? (double)Used / Limit * 100 : 0;
}
