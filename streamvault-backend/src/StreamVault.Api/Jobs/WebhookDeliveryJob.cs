using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Jobs;

public class WebhookDeliveryJob
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookDeliveryJob> _logger;

    public WebhookDeliveryJob(StreamVaultDbContext dbContext, IHttpClientFactory httpClientFactory, ILogger<WebhookDeliveryJob> logger)
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 0)]
    public async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var due = await _dbContext.WebhookDeliveries
            .Include(d => d.Subscription)
            .Where(d =>
                (d.Status == WebhookDeliveryStatus.Pending || d.Status == WebhookDeliveryStatus.Retrying)
                && d.NextAttemptAt <= now)
            .OrderBy(d => d.NextAttemptAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (due.Count == 0)
            return;

        foreach (var delivery in due)
        {
            await DeliverOneAsync(delivery, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task DeliverOneAsync(WebhookDelivery delivery, CancellationToken cancellationToken)
    {
        var subscription = delivery.Subscription;
        if (subscription == null || !subscription.IsActive)
        {
            delivery.Status = WebhookDeliveryStatus.Failed;
            delivery.LastError = "Subscription not active";
            return;
        }

        if (string.IsNullOrWhiteSpace(subscription.Url))
        {
            delivery.Status = WebhookDeliveryStatus.Failed;
            delivery.LastError = "Subscription URL missing";
            return;
        }

        delivery.AttemptCount += 1;

        try
        {
            var client = _httpClientFactory.CreateClient("webhooks");

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var signature = ComputeSignature(subscription.SigningSecret ?? string.Empty, timestamp, delivery.PayloadJson);

            using var req = new HttpRequestMessage(HttpMethod.Post, subscription.Url);
            req.Headers.UserAgent.Add(new ProductInfoHeaderValue("StreamVault", "1.0"));
            req.Headers.Add("X-StreamVault-Event", delivery.EventType);
            req.Headers.Add("X-StreamVault-Delivery-Id", delivery.Id.ToString());
            req.Headers.Add("X-StreamVault-Timestamp", timestamp);
            req.Headers.Add("X-StreamVault-Signature", signature);

            req.Content = new StringContent(delivery.PayloadJson, Encoding.UTF8, "application/json");

            using var resp = await client.SendAsync(req, cancellationToken);
            delivery.LastResponseStatusCode = (int)resp.StatusCode;

            var body = await resp.Content.ReadAsStringAsync(cancellationToken);
            delivery.LastResponseBody = Truncate(body, 4000);

            if ((int)resp.StatusCode >= 200 && (int)resp.StatusCode < 300)
            {
                delivery.Status = WebhookDeliveryStatus.Succeeded;
                delivery.DeliveredAt = DateTimeOffset.UtcNow;
                delivery.LastError = null;
                return;
            }

            ScheduleRetry(delivery, $"Non-success status code: {(int)resp.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Webhook delivery failed {DeliveryId}", delivery.Id);
            ScheduleRetry(delivery, ex.Message);
        }
    }

    private static void ScheduleRetry(WebhookDelivery delivery, string error)
    {
        delivery.LastError = error;

        var maxAttempts = 10;
        if (delivery.AttemptCount >= maxAttempts)
        {
            delivery.Status = WebhookDeliveryStatus.Failed;
            return;
        }

        delivery.Status = WebhookDeliveryStatus.Retrying;

        // Exponential backoff capped at 1 hour.
        var attempt = Math.Max(1, delivery.AttemptCount);
        var backoffSeconds = Math.Min(30 * Math.Pow(2, attempt - 1), 3600);
        var jitterSeconds = RandomNumberGenerator.GetInt32(0, 15);
        delivery.NextAttemptAt = DateTimeOffset.UtcNow.AddSeconds(backoffSeconds + jitterSeconds);
    }

    private static string ComputeSignature(string secret, string timestamp, string payloadJson)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.UTF8.GetBytes($"{timestamp}.{payloadJson}");

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(data);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    private static string? Truncate(string? value, int maxLen)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLen ? value : value.Substring(0, maxLen);
    }
}
