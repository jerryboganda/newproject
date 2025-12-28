using StreamVault.Application.Emails.DTOs;

namespace StreamVault.Application.Emails;

public interface IEmailTemplateService
{
    EmailTemplate GetVideoUploadedTemplate(VideoUploadedEmailData data);
    EmailTemplate GetCommentReceivedTemplate(CommentReceivedEmailData data);
    EmailTemplate GetSubscriptionRenewedTemplate(SubscriptionRenewedEmailData data);
    EmailTemplate GetPaymentReceivedTemplate(PaymentReceivedEmailData data);
    EmailTemplate GetWelcomeEmailTemplate(string userName, string loginUrl);
    EmailTemplate GetPasswordResetTemplate(string resetUrl);
    EmailTemplate GetEmailVerificationTemplate(string verificationUrl);
}
