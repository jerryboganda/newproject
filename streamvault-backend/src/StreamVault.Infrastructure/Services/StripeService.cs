using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace StreamVault.Infrastructure.Services
{
    /// <summary>
    /// Stripe integration service for payment processing
    /// </summary>
    public class StripeService
    {
        private readonly ILogger<StripeService> _logger;
        private readonly CustomerService _customerService;
        private readonly ProductService _productService;
        private readonly PriceService _priceService;
        private readonly SubscriptionService _subscriptionService;
        private readonly CheckoutService _checkoutService;
        private readonly PaymentIntentService _paymentIntentService;
        private readonly InvoiceService _invoiceService;
        private readonly WebhookEndpointService _webhookEndpointService;

        public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            var apiKey = configuration["Stripe:SecretKey"] ?? throw new InvalidOperationException("Stripe secret key not configured");
            StripeConfiguration.ApiKey = apiKey;
            
            // Initialize Stripe services
            _customerService = new CustomerService();
            _productService = new ProductService();
            _priceService = new PriceService();
            _subscriptionService = new SubscriptionService();
            _checkoutService = new CheckoutService();
            _paymentIntentService = new PaymentIntentService();
            _invoiceService = new InvoiceService();
            _webhookEndpointService = new WebhookEndpointService();
        }

        /// <summary>
        /// Create a new Stripe customer
        /// </summary>
        public async Task<Customer> CreateCustomerAsync(string email, string name, string? tenantId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating Stripe customer for email: {Email}", email);

                var options = new CustomerCreateOptions
                {
                    Email = email,
                    Name = name,
                    Metadata = tenantId != null ? new Dictionary<string, string> { { "tenant_id", tenantId } } : null
                };

                var customer = await _customerService.CreateAsync(options, cancellationToken: cancellationToken);
                
                _logger.LogInformation("Successfully created Stripe customer: {CustomerId}", customer.Id);
                return customer;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to create Stripe customer for email: {Email}", email);
                throw new StripeException("Customer creation failed", ex);
            }
        }

        /// <summary>
        /// Get customer by ID
        /// </summary>
        public async Task<Customer?> GetCustomerAsync(string customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _customerService.GetAsync(customerId, cancellationToken: cancellationToken);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to retrieve Stripe customer: {CustomerId}", customerId);
                return null;
            }
        }

        /// <summary>
        /// Create a product for subscription plans
        /// </summary>
        public async Task<Product> CreateProductAsync(string name, string description, CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new ProductCreateOptions
                {
                    Name = name,
                    Description = description,
                    Type = "service"
                };

                return await _productService.CreateAsync(options, cancellationToken: cancellationToken);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to create Stripe product: {Name}", name);
                throw new StripeException("Product creation failed", ex);
            }
        }

        /// <summary>
        /// Create a price for a product
        /// </summary>
        public async Task<Price> CreatePriceAsync(string productId, decimal amount, string currency, BillingInterval interval, CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new PriceCreateOptions
                {
                    Product = productId,
                    UnitAmount = (long)(amount * 100), // Convert to cents
                    Currency = currency.ToLower(),
                    Recurring = new PriceRecurringOptions
                    {
                        Interval = interval.ToLower(),
                        IntervalCount = 1
                    }
                };

                return await _priceService.CreateAsync(options, cancellationToken: cancellationToken);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to create Stripe price for product: {ProductId}", productId);
                throw new StripeException("Price creation failed", ex);
            }
        }

        /// <summary>
        /// Create a subscription for a customer
        /// </summary>
        public async Task<Subscription> CreateSubscriptionAsync(string customerId, string priceId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating subscription for customer: {CustomerId}", customerId);

                var options = new SubscriptionCreateOptions
                {
                    Customer = customerId,
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions { Price = priceId }
                    },
                    PaymentBehavior = "create_if_missing",
                    Expand = new List<string> { "latest_invoice.payment_intent" }
                };

                var subscription = await _subscriptionService.CreateAsync(options, cancellationToken: cancellationToken);
                
                _logger.LogInformation("Successfully created subscription: {SubscriptionId}", subscription.Id);
                return subscription;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to create subscription for customer: {CustomerId}", customerId);
                throw new StripeException("Subscription creation failed", ex);
            }
        }

        /// <summary>
        /// Create a checkout session for one-time payment
        /// </summary>
        public async Task<Session> CreateCheckoutSessionAsync(
            string customerId,
            string successUrl,
            string cancelUrl,
            List<SessionLineItemOptions> lineItems,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    Customer = customerId,
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = lineItems,
                    Mode = "payment",
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl
                };

                return await _checkoutService.CreateAsync(options, cancellationToken: cancellationToken);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to create checkout session for customer: {CustomerId}", customerId);
                throw new StripeException("Checkout session creation failed", ex);
            }
        }

        /// <summary>
        /// Create a checkout session for subscription
        /// </summary>
        public async Task<Session> CreateSubscriptionCheckoutSessionAsync(
            string customerId,
            string priceId,
            string successUrl,
            string cancelUrl,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    Customer = customerId,
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Price = priceId,
                            Quantity = 1
                        }
                    },
                    Mode = "subscription",
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl
                };

                return await _checkoutService.CreateAsync(options, cancellationToken: cancellationToken);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to create subscription checkout session for customer: {CustomerId}", customerId);
                throw new StripeException("Subscription checkout session creation failed", ex);
            }
        }

        /// <summary>
        /// Create a payment intent for direct payment
        /// </summary>
        public async Task<PaymentIntent> CreatePaymentIntentAsync(
            decimal amount,
            string currency,
            string customerId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100), // Convert to cents
                    Currency = currency.ToLower(),
                    Customer = customerId,
                    PaymentMethodTypes = new List<string> { "card" },
                    Confirm = true,
                    OffSession = true,
                    PaymentMethod = "pm_card_visa" // This should be retrieved from customer's saved payment methods
                };

                return await _paymentIntentService.CreateAsync(options, cancellationToken: cancellationToken);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to create payment intent for customer: {CustomerId}", customerId);
                throw new StripeException("Payment intent creation failed", ex);
            }
        }

        /// <summary>
        /// Cancel a subscription
        /// </summary>
        public async Task<Subscription> CancelSubscriptionAsync(string subscriptionId, bool atPeriodEnd = false, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Cancelling subscription: {SubscriptionId}", subscriptionId);

                var options = new SubscriptionCancelOptions
                {
                    AtPeriodEnd = atPeriodEnd
                };

                var subscription = await _subscriptionService.CancelAsync(subscriptionId, options, cancellationToken: cancellationToken);
                
                _logger.LogInformation("Successfully cancelled subscription: {SubscriptionId}", subscriptionId);
                return subscription;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to cancel subscription: {SubscriptionId}", subscriptionId);
                throw new StripeException("Subscription cancellation failed", ex);
            }
        }

        /// <summary>
        /// Update subscription (change price, quantity, etc.)
        /// </summary>
        public async Task<Subscription> UpdateSubscriptionAsync(string subscriptionId, string priceId, CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new SubscriptionUpdateOptions
                {
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Id = (await _subscriptionService.GetAsync(subscriptionId, cancellationToken: cancellationToken)).Items.First().Id,
                            Price = priceId
                        }
                    }
                };

                return await _subscriptionService.UpdateAsync(subscriptionId, options, cancellationToken: cancellationToken);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to update subscription: {SubscriptionId}", subscriptionId);
                throw new StripeException("Subscription update failed", ex);
            }
        }

        /// <summary>
        /// Get customer's subscriptions
        /// </summary>
        public async Task<List<Subscription>> GetCustomerSubscriptionsAsync(string customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new SubscriptionListOptions
                {
                    Customer = customerId,
                    Status = "all"
                };

                var subscriptions = await _subscriptionService.ListAsync(options, cancellationToken: cancellationToken);
                return subscriptions.ToList();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to retrieve subscriptions for customer: {CustomerId}", customerId);
                throw new StripeException("Failed to retrieve subscriptions", ex);
            }
        }

        /// <summary>
        /// Create an invoice for a customer
        /// </summary>
        public async Task<Invoice> CreateInvoiceAsync(string customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new InvoiceCreateOptions
                {
                    Customer = customerId,
                    AutoAdvance = true,
                    CollectionMethod = "charge_automatically"
                };

                return await _invoiceService.CreateAsync(options, cancellationToken: cancellationToken);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to create invoice for customer: {CustomerId}", customerId);
                throw new StripeException("Invoice creation failed", ex);
            }
        }

        /// <summary>
        /// Get customer's payment methods
        /// </summary>
        public async Task<List<PaymentMethod>> GetCustomerPaymentMethodsAsync(string customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new PaymentMethodListOptions
                {
                    Customer = customerId,
                    Type = "card"
                };

                var paymentMethods = await new PaymentMethodService().ListAsync(options, cancellationToken: cancellationToken);
                return paymentMethods.ToList();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to retrieve payment methods for customer: {CustomerId}", customerId);
                throw new StripeException("Failed to retrieve payment methods", ex);
            }
        }

        /// <summary>
        /// Construct webhook event from request
        /// </summary>
        public Event ConstructWebhookEvent(string json, string signatureHeader, string webhookSecret)
        {
            try
            {
                return EventUtility.ConstructEvent(
                    json,
                    signatureHeader,
                    webhookSecret,
                    300, // tolerance in seconds
                    false // throw on api version mismatch
                );
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to construct webhook event");
                throw new StripeException("Invalid webhook signature", ex);
            }
        }
    }

    public enum BillingInterval
    {
        Month,
        Year
    }

    public class StripeException : Exception
    {
        public StripeException(string message) : base(message) { }
        public StripeException(string message, Exception innerException) : base(message, innerException) { }
    }
}
