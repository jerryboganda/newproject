using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Subscriptions.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Subscriptions;

public class SubscriptionService : ISubscriptionService
{
    private readonly StreamVaultDbContext _dbContext;

    public SubscriptionService(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SubscriptionPlanDto>> GetAvailablePlansAsync(Guid tenantId)
    {
        var plans = await _dbContext.SubscriptionPlans
            .Where(sp => sp.IsActive)
            .OrderBy(sp => sp.PriceMonthly)
            .ToListAsync();

        return plans.Select(sp => new SubscriptionPlanDto
        {
            Id = sp.Id,
            Name = sp.Name,
            Description = sp.Description ?? "",
            Price = sp.PriceMonthly,
            Currency = "USD",
            BillingInterval = "monthly",
            Features = sp.Features ?? new Dictionary<string, object>(),
            Limits = sp.Limits?.ToDictionary(kvp => kvp.Key, kvp => (long)Convert.ToInt64(kvp.Value)) ?? new Dictionary<string, long>(),
            IsActive = sp.IsActive,
            StripePriceId = sp.StripePriceIdMonthly ?? ""
        }).ToList();
    }

    public async Task<SubscriptionDto> GetCurrentSubscriptionAsync(Guid tenantId)
    {
        var subscription = await _dbContext.TenantSubscriptions
            .Include(ts => ts.Plan)
            .Where(ts => ts.TenantId == tenantId && 
                        (ts.Status == SubscriptionStatus.Active || ts.Status == SubscriptionStatus.Trialing))
            .FirstOrDefaultAsync();

        if (subscription == null)
        {
            // Return default free plan
            var freePlan = await _dbContext.SubscriptionPlans
                .FirstOrDefaultAsync(sp => sp.PriceMonthly == 0 && sp.IsActive);

            return new SubscriptionDto
            {
                Id = Guid.Empty,
                TenantId = tenantId,
                PlanId = freePlan?.Id ?? Guid.Empty,
                PlanName = freePlan?.Name ?? "Free",
                Status = "inactive",
                CurrentPeriodStart = DateTimeOffset.UtcNow,
                CurrentPeriodEnd = DateTimeOffset.UtcNow.AddMonths(1),
                AutoRenew = false,
                Usage = new Dictionary<string, long>(),
                Limits = freePlan?.Limits?.ToDictionary(kvp => kvp.Key, kvp => (long)Convert.ToInt64(kvp.Value)) ?? new Dictionary<string, long>()
            };
        }

        // Calculate usage (mock data for now)
        var usage = new Dictionary<string, long>();
        if (subscription.Plan.Limits != null)
        {
            foreach (var limit in subscription.Plan.Limits)
            {
                usage[limit.Key] = Random.Shared.Next(0, (int)Convert.ToInt64(limit.Value));
            }
        }

        return new SubscriptionDto
        {
            Id = subscription.Id,
            TenantId = subscription.TenantId,
            PlanId = subscription.PlanId,
            PlanName = subscription.Plan.Name,
            Status = subscription.Status.ToString(),
            CurrentPeriodStart = subscription.CurrentPeriodStart ?? DateTimeOffset.UtcNow,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd ?? DateTimeOffset.UtcNow,
            CanceledAt = subscription.CancelAt,
            EndedAt = null,
            AutoRenew = true,
            Usage = usage,
            Limits = subscription.Plan.Limits?.ToDictionary(kvp => kvp.Key, kvp => (long)Convert.ToInt64(kvp.Value)) ?? new Dictionary<string, long>()
        };
    }

    public async Task<SubscriptionDto> SubscribeToPlanAsync(Guid tenantId, Guid planId, SubscribeRequest request)
    {
        var plan = await _dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(sp => sp.Id == planId && sp.IsActive);

        if (plan == null)
            throw new Exception("Subscription plan not found");

        // Cancel existing subscription if any
        var existingSubscription = await _dbContext.TenantSubscriptions
            .Where(ts => ts.TenantId == tenantId && 
                        (ts.Status == SubscriptionStatus.Active || ts.Status == SubscriptionStatus.Trialing))
            .FirstOrDefaultAsync();

        if (existingSubscription != null)
        {
            existingSubscription.Status = SubscriptionStatus.Canceled;
            existingSubscription.CancelAt = DateTimeOffset.UtcNow;
        }

        // Create new subscription
        var subscription = new TenantSubscription
        {
            TenantId = tenantId,
            PlanId = planId,
            Status = SubscriptionStatus.Active,
            CurrentPeriodStart = DateTimeOffset.UtcNow,
            CurrentPeriodEnd = request.BillingCycle == "yearly" 
                ? DateTimeOffset.UtcNow.AddYears(1)
                : DateTimeOffset.UtcNow.AddMonths(1),
            BillingCycle = request.BillingCycle == "yearly" ? BillingCycle.Yearly : BillingCycle.Monthly,
            TrialEnd = null
        };

        _dbContext.TenantSubscriptions.Add(subscription);
        await _dbContext.SaveChangesAsync();

        return await GetCurrentSubscriptionAsync(tenantId);
    }

    public async Task CancelSubscriptionAsync(Guid tenantId)
    {
        var subscription = await _dbContext.TenantSubscriptions
            .Where(ts => ts.TenantId == tenantId && 
                        (ts.Status == SubscriptionStatus.Active || ts.Status == SubscriptionStatus.Trialing))
            .FirstOrDefaultAsync();

        if (subscription == null)
            throw new Exception("No active subscription found");

        subscription.Status = SubscriptionStatus.Canceled;
        subscription.CancelAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<SubscriptionDto> UpdateSubscriptionAsync(Guid tenantId, Guid planId)
    {
        var currentSubscription = await _dbContext.TenantSubscriptions
            .Where(ts => ts.TenantId == tenantId && 
                        (ts.Status == SubscriptionStatus.Active || ts.Status == SubscriptionStatus.Trialing))
            .FirstOrDefaultAsync();

        if (currentSubscription == null)
            throw new Exception("No active subscription found");

        var newPlan = await _dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(sp => sp.Id == planId && sp.IsActive);

        if (newPlan == null)
            throw new Exception("Subscription plan not found");

        // Update subscription
        currentSubscription.PlanId = planId;
        currentSubscription.CurrentPeriodStart = DateTimeOffset.UtcNow;
        currentSubscription.CurrentPeriodEnd = newPlan.PriceYearly > 0 
            ? DateTimeOffset.UtcNow.AddYears(1)
            : DateTimeOffset.UtcNow.AddMonths(1);
        currentSubscription.BillingCycle = newPlan.PriceYearly > 0 ? BillingCycle.Yearly : BillingCycle.Monthly;

        await _dbContext.SaveChangesAsync();

        return await GetCurrentSubscriptionAsync(tenantId);
    }

    public async Task<List<SubscriptionDto>> GetSubscriptionHistoryAsync(Guid tenantId)
    {
        var subscriptions = await _dbContext.TenantSubscriptions
            .Include(ts => ts.Plan)
            .Where(ts => ts.TenantId == tenantId)
            .OrderByDescending(ts => ts.Id)
            .ToListAsync();

        return subscriptions.Select(ts => new SubscriptionDto
        {
            Id = ts.Id,
            TenantId = ts.TenantId,
            PlanId = ts.PlanId,
            PlanName = ts.Plan.Name,
            Status = ts.Status.ToString(),
            CurrentPeriodStart = ts.CurrentPeriodStart ?? DateTimeOffset.UtcNow,
            CurrentPeriodEnd = ts.CurrentPeriodEnd ?? DateTimeOffset.UtcNow,
            CanceledAt = ts.CancelAt,
            EndedAt = null,
            AutoRenew = true,
            Usage = new Dictionary<string, long>(),
            Limits = ts.Plan.Limits?.ToDictionary(kvp => kvp.Key, kvp => (long)Convert.ToInt64(kvp.Value)) ?? new Dictionary<string, long>()
        }).ToList();
    }
}
