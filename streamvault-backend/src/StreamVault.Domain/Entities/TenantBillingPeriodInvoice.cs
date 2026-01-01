using System.ComponentModel.DataAnnotations;
using StreamVault.Domain.Interfaces;

namespace StreamVault.Domain.Entities;

public class TenantBillingPeriodInvoice : ITenantEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TenantId { get; set; }

    public DateTimeOffset PeriodStartUtc { get; set; }

    public DateTimeOffset PeriodEndUtc { get; set; }

    [MaxLength(100)]
    public string StripeInvoiceId { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Tenant Tenant { get; set; } = null!;
}
