using StreamVault.Application.Chapters.DTOs;

namespace StreamVault.Application.Chapters;

public interface IChapterService
{
    Task<List<ChapterDto>> GetChaptersAsync(Guid videoId, Guid tenantId);
    Task<ChapterDto?> GetChapterAsync(Guid chapterId, Guid videoId, Guid tenantId);
    Task<ChapterDto> CreateChapterAsync(CreateChapterRequest request, Guid userId, Guid tenantId);
    Task<ChapterDto> UpdateChapterAsync(Guid chapterId, UpdateChapterRequest request, Guid userId, Guid tenantId);
    Task DeleteChapterAsync(Guid chapterId, Guid userId, Guid tenantId);
    Task<List<ChapterDto>> ReorderChaptersAsync(Guid videoId, List<Guid> chapterIds, Guid userId, Guid tenantId);
}
