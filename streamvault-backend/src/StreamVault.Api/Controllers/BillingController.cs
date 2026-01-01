using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using StreamVault.Application.Interfaces;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/billing")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BillingController> _logger;

    public BillingController(
        StreamVaultDbContext dbContext,
        ITenantContext tenantContext,
        IConfiguration configuration,
        ILogger<BillingController> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("plans")]
    public async Task<ActionResult<IReadOnlyList<SubscriptionPlanDto>>> GetPlans(CancellationToken cancellationToken)
    {
        var plans = await _dbContext.SubscriptionPlans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Name)
            .Select(p => new SubscriptionPlanDto(
                p.Id,
                p.Name,
                p.Slug,
                p.Description,
                p.PriceMonthly,
                p.PriceYearly,
                p.StripePriceIdMonthly,
                p.StripePriceIdYearly,
                p.IsCustom))
            .ToListAsync(cancellationToken);

        return Ok(plans);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<BillingSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var tenantId = await RequireTenantIdAsync(cancellationToken);

        var billingAccount = await _dbContext.TenantBillingAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var subscription = await _dbContext.TenantSubscriptions
            .AsNoTracking()
            .Include(s => s.Plan)
            .OrderByDescending(s => s.CurrentPeriodEnd)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

        var latestUsage = await _dbContext.TenantUsageSnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.PeriodStartUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var manualPayments = await _dbContext.ManualPayments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.PaidAt)
            .Take(20)
            .Select(x => new ManualPaymentDto(x.Id, x.Amount, x.Currency, x.PaidAt, x.Reference, x.Notes))
            .ToListAsync(cancellationToken);

        var invoices = Array.Empty<StripeInvoiceDto>();
        if (!string.IsNullOrWhiteSpace(billingAccount?.StripeCustomerId))
        {
            EnsureStripeConfigured();

            var invoiceService = new InvoiceService();
            var invoiceList = await invoiceService.ListAsync(new InvoiceListOptions
            {
                Customer = billingAccount.StripeCustomerId,
                Limit = 20
            }, cancellationToken: cancellationToken);

            invoices = invoiceList.Data
                .OrderByDescending(i => i.Created)
                .Select(i => new StripeInvoiceDto(
                    i.Id,
                    i.Status,
                    i.AmountDue / 100m,
                    i.Currency?.ToUpperInvariant() ?? "USD",
                    i.HostedInvoiceUrl,
                    i.InvoicePdf,
                    i.Created,
                    i.DueDate))
                .ToArray();
        }

        var globalMultipliers = await _dbContext.UsageMultipliers
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.MetricType)
            .Select(x => new UsageMultiplierDto(x.Id, x.Name, x.MetricType.ToString(), x.Multiplier, x.IsActive))
            .ToListAsync(cancellationToken);

        var overrides = await _dbContext.TenantUsageMultiplierOverrides
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.MetricType)
            .Select(x => new TenantUsageMultiplierOverrideDto(x.Id, x.MetricType.ToString(), x.Multiplier, x.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(new BillingSummaryDto(
            TenantId: tenantId,
            StripeCustomerId: billingAccount?.StripeCustomerId,
            StripeSubscriptionId: billingAccount?.StripeSubscriptionId,
            CurrentSubscription: subscription == null
                ? null
                : new TenantSubscriptionDto(
                    subscription.Id,
                    subscription.PlanId,
                    subscription.Plan.Name,
                    subscription.Status.ToString(),
                    subscription.BillingCycle.ToString(),
                    subscription.CurrentPeriodStart,
                    subscription.CurrentPeriodEnd,
                    subscription.CancelAt,
                    subscription.TrialEnd),
            LatestUsage: latestUsage == null
                ? null
                : new TenantUsageSnapshotDto(
                    latestUsage.PeriodStartUtc,
                    latestUsage.StorageBytes,
                    latestUsage.BandwidthBytes,
                    latestUsage.VideoCount),
            StripeInvoices: invoices,
            ManualPayments: manualPayments,
            GlobalUsageMultipliers: globalMultipliers,
            TenantUsageMultiplierOverrides: overrides));
    }

    [HttpPost("checkout-session")]
    public async Task<ActionResult<CreateCheckoutSessionResponse>> CreateCheckoutSession(
        [FromBody] CreateCheckoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = await RequireTenantIdAsync(cancellationToken);
        EnsureStripeConfigured();

        var plan = await _dbContext.SubscriptionPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.IsActive, cancellationToken);

        if (plan == null)
            return NotFound(new { error = "Plan not found" });

        var interval = request.Interval?.Trim().ToLowerInvariant();
        var stripePriceId = interval switch
        {
            "yearly" => plan.StripePriceIdYearly,
            "monthly" or null or "" => plan.StripePriceIdMonthly,
            _ => null
        };

        if (string.IsNullOrWhiteSpace(stripePriceId))
            return BadRequest(new { error = "Plan is missing Stripe price id for requested interval" });

        var billingAccount = await EnsureBillingAccountAsync(tenantId, cancellationToken);

        var sessionService = new Stripe.Checkout.SessionService();
        var session = await sessionService.CreateAsync(new SessionCreateOptions
        {
            Customer = billingAccount.StripeCustomerId,
            Mode = "subscription",
            AllowPromotionCodes = true,
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Price = stripePriceId,
                    Quantity = 1
                }
            },
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
            Metadata = new Dictionary<string, string>
            {
                ["tenant_id"] = tenantId.ToString(),
                ["plan_id"] = plan.Id.ToString(),
                ["interval"] = interval == "yearly" ? "yearly" : "monthly"
            }
        }, cancellationToken: cancellationToken);

        return Ok(new CreateCheckoutSessionResponse(session.Id, session.Url));
    }

    [HttpPost("portal-session")]
    public async Task<ActionResult<CreatePortalSessionResponse>> CreatePortalSession(
        [FromBody] CreatePortalSessionRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = await RequireTenantIdAsync(cancellationToken);
        EnsureStripeConfigured();

        var billingAccount = await _dbContext.TenantBillingAccounts
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (billingAccount?.StripeCustomerId == null)
            return BadRequest(new { error = "Stripe customer not created yet" });

        var portalService = new Stripe.BillingPortal.SessionService();
        var portal = await portalService.CreateAsync(new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = billingAccount.StripeCustomerId,
            ReturnUrl = request.ReturnUrl
        }, cancellationToken: cancellationToken);

        return Ok(new CreatePortalSessionResponse(portal.Id, portal.Url));
    }

    [HttpGet("manual-payments")]
    public async Task<ActionResult<IReadOnlyList<ManualPaymentDto>>> ListManualPayments(CancellationToken cancellationToken)
    {
        var tenantId = await RequireTenantIdAsync(cancellationToken);

        var items = await _dbContext.ManualPayments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.PaidAt)
            .Take(100)
            .Select(x => new ManualPaymentDto(x.Id, x.Amount, x.Currency, x.PaidAt, x.Reference, x.Notes))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost("manual-payments")]
    public async Task<ActionResult<ManualPaymentDto>> CreateManualPayment(
        [FromBody] CreateManualPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = await RequireTenantIdAsync(cancellationToken);

        if (request.Amount <= 0)
            return BadRequest(new { error = "Amount must be > 0" });

        var payment = new ManualPayment
        {
            TenantId = tenantId,
            Amount = request.Amount,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "USD" : request.Currency.Trim().ToUpperInvariant(),
            PaidAt = request.PaidAtUtc ?? DateTimeOffset.UtcNow,
            Reference = request.Reference,
            Notes = request.Notes,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.ManualPayments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new ManualPaymentDto(payment.Id, payment.Amount, payment.Currency, payment.PaidAt, payment.Reference, payment.Notes));
    }

    [HttpPut("usage-multipliers/override")]
    public async Task<ActionResult<TenantUsageMultiplierOverrideDto>> UpsertTenantMultiplierOverride(
        [FromBody] UpsertTenantMultiplierOverrideRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = await RequireTenantIdAsync(cancellationToken);

        if (!Enum.TryParse<MetricType>(request.MetricType, ignoreCase: true, out var metricType))
            return BadRequest(new { error = "Invalid metricType" });

        if (request.Multiplier <= 0)
            return BadRequest(new { error = "Multiplier must be > 0" });

        var existing = await _dbContext.TenantUsageMultiplierOverrides
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.MetricType == metricType, cancellationToken);

        if (existing == null)
        {
            existing = new TenantUsageMultiplierOverride
            {
                TenantId = tenantId,
                MetricType = metricType,
                Multiplier = request.Multiplier,
                IsActive = request.IsActive,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.TenantUsageMultiplierOverrides.Add(existing);
        }
        else
        {
            existing.Multiplier = request.Multiplier;
            existing.IsActive = request.IsActive;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new TenantUsageMultiplierOverrideDto(existing.Id, existing.MetricType.ToString(), existing.Multiplier, existing.IsActive));
    }

    private void EnsureStripeConfigured()
    {
        var apiKey = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Stripe secret key not configured (Stripe:SecretKey)");

        StripeConfiguration.ApiKey = apiKey;
    }

    private async Task<Guid> RequireTenantIdAsync(CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId.HasValue)
            return _tenantContext.TenantId.Value;

        var tenantClaim = User.FindFirstValue("tenant_id");
        if (Guid.TryParse(tenantClaim, out var tenantIdFromToken))
        {
            var tenant = await _dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantIdFromToken, cancellationToken);
            if (tenant != null)
            {
                _tenantContext.SetCurrentTenant(tenant);
                return tenant.Id;
            }
        }

        if (Request.Headers.TryGetValue("X-Tenant-Slug", out var slugValues))
        {
            var slug = slugValues.ToString();
            if (!string.IsNullOrWhiteSpace(slug))
            {
                var tenant = await _dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
                if (tenant != null)
                {
                    _tenantContext.SetCurrentTenant(tenant);
                    return tenant.Id;
                }
            }
        }

        throw new InvalidOperationException("Tenant not resolved");
    }

    private async Task<TenantBillingAccount> EnsureBillingAccountAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var billingAccount = await _dbContext.TenantBillingAccounts
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (billingAccount == null)
        {
            billingAccount = new TenantBillingAccount
            {
                TenantId = tenantId,
                Currency = "USD",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.TenantBillingAccounts.Add(billingAccount);
        }

        if (string.IsNullOrWhiteSpace(billingAccount.StripeCustomerId))
        {
            var tenant = await _dbContext.Tenants.AsNoTracking().FirstAsync(t => t.Id == tenantId, cancellationToken);

            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(new CustomerCreateOptions
            {
                Name = tenant.Name,
                Metadata = new Dictionary<string, string>
                {
                    ["tenant_id"] = tenantId.ToString(),
                    ["tenant_slug"] = tenant.Slug
                }
            }, cancellationToken: cancellationToken);

            billingAccount.StripeCustomerId = customer.Id;
            billingAccount.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return billingAccount;
    }
}

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    decimal PriceMonthly,
    decimal PriceYearly,
    string? StripePriceIdMonthly,
    string? StripePriceIdYearly,
    bool IsCustom);

public record CreateCheckoutSessionRequest(
    Guid PlanId,
    string? Interval,
    string SuccessUrl,
    string CancelUrl);

public record CreateCheckoutSessionResponse(string SessionId, string? Url);

public record CreatePortalSessionRequest(string ReturnUrl);

public record CreatePortalSessionResponse(string SessionId, string? Url);

public record BillingSummaryDto(
    Guid TenantId,
    string? StripeCustomerId,
    string? StripeSubscriptionId,
    TenantSubscriptionDto? CurrentSubscription,
    TenantUsageSnapshotDto? LatestUsage,
    IReadOnlyList<StripeInvoiceDto> StripeInvoices,
    IReadOnlyList<ManualPaymentDto> ManualPayments,
    IReadOnlyList<UsageMultiplierDto> GlobalUsageMultipliers,
    IReadOnlyList<TenantUsageMultiplierOverrideDto> TenantUsageMultiplierOverrides);

public record TenantSubscriptionDto(
    Guid Id,
    Guid PlanId,
    string PlanName,
    string Status,
    string BillingCycle,
    DateTimeOffset? CurrentPeriodStart,
    DateTimeOffset? CurrentPeriodEnd,
    DateTimeOffset? CancelAt,
    DateTimeOffset? TrialEnd);

public record TenantUsageSnapshotDto(
    DateTimeOffset PeriodStartUtc,
    long StorageBytes,
    long BandwidthBytes,
    int VideoCount);

public record StripeInvoiceDto(
    string Id,
    string Status,
    decimal AmountDue,
    string Currency,
    string? HostedInvoiceUrl,
    string? InvoicePdf,
    DateTime CreatedAtUtc,
    DateTime? DueDateUtc);

public record ManualPaymentDto(
    Guid Id,
    decimal Amount,
    string Currency,
    DateTimeOffset PaidAtUtc,
    string? Reference,
    string? Notes);

public record CreateManualPaymentRequest(
    decimal Amount,
    string? Currency,
    DateTimeOffset? PaidAtUtc,
    string? Reference,
    string? Notes);

public record UsageMultiplierDto(
    Guid Id,
    string Name,
    string MetricType,
    double Multiplier,
    bool IsActive);

public record TenantUsageMultiplierOverrideDto(
    Guid Id,
    string MetricType,
    double Multiplier,
    bool IsActive);

public record UpsertTenantMultiplierOverrideRequest(
    string MetricType,
    double Multiplier,
    bool IsActive);
