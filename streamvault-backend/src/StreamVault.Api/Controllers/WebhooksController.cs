using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using StreamVault.Api.Services;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("webhooks/bunny")]
[DisableRateLimiting]
public class WebhooksController : ControllerBase
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly ILogger<WebhooksController> _logger;
    private readonly IWebhookPublisher _webhookPublisher;

    public WebhooksController(StreamVaultDbContext dbContext, ILogger<WebhooksController> logger, IWebhookPublisher webhookPublisher)
    {
        _dbContext = dbContext;
        _logger = logger;
        _webhookPublisher = webhookPublisher;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Bunny([FromBody] JsonElement payload, CancellationToken cancellationToken)
    {
        // Bunny webhook payload formats can vary by product (Stream/Encoding/Webhooks).
        // We extract the most common identifiers + status values defensively.
        var (externalVideoId, status) = ExtractVideoIdAndStatus(payload);

        _logger.LogInformation("Received Bunny webhook. videoId={VideoId} status={Status}", externalVideoId, status);

        if (string.IsNullOrWhiteSpace(externalVideoId))
            return Ok(new { success = true });

        try
        {
            // In this codebase we currently don’t have a dedicated BunnyVideoId column.
            // We store the Bunny guid/id in Video.StoragePath during upload.
            var video = await _dbContext.Videos
                .FirstOrDefaultAsync(v => v.StoragePath == externalVideoId, cancellationToken);

            if (video == null)
                return Ok(new { success = true });

            var mapped = MapStatus(status);
            if (mapped.HasValue)
            {
                video.Status = mapped.Value;
                video.UpdatedAt = DateTimeOffset.UtcNow;
                if (mapped.Value == VideoStatus.Processed && !video.PublishedAt.HasValue)
                {
                    video.PublishedAt = DateTimeOffset.UtcNow;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);

                await _webhookPublisher.PublishAsync(
                    video.TenantId,
                    "video.status_changed",
                    new
                    {
                        id = video.Id,
                        storagePath = video.StoragePath,
                        status = video.Status.ToString(),
                        updatedAt = video.UpdatedAt
                    },
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // During early Phase 2, the Videos table/migrations might not exist yet.
            // Webhooks should be non-blocking (ack 200) to avoid Bunny retries storming the API.
            _logger.LogWarning(ex, "Bunny webhook processing skipped due to exception");
        }

        return Ok(new { success = true });
    }

    private static (string? videoId, string? status) ExtractVideoIdAndStatus(JsonElement payload)
    {
        static string? TryGetString(JsonElement obj, params string[] names)
        {
            foreach (var name in names)
            {
                if (obj.ValueKind == JsonValueKind.Object && obj.TryGetProperty(name, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.String)
                        return prop.GetString();
                    if (prop.ValueKind == JsonValueKind.Number)
                        return prop.GetRawText();
                }
            }
            return null;
        }

        // Common shapes:
        // { "videoId": "...", "status": "processed" }
        // { "VideoGuid": "...", "Status": "processing" }
        // { "Video": { "Guid": "...", "Status": "processed" } }
        var videoId =
            TryGetString(payload, "videoId", "videoGuid", "guid", "VideoId", "VideoGuid", "Guid")
            ?? (payload.TryGetProperty("Video", out var videoObj)
                ? TryGetString(videoObj, "videoId", "videoGuid", "guid", "VideoId", "VideoGuid", "Guid")
                : null);

        var status =
            TryGetString(payload, "status", "Status")
            ?? (payload.TryGetProperty("Video", out var videoObj2)
                ? TryGetString(videoObj2, "status", "Status")
                : null);

        return (videoId, status);
    }

    private static VideoStatus? MapStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        return status.Trim().ToLowerInvariant() switch
        {
            "uploaded" => VideoStatus.Uploaded,
            "processing" => VideoStatus.Processing,
            "processed" => VideoStatus.Processed,
            "ready" => VideoStatus.Processed,
            "completed" => VideoStatus.Processed,
            "failed" => VideoStatus.Failed,
            _ => null
        };
    }
}
