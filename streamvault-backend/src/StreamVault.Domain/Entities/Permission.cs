using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StreamVault.Domain.Entities;

public class Permission
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string NormalizedName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSystemPermission { get; set; } = false;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    // Navigation
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
