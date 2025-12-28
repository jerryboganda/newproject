using StreamVault.Application.Emails.DTOs;

namespace StreamVault.Application.Emails;

public class EmailTemplateService : IEmailTemplateService
{
    public EmailTemplate GetVideoUploadedTemplate(VideoUploadedEmailData data)
    {
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Video Uploaded Successfully</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h1 style='color: #00adef;'>Your Video Has Been Uploaded!</h1>
        <p>Hi {data.UserName},</p>
        <p>Great news! Your video <strong>{data.VideoTitle}</strong> has been successfully uploaded and is now being processed.</p>
        
        <div style='margin: 20px 0; text-align: center;'>
            <img src='{data.ThumbnailUrl}' alt='Video thumbnail' style='max-width: 300px; border-radius: 8px;'>
        </div>
        
        <p>You'll receive another notification once processing is complete and your video is ready to view.</p>
        
        <div style='margin: 30px 0; text-align: center;'>
            <a href='{data.VideoUrl}' style='background-color: #00adef; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                View Video
            </a>
        </div>
        
        <p>Thank you for using StreamVault!</p>
        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
        <p style='font-size: 12px; color: #666;'>This is an automated message. Please do not reply to this email.</p>
    </div>
</body>
</html>";

        var textBody = $@"
Your Video Has Been Uploaded!

Hi {data.UserName},

Great news! Your video '{data.VideoTitle}' has been successfully uploaded and is now being processed.

You'll receive another notification once processing is complete and your video is ready to view.

View Video: {data.VideoUrl}

Thank you for using StreamVault!
";

        return new EmailTemplate
        {
            Subject = "Your Video Has Been Uploaded Successfully",
            HtmlBody = htmlBody,
            TextBody = textBody
        };
    }

    public EmailTemplate GetCommentReceivedTemplate(CommentReceivedEmailData data)
    {
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>New Comment on Your Video</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h1 style='color: #00adef;'>New Comment on Your Video</h1>
        <p>Hi {data.UserName},</p>
        <p><strong>{data.CommenterName}</strong> commented on your video <strong>{data.VideoTitle}</strong>:</p>
        
        <div style='margin: 20px 0; padding: 15px; background-color: #f5f5f5; border-left: 4px solid #00adef;'>
            <p style='margin: 0;'>{data.CommentText}</p>
        </div>
        
        <div style='margin: 30px 0; text-align: center;'>
            <a href='{data.VideoUrl}' style='background-color: #00adef; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                View Comment
            </a>
        </div>
        
        <p>Keep engaging with your audience!</p>
        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
        <p style='font-size: 12px; color: #666;'>This is an automated message. Please do not reply to this email.</p>
    </div>
</body>
</html>";

        var textBody = $@"
New Comment on Your Video

Hi {data.UserName},

{data.CommenterName} commented on your video '{data.VideoTitle}':

'{data.CommentText}'

View Comment: {data.VideoUrl}

Keep engaging with your audience!
";

        return new EmailTemplate
        {
            Subject = "New Comment on Your Video",
            HtmlBody = htmlBody,
            TextBody = textBody
        };
    }

    public EmailTemplate GetSubscriptionRenewedTemplate(SubscriptionRenewedEmailData data)
    {
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Subscription Renewed</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h1 style='color: #00adef;'>Subscription Successfully Renewed</h1>
        <p>Hi {data.UserName},</p>
        <p>Your <strong>{data.PlanName}</strong> subscription has been successfully renewed!</p>
        
        <div style='margin: 20px 0; padding: 15px; background-color: #f5f5f5; border-radius: 5px;'>
            <p><strong>Next billing date:</strong> {data.NextBillingDate}</p>
        </div>
        
        <p>You can manage your subscription settings or update your payment method anytime.</p>
        
        <div style='margin: 30px 0; text-align: center;'>
            <a href='{data.ManageSubscriptionUrl}' style='background-color: #00adef; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                Manage Subscription
            </a>
        </div>
        
        <p>Thank you for your continued support!</p>
        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
        <p style='font-size: 12px; color: #666;'>This is an automated message. Please do not reply to this email.</p>
    </div>
</body>
</html>";

        var textBody = $@"
Subscription Successfully Renewed

Hi {data.UserName},

Your {data.PlanName} subscription has been successfully renewed!

Next billing date: {data.NextBillingDate}

You can manage your subscription settings or update your payment method anytime:
{data.ManageSubscriptionUrl}

Thank you for your continued support!
";

        return new EmailTemplate
        {
            Subject = "Subscription Successfully Renewed",
            HtmlBody = htmlBody,
            TextBody = textBody
        };
    }

    public EmailTemplate GetPaymentReceivedTemplate(PaymentReceivedEmailData data)
    {
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Payment Received</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h1 style='color: #00adef;'>Payment Received</h1>
        <p>Hi {data.UserName},</p>
        <p>We've successfully received your payment of <strong>{data.Amount} {data.Currency}</strong>.</p>
        
        <div style='margin: 20px 0; padding: 15px; background-color: #f5f5f5; border-radius: 5px;'>
            <p><strong>Transaction Date:</strong> {data.TransactionDate}</p>
            <p><strong>Amount:</strong> {data.Amount} {data.Currency}</p>
        </div>
        
        <p>A receipt for this transaction is available in your account.</p>
        
        <div style='margin: 30px 0; text-align: center;'>
            <a href='{data.InvoiceUrl}' style='background-color: #00adef; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                View Invoice
            </a>
        </div>
        
        <p>Thank you for your payment!</p>
        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
        <p style='font-size: 12px; color: #666;'>This is an automated message. Please do not reply to this email.</p>
    </div>
</body>
</html>";

        var textBody = $@"
Payment Received

Hi {data.UserName},

We've successfully received your payment of {data.Amount} {data.Currency}.

Transaction Date: {data.TransactionDate}
Amount: {data.Amount} {data.Currency}

A receipt for this transaction is available in your account:
{data.InvoiceUrl}

Thank you for your payment!
";

        return new EmailTemplate
        {
            Subject = "Payment Received",
            HtmlBody = htmlBody,
            TextBody = textBody
        };
    }

    public EmailTemplate GetWelcomeEmailTemplate(string userName, string loginUrl)
    {
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Welcome to StreamVault</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h1 style='color: #00adef;'>Welcome to StreamVault!</h1>
        <p>Hi {userName},</p>
        <p>Thank you for joining StreamVault! We're excited to have you on board.</p>
        
        <p>StreamVault is your platform for hosting and sharing videos with ease. Whether you're a content creator, educator, or business, we've got you covered.</p>
        
        <div style='margin: 30px 0; text-align: center;'>
            <a href='{loginUrl}' style='background-color: #00adef; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                Get Started
            </a>
        </div>
        
        <p>If you have any questions, feel free to reach out to our support team.</p>
        <p>Happy streaming!</p>
        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
        <p style='font-size: 12px; color: #666;'>This is an automated message. Please do not reply to this email.</p>
    </div>
</body>
</html>";

        var textBody = $@"
Welcome to StreamVault!

Hi {userName},

Thank you for joining StreamVault! We're excited to have you on board.

StreamVault is your platform for hosting and sharing videos with ease. Whether you're a content creator, educator, or business, we've got you covered.

Get Started: {loginUrl}

If you have any questions, feel free to reach out to our support team.

Happy streaming!
";

        return new EmailTemplate
        {
            Subject = "Welcome to StreamVault!",
            HtmlBody = htmlBody,
            TextBody = textBody
        };
    }

    public EmailTemplate GetPasswordResetTemplate(string resetUrl)
    {
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Reset Your Password</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h1 style='color: #00adef;'>Reset Your Password</h1>
        <p>Hi,</p>
        <p>We received a request to reset your password for your StreamVault account.</p>
        <p>Click the button below to reset your password. This link is valid for 24 hours.</p>
        
        <div style='margin: 30px 0; text-align: center;'>
            <a href='{resetUrl}' style='background-color: #00adef; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                Reset Password
            </a>
        </div>
        
        <p>If you didn't request this password reset, please ignore this email. Your password will remain unchanged.</p>
        <p>For security reasons, please don't share this link with anyone.</p>
        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
        <p style='font-size: 12px; color: #666;'>This is an automated message. Please do not reply to this email.</p>
    </div>
</body>
</html>";

        var textBody = $@"
Reset Your Password

Hi,

We received a request to reset your password for your StreamVault account.

Click the link below to reset your password. This link is valid for 24 hours:
{resetUrl}

If you didn't request this password reset, please ignore this email. Your password will remain unchanged.

For security reasons, please don't share this link with anyone.
";

        return new EmailTemplate
        {
            Subject = "Reset Your StreamVault Password",
            HtmlBody = htmlBody,
            TextBody = textBody
        };
    }

    public EmailTemplate GetEmailVerificationTemplate(string verificationUrl)
    {
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Verify Your Email Address</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h1 style='color: #00adef;'>Verify Your Email Address</h1>
        <p>Hi,</p>
        <p>Thanks for signing up for StreamVault! To complete your registration, please verify your email address.</p>
        <p>Click the button below to verify your email:</p>
        
        <div style='margin: 30px 0; text-align: center;'>
            <a href='{verificationUrl}' style='background-color: #00adef; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                Verify Email
            </a>
        </div>
        
        <p>If you didn't create an account with StreamVault, you can safely ignore this email.</p>
        <p>This verification link will expire in 24 hours.</p>
        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
        <p style='font-size: 12px; color: #666;'>This is an automated message. Please do not reply to this email.</p>
    </div>
</body>
</html>";

        var textBody = $@"
Verify Your Email Address

Hi,

Thanks for signing up for StreamVault! To complete your registration, please verify your email address.

Click the link below to verify your email:
{verificationUrl}

If you didn't create an account with StreamVault, you can safely ignore this email.

This verification link will expire in 24 hours.
";

        return new EmailTemplate
        {
            Subject = "Verify Your StreamVault Email Address",
            HtmlBody = htmlBody,
            TextBody = textBody
        };
    }
}
