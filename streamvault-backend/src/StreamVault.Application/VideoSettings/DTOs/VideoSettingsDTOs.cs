using System.ComponentModel.DataAnnotations;
using StreamVault.Domain.Entities;

namespace StreamVault.Application.VideoSettings.DTOs;

public class UpdateVideoSettingsRequest
{
    [Range(0.25, 2.0)]
    public double? PlaybackSpeed { get; set; }

    [Range(0, 100)]
    public int? Volume { get; set; }

    public bool? IsMuted { get; set; }

    public bool? Autoplay { get; set; }

    public VideoQuality? Quality { get; set; }

    public bool? CaptionsEnabled { get; set; }

    public string? CaptionsLanguage { get; set; }

    public bool? PictureInPicture { get; set; }

    public bool? TheaterMode { get; set; }

    public bool? Fullscreen { get; set; }
}

public class VideoSettingsDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid VideoId { get; set; }
    public double PlaybackSpeed { get; set; }
    public int Volume { get; set; }
    public bool IsMuted { get; set; }
    public bool Autoplay { get; set; }
    public VideoQuality Quality { get; set; }
    public bool CaptionsEnabled { get; set; }
    public string? CaptionsLanguage { get; set; }
    public bool PictureInPicture { get; set; }
    public bool TheaterMode { get; set; }
    public bool Fullscreen { get; set; }
    public double LastPositionSeconds { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class PlaybackSpeedRequest
{
    [Required]
    [Range(0.25, 2.0)]
    public double Speed { get; set; }
}

public class VolumeRequest
{
    [Required]
    [Range(0, 100)]
    public int Volume { get; set; }
}

public class PositionRequest
{
    [Required]
    [Range(0, double.MaxValue)]
    public double PositionSeconds { get; set; }
}
