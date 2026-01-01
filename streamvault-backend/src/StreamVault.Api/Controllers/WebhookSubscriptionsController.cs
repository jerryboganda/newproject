using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamVault.Api.Services;
using StreamVault.Application.Interfaces;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/webhooks/subscriptions")]
[Authorize]
public class WebhookSubscriptionsController : ControllerBase
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly AuditLogger _audit;

    public WebhookSubscriptionsController(StreamVaultDbContext dbContext, ITenantContext tenantContext, AuditLogger audit)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _audit = audit;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WebhookSubscriptionListItem>>> List(CancellationToken cancellationToken = default)
    {
        var (tenantId, _) = await ResolveTenantAsync(cancellationToken);
        if (tenantId == null)
            return BadRequest(new { error = "Tenant could not be resolved" });

        var items = await _dbContext.WebhookSubscriptions
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new WebhookSubscriptionListItem(s.Id, s.Url, s.Events, s.IsActive, s.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<CreateWebhookSubscriptionResponse>> Create([FromBody] CreateWebhookSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var (tenantId, _) = await ResolveTenantAsync(cancellationToken);
        if (tenantId == null)
            return BadRequest(new { error = "Tenant could not be resolved" });

        var url = (request.Url ?? string.Empty).Trim();
        if (!IsAllowedUrl(url))
            return BadRequest(new { error = "Url must be https (or http://localhost in development)" });

        var events = (request.Events ?? Array.Empty<string>())
            .Select(e => (e ?? string.Empty).Trim())
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (events.Length == 0)
            return BadRequest(new { error = "At least one event is required" });

        var secret = GenerateSecret();

        var sub = new WebhookSubscription
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            Url = url,
            Events = events,
            SigningSecret = secret,
            IsActive = request.IsActive ?? true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.WebhookSubscriptions.Add(sub);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _audit.TryLogAsync(
            HttpContext,
            action: "webhook_subscription.created",
            entityType: "WebhookSubscription",
            entityId: sub.Id,
            oldValues: null,
            newValues: new Dictionary<string, object>
            {
                ["url"] = sub.Url,
                ["events"] = sub.Events,
                ["isActive"] = sub.IsActive
            },
            tenantIdOverride: tenantId,
            cancellationToken: cancellationToken);

        return Ok(new CreateWebhookSubscriptionResponse(sub.Id, sub.Url, sub.Events, sub.IsActive, sub.CreatedAt, secret));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateWebhookSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var (tenantId, _) = await ResolveTenantAsync(cancellationToken);
        if (tenantId == null)
            return BadRequest(new { error = "Tenant could not be resolved" });

        var sub = await _dbContext.WebhookSubscriptions.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, cancellationToken);
        if (sub == null)
            return NotFound(new { error = "Subscription not found" });

        var oldUrl = sub.Url;
        var oldEvents = sub.Events;
        var oldActive = sub.IsActive;

        if (request.Url != null)
        {
            var url = request.Url.Trim();
            if (!IsAllowedUrl(url))
                return BadRequest(new { error = "Url must be https (or http://localhost in development)" });
            sub.Url = url;
        }

        if (request.Events != null)
        {
            var events = request.Events
                .Select(e => (e ?? string.Empty).Trim())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (events.Length == 0)
                return BadRequest(new { error = "At least one event is required" });

            sub.Events = events;
        }

        if (request.IsActive.HasValue)
            sub.IsActive = request.IsActive.Value;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _audit.TryLogAsync(
            HttpContext,
            action: "webhook_subscription.updated",
            entityType: "WebhookSubscription",
            entityId: sub.Id,
            oldValues: new Dictionary<string, object>
            {
                ["url"] = oldUrl,
                ["events"] = oldEvents,
                ["isActive"] = oldActive
            },
            newValues: new Dictionary<string, object>
            {
                ["url"] = sub.Url,
                ["events"] = sub.Events,
                ["isActive"] = sub.IsActive
            },
            tenantIdOverride: tenantId,
            cancellationToken: cancellationToken);

        return Ok(new { success = true });
    }

    [HttpPost("{id:guid}/rotate-secret")]
    public async Task<ActionResult<RotateWebhookSecretResponse>> RotateSecret([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var (tenantId, _) = await ResolveTenantAsync(cancellationToken);
        if (tenantId == null)
            return BadRequest(new { error = "Tenant could not be resolved" });

        var sub = await _dbContext.WebhookSubscriptions.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, cancellationToken);
        if (sub == null)
            return NotFound(new { error = "Subscription not found" });

        var secret = GenerateSecret();
        sub.SigningSecret = secret;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _audit.TryLogAsync(
            HttpContext,
            action: "webhook_subscription.secret_rotated",
            entityType: "WebhookSubscription",
            entityId: sub.Id,
            oldValues: null,
            newValues: new Dictionary<string, object>
            {
                ["url"] = sub.Url
            },
            tenantIdOverride: tenantId,
            cancellationToken: cancellationToken);

        return Ok(new RotateWebhookSecretResponse(sub.Id, secret));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var (tenantId, _) = await ResolveTenantAsync(cancellationToken);
        if (tenantId == null)
            return BadRequest(new { error = "Tenant could not be resolved" });

        var sub = await _dbContext.WebhookSubscriptions.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, cancellationToken);
        if (sub == null)
            return NotFound(new { error = "Subscription not found" });

        var oldActive = sub.IsActive;
        sub.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _audit.TryLogAsync(
            HttpContext,
            action: "webhook_subscription.deleted",
            entityType: "WebhookSubscription",
            entityId: sub.Id,
            oldValues: new Dictionary<string, object>
            {
                ["isActive"] = oldActive
            },
            newValues: new Dictionary<string, object>
            {
                ["isActive"] = sub.IsActive,
                ["url"] = sub.Url
            },
            tenantIdOverride: tenantId,
            cancellationToken: cancellationToken);

        return Ok(new { success = true });
    }

    private async Task<(Guid? tenantId, bool isAuthenticated)> ResolveTenantAsync(CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId.HasValue)
            return (_tenantContext.TenantId, User.Identity?.IsAuthenticated == true);

        var tenantClaim = User.FindFirstValue("tenant_id");
        if (Guid.TryParse(tenantClaim, out var tenantIdFromToken))
        {
            var tenant = await _dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantIdFromToken, cancellationToken);
            if (tenant != null)
            {
                _tenantContext.SetCurrentTenant(tenant);
                return (tenant.Id, User.Identity?.IsAuthenticated == true);
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
                    return (tenant.Id, User.Identity?.IsAuthenticated == true);
                }
            }
        }

        return (null, User.Identity?.IsAuthenticated == true);
    }

    private static bool IsAllowedUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            return true;

        if (uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) && uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private static string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}

public record WebhookSubscriptionListItem(Guid Id, string Url, string[] Events, bool IsActive, DateTimeOffset CreatedAt);

public record CreateWebhookSubscriptionRequest(string Url, string[] Events, bool? IsActive);

public record CreateWebhookSubscriptionResponse(Guid Id, string Url, string[] Events, bool IsActive, DateTimeOffset CreatedAt, string SigningSecret);

public record UpdateWebhookSubscriptionRequest(string? Url, string[]? Events, bool? IsActive);

public record RotateWebhookSecretResponse(Guid Id, string SigningSecret);
