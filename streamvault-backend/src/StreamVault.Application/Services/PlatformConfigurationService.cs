using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Services
{
    public interface IPlatformConfigurationService
    {
        Task<SubscriptionPlan> CreatePlanAsync(CreatePlanRequest request);
        Task<SubscriptionPlan> UpdatePlanAsync(Guid planId, UpdatePlanRequest request);
        Task<SubscriptionPlan?> GetPlanAsync(Guid planId);
        Task<IEnumerable<SubscriptionPlan>> GetPlansAsync(bool includeInactive = false);
        Task DeletePlanAsync(Guid planId);
        Task<OverageRate> CreateOverageRateAsync(CreateOverageRateRequest request);
        Task<OverageRate> UpdateOverageRateAsync(Guid rateId, UpdateOverageRateRequest request);
        Task<OverageRate?> GetOverageRateAsync(Guid rateId);
        Task<IEnumerable<OverageRate>> GetOverageRatesAsync();
        Task DeleteOverageRateAsync(Guid rateId);
        Task<UsageMultiplier> CreateUsageMultiplierAsync(CreateUsageMultiplierRequest request);
        Task<UsageMultiplier> UpdateUsageMultiplierAsync(Guid multiplierId, UpdateUsageMultiplierRequest request);
        Task<UsageMultiplier?> GetUsageMultiplierAsync(Guid multiplierId);
        Task<IEnumerable<UsageMultiplier>> GetUsageMultipliersAsync();
        Task DeleteUsageMultiplierAsync(Guid multiplierId);
        Task<PlatformConfiguration> GetConfigurationAsync();
        Task UpdateConfigurationAsync(PlatformConfiguration configuration);
        Task<FeatureFlag> CreateFeatureFlagAsync(CreateFeatureFlagRequest request);
        Task<FeatureFlag> UpdateFeatureFlagAsync(Guid flagId, UpdateFeatureFlagRequest request);
        Task<FeatureFlag?> GetFeatureFlagAsync(string key);
        Task<IEnumerable<FeatureFlag>> GetFeatureFlagsAsync();
        Task<bool> IsFeatureEnabledAsync(string key, Guid? tenantId = null);
        Task<SystemNotification> CreateSystemNotificationAsync(CreateSystemNotificationRequest request);
        Task<SystemNotification> UpdateSystemNotificationAsync(Guid notificationId, UpdateSystemNotificationRequest request);
        Task<SystemNotification?> GetSystemNotificationAsync(Guid notificationId);
        Task<IEnumerable<SystemNotification>> GetSystemNotificationsAsync(bool activeOnly = true);
        Task DeleteSystemNotificationAsync(Guid notificationId);
        Task<PlatformMetrics> GetPlatformMetricsAsync();
    }

    public class PlatformConfigurationService : IPlatformConfigurationService
    {
        private readonly StreamVaultDbContext _context;
        private readonly ICacheService _cacheService;

        public PlatformConfigurationService(StreamVaultDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<SubscriptionPlan> CreatePlanAsync(CreatePlanRequest request)
        {
            // Verify Stripe price exists
            var stripePrice = await GetStripePriceAsync(request.StripePriceId);
            if (stripePrice == null)
                throw new ArgumentException("Invalid Stripe price ID", nameof(request.StripePriceId));

            var plan = new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Currency = request.Currency,
                BillingInterval = request.BillingInterval,
                StripePriceId = request.StripePriceId,
                Features = request.Features ?? new List<string>(),
                Limits = new PlanLimits
                {
                    StorageLimitGB = request.StorageLimitGB,
                    BandwidthLimitGB = request.BandwidthLimitGB,
                    VideoLimit = request.VideoLimit,
                    UserLimit = request.UserLimit,
                    ApiCallsLimit = request.ApiCallsLimit
                },
                OverageRates = new PlanOverageRates
                {
                    StorageOveragePricePerGB = request.StorageOveragePricePerGB,
                    BandwidthOveragePricePerGB = request.BandwidthOveragePricePerGB,
                    VideoOveragePrice = request.VideoOveragePrice,
                    ApiCallsOveragePrice = request.ApiCallsOveragePrice
                },
                IsActive = true,
                IsPublic = request.IsPublic,
                TrialPeriodDays = request.TrialPeriodDays,
                SortOrder = request.SortOrder ?? 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SubscriptionPlans.Add(plan);
            await _context.SaveChangesAsync();

            // Clear cache
            await _cacheService.RemoveAsync("plans:all");

            return plan;
        }

        public async Task<SubscriptionPlan> UpdatePlanAsync(Guid planId, UpdatePlanRequest request)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null)
                throw new ArgumentException("Plan not found", nameof(planId));

            if (request.Name != null) plan.Name = request.Name;
            if (request.Description != null) plan.Description = request.Description;
            if (request.Price.HasValue) plan.Price = request.Price.Value;
            if (request.Currency != null) plan.Currency = request.Currency;
            if (request.BillingInterval.HasValue) plan.BillingInterval = request.BillingInterval.Value;
            if (request.StripePriceId != null) plan.StripePriceId = request.StripePriceId;
            if (request.Features != null) plan.Features = request.Features;
            if (request.IsActive.HasValue) plan.IsActive = request.IsActive.Value;
            if (request.IsPublic.HasValue) plan.IsPublic = request.IsPublic.Value;
            if (request.TrialPeriodDays.HasValue) plan.TrialPeriodDays = request.TrialPeriodDays.Value;
            if (request.SortOrder.HasValue) plan.SortOrder = request.SortOrder.Value;

            if (request.StorageLimitGB.HasValue) plan.Limits.StorageLimitGB = request.StorageLimitGB.Value;
            if (request.BandwidthLimitGB.HasValue) plan.Limits.BandwidthLimitGB = request.BandwidthLimitGB.Value;
            if (request.VideoLimit.HasValue) plan.Limits.VideoLimit = request.VideoLimit.Value;
            if (request.UserLimit.HasValue) plan.Limits.UserLimit = request.UserLimit.Value;
            if (request.ApiCallsLimit.HasValue) plan.Limits.ApiCallsLimit = request.ApiCallsLimit.Value;

            if (request.StorageOveragePricePerGB.HasValue) plan.OverageRates.StorageOveragePricePerGB = request.StorageOveragePricePerGB.Value;
            if (request.BandwidthOveragePricePerGB.HasValue) plan.OverageRates.BandwidthOveragePricePerGB = request.BandwidthOveragePricePerGB.Value;
            if (request.VideoOveragePrice.HasValue) plan.OverageRates.VideoOveragePrice = request.VideoOveragePrice.Value;
            if (request.ApiCallsOveragePrice.HasValue) plan.OverageRates.ApiCallsOveragePrice = request.ApiCallsOveragePrice.Value;

            plan.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Clear cache
            await _cacheService.RemoveAsync("plans:all");

            return plan;
        }

        public async Task<SubscriptionPlan?> GetPlanAsync(Guid planId)
        {
            return await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Id == planId);
        }

        public async Task<IEnumerable<SubscriptionPlan>> GetPlansAsync(bool includeInactive = false)
        {
            var query = _context.SubscriptionPlans.AsQueryable();

            if (!includeInactive)
                query = query.Where(p => p.IsActive);

            return await query
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.Price)
                .ToListAsync();
        }

        public async Task DeletePlanAsync(Guid planId)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null)
                throw new ArgumentException("Plan not found", nameof(planId));

            // Check if any active subscriptions use this plan
            var activeSubscriptions = await _context.TenantSubscriptions
                .AnyAsync(s => s.PlanId == planId && s.Status == SubscriptionStatus.Active);

            if (activeSubscriptions)
                throw new InvalidOperationException("Cannot delete plan with active subscriptions");

            plan.IsActive = false;
            plan.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Clear cache
            await _cacheService.RemoveAsync("plans:all");
        }

        public async Task<OverageRate> CreateOverageRateAsync(CreateOverageRateRequest request)
        {
            var rate = new OverageRate
            {
                Id = Guid.NewGuid(),
                MetricType = request.MetricType,
                UnitPrice = request.UnitPrice,
                Unit = request.Unit,
                Currency = request.Currency,
                Tiers = request.Tiers ?? new List<OverageTier>(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.OverageRates.Add(rate);
            await _context.SaveChangesAsync();

            return rate;
        }

        public async Task<OverageRate> UpdateOverageRateAsync(Guid rateId, UpdateOverageRateRequest request)
        {
            var rate = await _context.OverageRates.FindAsync(rateId);
            if (rate == null)
                throw new ArgumentException("Overage rate not found", nameof(rateId));

            if (request.UnitPrice.HasValue) rate.UnitPrice = request.UnitPrice.Value;
            if (request.Unit != null) rate.Unit = request.Unit;
            if (request.Currency != null) rate.Currency = request.Currency;
            if (request.Tiers != null) rate.Tiers = request.Tiers;
            if (request.IsActive.HasValue) rate.IsActive = request.IsActive.Value;

            rate.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return rate;
        }

        public async Task<OverageRate?> GetOverageRateAsync(Guid rateId)
        {
            return await _context.OverageRates
                .FirstOrDefaultAsync(r => r.Id == rateId);
        }

        public async Task<IEnumerable<OverageRate>> GetOverageRatesAsync()
        {
            return await _context.OverageRates
                .Where(r => r.IsActive)
                .OrderBy(r => r.MetricType)
                .ToListAsync();
        }

        public async Task DeleteOverageRateAsync(Guid rateId)
        {
            var rate = await _context.OverageRates.FindAsync(rateId);
            if (rate == null)
                throw new ArgumentException("Overage rate not found", nameof(rateId));

            rate.IsActive = false;
            rate.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<UsageMultiplier> CreateUsageMultiplierAsync(CreateUsageMultiplierRequest request)
        {
            var multiplier = new UsageMultiplier
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                MetricType = request.MetricType,
                Multiplier = request.Multiplier,
                Conditions = request.Conditions ?? new Dictionary<string, object>(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UsageMultipliers.Add(multiplier);
            await _context.SaveChangesAsync();

            return multiplier;
        }

        public async Task<UsageMultiplier> UpdateUsageMultiplierAsync(Guid multiplierId, UpdateUsageMultiplierRequest request)
        {
            var multiplier = await _context.UsageMultipliers.FindAsync(multiplierId);
            if (multiplier == null)
                throw new ArgumentException("Usage multiplier not found", nameof(multiplierId));

            if (request.Name != null) multiplier.Name = request.Name;
            if (request.Multiplier.HasValue) multiplier.Multiplier = request.Multiplier.Value;
            if (request.Conditions != null) multiplier.Conditions = request.Conditions;
            if (request.IsActive.HasValue) multiplier.IsActive = request.IsActive.Value;

            multiplier.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return multiplier;
        }

        public async Task<UsageMultiplier?> GetUsageMultiplierAsync(Guid multiplierId)
        {
            return await _context.UsageMultipliers
                .FirstOrDefaultAsync(m => m.Id == multiplierId);
        }

        public async Task<IEnumerable<UsageMultiplier>> GetUsageMultipliersAsync()
        {
            return await _context.UsageMultipliers
                .Where(m => m.IsActive)
                .OrderBy(m => m.MetricType)
                .ThenBy(m => m.Name)
                .ToListAsync();
        }

        public async Task DeleteUsageMultiplierAsync(Guid multiplierId)
        {
            var multiplier = await _context.UsageMultipliers.FindAsync(multiplierId);
            if (multiplier == null)
                throw new ArgumentException("Usage multiplier not found", nameof(multiplierId));

            multiplier.IsActive = false;
            multiplier.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<PlatformConfiguration> GetConfigurationAsync()
        {
            var config = await _context.PlatformConfigurations.FirstOrDefaultAsync();
            if (config == null)
            {
                // Create default configuration
                config = new PlatformConfiguration
                {
                    Id = Guid.NewGuid(),
                    PlatformName = "StreamVault",
                    PlatformUrl = "https://streamvault.example.com",
                    SupportEmail = "support@streamvault.example.com",
                    DefaultLanguage = "en",
                    AllowedLanguages = new List<string> { "en", "es", "fr", "de", "zh" },
                    DefaultTimezone = "UTC",
                    MaintenanceMode = false,
                    RegistrationEnabled = true,
                    TrialDays = 14,
                    MaxFileSizeMB = 2048,
                    SupportedVideoFormats = new List<string> { "mp4", "avi", "mov", "wmv", "flv", "webm" },
                    StorageProvider = "bunny",
                    CdnProvider = "bunny",
                    EmailProvider = "sendgrid",
                    PaymentProvider = "stripe",
                    AnalyticsProvider = "google",
                    Settings = new Dictionary<string, object>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PlatformConfigurations.Add(config);
                await _context.SaveChangesAsync();
            }

            return config;
        }

        public async Task UpdateConfigurationAsync(PlatformConfiguration configuration)
        {
            var existingConfig = await _context.PlatformConfigurations.FirstOrDefaultAsync();
            if (existingConfig == null)
            {
                configuration.Id = Guid.NewGuid();
                configuration.CreatedAt = DateTime.UtcNow;
                _context.PlatformConfigurations.Add(configuration);
            }
            else
            {
                existingConfig.PlatformName = configuration.PlatformName;
                existingConfig.PlatformUrl = configuration.PlatformUrl;
                existingConfig.SupportEmail = configuration.SupportEmail;
                existingConfig.DefaultLanguage = configuration.DefaultLanguage;
                existingConfig.AllowedLanguages = configuration.AllowedLanguages;
                existingConfig.DefaultTimezone = configuration.DefaultTimezone;
                existingConfig.MaintenanceMode = configuration.MaintenanceMode;
                existingConfig.RegistrationEnabled = configuration.RegistrationEnabled;
                existingConfig.TrialDays = configuration.TrialDays;
                existingConfig.MaxFileSizeMB = configuration.MaxFileSizeMB;
                existingConfig.SupportedVideoFormats = configuration.SupportedVideoFormats;
                existingConfig.StorageProvider = configuration.StorageProvider;
                existingConfig.CdnProvider = configuration.CdnProvider;
                existingConfig.EmailProvider = configuration.EmailProvider;
                existingConfig.PaymentProvider = configuration.PaymentProvider;
                existingConfig.AnalyticsProvider = configuration.AnalyticsProvider;
                existingConfig.Settings = configuration.Settings;
                existingConfig.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Clear cache
            await _cacheService.RemoveAsync("platform:config");
        }

        public async Task<FeatureFlag> CreateFeatureFlagAsync(CreateFeatureFlagRequest request)
        {
            var flag = new FeatureFlag
            {
                Id = Guid.NewGuid(),
                Key = request.Key,
                Name = request.Name,
                Description = request.Description,
                IsEnabled = request.IsEnabled,
                TargetTenants = request.TargetTenants ?? new List<Guid>(),
                Conditions = request.Conditions ?? new Dictionary<string, object>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.FeatureFlags.Add(flag);
            await _context.SaveChangesAsync();

            // Clear cache
            await _cacheService.RemoveAsync($"feature:{request.Key}");

            return flag;
        }

        public async Task<FeatureFlag> UpdateFeatureFlagAsync(Guid flagId, UpdateFeatureFlagRequest request)
        {
            var flag = await _context.FeatureFlags.FindAsync(flagId);
            if (flag == null)
                throw new ArgumentException("Feature flag not found", nameof(flagId));

            if (request.Name != null) flag.Name = request.Name;
            if (request.Description != null) flag.Description = request.Description;
            if (request.IsEnabled.HasValue) flag.IsEnabled = request.IsEnabled.Value;
            if (request.TargetTenants != null) flag.TargetTenants = request.TargetTenants;
            if (request.Conditions != null) flag.Conditions = request.Conditions;

            flag.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Clear cache
            await _cacheService.RemoveAsync($"feature:{flag.Key}");

            return flag;
        }

        public async Task<FeatureFlag?> GetFeatureFlagAsync(string key)
        {
            return await _context.FeatureFlags
                .FirstOrDefaultAsync(f => f.Key == key);
        }

        public async Task<IEnumerable<FeatureFlag>> GetFeatureFlagsAsync()
        {
            return await _context.FeatureFlags
                .OrderBy(f => f.Key)
                .ToListAsync();
        }

        public async Task<bool> IsFeatureEnabledAsync(string key, Guid? tenantId = null)
        {
            // Check cache first
            var cacheKey = $"feature:{key}:{tenantId?.ToString() ?? "global"}";
            var cached = await _cacheService.GetAsync<bool>(cacheKey);
            if (cached.HasValue)
                return cached.Value;

            var flag = await GetFeatureFlagAsync(key);
            if (flag == null)
                return false;

            bool isEnabled = flag.IsEnabled;

            // Check tenant-specific targeting
            if (isEnabled && tenantId.HasValue && flag.TargetTenants.Any())
            {
                isEnabled = flag.TargetTenants.Contains(tenantId.Value);
            }

            // Check conditions
            if (isEnabled && flag.Conditions.Any())
            {
                // Evaluate conditions based on tenant properties
                // This is a simplified version - in production, you'd have more sophisticated condition evaluation
                isEnabled = EvaluateConditions(flag.Conditions, tenantId);
            }

            // Cache result for 5 minutes
            await _cacheService.SetAsync(cacheKey, isEnabled, TimeSpan.FromMinutes(5));

            return isEnabled;
        }

        public async Task<SystemNotification> CreateSystemNotificationAsync(CreateSystemNotificationRequest request)
        {
            var notification = new SystemNotification
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                TargetRole = request.TargetRole,
                IsActive = true,
                StartsAt = request.StartsAt ?? DateTime.UtcNow,
                ExpiresAt = request.ExpiresAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SystemNotifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<SystemNotification> UpdateSystemNotificationAsync(Guid notificationId, UpdateSystemNotificationRequest request)
        {
            var notification = await _context.SystemNotifications.FindAsync(notificationId);
            if (notification == null)
                throw new ArgumentException("System notification not found", nameof(notificationId));

            if (request.Title != null) notification.Title = request.Title;
            if (request.Message != null) notification.Message = request.Message;
            if (request.Type.HasValue) notification.Type = request.Type.Value;
            if (request.TargetRole != null) notification.TargetRole = request.TargetRole;
            if (request.IsActive.HasValue) notification.IsActive = request.IsActive.Value;
            if (request.StartsAt.HasValue) notification.StartsAt = request.StartsAt.Value;
            if (request.ExpiresAt.HasValue) notification.ExpiresAt = request.ExpiresAt.Value;

            notification.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<SystemNotification?> GetSystemNotificationAsync(Guid notificationId)
        {
            return await _context.SystemNotifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);
        }

        public async Task<IEnumerable<SystemNotification>> GetSystemNotificationsAsync(bool activeOnly = true)
        {
            var query = _context.SystemNotifications.AsQueryable();

            if (activeOnly)
            {
                var now = DateTime.UtcNow;
                query = query.Where(n => n.IsActive && 
                                        n.StartsAt <= now && 
                                        (!n.ExpiresAt.HasValue || n.ExpiresAt > now));
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task DeleteSystemNotificationAsync(Guid notificationId)
        {
            var notification = await _context.SystemNotifications.FindAsync(notificationId);
            if (notification == null)
                throw new ArgumentException("System notification not found", nameof(notificationId));

            notification.IsActive = false;
            notification.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<PlatformMetrics> GetPlatformMetricsAsync()
        {
            var now = DateTime.UtcNow;
            var last24Hours = now.AddHours(-24);
            var last7Days = now.AddDays(-7);
            var last30Days = now.AddDays(-30);

            return new PlatformMetrics
            {
                TotalTenants = await _context.Tenants.CountAsync(),
                ActiveTenants = await _context.Tenants.CountAsync(t => t.IsActive),
                TotalUsers = await _context.Users.CountAsync(),
                ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                TotalVideos = await _context.Videos.CountAsync(),
                TotalViews = await _context.VideoViews.CountAsync(),
                TotalStorageUsed = await _context.Videos.SumAsync(v => v.FileSize),
                NewTenants24h = await _context.Tenants.CountAsync(t => t.CreatedAt >= last24Hours),
                NewUsers24h = await _context.Users.CountAsync(u => u.CreatedAt >= last24Hours),
                NewVideos24h = await _context.Videos.CountAsync(v => v.CreatedAt >= last24Hours),
                TotalRevenue = await CalculateTotalRevenue(last30Days),
                ActiveSubscriptions = await _context.TenantSubscriptions.CountAsync(s => s.Status == SubscriptionStatus.Active),
                ChurnRate = await CalculateChurnRate(last30Days)
            };
        }

        private async Task<dynamic> GetStripePriceAsync(string priceId)
        {
            // This would call Stripe API to validate the price
            // For now, return a placeholder
            return new { id = priceId, active = true };
        }

        private bool EvaluateConditions(Dictionary<string, object> conditions, Guid? tenantId)
        {
            // Simplified condition evaluation
            // In production, this would be more sophisticated
            return true;
        }

        private async Task<decimal> CalculateTotalRevenue(DateTime since)
        {
            // Calculate from subscription payments
            return 12500.50m; // Placeholder
        }

        private async Task<double> CalculateChurnRate(DateTime period)
        {
            // Calculate customer churn rate
            return 2.5; // Placeholder percentage
        }
    }

    // DTOs
    public class CreatePlanRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public BillingInterval BillingInterval { get; set; }
        public string StripePriceId { get; set; } = null!;
        public List<string>? Features { get; set; }
        public bool IsPublic { get; set; } = true;
        public int? TrialPeriodDays { get; set; }
        public int? SortOrder { get; set; }
        
        // Limits
        public double StorageLimitGB { get; set; }
        public double BandwidthLimitGB { get; set; }
        public int VideoLimit { get; set; }
        public int UserLimit { get; set; }
        public int ApiCallsLimit { get; set; }
        
        // Overage rates
        public decimal StorageOveragePricePerGB { get; set; }
        public decimal BandwidthOveragePricePerGB { get; set; }
        public decimal VideoOveragePrice { get; set; }
        public decimal ApiCallsOveragePrice { get; set; }
    }

    public class UpdatePlanRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Currency { get; set; }
        public BillingInterval? BillingInterval { get; set; }
        public string? StripePriceId { get; set; }
        public List<string>? Features { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsPublic { get; set; }
        public int? TrialPeriodDays { get; set; }
        public int? SortOrder { get; set; }
        
        // Limits
        public double? StorageLimitGB { get; set; }
        public double? BandwidthLimitGB { get; set; }
        public int? VideoLimit { get; set; }
        public int? UserLimit { get; set; }
        public int? ApiCallsLimit { get; set; }
        
        // Overage rates
        public decimal? StorageOveragePricePerGB { get; set; }
        public decimal? BandwidthOveragePricePerGB { get; set; }
        public decimal? VideoOveragePrice { get; set; }
        public decimal? ApiCallsOveragePrice { get; set; }
    }

    public class CreateOverageRateRequest
    {
        public MetricType MetricType { get; set; }
        public decimal UnitPrice { get; set; }
        public string Unit { get; set; } = null!;
        public string Currency { get; set; } = "USD";
        public List<OverageTier>? Tiers { get; set; }
    }

    public class UpdateOverageRateRequest
    {
        public decimal? UnitPrice { get; set; }
        public string? Unit { get; set; }
        public string? Currency { get; set; }
        public List<OverageTier>? Tiers { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CreateUsageMultiplierRequest
    {
        public string Name { get; set; } = null!;
        public MetricType MetricType { get; set; }
        public double Multiplier { get; set; }
        public Dictionary<string, object>? Conditions { get; set; }
    }

    public class UpdateUsageMultiplierRequest
    {
        public string? Name { get; set; }
        public double? Multiplier { get; set; }
        public Dictionary<string, object>? Conditions { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CreateFeatureFlagRequest
    {
        public string Key { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
        public List<Guid>? TargetTenants { get; set; }
        public Dictionary<string, object>? Conditions { get; set; }
    }

    public class UpdateFeatureFlagRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? IsEnabled { get; set; }
        public List<Guid>? TargetTenants { get; set; }
        public Dictionary<string, object>? Conditions { get; set; }
    }

    public class CreateSystemNotificationRequest
    {
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public NotificationType Type { get; set; }
        public string? TargetRole { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class UpdateSystemNotificationRequest
    {
        public string? Title { get; set; }
        public string? Message { get; set; }
        public NotificationType? Type { get; set; }
        public string? TargetRole { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class PlatformMetrics
    {
        public int TotalTenants { get; set; }
        public int ActiveTenants { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalVideos { get; set; }
        public int TotalViews { get; set; }
        public long TotalStorageUsed { get; set; }
        public int NewTenants24h { get; set; }
        public int NewUsers24h { get; set; }
        public int NewVideos24h { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ActiveSubscriptions { get; set; }
        public double ChurnRate { get; set; }
    }

    public enum MetricType
    {
        Storage,
        Bandwidth,
        VideoCount,
        UserCount,
        ApiCalls,
        Views
    }

    public enum BillingInterval
    {
        Monthly,
        Yearly
    }
}
