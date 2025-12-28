using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class VideoSettings
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid VideoId { get; set; }

    public double PlaybackSpeed { get; set; } = 1.0; // 0.25 to 2.0

    public int Volume { get; set; } = 100; // 0 to 100

    public bool IsMuted { get; set; } = false;

    public bool Autoplay { get; set; } = true;

    public VideoQuality Quality { get; set; } = VideoQuality.Auto;

    public bool CaptionsEnabled { get; set; } = false;

    public string? CaptionsLanguage { get; set; }

    public bool PictureInPicture { get; set; } = false;

    public bool TheaterMode { get; set; } = false;

    public bool Fullscreen { get; set; } = false;

    public double LastPositionSeconds { get; set; } = 0;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Video Video { get; set; } = null!;
}

public enum VideoQuality
{
    Auto,
    _2160p, // 4K
    _1440p, // 2K
    _1080p, // Full HD
    _720p, // HD
    _480p, // SD
    _360p, // SD
    _240p, // SD
    _144p  // SD
}
