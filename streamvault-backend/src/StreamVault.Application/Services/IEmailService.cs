using StreamVault.Application.Emails.DTOs;

namespace StreamVault.Application.Services;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string token);
    Task SendPasswordResetAsync(string email, string token);
    Task SendTwoFactorCodeAsync(string email, string code);
    Task SendEmailAsync(SendEmailRequest request);
}
