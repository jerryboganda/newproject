using StreamVault.Application.Emails.DTOs;

namespace StreamVault.Application.Emails;

public interface IEmailNotificationService
{
    Task SendVideoUploadedEmailAsync(string toEmail, VideoUploadedEmailData data);
    Task SendCommentReceivedEmailAsync(string toEmail, CommentReceivedEmailData data);
    Task SendSubscriptionRenewedEmailAsync(string toEmail, SubscriptionRenewedEmailData data);
    Task SendPaymentReceivedEmailAsync(string toEmail, PaymentReceivedEmailData data);
    Task SendWelcomeEmailAsync(string toEmail, string userName, string loginUrl);
    Task SendPasswordResetEmailAsync(string toEmail, string resetUrl);
    Task SendEmailVerificationEmailAsync(string toEmail, string verificationUrl);
    Task SendCustomEmailAsync(SendEmailRequest request);
}
