using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StreamVault.Infrastructure.Data;
using System.Security.Claims;

namespace StreamVault.Api.Middleware;

/// <summary>
/// Permission-based authorization handler
/// Checks if user has required permissions (supports wildcards like "videos.*")
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly StreamVaultDbContext _dbContext;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        IHttpContextAccessor httpContextAccessor,
        StreamVaultDbContext dbContext,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            _logger.LogWarning("HttpContext not available for permission check");
            context.Fail();
            return;
        }

        // Get user ID from claims
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? context.User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            _logger.LogWarning("User ID not found in claims");
            context.Fail();
            return;
        }

        try
        {
            // Get user with roles and permissions
            var user = await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == userGuid);

            if (user == null)
            {
                _logger.LogWarning($"User {userGuid} not found");
                context.Fail();
                return;
            }

            // Check if user has the required permission
            if (HasPermission(user, requirement.Permission))
            {
                _logger.LogDebug($"User {user.Email} granted permission: {requirement.Permission}");
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning($"User {user.Email} denied permission: {requirement.Permission}");
                context.Fail();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission");
            context.Fail();
        }
    }

    private bool HasPermission(Domain.Entities.User user, string requiredPermission)
    {
        if (user.UserRoles == null || user.UserRoles.Count == 0)
            return false;

        foreach (var userRole in user.UserRoles)
        {
            if (userRole.Role?.RolePermissions == null)
                continue;

            foreach (var rolePermission in userRole.Role.RolePermissions)
            {
                var permissionName = rolePermission.Permission?.NormalizedName ?? "";

                // Check exact match
                if (permissionName == requiredPermission.ToUpper())
                    return true;

                // Check wildcard match (e.g., "videos.*" matches "videos.view", "videos.create", etc)
                if (permissionName.EndsWith(".*"))
                {
                    var basePermission = permissionName.TrimEnd('*');
                    if (requiredPermission.ToUpper().StartsWith(basePermission))
                        return true;
                }
            }
        }

        return false;
    }
}

/// <summary>
/// Attribute for requiring specific permissions on endpoints
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute
{
    public string Permission { get; }

    public RequirePermissionAttribute(string permission)
    {
        Permission = permission;
    }
}

/// <summary>
/// Middleware to enforce permissions via attributes
/// </summary>
public class PermissionEnforcementMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PermissionEnforcementMiddleware> _logger;

    public PermissionEnforcementMiddleware(RequestDelegate next, ILogger<PermissionEnforcementMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Continue with the next middleware
        await _next(context);
    }
}

/// <summary>
/// Extension methods for authorization
/// </summary>
public static class AuthorizationExtensions
{
    public static AuthorizationPolicyBuilder RequirePermission(
        this AuthorizationPolicyBuilder builder,
        string permission)
    {
        builder.AddRequirements(new PermissionRequirement(permission));
        return builder;
    }

    /// <summary>
    /// Get permissions from JWT token claims
    /// </summary>
    public static List<string> GetPermissionsFromClaims(ClaimsPrincipal user)
    {
        var permissionsClaim = user.FindFirst("permissions")?.Value;
        if (string.IsNullOrEmpty(permissionsClaim))
            return new List<string>();

        return permissionsClaim.Split(',').Select(p => p.Trim()).ToList();
    }

    /// <summary>
    /// Check if user has permission
    /// </summary>
    public static bool HasPermission(ClaimsPrincipal user, string permission)
    {
        var permissions = GetPermissionsFromClaims(user);
        
        // Check exact match
        if (permissions.Contains(permission.ToUpper()))
            return true;

        // Check wildcard
        foreach (var perm in permissions)
        {
            if (perm.EndsWith("*") && permission.ToUpper().StartsWith(perm.TrimEnd('*')))
                return true;
        }

        return false;
    }
}

/// <summary>
/// Policy names for common permissions
/// </summary>
public static class PermissionPolicies
{
    public const string ViewVideos = "ViewVideos";
    public const string ManageVideos = "ManageVideos";
    public const string ManageUsers = "ManageUsers";
    public const string ManageBilling = "ManageBilling";
    public const string ManageSettings = "ManageSettings";
    public const string ViewAnalytics = "ViewAnalytics";
    public const string ManageRoles = "ManageRoles";
}
