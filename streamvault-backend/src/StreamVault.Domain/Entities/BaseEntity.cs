namespace StreamVault.Domain.Entities;

/// <summary>
/// Base entity for all database entities
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Base entity for tenant-specific data
/// </summary>
public abstract class TenantEntity : BaseEntity
{
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}

/// <summary>
/// Base entity for system-wide data (no tenant association)
/// </summary>
public abstract class SystemEntity : BaseEntity
{
}
