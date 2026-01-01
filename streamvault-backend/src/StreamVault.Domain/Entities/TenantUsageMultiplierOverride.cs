using System.ComponentModel.DataAnnotations;
using StreamVault.Domain.Interfaces;

namespace StreamVault.Domain.Entities;

public class TenantUsageMultiplierOverride : ITenantEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TenantId { get; set; }

    public MetricType MetricType { get; set; }

    public double Multiplier { get; set; } = 1.0;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Tenant Tenant { get; set; } = null!;
}
