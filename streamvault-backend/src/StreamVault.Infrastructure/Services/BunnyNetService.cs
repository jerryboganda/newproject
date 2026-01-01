using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StreamVault.Application.Interfaces;
using StreamVault.Application.Services;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Infrastructure.Services;

public class BunnyNetService : IBunnyNetService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITenantContext _tenantContext;
    private readonly StreamVaultDbContext _dbContext;
    private readonly ILogger<BunnyNetService> _logger;

    public BunnyNetService(
        IHttpClientFactory httpClientFactory,
        ITenantContext tenantContext,
        StreamVaultDbContext dbContext,
        ILogger<BunnyNetService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _tenantContext = tenantContext;
        _dbContext = dbContext;
        _logger = logger;
    }

    public bool IsConfiguredForCurrentTenant()
    {
        var tenant = _tenantContext.CurrentTenant;
        if (tenant == null) return false;

        return !string.IsNullOrWhiteSpace(tenant.BunnyApiKey)
            && !string.IsNullOrWhiteSpace(tenant.BunnyLibraryId);
    }

    public async Task<BunnyVideoUploadResult> UploadVideoToStreamAsync(
        Stream videoStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var tenant = await EnsureTenantResolvedAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(tenant.BunnyApiKey) || string.IsNullOrWhiteSpace(tenant.BunnyLibraryId))
            throw new InvalidOperationException("Bunny.net is not configured for this tenant");

        var libraryId = tenant.BunnyLibraryId!.Trim();
        var apiKey = tenant.BunnyApiKey!.Trim();

        _logger.LogInformation("Uploading video to Bunny Stream. tenant={TenantSlug} libraryId={LibraryId} file={FileName}", tenant.Slug, libraryId, fileName);

        var apiClient = _httpClientFactory.CreateClient("bunny-api");

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, $"https://api.bunny.net/library/{libraryId}/videos");
        createRequest.Headers.Add("AccessKey", apiKey);
        createRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var title = Path.GetFileNameWithoutExtension(fileName);
        createRequest.Content = JsonContent.Create(new { title, collectionId = (string?)null });

        using var createResponse = await apiClient.SendAsync(createRequest, cancellationToken);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<BunnyCreateVideoResponse>(cancellationToken: cancellationToken);
        if (created == null || created.Guid == Guid.Empty)
            throw new InvalidOperationException("Bunny Stream video creation failed");

        var videoGuid = created.Guid.ToString();

        using var uploadClient = _httpClientFactory.CreateClient("bunny-upload");
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(videoStream);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
        content.Add(fileContent, "file", fileName);

        using var uploadRequest = new HttpRequestMessage(HttpMethod.Put, $"https://video.bunny.net/upload/{videoGuid}");
        uploadRequest.Headers.Add("AccessKey", apiKey);
        uploadRequest.Content = content;

        using var uploadResponse = await uploadClient.SendAsync(uploadRequest, cancellationToken);
        uploadResponse.EnsureSuccessStatusCode();

        return new BunnyVideoUploadResult(
            VideoId: videoGuid,
            LibraryId: libraryId,
            CdnHostname: tenant.BunnyCdnHostname,
            Mp4Url: GetStreamMp4Url(videoGuid),
            ThumbnailUrl: GetStreamThumbnailUrl(videoGuid),
            Status: "uploaded");
    }

    public string? GetStreamMp4Url(string bunnyVideoId)
    {
        var tenant = _tenantContext.CurrentTenant;
        if (tenant == null) return null;

        var hostname = string.IsNullOrWhiteSpace(tenant.BunnyCdnHostname) ? null : tenant.BunnyCdnHostname.Trim();
        if (string.IsNullOrWhiteSpace(hostname))
            return null;

        return $"https://{hostname}/{bunnyVideoId}/play.mp4";
    }

    public string? GetStreamThumbnailUrl(string bunnyVideoId)
    {
        var tenant = _tenantContext.CurrentTenant;
        if (tenant == null) return null;

        var hostname = string.IsNullOrWhiteSpace(tenant.BunnyCdnHostname) ? null : tenant.BunnyCdnHostname.Trim();
        if (string.IsNullOrWhiteSpace(hostname))
            return null;

        return $"https://{hostname}/{bunnyVideoId}/thumbnail.jpg";
    }

    private async Task<StreamVault.Domain.Entities.Tenant> EnsureTenantResolvedAsync(CancellationToken cancellationToken)
    {
        if (_tenantContext.CurrentTenant != null)
            return _tenantContext.CurrentTenant;

        // Fallback: attempt to resolve tenant by header-driven slug stored on the context (if set elsewhere)
        // or by the first active tenant (demo-friendly).
        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Status == StreamVault.Domain.Entities.TenantStatus.Active, cancellationToken);
        if (tenant == null)
            throw new InvalidOperationException("No active tenant found");

        _tenantContext.SetCurrentTenant(tenant);
        return tenant;
    }

    private sealed class BunnyCreateVideoResponse
    {
        public Guid Guid { get; set; }
    }
}
