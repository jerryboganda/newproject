using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StreamVault.Infrastructure.Data;
using StreamVault.Application.Auth;
using StreamVault.Application.Services;
using StreamVault.Application.Videos;
using StreamVault.Application.Comments;
using StreamVault.Application.Playlists;
using StreamVault.Application.Subscriptions;
using StreamVault.Application.Admin;
using StreamVault.Application.Sharing;
using StreamVault.Application.Recommendations;
using StreamVault.Application.Payments;
using StreamVault.Application.Chapters;
using StreamVault.Application.Transcripts;
using StreamVault.Application.Embed;
using StreamVault.Application.Notifications;
using StreamVault.Application.Emails;
using StreamVault.Application.Annotations;
using StreamVault.Application.VideoSettings;
using StreamVault.Application.Thumbnails;
using StreamVault.Application.LiveStreaming;
using StreamVault.Application.Analytics;
using StreamVault.Application.SEO;
using StreamVault.Application.Monetization;

namespace StreamVault.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database context
        services.AddDbContext<StreamVaultDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Authentication service
        services.AddScoped<IAuthService, AuthService>();

        // Email service
        services.AddScoped<IEmailService, EmailService>();

        // Storage service
        services.AddScoped<IStorageService, StorageService>();

        // Video service
        services.AddScoped<IVideoService, VideoService>();

        // Background job service
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();

        // Video processing services
        services.AddScoped<IVideoTranscodingService, VideoTranscodingService>();
        services.AddScoped<IThumbnailService, ThumbnailService>();
        services.AddScoped<IVideoAnalyticsService, VideoAnalyticsService>();

        // Comment service
        services.AddScoped<ICommentService, CommentService>();

        // Playlist service
        services.AddScoped<IPlaylistService, PlaylistService>();

        // Subscription service
        services.AddScoped<ISubscriptionService, SubscriptionService>();

        // Admin service
        services.AddScoped<IAdminService, AdminService>();

        // Sharing service
        services.AddScoped<ISharingService, SharingService>();

        // Recommendation service
        services.AddScoped<IRecommendationService, RecommendationService>();

        // Payment service
        services.AddScoped<IPaymentService, PaymentService>();

        // Chapter service
        services.AddScoped<IChapterService, ChapterService>();

        // Transcript service
        services.AddScoped<ITranscriptService, TranscriptService>();

        // Embed service
        services.AddScoped<IEmbedService, EmbedService>();

        // Notification service
        services.AddScoped<INotificationService, NotificationService>();

        // Email services
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();

        // Annotation service
        services.AddScoped<IAnnotationService, AnnotationService>();

        // Video settings service
        services.AddScoped<IVideoSettingsService, VideoSettingsService>();

        // Thumbnail generator service
        services.AddScoped<IThumbnailGeneratorService, ThumbnailGeneratorService>();

        // Live streaming service
        services.AddScoped<ILiveStreamingService, LiveStreamingService>();

        // Video analytics service
        services.AddScoped<IVideoAnalyticsDashboardService, VideoAnalyticsDashboardService>();

        // Video SEO service
        services.AddScoped<IVideoSEOService, VideoSEOService>();

        // Monetization services
        services.AddScoped<IVideoMonetizationService, VideoMonetizationService>();
        services.AddScoped<ISubscriptionTierService, SubscriptionTierService>();

        // Payment gateway service
        services.AddScoped<IPaymentGatewayService, StripePaymentGatewayService>();

        // Revenue analytics service
        services.AddScoped<IRevenueAnalyticsService, RevenueAnalyticsService>();

        // Recommendation service
        services.AddScoped<IVideoRecommendationService, VideoRecommendationService>();

        // AI tagging service
        services.AddScoped<IVideoTaggingService, VideoTaggingService>();

        // Moderation service
        services.AddScoped<IVideoModerationService, VideoModerationService>();

        // Video chapter service
        services.AddScoped<IVideoChapterService, VideoChapterService>();

        // Add services here
        
        return services;
    }
}
