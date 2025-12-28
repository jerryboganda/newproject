using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StreamVault.Domain.Entities;

public class User
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
    public Tenant Tenant { get; set; } = null!;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = new List<EmailVerificationToken>();

    public ICollection<TwoFactorAuthCode> TwoFactorAuthCodes { get; set; } = new List<TwoFactorAuthCode>();
}

public enum UserStatus
{
    Active,
    Inactive,
    Suspended
}
