using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class Playlist
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    public bool IsPublic { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public ICollection<PlaylistVideo> PlaylistVideos { get; set; } = new List<PlaylistVideo>();
}
