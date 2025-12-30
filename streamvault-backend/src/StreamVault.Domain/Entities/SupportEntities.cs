using System;
using StreamVault.Domain.Interfaces;
using System.Collections.Generic;

namespace StreamVault.Domain.Entities
{
    public class SupportTicket : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid? AssignedToId { get; set; }
        public User? AssignedTo { get; set; }
        public string Subject { get; set; } = null!;
        public string Description { get; set; } = null!;
        public TicketCategory Category { get; set; }
        public TicketPriority Priority { get; set; }
        public TicketStatus Status { get; set; }
        public string? Resolution { get; set; }
        public string? EscalationReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? EscalatedAt { get; set; }
        public List<SupportTicketReply> Replies { get; set; } = new();
    }

    public class SupportTicketReply : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public SupportTicket Ticket { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public string Content { get; set; } = null!;
        public bool IsInternal { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class KnowledgeBaseArticle : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Summary { get; set; }
        public string Category { get; set; } = null!;
        public List<string> Tags { get; set; } = new();
        public bool IsPublished { get; set; }
        public int Views { get; set; }
        public int HelpfulVotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }
        public Guid? UpdatedByUserId { get; set; }
        public User? UpdatedByUser { get; set; }
    }

    public class CannedResponse : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }
        public string Name { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Category { get; set; } = null!;
        public List<string> Shortcuts { get; set; } = new();
        public bool IsActive { get; set; }
        public int UsageCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;
    }

    public class EmailTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string HtmlContent { get; set; } = null!;
        public string TextContent { get; set; } = null!;
        public string Category { get; set; } = null!;
        public List<string> Variables { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;
    }

    public class UserImpersonationToken
    {
        public Guid Id { get; set; }
        public Guid AdminUserId { get; set; }
        public User AdminUser { get; set; } = null!;
        public Guid TargetUserId { get; set; }
        public User TargetUser { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class SystemSettings
    {
        public Guid Id { get; set; }
        public bool AllowNewRegistrations { get; set; }
        public bool RequireEmailVerification { get; set; }
        public Guid DefaultSubscriptionPlanId { get; set; }
        public int MaxFileSizeMB { get; set; }
        public List<string> SupportedVideoFormats { get; set; } = new();
        public bool MaintenanceMode { get; set; }
        public string? MaintenanceMessage { get; set; }
        public Dictionary<string, object> Settings { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SystemNotification
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public NotificationType Type { get; set; }
        public string? TargetRole { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PlanLimits
    {
        public double StorageLimitGB { get; set; }
        public double BandwidthLimitGB { get; set; }
        public int VideoLimit { get; set; }
        public int UserLimit { get; set; }
        public int ApiCallsLimit { get; set; }
    }

    public class PlanOverageRates
    {
        public decimal StorageOveragePricePerGB { get; set; }
        public decimal BandwidthOveragePricePerGB { get; set; }
        public decimal VideoOveragePrice { get; set; }
        public decimal ApiCallsOveragePrice { get; set; }
    }

    public class OverageRate
    {
        public Guid Id { get; set; }
        public MetricType MetricType { get; set; }
        public decimal UnitPrice { get; set; }
        public string Unit { get; set; } = null!;
        public string Currency { get; set; } = "USD";
        public List<OverageTier> Tiers { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class OverageTier
    {
        public double From { get; set; }
        public double? To { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class UsageMultiplier
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public MetricType MetricType { get; set; }
        public double Multiplier { get; set; }
        public Dictionary<string, object> Conditions { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PlatformConfiguration
    {
        public Guid Id { get; set; }
        public string PlatformName { get; set; } = null!;
        public string PlatformUrl { get; set; } = null!;
        public string SupportEmail { get; set; } = null!;
        public string DefaultLanguage { get; set; } = "en";
        public List<string> AllowedLanguages { get; set; } = new();
        public string DefaultTimezone { get; set; } = "UTC";
        public bool MaintenanceMode { get; set; }
        public string? MaintenanceMessage { get; set; }
        public bool RegistrationEnabled { get; set; }
        public int TrialDays { get; set; }
        public int MaxFileSizeMB { get; set; }
        public List<string> SupportedVideoFormats { get; set; } = new();
        public string StorageProvider { get; set; } = null!;
        public string CdnProvider { get; set; } = null!;
        public string EmailProvider { get; set; } = null!;
        public string PaymentProvider { get; set; } = null!;
        public string AnalyticsProvider { get; set; } = null!;
        public Dictionary<string, object> Settings { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class FeatureFlag
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
        public List<Guid> TargetTenants { get; set; } = new();
        public Dictionary<string, object> Conditions { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public enum TicketStatus
    {
        Open,
        InProgress,
        WaitingForCustomer,
        WaitingForSupport,
        Resolved,
        Closed,
        Reopened,
        Escalated
    }

    public enum TicketCategory
    {
        General,
        Technical,
        Billing,
        Account,
        FeatureRequest,
        BugReport,
        Abuse
    }

    public enum TicketPriority
    {
        Low,
        Normal,
        High,
        Critical,
        Urgent
    }

    public enum BillingInterval
    {
        Monthly,
        Yearly
    }

    public enum MetricType
    {
        Storage,
        Bandwidth,
        VideoCount,
        UserCount,
        ApiCalls,
        Views,
        Downloads
    }
}
