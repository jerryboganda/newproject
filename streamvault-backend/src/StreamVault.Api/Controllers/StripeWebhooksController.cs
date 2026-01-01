using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("webhooks/stripe")]
[DisableRateLimiting]
public class StripeWebhooksController : ControllerBase
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeWebhooksController> _logger;

    public StripeWebhooksController(StreamVaultDbContext dbContext, IConfiguration configuration, ILogger<StripeWebhooksController> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        var webhookSecret = _configuration["Stripe:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(webhookSecret))
            return StatusCode(500, new { error = "Stripe webhook secret not configured (Stripe:WebhookSecret)" });

        var apiKey = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return StatusCode(500, new { error = "Stripe secret key not configured (Stripe:SecretKey)" });

        StripeConfiguration.ApiKey = apiKey;

        var json = await new StreamReader(Request.Body, Encoding.UTF8).ReadToEndAsync(cancellationToken);
        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, webhookSecret);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Stripe webhook signature verification failed");
            return BadRequest();
        }

        try
        {
            switch (stripeEvent.Type)
            {
                case EventTypes.CheckoutSessionCompleted:
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session != null)
                        await HandleCheckoutCompletedAsync(session, cancellationToken);
                    break;
                }

                case EventTypes.CustomerSubscriptionCreated:
                case EventTypes.CustomerSubscriptionUpdated:
                case EventTypes.CustomerSubscriptionDeleted:
                {
                    var subscription = stripeEvent.Data.Object as Subscription;
                    if (subscription != null)
                        await HandleSubscriptionChangedAsync(subscription, cancellationToken);
                    break;
                }

                case EventTypes.InvoicePaid:
                {
                    var invoice = stripeEvent.Data.Object as Stripe.Invoice;
                    if (invoice != null)
                        await HandleInvoicePaidAsync(invoice, cancellationToken);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe webhook processing failed. type={EventType}", stripeEvent.Type);
            // Ack 200 to avoid retry storms; failures should be visible in logs.
        }

        return Ok(new { received = true });
    }

    private async Task HandleCheckoutCompletedAsync(Session session, CancellationToken cancellationToken)
    {
        var tenantIdString = session.Metadata != null && session.Metadata.TryGetValue("tenant_id", out var t) ? t : null;
        if (!Guid.TryParse(tenantIdString, out var tenantId))
        {
            _logger.LogWarning("Stripe checkout completed but tenant_id missing in metadata. session={SessionId}", session.Id);
            return;
        }

        var billing = await _dbContext.TenantBillingAccounts
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (billing == null)
        {
            billing = new TenantBillingAccount
            {
                TenantId = tenantId,
                Currency = "USD",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.TenantBillingAccounts.Add(billing);
        }

        billing.StripeCustomerId = session.CustomerId ?? billing.StripeCustomerId;
        billing.StripeSubscriptionId = session.SubscriptionId ?? billing.StripeSubscriptionId;
        billing.UpdatedAt = DateTimeOffset.UtcNow;

        var planIdString = session.Metadata != null && session.Metadata.TryGetValue("plan_id", out var p) ? p : null;
        if (Guid.TryParse(planIdString, out var planId))
        {
            var interval = session.Metadata != null && session.Metadata.TryGetValue("interval", out var i) ? i : "monthly";

            var existingSub = await _dbContext.TenantSubscriptions
                .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

            if (existingSub == null)
            {
                existingSub = new TenantSubscription
                {
                    TenantId = tenantId,
                    PlanId = planId,
                    Status = SubscriptionStatus.Active,
                    BillingCycle = interval.Equals("yearly", StringComparison.OrdinalIgnoreCase) ? BillingCycle.Yearly : BillingCycle.Monthly,
                    StripeSubscriptionId = billing.StripeSubscriptionId,
                    StripeCustomerId = billing.StripeCustomerId
                };
                _dbContext.TenantSubscriptions.Add(existingSub);
            }
            else
            {
                existingSub.PlanId = planId;
                existingSub.Status = SubscriptionStatus.Active;
                existingSub.BillingCycle = interval.Equals("yearly", StringComparison.OrdinalIgnoreCase) ? BillingCycle.Yearly : BillingCycle.Monthly;
                existingSub.StripeSubscriptionId = billing.StripeSubscriptionId;
                existingSub.StripeCustomerId = billing.StripeCustomerId;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleSubscriptionChangedAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        var customerId = subscription.CustomerId ?? subscription.Customer?.Id;
        if (string.IsNullOrWhiteSpace(customerId))
            return;

        var billing = await _dbContext.TenantBillingAccounts
            .FirstOrDefaultAsync(x => x.StripeCustomerId == customerId, cancellationToken);

        if (billing == null)
            return;

        billing.StripeSubscriptionId = subscription.Id;
        billing.UpdatedAt = DateTimeOffset.UtcNow;

        var sub = await _dbContext.TenantSubscriptions
            .FirstOrDefaultAsync(s => s.TenantId == billing.TenantId, cancellationToken);

        if (sub != null)
        {
            var firstItem = subscription.Items?.Data?.FirstOrDefault();

            sub.StripeSubscriptionId = subscription.Id;
            sub.StripeCustomerId = customerId;
            sub.CurrentPeriodStart = firstItem == null
                ? null
                : new DateTimeOffset(DateTime.SpecifyKind(firstItem.CurrentPeriodStart, DateTimeKind.Utc));
            sub.CurrentPeriodEnd = firstItem == null
                ? null
                : new DateTimeOffset(DateTime.SpecifyKind(firstItem.CurrentPeriodEnd, DateTimeKind.Utc));
            sub.CancelAt = subscription.CancelAt.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(subscription.CancelAt.Value, DateTimeKind.Utc))
                : null;
            sub.TrialEnd = subscription.TrialEnd.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(subscription.TrialEnd.Value, DateTimeKind.Utc))
                : null;

            sub.Status = subscription.Status switch
            {
                "active" => SubscriptionStatus.Active,
                "trialing" => SubscriptionStatus.Trialing,
                "canceled" => SubscriptionStatus.Canceled,
                "incomplete" => SubscriptionStatus.Incomplete,
                "past_due" => SubscriptionStatus.PastDue,
                _ => sub.Status
            };
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleInvoicePaidAsync(Stripe.Invoice invoice, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Stripe invoice paid. invoice={InvoiceId} customer={CustomerId} amountPaid={AmountPaid} currency={Currency}",
            invoice.Id,
            invoice.CustomerId,
            invoice.AmountPaid,
            invoice.Currency);

        await Task.CompletedTask;
    }
}
