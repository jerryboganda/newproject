using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StreamVault.Domain.Entities;

public class UserRole
{
    [Key]
    public Guid UserId { get; set; }

    [Key]
    public Guid RoleId { get; set; }

    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.Now;

    // Navigation
    public User User { get; set; } = null!;

    public Role Role { get; set; } = null!;
}
