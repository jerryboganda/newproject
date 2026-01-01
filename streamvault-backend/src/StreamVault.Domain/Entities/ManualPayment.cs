using System.ComponentModel.DataAnnotations;
using StreamVault.Domain.Interfaces;

namespace StreamVault.Domain.Entities;

public class ManualPayment : ITenantEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TenantId { get; set; }

    public decimal Amount { get; set; }

    [MaxLength(50)]
    public string Currency { get; set; } = "USD";

    public DateTimeOffset PaidAt { get; set; }

    [MaxLength(200)]
    public string? Reference { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Tenant Tenant { get; set; } = null!;
}
