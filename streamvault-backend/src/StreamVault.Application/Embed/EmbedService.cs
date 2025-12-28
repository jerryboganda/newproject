using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StreamVault.Application.Embed.DTOs;
using StreamVault.Application.Services;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Embed;

public class EmbedService : IEmbedService
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly IStorageService _storageService;
    private readonly IConfiguration _configuration;

    public EmbedService(StreamVaultDbContext dbContext, IStorageService storageService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _configuration = configuration;
    }

    public async Task<EmbedConfigDto> GetEmbedConfigAsync(Guid videoId, Guid tenantId, EmbedOptionsDto options)
    {
        // Verify video exists and belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var baseUrl = _configuration["BaseUrl"] ?? "https://your-domain.com";
        var embedUrl = $"{baseUrl}/embed/{videoId}";
        
        // Build query parameters
        var queryParams = new List<string>();
        if (options.Autoplay) queryParams.Add("autoplay=1");
        if (!options.Controls) queryParams.Add("controls=0");
        if (options.Loop) queryParams.Add("loop=1");
        if (options.Muted) queryParams.Add("muted=1");
        if (!string.IsNullOrEmpty(options.Color)) queryParams.Add($"color={Uri.EscapeDataString(options.Color)}");
        if (!options.ShowTitle) queryParams.Add("title=0");
        if (!options.ShowPortrait) queryParams.Add("portrait=0");
        if (!options.ShowByline) queryParams.Add("byline=0");
        if (!string.IsNullOrEmpty(options.StartAt)) queryParams.Add($"start={options.StartAt}");

        if (queryParams.Any())
            embedUrl += "?" + string.Join("&", queryParams);

        // Generate embed code
        var embedCode = options.Responsive
            ? $"<div style=\"padding:56.25% 0 0 0;position:relative;\"><iframe src=\"{embedUrl}\" style=\"position:absolute;top:0;left:0;width:100%;height:100%;\" frameborder=\"0\" allow=\"autoplay; fullscreen; picture-in-picture\" allowfullscreen></iframe></div>"
            : $"<iframe src=\"{embedUrl}\" width=\"{options.Width}\" height=\"{options.Height}\" frameborder=\"0\" allow=\"autoplay; fullscreen; picture-in-picture\" allowfullscreen></iframe>";

        return new EmbedConfigDto
        {
            VideoId = videoId,
            EmbedUrl = embedUrl,
            EmbedCode = embedCode,
            Options = options,
            PlayerUrl = $"{baseUrl}/player/{videoId}"
        };
    }

    public async Task<string> GenerateEmbedCodeAsync(Guid videoId, Guid tenantId, EmbedOptionsDto options)
    {
        var config = await GetEmbedConfigAsync(videoId, tenantId, options);
        return config.EmbedCode;
    }

    public async Task<List<EmbedAnalyticsDto>> GetEmbedAnalyticsAsync(Guid videoId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        // Verify video exists and belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        // This is a simplified implementation
        // In production, you'd track embed analytics in your database
        var analytics = new List<EmbedAnalyticsDto>();

        // Mock data
        for (int i = 0; i < 10; i++)
        {
            analytics.Add(new EmbedAnalyticsDto
            {
                Id = Guid.NewGuid(),
                VideoId = videoId,
                Domain = "example.com",
                Referrer = "https://google.com",
                UserAgent = "Mozilla/5.0...",
                IpAddress = "192.168.1.1",
                Timestamp = DateTimeOffset.UtcNow.AddDays(-i),
                PlayCount = Random.Shared.Next(1, 10),
                WatchTimeSeconds = Random.Shared.Next(30, 300)
            });
        }

        return analytics;
    }
}
