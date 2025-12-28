using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StreamVault.Application.Payments.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;
using Stripe;

namespace StreamVault.Application.Payments;

public class PaymentService : IPaymentService
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public PaymentService(StreamVaultDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        
        // Configure Stripe
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    public async Task<PaymentIntentDto> CreatePaymentIntentAsync(CreatePaymentIntentRequest request, Guid userId, Guid tenantId)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(request.Amount * 100), // Convert to cents
            Currency = request.Currency,
            PaymentMethod = request.PaymentMethodId,
            ConfirmationMethod = "manual",
            Confirm = false,
            Description = request.Description,
            Metadata = new Dictionary<string, string>
            {
                { "tenant_id", tenantId.ToString() },
                { "user_id", userId.ToString() }
            }
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);

        return new PaymentIntentDto
        {
            Id = paymentIntent.Id,
            Amount = paymentIntent.Amount / 100m,
            Currency = paymentIntent.Currency,
            Status = MapStripeStatus(paymentIntent.Status),
            ClientSecret = paymentIntent.ClientSecret,
            PaymentMethodId = paymentIntent.PaymentMethodId,
            CreatedAt = DateTimeOffset.UtcNow,
            Description = paymentIntent.Description
        };
    }

    public async Task<PaymentIntentDto> GetPaymentIntentAsync(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        var paymentIntent = await service.GetAsync(paymentIntentId);

        return new PaymentIntentDto
        {
            Id = paymentIntent.Id,
            Amount = paymentIntent.Amount / 100m,
            Currency = paymentIntent.Currency,
            Status = MapStripeStatus(paymentIntent.Status),
            ClientSecret = paymentIntent.ClientSecret,
            PaymentMethodId = paymentIntent.PaymentMethodId,
            CreatedAt = DateTimeOffset.UtcNow,
            Description = paymentIntent.Description
        };
    }

    public async Task<bool> ConfirmPaymentAsync(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        var paymentIntent = await service.ConfirmAsync(paymentIntentId);

        return paymentIntent.Status == "succeeded";
    }

    public async Task<SubscriptionPaymentResultDto> ProcessSubscriptionPaymentAsync(ProcessSubscriptionPaymentRequest request, Guid userId, Guid tenantId)
    {
        try
        {
            // Get the subscription plan
            var plan = await _dbContext.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Id == request.SubscriptionPlanId && p.IsActive);

            if (plan == null)
                return new SubscriptionPaymentResultDto { Success = false, ErrorMessage = "Subscription plan not found" };

            // Create Stripe customer if not exists
            var customer = await GetOrCreateStripeCustomer(userId, tenantId);

            // Attach payment method to customer
            var paymentMethodService = new PaymentMethodService();
            await paymentMethodService.AttachAsync(request.PaymentMethodId, new PaymentMethodAttachOptions
            {
                Customer = customer.Id
            });

            // Create subscription
            var priceId = request.BillingCycle == "yearly" ? plan.StripePriceIdYearly : plan.StripePriceIdMonthly;
            
            if (string.IsNullOrEmpty(priceId))
                return new SubscriptionPaymentResultDto { Success = false, ErrorMessage = "No price configured for this billing cycle" };

            var subscriptionOptions = new SubscriptionCreateOptions
            {
                Customer = customer.Id,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = priceId,
                    },
                },
                DefaultPaymentMethod = request.PaymentMethodId,
                Expand = new List<string> { "latest_invoice.payment_intent" },
                Metadata = new Dictionary<string, string>
                {
                    { "tenant_id", tenantId.ToString() },
                    { "user_id", userId.ToString() }
                }
            };

            var subscriptionService = new SubscriptionService();
            var subscription = await subscriptionService.CreateAsync(subscriptionOptions);

            // Update tenant subscription in database
            await UpdateTenantSubscription(tenantId, request.SubscriptionPlanId, subscription.Id, request.BillingCycle);

            return new SubscriptionPaymentResultDto
            {
                Success = true,
                SubscriptionId = subscription.Id,
                ClientSecret = string.Empty // Simplified for now
            };
        }
        catch (StripeException ex)
        {
            return new SubscriptionPaymentResultDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<List<PaymentMethodDto>> GetPaymentMethodsAsync(Guid userId, Guid tenantId)
    {
        var customer = await GetOrCreateStripeCustomer(userId, tenantId);
        
        var options = new PaymentMethodListOptions
        {
            Customer = customer.Id,
            Type = "card"
        };

        var service = new PaymentMethodService();
        var paymentMethods = await service.ListAsync(options);

        return paymentMethods.Data.Select(pm => new PaymentMethodDto
        {
            Id = pm.Id,
            Type = pm.Type,
            Brand = pm.Card?.Brand ?? "",
            Last4 = pm.Card?.Last4 ?? "",
            ExpiryMonth = (int)(pm.Card?.ExpMonth ?? 0),
            ExpiryYear = (int)(pm.Card?.ExpYear ?? 0),
            IsDefault = pm.Metadata.ContainsKey("is_default") && bool.Parse(pm.Metadata["is_default"]),
            CreatedAt = DateTimeOffset.UtcNow,
        }).ToList();
    }

    public async Task<PaymentMethodDto> AddPaymentMethodAsync(AddPaymentMethodRequest request, Guid userId, Guid tenantId)
    {
        var customer = await GetOrCreateStripeCustomer(userId, tenantId);

        // Attach payment method to customer
        var paymentMethodService = new PaymentMethodService();
        var paymentMethod = await paymentMethodService.AttachAsync(request.PaymentMethodId, new PaymentMethodAttachOptions
        {
            Customer = customer.Id
        });

        // Set as default if requested
        if (request.IsDefault)
        {
            var customerService = new CustomerService();
            await customerService.UpdateAsync(customer.Id, new CustomerUpdateOptions
            {
                InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = paymentMethod.Id
                }
            });
        }

        return new PaymentMethodDto
        {
            Id = paymentMethod.Id,
            Type = paymentMethod.Type,
            Brand = paymentMethod.Card?.Brand ?? "",
            Last4 = paymentMethod.Card?.Last4 ?? "",
            ExpiryMonth = (int)(paymentMethod.Card?.ExpMonth ?? 0),
            ExpiryYear = (int)(paymentMethod.Card?.ExpYear ?? 0),
            IsDefault = request.IsDefault,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public async Task<bool> RemovePaymentMethodAsync(string paymentMethodId, Guid userId, Guid tenantId)
    {
        try
        {
            var service = new PaymentMethodService();
            await service.DetachAsync(paymentMethodId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<TransactionDto>> GetTransactionHistoryAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20)
    {
        // This is a simplified implementation
        // In production, you'd store payment transactions in your database
        var transactions = new List<TransactionDto>();

        // Mock data
        for (int i = 0; i < pageSize; i++)
        {
            transactions.Add(new TransactionDto
            {
                Id = Guid.NewGuid(),
                Amount = 29.99m,
                Currency = "USD",
                Status = PaymentStatus.Succeeded,
                Type = "payment",
                Description = "Monthly subscription",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-i)
            });
        }

        return transactions;
    }

    private async Task<Customer> GetOrCreateStripeCustomer(Guid userId, Guid tenantId)
    {
        // Check if customer already exists
        var user = await _dbContext.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user?.StripeCustomerId != null)
        {
            var customerService = new CustomerService();
            return await customerService.GetAsync(user.StripeCustomerId);
        }

        // Create new customer
        var options = new CustomerCreateOptions
        {
            Email = user?.Email,
            Name = $"{user?.FirstName} {user?.LastName}",
            Metadata = new Dictionary<string, string>
            {
                { "tenant_id", tenantId.ToString() },
                { "user_id", userId.ToString() }
            }
        };

        var service = new CustomerService();
        var customer = await service.CreateAsync(options);

        // Save customer ID to user
        if (user != null)
        {
            user.StripeCustomerId = customer.Id;
            await _dbContext.SaveChangesAsync();
        }

        return customer;
    }

    private async Task UpdateTenantSubscription(Guid tenantId, Guid planId, string stripeSubscriptionId, string billingCycle)
    {
        var existingSubscription = await _dbContext.TenantSubscriptions
            .Where(ts => ts.TenantId == tenantId && 
                        (ts.Status == SubscriptionStatus.Active || ts.Status == SubscriptionStatus.Trialing))
            .FirstOrDefaultAsync();

        if (existingSubscription != null)
        {
            existingSubscription.Status = SubscriptionStatus.Canceled;
            existingSubscription.CancelAt = DateTimeOffset.UtcNow;
        }

        var newSubscription = new TenantSubscription
        {
            TenantId = tenantId,
            PlanId = planId,
            Status = SubscriptionStatus.Active,
            StripeSubscriptionId = stripeSubscriptionId,
            CurrentPeriodStart = DateTimeOffset.UtcNow,
            CurrentPeriodEnd = billingCycle == "yearly" 
                ? DateTimeOffset.UtcNow.AddYears(1)
                : DateTimeOffset.UtcNow.AddMonths(1),
            BillingCycle = billingCycle == "yearly" ? BillingCycle.Yearly : BillingCycle.Monthly
        };

        _dbContext.TenantSubscriptions.Add(newSubscription);
        await _dbContext.SaveChangesAsync();
    }

    private PaymentStatus MapStripeStatus(string stripeStatus)
    {
        return stripeStatus switch
        {
            "requires_payment_method" => PaymentStatus.RequiresPaymentMethod,
            "requires_confirmation" => PaymentStatus.RequiresConfirmation,
            "requires_action" => PaymentStatus.RequiresAction,
            "processing" => PaymentStatus.Processing,
            "succeeded" => PaymentStatus.Succeeded,
            "canceled" => PaymentStatus.Canceled,
            _ => PaymentStatus.Failed
        };
    }
}
