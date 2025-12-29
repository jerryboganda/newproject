using System;
using StreamVault.Domain.Entities;

namespace StreamVault.Application.Interfaces
{
    /// <summary>
    /// Interface for managing tenant context in the application
    /// </summary>
    public interface ITenantContext
    {
        /// <summary>
        /// Gets the current tenant
        /// </summary>
        Tenant? CurrentTenant { get; }

        /// <summary>
        /// Gets the current tenant ID
        /// </summary>
        Guid? TenantId { get; }

        /// <summary>
        /// Gets the current tenant slug
        /// </summary>
        string? TenantSlug { get; }

        /// <summary>
        /// Checks if a tenant is currently set
        /// </summary>
        bool HasCurrentTenant { get; }

        /// <summary>
        /// Sets the current tenant
        /// </summary>
        /// <param name="tenant">The tenant to set</param>
        void SetCurrentTenant(Tenant tenant);

        /// <summary>
        /// Clears the current tenant
        /// </summary>
        void ClearCurrentTenant();
    }
}
