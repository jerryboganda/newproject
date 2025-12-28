using StreamVault.Application.VideoSettings.DTOs;

namespace StreamVault.Application.VideoSettings;

public interface IVideoSettingsService
{
    Task<VideoSettingsDto> GetSettingsAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<VideoSettingsDto> UpdateSettingsAsync(Guid videoId, UpdateVideoSettingsRequest request, Guid userId, Guid tenantId);
    Task<VideoSettingsDto> UpdatePlaybackSpeedAsync(Guid videoId, double speed, Guid userId, Guid tenantId);
    Task<VideoSettingsDto> UpdateVolumeAsync(Guid videoId, int volume, Guid userId, Guid tenantId);
    Task<VideoSettingsDto> UpdatePositionAsync(Guid videoId, double positionSeconds, Guid userId, Guid tenantId);
    Task ResetSettingsAsync(Guid videoId, Guid userId, Guid tenantId);
}
