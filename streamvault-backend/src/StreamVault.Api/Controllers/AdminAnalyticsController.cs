using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/admin/analytics")]
[Authorize(Roles = "SuperAdmin")]
public class AdminAnalyticsController : ControllerBase
{
    private readonly StreamVaultDbContext _db;

    public AdminAnalyticsController(StreamVaultDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<AdminPlatformAnalyticsResponse>> Get(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var startOfMonth = new DateTimeOffset(new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc));

        var totalTenants = await _db.Tenants.AsNoTracking().CountAsync(cancellationToken);
        var activeTenants = await _db.Tenants.AsNoTracking()
            .CountAsync(t => t.Status == TenantStatus.Active || (t.Status == TenantStatus.Trial && t.TrialEndsAt.HasValue && t.TrialEndsAt > now), cancellationToken);

        var totalUsers = await _db.Users.AsNoTracking().CountAsync(cancellationToken);
        var activeUsers = await _db.Users.AsNoTracking().CountAsync(u => u.Status == UserStatus.Active, cancellationToken);

        var totalVideos = await _db.Videos.AsNoTracking().CountAsync(v => v.Status != VideoStatus.Deleted, cancellationToken);
        var totalViews = await _db.Videos.AsNoTracking().Where(v => v.Status != VideoStatus.Deleted).SumAsync(v => (long)v.ViewCount, cancellationToken);
        var totalStorageUsed = await _db.Videos.AsNoTracking().Where(v => v.Status != VideoStatus.Deleted).SumAsync(v => v.FileSizeBytes, cancellationToken);

        var revenue = await _db.ManualPayments.AsNoTracking().SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

        var newTenantsThisMonth = await _db.Tenants.AsNoTracking().CountAsync(t => t.CreatedAt >= startOfMonth, cancellationToken);
        var newUsersThisMonth = await _db.Users.AsNoTracking().CountAsync(u => u.CreatedAt >= startOfMonth, cancellationToken);

        var series = await BuildSixMonthSeriesAsync(cancellationToken);

        return Ok(new AdminPlatformAnalyticsResponse
        {
            TotalTenants = totalTenants,
            ActiveTenants = activeTenants,
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            TotalVideos = totalVideos,
            TotalViews = totalViews,
            TotalStorageUsed = totalStorageUsed,
            Revenue = revenue,
            NewTenantsThisMonth = newTenantsThisMonth,
            NewUsersThisMonth = newUsersThisMonth,
            ChurnRate = 0,
            Series = series
        });
    }

    private async Task<List<AdminPlatformAnalyticsSeriesPoint>> BuildSixMonthSeriesAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var startOfThisMonth = new DateTimeOffset(new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc));

        var months = Enumerable.Range(0, 6)
            .Select(i => startOfThisMonth.AddMonths(-5 + i))
            .ToList();

        var tenantPoints = new List<AdminPlatformAnalyticsSeriesPoint>(6);

        foreach (var monthStart in months)
        {
            var monthEnd = monthStart.AddMonths(1);
            var label = monthStart.ToString("MMM");

            var tenants = await _db.Tenants.AsNoTracking().CountAsync(t => t.CreatedAt < monthEnd, cancellationToken);
            var users = await _db.Users.AsNoTracking().CountAsync(u => u.CreatedAt < monthEnd, cancellationToken);
            var revenue = await _db.ManualPayments.AsNoTracking()
                .Where(p => p.PaidAt < monthEnd)
                .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

            tenantPoints.Add(new AdminPlatformAnalyticsSeriesPoint
            {
                Name = label,
                Tenants = tenants,
                Users = users,
                Revenue = revenue
            });
        }

        return tenantPoints;
    }

    public sealed class AdminPlatformAnalyticsResponse
    {
        public int TotalTenants { get; set; }
        public int ActiveTenants { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalVideos { get; set; }
        public long TotalViews { get; set; }
        public long TotalStorageUsed { get; set; }
        public decimal Revenue { get; set; }
        public int NewTenantsThisMonth { get; set; }
        public int NewUsersThisMonth { get; set; }
        public double ChurnRate { get; set; }
        public List<AdminPlatformAnalyticsSeriesPoint> Series { get; set; } = new();
    }

    public sealed class AdminPlatformAnalyticsSeriesPoint
    {
        public string Name { get; set; } = string.Empty;
        public int Tenants { get; set; }
        public int Users { get; set; }
        public decimal Revenue { get; set; }
    }
}
