using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Monetization.DTOs;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Monetization;

public class SubscriptionTierService : ISubscriptionTierService
{
    private readonly StreamVaultDbContext _dbContext;

    public SubscriptionTierService(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SubscriptionTierDto>> GetSubscriptionTiersAsync(Guid tenantId)
    {
        var tiers = await _dbContext.SubscriptionTiers
            .Include(st => st.Features)
            .Include(st => st.Limits)
            .Include(st => st.Subscriptions)
            .Where(st => st.IsActive)
            .OrderBy(st => st.SortOrder)
            .ToListAsync();

        return tiers.Select(MapToDto).ToList();
    }

    public async Task<SubscriptionTierDto> GetSubscriptionTierAsync(Guid tierId, Guid tenantId)
    {
        var tier = await _dbContext.SubscriptionTiers
            .Include(st => st.Features)
            .Include(st => st.Limits)
            .Include(st => st.Subscriptions)
            .FirstOrDefaultAsync(st => st.Id == tierId);

        if (tier == null)
            throw new Exception("Subscription tier not found");

        return MapToDto(tier);
    }

    public async Task<SubscriptionTierDto> CreateSubscriptionTierAsync(CreateSubscriptionTierRequest request, Guid tenantId)
    {
        var tier = new SubscriptionTier
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Currency = request.Currency,
            BillingCycle = request.BillingCycle,
            IsActive = true,
            SortOrder = request.SortOrder,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Add features
        foreach (var featureRequest in request.Features)
        {
            tier.Features.Add(new TierFeature
            {
                Id = Guid.NewGuid(),
                FeatureName = featureRequest.FeatureName,
                IsIncluded = featureRequest.IsIncluded,
                Description = featureRequest.Description,
                SortOrder = featureRequest.SortOrder
            });
        }

        // Add limits
        foreach (var limitRequest in request.Limits)
        {
            tier.Limits.Add(new TierLimit
            {
                Id = Guid.NewGuid(),
                LimitType = limitRequest.LimitType,
                LimitValue = limitRequest.LimitValue,
                Unit = limitRequest.Unit
            });
        }

        _dbContext.SubscriptionTiers.Add(tier);
        await _dbContext.SaveChangesAsync();

        return MapToDto(tier);
    }

    public async Task<SubscriptionTierDto> UpdateSubscriptionTierAsync(Guid tierId, UpdateSubscriptionTierRequest request, Guid tenantId)
    {
        var tier = await _dbContext.SubscriptionTiers
            .Include(st => st.Features)
            .Include(st => st.Limits)
            .FirstOrDefaultAsync(st => st.Id == tierId);

        if (tier == null)
            throw new Exception("Subscription tier not found");

        // Update basic properties
        if (request.Name != null)
            tier.Name = request.Name;

        if (request.Description != null)
            tier.Description = request.Description;

        if (request.Price.HasValue)
            tier.Price = request.Price.Value;

        if (request.Currency != null)
            tier.Currency = request.Currency;

        if (request.BillingCycle.HasValue)
            tier.BillingCycle = request.BillingCycle.Value;

        if (request.IsActive.HasValue)
            tier.IsActive = request.IsActive.Value;

        if (request.SortOrder.HasValue)
            tier.SortOrder = request.SortOrder.Value;

        // Update features
        if (request.Features != null)
        {
            foreach (var featureRequest in request.Features)
            {
                var feature = tier.Features.FirstOrDefault(f => f.Id == featureRequest.Id);
                if (feature != null)
                {
                    if (featureRequest.FeatureName != null)
                        feature.FeatureName = featureRequest.FeatureName;

                    if (featureRequest.IsIncluded.HasValue)
                        feature.IsIncluded = featureRequest.IsIncluded.Value;

                    if (featureRequest.Description != null)
                        feature.Description = featureRequest.Description;

                    if (featureRequest.SortOrder.HasValue)
                        feature.SortOrder = featureRequest.SortOrder.Value;
                }
            }
        }

        // Update limits
        if (request.Limits != null)
        {
            foreach (var limitRequest in request.Limits)
            {
                var limit = tier.Limits.FirstOrDefault(l => l.Id == limitRequest.Id);
                if (limit != null)
                {
                    if (limitRequest.LimitType != null)
                        limit.LimitType = limitRequest.LimitType;

                    if (limitRequest.LimitValue.HasValue)
                        limit.LimitValue = limitRequest.LimitValue.Value;

                    if (limitRequest.Unit != null)
                        limit.Unit = limitRequest.Unit;
                }
            }
        }

        tier.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();

        return MapToDto(tier);
    }

    public async Task<bool> DeleteSubscriptionTierAsync(Guid tierId, Guid tenantId)
    {
        var tier = await _dbContext.SubscriptionTiers
            .FirstOrDefaultAsync(st => st.Id == tierId);

        if (tier == null)
            return false;

        // Check if there are active subscriptions
        var activeSubscriptions = await _dbContext.UserSubscriptions
            .CountAsync(us => us.SubscriptionTierId == tierId && us.Status == SubscriptionStatus.Active);

        if (activeSubscriptions > 0)
            throw new Exception("Cannot delete tier with active subscriptions");

        _dbContext.SubscriptionTiers.Remove(tier);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<UserSubscriptionDto> SubscribeToTierAsync(Guid tierId, CreateSubscriptionRequest request, Guid userId, Guid tenantId)
    {
        // Verify tier exists and is active
        var tier = await _dbContext.SubscriptionTiers
            .FirstOrDefaultAsync(st => st.Id == tierId && st.IsActive);

        if (tier == null)
            throw new Exception("Subscription tier not found or inactive");

        // Check if user already has an active subscription to this tier
        var existingSubscription = await _dbContext.UserSubscriptions
            .FirstOrDefaultAsync(us => us.UserId == userId && 
                                     us.SubscriptionTierId == tierId && 
                                     us.Status == SubscriptionStatus.Active);

        if (existingSubscription != null)
            throw new Exception("User already has an active subscription to this tier");

        // Check if user has any active subscription (upgrade/downgrade logic)
        var activeSubscription = await _dbContext.UserSubscriptions
            .FirstOrDefaultAsync(us => us.UserId == userId && us.Status == SubscriptionStatus.Active);

        if (activeSubscription != null)
        {
            // Cancel current subscription and create new one
            activeSubscription.Status = SubscriptionStatus.Canceled;
            activeSubscription.CanceledAt = DateTimeOffset.UtcNow;
            activeSubscription.UpdatedAt = DateTimeOffset.UtcNow;
        }

        // Process payment (in production, integrate with Stripe)
        var stripeSubscriptionId = Guid.NewGuid().ToString();

        // Create new subscription
        var subscription = new UserSubscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SubscriptionTierId = tierId,
            StripeSubscriptionId = stripeSubscriptionId,
            Status = SubscriptionStatus.Active,
            StartDate = DateTimeOffset.UtcNow,
            CurrentPeriodStart = DateTimeOffset.UtcNow,
            CurrentPeriodEnd = CalculateNextBillingDate(tier.BillingCycle),
            Price = tier.Price,
            Currency = tier.Currency,
            BillingCycle = tier.BillingCycle,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.UserSubscriptions.Add(subscription);
        await _dbContext.SaveChangesAsync();

        // Map to DTO
        var subscriptionDto = MapSubscriptionToDto(subscription);
        subscriptionDto.SubscriptionTier = MapToDto(tier);
        
        return subscriptionDto;
    }

    public async Task<bool> CancelSubscriptionAsync(Guid subscriptionId, Guid userId, Guid tenantId)
    {
        var subscription = await _dbContext.UserSubscriptions
            .Include(us => us.SubscriptionTier)
            .FirstOrDefaultAsync(us => us.Id == subscriptionId && us.UserId == userId);

        if (subscription == null)
            throw new Exception("Subscription not found");

        if (subscription.Status != SubscriptionStatus.Active)
            throw new Exception("Subscription is not active");

        // Cancel at period end
        subscription.Status = SubscriptionStatus.Canceled;
        subscription.CanceledAt = DateTimeOffset.UtcNow;
        subscription.CancelAtPeriodEnd = subscription.CurrentPeriodEnd;
        subscription.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<UserSubscriptionDto> GetUserSubscriptionAsync(Guid userId, Guid tenantId)
    {
        var subscription = await _dbContext.UserSubscriptions
            .Include(us => us.SubscriptionTier)
                .ThenInclude(st => st.Features)
            .Include(us => us.SubscriptionTier)
                .ThenInclude(st => st.Limits)
            .Include(us => us.User)
            .Where(us => us.UserId == userId && us.Status == SubscriptionStatus.Active)
            .OrderByDescending(us => us.CreatedAt)
            .FirstOrDefaultAsync();

        if (subscription == null)
            throw new Exception("No active subscription found");

        var subscriptionDto = MapSubscriptionToDto(subscription);
        subscriptionDto.SubscriptionTier = MapToDto(subscription.SubscriptionTier);
        
        return subscriptionDto;
    }

    public async Task<List<UserSubscriptionDto>> GetUserSubscriptionsAsync(Guid userId, Guid tenantId)
    {
        var subscriptions = await _dbContext.UserSubscriptions
            .Include(us => us.SubscriptionTier)
            .Include(us => us.User)
            .Where(us => us.UserId == userId)
            .OrderByDescending(us => us.CreatedAt)
            .ToListAsync();

        return subscriptions.Select(s =>
        {
            var dto = MapSubscriptionToDto(s);
            dto.SubscriptionTier = MapToDto(s.SubscriptionTier);
            return dto;
        }).ToList();
    }

    public async Task<bool> CanUserAccessTierFeatureAsync(Guid userId, string featureName, Guid tenantId)
    {
        var subscription = await _dbContext.UserSubscriptions
            .Include(us => us.SubscriptionTier)
                .ThenInclude(st => st.Features)
            .FirstOrDefaultAsync(us => us.UserId == userId && 
                                     us.Status == SubscriptionStatus.Active &&
                                     us.CurrentPeriodEnd > DateTimeOffset.UtcNow);

        if (subscription == null)
            return false;

        return subscription.SubscriptionTier.Features
            .Any(f => f.FeatureName.Equals(featureName, StringComparison.OrdinalIgnoreCase) && f.IsIncluded);
    }

    public async Task<int> GetUserTierLimitAsync(Guid userId, string limitType, Guid tenantId)
    {
        var subscription = await _dbContext.UserSubscriptions
            .Include(us => us.SubscriptionTier)
                .ThenInclude(st => st.Limits)
            .FirstOrDefaultAsync(us => us.UserId == userId && 
                                     us.Status == SubscriptionStatus.Active &&
                                     us.CurrentPeriodEnd > DateTimeOffset.UtcNow);

        if (subscription == null)
            return 0;

        var limit = subscription.SubscriptionTier.Limits
            .FirstOrDefault(l => l.LimitType.Equals(limitType, StringComparison.OrdinalIgnoreCase));

        return limit?.LimitValue ?? 0;
    }

    public async Task<List<UserSubscriptionDto>> GetActiveSubscriptionsAsync(Guid tenantId)
    {
        var subscriptions = await _dbContext.UserSubscriptions
            .Include(us => us.SubscriptionTier)
            .Include(us => us.User)
            .Where(us => us.Status == SubscriptionStatus.Active && us.User.TenantId == tenantId)
            .OrderByDescending(us => us.CreatedAt)
            .ToListAsync();

        return subscriptions.Select(s =>
        {
            var dto = MapSubscriptionToDto(s);
            dto.SubscriptionTier = MapToDto(s.SubscriptionTier);
            return dto;
        }).ToList();
    }

    public async Task<RevenueDto> GetSubscriptionRevenueAsync(Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        var start = startDate ?? DateTimeOffset.UtcNow.AddDays(-30);
        var end = endDate ?? DateTimeOffset.UtcNow;

        var revenue = await _dbContext.UserSubscriptions
            .Include(us => us.User)
            .Include(us => us.SubscriptionTier)
            .Where(us => us.User.TenantId == tenantId && 
                        us.Status == SubscriptionStatus.Active &&
                        us.CreatedAt >= start && us.CreatedAt <= end)
            .ToListAsync();

        var totalRevenue = revenue.Sum(r => r.Price);
        var activeCount = revenue.Count(r => r.Status == SubscriptionStatus.Active);

        return new RevenueDto
        {
            VideoId = Guid.Empty,
            VideoTitle = "Subscription Revenue",
            TotalRevenue = totalRevenue,
            PurchaseRevenue = totalRevenue,
            Currency = "USD",
            Period = DateOnly.FromDateTime(DateTime.UtcNow)
        };
    }

    private static SubscriptionTierDto MapToDto(SubscriptionTier tier)
    {
        return new SubscriptionTierDto
        {
            Id = tier.Id,
            Name = tier.Name,
            Description = tier.Description,
            Price = tier.Price,
            Currency = tier.Currency,
            BillingCycle = tier.BillingCycle,
            IsActive = tier.IsActive,
            SortOrder = tier.SortOrder,
            StripePriceId = tier.StripePriceId,
            Features = tier.Features.Select(f => new TierFeatureDto
            {
                Id = f.Id,
                FeatureName = f.FeatureName,
                IsIncluded = f.IsIncluded,
                Description = f.Description,
                SortOrder = f.SortOrder
            }).ToList(),
            Limits = tier.Limits.Select(l => new TierLimitDto
            {
                Id = l.Id,
                LimitType = l.LimitType,
                LimitValue = l.LimitValue,
                Unit = l.Unit
            }).ToList(),
            SubscriberCount = tier.Subscriptions.Count(s => s.Status == SubscriptionStatus.Active),
            CreatedAt = tier.CreatedAt,
            UpdatedAt = tier.UpdatedAt
        };
    }

    private static UserSubscriptionDto MapSubscriptionToDto(UserSubscription subscription)
    {
        return new UserSubscriptionDto
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            SubscriptionTierId = subscription.SubscriptionTierId,
            StripeSubscriptionId = subscription.StripeSubscriptionId,
            Status = subscription.Status,
            StartDate = subscription.StartDate,
            CurrentPeriodStart = subscription.CurrentPeriodStart,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd,
            CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
            CanceledAt = subscription.CanceledAt,
            EndedAt = subscription.EndedAt,
            Price = subscription.Price,
            Currency = subscription.Currency,
            BillingCycle = subscription.BillingCycle,
            CreatedAt = subscription.CreatedAt,
            UpdatedAt = subscription.UpdatedAt,
            User = new UserDto
            {
                Id = subscription.User.Id,
                Email = subscription.User.Email,
                FirstName = subscription.User.FirstName,
                LastName = subscription.User.LastName,
                AvatarUrl = subscription.User.AvatarUrl
            }
        };
    }

    private static DateTimeOffset CalculateNextBillingDate(BillingCycle cycle)
    {
        return cycle switch
        {
            BillingCycle.Monthly => DateTimeOffset.UtcNow.AddMonths(1),
            BillingCycle.Quarterly => DateTimeOffset.UtcNow.AddMonths(3),
            BillingCycle.Yearly => DateTimeOffset.UtcNow.AddYears(1),
            _ => DateTimeOffset.UtcNow.AddMonths(1)
        };
    }
}
