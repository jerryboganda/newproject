using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StreamVault.Infrastructure.Services
{
    /// <summary>
    /// Bunny.net API client for video storage and CDN services
    /// </summary>
    public class BunnyCDNService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BunnyCDNService> _logger;
        private readonly string _apiKey;
        private readonly string _libraryId;
        private readonly string _pullZoneId;
        private readonly string _cdnHostname;

        public BunnyCDNService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<BunnyCDNService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _apiKey = configuration["BunnyCDN:ApiKey"] ?? throw new InvalidOperationException("BunnyCDN API key not configured");
            _libraryId = configuration["BunnyCDN:LibraryId"] ?? throw new InvalidOperationException("BunnyCDN Library ID not configured");
            _pullZoneId = configuration["BunnyCDN:PullZoneId"] ?? throw new InvalidOperationException("BunnyCDN Pull Zone ID not configured");
            _cdnHostname = configuration["BunnyCDN:Hostname"] ?? "cdn.bunny.net";

            _httpClient.BaseAddress = new Uri("https://api.bunny.net");
            _httpClient.DefaultRequestHeaders.Add("AccessKey", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Upload a video file to Bunny.net storage
        /// </summary>
        public async Task<BunnyVideoUploadResult> UploadVideoAsync(
            Stream videoStream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting video upload to Bunny.net: {FileName}", fileName);

                // Create video object
                var createVideoRequest = new
                {
                    title = Path.GetFileNameWithoutExtension(fileName),
                    collectionId = (string?)null
                };

                var createResponse = await _httpClient.PostAsJsonAsync(
                    $"/library/{_libraryId}/videos",
                    createVideoRequest,
                    cancellationToken);

                createResponse.EnsureSuccessStatusCode();
                
                var videoResponse = await createResponse.Content.ReadFromJsonAsync<BunnyVideoResponse>(cancellationToken: cancellationToken);
                
                if (videoResponse == null)
                    throw new InvalidOperationException("Failed to create video object");

                // Upload the file
                using var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(videoStream);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                content.Add(fileContent, "file", fileName);

                var uploadResponse = await _httpClient.PutAsync(
                    $"https://video.bunny.net/upload/{videoResponse.Guid}",
                    content,
                    cancellationToken);

                uploadResponse.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully uploaded video: {VideoId}", videoResponse.Guid);

                return new BunnyVideoUploadResult
                {
                    VideoId = videoResponse.Guid,
                    LibraryId = _libraryId,
                    VideoUrl = $"https://{_cdnHostname}/{videoResponse.Guid}/play.mp4",
                    ThumbnailUrl = $"https://{_cdnHostname}/{videoResponse.Guid}/thumbnail.jpg",
                    Status = "uploaded"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload video to Bunny.net: {FileName}", fileName);
                throw new BunnyCDNException("Video upload failed", ex);
            }
        }

        /// <summary>
        /// Get video details from Bunny.net
        /// </summary>
        public async Task<BunnyVideoDetails?> GetVideoAsync(string videoId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"/library/{_libraryId}/videos/{videoId}",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<BunnyVideoDetails>(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get video details: {VideoId}", videoId);
                throw new BunnyCDNException("Failed to retrieve video details", ex);
            }
        }

        /// <summary>
        /// Delete a video from Bunny.net
        /// </summary>
        public async Task DeleteVideoAsync(string videoId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deleting video from Bunny.net: {VideoId}", videoId);

                var response = await _httpClient.DeleteAsync(
                    $"/library/{_libraryId}/videos/{videoId}",
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully deleted video: {VideoId}", videoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete video: {VideoId}", videoId);
                throw new BunnyCDNException("Video deletion failed", ex);
            }
        }

        /// <summary>
        /// Get video processing status
        /// </summary>
        public async Task<BunnyVideoStatus> GetVideoStatusAsync(string videoId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"/library/{_libraryId}/videos/{videoId}",
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var video = await response.Content.ReadFromJsonAsync<BunnyVideoDetails>(cancellationToken: cancellationToken);
                
                return video?.Status switch
                {
                    "uploaded" => BunnyVideoStatus.Uploaded,
                    "processing" => BunnyVideoStatus.Processing,
                    "processed" => BunnyVideoStatus.Processed,
                    "failed" => BunnyVideoStatus.Failed,
                    _ => BunnyVideoStatus.Unknown
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get video status: {VideoId}", videoId);
                throw new BunnyCDNException("Failed to retrieve video status", ex);
            }
        }

        /// <summary>
        /// Purge CDN cache for a specific file
        /// </summary>
        public async Task PurgeCacheAsync(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                var purgeRequest = new { url };
                
                var response = await _httpClient.PostAsJsonAsync(
                    $"/pullzone/{_pullZoneId}/purgeCache",
                    purgeRequest,
                    cancellationToken);

                response.EnsureSuccessStatusCode();
                
                _logger.LogInformation("Successfully purged cache for: {Url}", url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to purge cache: {Url}", url);
                throw new BunnyCDNException("Cache purge failed", ex);
            }
        }

        /// <summary>
        /// Get storage statistics
        /// </summary>
        public async Task<BunnyStorageStats> GetStorageStatsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"/library/{_libraryId}/statistics",
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<BunnyStorageStats>(cancellationToken: cancellationToken)
                    ?? new BunnyStorageStats();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get storage statistics");
                throw new BunnyCDNException("Failed to retrieve storage statistics", ex);
            }
        }
    }

    // Data Transfer Objects
    public class BunnyVideoUploadResult
    {
        public string VideoId { get; set; } = string.Empty;
        public string LibraryId { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class BunnyVideoResponse
    {
        public Guid Guid { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime DateUploaded { get; set; }
        public long StorageSize { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? PreviewUrl { get; set; }
    }

    public class BunnyVideoDetails
    {
        public Guid Guid { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DateUploaded { get; set; }
        public long StorageSize { get; set; }
        public int Length { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? PreviewUrl { get; set; }
        public string? VideoUrl { get; set; }
    }

    public class BunnyStorageStats
    {
        public long TotalStorageUsed { get; set; }
        public long TotalBandwidthUsed { get; set; }
        public int TotalVideos { get; set; }
    }

    public enum BunnyVideoStatus
    {
        Unknown,
        Uploaded,
        Processing,
        Processed,
        Failed
    }

    public class BunnyCDNException : Exception
    {
        public BunnyCDNException(string message) : base(message) { }
        public BunnyCDNException(string message, Exception innerException) : base(message, innerException) { }
    }
}
