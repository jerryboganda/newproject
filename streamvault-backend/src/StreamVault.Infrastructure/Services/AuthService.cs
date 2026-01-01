using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StreamVault.Application.Auth;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Application.Services;
using AppEmailService = StreamVault.Application.Services.IEmailService;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;
using System.Security.Cryptography;

namespace StreamVault.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly AppEmailService _emailService;
    private readonly ITokenService _tokenService;

    public AuthService(
        StreamVaultDbContext dbContext,
        IConfiguration configuration,
        AppEmailService emailService,
        ITokenService tokenService)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _emailService = emailService;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TenantSlug))
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
        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == request.TenantSlug);
        if (tenant == null)
        {
            tenant = new Tenant(request.TenantSlug, request.TenantSlug.ToLowerInvariant());
            tenant.StartTrial(14);
            _dbContext.Tenants.Add(tenant);
            await _dbContext.SaveChangesAsync();
        }

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
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken && u.RefreshTokenExpiry > DateTimeOffset.UtcNow);

        if (user == null)
            throw new Exception("Invalid refresh token");

        return await GenerateTokensAsync(user);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        if (user == null)
            return;

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        var verificationToken = await _dbContext.EmailVerificationTokens
            .Include(evt => evt.User)
            .FirstOrDefaultAsync(evt => evt.Token == token && !evt.IsUsed && evt.ExpiresAt > DateTimeOffset.UtcNow);

        if (verificationToken == null)
            return false;

        verificationToken.IsUsed = true;
        verificationToken.UsedAt = DateTimeOffset.UtcNow;

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

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var verificationToken = new EmailVerificationToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
        };

        _dbContext.EmailVerificationTokens.Add(verificationToken);
        await _dbContext.SaveChangesAsync();

        await _emailService.SendEmailVerificationAsync(email, token);
        return true;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return false;

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var resetToken = new EmailVerificationToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };

        _dbContext.EmailVerificationTokens.Add(resetToken);
        await _dbContext.SaveChangesAsync();

        await _emailService.SendPasswordResetAsync(email, token);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var resetToken = await _dbContext.EmailVerificationTokens
            .Include(evt => evt.User)
            .FirstOrDefaultAsync(evt => evt.Token == request.Token && !evt.IsUsed && evt.ExpiresAt > DateTimeOffset.UtcNow);

        if (resetToken == null)
            return false;

        // Frontends may not include the email address on reset.
        if (!string.IsNullOrWhiteSpace(request.Email) &&
            !string.Equals(resetToken.User.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            return false;

        resetToken.IsUsed = true;
        resetToken.UsedAt = DateTimeOffset.UtcNow;

        resetToken.User.PasswordHash = HashPassword(request.NewPassword);
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

        var code = RandomNumberGenerator.GetInt32(0, 999999).ToString("D6");

        var twoFactorCode = new TwoFactorAuthCode
        {
            UserId = userId,
            Code = code,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
        };

        _dbContext.TwoFactorAuthCodes.Add(twoFactorCode);
        await _dbContext.SaveChangesAsync();

        await _emailService.SendTwoFactorCodeAsync(user.Email, code);
        return code;
    }

    public async Task<bool> VerifyTwoFactorCodeAsync(Guid userId, string code)
    {
        var twoFactorCode = await _dbContext.TwoFactorAuthCodes
            .Where(tfac => tfac.UserId == userId && !tfac.IsUsed && tfac.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(tfac => tfac.CreatedAt)
            .FirstOrDefaultAsync();

        if (twoFactorCode == null)
            return false;

        if (!string.Equals(twoFactorCode.Code, code, StringComparison.Ordinal))
            return false;

        twoFactorCode.IsUsed = true;
        twoFactorCode.UsedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private async Task<AuthResponse> GenerateTokensAsync(User user)
    {
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

        var accessToken = _tokenService.GenerateAccessToken(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.TenantId,
            roles);

        var refreshToken = _tokenService.GenerateRefreshToken();

        var refreshDays = int.Parse(
            _configuration["JwtSettings:RefreshTokenExpiration"]
            ?? _configuration["JwtSettings:RefreshTokenExpiryInDays"]
            ?? "7");

        var accessMinutes = int.Parse(
            _configuration["JwtSettings:AccessTokenExpiration"]
            ?? _configuration["JwtSettings:ExpiryInMinutes"]
            ?? "15");

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(refreshDays);
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(accessMinutes),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AvatarUrl = user.AvatarUrl,
                Roles = roles
            }
        };
    }

    private static string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    private static bool VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}