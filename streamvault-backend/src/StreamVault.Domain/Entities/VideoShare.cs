using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class VideoShare
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VideoId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    public ShareType ShareType { get; set; } = ShareType.Public;

    public bool AllowDownload { get; set; } = false;

    public bool ShowComments { get; set; } = true;

    public DateTimeOffset? ExpiresAt { get; set; }

    public string? Password { get; set; }

    public int ViewCount { get; set; } = 0;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public Video Video { get; set; } = null!;
    public User User { get; set; } = null!;
}

public enum ShareType
{
    Public,
    Private,
    Unlisted
}
