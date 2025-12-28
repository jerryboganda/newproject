using StreamVault.Application.Playlists.DTOs;

namespace StreamVault.Application.Playlists;

public interface IPlaylistService
{
    Task<PlaylistDto> CreatePlaylistAsync(CreatePlaylistRequest request, Guid userId, Guid tenantId);
    Task<PlaylistDto> UpdatePlaylistAsync(Guid id, UpdatePlaylistRequest request, Guid userId, Guid tenantId);
    Task DeletePlaylistAsync(Guid id, Guid userId, Guid tenantId);
    Task<PlaylistDto?> GetPlaylistAsync(Guid id, Guid tenantId);
    Task<List<PlaylistDto>> GetUserPlaylistsAsync(Guid userId, Guid tenantId);
    Task AddVideoToPlaylistAsync(Guid playlistId, Guid videoId, Guid userId, Guid tenantId);
    Task RemoveVideoFromPlaylistAsync(Guid playlistId, Guid videoId, Guid userId, Guid tenantId);
    Task ReorderPlaylistAsync(Guid playlistId, List<PlaylistVideoOrder> videoOrders, Guid userId, Guid tenantId);
}
