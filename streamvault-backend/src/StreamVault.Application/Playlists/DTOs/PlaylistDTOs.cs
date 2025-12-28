using System.ComponentModel.DataAnnotations;
using StreamVault.Application.Auth.DTOs;

namespace StreamVault.Application.Playlists.DTOs;

public class CreatePlaylistRequest
{
    [Required, MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsPublic { get; set; } = false;
}

public class UpdatePlaylistRequest
{
    [MaxLength(255)]
    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool? IsPublic { get; set; }
}

public class PlaylistDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int VideoCount { get; set; }
    public UserDto User { get; set; } = null!;
    public List<PlaylistVideoDto> Videos { get; set; } = new List<PlaylistVideoDto>();
}

public class PlaylistVideoDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public int Position { get; set; }
    public DateTimeOffset AddedAt { get; set; }
}

public class PlaylistVideoOrder
{
    public Guid VideoId { get; set; }
    public int Position { get; set; }
}
