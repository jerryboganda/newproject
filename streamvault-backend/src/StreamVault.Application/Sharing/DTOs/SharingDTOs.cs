using System.ComponentModel.DataAnnotations;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Domain.Entities;

namespace StreamVault.Application.Sharing.DTOs;

public class CreateShareLinkRequest
{
    [Required]
    public Guid VideoId { get; set; }

    public ShareType ShareType { get; set; } = ShareType.Public;

    public bool AllowDownload { get; set; } = false;

    public bool ShowComments { get; set; } = true;

    public DateTimeOffset? ExpiresAt { get; set; }

    public string? Password { get; set; }
}

public class UpdateShareLinkRequest
{
    public ShareType? ShareType { get; set; }

    public bool? AllowDownload { get; set; }

    public bool? ShowComments { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public string? Password { get; set; }
}

public class ShareLinkDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public string VideoTitle { get; set; } = string.Empty;
    public string ShareUrl { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public ShareType ShareType { get; set; }
    public bool AllowDownload { get; set; }
    public bool ShowComments { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTimeOffset.UtcNow;
    public bool HasPassword => !string.IsNullOrEmpty(Password);
    public string? Password { get; set; }
    public int ViewCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public UserDto CreatedBy { get; set; } = null!;
}

public class VideoDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int DurationSeconds { get; set; }
    public int ViewCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public UserDto User { get; set; } = null!;
}
