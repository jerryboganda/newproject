using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using StreamVault.Domain.Entities;

namespace StreamVault.Application.Interfaces
{
    /// <summary>
    /// Interface for resolving tenants from HTTP requests
    /// </summary>
    public interface ITenantResolver
    {
        /// <summary>
        /// Resolves the tenant from the HTTP request
        /// </summary>
        /// <param name="request">The HTTP request</param>
        /// <returns>The resolved tenant or null if not found</returns>
        Task<Tenant?> ResolveTenantAsync(HttpRequest request);
    }
}
