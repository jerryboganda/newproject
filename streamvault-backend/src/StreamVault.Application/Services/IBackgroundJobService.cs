namespace StreamVault.Application.Services;

public interface IBackgroundJobService
{
    Task EnqueueVideoProcessingAsync(Guid videoId, string jobType, Dictionary<string, object>? metadata = null);
    Task EnqueueThumbnailGenerationAsync(Guid videoId);
    Task EnqueueVideoTranscodingAsync(Guid videoId, string outputFormat = "mp4");
    Task EnqueueVideoAnalysisAsync(Guid videoId);
}
