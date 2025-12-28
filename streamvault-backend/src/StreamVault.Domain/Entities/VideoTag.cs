using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class VideoTag
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VideoId { get; set; }

    [Required, MaxLength(100)]
    public string Tag { get; set; } = string.Empty;

    // Navigation properties
    public Video Video { get; set; } = null!;
}
