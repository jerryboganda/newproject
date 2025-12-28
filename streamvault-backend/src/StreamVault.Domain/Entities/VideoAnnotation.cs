using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class VideoAnnotation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VideoId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required, MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public int StartTimeSeconds { get; set; }

    public int EndTimeSeconds { get; set; }

    public AnnotationType Type { get; set; } = AnnotationType.Note;

    public string? Color { get; set; } = "#FFD700";

    public double PositionX { get; set; } = 0.5; // 0 to 1 (percentage of video width)

    public double PositionY { get; set; } = 0.5; // 0 to 1 (percentage of video height)

    public bool IsPublic { get; set; } = false;

    public bool IsResolved { get; set; } = false;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public Video Video { get; set; } = null!;
    public User User { get; set; } = null!;
    public List<AnnotationReply> Replies { get; set; } = new();
}

public class AnnotationReply
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid AnnotationId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required, MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public VideoAnnotation Annotation { get; set; } = null!;
    public User User { get; set; } = null!;
}

public enum AnnotationType
{
    Note,
    Question,
    Correction,
    Highlight,
    Bookmark,
    Comment
}
