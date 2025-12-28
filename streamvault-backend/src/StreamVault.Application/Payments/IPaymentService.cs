using StreamVault.Application.Payments.DTOs;

namespace StreamVault.Application.Payments;

public interface IPaymentService
{
    Task<PaymentIntentDto> CreatePaymentIntentAsync(CreatePaymentIntentRequest request, Guid userId, Guid tenantId);
    Task<PaymentIntentDto> GetPaymentIntentAsync(string paymentIntentId);
    Task<bool> ConfirmPaymentAsync(string paymentIntentId);
    Task<SubscriptionPaymentResultDto> ProcessSubscriptionPaymentAsync(ProcessSubscriptionPaymentRequest request, Guid userId, Guid tenantId);
    Task<List<PaymentMethodDto>> GetPaymentMethodsAsync(Guid userId, Guid tenantId);
    Task<PaymentMethodDto> AddPaymentMethodAsync(AddPaymentMethodRequest request, Guid userId, Guid tenantId);
    Task<bool> RemovePaymentMethodAsync(string paymentMethodId, Guid userId, Guid tenantId);
    Task<List<TransactionDto>> GetTransactionHistoryAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20);
}
