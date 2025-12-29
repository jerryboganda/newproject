using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StreamVault.Infrastructure.Services
{
    /// <summary>
    /// File storage service interface
    /// </summary>
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
        Task<Stream> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default);
        Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
        Task<bool> FileExistsAsync(string fileUrl, CancellationToken cancellationToken = default);
        Task<string> GeneratePresignedUrlAsync(string fileUrl, TimeSpan expiry, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> ListFilesAsync(string prefix = "", CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Bunny.net storage implementation
    /// </summary>
    public class BunnyStorageService : IFileStorageService
    {
        private readonly ILogger<BunnyStorageService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _storageZoneName;
        private readonly string _accessKey;
        private readonly string _baseUrl;
        private readonly string _pullZoneUrl;

        public BunnyStorageService(IConfiguration configuration, ILogger<BunnyStorageService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = new HttpClient();
            
            _storageZoneName = configuration["BunnyCDN:StorageZoneName"] ?? throw new InvalidOperationException("BunnyCDN Storage Zone Name not configured");
            _accessKey = configuration["BunnyCDN:StorageAccessKey"] ?? throw new InvalidOperationException("BunnyCDN Storage Access Key not configured");
            _baseUrl = configuration["BunnyCDN:StorageBaseUrl"] ?? "https://storage.bunnycdn.com";
            _pullZoneUrl = configuration["BunnyCDN:PullZoneUrl"] ?? throw new InvalidOperationException("BunnyCDN Pull Zone URL not configured");
            
            _httpClient.DefaultRequestHeaders.Add("AccessKey", _accessKey);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Uploading file to Bunny.net: {FileName}", fileName);

                // Generate unique file name
                var uniqueFileName = $"{Guid.NewGuid()}/{fileName}";
                var url = $"{_baseUrl}/{_storageZoneName}/{uniqueFileName}";

                using var content = new StreamContent(fileStream);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                var response = await _httpClient.PutAsync(url, content, cancellationToken);
                response.EnsureSuccessStatusCode();

                var publicUrl = $"{_pullZoneUrl}/{uniqueFileName}";
                
                _logger.LogInformation("Successfully uploaded file: {PublicUrl}", publicUrl);
                return publicUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file to Bunny.net: {FileName}", fileName);
                throw new FileStorageException("File upload failed", ex);
            }
        }

        public async Task<Stream> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Downloading file from Bunny.net: {FileUrl}", fileUrl);

                var response = await _httpClient.GetAsync(fileUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStreamAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download file from Bunny.net: {FileUrl}", fileUrl);
                throw new FileStorageException("File download failed", ex);
            }
        }

        public async Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deleting file from Bunny.net: {FileUrl}", fileUrl);

                // Extract path from URL
                var uri = new Uri(fileUrl);
                var path = uri.AbsolutePath.Trim('/');
                var storageUrl = $"{_baseUrl}/{_storageZoneName}/{path}";

                var response = await _httpClient.DeleteAsync(storageUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully deleted file: {FileUrl}", fileUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file from Bunny.net: {FileUrl}", fileUrl);
                throw new FileStorageException("File deletion failed", ex);
            }
        }

        public async Task<bool> FileExistsAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.SendAsync(
                    new HttpRequestMessage(HttpMethod.Head, fileUrl),
                    cancellationToken);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check file existence: {FileUrl}", fileUrl);
                return false;
            }
        }

        public async Task<string> GeneratePresignedUrlAsync(string fileUrl, TimeSpan expiry, CancellationToken cancellationToken = default)
        {
            // Bunny.net doesn't support presigned URLs the same way as S3
            // Files are public by default when using a pull zone
            // For private files, you would need to implement token-based authentication
            return fileUrl;
        }

        public async Task<IEnumerable<string>> ListFilesAsync(string prefix = "", CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Listing files from Bunny.net with prefix: {Prefix}", prefix);

                var url = $"{_baseUrl}/{_storageZoneName}/";
                if (!string.IsNullOrEmpty(prefix))
                {
                    url += prefix;
                }

                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                
                // Parse the response to extract file names
                // Bunny.net returns a simple text list of file paths
                var files = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                
                return files.Select(f => $"{_pullZoneUrl}/{f.Trim('/')}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list files from Bunny.net");
                throw new FileStorageException("Failed to list files", ex);
            }
        }
    }

    /// <summary>
    /// AWS S3 storage implementation (alternative option)
    /// </summary>
    public class S3StorageService : IFileStorageService
    {
        private readonly ILogger<S3StorageService> _logger;
        private readonly IConfiguration _configuration;

        public S3StorageService(IConfiguration configuration, ILogger<S3StorageService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Uploading file to S3: {FileName}", fileName);

                // In production, you would use AWS SDK for .NET
                // This is a placeholder implementation
                var bucketName = _configuration["AWS:S3:BucketName"] ?? throw new InvalidOperationException("S3 bucket name not configured");
                var region = _configuration["AWS:S3:Region"] ?? "us-east-1";
                
                var uniqueFileName = $"{Guid.NewGuid()}/{fileName}";
                var fileUrl = $"https://{bucketName}.s3.{region}.amazonaws.com/{uniqueFileName}";

                // Simulate upload
                await Task.Delay(100, cancellationToken);

                _logger.LogInformation("Successfully uploaded file to S3: {FileUrl}", fileUrl);
                return fileUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file to S3: {FileName}", fileName);
                throw new FileStorageException("File upload failed", ex);
            }
        }

        public async Task<Stream> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Downloading file from S3: {FileUrl}", fileUrl);

                // In production, use AWS SDK to download from S3
                await Task.Delay(100, cancellationToken);
                
                // Return empty stream for now
                return new MemoryStream();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download file from S3: {FileUrl}", fileUrl);
                throw new FileStorageException("File download failed", ex);
            }
        }

        public async Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deleting file from S3: {FileUrl}", fileUrl);

                // In production, use AWS SDK to delete from S3
                await Task.Delay(100, cancellationToken);

                _logger.LogInformation("Successfully deleted file from S3: {FileUrl}", fileUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file from S3: {FileUrl}", fileUrl);
                throw new FileStorageException("File deletion failed", ex);
            }
        }

        public async Task<bool> FileExistsAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                // In production, use AWS SDK to check file existence
                await Task.Delay(100, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check file existence in S3: {FileUrl}", fileUrl);
                return false;
            }
        }

        public async Task<string> GeneratePresignedUrlAsync(string fileUrl, TimeSpan expiry, CancellationToken cancellationToken = default)
        {
            try
            {
                // In production, use AWS SDK to generate presigned URL
                await Task.Delay(100, cancellationToken);
                return fileUrl + "?presigned=true";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate presigned URL for S3 file: {FileUrl}", fileUrl);
                throw new FileStorageException("Failed to generate presigned URL", ex);
            }
        }

        public async Task<IEnumerable<string>> ListFilesAsync(string prefix = "", CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Listing files from S3 with prefix: {Prefix}", prefix);

                // In production, use AWS SDK to list objects
                await Task.Delay(100, cancellationToken);

                return new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list files from S3");
                throw new FileStorageException("Failed to list files", ex);
            }
        }
    }

    /// <summary>
    /// Local file storage implementation (for development)
    /// </summary>
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly ILogger<LocalFileStorageService> _logger;
        private readonly string _storagePath;
        private readonly string _baseUrl;

        public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _storagePath = configuration["FileStorage:LocalPath"] ?? Path.Combine(Path.GetTempPath(), "StreamVaultStorage");
            _baseUrl = configuration["FileStorage:BaseUrl"] ?? "http://localhost:5000/files";

            // Ensure directory exists
            Directory.CreateDirectory(_storagePath);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Uploading file locally: {FileName}", fileName);

                var uniqueFileName = $"{Guid.NewGuid()}/{fileName}";
                var fullPath = Path.Combine(_storagePath, uniqueFileName);
                var directory = Path.GetDirectoryName(fullPath);
                
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var fileStreamOutput = new FileStream(fullPath, FileMode.Create);
                await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);

                var publicUrl = $"{_baseUrl}/{uniqueFileName.Replace('\\', '/')}";
                
                _logger.LogInformation("Successfully uploaded file locally: {PublicUrl}", publicUrl);
                return publicUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file locally: {FileName}", fileName);
                throw new FileStorageException("File upload failed", ex);
            }
        }

        public async Task<Stream> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var relativePath = fileUrl.Replace(_baseUrl + "/", "").Replace("/", Path.DirectorySeparatorChar.ToString());
                var fullPath = Path.Combine(_storagePath, relativePath);

                if (!File.Exists(fullPath))
                    throw new FileNotFoundException("File not found", fullPath);

                return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download file locally: {FileUrl}", fileUrl);
                throw new FileStorageException("File download failed", ex);
            }
        }

        public async Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var relativePath = fileUrl.Replace(_baseUrl + "/", "").Replace("/", Path.DirectorySeparatorChar.ToString());
                var fullPath = Path.Combine(_storagePath, relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Successfully deleted local file: {FileUrl}", fileUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete local file: {FileUrl}", fileUrl);
                throw new FileStorageException("File deletion failed", ex);
            }
        }

        public async Task<bool> FileExistsAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var relativePath = fileUrl.Replace(_baseUrl + "/", "").Replace("/", Path.DirectorySeparatorChar.ToString());
                var fullPath = Path.Combine(_storagePath, relativePath);
                return File.Exists(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check file existence locally: {FileUrl}", fileUrl);
                return false;
            }
        }

        public async Task<string> GeneratePresignedUrlAsync(string fileUrl, TimeSpan expiry, CancellationToken cancellationToken = default)
        {
            // For local storage, files are already publicly accessible
            return fileUrl;
        }

        public async Task<IEnumerable<string>> ListFilesAsync(string prefix = "", CancellationToken cancellationToken = default)
        {
            try
            {
                var directory = Path.Combine(_storagePath, prefix.Replace("/", Path.DirectorySeparatorChar.ToString()));
                
                if (!Directory.Exists(directory))
                    return Enumerable.Empty<string>();

                var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
                
                return files.Select(f => 
                {
                    var relativePath = Path.GetRelativePath(_storagePath, f).Replace(Path.DirectorySeparatorChar, '/');
                    return $"{_baseUrl}/{relativePath}";
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list local files");
                throw new FileStorageException("Failed to list files", ex);
            }
        }
    }

    public class FileStorageException : Exception
    {
        public FileStorageException(string message) : base(message) { }
        public FileStorageException(string message, Exception innerException) : base(message, innerException) { }
    }
}
