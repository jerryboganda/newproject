using System.Text.Json;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Services;

public interface IWebhookPublisher
{
    Task PublishAsync(Guid tenantId, string eventType, object payload, CancellationToken cancellationToken = default);
}

public sealed class WebhookPublisher : IWebhookPublisher
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly IBackgroundJobClient _backgroundJobs;

    public WebhookPublisher(StreamVaultDbContext dbContext, IBackgroundJobClient backgroundJobs)
    {
        _dbContext = dbContext;
        _backgroundJobs = backgroundJobs;
    }

    public async Task PublishAsync(Guid tenantId, string eventType, object payload, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            return;

        var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var subscriptions = await _dbContext.WebhookSubscriptions
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.IsActive && (s.Events.Contains(eventType) || s.Events.Contains("*")))
            .ToListAsync(cancellationToken);

        if (subscriptions.Count == 0)
            return;

        var now = DateTimeOffset.UtcNow;

        foreach (var subscription in subscriptions)
        {
            _dbContext.WebhookDeliveries.Add(new WebhookDelivery
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SubscriptionId = subscription.Id,
                EventType = eventType.Trim(),
                PayloadJson = payloadJson,
                Status = WebhookDeliveryStatus.Pending,
                AttemptCount = 0,
                NextAttemptAt = now,
                CreatedAt = now
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Kick delivery in the background right away.
        _backgroundJobs.Enqueue<Jobs.WebhookDeliveryJob>(job => job.RunOnceAsync(CancellationToken.None));
    }
}
