using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StreamVault.Infrastructure.Services
{
    /// <summary>
    /// Email service interface
    /// </summary>
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default);
        Task SendEmailWithTemplateAsync(string to, string templateId, Dictionary<string, object> templateData, CancellationToken cancellationToken = default);
        Task SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// SendGrid email service implementation
    /// </summary>
    public class SendGridEmailService : IEmailService
    {
        private readonly ILogger<SendGridEmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _apiKey = _configuration["Email:SendGrid:ApiKey"] ?? throw new InvalidOperationException("SendGrid API key not configured");
            _fromEmail = _configuration["Email:From"] ?? throw new InvalidOperationException("From email not configured");
            _fromName = _configuration["Email:FromName"] ?? "StreamVault";
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Sending email to {Email} with subject: {Subject}", to, subject);

                // Note: In production, you would use SendGrid SDK
                // This is a placeholder implementation showing the structure
                var emailData = new
                {
                    from = new { email = _fromEmail, name = _fromName },
                    personalizations = new[]
                    {
                        new
                        {
                            to = new[] { new { email = to } },
                            subject = subject
                        }
                    },
                    content = new[]
                    {
                        new { type = "text/html", value = htmlBody },
                        textBody != null ? new { type = "text/plain", value = textBody } : null
                    }
                };

                // Send email using SendGrid API
                await SendEmailViaApi(emailData, cancellationToken);

                _logger.LogInformation("Successfully sent email to {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                throw new EmailServiceException("Failed to send email", ex);
            }
        }

        public async Task SendEmailWithTemplateAsync(string to, string templateId, Dictionary<string, object> templateData, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Sending templated email to {Email} using template: {TemplateId}", to, templateId);

                var emailData = new
                {
                    from = new { email = _fromEmail, name = _fromName },
                    personalizations = new[]
                    {
                        new
                        {
                            to = new[] { new { email = to } },
                            dynamic_template_data = templateData
                        }
                    },
                    template_id = templateId
                };

                await SendEmailViaApi(emailData, cancellationToken);

                _logger.LogInformation("Successfully sent templated email to {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send templated email to {Email}", to);
                throw new EmailServiceException("Failed to send templated email", ex);
            }
        }

        public async Task SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Sending bulk email to {Count} recipients", recipients.Count());

                foreach (var recipient in recipients)
                {
                    await SendEmailAsync(recipient, subject, htmlBody, textBody, cancellationToken);
                    // Add delay to avoid rate limiting
                    await Task.Delay(100, cancellationToken);
                }

                _logger.LogInformation("Successfully sent bulk email to {Count} recipients", recipients.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send bulk email");
                throw new EmailServiceException("Failed to send bulk email", ex);
            }
        }

        private async Task SendEmailViaApi(object emailData, CancellationToken cancellationToken)
        {
            // This would be the actual SendGrid API call
            // For now, we'll log the data
            _logger.LogDebug("Email data: {@EmailData}", emailData);
            
            // Simulate API call
            await Task.Delay(100, cancellationToken);
        }
    }

    /// <summary>
    /// SMTP email service implementation (fallback option)
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly ILogger<SmtpEmailService> _logger;
        private readonly IConfiguration _configuration;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Sending email via SMTP to {Email}", to);

                // In production, you would use System.Net.Mail or similar
                // This is a placeholder implementation
                var smtpConfig = new
                {
                    Host = _configuration["Email:Smtp:Host"],
                    Port = _configuration["Email:Smtp:Port"],
                    Username = _configuration["Email:Smtp:Username"],
                    Password = _configuration["Email:Smtp:Password"],
                    From = _configuration["Email:From"],
                    FromName = _configuration["Email:FromName"] ?? "StreamVault"
                };

                _logger.LogDebug("SMTP Configuration: {@Config}", new { smtpConfig.Host, smtpConfig.Port, smtpConfig.Username });

                // Simulate sending email
                await Task.Delay(100, cancellationToken);

                _logger.LogInformation("Successfully sent email via SMTP to {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via SMTP to {Email}", to);
                throw new EmailServiceException("Failed to send email via SMTP", ex);
            }
        }

        public async Task SendEmailWithTemplateAsync(string to, string templateId, Dictionary<string, object> templateData, CancellationToken cancellationToken = default)
        {
            // For SMTP, templates would need to be rendered locally
            // This is a simplified implementation
            var htmlBody = await RenderTemplateAsync(templateId, templateData);
            await SendEmailAsync(to, "Email from StreamVault", htmlBody, null, cancellationToken);
        }

        public async Task SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default)
        {
            foreach (var recipient in recipients)
            {
                await SendEmailAsync(recipient, subject, htmlBody, textBody, cancellationToken);
                await Task.Delay(100, cancellationToken);
            }
        }

        private async Task<string> RenderTemplateAsync(string templateId, Dictionary<string, object> data)
        {
            // In production, you would use a template engine like Razor or Scriban
            // This is a placeholder
            return $"<html><body><h1>Hello {data.GetValueOrDefault("name", "User")}</h1><p>This is a template email.</p></body></html>";
        }
    }

    /// <summary>
    /// Email template service for managing email templates
    /// </summary>
    public class EmailTemplateService
    {
        private readonly ILogger<EmailTemplateService> _logger;
        private readonly Dictionary<string, EmailTemplate> _templates;

        public EmailTemplateService(ILogger<EmailTemplateService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _templates = new Dictionary<string, EmailTemplate>
            {
                ["welcome"] = new EmailTemplate
                {
                    Subject = "Welcome to StreamVault!",
                    HtmlBody = @"
                        <html>
                        <body>
                            <h2>Welcome to StreamVault, {{name}}!</h2>
                            <p>Thank you for joining our platform. Your account has been successfully created.</p>
                            <p>You can now start uploading and sharing your videos.</p>
                            <p>Best regards,<br/>The StreamVault Team</p>
                        </body>
                        </html>",
                    TextBody = @"Welcome to StreamVault, {{name}}!

Thank you for joining our platform. Your account has been successfully created.

You can now start uploading and sharing your videos.

Best regards,
The StreamVault Team"
                },
                ["password_reset"] = new EmailTemplate
                {
                    Subject = "Reset your StreamVault password",
                    HtmlBody = @"
                        <html>
                        <body>
                            <h2>Password Reset Request</h2>
                            <p>Hello {{name}},</p>
                            <p>You requested to reset your password. Click the link below to reset it:</p>
                            <p><a href=""{{reset_url}}"">Reset Password</a></p>
                            <p>This link will expire in 24 hours.</p>
                            <p>If you didn't request this, please ignore this email.</p>
                            <p>Best regards,<br/>The StreamVault Team</p>
                        </body>
                        </html>",
                    TextBody = @"Password Reset Request

Hello {{name}},

You requested to reset your password. Copy and paste the following link into your browser:
{{reset_url}}

This link will expire in 24 hours.

If you didn't request this, please ignore this email.

Best regards,
The StreamVault Team"
                },
                ["subscription_created"] = new EmailTemplate
                {
                    Subject = "Your StreamVault subscription is active!",
                    HtmlBody = @"
                        <html>
                        <body>
                            <h2>Subscription Activated</h2>
                            <p>Hello {{name}},</p>
                            <p>Thank you for subscribing to {{plan_name}}! Your subscription is now active.</p>
                            <p>Subscription details:</p>
                            <ul>
                                <li>Plan: {{plan_name}}</li>
                                <li>Price: {{plan_price}}</li>
                                <li>Billing cycle: {{billing_cycle}}</li>
                                <li>Next billing date: {{next_billing_date}}</li>
                            </ul>
                            <p>You can now enjoy all the features of your plan.</p>
                            <p>Best regards,<br/>The StreamVault Team</p>
                        </body>
                        </html>",
                    TextBody = @"Subscription Activated

Hello {{name}},

Thank you for subscribing to {{plan_name}}! Your subscription is now active.

Subscription details:
- Plan: {{plan_name}}
- Price: {{plan_price}}
- Billing cycle: {{billing_cycle}}
- Next billing date: {{next_billing_date}}

You can now enjoy all the features of your plan.

Best regards,
The StreamVault Team"
                },
                ["video_processed"] = new EmailTemplate
                {
                    Subject = "Your video is ready!",
                    HtmlBody = @"
                        <html>
                        <body>
                            <h2>Video Processing Complete</h2>
                            <p>Hello {{name}},</p>
                            <p>Your video '{{video_title}}' has been successfully processed and is ready to view.</p>
                            <p><a href=""{{video_url}}"">Watch Video</a></p>
                            <p>Video details:</p>
                            <ul>
                                <li>Duration: {{video_duration}}</li>
                                <li>Size: {{video_size}}</li>
                                <li>Upload date: {{upload_date}}</li>
                            </ul>
                            <p>Best regards,<br/>The StreamVault Team</p>
                        </body>
                        </html>",
                    TextBody = @"Video Processing Complete

Hello {{name}},

Your video '{{video_title}}' has been successfully processed and is ready to view.

Video details:
- Duration: {{video_duration}}
- Size: {{video_size}}
- Upload date: {{upload_date}}

Best regards,
The StreamVault Team"
                },
            };
        }

        public EmailTemplate GetTemplate(string templateId)
        {
            if (_templates.TryGetValue(templateId, out var template))
            {
                return template;
            }

            throw new ArgumentException($"Template '{templateId}' not found", nameof(templateId));
        }

        public string RenderTemplate(string templateId, Dictionary<string, object> data)
        {
            var template = GetTemplate(templateId);
            var htmlBody = template.HtmlBody;
            var textBody = template.TextBody;

            foreach (var kvp in data)
            {
                var placeholder = $"{{{{{kvp.Key}}}}}";
                var value = kvp.Value?.ToString() ?? string.Empty;
                
                htmlBody = htmlBody.Replace(placeholder, value);
                textBody = textBody.Replace(placeholder, value);
            }

            return htmlBody;
        }
    }

    public class EmailTemplate
    {
        public string Subject { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
        public string TextBody { get; set; } = string.Empty;
    }

    public class EmailServiceException : Exception
    {
        public EmailServiceException(string message) : base(message) { }
        public EmailServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
