using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Analytics.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Analytics;

public class RevenueAnalyticsService : IRevenueAnalyticsService
{
    private readonly StreamVaultDbContext _dbContext;

    public RevenueAnalyticsService(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RevenueOverviewDto> GetRevenueOverviewAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var end = endDate ?? DateTimeOffset.UtcNow;
        var start = startDate ?? end.AddDays(-30);

        // Get user's videos
        var videoIds = await _dbContext.Videos
            .Where(v => v.UserId == userId)
            .Select(v => v.Id)
            .ToListAsync();

        // Calculate revenue metrics
        var purchases = await _dbContext.VideoPurchases
            .Where(vp => videoIds.Contains(vp.VideoId) && 
                        vp.PurchasedAt >= start && vp.PurchasedAt <= end && !vp.IsRefunded)
            .ToListAsync();

        var rentals = await _dbContext.VideoRentals
            .Where(vr => videoIds.Contains(vr.VideoId) && 
                        vr.RentedAt >= start && vr.RentedAt <= end)
            .ToListAsync();

        var adRevenues = await _dbContext.AdRevenues
            .Where(ar => videoIds.Contains(ar.VideoId) && 
                        ar.CreatedAt >= start && ar.CreatedAt <= end)
            .ToListAsync();

        var sponsorships = await _dbContext.Sponsorships
            .Where(s => videoIds.Contains(s.VideoId) && 
                        s.StartDate >= start && s.StartDate <= end)
            .ToListAsync();

        var subscriptions = await _dbContext.UserSubscriptions
            .Where(us => us.UserId == userId && 
                        us.Status == SubscriptionStatus.Active &&
                        us.CreatedAt >= start && us.CreatedAt <= end)
            .ToListAsync();

        // Calculate previous period for growth rates
        var previousStart = start.AddDays(-30);
        var previousEnd = start;

        var previousPurchases = await _dbContext.VideoPurchases
            .Where(vp => videoIds.Contains(vp.VideoId) && 
                        vp.PurchasedAt >= previousStart && vp.PurchasedAt <= previousEnd && !vp.IsRefunded)
            .SumAsync(vp => vp.Price);

        var previousRentals = await _dbContext.VideoRentals
            .Where(vr => videoIds.Contains(vr.VideoId) && 
                        vr.RentedAt >= previousStart && vr.RentedAt <= previousEnd)
            .SumAsync(vr => vr.Price);

        var previousSubscriptions = await _dbContext.UserSubscriptions
            .Where(us => us.UserId == userId && 
                        us.Status == SubscriptionStatus.Active &&
                        us.CreatedAt >= previousStart && us.CreatedAt <= previousEnd)
            .SumAsync(us => us.Price);

        var previousAdRevenues = await _dbContext.AdRevenues
            .Where(ar => videoIds.Contains(ar.VideoId) && 
                        ar.CreatedAt >= previousStart && ar.CreatedAt <= previousEnd)
            .SumAsync(ar => ar.Revenue);

        var previousSponsorships = await _dbContext.Sponsorships
            .Where(s => videoIds.Contains(s.VideoId) && 
                        s.StartDate >= previousStart && s.StartDate <= previousEnd)
            .SumAsync(s => s.Amount);

        // Calculate totals
        var totalRevenue = purchases.Sum(p => p.Price) + 
                          rentals.Sum(r => r.Price) + 
                          adRevenues.Sum(ar => ar.Revenue) + 
                          sponsorships.Sum(s => s.Amount) + 
                          subscriptions.Sum(s => s.Price);

        var previousTotalRevenue = previousPurchases + previousRentals + 
                                  previousSubscriptions + previousAdRevenues + previousSponsorships;

        // Calculate growth rates
        var monthlyGrowthRate = previousTotalRevenue > 0 ? 
            ((totalRevenue - previousTotalRevenue) / previousTotalRevenue) * 100 : 0;

        // Calculate platform fees (assuming 10% platform fee)
        var platformFee = totalRevenue * 0.10m;
        var netRevenue = totalRevenue - platformFee;

        return new RevenueOverviewDto
        {
            TotalRevenue = totalRevenue,
            MonthlyRevenue = totalRevenue,
            WeeklyRevenue = totalRevenue / 4, // Approximation
            DailyRevenue = totalRevenue / 30, // Approximation
            PreviousMonthRevenue = previousTotalRevenue,
            PreviousWeekRevenue = previousTotalRevenue / 4,
            PreviousDayRevenue = previousTotalRevenue / 30,
            MonthlyGrowthRate = monthlyGrowthRate,
            WeeklyGrowthRate = monthlyGrowthRate / 4, // Approximation
            DailyGrowthRate = monthlyGrowthRate / 30, // Approximation
            TotalPurchases = purchases.Count,
            TotalRentals = rentals.Count,
            TotalSubscriptions = subscriptions.Count,
            AverageRevenuePerUser = subscriptions.Count > 0 ? (double)(totalRevenue / subscriptions.Count) : 0,
            PlatformFee = platformFee,
            NetRevenue = netRevenue,
            Currency = "USD"
        };
    }

    public async Task<List<RevenueTrendDto>> GetRevenueTrendsAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, string? period = null)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var end = endDate ?? DateTimeOffset.UtcNow;
        var start = startDate ?? end.AddDays(-30);

        // Get user's videos
        var videoIds = await _dbContext.Videos
            .Where(v => v.UserId == userId)
            .Select(v => v.Id)
            .ToListAsync();

        var trends = new List<RevenueTrendDto>();
        var current = start.Date;

        while (current <= end.Date)
        {
            var dayStart = new DateTimeOffset(current, TimeSpan.Zero);
            var dayEnd = dayStart.AddDays(1);

            var dayPurchases = await _dbContext.VideoPurchases
                .Where(vp => videoIds.Contains(vp.VideoId) && 
                            vp.PurchasedAt >= dayStart && vp.PurchasedAt < dayEnd && !vp.IsRefunded)
                .SumAsync(vp => vp.Price);

            var dayRentals = await _dbContext.VideoRentals
                .Where(vr => videoIds.Contains(vr.VideoId) && 
                            vr.RentedAt >= dayStart && vr.RentedAt < dayEnd)
                .SumAsync(vr => vr.Price);

            var dayAdRevenue = await _dbContext.AdRevenues
                .Where(ar => videoIds.Contains(ar.VideoId) && 
                            ar.CreatedAt >= dayStart && ar.CreatedAt < dayEnd)
                .SumAsync(ar => ar.Revenue);

            var daySubscriptions = await _dbContext.UserSubscriptions
                .Where(us => us.UserId == userId && 
                            us.Status == SubscriptionStatus.Active &&
                            us.CreatedAt >= dayStart && us.CreatedAt < dayEnd)
                .SumAsync(us => us.Price);

            var dayRevenue = dayPurchases + dayRentals + dayAdRevenue + daySubscriptions;

            trends.Add(new RevenueTrendDto
            {
                Date = DateOnly.FromDateTime(current),
                Revenue = dayRevenue,
                Purchases = await _dbContext.VideoPurchases
                    .CountAsync(vp => videoIds.Contains(vp.VideoId) && 
                                    vp.PurchasedAt >= dayStart && vp.PurchasedAt < dayEnd && !vp.IsRefunded),
                Rentals = await _dbContext.VideoRentals
                    .CountAsync(vr => videoIds.Contains(vr.VideoId) && 
                                    vr.RentedAt >= dayStart && vr.RentedAt < dayEnd),
                Subscriptions = await _dbContext.UserSubscriptions
                    .CountAsync(us => us.UserId == userId && 
                                    us.Status == SubscriptionStatus.Active &&
                                    us.CreatedAt >= dayStart && us.CreatedAt < dayEnd),
                AdRevenue = dayAdRevenue,
                Currency = "USD"
            });

            current = current.AddDays(1);
        }

        return trends;
    }

    public async Task<List<TopEarningVideoDto>> GetTopEarningVideosAsync(Guid userId, Guid tenantId, int limit = 10, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var end = endDate ?? DateTimeOffset.UtcNow;
        var start = startDate ?? end.AddDays(-30);

        // Get user's videos with revenue
        var videos = await _dbContext.Videos
            .Where(v => v.UserId == userId)
            .Select(v => new
            {
                v.Id,
                v.Title,
                v.ThumbnailPath,
                Purchases = _dbContext.VideoPurchases
                    .Where(vp => vp.VideoId == v.Id && vp.PurchasedAt >= start && vp.PurchasedAt <= end && !vp.IsRefunded)
                    .Sum(vp => vp.Price),
                RentalCount = _dbContext.VideoRentals
                    .Count(vr => vr.VideoId == v.Id && vr.RentedAt >= start && vr.RentedAt <= end),
                RentalRevenue = _dbContext.VideoRentals
                    .Where(vr => vr.VideoId == v.Id && vr.RentedAt >= start && vr.RentedAt <= end)
                    .Sum(vr => vr.Price),
                AdRevenue = _dbContext.AdRevenues
                    .Where(ar => ar.VideoId == v.Id && ar.CreatedAt >= start && ar.CreatedAt <= end)
                    .Sum(ar => ar.Revenue),
                ViewCount = _dbContext.VideoAnalytics
                    .Count(va => va.VideoId == v.Id && va.Timestamp >= start && va.Timestamp <= end)
            })
            .OrderByDescending(v => v.Purchases + v.RentalRevenue + v.AdRevenue)
            .Take(limit)
            .ToListAsync();

        return videos.Select(v => new TopEarningVideoDto
        {
            VideoId = v.Id,
            Title = v.Title,
            ThumbnailUrl = v.ThumbnailPath,
            TotalRevenue = v.Purchases + v.RentalRevenue + v.AdRevenue,
            Purchases = (int)(v.Purchases > 0 ? v.Purchases / (v.Purchases + v.RentalRevenue + v.AdRevenue) * v.Purchases : 0),
            Rentals = v.RentalCount,
            Views = v.ViewCount,
            AverageRevenuePerView = v.ViewCount > 0 ? (double)(v.Purchases + v.RentalRevenue + v.AdRevenue) / v.ViewCount : 0,
            Currency = "USD"
        }).ToList();
    }

    public async Task<RevenueBySourceDto> GetRevenueBySourceAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var end = endDate ?? DateTimeOffset.UtcNow;
        var start = startDate ?? end.AddDays(-30);

        // Get user's videos
        var videoIds = await _dbContext.Videos
            .Where(v => v.UserId == userId)
            .Select(v => v.Id)
            .ToListAsync();

        // Calculate revenue by source
        var purchaseRevenue = await _dbContext.VideoPurchases
            .Where(vp => videoIds.Contains(vp.VideoId) && 
                        vp.PurchasedAt >= start && vp.PurchasedAt <= end && !vp.IsRefunded)
            .SumAsync(vp => vp.Price);

        var rentalRevenue = await _dbContext.VideoRentals
            .Where(vr => videoIds.Contains(vr.VideoId) && 
                        vr.RentedAt >= start && vr.RentedAt <= end)
            .SumAsync(vr => vr.Price);

        var subscriptionRevenue = await _dbContext.UserSubscriptions
            .Where(us => us.UserId == userId && 
                        us.Status == SubscriptionStatus.Active &&
                        us.CreatedAt >= start && us.CreatedAt <= end)
            .SumAsync(us => us.Price);

        var adRevenue = await _dbContext.AdRevenues
            .Where(ar => videoIds.Contains(ar.VideoId) && 
                        ar.CreatedAt >= start && ar.CreatedAt <= end)
            .SumAsync(ar => ar.Revenue);

        var sponsorshipRevenue = await _dbContext.Sponsorships
            .Where(s => videoIds.Contains(s.VideoId) && 
                        s.StartDate >= start && s.StartDate <= end)
            .SumAsync(s => s.Amount);

        var totalRevenue = purchaseRevenue + rentalRevenue + subscriptionRevenue + adRevenue + sponsorshipRevenue;

        return new RevenueBySourceDto
        {
            PurchaseRevenue = purchaseRevenue,
            RentalRevenue = rentalRevenue,
            SubscriptionRevenue = subscriptionRevenue,
            AdRevenue = adRevenue,
            SponsorshipRevenue = sponsorshipRevenue,
            Breakdown = new List<SourceBreakdownDto>
            {
                new() { Source = "Purchases", Revenue = purchaseRevenue, Percentage = totalRevenue > 0 ? (double)(purchaseRevenue / totalRevenue) * 100 : 0, Count = await _dbContext.VideoPurchases.CountAsync(vp => videoIds.Contains(vp.VideoId) && vp.PurchasedAt >= start && vp.PurchasedAt <= end && !vp.IsRefunded) },
                new() { Source = "Rentals", Revenue = rentalRevenue, Percentage = totalRevenue > 0 ? (double)(rentalRevenue / totalRevenue) * 100 : 0, Count = await _dbContext.VideoRentals.CountAsync(vr => videoIds.Contains(vr.VideoId) && vr.RentedAt >= start && vr.RentedAt <= end) },
                new() { Source = "Subscriptions", Revenue = subscriptionRevenue, Percentage = totalRevenue > 0 ? (double)(subscriptionRevenue / totalRevenue) * 100 : 0, Count = await _dbContext.UserSubscriptions.CountAsync(us => us.UserId == userId && us.Status == SubscriptionStatus.Active) },
                new() { Source = "Advertisements", Revenue = adRevenue, Percentage = totalRevenue > 0 ? (double)(adRevenue / totalRevenue) * 100 : 0, Count = await _dbContext.AdRevenues.CountAsync(ar => videoIds.Contains(ar.VideoId) && ar.CreatedAt >= start && ar.CreatedAt <= end) },
                new() { Source = "Sponsorships", Revenue = sponsorshipRevenue, Percentage = totalRevenue > 0 ? (double)(sponsorshipRevenue / totalRevenue) * 100 : 0, Count = await _dbContext.Sponsorships.CountAsync(s => videoIds.Contains(s.VideoId) && s.StartDate >= start && s.StartDate <= end) }
            },
            Currency = "USD"
        };
    }

    public async Task<RevenueByCountryDto> GetRevenueByCountryAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        // This would require country data in purchases/rentals
        // For now, return empty data
        return new RevenueByCountryDto
        {
            Countries = new List<CountryRevenueDto>(),
            Currency = "USD"
        };
    }

    public async Task<SubscriberAnalyticsDto> GetSubscriberAnalyticsAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var end = endDate ?? DateTimeOffset.UtcNow;
        var start = startDate ?? end.AddDays(-30);

        // Get subscriber data
        var totalSubscribers = await _dbContext.UserSubscriptions
            .CountAsync(us => us.UserId == userId);

        var activeSubscribers = await _dbContext.UserSubscriptions
            .CountAsync(us => us.UserId == userId && us.Status == SubscriptionStatus.Active);

        var newSubscribers = await _dbContext.UserSubscriptions
            .CountAsync(us => us.UserId == userId && us.CreatedAt >= start && us.CreatedAt <= end);

        var churnedSubscribers = await _dbContext.UserSubscriptions
            .CountAsync(us => us.UserId == userId && 
                        us.Status == SubscriptionStatus.Canceled &&
                        us.CanceledAt >= start && us.CanceledAt <= end);

        var churnRate = activeSubscribers > 0 ? (double)churnedSubscribers / activeSubscribers * 100 : 0;

        var monthlyRecurringRevenue = await _dbContext.UserSubscriptions
            .Where(us => us.UserId == userId && us.Status == SubscriptionStatus.Active)
            .SumAsync(us => us.Price);

        var averageRevenuePerSubscriber = activeSubscribers > 0 ? (double)monthlyRecurringRevenue / activeSubscribers : 0;

        return new SubscriberAnalyticsDto
        {
            TotalSubscribers = totalSubscribers,
            ActiveSubscribers = activeSubscribers,
            NewSubscribers = newSubscribers,
            ChurnedSubscribers = churnedSubscribers,
            ChurnRate = churnRate,
            MonthlyRecurringRevenue = monthlyRecurringRevenue,
            AverageRevenuePerSubscriber = averageRevenuePerSubscriber,
            Trends = new List<SubscriberTrendDto>(),
            TierDistribution = new List<TierDistributionDto>(),
            Currency = "USD"
        };
    }

    public async Task<RevenueForecastDto> GetRevenueForecastAsync(Guid userId, Guid tenantId, int months = 6)
    {
        // Simple linear regression based forecast
        var forecast = new List<ForecastDataPointDto>();
        var current = DateOnly.FromDateTime(DateTime.UtcNow);
        
        for (int i = 1; i <= months; i++)
        {
            var forecastDate = current.AddMonths(i);
            var forecastedRevenue = 1000 * i; // Simple placeholder calculation
            
            forecast.Add(new ForecastDataPointDto
            {
                Date = forecastDate,
                ForecastedRevenue = forecastedRevenue,
                LowerBound = forecastedRevenue * 0.8m,
                UpperBound = forecastedRevenue * 1.2m
            });
        }

        return new RevenueForecastDto
        {
            Forecast = forecast,
            TotalForecastedRevenue = forecast.Sum(f => f.ForecastedRevenue),
            ConfidenceLevel = 0.85,
            Currency = "USD"
        };
    }

    public async Task<List<MonthlyRevenueReportDto>> GetMonthlyRevenueReportAsync(Guid userId, Guid tenantId, int months = 12)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var reports = new List<MonthlyRevenueReportDto>();
        var current = DateOnly.FromDateTime(DateTime.UtcNow);

        for (int i = months - 1; i >= 0; i--)
        {
            var month = current.AddMonths(-i);
            var monthStart = new DateTimeOffset(month.Year, month.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var monthEnd = monthStart.AddMonths(1);

            // Get user's videos
            var videoIds = await _dbContext.Videos
                .Where(v => v.UserId == userId)
                .Select(v => v.Id)
                .ToListAsync();

            var purchaseRevenue = await _dbContext.VideoPurchases
                .Where(vp => videoIds.Contains(vp.VideoId) && 
                            vp.PurchasedAt >= monthStart && vp.PurchasedAt < monthEnd && !vp.IsRefunded)
                .SumAsync(vp => vp.Price);

            var rentalRevenue = await _dbContext.VideoRentals
                .Where(vr => videoIds.Contains(vr.VideoId) && 
                            vr.RentedAt >= monthStart && vr.RentedAt < monthEnd)
                .SumAsync(vr => vr.Price);

            var subscriptionRevenue = await _dbContext.UserSubscriptions
                .Where(us => us.UserId == userId && 
                        us.Status == SubscriptionStatus.Active &&
                        us.CreatedAt >= monthStart && us.CreatedAt < monthEnd)
                .SumAsync(us => us.Price);

            var adRevenue = await _dbContext.AdRevenues
                .Where(ar => videoIds.Contains(ar.VideoId) && 
                            ar.CreatedAt >= monthStart && ar.CreatedAt < monthEnd)
                .SumAsync(ar => ar.Revenue);

            var sponsorshipRevenue = await _dbContext.Sponsorships
                .Where(s => videoIds.Contains(s.VideoId) && 
                            s.StartDate >= monthStart && s.StartDate < monthEnd)
                .SumAsync(s => s.Amount);

            var totalRevenue = purchaseRevenue + rentalRevenue + subscriptionRevenue + adRevenue + sponsorshipRevenue;
            var platformFee = totalRevenue * 0.10m;
            var netRevenue = totalRevenue - platformFee;

            reports.Add(new MonthlyRevenueReportDto
            {
                Month = month,
                TotalRevenue = totalRevenue,
                PurchaseRevenue = purchaseRevenue,
                RentalRevenue = rentalRevenue,
                SubscriptionRevenue = subscriptionRevenue,
                AdRevenue = adRevenue,
                SponsorshipRevenue = sponsorshipRevenue,
                TotalTransactions = await _dbContext.VideoPurchases.CountAsync(vp => videoIds.Contains(vp.VideoId) && vp.PurchasedAt >= monthStart && vp.PurchasedAt < monthEnd && !vp.IsRefunded) +
                                      await _dbContext.VideoRentals.CountAsync(vr => videoIds.Contains(vr.VideoId) && vr.RentedAt >= monthStart && vr.RentedAt < monthEnd),
                Purchases = await _dbContext.VideoPurchases.CountAsync(vp => videoIds.Contains(vp.VideoId) && vp.PurchasedAt >= monthStart && vp.PurchasedAt < monthEnd && !vp.IsRefunded),
                Rentals = await _dbContext.VideoRentals.CountAsync(vr => videoIds.Contains(vr.VideoId) && vr.RentedAt >= monthStart && vr.RentedAt < monthEnd),
                ActiveSubscriptions = await _dbContext.UserSubscriptions.CountAsync(us => us.UserId == userId && us.Status == SubscriptionStatus.Active),
                PlatformFee = platformFee,
                NetRevenue = netRevenue,
                Currency = "USD"
            });
        }

        return reports;
    }

    public async Task<RevenueMetricsDto> GetRevenueMetricsAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var end = endDate ?? DateTimeOffset.UtcNow;
        var start = startDate ?? end.AddDays(-30);

        // Get user's videos
        var videoIds = await _dbContext.Videos
            .Where(v => v.UserId == userId)
            .Select(v => v.Id)
            .ToListAsync();

        // Calculate metrics
        var totalRevenue = await _dbContext.VideoPurchases
            .Where(vp => videoIds.Contains(vp.VideoId) && !vp.IsRefunded)
            .SumAsync(vp => vp.Price) +
            await _dbContext.VideoRentals
            .Where(vr => videoIds.Contains(vr.VideoId))
            .SumAsync(vr => vr.Price);

        var totalViews = await _dbContext.VideoAnalytics
            .CountAsync(va => videoIds.Contains(va.VideoId));

        var totalCustomers = await _dbContext.VideoPurchases
            .Where(vp => videoIds.Contains(vp.VideoId) && !vp.IsRefunded)
            .Select(vp => vp.UserId)
            .Distinct()
            .CountAsync();

        return new RevenueMetricsDto
        {
            LifetimeValue = totalCustomers > 0 ? totalRevenue / totalCustomers : 0,
            CustomerAcquisitionCost = 50, // Placeholder
            PaybackPeriod = 12, // Placeholder in months
            AverageOrderValue = totalCustomers > 0 ? (double)totalRevenue / totalCustomers : 0,
            ConversionRate = 2.5, // Placeholder percentage
            RevenuePerThousandViews = totalViews > 0 ? (double)(totalRevenue * 1000) / totalViews : 0,
            SubscriberLifetimeValue = 100, // Placeholder
            MonthlyChurnRate = 5.0, // Placeholder percentage
            Currency = "USD"
        };
    }
}
