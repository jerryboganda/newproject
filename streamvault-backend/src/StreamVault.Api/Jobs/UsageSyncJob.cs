using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Jobs;

public class UsageSyncJob
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly StreamVaultDbContext _dbContext;
    private readonly ILogger<UsageSyncJob> _logger;

    public UsageSyncJob(IHttpClientFactory httpClientFactory, StreamVaultDbContext dbContext, ILogger<UsageSyncJob> logger)
    {
        _httpClientFactory = httpClientFactory;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var periodStartUtc = TruncateToHour(DateTimeOffset.UtcNow);

        var tenants = await _dbContext.Tenants
            .AsNoTracking()
            .Where(t => t.Status == TenantStatus.Active)
            .Where(t => t.BunnyLibraryId != null && t.BunnyApiKey != null)
            .Select(t => new { t.Id, t.Slug, t.BunnyLibraryId, t.BunnyApiKey })
            .ToListAsync(cancellationToken);

        foreach (var tenant in tenants)
        {
            try
            {
                var stats = await GetLibraryStatisticsAsync(tenant.BunnyLibraryId!, tenant.BunnyApiKey!, cancellationToken);

                var existing = await _dbContext.TenantUsageSnapshots
                    .FirstOrDefaultAsync(x => x.TenantId == tenant.Id && x.PeriodStartUtc == periodStartUtc, cancellationToken);

                if (existing == null)
                {
                    existing = new TenantUsageSnapshot
                    {
                        TenantId = tenant.Id,
                        PeriodStartUtc = periodStartUtc,
                        StorageBytes = stats.TotalStorageUsed,
                        BandwidthBytes = stats.TotalBandwidthUsed,
                        VideoCount = stats.TotalVideos,
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    _dbContext.TenantUsageSnapshots.Add(existing);
                }
                else
                {
                    existing.StorageBytes = stats.TotalStorageUsed;
                    existing.BandwidthBytes = stats.TotalBandwidthUsed;
                    existing.VideoCount = stats.TotalVideos;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Usage sync failed for tenant={TenantSlug}", tenant.Slug);
            }
        }
    }

    private async Task<BunnyStorageStats> GetLibraryStatisticsAsync(string libraryId, string accessKey, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.bunny.net/library/{libraryId.Trim()}/statistics");
        request.Headers.Add("AccessKey", accessKey.Trim());
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var stats = await response.Content.ReadFromJsonAsync<BunnyStorageStats>(cancellationToken: cancellationToken);
        return stats ?? new BunnyStorageStats();
    }

    private static DateTimeOffset TruncateToHour(DateTimeOffset dt)
    {
        return new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, TimeSpan.Zero);
    }

    private sealed class BunnyStorageStats
    {
        public long TotalStorageUsed { get; set; }
        public long TotalBandwidthUsed { get; set; }
        public int TotalVideos { get; set; }
    }
}
