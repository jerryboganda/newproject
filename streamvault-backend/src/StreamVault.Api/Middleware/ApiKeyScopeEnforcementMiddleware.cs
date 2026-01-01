using Microsoft.AspNetCore.Http;

namespace StreamVault.Api.Middleware;

public sealed class ApiKeyScopeEnforcementMiddleware
{
    private readonly RequestDelegate _next;

    public ApiKeyScopeEnforcementMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var principal = context.User;
        var apiKeyId = principal?.FindFirst("api_key_id")?.Value;
        if (string.IsNullOrWhiteSpace(apiKeyId))
        {
            await _next(context);
            return;
        }

        var requiredScope = ResolveRequiredScope(context.Request.Path, context.Request.Method);
        if (requiredScope == null)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "API key not permitted for this endpoint" });
            return;
        }

        var scopes = principal?.FindAll("api_scope").Select(c => c.Value).ToArray() ?? Array.Empty<string>();
        if (!HasScope(scopes, requiredScope))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "API key scope missing", requiredScope });
            return;
        }

        await _next(context);
    }

    private static string? ResolveRequiredScope(PathString path, string method)
    {
        var p = path.Value?.ToLowerInvariant() ?? string.Empty;
        var m = (method ?? string.Empty).ToUpperInvariant();

        // Block management endpoints by default unless explicitly scoped.
        if (p.StartsWith("/api/v1/api-keys"))
            return "api_keys.manage";

        if (p.StartsWith("/api/v1/webhooks/subscriptions"))
            return "webhooks.manage";

        if (p.StartsWith("/api/v1/uploads/tus"))
            return "videos.write";

        if (p.StartsWith("/api/v1/videos"))
        {
            if (m == "GET") return "videos.read";
            return "videos.write";
        }

        if (p.StartsWith("/api/v1/collections"))
        {
            if (m == "GET") return "collections.read";
            return "collections.write";
        }

        if (p.StartsWith("/api/v1/analytics"))
            return "analytics.read";

        if (p.StartsWith("/api/v1/billing"))
        {
            if (m == "GET") return "billing.read";
            return "billing.write";
        }

        if (p.StartsWith("/api/v1/support"))
        {
            if (m == "GET") return "support.read";
            return "support.write";
        }

        if (p.StartsWith("/api/v1/kb"))
        {
            if (m == "GET") return "kb.read";
            return "kb.write";
        }

        // Unknown endpoints are denied for API keys by default.
        return null;
    }

    private static bool HasScope(IEnumerable<string> scopes, string requiredScope)
    {
        var required = NormalizeScope(requiredScope);

        foreach (var raw in scopes)
        {
            var s = NormalizeScope(raw);
            if (string.IsNullOrWhiteSpace(s)) continue;

            if (s == "*") return true;
            if (s == required) return true;

            if (s.EndsWith(".*", StringComparison.Ordinal))
            {
                var prefix = s.Substring(0, s.Length - 2);
                if (required.StartsWith(prefix + ".", StringComparison.Ordinal))
                    return true;
            }
        }

        return false;
    }

    private static string NormalizeScope(string scope)
    {
        return (scope ?? string.Empty).Trim().ToLowerInvariant();
    }
}
