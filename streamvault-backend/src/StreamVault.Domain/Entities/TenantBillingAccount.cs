using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StreamVault.Domain.Interfaces;

namespace StreamVault.Domain.Entities;

public class TenantBillingAccount : ITenantEntity
{
    [Key]
    [ForeignKey(nameof(Tenant))]
    public Guid TenantId { get; set; }

    [MaxLength(100)]
    public string? StripeCustomerId { get; set; }

    [MaxLength(100)]
    public string? StripeSubscriptionId { get; set; }

    [MaxLength(100)]
    public string? StripeDefaultPaymentMethodId { get; set; }

    [MaxLength(50)]
    public string Currency { get; set; } = "USD";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Tenant Tenant { get; set; } = null!;
}
