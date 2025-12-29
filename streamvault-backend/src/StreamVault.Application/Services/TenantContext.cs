using System;
using System.Threading;
using StreamVault.Application.Interfaces;
using StreamVault.Domain.Entities;

namespace StreamVault.Application.Services
{
    /// <summary>
    /// Implementation of ITenantContext for managing tenant context
    /// </summary>
    public class TenantContext : ITenantContext
    {
        private static readonly AsyncLocal<TenantHolder> _currentTenant = new();

        public Tenant? CurrentTenant => _currentTenant.Value?.Tenant;

        public Guid? TenantId => _currentTenant.Value?.Tenant?.Id;

        public string? TenantSlug => _currentTenant.Value?.Tenant?.Slug;

        public bool HasCurrentTenant => _currentTenant.Value?.Tenant != null;

        public void SetCurrentTenant(Tenant tenant)
        {
            if (tenant == null)
                throw new ArgumentNullException(nameof(tenant));

            _currentTenant.Value = new TenantHolder { Tenant = tenant };
        }

        public void ClearCurrentTenant()
        {
            _currentTenant.Value = null;
        }

        /// <summary>
        /// Private class to hold tenant in AsyncLocal
        /// </summary>
        private class TenantHolder
        {
            public Tenant? Tenant { get; set; }
        }
    }
}
