using StreamVault.Application.Subscriptions.DTOs;

namespace StreamVault.Application.Subscriptions;

public interface ISubscriptionService
{
    Task<List<SubscriptionPlanDto>> GetAvailablePlansAsync(Guid tenantId);
    Task<SubscriptionDto> GetCurrentSubscriptionAsync(Guid tenantId);
    Task<SubscriptionDto> SubscribeToPlanAsync(Guid tenantId, Guid planId, SubscribeRequest request);
    Task CancelSubscriptionAsync(Guid tenantId);
    Task<SubscriptionDto> UpdateSubscriptionAsync(Guid tenantId, Guid planId);
    Task<List<SubscriptionDto>> GetSubscriptionHistoryAsync(Guid tenantId);
}
