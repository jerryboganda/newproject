using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace StreamVault.Application.Auth;

public class AuthService : IAuthService
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthService(StreamVaultDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Find user by email and tenant
        var tenantSlug = request.TenantSlug ?? GetTenantFromContext();
        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == tenantSlug);

        if (tenant == null)
            throw new Exception("Tenant not found");

        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.TenantId == tenant.Id);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            throw new Exception("Invalid credentials");

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Find or create tenant
        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == request.TenantSlug);
        if (tenant == null)
        {
            // Create new tenant (skeleton - in reality, check plan, etc.)
            tenant = new Tenant
            {
                Name = request.TenantSlug,
                Slug = request.TenantSlug,
                Status = TenantStatus.Trial
            };
            _dbContext.Tenants.Add(tenant);
            await _dbContext.SaveChangesAsync();
        }

        // Create user
        var user = new User
        {
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            TenantId = tenant.Id
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // Implement refresh token logic
        throw new NotImplementedException();
    }

    public Task LogoutAsync(string refreshToken)
    {
        // Invalidate refresh token
        throw new NotImplementedException();
    }

    private async Task<AuthResponse> GenerateTokensAsync(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("tenant_id", user.TenantId?.ToString() ?? "")
        };

        // Add roles
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = GenerateRefreshToken();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = token.ValidTo,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = "", // Add first/last name to User entity
                LastName = "",
                Roles = roles
            }
        };
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string? GetTenantFromContext()
    {
        // Get from HttpContext (set by middleware)
        // For now, return null
        return null;
    }

    // Simple password hash for demo (not secure - replace with BCrypt later)
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}
