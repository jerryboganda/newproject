using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StreamVault.Domain.Interfaces;

namespace StreamVault.Domain.Entities;

public class Role : ITenantEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? TenantId { get; set; } // Null for system roles

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string NormalizedName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSystemRole { get; set; } = false; // E.g., Admin, Editor, Viewer

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public virtual Tenant? Tenant { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
