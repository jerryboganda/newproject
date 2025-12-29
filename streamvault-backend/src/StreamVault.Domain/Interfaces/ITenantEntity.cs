namespace StreamVault.Domain.Interfaces;

/// <summary>
/// Interface for tenant-aware entities
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// Tenant identifier
    /// </summary>
    Guid TenantId { get; set; }
}
