using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StreamVault.Application.Auth;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Infrastructure.Data;
using StreamVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace StreamVault.Api.Controllers;

/// <summary>
/// Authentication Controller - Handles login, registration, token refresh, 2FA, password reset
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly StreamVaultDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        StreamVaultDbContext dbContext,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _dbContext = dbContext;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <remarks>
    /// Returns JWT access token and refresh token
    /// 
    /// If 2FA is enabled, returns 2FA code requirement instead
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { error = "Email and password are required" });

            // Validate tenant exists
            var tenant = await _dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Slug == request.TenantSlug);

            if (tenant == null)
                return NotFound(new { error = "Tenant not found" });

            // Find user by email and tenant
            var user = await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.TenantId == tenant.Id);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning($"Failed login attempt for email: {request.Email}, tenant: {request.TenantSlug}");
                return Unauthorized(new { error = "Invalid credentials" });
            }

            // Check if user is active
            if (user.Status != UserStatus.Active)
                return Unauthorized(new { error = "User account is not active" });

            // Check if email is verified (unless in development)
            if (!user.IsEmailVerified && _configuration["Environment"] == "Production")
                return Unauthorized(new { error = "Email not verified. Please check your inbox" });

            // If 2FA is enabled, send code and return requiring verification
            if (user.TwoFactorEnabled)
            {
                var twoFactorCode = GenerateTwoFactorCode();
                var twoFactorEntity = new TwoFactorAuthCode
                {
                    UserId = user.Id,
                    Code = HashCode(twoFactorCode),
                    ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
                    IsUsed = false
                };

                _dbContext.TwoFactorAuthCodes.Add(twoFactorEntity);
                await _dbContext.SaveChangesAsync();

                // Send email with 2FA code
                await _emailService.SendAsync(
                    to: user.Email,
                    subject: "Your StreamVault Two-Factor Authentication Code",
                    htmlContent: $"""
                        <h2>Two-Factor Authentication</h2>
                        <p>Your 2FA code is: <strong>{twoFactorCode}</strong></p>
                        <p>This code expires in 10 minutes.</p>
                        """,
                    isHtml: true
                );

                return Ok(new
                {
                    success = true,
                    message = "2FA code sent to your email",
                    requires2FA = true,
                    userId = user.Id,
                    email = user.Email
                });
            }

            // Generate tokens
            var response = await _authService.LoginAsync(request);
            
            // Update last login
            user.LastLoginAt = DateTimeOffset.UtcNow;
            user.LastLoginIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"User logged in: {user.Email}, tenant: {tenant.Slug}");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Verify 2FA code and get access tokens
    /// </summary>
    [HttpPost("login/verify-2fa")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyTwoFactorCode([FromBody] TwoFactorVerificationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.Code))
                return BadRequest(new { error = "2FA code is required" });

            var user = await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == request.UserId);

            if (user == null)
                return NotFound(new { error = "User not found" });

            // Find valid 2FA code
            var twoFactorCode = await _dbContext.TwoFactorAuthCodes
                .Where(t => t.UserId == request.UserId && !t.IsUsed && t.ExpiresAt > DateTimeOffset.UtcNow)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            if (twoFactorCode == null || !VerifyCode(request.Code, twoFactorCode.Code))
            {
                _logger.LogWarning($"Invalid 2FA code for user: {user.Email}");
                return Unauthorized(new { error = "Invalid or expired 2FA code" });
            }

            // Mark code as used
            twoFactorCode.IsUsed = true;
            await _dbContext.SaveChangesAsync();

            // Generate tokens
            var loginRequest = new LoginRequest
            {
                Email = user.Email,
                Password = "", // Not needed - already verified
                TenantSlug = user.Tenant?.Slug ?? ""
            };

            var response = await _authService.LoginAsync(loginRequest);

            _logger.LogInformation($"User completed 2FA: {user.Email}");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during 2FA verification");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred" });
        }
    }

    /// <summary>
    /// Register new tenant and user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { error = "Email and password are required" });

            if (request.Password.Length < 8)
                return BadRequest(new { error = "Password must be at least 8 characters" });

            // Check if tenant slug already exists
            var existingTenant = await _dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Slug == request.TenantSlug);

            if (existingTenant != null)
                return BadRequest(new { error = "Tenant slug already exists" });

            // Check if email already exists
            var existingUser = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
                return BadRequest(new { error = "Email already registered" });

            // Create tenant
            var tenant = new Tenant
            {
                Name = request.TenantName ?? request.TenantSlug,
                Slug = request.TenantSlug.ToLowerInvariant(),
                Status = TenantStatus.Trial,
                DatabaseType = TenantDatabaseType.Shared,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.Tenants.Add(tenant);
            await _dbContext.SaveChangesAsync();

            // Create default branding
            var branding = new TenantBranding
            {
                TenantId = tenant.Id,
                PrimaryColor = "#3B82F6",
                SecondaryColor = "#1E40AF",
                AccentColor = "#0EA5E9"
            };

            _dbContext.TenantBrandings.Add(branding);

            // Create default roles
            var adminRole = new Role
            {
                TenantId = tenant.Id,
                Name = "Admin",
                NormalizedName = "ADMIN",
                IsSystemRole = true,
                Description = "Full access administrator"
            };

            var editorRole = new Role
            {
                TenantId = tenant.Id,
                Name = "Editor",
                NormalizedName = "EDITOR",
                IsSystemRole = true,
                Description = "Can manage videos and collections"
            };

            _dbContext.Roles.Add(adminRole);
            _dbContext.Roles.Add(editorRole);
            await _dbContext.SaveChangesAsync();

            // Create user
            var user = new User
            {
                Email = request.Email.ToLowerInvariant(),
                FirstName = request.FirstName ?? "User",
                LastName = request.LastName ?? "",
                PasswordHash = HashPassword(request.Password),
                TenantId = tenant.Id,
                Status = UserStatus.Active,
                IsEmailVerified = false,
                IsOwner = true
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Assign admin role
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = adminRole.Id,
                AssignedAt = DateTimeOffset.UtcNow
            };

            _dbContext.UserRoles.Add(userRole);

            // Create email verification token
            var verificationToken = new EmailVerificationToken
            {
                UserId = user.Id,
                Token = GenerateToken(),
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
                IsUsed = false
            };

            _dbContext.EmailVerificationTokens.Add(verificationToken);
            await _dbContext.SaveChangesAsync();

            // Send verification email
            var verificationUrl = $"{_configuration["AppUrl"]}/verify-email?token={verificationToken.Token}";
            await _emailService.SendAsync(
                to: user.Email,
                subject: "Verify your StreamVault account",
                htmlContent: $"""
                    <h2>Welcome to StreamVault!</h2>
                    <p>Please verify your email address by clicking the link below:</p>
                    <a href="{verificationUrl}">Verify Email</a>
                    <p>This link expires in 24 hours.</p>
                    """,
                isHtml: true
            );

            _logger.LogInformation($"New tenant registered: {tenant.Slug}, user: {user.Email}");

            return Ok(new
            {
                success = true,
                message = "Registration successful. Please check your email to verify your account.",
                tenantId = tenant.Id,
                userId = user.Id,
                email = user.Email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Verify email address with token
    /// </summary>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                return BadRequest(new { error = "Verification token is required" });

            var verificationToken = await _dbContext.EmailVerificationTokens
                .Include(evt => evt.User)
                .FirstOrDefaultAsync(evt => evt.Token == request.Token && !evt.IsUsed && evt.ExpiresAt > DateTimeOffset.UtcNow);

            if (verificationToken == null)
                return BadRequest(new { error = "Invalid or expired verification token" });

            // Mark email as verified
            var user = verificationToken.User;
            user.IsEmailVerified = true;
            user.EmailVerifiedAt = DateTimeOffset.UtcNow;

            verificationToken.IsUsed = true;
            verificationToken.UsedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Email verified for user: {user.Email}");

            return Ok(new { success = true, message = "Email verified successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred" });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new { error = "Refresh token is required" });

            var response = await _authService.RefreshTokenAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Invalid refresh token");
            return Unauthorized(new { error = "Invalid or expired refresh token" });
        }
    }

    /// <summary>
    /// Logout - revoke refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!Guid.TryParse(userId, out var userIdGuid))
                return Unauthorized();

            var user = await _dbContext.Users.FindAsync(userIdGuid);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"User logged out: {user.Email}");
            }

            return Ok(new { success = true, message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred" });
        }
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { error = "Email is required" });

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                // Don't reveal if email exists for security
                return Ok(new { success = true, message = "If an account exists, a password reset link has been sent" });
            }

            // Create password reset token
            var resetToken = new EmailVerificationToken
            {
                UserId = user.Id,
                Token = GenerateToken(),
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
                IsUsed = false
            };

            _dbContext.EmailVerificationTokens.Add(resetToken);
            await _dbContext.SaveChangesAsync();

            // Send reset email
            var resetUrl = $"{_configuration["AppUrl"]}/reset-password?token={resetToken.Token}";
            await _emailService.SendAsync(
                to: user.Email,
                subject: "Reset your StreamVault password",
                htmlContent: $"""
                    <h2>Password Reset Request</h2>
                    <p>Click the link below to reset your password:</p>
                    <a href="{resetUrl}">Reset Password</a>
                    <p>This link expires in 1 hour.</p>
                    <p>If you didn't request this, ignore this email.</p>
                    """,
                isHtml: true
            );

            _logger.LogInformation($"Password reset requested for: {user.Email}");

            return Ok(new { success = true, message = "If an account exists, a password reset link has been sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred" });
        }
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new { error = "Token and new password are required" });

            if (request.NewPassword.Length < 8)
                return BadRequest(new { error = "Password must be at least 8 characters" });

            var resetToken = await _dbContext.EmailVerificationTokens
                .Include(evt => evt.User)
                .FirstOrDefaultAsync(evt => evt.Token == request.Token && !evt.IsUsed && evt.ExpiresAt > DateTimeOffset.UtcNow);

            if (resetToken == null)
                return BadRequest(new { error = "Invalid or expired reset token" });

            // Update password
            var user = resetToken.User;
            user.PasswordHash = HashPassword(request.NewPassword);
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;

            resetToken.IsUsed = true;
            resetToken.UsedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Send confirmation email
            await _emailService.SendAsync(
                to: user.Email,
                subject: "Your password has been reset",
                htmlContent: "<h2>Password Reset Successful</h2><p>Your StreamVault password has been successfully reset.</p>",
                isHtml: true
            );

            _logger.LogInformation($"Password reset completed for: {user.Email}");

            return Ok(new { success = true, message = "Password reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred" });
        }
    }

    /// <summary>
    /// Super admin impersonation - login as another user
    /// </summary>
    [HttpPost("impersonate")]
    [Authorize]
    public async Task<IActionResult> ImpersonateUser([FromBody] ImpersonationRequest request)
    {
        try
        {
            var superAdminId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(superAdminId) || !Guid.TryParse(superAdminId, out var adminGuid))
                return Unauthorized();

            var superAdmin = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == adminGuid);

            if (superAdmin == null)
                return Unauthorized();

            // TODO: Check if user is super admin (needs super admin table)
            var userToImpersonate = await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == request.UserId);

            if (userToImpersonate == null)
                return NotFound(new { error = "User not found" });

            // Generate impersonation token
            var response = await _authService.LoginAsync(new LoginRequest
            {
                Email = userToImpersonate.Email,
                Password = "",
                TenantSlug = userToImpersonate.Tenant?.Slug ?? ""
            });

            _logger.LogWarning($"Super admin {superAdmin.Email} is impersonating {userToImpersonate.Email}");

            return Ok(new { success = true, data = response, impersonating = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during impersonation");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred" });
        }
    }

    /// <summary>
    /// Get current user info
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized();

            var user = await _dbContext.Users
                .Include(u => u.Tenant)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userGuid);

            if (user == null)
                return NotFound();

            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                tenantId = user.TenantId,
                tenantName = user.Tenant?.Name,
                roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList(),
                twoFactorEnabled = user.TwoFactorEnabled,
                emailVerified = user.IsEmailVerified
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching current user");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    #region Helper Methods

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    private bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    private string GenerateTwoFactorCode()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] tokenData = new byte[4];
            rng.GetBytes(tokenData);
            int code = Math.Abs(BitConverter.ToInt32(tokenData, 0)) % 1000000;
            return code.ToString("D6");
        }
    }

    private string HashCode(string code)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(code));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyCode(string code, string hash)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(code));
            string computedHash = Convert.ToBase64String(hashedBytes);
            return computedHash == hash;
        }
    }

    private string GenerateToken()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] tokenData = new byte[32];
            rng.GetBytes(tokenData);
            return Convert.ToBase64String(tokenData);
        }
    }

    #endregion
}

// DTOs
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
    public string? TenantName { get; set; }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class TwoFactorVerificationRequest
{
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
}

public class VerifyEmailRequest
{
    public string Token { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ImpersonationRequest
{
    public Guid UserId { get; set; }
}
