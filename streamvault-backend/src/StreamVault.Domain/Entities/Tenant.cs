using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StreamVault.Domain.Entities;

public class Tenant
{
    [Key]
    public Guid Id { get; private set; } = Guid.NewGuid();

    [Required, MaxLength(255)]
    public string Name { get; private set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Slug { get; private set; } = string.Empty; // For subdomain

    [MaxLength(500)]
    public string? Description { get; private set; }

    [MaxLength(500)]
    public string? LogoUrl { get; private set; }

    [MaxLength(7)]
    public string? PrimaryColor { get; private set; }

    [MaxLength(7)]
    public string? SecondaryColor { get; private set; }

    [MaxLength(255)]
    public string? CustomDomain { get; private set; }

    public TenantStatus Status { get; private set; } = TenantStatus.Trial;

    public DatabaseType DatabaseType { get; private set; } = DatabaseType.Shared;

    public string? DedicatedDbConn { get; private set; } // Encrypted

    public string? BunnyLibraryId { get; set; } // Encrypted

    public string? BunnyApiKey { get; set; } // Encrypted

    public string? BunnyPullZoneId { get; set; }

    public string? BunnyCdnHostname { get; set; }

    public string? StripeCustomerId { get; private set; }

    public string? SubscriptionId { get; private set; }

    public Guid? PlanId { get; private set; }

    public BillingCycle BillingCycle { get; private set; } = BillingCycle.Monthly;

    public bool IsWhiteLabel { get; private set; } = false;

    public DateTimeOffset? TrialEndsAt { get; private set; }

    public DateTimeOffset? SuspendedAt { get; private set; }

    [MaxLength(1000)]
    public string? SuspensionReason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Dictionary<string, object>? Settings { get; private set; } = new Dictionary<string, object>(); // JSONB

    // Navigation
    public virtual TenantBranding? Branding { get; private set; }

    public virtual ICollection<TenantSubscription> Subscriptions { get; private set; } = new List<TenantSubscription>();

    public virtual ICollection<User> Users { get; private set; } = new List<User>();

    public virtual ICollection<Video> Videos { get; private set; } = new List<Video>();

    public virtual ICollection<Collection> Collections { get; private set; } = new List<Collection>();

    public virtual ICollection<Invoice> Invoices { get; private set; } = new List<Invoice>();

    public virtual ICollection<SupportTicket> SupportTickets { get; private set; } = new List<SupportTicket>();

    public virtual ICollection<WebhookSubscription> WebhookSubscriptions { get; private set; } = new List<WebhookSubscription>();

    public virtual ICollection<ApiKey> ApiKeys { get; private set; } = new List<ApiKey>();

    public virtual ICollection<EmbedDomain> EmbedDomains { get; private set; } = new List<EmbedDomain>();

    public virtual ICollection<UsageMultiplier> UsageMultipliers { get; private set; } = new List<UsageMultiplier>();

    private Tenant() { }

    public Tenant(string name, string slug, Guid? planId = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Slug = slug ?? throw new ArgumentNullException(nameof(slug));
        PlanId = planId;
        Status = TenantStatus.Trial;
        DatabaseType = DatabaseType.Shared;
        BillingCycle = BillingCycle.Monthly;
        IsWhiteLabel = false;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateName(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateBranding(string? logoUrl, string? primaryColor, string? secondaryColor)
    {
        LogoUrl = logoUrl;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetCustomDomain(string? customDomain)
    {
        CustomDomain = customDomain?.ToLowerInvariant();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        Status = TenantStatus.Active;
        SuspendedAt = null;
        SuspensionReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Suspend(string reason)
    {
        Status = TenantStatus.Suspended;
        SuspendedAt = DateTimeOffset.UtcNow;
        SuspensionReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void StartTrial(int trialDays)
    {
        TrialEndsAt = DateTimeOffset.UtcNow.AddDays(trialDays);
        Status = TenantStatus.Trial;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPlan(Guid planId, BillingCycle billingCycle)
    {
        PlanId = planId;
        BillingCycle = billingCycle;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void EnableWhiteLabel()
    {
        IsWhiteLabel = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void DisableWhiteLabel()
    {
        IsWhiteLabel = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateSettings(Dictionary<string, object> settings)
    {
        Settings = settings ?? new Dictionary<string, object>();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetBunnyConfiguration(string libraryId, string apiKey)
    {
        BunnyLibraryId = libraryId;
        BunnyApiKey = apiKey;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetStripeCustomerId(string stripeCustomerId)
    {
        StripeCustomerId = stripeCustomerId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetSubscriptionId(string subscriptionId)
    {
        SubscriptionId = subscriptionId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsInTrial()
    {
        return Status == TenantStatus.Trial && TrialEndsAt.HasValue && TrialEndsAt > DateTimeOffset.UtcNow;
    }

    public bool IsSuspended()
    {
        return Status == TenantStatus.Suspended;
    }

    public bool IsActive()
    {
        return Status == TenantStatus.Active || (Status == TenantStatus.Trial && IsInTrial());
    }
}

public enum TenantStatus
{
    Trial,
    Active,
    Suspended,
    Cancelled
}

public enum BillingCycle
{
    Monthly = 1,
    Yearly = 2
}

public enum DatabaseType
{
    Shared,
    Dedicated
}
