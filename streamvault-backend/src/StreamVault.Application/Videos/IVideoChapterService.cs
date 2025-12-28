using StreamVault.Application.Videos.DTOs;

namespace StreamVault.Application.Videos;

public interface IVideoChapterService
{
    Task<VideoChapterDto> CreateChapterAsync(Guid videoId, Guid userId, Guid tenantId, CreateChapterRequest request);
    Task<bool> UpdateChapterAsync(Guid chapterId, Guid userId, Guid tenantId, UpdateChapterRequest request);
    Task<bool> DeleteChapterAsync(Guid chapterId, Guid userId, Guid tenantId);
    Task<List<VideoChapterDto>> GetVideoChaptersAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<VideoChapterDto?> GetChapterAsync(Guid chapterId, Guid userId, Guid tenantId);
    Task<bool> ReorderChaptersAsync(Guid videoId, Guid userId, Guid tenantId, List<Guid> chapterIds);
    Task<VideoMarkerDto> AddMarkerAsync(Guid videoId, Guid userId, Guid tenantId, CreateMarkerRequest request);
    Task<bool> UpdateMarkerAsync(Guid markerId, Guid userId, Guid tenantId, UpdateMarkerRequest request);
    Task<bool> DeleteMarkerAsync(Guid markerId, Guid userId, Guid tenantId);
    Task<List<VideoMarkerDto>> GetVideoMarkersAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<List<VideoMarkerDto>> GetMarkersByTypeAsync(Guid videoId, Guid userId, Guid tenantId, string type);
    Task<VideoChapterDto> AutoGenerateChaptersAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<bool> ImportChaptersAsync(Guid videoId, Guid userId, Guid tenantId, ImportChaptersRequest request);
    Task<string> ExportChaptersAsync(Guid videoId, Guid userId, Guid tenantId, string format = "json");
    Task<List<ChapterNavigationDto>> GetChapterNavigationAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<bool> SetChapterThumbnailAsync(Guid chapterId, Guid userId, Guid tenantId, string thumbnailUrl);
}
