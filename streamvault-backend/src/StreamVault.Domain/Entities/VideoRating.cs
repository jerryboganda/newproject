using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class VideoRating
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VideoId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public Video Video { get; set; } = null!;
    public User User { get; set; } = null!;
}
