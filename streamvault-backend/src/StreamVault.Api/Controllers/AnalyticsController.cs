using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StreamVault.Api.Hubs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly IHubContext<LiveAnalyticsHub> _liveHub;

    public AnalyticsController(StreamVaultDbContext dbContext, IHubContext<LiveAnalyticsHub> liveHub)
    {
        _dbContext = dbContext;
        _liveHub = liveHub;
    }

    [HttpPost("track")]
    [AllowAnonymous]
    public async Task<ActionResult<TrackEventResponse>> Track([FromBody] TrackEventRequest request, CancellationToken cancellationToken = default)
    {
        if (request.VideoId == Guid.Empty)
            return BadRequest(new { error = "videoId is required" });

        var tenantId = await ResolveTenantIdAsync(cancellationToken);
        if (tenantId == null)
            return BadRequest(new { error = "Tenant could not be resolved" });

        var videoExists = await _dbContext.Videos
            .AsNoTracking()
            .AnyAsync(v => v.Id == request.VideoId && v.TenantId == tenantId.Value, cancellationToken);
        if (!videoExists)
            return NotFound(new { error = "Video not found" });

        var userId = GetOptionalUserId();

        // De-dupe view events per viewer key (userId or sessionId) within a short window.
        if (request.EventType == AnalyticsEventType.View)
        {
            var windowStart = DateTimeOffset.UtcNow.AddMinutes(-30);

            if (userId.HasValue)
            {
                var already = await _dbContext.VideoAnalytics
                    .AsNoTracking()
                    .AnyAsync(e => e.TenantId == tenantId.Value
                               && e.VideoId == request.VideoId
                               && e.EventType == AnalyticsEventType.View
                               && e.UserId == userId
                               && e.Timestamp >= windowStart, cancellationToken);

                if (already)
                    return Ok(new TrackEventResponse(false, true));
            }
            else if (!string.IsNullOrWhiteSpace(request.SessionId))
            {
                var already = await _dbContext.VideoAnalytics
                    .AsNoTracking()
                    .AnyAsync(e => e.TenantId == tenantId.Value
                               && e.VideoId == request.VideoId
                               && e.EventType == AnalyticsEventType.View
                               && e.UserId == null
                               && e.SessionId == request.SessionId
                               && e.Timestamp >= windowStart, cancellationToken);

                if (already)
                    return Ok(new TrackEventResponse(false, true));
            }
        }

        var entity = new VideoAnalytics
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            VideoId = request.VideoId,
            UserId = userId,
            EventType = request.EventType,
            PositionSeconds = request.PositionSeconds,
            DeviceType = request.DeviceType,
            Browser = request.Browser,
            OS = request.OS,
            Country = request.Country,
            City = request.City,
            Referrer = request.Referrer,
            UTMSource = request.UTMSource,
            UTMMedium = request.UTMMedium,
            UTMCampaign = request.UTMCampaign,
            SessionId = request.SessionId,
            Metadata = request.Metadata,
            Timestamp = DateTimeOffset.UtcNow
        };

        _dbContext.VideoAnalytics.Add(entity);

        if (request.EventType == AnalyticsEventType.View)
        {
            var video = await _dbContext.Videos.FirstOrDefaultAsync(v => v.Id == request.VideoId && v.TenantId == tenantId.Value, cancellationToken);
            if (video != null)
            {
                video.ViewCount++;
                video.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (request.EventType == AnalyticsEventType.View)
        {
            var payload = new LiveViewEvent(request.VideoId, DateTimeOffset.UtcNow);
            await _liveHub.Clients.Group(LiveAnalyticsHub.TenantGroup(tenantId.Value)).SendAsync("view", payload, cancellationToken);
            await _liveHub.Clients.Group(LiveAnalyticsHub.VideoGroup(tenantId.Value, request.VideoId)).SendAsync("view", payload, cancellationToken);
        }

        return Ok(new TrackEventResponse(true, false));
    }

    [HttpGet("overview")]
    [Authorize]
    public async Task<ActionResult<AnalyticsOverviewResponse>> Overview(
        [FromQuery] DateTimeOffset? startUtc = null,
        [FromQuery] DateTimeOffset? endUtc = null,
        [FromQuery] int top = 10,
        CancellationToken cancellationToken = default)
    {
        if (top is < 1 or > 50) top = 10;

        var tenantId = GetRequiredTenantId();

        var start = startUtc ?? DateTimeOffset.UtcNow.AddDays(-30);
        var end = endUtc ?? DateTimeOffset.UtcNow;
        if (end < start)
            (start, end) = (end, start);

        var startDate = DateOnly.FromDateTime(start.UtcDateTime);
        var endDate = DateOnly.FromDateTime(end.UtcDateTime);

        var daily = await _dbContext.VideoAnalyticsDailyAggregates
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.DateUtc >= startDate && a.DateUtc <= endDate)
            .GroupBy(a => a.DateUtc)
            .Select(g => new AnalyticsTimePoint(
                g.Key,
                g.Sum(x => x.Views),
                g.Sum(x => x.UniqueViewers),
                g.Sum(x => x.WatchTimeSeconds),
                g.Sum(x => x.Completes)))
            .OrderBy(x => x.DateUtc)
            .ToListAsync(cancellationToken);

        // Fallback to raw events when aggregates are empty (e.g., before first rollup)
        if (daily.Count == 0)
        {
            daily = await _dbContext.VideoAnalytics
                .AsNoTracking()
                .Where(e => e.TenantId == tenantId && e.Timestamp >= start && e.Timestamp <= end)
                .GroupBy(e => DateOnly.FromDateTime(e.Timestamp.UtcDateTime))
                .Select(g => new AnalyticsTimePoint(
                    g.Key,
                    g.Count(x => x.EventType == AnalyticsEventType.View),
                    g.Where(x => x.UserId != null).Select(x => x.UserId).Distinct().Count() +
                    g.Where(x => x.UserId == null && x.SessionId != null).Select(x => x.SessionId).Distinct().Count(),
                    g.Where(x => x.EventType == AnalyticsEventType.Complete || x.EventType == AnalyticsEventType.Exit).Sum(x => x.PositionSeconds ?? 0),
                    g.Count(x => x.EventType == AnalyticsEventType.Complete)))
                .OrderBy(x => x.DateUtc)
                .ToListAsync(cancellationToken);
        }

        var totalViews = daily.Sum(x => x.Views);
        var totalWatchTimeSeconds = daily.Sum(x => x.WatchTimeSeconds);
        var totalCompletes = daily.Sum(x => x.Completes);
        var avgWatchTimeSeconds = totalViews > 0 ? totalWatchTimeSeconds / totalViews : 0;

        var topVideos = await _dbContext.VideoAnalyticsDailyAggregates
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.DateUtc >= startDate && a.DateUtc <= endDate)
            .GroupBy(a => a.VideoId)
            .Select(g => new { VideoId = g.Key, Views = g.Sum(x => x.Views) })
            .OrderByDescending(x => x.Views)
            .Take(top)
            .ToListAsync(cancellationToken);

        var videoTitles = await _dbContext.Videos
            .AsNoTracking()
            .Where(v => v.TenantId == tenantId)
            .Where(v => topVideos.Select(tv => tv.VideoId).Contains(v.Id))
            .Select(v => new { v.Id, v.Title })
            .ToListAsync(cancellationToken);

        var topVideoDtos = topVideos
            .Join(videoTitles, tv => tv.VideoId, v => v.Id, (tv, v) => new TopVideoPoint(tv.VideoId, v.Title, tv.Views))
            .ToList();

        var countries = await _dbContext.VideoAnalyticsDailyCountryAggregates
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.DateUtc >= startDate && a.DateUtc <= endDate)
            .GroupBy(a => a.CountryCode)
            .Select(g => new CountryPoint(g.Key, g.Sum(x => x.Views)))
            .OrderByDescending(x => x.Views)
            .Take(50)
            .ToListAsync(cancellationToken);

        return Ok(new AnalyticsOverviewResponse(
            start,
            end,
            totalViews,
            daily.Sum(x => x.UniqueViewers),
            totalWatchTimeSeconds,
            avgWatchTimeSeconds,
            totalCompletes,
            daily,
            topVideoDtos,
            countries));
    }

    [HttpGet("videos/{videoId:guid}/timeseries")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<VideoSeriesPoint>>> VideoTimeseries(
        [FromRoute] Guid videoId,
        [FromQuery] string bucket = "day",
        [FromQuery] DateTimeOffset? startUtc = null,
        [FromQuery] DateTimeOffset? endUtc = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();

        var start = startUtc ?? DateTimeOffset.UtcNow.AddDays(-30);
        var end = endUtc ?? DateTimeOffset.UtcNow;
        if (end < start)
            (start, end) = (end, start);

        var startDate = DateOnly.FromDateTime(start.UtcDateTime);
        var endDate = DateOnly.FromDateTime(end.UtcDateTime);

        if (string.Equals(bucket, "hour", StringComparison.OrdinalIgnoreCase))
        {
            var startHour = TruncateToHour(start);
            var endHour = TruncateToHour(end).AddHours(1);

            var data = await _dbContext.VideoAnalyticsHourlyAggregates
                .AsNoTracking()
                .Where(a => a.TenantId == tenantId && a.VideoId == videoId)
                .Where(a => a.BucketStartUtc >= startHour && a.BucketStartUtc < endHour)
                .OrderBy(a => a.BucketStartUtc)
                .Select(a => new VideoSeriesPoint(a.BucketStartUtc, a.Views, a.UniqueViewers, a.WatchTimeSeconds, a.Completes))
                .ToListAsync(cancellationToken);

            return Ok(data);
        }

        var daily = await _dbContext.VideoAnalyticsDailyAggregates
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.VideoId == videoId)
            .Where(a => a.DateUtc >= startDate && a.DateUtc <= endDate)
            .OrderBy(a => a.DateUtc)
            .Select(a => new VideoSeriesPoint(new DateTimeOffset(a.DateUtc.Year, a.DateUtc.Month, a.DateUtc.Day, 0, 0, 0, TimeSpan.Zero), a.Views, a.UniqueViewers, a.WatchTimeSeconds, a.Completes))
            .ToListAsync(cancellationToken);

        return Ok(daily);
    }

    [HttpGet("videos/{videoId:guid}/export")]
    [Authorize]
    public async Task<IActionResult> ExportVideoEvents(
        [FromRoute] Guid videoId,
        [FromQuery] DateTimeOffset? startUtc = null,
        [FromQuery] DateTimeOffset? endUtc = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();

        var start = startUtc ?? DateTimeOffset.UtcNow.AddDays(-30);
        var end = endUtc ?? DateTimeOffset.UtcNow;
        if (end < start)
            (start, end) = (end, start);

        var events = await _dbContext.VideoAnalytics
            .AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.VideoId == videoId)
            .Where(e => e.Timestamp >= start && e.Timestamp <= end)
            .OrderBy(e => e.Timestamp)
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("timestampUtc,eventType,userId,sessionId,country,deviceType,browser,os,positionSeconds,referrer");

        foreach (var e in events)
        {
            sb.Append(Escape(e.Timestamp.UtcDateTime.ToString("O"))).Append(',')
              .Append(Escape(e.EventType.ToString())).Append(',')
              .Append(Escape(e.UserId?.ToString() ?? "")).Append(',')
              .Append(Escape(e.SessionId ?? "")).Append(',')
              .Append(Escape(e.Country ?? "")).Append(',')
              .Append(Escape(e.DeviceType ?? "")).Append(',')
              .Append(Escape(e.Browser ?? "")).Append(',')
              .Append(Escape(e.OS ?? "")).Append(',')
              .Append(Escape(e.PositionSeconds?.ToString() ?? "")).Append(',')
              .Append(Escape(e.Referrer ?? ""))
              .AppendLine();
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"video_{videoId}_analytics.csv");
    }

    private Guid GetRequiredTenantId()
    {
        var tenantClaim = User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantClaim, out var tenantId))
            throw new InvalidOperationException("tenant_id missing from token");
        return tenantId;
    }

    private Guid? GetOptionalUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (Guid.TryParse(id, out var userId))
            return userId;
        return null;
    }

    private async Task<Guid?> ResolveTenantIdAsync(CancellationToken cancellationToken)
    {
        var tenantClaim = User.FindFirstValue("tenant_id");
        if (Guid.TryParse(tenantClaim, out var tenantIdFromToken))
        {
            var tenant = await _dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantIdFromToken, cancellationToken);
            if (tenant != null)
                return tenant.Id;
        }

        if (Request.Headers.TryGetValue("X-Tenant-Slug", out var slugValues))
        {
            var slug = slugValues.ToString();
            if (!string.IsNullOrWhiteSpace(slug))
            {
                var tenant = await _dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
                if (tenant != null)
                    return tenant.Id;
            }
        }

        return null;
    }

    private static DateTimeOffset TruncateToHour(DateTimeOffset dt)
    {
        return new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, TimeSpan.Zero);
    }

    private static string Escape(string value)
    {
        var v = value ?? string.Empty;
        if (v.Contains('"')) v = v.Replace("\"", "\"\"");
        if (v.Contains(',') || v.Contains('\n') || v.Contains('\r') || v.Contains('"'))
            return $"\"{v}\"";
        return v;
    }
}

public record TrackEventRequest(
    Guid VideoId,
    AnalyticsEventType EventType,
    double? PositionSeconds = null,
    string? DeviceType = null,
    string? Browser = null,
    string? OS = null,
    string? Country = null,
    string? City = null,
    string? Referrer = null,
    string? UTMSource = null,
    string? UTMMedium = null,
    string? UTMCampaign = null,
    string? SessionId = null,
    string? Metadata = null);

public record TrackEventResponse(bool Recorded, bool Deduped);

public record AnalyticsOverviewResponse(
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    int TotalViews,
    int UniqueViewers,
    double TotalWatchTimeSeconds,
    double AverageWatchTimeSeconds,
    int Completes,
    IReadOnlyList<AnalyticsTimePoint> ViewsByDay,
    IReadOnlyList<TopVideoPoint> TopVideos,
    IReadOnlyList<CountryPoint> Countries);

public record AnalyticsTimePoint(
    DateOnly DateUtc,
    int Views,
    int UniqueViewers,
    double WatchTimeSeconds,
    int Completes);

public record TopVideoPoint(Guid VideoId, string Title, int Views);

public record CountryPoint(string CountryCode, int Views);

public record VideoSeriesPoint(
    DateTimeOffset BucketStartUtc,
    int Views,
    int UniqueViewers,
    double WatchTimeSeconds,
    int Completes);

public record LiveViewEvent(Guid VideoId, DateTimeOffset OccurredAtUtc);
