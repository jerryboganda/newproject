using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/admin/tenants")]
[Authorize(Roles = "SuperAdmin")]
public class AdminTenantsController : ControllerBase
{
    private readonly StreamVaultDbContext _db;

    public AdminTenantsController(StreamVaultDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminTenantListItem>>> ListTenants(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = _db.Tenants.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(t => EF.Functions.ILike(t.Name, $"%{term}%") || EF.Functions.ILike(t.Slug, $"%{term}%"));
        }

        var tenants = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Slug,
                t.LogoUrl,
                t.Status,
                t.SuspensionReason,
                t.CreatedAt,
                t.UpdatedAt,
                t.TrialEndsAt
            })
            .ToListAsync(cancellationToken);

        var tenantIds = tenants.Select(t => t.Id).ToArray();

        var userCounts = await _db.Users.AsNoTracking()
            .Where(u => tenantIds.Contains(u.TenantId))
            .GroupBy(u => u.TenantId)
            .Select(g => new { TenantId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var videoStats = await _db.Videos.AsNoTracking()
            .Where(v => tenantIds.Contains(v.TenantId) && v.Status != VideoStatus.Deleted)
            .GroupBy(v => v.TenantId)
            .Select(g => new
            {
                TenantId = g.Key,
                Videos = g.Count(),
                Views = g.Sum(x => x.ViewCount),
                StorageUsedBytes = g.Sum(x => x.FileSizeBytes)
            })
            .ToListAsync(cancellationToken);

        var subscriptionSummaries = await _db.TenantSubscriptions.AsNoTracking()
            .Where(s => tenantIds.Contains(s.TenantId))
            .OrderByDescending(s => s.CurrentPeriodEnd ?? DateTimeOffset.MinValue)
            .Select(s => new
            {
                s.TenantId,
                s.Status,
                s.BillingCycle,
                s.CurrentPeriodEnd,
                s.CurrentPeriodStart,
                s.CancelAt,
                s.TrialEnd,
                PlanName = s.Plan.Name
            })
            .ToListAsync(cancellationToken);

        var subscriptionsByTenant = subscriptionSummaries
            .GroupBy(x => x.TenantId)
            .ToDictionary(g => g.Key, g => g.FirstOrDefault());

        var userCountsByTenant = userCounts.ToDictionary(x => x.TenantId, x => x.Count);
        var videoStatsByTenant = videoStats.ToDictionary(x => x.TenantId, x => x);

        var items = tenants.Select(t =>
        {
            userCountsByTenant.TryGetValue(t.Id, out var users);
            videoStatsByTenant.TryGetValue(t.Id, out var vs);
            subscriptionsByTenant.TryGetValue(t.Id, out var sub);

            var isActive = t.Status == TenantStatus.Active || (t.Status == TenantStatus.Trial && t.TrialEndsAt.HasValue && t.TrialEndsAt > DateTimeOffset.UtcNow);

            return new AdminTenantListItem
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                LogoUrl = t.LogoUrl,
                IsActive = isActive,
                IsSuspended = t.Status == TenantStatus.Suspended,
                SuspensionReason = t.SuspensionReason,
                CreatedAt = t.CreatedAt,
                Subscription = sub == null
                    ? null
                    : new AdminTenantSubscriptionSummary
                    {
                        Plan = sub.PlanName,
                        Status = sub.Status.ToString(),
                        BillingCycle = sub.BillingCycle.ToString(),
                        CurrentPeriodStart = sub.CurrentPeriodStart,
                        CurrentPeriodEnd = sub.CurrentPeriodEnd,
                        CancelAt = sub.CancelAt,
                        TrialEnd = sub.TrialEnd
                    },
                Stats = new AdminTenantStats
                {
                    Users = users,
                    Videos = vs?.Videos ?? 0,
                    Views = vs?.Views ?? 0,
                    StorageUsedBytes = vs?.StorageUsedBytes ?? 0
                }
            };
        }).ToList();

        return Ok(items);
    }

    [HttpPost("{tenantId:guid}/suspend")]
    public async Task<IActionResult> SuspendTenant([FromRoute] Guid tenantId, [FromBody] SuspendTenantRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { error = "Reason is required" });

        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
        if (tenant == null)
            return NotFound();

        tenant.Suspend(request.Reason.Trim());

        var users = await _db.Users.Where(u => u.TenantId == tenantId).ToListAsync(cancellationToken);
        foreach (var user in users)
        {
            user.Status = UserStatus.Suspended;
            user.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{tenantId:guid}/unsuspend")]
    public async Task<IActionResult> UnsuspendTenant([FromRoute] Guid tenantId, CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
        if (tenant == null)
            return NotFound();

        tenant.Activate();

        var users = await _db.Users.Where(u => u.TenantId == tenantId && u.Status == UserStatus.Suspended).ToListAsync(cancellationToken);
        foreach (var user in users)
        {
            user.Status = UserStatus.Active;
            user.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    public sealed class SuspendTenantRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public sealed class AdminTenantListItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuspended { get; set; }
        public string? SuspensionReason { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public AdminTenantSubscriptionSummary? Subscription { get; set; }
        public AdminTenantStats Stats { get; set; } = new();
    }

    public sealed class AdminTenantStats
    {
        public int Users { get; set; }
        public int Videos { get; set; }
        public int Views { get; set; }
        public long StorageUsedBytes { get; set; }
    }

    public sealed class AdminTenantSubscriptionSummary
    {
        public string Plan { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string BillingCycle { get; set; } = string.Empty;
        public DateTimeOffset? CurrentPeriodStart { get; set; }
        public DateTimeOffset? CurrentPeriodEnd { get; set; }
        public DateTimeOffset? CancelAt { get; set; }
        public DateTimeOffset? TrialEnd { get; set; }
    }
}
