using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StreamVault.Application.Interfaces;

namespace StreamVault.Application.Services
{
    /// <summary>
    /// JWT token service for authentication
    /// </summary>
    public interface ITokenService
    {
        string GenerateAccessToken(Guid userId, string email, string firstName, string lastName, Guid? tenantId, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        bool ValidateToken(string token);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenExpiration;
        private readonly int _refreshTokenExpiration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            _secretKey = _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            _issuer = _configuration["JwtSettings:Issuer"] ?? "StreamVault";
            _audience = _configuration["JwtSettings:Audience"] ?? "StreamVault";
            _accessTokenExpiration = int.Parse(_configuration["JwtSettings:AccessTokenExpiration"] ?? "15"); // minutes
            _refreshTokenExpiration = int.Parse(_configuration["JwtSettings:RefreshTokenExpiration"] ?? "7"); // days
        }

        public string GenerateAccessToken(Guid userId, string email, string firstName, string lastName, Guid? tenantId, IList<string> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.GivenName, firstName),
                new Claim(ClaimTypes.Surname, lastName),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add tenant claim if present
            if (tenantId.HasValue)
            {
                claims.Add(new Claim("tenant_id", tenantId.Value.ToString()));
            }

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpiration),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _issuer,
                Audience = _audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                ValidateLifetime = false // Don't validate expiration here
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                
                if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Authentication service for managing user authentication
    /// </summary>
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(string email, string password, string? tenantSlug = null);
        Task<AuthResult> RegisterAsync(string email, string password, string firstName, string lastName, string? tenantSlug = null);
        Task<AuthResult> RefreshTokenAsync(string accessToken, string refreshToken);
        Task LogoutAsync(Guid userId);
        Task<bool> VerifyEmailAsync(Guid userId, string token);
        Task SendPasswordResetEmailAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task Enable2FAAsync(Guid userId);
        Task<bool> Verify2FAAsync(Guid userId, string code);
    }

    public class AuthService : IAuthService
    {
        private readonly StreamVault.Infrastructure.Data.StreamVaultDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly EmailTemplateService _templateService;
        private readonly ITenantResolver _tenantResolver;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            StreamVault.Infrastructure.Data.StreamVaultDbContext context,
            ITokenService tokenService,
            IEmailService emailService,
            EmailTemplateService templateService,
            ITenantResolver tenantResolver,
            ILogger<AuthService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            _tenantResolver = tenantResolver ?? throw new ArgumentNullException(nameof(tenantResolver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthResult> LoginAsync(string email, string password, string? tenantSlug = null)
        {
            try
            {
                // Resolve tenant if slug is provided
                Domain.Entities.Tenant? tenant = null;
                if (!string.IsNullOrEmpty(tenantSlug))
                {
                    tenant = await _tenantResolver.ResolveTenantAsync(tenantSlug);
                    if (tenant == null)
                    {
                        return new AuthResult { Success = false, Error = "Invalid tenant" };
                    }
                }

                // Find user
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return new AuthResult { Success = false, Error = "Invalid credentials" };
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    return new AuthResult { Success = false, Error = "Invalid credentials" };
                }

                // Check if user is active
                if (user.Status != 1) // Assuming 1 = Active
                {
                    return new AuthResult { Success = false, Error = "Account is not active" };
                }

                // Check tenant membership if tenant is specified
                if (tenant != null && user.TenantId != tenant.Id)
                {
                    return new AuthResult { Success = false, Error = "User is not a member of this tenant" };
                }

                // Get user roles
                var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

                // Generate tokens
                var accessToken = _tokenService.GenerateAccessToken(
                    user.Id, 
                    user.Email, 
                    user.FirstName, 
                    user.LastName, 
                    user.TenantId, 
                    roles);
                
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Update user with refresh token
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

                return new AuthResult
                {
                    Success = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        AvatarUrl = user.AvatarUrl,
                        TenantId = user.TenantId,
                        Roles = roles,
                        IsEmailVerified = user.IsEmailVerified,
                        TwoFactorEnabled = user.TwoFactorEnabled
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", email);
                return new AuthResult { Success = false, Error = "An error occurred during login" };
            }
        }

        public async Task<AuthResult> RegisterAsync(string email, string password, string firstName, string lastName, string? tenantSlug = null)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUser != null)
                {
                    return new AuthResult { Success = false, Error = "User already exists" };
                }

                // Resolve tenant if slug is provided
                Domain.Entities.Tenant? tenant = null;
                if (!string.IsNullOrEmpty(tenantSlug))
                {
                    tenant = await _tenantResolver.ResolveTenantAsync(tenantSlug);
                    if (tenant == null)
                    {
                        return new AuthResult { Success = false, Error = "Invalid tenant" };
                    }
                }

                // Create new user
                var user = new Domain.Entities.User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    TenantId = tenant?.Id,
                    Status = 1, // Active
                    IsEmailVerified = false,
                    TwoFactorEnabled = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Add default role if tenant exists
                if (tenant != null)
                {
                    var defaultRole = await _context.Roles
                        .FirstOrDefaultAsync(r => r.TenantId == tenant.Id && r.Name == "User");
                    
                    if (defaultRole != null)
                    {
                        user.UserRoles.Add(new Domain.Entities.UserRole
                        {
                            RoleId = defaultRole.Id,
                            AssignedAt = DateTime.UtcNow
                        });
                    }
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate email verification token
                var verificationToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.Id}:{DateTime.UtcNow.AddDays(1):O}"));

                // Send verification email
                var templateData = new Dictionary<string, object>
                {
                    ["name"] = $"{firstName} {lastName}",
                    ["verification_url"] = $"https://{tenant?.Slug ?? "app"}.streamvault.com/verify-email?token={Uri.EscapeDataString(verificationToken)}"
                };

                await _emailService.SendEmailWithTemplateAsync(
                    email, 
                    "email_verification", 
                    templateData);

                _logger.LogInformation("User registered successfully: {UserId}", user.Id);

                return new AuthResult
                {
                    Success = true,
                    Message = "Registration successful. Please check your email to verify your account.",
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        TenantId = user.TenantId,
                        IsEmailVerified = false,
                        TwoFactorEnabled = false
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", email);
                return new AuthResult { Success = false, Error = "An error occurred during registration" };
            }
        }

        public async Task<AuthResult> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            try
            {
                var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
                if (principal == null)
                {
                    return new AuthResult { Success = false, Error = "Invalid access token" };
                }

                var userId = Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
                {
                    return new AuthResult { Success = false, Error = "Invalid refresh token" };
                }

                var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

                var newAccessToken = _tokenService.GenerateAccessToken(
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.TenantId,
                    roles);

                var newRefreshToken = _tokenService.GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                return new AuthResult
                {
                    Success = true,
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return new AuthResult { Success = false, Error = "An error occurred during token refresh" };
            }
        }

        public async Task LogoutAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiry = null;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
            }
        }

        public async Task<bool> VerifyEmailAsync(Guid userId, string token)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.IsEmailVerified)
                {
                    return false;
                }

                // Decode and verify token
                var tokenData = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = tokenData.Split(':');
                
                if (parts.Length != 2 || !Guid.TryParse(parts[0], out var tokenUserId) || tokenUserId != userId)
                {
                    return false;
                }

                if (!DateTime.TryParse(parts[1], out var expiry) || expiry < DateTime.UtcNow)
                {
                    return false;
                }

                user.IsEmailVerified = true;
                user.EmailVerifiedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Email verified successfully for user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification for user: {UserId}", userId);
                return false;
            }
        }

        public async Task SendPasswordResetEmailAsync(string email)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    // Don't reveal that user doesn't exist
                    return;
                }

                var resetToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.Id}:{DateTime.UtcNow.AddHours(1):O}"));

                var templateData = new Dictionary<string, object>
                {
                    ["name"] = $"{user.FirstName} {user.LastName}",
                    ["reset_url"] = $"https://app.streamvault.com/reset-password?token={Uri.EscapeDataString(resetToken)}"
                };

                await _emailService.SendEmailWithTemplateAsync(
                    email,
                    "password_reset",
                    templateData);

                _logger.LogInformation("Password reset email sent to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to: {Email}", email);
            }
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                var tokenData = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = tokenData.Split(':');

                if (parts.Length != 2 || !Guid.TryParse(parts[0], out var userId))
                {
                    return false;
                }

                if (!DateTime.TryParse(parts[1], out var expiry) || expiry < DateTime.UtcNow)
                {
                    return false;
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.RefreshToken = null; // Invalidate all refresh tokens
                user.RefreshTokenExpiry = null;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Password reset successfully for user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return false;
            }
        }

        public async Task Enable2FAAsync(Guid userId)
        {
            // Implementation for enabling 2FA
            // This would involve generating a secret key and QR code
            await Task.CompletedTask;
        }

        public async Task<bool> Verify2FAAsync(Guid userId, string code)
        {
            // Implementation for verifying 2FA code
            await Task.CompletedTask;
            return true;
        }
    }

    // DTOs
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public UserDto? User { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public Guid? TenantId { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsEmailVerified { get; set; }
        public bool TwoFactorEnabled { get; set; }
    }
}
