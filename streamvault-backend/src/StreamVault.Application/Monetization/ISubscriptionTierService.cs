using StreamVault.Application.Monetization.DTOs;
using StreamVault.Domain.Entities;

namespace StreamVault.Application.Monetization;

public interface ISubscriptionTierService
{
    Task<List<SubscriptionTierDto>> GetSubscriptionTiersAsync(Guid tenantId);
    Task<SubscriptionTierDto> GetSubscriptionTierAsync(Guid tierId, Guid tenantId);
    Task<SubscriptionTierDto> CreateSubscriptionTierAsync(CreateSubscriptionTierRequest request, Guid tenantId);
    Task<SubscriptionTierDto> UpdateSubscriptionTierAsync(Guid tierId, UpdateSubscriptionTierRequest request, Guid tenantId);
    Task<bool> DeleteSubscriptionTierAsync(Guid tierId, Guid tenantId);
    Task<UserSubscriptionDto> SubscribeToTierAsync(Guid tierId, CreateSubscriptionRequest request, Guid userId, Guid tenantId);
    Task<bool> CancelSubscriptionAsync(Guid subscriptionId, Guid userId, Guid tenantId);
    Task<UserSubscriptionDto> GetUserSubscriptionAsync(Guid userId, Guid tenantId);
    Task<List<UserSubscriptionDto>> GetUserSubscriptionsAsync(Guid userId, Guid tenantId);
    Task<bool> CanUserAccessTierFeatureAsync(Guid userId, string featureName, Guid tenantId);
    Task<int> GetUserTierLimitAsync(Guid userId, string limitType, Guid tenantId);
    Task<List<UserSubscriptionDto>> GetActiveSubscriptionsAsync(Guid tenantId);
    Task<RevenueDto> GetSubscriptionRevenueAsync(Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
}
