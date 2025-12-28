using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StreamVault.Application.Emails.DTOs;

namespace StreamVault.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailVerificationAsync(string email, string token)
    {
        // In development, just log the token
        _logger.LogInformation("Email verification for {Email}: {Token}", email, token);
        
        // TODO: Implement actual email sending
        // For production, integrate with SendGrid, Mailgun, or SMTP
    }

    public async Task SendPasswordResetAsync(string email, string token)
    {
        // In development, just log the token
        _logger.LogInformation("Password reset for {Email}: {Token}", email, token);
        
        // TODO: Implement actual email sending
    }

    public async Task SendTwoFactorCodeAsync(string email, string code)
    {
        // In development, just log the code
        _logger.LogInformation("2FA code for {Email}: {Code}", email, code);
        
        // TODO: Implement actual email sending
    }

    public async Task SendEmailAsync(SendEmailRequest request)
    {
        // In development, just log the email details
        _logger.LogInformation("Sending email to {Email} with subject: {Subject}", request.To, request.Subject);
        _logger.LogInformation("Email body: {Body}", request.HtmlBody);
        
        // TODO: Implement actual email sending
        // For production, integrate with SendGrid, Mailgun, or SMTP
    }
}
