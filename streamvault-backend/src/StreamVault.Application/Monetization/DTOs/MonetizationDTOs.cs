using System.ComponentModel.DataAnnotations;
using StreamVault.Domain.Entities;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Application.Videos.DTOs;

namespace StreamVault.Application.Monetization.DTOs;

public class UpdateVideoMonetizationRequest
{
    public MonetizationType MonetizationType { get; set; }

    public decimal? Price { get; set; }

    public string? Currency { get; set; } = "USD";

    public bool? IsActive { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }

    public string? SubscriptionTierRequired { get; set; }

    public bool? AllowRental { get; set; }

    public decimal? RentalPrice { get; set; }

    public int? RentalPeriodHours { get; set; }

    public bool? AllowPurchase { get; set; }

    public decimal? PurchasePrice { get; set; }

    public bool? EnableAdSupport { get; set; }

    public double? AdRevenueSharePercentage { get; set; }

    public string? SponsorshipDetails { get; set; }
}

public class EnableMonetizationRequest
{
    [Required]
    public MonetizationType MonetizationType { get; set; }

    public decimal? Price { get; set; }

    public string? Currency { get; set; } = "USD";

    public bool AllowRental { get; set; } = false;

    public decimal? RentalPrice { get; set; }

    public int RentalPeriodHours { get; set; } = 24;

    public bool AllowPurchase { get; set; } = false;

    public decimal? PurchasePrice { get; set; }

    public bool EnableAdSupport { get; set; } = false;

    public double AdRevenueSharePercentage { get; set; } = 55;

    public string? SubscriptionTierRequired { get; set; }
}

public class PurchaseVideoRequest
{
    public string PaymentMethodId { get; set; } = string.Empty;
    public string? CouponCode { get; set; }
}

public class RentVideoRequest
{
    public string PaymentMethodId { get; set; } = string.Empty;
    public string? CouponCode { get; set; }
}

public class RecordAdRevenueRequest
{
    [Required]
    public Guid VideoId { get; set; }

    [Required]
    public decimal Revenue { get; set; }

    public string Currency { get; set; } = "USD";

    [Required]
    public int Impressions { get; set; }

    [Required]
    public int Clicks { get; set; }

    [Required]
    public double CPM { get; set; }

    public string? AdNetwork { get; set; }

    public string? AdType { get; set; }

    public DateOnly? Date { get; set; }
}

public class CreateSponsorshipRequest
{
    [Required]
    public Guid SponsorId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public string Currency { get; set; } = "USD";

    [Required]
    public string SponsorName { get; set; } = string.Empty;

    public string? SponsorLogo { get; set; }

    public string? SponsorUrl { get; set; }

    public int DisplayDurationSeconds { get; set; } = 30;

    public int DisplayPosition { get; set; } = 0;

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }
}

public class VideoMonetizationDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public MonetizationType MonetizationType { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? SubscriptionTierRequired { get; set; }
    public bool AllowRental { get; set; }
    public decimal? RentalPrice { get; set; }
    public int RentalPeriodHours { get; set; }
    public bool AllowPurchase { get; set; }
    public decimal? PurchasePrice { get; set; }
    public bool EnableAdSupport { get; set; }
    public double AdRevenueSharePercentage { get; set; }
    public string? SponsorshipDetails { get; set; }
    public List<MonetizationRuleDto> Rules { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class MonetizationRuleDto
{
    public Guid Id { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public string RuleValue { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class VideoPurchaseDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public Guid UserId { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentIntentId { get; set; } = string.Empty;
    public string? ReceiptUrl { get; set; }
    public DateTimeOffset PurchasedAt { get; set; }
    public bool IsRefunded { get; set; }
    public DateTimeOffset? RefundedAt { get; set; }
    public string? RefundReason { get; set; }
    public VideoDto Video { get; set; } = null!;
    public UserDto User { get; set; } = null!;
}

public class VideoRentalDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public Guid UserId { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentIntentId { get; set; } = string.Empty;
    public DateTimeOffset RentedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsExpired { get; set; }
    public bool IsActive { get; set; }
    public VideoDto Video { get; set; } = null!;
    public UserDto User { get; set; } = null!;
}

public class RevenueDto
{
    public Guid VideoId { get; set; }
    public string VideoTitle { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public decimal PurchaseRevenue { get; set; }
    public decimal RentalRevenue { get; set; }
    public decimal AdRevenue { get; set; }
    public decimal SponsorshipRevenue { get; set; }
    public int TotalPurchases { get; set; }
    public int TotalRentals { get; set; }
    public int TotalViews { get; set; }
    public string Currency { get; set; } = "USD";
    public DateOnly Period { get; set; }
}

public class AdRevenueDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public decimal Revenue { get; set; }
    public string Currency { get; set; } = "USD";
    public int Impressions { get; set; }
    public int Clicks { get; set; }
    public double CTR { get; set; }
    public double CPM { get; set; }
    public DateOnly Date { get; set; }
    public string? AdNetwork { get; set; }
    public string? AdType { get; set; }
}

public class SponsorshipDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public Guid SponsorId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string SponsorName { get; set; } = string.Empty;
    public string? SponsorLogo { get; set; }
    public string? SponsorUrl { get; set; }
    public int DisplayDurationSeconds { get; set; }
    public int DisplayPosition { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class MonetizationSummaryDto
{
    public Guid VideoId { get; set; }
    public string VideoTitle { get; set; } = string.Empty;
    public bool IsMonetized { get; set; }
    public MonetizationType MonetizationType { get; set; }
    public decimal? Price { get; set; }
    public bool AllowPurchase { get; set; }
    public bool AllowRental { get; set; }
    public bool EnableAdSupport { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalPurchases { get; set; }
    public int TotalRentals { get; set; }
    public decimal AverageRevenuePerView { get; set; }
}

public class CreatorRevenueSummaryDto
{
    public Guid UserId { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal PendingPayouts { get; set; }
    public decimal LifetimeEarnings { get; set; }
    public int MonetizedVideos { get; set; }
    public int TotalPurchases { get; set; }
    public int TotalRentals { get; set; }
    public int TotalViews { get; set; }
    public List<RevenueDto> MonthlyRevenue { get; set; } = new();
    public List<RevenueDto> TopEarningVideos { get; set; } = new();
}
