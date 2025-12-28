using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class PlaylistVideo
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid PlaylistId { get; set; }

    [Required]
    public Guid VideoId { get; set; }

    public int Position { get; set; }

    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public Playlist Playlist { get; set; } = null!;
    public Video Video { get; set; } = null!;
}
