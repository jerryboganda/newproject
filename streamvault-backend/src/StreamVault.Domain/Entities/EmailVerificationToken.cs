using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class EmailVerificationToken
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required, MaxLength(255)]
    public string Token { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTimeOffset? UsedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
