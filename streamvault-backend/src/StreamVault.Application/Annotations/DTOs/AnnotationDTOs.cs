using System.ComponentModel.DataAnnotations;
using StreamVault.Domain.Entities;
using StreamVault.Application.Auth.DTOs;

namespace StreamVault.Application.Annotations.DTOs;

public class CreateAnnotationRequest
{
    [Required]
    public Guid VideoId { get; set; }

    [Required, MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public int StartTimeSeconds { get; set; }

    public int EndTimeSeconds { get; set; }

    public AnnotationType Type { get; set; } = AnnotationType.Note;

    public string? Color { get; set; } = "#FFD700";

    public double PositionX { get; set; } = 0.5;

    public double PositionY { get; set; } = 0.5;

    public bool IsPublic { get; set; } = false;
}

public class UpdateAnnotationRequest
{
    public string? Title { get; set; }

    public string? Content { get; set; }

    public int? StartTimeSeconds { get; set; }

    public int? EndTimeSeconds { get; set; }

    public AnnotationType? Type { get; set; }

    public string? Color { get; set; }

    public double? PositionX { get; set; }

    public double? PositionY { get; set; }

    public bool? IsPublic { get; set; }
}

public class AnnotationDto
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int StartTimeSeconds { get; set; }
    public int EndTimeSeconds { get; set; }
    public AnnotationType Type { get; set; }
    public string? Color { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public bool IsPublic { get; set; }
    public bool IsResolved { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public UserDto User { get; set; } = null!;
    public List<AnnotationReplyDto> Replies { get; set; } = new();
}

public class CreateReplyRequest
{
    [Required, MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
}

public class AnnotationReplyDto
{
    public Guid Id { get; set; }
    public Guid AnnotationId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public UserDto User { get; set; } = null!;
}
