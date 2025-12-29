using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;
using Stripe;

namespace StreamVault.Application.Services
{
    public interface IBillingService
    {
        Task<TenantSubscription> CreateSubscriptionAsync(Guid tenantId, Guid planId, string paymentMethodId);
        Task CancelSubscriptionAsync(Guid subscriptionId, bool immediate = false);
        Task UpdateSubscriptionAsync(Guid subscriptionId, Guid newPlanId);
        Task<TenantSubscription> GetSubscriptionAsync(Guid tenantId);
        Task<IEnumerable<SubscriptionPlan>> GetAvailablePlansAsync();
        Task<TenantInvoice> GenerateInvoiceAsync(Guid tenantId, DateTime periodStart, DateTime periodEnd);
        Task<IEnumerable<TenantInvoice>> GetInvoicesAsync(Guid tenantId);
        Task<UsageMetrics> CalculateUsageAsync(Guid tenantId, DateTime periodStart, DateTime periodEnd);
        Task<PaymentMethod> AddPaymentMethodAsync(Guid tenantId, string paymentMethodId);
        Task RemovePaymentMethodAsync(Guid tenantId, string paymentMethodId);
        Task<IEnumerable<PaymentMethod>> GetPaymentMethodsAsync(Guid tenantId);
        Task ProcessPaymentAsync(Guid invoiceId);
    }

    public class BillingService : IBillingService
    {
        private readonly StreamVaultDbContext _context;
        private readonly IStripeService _stripeService;
        private readonly IAnalyticsService _analyticsService;

        public BillingService(StreamVaultDbContext context, IStripeService stripeService, IAnalyticsService analyticsService)
        {
            _context = context;
            _stripeService = stripeService;
            _analyticsService = analyticsService;
        }

        public async Task<TenantSubscription> CreateSubscriptionAsync(Guid tenantId, Guid planId, string paymentMethodId)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                throw new ArgumentException("Tenant not found", nameof(tenantId));

            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null)
                throw new ArgumentException("Plan not found", nameof(planId));

            // Check if tenant already has an active subscription
            var existingSubscription = await _context.TenantSubscriptions
                .FirstOrDefaultAsync(ts => ts.TenantId == tenantId && 
                                          ts.Status == SubscriptionStatus.Active);

            if (existingSubscription != null)
                throw new InvalidOperationException("Tenant already has an active subscription");

            // Create Stripe subscription
            var stripeSubscription = await _stripeService.CreateSubscriptionAsync(tenant.StripeCustomerId, plan.StripePriceId, paymentMethodId);

            var subscription = new TenantSubscription
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PlanId = planId,
                StripeSubscriptionId = stripeSubscription.Id,
                Status = SubscriptionStatus.Active,
                CurrentPeriodStart = stripeSubscription.CurrentPeriodStart,
                CurrentPeriodEnd = stripeSubscription.CurrentPeriodEnd,
                CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TenantSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return subscription;
        }

        public async Task CancelSubscriptionAsync(Guid subscriptionId, bool immediate = false)
        {
            var subscription = await _context.TenantSubscriptions
                .Include(ts => ts.Tenant)
                .FirstOrDefaultAsync(ts => ts.Id == subscriptionId);

            if (subscription == null)
                throw new ArgumentException("Subscription not found", nameof(subscriptionId));

            if (immediate)
            {
                await _stripeService.CancelSubscriptionImmediatelyAsync(subscription.StripeSubscriptionId);
                subscription.Status = SubscriptionStatus.Canceled;
                subscription.CanceledAt = DateTime.UtcNow;
            }
            else
            {
                await _stripeService.CancelSubscriptionAtPeriodEndAsync(subscription.StripeSubscriptionId);
                subscription.CancelAtPeriodEnd = true;
            }

            subscription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSubscriptionAsync(Guid subscriptionId, Guid newPlanId)
        {
            var subscription = await _context.TenantSubscriptions
                .Include(ts => ts.Plan)
                .FirstOrDefaultAsync(ts => ts.Id == subscriptionId);

            if (subscription == null)
                throw new ArgumentException("Subscription not found", nameof(subscriptionId));

            var newPlan = await _context.SubscriptionPlans.FindAsync(newPlanId);
            if (newPlan == null)
                throw new ArgumentException("Plan not found", nameof(newPlanId));

            // Update Stripe subscription
            await _stripeService.UpdateSubscriptionAsync(subscription.StripeSubscriptionId, newPlan.StripePriceId);

            subscription.PlanId = newPlanId;
            subscription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<TenantSubscription> GetSubscriptionAsync(Guid tenantId)
        {
            return await _context.TenantSubscriptions
                .Include(ts => ts.Plan)
                .FirstOrDefaultAsync(ts => ts.TenantId == tenantId && 
                                          (ts.Status == SubscriptionStatus.Active || 
                                           ts.Status == SubscriptionStatus.Trialing));
        }

        public async Task<IEnumerable<SubscriptionPlan>> GetAvailablePlansAsync()
        {
            return await _context.SubscriptionPlans
                .Where(sp => sp.IsActive)
                .OrderBy(sp => sp.Price)
                .ToListAsync();
        }

        public async Task<TenantInvoice> GenerateInvoiceAsync(Guid tenantId, DateTime periodStart, DateTime periodEnd)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                throw new ArgumentException("Tenant not found", nameof(tenantId));

            var subscription = await GetSubscriptionAsync(tenantId);
            if (subscription == null)
                throw new InvalidOperationException("No active subscription found");

            // Calculate usage
            var usage = await CalculateUsageAsync(tenantId, periodStart, periodEnd);

            // Calculate costs
            var baseCost = subscription.Plan.Price;
            var overageCost = CalculateOverageCost(usage, subscription.Plan);
            var totalAmount = baseCost + overageCost;

            // Create Stripe invoice
            var stripeInvoice = await _stripeService.CreateInvoiceAsync(tenant.StripeCustomerId, totalAmount * 100); // Convert to cents

            var invoice = new TenantInvoice
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SubscriptionId = subscription.Id,
                StripeInvoiceId = stripeInvoice.Id,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                BaseAmount = baseCost,
                OverageAmount = overageCost,
                TotalAmount = totalAmount,
                Status = InvoiceStatus.Draft,
                Currency = subscription.Plan.Currency,
                DueDate = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UsageBreakdown = new Dictionary<string, object>
                {
                    ["StorageGB"] = usage.StorageUsedGB,
                    ["BandwidthGB"] = usage.BandwidthUsedGB,
                    ["Views"] = usage.TotalViews,
                    ["Videos"] = usage.VideoCount
                }
            };

            _context.TenantInvoices.Add(invoice);
            await _context.SaveChangesAsync();

            return invoice;
        }

        public async Task<IEnumerable<TenantInvoice>> GetInvoicesAsync(Guid tenantId)
        {
            return await _context.TenantInvoices
                .Where(ti => ti.TenantId == tenantId)
                .OrderByDescending(ti => ti.CreatedAt)
                .ToListAsync();
        }

        public async Task<UsageMetrics> CalculateUsageAsync(Guid tenantId, DateTime periodStart, DateTime periodEnd)
        {
            var videos = await _context.Videos
                .Where(v => v.TenantId == tenantId && 
                           v.CreatedAt >= periodStart && 
                           v.CreatedAt <= periodEnd)
                .ToListAsync();

            var videoIds = videos.Select(v => v.Id).ToList();
            var views = await _context.VideoViews
                .Where(vv => videoIds.Contains(vv.VideoId) && 
                             vv.ViewedAt >= periodStart && 
                             vv.ViewedAt <= periodEnd)
                .ToListAsync();

            return new UsageMetrics
            {
                StorageUsedGB = videos.Sum(v => v.FileSize) / (1024.0 * 1024.0 * 1024.0),
                BandwidthUsedGB = views.Sum(v => v.Video.FileSize) / (1024.0 * 1024.0 * 1024.0),
                TotalViews = views.Count,
                VideoCount = videos.Count,
                UniqueViewers = views.Select(v => v.UserId).Distinct().Count(),
                AverageVideoDuration = videos.Any() ? videos.Average(v => v.Duration.TotalMinutes) : 0
            };
        }

        public async Task<PaymentMethod> AddPaymentMethodAsync(Guid tenantId, string paymentMethodId)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                throw new ArgumentException("Tenant not found", nameof(tenantId));

            // Retrieve payment method from Stripe
            var stripePaymentMethod = await _stripeService.GetPaymentMethodAsync(paymentMethodId);

            var paymentMethod = new PaymentMethod
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                StripePaymentMethodId = paymentMethodId,
                Type = stripePaymentMethod.Type,
                Brand = stripePaymentMethod.Card?.Brand,
                Last4 = stripePaymentMethod.Card?.Last4,
                ExpiryMonth = stripePaymentMethod.Card?.ExpMonth,
                ExpiryYear = stripePaymentMethod.Card?.ExpYear,
                IsDefault = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.PaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync();

            return paymentMethod;
        }

        public async Task RemovePaymentMethodAsync(Guid tenantId, string paymentMethodId)
        {
            var paymentMethod = await _context.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.TenantId == tenantId && 
                                          pm.StripePaymentMethodId == paymentMethodId);

            if (paymentMethod == null)
                throw new ArgumentException("Payment method not found");

            await _stripeService.DetachPaymentMethodAsync(paymentMethodId);
            _context.PaymentMethods.Remove(paymentMethod);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PaymentMethod>> GetPaymentMethodsAsync(Guid tenantId)
        {
            return await _context.PaymentMethods
                .Where(pm => pm.TenantId == tenantId)
                .OrderByDescending(pm => pm.IsDefault)
                .ThenByDescending(pm => pm.CreatedAt)
                .ToListAsync();
        }

        public async Task ProcessPaymentAsync(Guid invoiceId)
        {
            var invoice = await _context.TenantInvoices
                .Include(ti => ti.Tenant)
                .FirstOrDefaultAsync(ti => ti.Id == invoiceId);

            if (invoice == null)
                throw new ArgumentException("Invoice not found", nameof(invoiceId));

            // Finalize and pay Stripe invoice
            await _stripeService.PayInvoiceAsync(invoice.StripeInvoiceId);

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;
            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        private decimal CalculateOverageCost(UsageMetrics usage, SubscriptionPlan plan)
        {
            decimal cost = 0;

            // Storage overage
            if (usage.StorageUsedGB > plan.StorageLimitGB)
            {
                var storageOverage = usage.StorageUsedGB - plan.StorageLimitGB;
                cost += storageOverage * plan.StorageOveragePricePerGB;
            }

            // Bandwidth overage
            if (usage.BandwidthUsedGB > plan.BandwidthLimitGB)
            {
                var bandwidthOverage = usage.BandwidthUsedGB - plan.BandwidthLimitGB;
                cost += bandwidthOverage * plan.BandwidthOveragePricePerGB;
            }

            // Video count overage
            if (usage.VideoCount > plan.VideoLimit)
            {
                var videoOverage = usage.VideoCount - plan.VideoLimit;
                cost += videoOverage * plan.VideoOveragePrice;
            }

            return cost;
        }
    }

    public class UsageMetrics
    {
        public double StorageUsedGB { get; set; }
        public double BandwidthUsedGB { get; set; }
        public int TotalViews { get; set; }
        public int VideoCount { get; set; }
        public int UniqueViewers { get; set; }
        public double AverageVideoDuration { get; set; }
    }

    public enum SubscriptionStatus
    {
        Incomplete,
        IncompleteExpired,
        Trialing,
        Active,
        PastDue,
        Canceled,
        Unpaid
    }

    public enum InvoiceStatus
    {
        Draft,
        Open,
        Paid,
        Void,
        Uncollectible
    }
}
