using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class TwoFactorAuthCode
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required, MaxLength(6)]
    public string Code { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTimeOffset? UsedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
