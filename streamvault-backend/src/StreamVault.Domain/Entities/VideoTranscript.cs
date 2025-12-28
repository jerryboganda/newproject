using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class VideoTranscript
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VideoId { get; set; }

    [Required]
    public int StartTimeSeconds { get; set; }

    [Required]
    public int EndTimeSeconds { get; set; }

    [Required]
    public string Text { get; set; } = string.Empty;

    public float Confidence { get; set; } = 0.0f;

    public string? Language { get; set; }

    public string? Speaker { get; set; }

    public int SortOrder { get; set; } = 0;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public Video Video { get; set; } = null!;
}
