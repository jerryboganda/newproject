using System.ComponentModel.DataAnnotations;

namespace StreamVault.Domain.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsEmailVerified { get; set; } = false;

    public DateTimeOffset? EmailVerifiedAt { get; set; }

    public bool TwoFactorEnabled { get; set; } = false;

    public string? RefreshToken { get; set; }

    public DateTimeOffset? RefreshTokenExpiry { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Foreign key
    public Guid TenantId { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = new List<EmailVerificationToken>();

    public ICollection<TwoFactorAuthCode> TwoFactorAuthCodes { get; set; } = new List<TwoFactorAuthCode>();
}
