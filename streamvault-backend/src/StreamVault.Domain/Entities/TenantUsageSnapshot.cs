using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StreamVault.Domain.Interfaces;

namespace StreamVault.Domain.Entities;

public class TenantUsageSnapshot : ITenantEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TenantId { get; set; }

    public DateTimeOffset PeriodStartUtc { get; set; }

    public long StorageBytes { get; set; }

    public long BandwidthBytes { get; set; }

    public int VideoCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Tenant Tenant { get; set; } = null!;
}
