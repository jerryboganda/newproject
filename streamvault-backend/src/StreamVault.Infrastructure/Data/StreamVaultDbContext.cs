using Microsoft.EntityFrameworkCore;
using StreamVault.Domain.Entities;

namespace StreamVault.Infrastructure.Data;

public class StreamVaultDbContext : DbContext
{
    public StreamVaultDbContext(DbContextOptions<StreamVaultDbContext> options) : base(options) { }

    // Master DB tables
    public DbSet<Tenant> Tenants { get; set; } = null!;

    public DbSet<TenantBranding> TenantBrandings { get; set; } = null!;

    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;

    public DbSet<TenantSubscription> TenantSubscriptions { get; set; } = null!;

    public DbSet<User> Users { get; set; } = null!;

    public DbSet<Notification> Notifications { get; set; } = null!;

    public DbSet<Role> Roles { get; set; } = null!;

    public DbSet<Permission> Permissions { get; set; } = null!;

    public DbSet<UserRole> UserRoles { get; set; } = null!;

    public DbSet<RolePermission> RolePermissions { get; set; } = null!;

    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; } = null!;

    public DbSet<TwoFactorAuthCode> TwoFactorAuthCodes { get; set; } = null!;

    // Video-related entities
    public DbSet<Video> Videos { get; set; } = null!;

    public DbSet<VideoTag> VideoTags { get; set; } = null!;

    public DbSet<VideoProcessingJob> VideoProcessingJobs { get; set; } = null!;

    public DbSet<VideoCategory> VideoCategories { get; set; } = null!;

    public DbSet<VideoChapter> VideoChapters { get; set; } = null!;

    public DbSet<VideoTranscript> VideoTranscripts { get; set; } = null!;

    public DbSet<VideoAnnotation> VideoAnnotations { get; set; } = null!;

    public DbSet<AnnotationReply> AnnotationReplies { get; set; } = null!;

    public DbSet<VideoSettings> VideoSettings { get; set; } = null!;

    public DbSet<VideoThumbnail> VideoThumbnails { get; set; } = null!;

    public DbSet<LiveStream> LiveStreams { get; set; } = null!;

    public DbSet<LiveStreamViewer> LiveStreamViewers { get; set; } = null!;

    public DbSet<LiveStreamChatMessage> LiveStreamChatMessages { get; set; } = null!;

    public DbSet<VideoAnalytics> VideoAnalytics { get; set; } = null!;

    public DbSet<VideoAnalyticsSummary> VideoAnalyticsSummaries { get; set; } = null!;

    public DbSet<VideoSEO> VideoSEOs { get; set; } = null!;

    public DbSet<VideoSearchKeyword> VideoSearchKeywords { get; set; } = null!;

    public DbSet<VideoBacklink> VideoBacklinks { get; set; } = null!;

    // Monetization
    public DbSet<VideoMonetization> VideoMonetizations { get; set; } = null!;
    public DbSet<MonetizationRule> MonetizationRules { get; set; } = null!;
    public DbSet<VideoPurchase> VideoPurchases { get; set; } = null!;
    public DbSet<VideoRental> VideoRentals { get; set; } = null!;
    public DbSet<AdRevenue> AdRevenues { get; set; } = null!;
    public DbSet<CreatorPayout> CreatorPayouts { get; set; } = null!;
    public DbSet<Sponsorship> Sponsorships { get; set; } = null!;

    // Subscription Tiers
    public DbSet<SubscriptionTier> SubscriptionTiers { get; set; } = null!;
    public DbSet<TierFeature> TierFeatures { get; set; } = null!;
    public DbSet<TierLimit> TierLimits { get; set; } = null!;
    public DbSet<UserSubscription> UserSubscriptions { get; set; } = null!;

    // Comments and ratings
    public DbSet<Comment> Comments { get; set; } = null!;

    public DbSet<VideoRating> VideoRatings { get; set; } = null!;

    // Playlists
    public DbSet<Playlist> Playlists { get; set; } = null!;

    public DbSet<PlaylistVideo> PlaylistVideos { get; set; } = null!;

    // Sharing
    public DbSet<VideoShare> VideoShares { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure composite keys for junction tables
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // Configure relationships
        modelBuilder.Entity<TenantBranding>()
            .HasOne(tb => tb.Tenant)
            .WithOne(t => t.Branding)
            .HasForeignKey<TenantBranding>(tb => tb.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TenantSubscription>()
            .HasOne(ts => ts.Tenant)
            .WithMany(t => t.Subscriptions)
            .HasForeignKey(ts => ts.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TenantSubscription>()
            .HasOne(ts => ts.Plan)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(ts => ts.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Notification
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Role>()
            .HasOne(r => r.Tenant)
            .WithMany()
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure EmailVerificationToken
        modelBuilder.Entity<EmailVerificationToken>()
            .HasOne(evt => evt.User)
            .WithMany(u => u.EmailVerificationTokens)
            .HasForeignKey(evt => evt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure TwoFactorAuthCode
        modelBuilder.Entity<TwoFactorAuthCode>()
            .HasOne(tfac => tfac.User)
            .WithMany(u => u.TwoFactorAuthCodes)
            .HasForeignKey(tfac => tfac.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Video
        modelBuilder.Entity<Video>()
            .HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Video>()
            .HasOne(v => v.Tenant)
            .WithMany()
            .HasForeignKey(v => v.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure VideoTag
        modelBuilder.Entity<VideoTag>()
            .HasOne(vt => vt.Video)
            .WithMany(v => v.VideoTags)
            .HasForeignKey(vt => vt.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure VideoProcessingJob
        modelBuilder.Entity<VideoProcessingJob>()
            .HasOne(vpj => vpj.Video)
            .WithMany(v => v.ProcessingJobs)
            .HasForeignKey(vpj => vpj.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure VideoCategory
        modelBuilder.Entity<VideoCategory>()
            .HasOne(vc => vc.Tenant)
            .WithMany()
            .HasForeignKey(vc => vc.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Video>()
            .HasOne(v => v.Category)
            .WithMany(vc => vc.Videos)
            .HasForeignKey(v => v.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure VideoChapter
        modelBuilder.Entity<VideoChapter>()
            .HasOne(vc => vc.Video)
            .WithMany()
            .HasForeignKey(vc => vc.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure VideoTranscript
        modelBuilder.Entity<VideoTranscript>()
            .HasOne(vt => vt.Video)
            .WithMany()
            .HasForeignKey(vt => vt.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure VideoAnnotation
        modelBuilder.Entity<VideoAnnotation>()
            .HasOne(va => va.Video)
            .WithMany()
            .HasForeignKey(va => va.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VideoAnnotation>()
            .HasOne(va => va.User)
            .WithMany()
            .HasForeignKey(va => va.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure AnnotationReply
        modelBuilder.Entity<AnnotationReply>()
            .HasOne(ar => ar.Annotation)
            .WithMany(a => a.Replies)
            .HasForeignKey(ar => ar.AnnotationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AnnotationReply>()
            .HasOne(ar => ar.User)
            .WithMany()
            .HasForeignKey(ar => ar.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure VideoSettings
        modelBuilder.Entity<VideoSettings>()
            .HasOne(vs => vs.User)
            .WithMany()
            .HasForeignKey(vs => vs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VideoSettings>()
            .HasOne(vs => vs.Video)
            .WithMany()
            .HasForeignKey(vs => vs.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure VideoThumbnail
        modelBuilder.Entity<VideoThumbnail>()
            .HasOne(vt => vt.Video)
            .WithMany()
            .HasForeignKey(vt => vt.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure LiveStream
        modelBuilder.Entity<LiveStream>()
            .HasOne(ls => ls.User)
            .WithMany()
            .HasForeignKey(ls => ls.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LiveStream>()
            .HasOne(ls => ls.RecordedVideo)
            .WithOne()
            .HasForeignKey<LiveStream>(ls => ls.RecordedVideoId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure LiveStreamViewer
        modelBuilder.Entity<LiveStreamViewer>()
            .HasOne(lsv => lsv.LiveStream)
            .WithMany(ls => ls.Viewers)
            .HasForeignKey(lsv => lsv.LiveStreamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LiveStreamViewer>()
            .HasOne(lsv => lsv.User)
            .WithMany()
            .HasForeignKey(lsv => lsv.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure LiveStreamChatMessage
        modelBuilder.Entity<LiveStreamChatMessage>()
            .HasOne(lscm => lscm.LiveStream)
            .WithMany(ls => ls.ChatMessages)
            .HasForeignKey(lscm => lscm.LiveStreamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LiveStreamChatMessage>()
            .HasOne(lscm => lscm.User)
            .WithMany()
            .HasForeignKey(lscm => lscm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure VideoAnalytics
        modelBuilder.Entity<VideoAnalytics>()
            .HasOne(va => va.Video)
            .WithMany()
            .HasForeignKey(va => va.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VideoAnalytics>()
            .HasOne(va => va.User)
            .WithMany()
            .HasForeignKey(va => va.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure VideoAnalyticsSummary
        modelBuilder.Entity<VideoAnalyticsSummary>()
            .HasOne(vas => vas.Video)
            .WithMany()
            .HasForeignKey(vas => vas.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure VideoSEO
        modelBuilder.Entity<VideoSEO>()
            .HasOne(vs => vs.Video)
            .WithOne()
            .HasForeignKey<VideoSEO>(vs => vs.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure VideoSearchKeyword
        modelBuilder.Entity<VideoSearchKeyword>()
            .HasOne(vsk => vsk.Video)
            .WithMany()
            .HasForeignKey(vsk => vsk.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure VideoBacklink
        modelBuilder.Entity<VideoBacklink>()
            .HasOne(vb => vb.Video)
            .WithMany()
            .HasForeignKey(vb => vb.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure VideoMonetization
        modelBuilder.Entity<VideoMonetization>()
            .HasOne(vm => vm.Video)
            .WithOne()
            .HasForeignKey<VideoMonetization>(vm => vm.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure MonetizationRule
        modelBuilder.Entity<MonetizationRule>()
            .HasOne(mr => mr.VideoMonetization)
            .WithMany(vm => vm.Rules)
            .HasForeignKey(mr => mr.VideoMonetizationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure VideoPurchase
        modelBuilder.Entity<VideoPurchase>()
            .HasOne(vp => vp.Video)
            .WithMany(vm => vm.Purchases)
            .HasForeignKey(vp => vp.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VideoPurchase>()
            .HasOne(vp => vp.User)
            .WithMany()
            .HasForeignKey(vp => vp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure VideoRental
        modelBuilder.Entity<VideoRental>()
            .HasOne(vr => vr.Video)
            .WithMany(vm => vm.Rentals)
            .HasForeignKey(vr => vr.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VideoRental>()
            .HasOne(vr => vr.User)
            .WithMany()
            .HasForeignKey(vr => vr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure AdRevenue
        modelBuilder.Entity<AdRevenue>()
            .HasOne(ar => ar.Video)
            .WithMany(vm => vm.AdRevenues)
            .HasForeignKey(ar => ar.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AdRevenue>()
            .HasOne(ar => ar.User)
            .WithMany()
            .HasForeignKey(ar => ar.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure CreatorPayout
        modelBuilder.Entity<CreatorPayout>()
            .HasOne(cp => cp.User)
            .WithMany()
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Sponsorship
        modelBuilder.Entity<Sponsorship>()
            .HasOne(s => s.Video)
            .WithMany()
            .HasForeignKey(s => s.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Sponsorship>()
            .HasOne(s => s.Sponsor)
            .WithMany()
            .HasForeignKey(s => s.SponsorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure SubscriptionTier
        modelBuilder.Entity<SubscriptionTier>()
            .HasMany(st => st.Features)
            .WithOne(tf => tf.SubscriptionTier)
            .HasForeignKey(tf => tf.SubscriptionTierId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SubscriptionTier>()
            .HasMany(st => st.Limits)
            .WithOne(tl => tl.SubscriptionTier)
            .HasForeignKey(tl => tl.SubscriptionTierId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SubscriptionTier>()
            .HasMany(st => st.Subscriptions)
            .WithOne(us => us.SubscriptionTier)
            .HasForeignKey(us => us.SubscriptionTierId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure TierLimit
        modelBuilder.Entity<TierLimit>()
            .HasOne(tl => tl.SubscriptionTier)
            .WithMany(st => st.Limits)
            .HasForeignKey(tl => tl.SubscriptionTierId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure UserSubscription
        modelBuilder.Entity<UserSubscription>()
            .HasOne(us => us.User)
            .WithMany()
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserSubscription>()
            .HasOne(us => us.SubscriptionTier)
            .WithMany(st => st.Subscriptions)
            .HasForeignKey(us => us.SubscriptionTierId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Comment
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Video)
            .WithMany()
            .HasForeignKey(c => c.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Parent)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure VideoRating
        modelBuilder.Entity<VideoRating>()
            .HasOne(vr => vr.Video)
            .WithMany()
            .HasForeignKey(vr => vr.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VideoRating>()
            .HasOne(vr => vr.User)
            .WithMany()
            .HasForeignKey(vr => vr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Playlist
        modelBuilder.Entity<Playlist>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Playlist>()
            .HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure PlaylistVideo
        modelBuilder.Entity<PlaylistVideo>()
            .HasOne(pv => pv.Playlist)
            .WithMany(p => p.PlaylistVideos)
            .HasForeignKey(pv => pv.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlaylistVideo>()
            .HasOne(pv => pv.Video)
            .WithMany()
            .HasForeignKey(pv => pv.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure VideoShare
        modelBuilder.Entity<VideoShare>()
            .HasOne(vs => vs.Video)
            .WithMany()
            .HasForeignKey(vs => vs.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VideoShare>()
            .HasOne(vs => vs.User)
            .WithMany()
            .HasForeignKey(vs => vs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Users indexes
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.TenantId);

        // Notification indexes
        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead });

        modelBuilder.Entity<Notification>()
            .HasIndex(n => n.CreatedAt);

        // Configure indexes
        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.Slug)
            .IsUnique();

        modelBuilder.Entity<SubscriptionPlan>()
            .HasIndex(sp => sp.Slug)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(r => new { r.TenantId, r.Name })
            .IsUnique();

        modelBuilder.Entity<Permission>()
            .HasIndex(p => p.NormalizedName)
            .IsUnique();

        modelBuilder.Entity<Video>()
            .HasIndex(v => new { v.TenantId, v.UserId });

        modelBuilder.Entity<Video>()
            .HasIndex(v => v.CreatedAt);

        modelBuilder.Entity<Video>()
            .HasIndex(v => v.Status);

        modelBuilder.Entity<Video>()
            .HasIndex(v => v.IsPublic);

        modelBuilder.Entity<VideoTag>()
            .HasIndex(vt => vt.Tag);

        // VideoCategory indexes
        modelBuilder.Entity<VideoCategory>()
            .HasIndex(vc => new { vc.TenantId, vc.Slug });

        modelBuilder.Entity<Video>()
            .HasIndex(v => v.CategoryId);

        // VideoChapter indexes
        modelBuilder.Entity<VideoChapter>()
            .HasIndex(vc => new { vc.VideoId, vc.SortOrder });

        modelBuilder.Entity<VideoChapter>()
            .HasIndex(vc => new { vc.VideoId, vc.StartTimeSeconds });

        // VideoTranscript indexes
        modelBuilder.Entity<VideoTranscript>()
            .HasIndex(vt => new { vt.VideoId, vt.SortOrder });

        modelBuilder.Entity<VideoTranscript>()
            .HasIndex(vt => new { vt.VideoId, vt.StartTimeSeconds });

        // VideoAnnotation indexes
        modelBuilder.Entity<VideoAnnotation>()
            .HasIndex(va => new { va.VideoId, va.StartTimeSeconds });

        modelBuilder.Entity<VideoAnnotation>()
            .HasIndex(va => new { va.VideoId, va.UserId });

        modelBuilder.Entity<VideoAnnotation>()
            .HasIndex(va => va.CreatedAt);

        // AnnotationReply indexes
        modelBuilder.Entity<AnnotationReply>()
            .HasIndex(ar => new { ar.AnnotationId, ar.CreatedAt });

        // VideoSettings indexes
        modelBuilder.Entity<VideoSettings>()
            .HasIndex(vs => new { vs.VideoId, vs.UserId })
            .IsUnique();

        // VideoThumbnail indexes
        modelBuilder.Entity<VideoThumbnail>()
            .HasIndex(vt => new { vt.VideoId, vt.PositionSeconds });

        modelBuilder.Entity<VideoThumbnail>()
            .HasIndex(vt => vt.IsDefault);

        // LiveStream indexes
        modelBuilder.Entity<LiveStream>()
            .HasIndex(ls => new { ls.UserId, ls.Status });

        modelBuilder.Entity<LiveStream>()
            .HasIndex(ls => ls.Status);

        modelBuilder.Entity<LiveStream>()
            .HasIndex(ls => ls.ScheduledAt);

        modelBuilder.Entity<LiveStream>()
            .HasIndex(ls => ls.CreatedAt);

        // LiveStreamViewer indexes
        modelBuilder.Entity<LiveStreamViewer>()
            .HasIndex(lsv => new { lsv.LiveStreamId, lsv.JoinedAt });

        modelBuilder.Entity<LiveStreamViewer>()
            .HasIndex(lsv => new { lsv.LiveStreamId, lsv.LeftAt });

        // LiveStreamChatMessage indexes
        modelBuilder.Entity<LiveStreamChatMessage>()
            .HasIndex(lscm => new { lscm.LiveStreamId, lscm.CreatedAt });

        // VideoAnalytics indexes
        modelBuilder.Entity<VideoAnalytics>()
            .HasIndex(va => new { va.VideoId, va.Timestamp });

        modelBuilder.Entity<VideoAnalytics>()
            .HasIndex(va => new { va.VideoId, va.UserId });

        modelBuilder.Entity<VideoAnalytics>()
            .HasIndex(va => new { va.VideoId, va.EventType });

        modelBuilder.Entity<VideoAnalytics>()
            .HasIndex(va => va.Timestamp);

        // VideoAnalyticsSummary indexes
        modelBuilder.Entity<VideoAnalyticsSummary>()
            .HasIndex(vas => new { vas.VideoId, vas.Date })
            .IsUnique();

        // VideoSEO indexes
        modelBuilder.Entity<VideoSEO>()
            .HasIndex(vs => vs.VideoId)
            .IsUnique();

        modelBuilder.Entity<VideoSEO>()
            .HasIndex(vs => vs.EnableIndexing);

        modelBuilder.Entity<VideoSEO>()
            .HasIndex(vs => vs.EnableSitemap);

        modelBuilder.Entity<VideoSEO>()
            .HasIndex(vs => vs.LastIndexed);

        // VideoSearchKeyword indexes
        modelBuilder.Entity<VideoSearchKeyword>()
            .HasIndex(vsk => new { vsk.VideoId, vsk.Keyword });

        modelBuilder.Entity<VideoSearchKeyword>()
            .HasIndex(vsk => new { vsk.Keyword, vsk.RelevanceScore });

        modelBuilder.Entity<VideoSearchKeyword>()
            .HasIndex(vsk => vsk.Position);

        // VideoBacklink indexes
        modelBuilder.Entity<VideoBacklink>()
            .HasIndex(vb => new { vb.VideoId, vb.SourceUrl });

        modelBuilder.Entity<VideoBacklink>()
            .HasIndex(vb => vb.SourceUrl);

        modelBuilder.Entity<VideoBacklink>()
            .HasIndex(vb => vb.IsActive);

        modelBuilder.Entity<VideoBacklink>()
            .HasIndex(vb => vb.DomainAuthority);

        // VideoMonetization indexes
        modelBuilder.Entity<VideoMonetization>()
            .HasIndex(vm => vm.VideoId)
            .IsUnique();

        modelBuilder.Entity<VideoMonetization>()
            .HasIndex(vm => vm.MonetizationType);

        modelBuilder.Entity<VideoMonetization>()
            .HasIndex(vm => vm.IsActive);

        // VideoPurchase indexes
        modelBuilder.Entity<VideoPurchase>()
            .HasIndex(vp => new { vp.VideoId, vp.UserId });

        modelBuilder.Entity<VideoPurchase>()
            .HasIndex(vp => vp.PaymentIntentId)
            .IsUnique();

        modelBuilder.Entity<VideoPurchase>()
            .HasIndex(vp => vp.PurchasedAt);

        // VideoRental indexes
        modelBuilder.Entity<VideoRental>()
            .HasIndex(vr => new { vr.VideoId, vr.UserId });

        modelBuilder.Entity<VideoRental>()
            .HasIndex(vr => vr.ExpiresAt);

        modelBuilder.Entity<VideoRental>()
            .HasIndex(vr => vr.IsActive);

        // AdRevenue indexes
        modelBuilder.Entity<AdRevenue>()
            .HasIndex(ar => new { ar.VideoId, ar.Date });

        modelBuilder.Entity<AdRevenue>()
            .HasIndex(ar => new { ar.UserId, ar.Date });

        modelBuilder.Entity<AdRevenue>()
            .HasIndex(ar => ar.Date);

        // CreatorPayout indexes
        modelBuilder.Entity<CreatorPayout>()
            .HasIndex(cp => new { cp.UserId, cp.Status });

        modelBuilder.Entity<CreatorPayout>()
            .HasIndex(cp => cp.Status);

        modelBuilder.Entity<CreatorPayout>()
            .HasIndex(cp => new { cp.PeriodStart, cp.PeriodEnd });

        // Sponsorship indexes
        modelBuilder.Entity<Sponsorship>()
            .HasIndex(s => new { s.VideoId, s.SponsorId });

        modelBuilder.Entity<Sponsorship>()
            .HasIndex(s => s.SponsorId);

        modelBuilder.Entity<Sponsorship>()
            .HasIndex(s => s.IsActive);

        // SubscriptionTier indexes
        modelBuilder.Entity<SubscriptionTier>()
            .HasIndex(st => st.IsActive);

        modelBuilder.Entity<SubscriptionTier>()
            .HasIndex(st => st.SortOrder);

        modelBuilder.Entity<SubscriptionTier>()
            .HasIndex(st => st.Price);

        // UserSubscription indexes
        modelBuilder.Entity<UserSubscription>()
            .HasIndex(us => new { us.UserId, us.Status });

        modelBuilder.Entity<UserSubscription>()
            .HasIndex(us => us.StripeSubscriptionId)
            .IsUnique();

        modelBuilder.Entity<UserSubscription>()
            .HasIndex(us => us.Status);

        modelBuilder.Entity<UserSubscription>()
            .HasIndex(us => us.CurrentPeriodEnd);

        // Comments indexes
        modelBuilder.Entity<Comment>()
            .HasIndex(c => new { c.VideoId, c.CreatedAt });

        modelBuilder.Entity<Comment>()
            .HasIndex(c => c.UserId);

        modelBuilder.Entity<Comment>()
            .HasIndex(c => c.ParentId);

        // VideoRating indexes
        modelBuilder.Entity<VideoRating>()
            .HasIndex(vr => new { vr.VideoId, vr.UserId })
            .IsUnique();

        // Playlist indexes
        modelBuilder.Entity<Playlist>()
            .HasIndex(p => new { p.UserId, p.IsDeleted });

        modelBuilder.Entity<Playlist>()
            .HasIndex(p => p.TenantId);

        // PlaylistVideo indexes
        modelBuilder.Entity<PlaylistVideo>()
            .HasIndex(pv => new { pv.PlaylistId, pv.Position });

        // VideoShare indexes
        modelBuilder.Entity<VideoShare>()
            .HasIndex(vs => vs.Token)
            .IsUnique();

        modelBuilder.Entity<VideoShare>()
            .HasIndex(vs => new { vs.VideoId, vs.UserId });

        // Configure JSONB columns for PostgreSQL
        modelBuilder.Entity<Tenant>()
            .Property(t => t.Settings)
            .HasColumnType("jsonb");

        modelBuilder.Entity<SubscriptionPlan>()
            .Property(sp => sp.Features)
            .HasColumnType("jsonb");

        modelBuilder.Entity<SubscriptionPlan>()
            .Property(sp => sp.Limits)
            .HasColumnType("jsonb");
    }
}
