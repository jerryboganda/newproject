using StreamVault.Application.AI.DTOs;

namespace StreamVault.Application.AI;

public interface IVideoTaggingService
{
    Task<List<GeneratedTagDto>> GenerateTagsAsync(Guid videoId, Guid tenantId);
    Task<List<GeneratedTagDto>> GenerateTagsFromTranscriptAsync(string transcript);
    Task<List<GeneratedTagDto>> GenerateTagsFromThumbnailAsync(Guid videoId, Guid tenantId);
    Task<List<GeneratedTagDto>> GenerateTagsFromAudioAsync(Guid videoId, Guid tenantId);
    Task<List<GeneratedTagDto>> GenerateTagsFromMetadataAsync(VideoMetadataDto metadata);
    Task<List<GeneratedCategoryDto>> SuggestCategoriesAsync(Guid videoId, Guid tenantId);
    Task<VideoContentAnalysisDto> AnalyzeVideoContentAsync(Guid videoId, Guid tenantId);
    Task<bool> ApplyGeneratedTagsAsync(Guid videoId, List<Guid> tagIds, Guid tenantId);
    Task<List<VideoInsightDto>> GenerateVideoInsightsAsync(Guid videoId, Guid tenantId);
    Task<List<TrendingTagDto>> GetTrendingTagsAsync(Guid tenantId, int limit = 50);
    Task<List<TagSuggestionDto>> GetTagSuggestionsAsync(Guid videoId, Guid tenantId, string? query = null);
}
