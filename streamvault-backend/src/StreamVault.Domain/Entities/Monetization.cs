using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class VideoMonetization
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VideoId { get; set; }

    public MonetizationType MonetizationType { get; set; }

    public decimal? Price { get; set; }

    public string? Currency { get; set; } = "USD";

    public bool IsActive { get; set; } = true;

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }

    public string? SubscriptionTierRequired { get; set; }

    public bool AllowRental { get; set; } = false;

    public decimal? RentalPrice { get; set; }

    public int RentalPeriodHours { get; set; } = 24;

    public bool AllowPurchase { get; set; } = false;

    public decimal? PurchasePrice { get; set; }

    public bool EnableAdSupport { get; set; } = false;

    public double AdRevenueSharePercentage { get; set; } = 55; // Creator gets 55%

    public string? SponsorshipDetails { get; set; }

    public List<MonetizationRule> Rules { get; set; } = new();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public Video Video { get; set; } = null!;
    public List<VideoPurchase> Purchases { get; set; } = new();
    public List<VideoRental> Rentals { get; set; } = new();
    public List<AdRevenue> AdRevenues { get; set; } = new();
}

public class MonetizationRule
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VideoMonetizationId { get; set; }

    public string RuleType { get; set; } = string.Empty; // geo_restriction, age_restriction, etc.

    public string RuleValue { get; set; } = string.Empty; // JSON or string value

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public VideoMonetization VideoMonetization { get; set; } = null!;
}

public class VideoPurchase
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VideoId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "USD";

    public string PaymentIntentId { get; set; } = string.Empty;

    public string? ReceiptUrl { get; set; }

    public DateTimeOffset PurchasedAt { get; set; } = DateTimeOffset.UtcNow;

    public bool IsRefunded { get; set; } = false;

    public DateTimeOffset? RefundedAt { get; set; }

    public string? RefundReason { get; set; }

    // Navigation properties
    public Video Video { get; set; } = null!;
    public User User { get; set; } = null!;
}

public class VideoRental
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VideoId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "USD";

    public string PaymentIntentId { get; set; } = string.Empty;

    public DateTimeOffset RentedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Video Video { get; set; } = null!;
    public User User { get; set; } = null!;
}

public class AdRevenue
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VideoId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public decimal Revenue { get; set; }

    public string Currency { get; set; } = "USD";

    public int Impressions { get; set; }

    public int Clicks { get; set; }

    public double CTR { get; set; } // Click-through rate

    public double CPM { get; set; } // Cost per mille

    public DateOnly Date { get; set; }

    public string? AdNetwork { get; set; }

    public string? AdType { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public Video Video { get; set; } = null!;
    public User User { get; set; } = null!;
}

public class CreatorPayout
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    public decimal TotalRevenue { get; set; }

    public decimal PlatformFee { get; set; }

    public decimal NetAmount { get; set; }

    public string Currency { get; set; } = "USD";

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public PayoutStatus Status { get; set; } = PayoutStatus.Pending;

    public string? TransactionId { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
}

public class Sponsorship
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VideoId { get; set; }

    [Required]
    public Guid SponsorId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "USD";

    public string SponsorName { get; set; } = string.Empty;

    public string? SponsorLogo { get; set; }

    public string? SponsorUrl { get; set; }

    public int DisplayDurationSeconds { get; set; }

    public int DisplayPosition { get; set; } // 0 = preroll, 1 = midroll, 2 = postroll

    public bool IsActive { get; set; } = true;

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public Video Video { get; set; } = null!;
    public User Sponsor { get; set; } = null!;
}

public enum MonetizationType
{
    Free,
    Subscription,
    PayPerView,
    Rental,
    Hybrid
}

public enum PayoutStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}
