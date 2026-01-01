using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Jobs;

public class AnalyticsRollupJob
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly ILogger<AnalyticsRollupJob> _logger;

    public AnalyticsRollupJob(StreamVaultDbContext dbContext, ILogger<AnalyticsRollupJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await RollupLastCompleteHourAsync(cancellationToken);
        await RollupLastCompleteDayAsync(cancellationToken);
    }

    private async Task RollupLastCompleteHourAsync(CancellationToken cancellationToken)
    {
        var thisHour = TruncateToHour(DateTimeOffset.UtcNow);
        var bucketStartUtc = thisHour.AddHours(-1);
        var bucketEndUtc = thisHour;

        var grouped = await _dbContext.VideoAnalytics
            .AsNoTracking()
            .Where(e => e.Timestamp >= bucketStartUtc && e.Timestamp < bucketEndUtc)
            .GroupBy(e => new { e.TenantId, e.VideoId })
            .Select(g => new
            {
                g.Key.TenantId,
                g.Key.VideoId,
                Views = g.Count(x => x.EventType == AnalyticsEventType.View),
                UniqueUsers = g.Where(x => x.UserId != null).Select(x => x.UserId).Distinct().Count(),
                UniqueSessions = g.Where(x => x.UserId == null && x.SessionId != null).Select(x => x.SessionId).Distinct().Count(),
                WatchTimeSeconds = g.Where(x => x.EventType == AnalyticsEventType.Complete || x.EventType == AnalyticsEventType.Exit)
                    .Sum(x => (double?)(x.PositionSeconds ?? 0)) ?? 0,
                Completes = g.Count(x => x.EventType == AnalyticsEventType.Complete)
            })
            .ToListAsync(cancellationToken);

        if (grouped.Count == 0)
            return;

        var existing = await _dbContext.VideoAnalyticsHourlyAggregates
            .Where(a => a.BucketStartUtc == bucketStartUtc)
            .ToListAsync(cancellationToken);

        var existingMap = existing.ToDictionary(a => (a.TenantId, a.VideoId));

        foreach (var row in grouped)
        {
            if (!existingMap.TryGetValue((row.TenantId, row.VideoId), out var aggregate))
            {
                aggregate = new VideoAnalyticsHourlyAggregate
                {
                    Id = Guid.NewGuid(),
                    TenantId = row.TenantId,
                    VideoId = row.VideoId,
                    BucketStartUtc = bucketStartUtc,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                _dbContext.VideoAnalyticsHourlyAggregates.Add(aggregate);
                existingMap[(row.TenantId, row.VideoId)] = aggregate;
            }

            aggregate.Views = row.Views;
            aggregate.UniqueViewers = row.UniqueUsers + row.UniqueSessions;
            aggregate.WatchTimeSeconds = row.WatchTimeSeconds;
            aggregate.Completes = row.Completes;
            aggregate.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task RollupLastCompleteDayAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dateUtc = today.AddDays(-1);

        var dayStartUtc = new DateTimeOffset(dateUtc.Year, dateUtc.Month, dateUtc.Day, 0, 0, 0, TimeSpan.Zero);
        var dayEndUtc = dayStartUtc.AddDays(1);

        // Roll daily aggregates from hourly aggregates
        var dailyFromHourly = await _dbContext.VideoAnalyticsHourlyAggregates
            .AsNoTracking()
            .Where(a => a.BucketStartUtc >= dayStartUtc && a.BucketStartUtc < dayEndUtc)
            .GroupBy(a => new { a.TenantId, a.VideoId })
            .Select(g => new
            {
                g.Key.TenantId,
                g.Key.VideoId,
                Views = g.Sum(x => x.Views),
                UniqueViewers = g.Sum(x => x.UniqueViewers),
                WatchTimeSeconds = g.Sum(x => x.WatchTimeSeconds),
                Completes = g.Sum(x => x.Completes)
            })
            .ToListAsync(cancellationToken);

        if (dailyFromHourly.Count > 0)
        {
            var existingDaily = await _dbContext.VideoAnalyticsDailyAggregates
                .Where(a => a.DateUtc == dateUtc)
                .ToListAsync(cancellationToken);

            var existingDailyMap = existingDaily.ToDictionary(a => (a.TenantId, a.VideoId));

            foreach (var row in dailyFromHourly)
            {
                if (!existingDailyMap.TryGetValue((row.TenantId, row.VideoId), out var aggregate))
                {
                    aggregate = new VideoAnalyticsDailyAggregate
                    {
                        Id = Guid.NewGuid(),
                        TenantId = row.TenantId,
                        VideoId = row.VideoId,
                        DateUtc = dateUtc,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow
                    };
                    _dbContext.VideoAnalyticsDailyAggregates.Add(aggregate);
                    existingDailyMap[(row.TenantId, row.VideoId)] = aggregate;
                }

                aggregate.Views = row.Views;
                aggregate.UniqueViewers = row.UniqueViewers;
                aggregate.WatchTimeSeconds = row.WatchTimeSeconds;
                aggregate.Completes = row.Completes;
                aggregate.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // Roll daily country aggregates from raw events (only when country looks like ISO-2)
        var countryRows = await _dbContext.VideoAnalytics
            .AsNoTracking()
            .Where(e => e.Timestamp >= dayStartUtc && e.Timestamp < dayEndUtc)
            .Where(e => e.EventType == AnalyticsEventType.View)
            .Where(e => e.Country != null && e.Country.Length == 2)
            .GroupBy(e => new { e.TenantId, e.VideoId, Country = e.Country!.ToUpper() })
            .Select(g => new
            {
                g.Key.TenantId,
                g.Key.VideoId,
                g.Key.Country,
                Views = g.Count(),
                UniqueUsers = g.Where(x => x.UserId != null).Select(x => x.UserId).Distinct().Count(),
                UniqueSessions = g.Where(x => x.UserId == null && x.SessionId != null).Select(x => x.SessionId).Distinct().Count()
            })
            .ToListAsync(cancellationToken);

        if (countryRows.Count == 0)
            return;

        var existingCountry = await _dbContext.VideoAnalyticsDailyCountryAggregates
            .Where(a => a.DateUtc == dateUtc)
            .ToListAsync(cancellationToken);

        var existingCountryMap = existingCountry.ToDictionary(a => (a.TenantId, a.VideoId, a.CountryCode));

        foreach (var row in countryRows)
        {
            if (!existingCountryMap.TryGetValue((row.TenantId, row.VideoId, row.Country), out var aggregate))
            {
                aggregate = new VideoAnalyticsDailyCountryAggregate
                {
                    Id = Guid.NewGuid(),
                    TenantId = row.TenantId,
                    VideoId = row.VideoId,
                    DateUtc = dateUtc,
                    CountryCode = row.Country,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                _dbContext.VideoAnalyticsDailyCountryAggregates.Add(aggregate);
                existingCountryMap[(row.TenantId, row.VideoId, row.Country)] = aggregate;
            }

            aggregate.Views = row.Views;
            aggregate.UniqueViewers = row.UniqueUsers + row.UniqueSessions;
            aggregate.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static DateTimeOffset TruncateToHour(DateTimeOffset dt)
    {
        return new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, TimeSpan.Zero);
    }
}
