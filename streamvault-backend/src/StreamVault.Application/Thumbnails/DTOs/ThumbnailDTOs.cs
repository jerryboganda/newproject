using System.ComponentModel.DataAnnotations;
using StreamVault.Domain.Entities;

namespace StreamVault.Application.Thumbnails.DTOs;

public class GenerateThumbnailsRequest
{
    [Range(1, 100)]
    public int Count { get; set; } = 10;

    public ThumbnailOptions Options { get; set; } = new();
}

public class ThumbnailOptions
{
    public int Width { get; set; } = 320;

    public int Height { get; set; } = 180;

    public string Format { get; set; } = "jpg";

    public int Quality { get; set; } = 80;
}

public class UploadThumbnailRequest
{
    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string ContentType { get; set; } = string.Empty;

    public byte[] Data { get; set; } = Array.Empty<byte>();

    public int PositionSeconds { get; set; }
}

public class SpriteSheetRequest
{
    [Range(1, 50)]
    public int Columns { get; set; } = 10;

    [Range(1, 100)]
    public int Count { get; set; } = 50;

    public ThumbnailOptions Options { get; set; } = new();
}

public class VideoThumbnailDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public int PositionSeconds { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public ThumbnailType Type { get; set; }
    public bool IsDefault { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ThumbnailGenerationJobDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
