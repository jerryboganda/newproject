using StreamVault.Domain.Interfaces;

namespace StreamVault.Domain.Entities;

public class WebhookDelivery : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Guid SubscriptionId { get; set; }
    public WebhookSubscription Subscription { get; set; } = null!;

    public string EventType { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public WebhookDeliveryStatus Status { get; set; } = WebhookDeliveryStatus.Pending;

    public int AttemptCount { get; set; } = 0;

    public DateTimeOffset NextAttemptAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? DeliveredAt { get; set; }

    public int? LastResponseStatusCode { get; set; }

    public string? LastResponseBody { get; set; }

    public string? LastError { get; set; }
}

public enum WebhookDeliveryStatus
{
    Pending,
    Retrying,
    Succeeded,
    Failed
}
