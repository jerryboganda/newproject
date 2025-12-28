using Microsoft.Extensions.Logging;
using StreamVault.Application.Emails.DTOs;
using StreamVault.Application.Services;

namespace StreamVault.Application.Emails;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        IEmailService emailService,
        IEmailTemplateService templateService,
        ILogger<EmailNotificationService> logger)
    {
        _emailService = emailService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task SendVideoUploadedEmailAsync(string toEmail, VideoUploadedEmailData data)
    {
        try
        {
            var template = _templateService.GetVideoUploadedTemplate(data);
            
            await _emailService.SendEmailAsync(new SendEmailRequest
            {
                To = toEmail,
                Subject = template.Subject,
                HtmlBody = template.HtmlBody,
                TextBody = template.TextBody
            });

            _logger.LogInformation("Video uploaded email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send video uploaded email to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendCommentReceivedEmailAsync(string toEmail, CommentReceivedEmailData data)
    {
        try
        {
            var template = _templateService.GetCommentReceivedTemplate(data);
            
            await _emailService.SendEmailAsync(new SendEmailRequest
            {
                To = toEmail,
                Subject = template.Subject,
                HtmlBody = template.HtmlBody,
                TextBody = template.TextBody
            });

            _logger.LogInformation("Comment received email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send comment received email to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendSubscriptionRenewedEmailAsync(string toEmail, SubscriptionRenewedEmailData data)
    {
        try
        {
            var template = _templateService.GetSubscriptionRenewedTemplate(data);
            
            await _emailService.SendEmailAsync(new SendEmailRequest
            {
                To = toEmail,
                Subject = template.Subject,
                HtmlBody = template.HtmlBody,
                TextBody = template.TextBody
            });

            _logger.LogInformation("Subscription renewed email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send subscription renewed email to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendPaymentReceivedEmailAsync(string toEmail, PaymentReceivedEmailData data)
    {
        try
        {
            var template = _templateService.GetPaymentReceivedTemplate(data);
            
            await _emailService.SendEmailAsync(new SendEmailRequest
            {
                To = toEmail,
                Subject = template.Subject,
                HtmlBody = template.HtmlBody,
                TextBody = template.TextBody
            });

            _logger.LogInformation("Payment received email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payment received email to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName, string loginUrl)
    {
        try
        {
            var template = _templateService.GetWelcomeEmailTemplate(userName, loginUrl);
            
            await _emailService.SendEmailAsync(new SendEmailRequest
            {
                To = toEmail,
                Subject = template.Subject,
                HtmlBody = template.HtmlBody,
                TextBody = template.TextBody
            });

            _logger.LogInformation("Welcome email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetUrl)
    {
        try
        {
            var template = _templateService.GetPasswordResetTemplate(resetUrl);
            
            await _emailService.SendEmailAsync(new SendEmailRequest
            {
                To = toEmail,
                Subject = template.Subject,
                HtmlBody = template.HtmlBody,
                TextBody = template.TextBody
            });

            _logger.LogInformation("Password reset email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendEmailVerificationEmailAsync(string toEmail, string verificationUrl)
    {
        try
        {
            var template = _templateService.GetEmailVerificationTemplate(verificationUrl);
            
            await _emailService.SendEmailAsync(new SendEmailRequest
            {
                To = toEmail,
                Subject = template.Subject,
                HtmlBody = template.HtmlBody,
                TextBody = template.TextBody
            });

            _logger.LogInformation("Email verification sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email verification to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendCustomEmailAsync(SendEmailRequest request)
    {
        try
        {
            await _emailService.SendEmailAsync(request);
            _logger.LogInformation("Custom email sent to {Email}", request.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send custom email to {Email}", request.To);
            throw;
        }
    }
}
