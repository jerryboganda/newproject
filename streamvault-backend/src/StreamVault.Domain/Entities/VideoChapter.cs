using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class VideoChapter
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

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

    public string? ThumbnailPath { get; set; }

    public int SortOrder { get; set; } = 0;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public Video Video { get; set; } = null!;
}
