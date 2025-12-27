using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StreamVault.Domain.Entities;

public class TenantBranding
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, ForeignKey("Tenant")]
    public Guid TenantId { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [MaxLength(500)]
    public string? FaviconUrl { get; set; }

    [MaxLength(7)] // #RRGGBB
    public string? PrimaryColor { get; set; }

    [MaxLength(7)]
    public string? SecondaryColor { get; set; }

    [MaxLength(7)]
    public string? AccentColor { get; set; }

    [MaxLength(500)]
    public string? PlayerLogoUrl { get; set; }

    public PlayerLogoPosition? PlayerLogoPosition { get; set; }

    public string? CustomCss { get; set; }

    public string? EmailFooterText { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
}

public enum PlayerLogoPosition
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}
