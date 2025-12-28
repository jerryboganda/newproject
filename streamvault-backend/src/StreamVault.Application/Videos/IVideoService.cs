using StreamVault.Application.Videos.DTOs;

namespace StreamVault.Application.Videos;

public interface IVideoService
{
    Task<VideoUploadResponse> InitiateUploadAsync(VideoUploadRequest request, Guid userId, Guid tenantId);
    Task<ChunkUploadResponse> GetChunkUploadUrlAsync(ChunkUploadRequest request);
    Task CompleteUploadAsync(string videoId, string uploadToken);
    Task<VideoDto?> GetVideoAsync(Guid id, Guid tenantId);
    Task<VideoListResponse> GetVideosAsync(VideoListRequest request, Guid tenantId);
    Task<VideoDto> UpdateVideoAsync(Guid id, VideoUpdateRequest request, Guid userId, Guid tenantId);
    Task DeleteVideoAsync(Guid id, Guid userId, Guid tenantId);
    Task IncrementViewCountAsync(Guid id, Guid tenantId);
}
