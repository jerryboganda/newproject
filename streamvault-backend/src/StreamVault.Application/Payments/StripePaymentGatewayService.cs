using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using StreamVault.Application.Payments.DTOs;
using StreamVault.Domain.Entities;

namespace StreamVault.Application.Payments;

public class StripePaymentGatewayService : IPaymentGatewayService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentGatewayService> _logger;

    public StripePaymentGatewayService(IConfiguration configuration, ILogger<StripePaymentGatewayService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Configure Stripe
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    public async Task<PaymentIntentDto> CreatePaymentIntentAsync(CreatePaymentIntentRequest request)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100), // Convert to cents
                Currency = request.Currency.ToLower(),
                Customer = request.CustomerId,
                PaymentMethod = request.PaymentMethodId,
                SetupFutureUsage = request.SetupFutureUsage ? "off_session" : null,
                Description = request.Description,
                Metadata = request.Metadata ?? new Dictionary<string, string>(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                    AllowRedirects = "never"
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return new PaymentIntentDto
            {
                Id = paymentIntent.Id,
                Amount = paymentIntent.Amount / 100m,
                Currency = paymentIntent.Currency.ToUpper(),
                Status = MapPaymentStatus(paymentIntent.Status),
                ClientSecret = paymentIntent.ClientSecret,
                PaymentMethodId = paymentIntent.PaymentMethodId,
                CustomerId = paymentIntent.CustomerId,
                Description = paymentIntent.Description,
                Metadata = paymentIntent.Metadata,
                CreatedAt = paymentIntent.Created,
                ConfirmedAt = paymentIntent.Status == "succeeded" ? DateTimeOffset.UtcNow : null
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error creating payment intent");
            throw new Exception($"Payment intent creation failed: {ex.Message}");
        }
    }

    public async Task<PaymentIntentDto> ConfirmPaymentAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.ConfirmAsync(paymentIntentId);

            return new PaymentIntentDto
            {
                Id = paymentIntent.Id,
                Amount = paymentIntent.Amount / 100m,
                Currency = paymentIntent.Currency.ToUpper(),
                Status = MapPaymentStatus(paymentIntent.Status),
                ClientSecret = paymentIntent.ClientSecret,
                PaymentMethodId = paymentIntent.PaymentMethodId,
                CustomerId = paymentIntent.CustomerId,
                Description = paymentIntent.Description,
                Metadata = paymentIntent.Metadata,
                CreatedAt = paymentIntent.Created,
                ConfirmedAt = paymentIntent.Status == "succeeded" ? DateTimeOffset.UtcNow : null
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error confirming payment");
            throw new Exception($"Payment confirmation failed: {ex.Message}");
        }
    }

    public async Task<bool> CancelPaymentAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.CancelAsync(paymentIntentId);

            return paymentIntent.Status == "canceled";
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error canceling payment");
            throw new Exception($"Payment cancellation failed: {ex.Message}");
        }
    }

    public async Task<RefundDto> CreateRefundAsync(CreateRefundRequest request)
    {
        try
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = request.PaymentIntentId,
                Amount = request.Amount.HasValue ? (long)(request.Amount.Value * 100) : null,
                Reason = request.Reason ?? "requested_by_customer",
                Metadata = request.Metadata ?? new Dictionary<string, string>()
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options);

            return new RefundDto
            {
                Id = refund.Id,
                Amount = refund.Amount / 100m,
                Currency = refund.Currency.ToUpper(),
                PaymentIntentId = refund.PaymentIntentId,
                Status = refund.Status,
                Reason = refund.Reason,
                CreatedAt = refund.Created
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error creating refund");
            throw new Exception($"Refund creation failed: {ex.Message}");
        }
    }

    public async Task<PaymentMethodDto> CreatePaymentMethodAsync(CreatePaymentMethodRequest request)
    {
        try
        {
            var options = new PaymentMethodCreateOptions
            {
                Type = request.Type,
                Customer = request.CustomerId,
                Card = request.Card != null ? new PaymentMethodCardOptions
                {
                    Number = request.Card.Number,
                    ExpMonth = request.Card.ExpMonth,
                    ExpYear = request.Card.ExpYear,
                    Cvc = request.Card.Cvc,
                    Name = request.Card.Name
                } : null,
                Metadata = request.Metadata ?? new Dictionary<string, string>()
            };

            var service = new PaymentMethodService();
            var paymentMethod = await service.CreateAsync(options);

            // Attach to customer if not already attached
            if (!string.IsNullOrEmpty(request.CustomerId))
            {
                var attachOptions = new PaymentMethodAttachOptions
                {
                    Customer = request.CustomerId
                };
                await service.AttachAsync(paymentMethod.Id, attachOptions);
            }

            // Set as default if requested
            if (request.IsDefault)
            {
                var customerOptions = new CustomerUpdateOptions
                {
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = paymentMethod.Id
                    }
                };
                var customerService = new CustomerService();
                await customerService.UpdateAsync(request.CustomerId, customerOptions);
            }

            return MapPaymentMethod(paymentMethod);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error creating payment method");
            throw new Exception($"Payment method creation failed: {ex.Message}");
        }
    }

    public async Task<bool> DeletePaymentMethodAsync(string paymentMethodId)
    {
        try
        {
            var service = new PaymentMethodService();
            await service.DetachAsync(paymentMethodId);
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error deleting payment method");
            throw new Exception($"Payment method deletion failed: {ex.Message}");
        }
    }

    public async Task<List<PaymentMethodDto>> GetCustomerPaymentMethodsAsync(string customerId, int? limit = null)
    {
        try
        {
            var options = new PaymentMethodListOptions
            {
                Customer = customerId,
                Type = "card",
                Limit = limit
            };

            var service = new PaymentMethodService();
            var paymentMethods = await service.ListAsync(options);

            return paymentMethods.Data.Select(MapPaymentMethod).ToList();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error retrieving payment methods");
            throw new Exception($"Payment methods retrieval failed: {ex.Message}");
        }
    }

    public async Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionRequest request)
    {
        try
        {
            var options = new SubscriptionCreateOptions
            {
                Customer = request.CustomerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = request.PriceId
                    }
                },
                PaymentBehavior = "default_incomplete",
                PaymentSettings = new SubscriptionPaymentSettingsOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    SaveDefaultPaymentMethod = "on_subscription"
                },
                Coupon = request.CouponCode,
                TrialEnd = request.TrialEnd?.ToUnixTimeSeconds(),
                Metadata = request.Metadata ?? new Dictionary<string, string>()
            };

            var service = new SubscriptionService();
            var subscription = await service.CreateAsync(options);

            return MapSubscription(subscription);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error creating subscription");
            throw new Exception($"Subscription creation failed: {ex.Message}");
        }
    }

    public async Task<SubscriptionDto> UpdateSubscriptionAsync(string subscriptionId, UpdateSubscriptionRequest request)
    {
        try
        {
            var options = new SubscriptionUpdateOptions
            {
                ProrationBehavior = request.ProrationBehavior.HasValue ? 
                    (request.ProrationBehavior.Value ? "create_prorations" : "none") : "create_prorations",
                Metadata = request.Metadata ?? new Dictionary<string, string>()
            };

            if (!string.IsNullOrEmpty(request.PriceId))
            {
                options.Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Id = subscriptionId, // This should be the subscription item ID
                        Price = request.PriceId
                    }
                };
            }

            if (!string.IsNullOrEmpty(request.PaymentMethodId))
            {
                options.DefaultPaymentMethod = request.PaymentMethodId;
            }

            if (!string.IsNullOrEmpty(request.CouponCode))
            {
                options.Coupon = request.CouponCode;
            }

            var service = new SubscriptionService();
            var subscription = await service.UpdateAsync(subscriptionId, options);

            return MapSubscription(subscription);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error updating subscription");
            throw new Exception($"Subscription update failed: {ex.Message}");
        }
    }

    public async Task<bool> CancelSubscriptionAsync(string subscriptionId, bool immediate = false)
    {
        try
        {
            var service = new SubscriptionService();
            
            if (immediate)
            {
                await service.CancelAsync(subscriptionId);
            }
            else
            {
                var options = new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = true
                };
                await service.UpdateAsync(subscriptionId, options);
            }

            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error canceling subscription");
            throw new Exception($"Subscription cancellation failed: {ex.Message}");
        }
    }

    public async Task<InvoiceDto> GetInvoiceAsync(string invoiceId)
    {
        try
        {
            var service = new InvoiceService();
            var invoice = await service.GetAsync(invoiceId);

            return MapInvoice(invoice);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error retrieving invoice");
            throw new Exception($"Invoice retrieval failed: {ex.Message}");
        }
    }

    public async Task<List<InvoiceDto>> GetCustomerInvoicesAsync(string customerId, int? limit = null)
    {
        try
        {
            var options = new InvoiceListOptions
            {
                Customer = customerId,
                Limit = limit
            };

            var service = new InvoiceService();
            var invoices = await service.ListAsync(options);

            return invoices.Data.Select(MapInvoice).ToList();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error retrieving invoices");
            throw new Exception($"Invoices retrieval failed: {ex.Message}");
        }
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
    {
        try
        {
            var options = new CustomerCreateOptions
            {
                Email = request.Email,
                Name = request.Name,
                Phone = request.Phone,
                Address = request.Address != null ? new AddressOptions
                {
                    Line1 = request.Address.Line1,
                    Line2 = request.Address.Line2,
                    City = request.Address.City,
                    State = request.Address.State,
                    PostalCode = request.Address.PostalCode,
                    Country = request.Address.Country
                } : null,
                PaymentMethod = request.PaymentMethodId,
                InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = request.PaymentMethodId
                },
                Metadata = request.Metadata ?? new Dictionary<string, string>()
            };

            var service = new CustomerService();
            var customer = await service.CreateAsync(options);

            return MapCustomer(customer);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error creating customer");
            throw new Exception($"Customer creation failed: {ex.Message}");
        }
    }

    public async Task<CustomerDto> UpdateCustomerAsync(string customerId, UpdateCustomerRequest request)
    {
        try
        {
            var options = new CustomerUpdateOptions
            {
                Email = request.Email,
                Name = request.Name,
                Phone = request.Phone,
                Address = request.Address != null ? new AddressOptions
                {
                    Line1 = request.Address.Line1,
                    Line2 = request.Address.Line2,
                    City = request.Address.City,
                    State = request.Address.State,
                    PostalCode = request.Address.PostalCode,
                    Country = request.Address.Country
                } : null,
                Metadata = request.Metadata ?? new Dictionary<string, string>()
            };

            var service = new CustomerService();
            var customer = await service.UpdateAsync(customerId, options);

            return MapCustomer(customer);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error updating customer");
            throw new Exception($"Customer update failed: {ex.Message}");
        }
    }

    public async Task<CustomerDto> GetCustomerAsync(string customerId)
    {
        try
        {
            var service = new CustomerService();
            var customer = await service.GetAsync(customerId);

            return MapCustomer(customer);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error retrieving customer");
            throw new Exception($"Customer retrieval failed: {ex.Message}");
        }
    }

    private static PaymentStatus MapPaymentStatus(string status)
    {
        return status switch
        {
            "requires_payment_method" => PaymentStatus.RequiresPaymentMethod,
            "requires_confirmation" => PaymentStatus.RequiresConfirmation,
            "requires_action" => PaymentStatus.RequiresAction,
            "processing" => PaymentStatus.Processing,
            "succeeded" => PaymentStatus.Succeeded,
            "canceled" => PaymentStatus.Canceled,
            _ => PaymentStatus.RequiresPaymentMethod
        };
    }

    private static PaymentMethodDto MapPaymentMethod(PaymentMethod paymentMethod)
    {
        return new PaymentMethodDto
        {
            Id = paymentMethod.Id,
            Type = paymentMethod.Type,
            Card = paymentMethod.Card != null ? new CardDto
            {
                Brand = paymentMethod.Card.Brand,
                Last4 = paymentMethod.Card.Last4,
                ExpMonth = paymentMethod.Card.ExpMonth.ToString(),
                ExpYear = paymentMethod.Card.ExpYear.ToString(),
                Funding = paymentMethod.Card.Funding,
                Country = paymentMethod.Card.Country
            } : null,
            CustomerId = paymentMethod.CustomerId,
            IsDefault = false, // Would need to check customer's default payment method
            CreatedAt = paymentMethod.Created
        };
    }

    private static SubscriptionDto MapSubscription(Subscription subscription)
    {
        return new SubscriptionDto
        {
            Id = subscription.Id,
            CustomerId = subscription.CustomerId,
            PriceId = subscription.Items.Data.FirstOrDefault()?.Price.Id ?? string.Empty,
            Status = MapSubscriptionStatus(subscription.Status),
            CurrentPeriodStart = subscription.CurrentPeriodStart,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd,
            TrialEnd = subscription.TrialEnd,
            CanceledAt = subscription.CanceledAt,
            EndedAt = subscription.EndedAt,
            Amount = subscription.Items.Data.FirstOrDefault()?.Price.UnitAmountDecimal / 100m ?? 0,
            Currency = subscription.Items.Data.FirstOrDefault()?.Price.Currency.ToUpper() ?? "USD",
            BillingCycle = subscription.Items.Data.FirstOrDefault()?.Price.Recurring?.Interval ?? "month",
            CreatedAt = subscription.Created
        };
    }

    private static SubscriptionStatus MapSubscriptionStatus(string status)
    {
        return status switch
        {
            "trialing" => SubscriptionStatus.Trialing,
            "active" => SubscriptionStatus.Active,
            "past_due" => SubscriptionStatus.PastDue,
            "canceled" => SubscriptionStatus.Canceled,
            "unpaid" => SubscriptionStatus.Unpaid,
            "incomplete" => SubscriptionStatus.Incomplete,
            "incomplete_expired" => SubscriptionStatus.IncompleteExpired,
            _ => SubscriptionStatus.Incomplete
        };
    }

    private static InvoiceDto MapInvoice(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            CustomerId = invoice.CustomerId,
            SubscriptionId = invoice.SubscriptionId ?? string.Empty,
            AmountDue = invoice.AmountDue / 100m,
            AmountPaid = invoice.AmountPaid / 100m,
            AmountRemaining = invoice.AmountRemaining / 100m,
            Currency = invoice.Currency.ToUpper(),
            Status = MapInvoiceStatus(invoice.Status),
            CreatedAt = invoice.Created,
            DueDate = invoice.DueDate,
            HostedInvoiceUrl = invoice.HostedInvoiceUrl,
            InvoicePdf = invoice.InvoicePdf
        };
    }

    private static InvoiceStatus MapInvoiceStatus(string status)
    {
        return status switch
        {
            "draft" => InvoiceStatus.Draft,
            "open" => InvoiceStatus.Open,
            "paid" => InvoiceStatus.Paid,
            "void" => InvoiceStatus.Void,
            "uncollectible" => InvoiceStatus.Uncollectible,
            _ => InvoiceStatus.Draft
        };
    }

    private static CustomerDto MapCustomer(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Email = customer.Email,
            Name = customer.Name,
            Phone = customer.Phone,
            Address = customer.Address != null ? new AddressDto
            {
                Line1 = customer.Address.Line1,
                Line2 = customer.Address.Line2,
                City = customer.Address.City,
                State = customer.Address.State,
                PostalCode = customer.Address.PostalCode,
                Country = customer.Address.Country
            } : null,
            CreatedAt = customer.Created,
            Metadata = customer.Metadata
        };
    }
}
