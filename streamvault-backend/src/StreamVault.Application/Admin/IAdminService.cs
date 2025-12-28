using StreamVault.Application.Admin.DTOs;

namespace StreamVault.Application.Admin;

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardAsync(Guid tenantId);
    Task<List<UserDto>> GetUsersAsync(Guid tenantId, int page = 1, int pageSize = 20);
    Task<UserDto?> GetUserAsync(Guid userId, Guid tenantId);
    Task UpdateUserStatusAsync(Guid userId, UserStatusUpdateRequest request, Guid tenantId);
    Task<List<TenantDto>> GetTenantsAsync(int page = 1, int pageSize = 20);
    Task<TenantDto?> GetTenantAsync(Guid tenantId);
    Task UpdateTenantStatusAsync(Guid tenantId, TenantStatusUpdateRequest request);
    Task<SystemStatsDto> GetSystemStatsAsync();
}
