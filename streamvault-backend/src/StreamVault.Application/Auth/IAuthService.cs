using StreamVault.Application.Auth.DTOs;

namespace StreamVault.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task LogoutAsync(string refreshToken);
    Task<bool> VerifyEmailAsync(string token);
    Task<bool> SendEmailVerificationAsync(string email);
    Task<bool> SendPasswordResetEmailAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request, Guid userId);
    Task<bool> EnableTwoFactorAsync(Guid userId);
    Task<string> GenerateTwoFactorCodeAsync(Guid userId);
    Task<bool> VerifyTwoFactorCodeAsync(Guid userId, string code);
}
