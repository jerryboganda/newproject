using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StreamVault.Domain.Interfaces;

namespace StreamVault.Domain.Entities;

public class User : ITenantEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? TenantId { get; set; } // Null for super admins

    [Required, MaxLength(255), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTimeOffset? LastLoginAt { get; set; }

    public bool TwoFactorEnabled { get; set; } = false;

    public bool IsEmailVerified { get; set; } = false;

    public DateTimeOffset? EmailVerifiedAt { get; set; }

    public string? RefreshToken { get; set; }

    public DateTimeOffset? RefreshTokenExpiry { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public string? StripeCustomerId { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    // Navigation
    public virtual Tenant? Tenant { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = new List<EmailVerificationToken>();

    public virtual ICollection<TwoFactorAuthCode> TwoFactorAuthCodes { get; set; } = new List<TwoFactorAuthCode>();
}

public enum UserStatus
{
    Active,
    Inactive,
    Suspended
}
