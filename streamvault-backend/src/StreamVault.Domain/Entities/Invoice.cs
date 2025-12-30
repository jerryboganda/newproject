using StreamVault.Domain.Interfaces;

namespace StreamVault.Domain.Entities;

public class Invoice : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string? StripeInvoiceId { get; set; }
    public string Status { get; set; } = "draft";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
