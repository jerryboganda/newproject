using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Application.Services;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace StreamVault.Application.Auth;

public class AuthService : IAuthService
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthService(
        StreamVaultDbContext dbContext, 
        IConfiguration configuration, 
        IEmailService emailService)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Find user by email and tenant
        if (string.IsNullOrEmpty(request.TenantSlug))
            throw new Exception("Tenant slug is required");

        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == request.TenantSlug);

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
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = HashPassword(request.Password),
            TenantId = tenant.Id,
            Status = UserStatus.Active,
            IsEmailVerified = false
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // Find user by refresh token
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken && u.RefreshTokenExpiry > DateTime.UtcNow);

        if (user == null)
            throw new Exception("Invalid refresh token");

        // Generate new tokens
        return await GenerateTokensAsync(user);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _dbContext.SaveChangesAsync();
        }
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

        // Store refresh token for user
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        user.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = token.ValidTo,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
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

    // BCrypt password hashing
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        var verificationToken = await _dbContext.EmailVerificationTokens
            .Include(evt => evt.User)
            .FirstOrDefaultAsync(evt => evt.Token == token && !evt.IsUsed && evt.ExpiresAt > DateTimeOffset.UtcNow);

        if (verificationToken == null)
            return false;

        // Mark token as used
        verificationToken.IsUsed = true;
        verificationToken.UsedAt = DateTimeOffset.UtcNow;

        // Update user email verification status
        verificationToken.User.IsEmailVerified = true;
        verificationToken.User.EmailVerifiedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SendEmailVerificationAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || user.IsEmailVerified)
            return false;

        // Generate token
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        
        // Create verification token
        var verificationToken = new EmailVerificationToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
        };

        _dbContext.EmailVerificationTokens.Add(verificationToken);
        await _dbContext.SaveChangesAsync();

        // Send email
        await _emailService.SendEmailVerificationAsync(email, token);
        return true;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return false;

        // Generate token
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        
        // Store token (for simplicity, using EmailVerificationToken table)
        var resetToken = new EmailVerificationToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };

        _dbContext.EmailVerificationTokens.Add(resetToken);
        await _dbContext.SaveChangesAsync();

        // Send email
        await _emailService.SendPasswordResetAsync(email, token);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var resetToken = await _dbContext.EmailVerificationTokens
            .Include(evt => evt.User)
            .FirstOrDefaultAsync(evt => evt.Token == request.Token && !evt.IsUsed && evt.ExpiresAt > DateTimeOffset.UtcNow);

        if (resetToken == null || resetToken.User.Email != request.Email)
            return false;

        // Mark token as used
        resetToken.IsUsed = true;
        resetToken.UsedAt = DateTimeOffset.UtcNow;

        // Update password
        resetToken.User.PasswordHash = HashPassword(request.NewPassword);
        
        // Invalidate refresh tokens
        resetToken.User.RefreshToken = null;
        resetToken.User.RefreshTokenExpiry = null;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request, Guid userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null || !VerifyPassword(request.CurrentPassword, user.PasswordHash))
            return false;

        user.PasswordHash = HashPassword(request.NewPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;

        // Invalidate refresh tokens
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EnableTwoFactorAsync(Guid userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
            return false;

        user.TwoFactorEnabled = true;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenerateTwoFactorCodeAsync(Guid userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null || !user.TwoFactorEnabled)
            throw new Exception("2FA not enabled for user");

        // Generate 6-digit code
        var code = new Random().Next(0, 999999).ToString("D6");

        // Store code
        var twoFactorCode = new TwoFactorAuthCode
        {
            UserId = userId,
            Code = HashPassword(code), // Hash the code for security
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
        };

        _dbContext.TwoFactorAuthCodes.Add(twoFactorCode);
        await _dbContext.SaveChangesAsync();

        // Send email with code
        await _emailService.SendTwoFactorCodeAsync(user.Email, code);
        return code; // Return for testing purposes
    }

    public async Task<bool> VerifyTwoFactorCodeAsync(Guid userId, string code)
    {
        var twoFactorCode = await _dbContext.TwoFactorAuthCodes
            .FirstOrDefaultAsync(tfac => tfac.UserId == userId && !tfac.IsUsed && tfac.ExpiresAt > DateTimeOffset.UtcNow);

        if (twoFactorCode == null)
            return false;

        // Verify the code
        if (!VerifyPassword(code, twoFactorCode.Code))
            return false;

        // Mark as used
        twoFactorCode.IsUsed = true;
        twoFactorCode.UsedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }
}
