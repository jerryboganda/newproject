using System.ComponentModel.DataAnnotations;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Domain.Entities;

namespace StreamVault.Application.Videos.DTOs;

public class VideoUploadRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public bool IsPublic { get; set; } = false;
    
    public List<string>? Tags { get; set; }
}

public class VideoUploadResponse
{
    public string UploadUrl { get; set; } = string.Empty;
    public string VideoId { get; set; } = string.Empty;
    public string UploadToken { get; set; } = string.Empty;
}

public class VideoDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? MimeType { get; set; }
    public int DurationSeconds { get; set; }
    public VideoStatus Status { get; set; }
    public bool IsPublic { get; set; }
    public int ViewCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public UserDto User { get; set; } = null!;
    public List<string> Tags { get; set; } = new List<string>();
}

public class VideoListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public int ViewCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public UserDto User { get; set; } = null!;
    public List<string> Tags { get; set; } = new List<string>();
    public string? Category { get; set; }
    public bool IsPublic { get; set; }
}

public class VideoListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public string? Tag { get; set; }
    public bool? IsPublic { get; set; }
    public Guid? UserId { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortOrder { get; set; } = "desc";
    public int? MinDuration { get; set; }
    public int? MaxDuration { get; set; }
    public DateTimeOffset? UploadedAfter { get; set; }
    public DateTimeOffset? UploadedBefore { get; set; }
    public string[]? Tags { get; set; }
}

public class VideoListResponse
{
    public List<VideoDto> Videos { get; set; } = new List<VideoDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class VideoUpdateRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool? IsPublic { get; set; }
    public List<string>? Tags { get; set; }
}

public class ChunkUploadRequest
{
    public string VideoId { get; set; } = string.Empty;
    public string UploadToken { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public int TotalChunks { get; set; }
    public string ChunkHash { get; set; } = string.Empty;
}

public class ChunkUploadResponse
{
    public string UploadUrl { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
}
