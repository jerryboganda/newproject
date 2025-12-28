namespace StreamVault.Application.Services;

public interface IStorageService
{
    Task<string> GenerateUploadUrlAsync(string key, string contentType, long contentLength);
    Task<string> GeneratePresignedUrlAsync(string key, TimeSpan expiry);
    Task DeleteFileAsync(string key);
    Task<bool> FileExistsAsync(string key);
    Task<long> GetFileSizeAsync(string key);
    Task<string> CopyFileAsync(string sourceKey, string destinationKey);
    Task UploadFileAsync(string key, byte[] data, string contentType);
    Task<string> GetPresignedUrlAsync(string key, TimeSpan expiry, bool isDownload = false);
}
