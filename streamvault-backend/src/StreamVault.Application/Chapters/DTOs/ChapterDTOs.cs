using System.ComponentModel.DataAnnotations;

namespace StreamVault.Application.Chapters.DTOs;

public class CreateChapterRequest
{
    [Required]
    public Guid VideoId { get; set; }

    [Required]
    public int StartTimeSeconds { get; set; }

    [Required]
    public int EndTimeSeconds { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? Thumbnail { get; set; }

    public int SortOrder { get; set; } = 0;
}

public class UpdateChapterRequest
{
    public int? StartTimeSeconds { get; set; }

    public int? EndTimeSeconds { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Thumbnail { get; set; }

    public int? SortOrder { get; set; }
}

public class ChapterDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public int StartTimeSeconds { get; set; }
    public int EndTimeSeconds { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
