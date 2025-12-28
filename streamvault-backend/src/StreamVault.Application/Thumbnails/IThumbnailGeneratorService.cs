using StreamVault.Application.Thumbnails.DTOs;

namespace StreamVault.Application.Thumbnails;

public interface IThumbnailGeneratorService
{
    Task<List<VideoThumbnailDto>> GenerateThumbnailsAsync(Guid videoId, GenerateThumbnailsRequest request, Guid userId, Guid tenantId);
    Task<VideoThumbnailDto> GenerateThumbnailAtPositionAsync(Guid videoId, int positionSeconds, ThumbnailOptions options, Guid userId, Guid tenantId);
    Task<VideoThumbnailDto> UploadCustomThumbnailAsync(Guid videoId, UploadThumbnailRequest request, Guid userId, Guid tenantId);
    Task<List<VideoThumbnailDto>> GetThumbnailsAsync(Guid videoId, Guid tenantId);
    Task SetDefaultThumbnailAsync(Guid videoId, Guid thumbnailId, Guid userId, Guid tenantId);
    Task DeleteThumbnailAsync(Guid thumbnailId, Guid userId, Guid tenantId);
    Task<string> GenerateSpriteSheetAsync(Guid videoId, SpriteSheetRequest request, Guid userId, Guid tenantId);
}
