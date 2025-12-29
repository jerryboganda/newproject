using System;
using StreamVault.Domain.Interfaces;

namespace StreamVault.Domain.Entities
{
    public class PaymentMethod : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public string StripePaymentMethodId { get; set; } = null!;
        public PaymentMethodType Type { get; set; }
        public string? Brand { get; set; }
        public string? Last4 { get; set; }
        public int? ExpiryMonth { get; set; }
        public int? ExpiryYear { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public enum PaymentMethodType
    {
        Card = 1,
        BankAccount = 2,
        SepaDebit = 3
    }
}
