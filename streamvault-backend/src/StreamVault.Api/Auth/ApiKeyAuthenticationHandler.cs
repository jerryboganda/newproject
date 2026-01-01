using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Auth;

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-API-Key";

    private readonly StreamVaultDbContext _dbContext;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder,
        StreamVaultDbContext dbContext) : base(options, logger, encoder)
    {
        _dbContext = dbContext;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var values))
            return AuthenticateResult.NoResult();

        var apiKeyRaw = values.ToString().Trim();
        if (string.IsNullOrWhiteSpace(apiKeyRaw))
            return AuthenticateResult.Fail("API key missing");

        var hash = ComputeSha256Hex(apiKeyRaw);

        var now = DateTime.UtcNow;
        var apiKey = await _dbContext.ApiKeys
            .FirstOrDefaultAsync(k => k.KeyHash == hash && k.IsActive && k.ExpiresAt > now);

        if (apiKey == null)
            return AuthenticateResult.Fail("Invalid API key");

        if (apiKey.LastUsedAt < now.AddMinutes(-5))
        {
            apiKey.LastUsedAt = now;
            await _dbContext.SaveChangesAsync();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, apiKey.UserId.ToString()),
            new("tenant_id", apiKey.TenantId.ToString()),
            new("api_key_id", apiKey.Id.ToString()),
            new("api_key_name", apiKey.Name ?? string.Empty),
        };

        foreach (var scope in apiKey.Scopes ?? Array.Empty<string>())
        {
            if (!string.IsNullOrWhiteSpace(scope))
                claims.Add(new Claim("api_scope", scope));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return AuthenticateResult.Success(ticket);
    }

    private static string ComputeSha256Hex(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
