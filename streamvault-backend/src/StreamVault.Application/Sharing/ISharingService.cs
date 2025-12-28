using StreamVault.Application.Sharing.DTOs;

namespace StreamVault.Application.Sharing;

public interface ISharingService
{
    Task<ShareLinkDto> GenerateShareLinkAsync(Guid videoId, CreateShareLinkRequest request, Guid userId, Guid tenantId);
    Task<List<ShareLinkDto>> GetVideoShareLinksAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<ShareLinkDto?> GetShareLinkAsync(Guid shareId, string token);
    Task UpdateShareLinkAsync(Guid shareId, UpdateShareLinkRequest request, Guid userId, Guid tenantId);
    Task DeleteShareLinkAsync(Guid shareId, Guid userId, Guid tenantId);
    Task<VideoDto?> GetSharedVideoAsync(string token);
}
