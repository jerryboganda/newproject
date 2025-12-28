using StreamVault.Application.Payments.DTOs;

namespace StreamVault.Application.Payments;

public interface IPaymentGatewayService
{
    Task<PaymentIntentDto> CreatePaymentIntentAsync(CreatePaymentIntentRequest request);
    Task<PaymentIntentDto> ConfirmPaymentAsync(string paymentIntentId);
    Task<bool> CancelPaymentAsync(string paymentIntentId);
    Task<RefundDto> CreateRefundAsync(CreateRefundRequest request);
    Task<PaymentMethodDto> CreatePaymentMethodAsync(CreatePaymentMethodRequest request);
    Task<bool> DeletePaymentMethodAsync(string paymentMethodId);
    Task<List<PaymentMethodDto>> GetCustomerPaymentMethodsAsync(string customerId, int? limit = null);
    Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionRequest request);
    Task<SubscriptionDto> UpdateSubscriptionAsync(string subscriptionId, UpdateSubscriptionRequest request);
    Task<bool> CancelSubscriptionAsync(string subscriptionId, bool immediate = false);
    Task<InvoiceDto> GetInvoiceAsync(string invoiceId);
    Task<List<InvoiceDto>> GetCustomerInvoicesAsync(string customerId, int? limit = null);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request);
    Task<CustomerDto> UpdateCustomerAsync(string customerId, UpdateCustomerRequest request);
    Task<CustomerDto> GetCustomerAsync(string customerId);
}
