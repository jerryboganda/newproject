using StreamVault.Application.Embed.DTOs;

namespace StreamVault.Application.Embed;

public interface IEmbedService
{
    Task<EmbedConfigDto> GetEmbedConfigAsync(Guid videoId, Guid tenantId, EmbedOptionsDto options);
    Task<string> GenerateEmbedCodeAsync(Guid videoId, Guid tenantId, EmbedOptionsDto options);
    Task<List<EmbedAnalyticsDto>> GetEmbedAnalyticsAsync(Guid videoId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
}
