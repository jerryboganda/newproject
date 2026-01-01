using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StreamVault.Application.Emails.DTOs;
using StreamVault.Application.Services;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Infrastructure.Services;

public sealed class TemplatedEmailService : IEmailService
{
    private readonly StreamVaultDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TemplatedEmailService> _logger;

    public TemplatedEmailService(StreamVaultDbContext db, IConfiguration configuration, ILogger<TemplatedEmailService> logger)
    {
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    public Task SendEmailVerificationAsync(string email, string token)
        => SendNamedTemplateAsync(email, "email_verification", new Dictionary<string, object>
        {
            ["email"] = email,
            ["token"] = token,
            ["verify_url"] = BuildFrontendUrl($"/verify-email?token={Uri.EscapeDataString(token)}")
        });

    public Task SendPasswordResetAsync(string email, string token)
        => SendNamedTemplateAsync(email, "password_reset", new Dictionary<string, object>
        {
            ["email"] = email,
            ["token"] = token,
            ["reset_url"] = BuildFrontendUrl($"/forgot-password?token={Uri.EscapeDataString(token)}")
        });

    public Task SendTwoFactorCodeAsync(string email, string code)
        => SendNamedTemplateAsync(email, "two_factor", new Dictionary<string, object>
        {
            ["email"] = email,
            ["code"] = code
        });

    public Task SendEmailAsync(SendEmailRequest request)
        => SendRawAsync(request.To, request.Subject, request.HtmlBody, request.TextBody);

    private async Task SendNamedTemplateAsync(string to, string templateName, Dictionary<string, object> data, CancellationToken cancellationToken = default)
    {
        var template = await _db.EmailTemplates.AsNoTracking()
            .Where(t => t.IsActive && t.Name == templateName)
            .OrderByDescending(t => t.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (template == null)
        {
            _logger.LogWarning("Email template '{TemplateName}' not found; skipping send to {Email}", templateName, to);
            return;
        }

        var subject = Render(template.Subject, data);
        var html = Render(template.HtmlContent, data);
        var text = Render(template.TextContent, data);

        await SendRawAsync(to, subject, html, text, cancellationToken);
    }

    private async Task SendRawAsync(string to, string subject, string htmlBody, string? textBody, CancellationToken cancellationToken = default)
    {
        var smtpHost = _configuration["Email:Smtp:Host"];
        var smtpPortStr = _configuration["Email:Smtp:Port"];
        var smtpUser = _configuration["Email:Smtp:Username"];
        var smtpPass = _configuration["Email:Smtp:Password"];
        var smtpSslStr = _configuration["Email:Smtp:UseSsl"];

        var fromEmail = _configuration["Email:From"];
        var fromName = _configuration["Email:FromName"] ?? "StreamVault";

        if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(fromEmail))
        {
            _logger.LogWarning("SMTP not configured (Email:Smtp:Host, Email:From). Email to {Email} with subject '{Subject}' was not sent.", to, subject);
            return;
        }

        var port = 587;
        if (int.TryParse(smtpPortStr, out var parsedPort)) port = parsedPort;

        var useSsl = true;
        if (bool.TryParse(smtpSslStr, out var parsedSsl)) useSsl = parsedSsl;

#pragma warning disable SYSLIB0014
        using var client = new SmtpClient(smtpHost, port)
        {
            EnableSsl = useSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
#pragma warning restore SYSLIB0014

        if (!string.IsNullOrWhiteSpace(smtpUser))
            client.Credentials = new NetworkCredential(smtpUser, smtpPass);

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(to));

        if (!string.IsNullOrWhiteSpace(textBody))
        {
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody, null, "text/plain"));
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html"));
        }

        // SmtpClient has no CancellationToken support; best-effort.
        await client.SendMailAsync(message);

        _logger.LogInformation("Sent email to {Email} with subject '{Subject}'", to, subject);
    }

    private string BuildFrontendUrl(string path)
    {
        var baseUrl = (_configuration["Frontend:BaseUrl"] ?? "http://localhost:3000").TrimEnd('/');
        if (!path.StartsWith('/')) path = "/" + path;
        return baseUrl + path;
    }

    private static string Render(string template, Dictionary<string, object> data)
    {
        var result = template ?? string.Empty;
        foreach (var kvp in data)
        {
            var placeholder = $"{{{{{kvp.Key}}}}}";
            result = result.Replace(placeholder, kvp.Value?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
        return result;
    }
}
