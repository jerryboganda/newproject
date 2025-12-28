namespace StreamVault.Application.Analytics.DTOs;

public class RevenueOverviewDto
{
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal WeeklyRevenue { get; set; }
    public decimal DailyRevenue { get; set; }
    public decimal PreviousMonthRevenue { get; set; }
    public decimal PreviousWeekRevenue { get; set; }
    public decimal PreviousDayRevenue { get; set; }
    public double MonthlyGrowthRate { get; set; }
    public double WeeklyGrowthRate { get; set; }
    public double DailyGrowthRate { get; set; }
    public int TotalPurchases { get; set; }
    public int TotalRentals { get; set; }
    public int TotalSubscriptions { get; set; }
    public double AverageRevenuePerUser { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal NetRevenue { get; set; }
    public string Currency { get; set; } = "USD";
}

public class RevenueTrendDto
{
    public DateOnly Date { get; set; }
    public decimal Revenue { get; set; }
    public int Purchases { get; set; }
    public int Rentals { get; set; }
    public int Subscriptions { get; set; }
    public decimal AdRevenue { get; set; }
    public string Currency { get; set; } = "USD";
}

public class TopEarningVideoDto
{
    public Guid VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int Purchases { get; set; }
    public int Rentals { get; set; }
    public int Views { get; set; }
    public decimal AverageRevenuePerView { get; set; }
    public string Currency { get; set; } = "USD";
}

public class RevenueBySourceDto
{
    public decimal PurchaseRevenue { get; set; }
    public decimal RentalRevenue { get; set; }
    public decimal SubscriptionRevenue { get; set; }
    public decimal AdRevenue { get; set; }
    public decimal SponsorshipRevenue { get; set; }
    public List<SourceBreakdownDto> Breakdown { get; set; } = new();
    public string Currency { get; set; } = "USD";
}

public class SourceBreakdownDto
{
    public string Source { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public double Percentage { get; set; }
    public int Count { get; set; }
}

public class RevenueByCountryDto
{
    public List<CountryRevenueDto> Countries { get; set; } = new();
    public string Currency { get; set; } = "USD";
}

public class CountryRevenueDto
{
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Users { get; set; }
    public double Percentage { get; set; }
}

public class SubscriberAnalyticsDto
{
    public int TotalSubscribers { get; set; }
    public int ActiveSubscribers { get; set; }
    public int NewSubscribers { get; set; }
    public int ChurnedSubscribers { get; set; }
    public double ChurnRate { get; set; }
    public double MonthlyRecurringRevenue { get; set; }
    public double AverageRevenuePerSubscriber { get; set; }
    public List<SubscriberTrendDto> Trends { get; set; } = new();
    public List<TierDistributionDto> TierDistribution { get; set; } = new();
    public string Currency { get; set; } = "USD";
}

public class SubscriberTrendDto
{
    public DateOnly Date { get; set; }
    public int NewSubscribers { get; set; }
    public int ChurnedSubscribers { get; set; }
    public int ActiveSubscribers { get; set; }
}

public class TierDistributionDto
{
    public string TierName { get; set; } = string.Empty;
    public int SubscriberCount { get; set; }
    public decimal Revenue { get; set; }
    public double Percentage { get; set; }
}

public class RevenueForecastDto
{
    public List<ForecastDataPointDto> Forecast { get; set; } = new();
    public decimal TotalForecastedRevenue { get; set; }
    public double ConfidenceLevel { get; set; }
    public string Currency { get; set; } = "USD";
}

public class ForecastDataPointDto
{
    public DateOnly Date { get; set; }
    public decimal ForecastedRevenue { get; set; }
    public decimal LowerBound { get; set; }
    public decimal UpperBound { get; set; }
}

public class MonthlyRevenueReportDto
{
    public DateOnly Month { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PurchaseRevenue { get; set; }
    public decimal RentalRevenue { get; set; }
    public decimal SubscriptionRevenue { get; set; }
    public decimal AdRevenue { get; set; }
    public decimal SponsorshipRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public int Purchases { get; set; }
    public int Rentals { get; set; }
    public int ActiveSubscriptions { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal NetRevenue { get; set; }
    public string Currency { get; set; } = "USD";
}

public class RevenueMetricsDto
{
    public decimal LifetimeValue { get; set; }
    public decimal CustomerAcquisitionCost { get; set; }
    public double PaybackPeriod { get; set; } // in months
    public double AverageOrderValue { get; set; }
    public double ConversionRate { get; set; }
    public double RevenuePerThousandViews { get; set; }
    public double SubscriberLifetimeValue { get; set; }
    public double MonthlyChurnRate { get; set; }
    public string Currency { get; set; } = "USD";
}
