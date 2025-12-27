using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StreamVault.Domain.Entities;

public class SubscriptionPlan
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PriceMonthly { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PriceYearly { get; set; }

    [MaxLength(100)]
    public string? StripePriceIdMonthly { get; set; }

    [MaxLength(100)]
    public string? StripePriceIdYearly { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsCustom { get; set; } = false; // For enterprise custom plans

    public Dictionary<string, object>? Features { get; set; } // JSONB

    public Dictionary<string, object>? Limits { get; set; } // JSONB

    public int SortOrder { get; set; } = 0;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? UpdatedAt { get; set; }

    // Navigation
    public ICollection<TenantSubscription> Subscriptions { get; set; } = new List<TenantSubscription>();
}
