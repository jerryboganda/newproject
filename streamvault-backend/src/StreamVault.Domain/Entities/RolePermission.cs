using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StreamVault.Domain.Entities;

public class RolePermission
{
    [Key]
    public Guid RoleId { get; set; }

    [Key]
    public Guid PermissionId { get; set; }

    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.Now;

    // Navigation
    public Role Role { get; set; } = null!;

    public Permission Permission { get; set; } = null!;
}
