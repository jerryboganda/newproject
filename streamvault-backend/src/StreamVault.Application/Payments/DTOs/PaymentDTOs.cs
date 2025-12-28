using System.ComponentModel.DataAnnotations;
using StreamVault.Domain.Entities;

namespace StreamVault.Application.Payments.DTOs;

public class CreatePaymentIntentRequest
{
    [Required]
    public decimal Amount { get; set; }

    [Required]
    public string Currency { get; set; } = "USD";

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    public string? PaymentMethodId { get; set; }

    public bool SetupFutureUsage { get; set; } = false;

    public string? Description { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }
}

public class PaymentIntentDto
{
    public string Id { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string? ClientSecret { get; set; }
    public string? PaymentMethodId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
}

public enum PaymentStatus
{
    RequiresPaymentMethod,
    RequiresConfirmation,
    RequiresAction,
    Processing,
    Succeeded,
    Canceled,
    Failed
}

public class CreateRefundRequest
{
    [Required]
    public string PaymentIntentId { get; set; } = string.Empty;

    public decimal? Amount { get; set; }

    public string? Reason { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }
}

public class RefundDto
{
    public string Id { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class CreatePaymentMethodRequest
{
    [Required]
    public string Type { get; set; } = string.Empty; // card, sepa_debit, etc.

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    public CardDetailsDto? Card { get; set; }

    public bool IsDefault { get; set; } = false;

    public Dictionary<string, string>? Metadata { get; set; }
}

public class CardDetailsDto
{
    [Required]
    public string Number { get; set; } = string.Empty;

    [Required]
    public string ExpMonth { get; set; } = string.Empty;

    [Required]
    public string ExpYear { get; set; } = string.Empty;

    [Required]
    public string Cvc { get; set; } = string.Empty;

    public string? Name { get; set; }
}

public class PaymentMethodDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public CardDto? Card { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class CardDto
{
    public string Brand { get; set; } = string.Empty;
    public string Last4 { get; set; } = string.Empty;
    public string ExpMonth { get; set; } = string.Empty;
    public string ExpYear { get; set; } = string.Empty;
    public string Funding { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class CreateSubscriptionRequest
{
    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    public string PriceId { get; set; } = string.Empty;

    public string? PaymentMethodId { get; set; }

    public string? CouponCode { get; set; }

    public DateTimeOffset? TrialEnd { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }
}

public class UpdateSubscriptionRequest
{
    public string? PriceId { get; set; }

    public string? PaymentMethodId { get; set; }

    public string? CouponCode { get; set; }

    public bool? ProrationBehavior { get; set; } // true = create_prorations, false = none

    public Dictionary<string, string>? Metadata { get; set; }
}

public class SubscriptionDto
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string PriceId { get; set; } = string.Empty;
    public SubscriptionStatus Status { get; set; }
    public DateTimeOffset CurrentPeriodStart { get; set; }
    public DateTimeOffset CurrentPeriodEnd { get; set; }
    public DateTimeOffset? TrialEnd { get; set; }
    public DateTimeOffset? CanceledAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public class InvoiceDto
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountRemaining { get; set; }
    public string Currency { get; set; } = string.Empty;
    public InvoiceStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public string? HostedInvoiceUrl { get; set; }
    public string? InvoicePdf { get; set; }
}

public class CreateCustomerRequest
{
    public string? Email { get; set; }

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public AddressDto? Address { get; set; }

    public string? PaymentMethodId { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }
}

public class UpdateCustomerRequest
{
    public string? Email { get; set; }

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public AddressDto? Address { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }
}

public class CustomerDto
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public AddressDto? Address { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class AddressDto
{
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public enum InvoiceStatus
{
    Draft,
    Open,
    Paid,
    Void,
    Uncollectible
}

public class WebhookEventDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public object Data { get; set; } = new();
}

public class ProcessSubscriptionPaymentRequest
{
    [Required]
    public Guid SubscriptionPlanId { get; set; }

    [Required]
    public string PaymentMethodId { get; set; } = string.Empty;

    public bool SavePaymentMethod { get; set; } = false;
}

public class SubscriptionPaymentDto
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public class SubscriptionPaymentResultDto
{
    public bool Success { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? ErrorMessage { get; set; }
    public SubscriptionPaymentDto? Payment { get; set; }
}

public class AddPaymentMethodRequest
{
    [Required]
    public string PaymentMethodId { get; set; } = string.Empty;

    public bool IsDefault { get; set; } = false;
}

public class TransactionDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string Type { get; set; } = string.Empty; // payment, refund, etc.
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
