namespace StreamVault.Application.Services;

public record BunnyVideoUploadResult(
    string VideoId,
    string LibraryId,
    string? CdnHostname,
    string? Mp4Url,
    string? ThumbnailUrl,
    string Status);

public interface IBunnyNetService
{
    bool IsConfiguredForCurrentTenant();

    Task<BunnyVideoUploadResult> UploadVideoToStreamAsync(
        Stream videoStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    string? GetStreamMp4Url(string bunnyVideoId);
    string? GetStreamThumbnailUrl(string bunnyVideoId);
}
