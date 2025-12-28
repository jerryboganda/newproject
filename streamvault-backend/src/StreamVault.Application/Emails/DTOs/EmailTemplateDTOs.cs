using System.ComponentModel.DataAnnotations;

namespace StreamVault.Application.Emails.DTOs;

public class EmailTemplate
{
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string TextBody { get; set; } = string.Empty;
}

public class SendEmailRequest
{
    [Required]
    public string To { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string HtmlBody { get; set; } = string.Empty;

    public string? TextBody { get; set; }

    public Dictionary<string, string> TemplateData { get; set; } = new();
}

public class VideoUploadedEmailData
{
    public string UserName { get; set; } = string.Empty;
    public string VideoTitle { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
}

public class CommentReceivedEmailData
{
    public string UserName { get; set; } = string.Empty;
    public string CommenterName { get; set; } = string.Empty;
    public string VideoTitle { get; set; } = string.Empty;
    public string CommentText { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
}

public class SubscriptionRenewedEmailData
{
    public string UserName { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string NextBillingDate { get; set; } = string.Empty;
    public string ManageSubscriptionUrl { get; set; } = string.Empty;
}

public class PaymentReceivedEmailData
{
    public string UserName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string TransactionDate { get; set; } = string.Empty;
    public string InvoiceUrl { get; set; } = string.Empty;
}
