using Microsoft.EntityFrameworkCore;
using Stripe;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Jobs;

public class MonthlyOverageInvoiceJob
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MonthlyOverageInvoiceJob> _logger;

    public MonthlyOverageInvoiceJob(StreamVaultDbContext dbContext, IConfiguration configuration, ILogger<MonthlyOverageInvoiceJob> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        // Runs daily, but only invoices on the 1st of the month (UTC) for the previous month.
        var now = DateTimeOffset.UtcNow;
        if (now.Day != 1)
            return;

        EnsureStripeConfigured();

        var periodEnd = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var previous = periodEnd.AddMonths(-1);
        var periodStart = new DateTimeOffset(previous.Year, previous.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var tenants = await _dbContext.Tenants
            .AsNoTracking()
            .Where(t => t.Status == TenantStatus.Active)
            .Select(t => new { t.Id, t.Slug })
            .ToListAsync(cancellationToken);

        foreach (var tenant in tenants)
        {
            try
            {
                var billing = await _dbContext.TenantBillingAccounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenant.Id, cancellationToken);

                if (billing?.StripeCustomerId == null)
                    continue;

                var subscription = await _dbContext.TenantSubscriptions
                    .AsNoTracking()
                    .Include(s => s.Plan)
                    .FirstOrDefaultAsync(s => s.TenantId == tenant.Id && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing), cancellationToken);

                if (subscription?.Plan == null)
                    continue;

                var already = await _dbContext.TenantBillingPeriodInvoices
                    .AsNoTracking()
                    .AnyAsync(x => x.TenantId == tenant.Id && x.PeriodStartUtc == periodStart && x.PeriodEndUtc == periodEnd, cancellationToken);

                if (already)
                    continue;

                var usage = await CalculatePeriodUsageAsync(tenant.Id, periodStart, periodEnd, cancellationToken);
                if (usage == null)
                    continue;

                var pricing = GetPricingForPlan(subscription.Plan.Slug);

                var storageGbPeak = BytesToGiB(usage.PeakStorageBytes) * GetEffectiveMultiplier(tenant.Id, MetricType.Storage, cancellationToken);
                var bandwidthGb = BytesToGiB(usage.BandwidthBytesDelta) * GetEffectiveMultiplier(tenant.Id, MetricType.Bandwidth, cancellationToken);

                var storageOverGb = Math.Max(0, storageGbPeak - pricing.IncludedStorageGiB);
                var bandwidthOverGb = Math.Max(0, bandwidthGb - pricing.IncludedBandwidthGiB);

                var overageTotal = (decimal)(storageOverGb * pricing.StorageOveragePricePerGiB + bandwidthOverGb * pricing.BandwidthOveragePricePerGiB);
                if (overageTotal <= 0)
                    continue;

                var currency = (billing.Currency ?? "USD").ToLowerInvariant();
                var amountCents = (long)Math.Round(overageTotal * 100m, MidpointRounding.AwayFromZero);

                var invoiceItemService = new InvoiceItemService();
                await invoiceItemService.CreateAsync(new InvoiceItemCreateOptions
                {
                    Customer = billing.StripeCustomerId,
                    Amount = amountCents,
                    Currency = currency,
                    Description = $"Overage charges for {periodStart:yyyy-MM} (Storage + Bandwidth)"
                }, cancellationToken: cancellationToken);

                var invoiceService = new InvoiceService();
                var invoice = await invoiceService.CreateAsync(new InvoiceCreateOptions
                {
                    Customer = billing.StripeCustomerId,
                    AutoAdvance = true,
                    Description = $"StreamVault overage invoice for {periodStart:yyyy-MM}"
                }, cancellationToken: cancellationToken);

                // Finalize immediately so it becomes payable/collectable.
                invoice = await invoiceService.FinalizeInvoiceAsync(invoice.Id, cancellationToken: cancellationToken);

                _dbContext.TenantBillingPeriodInvoices.Add(new TenantBillingPeriodInvoice
                {
                    TenantId = tenant.Id,
                    PeriodStartUtc = periodStart,
                    PeriodEndUtc = periodEnd,
                    StripeInvoiceId = invoice.Id,
                    CreatedAt = DateTimeOffset.UtcNow
                });

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Created overage invoice for tenant={TenantSlug} invoice={InvoiceId} amountCents={Amount}", tenant.Slug, invoice.Id, amountCents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Overage invoicing failed for tenant={TenantSlug}", tenant.Slug);
            }
        }
    }

    private void EnsureStripeConfigured()
    {
        var apiKey = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Stripe secret key not configured (Stripe:SecretKey)");

        StripeConfiguration.ApiKey = apiKey;
    }

    private async Task<PeriodUsage?> CalculatePeriodUsageAsync(Guid tenantId, DateTimeOffset periodStart, DateTimeOffset periodEnd, CancellationToken cancellationToken)
    {
        var snapshots = await _dbContext.TenantUsageSnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Where(x => x.PeriodStartUtc >= periodStart && x.PeriodStartUtc < periodEnd)
            .OrderBy(x => x.PeriodStartUtc)
            .Select(x => new { x.PeriodStartUtc, x.StorageBytes, x.BandwidthBytes })
            .ToListAsync(cancellationToken);

        if (snapshots.Count < 2)
            return null;

        var first = snapshots.First();
        var last = snapshots.Last();

        var peakStorage = snapshots.Max(s => s.StorageBytes);
        var bandwidthDelta = Math.Max(0, last.BandwidthBytes - first.BandwidthBytes);

        return new PeriodUsage(peakStorage, bandwidthDelta);
    }

    private record PeriodUsage(long PeakStorageBytes, long BandwidthBytesDelta);

    private PlanPricing GetPricingForPlan(string planSlug)
    {
        var section = _configuration.GetSection($"Billing:Plans:{planSlug}");

        // Defaults are intentionally conservative (no free allowance) if config is missing.
        var includedStorage = section.GetValue<double?>("IncludedStorageGiB") ?? 0;
        var includedBandwidth = section.GetValue<double?>("IncludedBandwidthGiB") ?? 0;
        var storagePrice = section.GetValue<double?>("StorageOveragePricePerGiB") ?? 0;
        var bandwidthPrice = section.GetValue<double?>("BandwidthOveragePricePerGiB") ?? 0;

        return new PlanPricing(includedStorage, includedBandwidth, storagePrice, bandwidthPrice);
    }

    private double GetEffectiveMultiplier(Guid tenantId, MetricType metricType, CancellationToken cancellationToken)
    {
        // Global multipliers are multiplicative.
        var globalProduct = _dbContext.UsageMultipliers
            .AsNoTracking()
            .Where(x => x.IsActive && x.MetricType == metricType)
            .Select(x => x.Multiplier)
            .AsEnumerable()
            .Aggregate(1.0, (acc, m) => acc * m);

        var tenantOverride = _dbContext.TenantUsageMultiplierOverrides
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.MetricType == metricType && x.IsActive)
            .Select(x => (double?)x.Multiplier)
            .FirstOrDefault();

        return globalProduct * (tenantOverride ?? 1.0);
    }

    private record PlanPricing(
        double IncludedStorageGiB,
        double IncludedBandwidthGiB,
        double StorageOveragePricePerGiB,
        double BandwidthOveragePricePerGiB);

    private static double BytesToGiB(long bytes)
    {
        return bytes / (1024d * 1024d * 1024d);
    }
}
