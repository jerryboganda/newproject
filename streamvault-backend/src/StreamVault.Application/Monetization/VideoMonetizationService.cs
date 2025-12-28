using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Monetization.DTOs;
using StreamVault.Application.Auth.DTOs;
using StreamVault.Application.Videos.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Monetization;

public class VideoMonetizationService : IVideoMonetizationService
{
    private readonly StreamVaultDbContext _dbContext;

    public VideoMonetizationService(StreamVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VideoMonetizationDto> GetVideoMonetizationAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var monetization = await _dbContext.VideoMonetizations
            .Include(vm => vm.Rules)
            .FirstOrDefaultAsync(vm => vm.VideoId == videoId);

        if (monetization == null)
        {
            // Return default monetization object
            return new VideoMonetizationDto
            {
                Id = Guid.Empty,
                VideoId = videoId,
                MonetizationType = MonetizationType.Free,
                IsActive = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        }

        return MapToDto(monetization);
    }

    public async Task<VideoMonetizationDto> UpdateVideoMonetizationAsync(Guid videoId, UpdateVideoMonetizationRequest request, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var monetization = await _dbContext.VideoMonetizations
            .Include(vm => vm.Rules)
            .FirstOrDefaultAsync(vm => vm.VideoId == videoId);

        if (monetization == null)
        {
            monetization = new VideoMonetization
            {
                Id = Guid.NewGuid(),
                VideoId = videoId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.VideoMonetizations.Add(monetization);
        }

        // Update properties
        monetization.MonetizationType = request.MonetizationType;
        monetization.Price = request.Price;
        monetization.Currency = request.Currency ?? "USD";
        monetization.IsActive = request.IsActive ?? true;
        monetization.StartDate = request.StartDate;
        monetization.EndDate = request.EndDate;
        monetization.SubscriptionTierRequired = request.SubscriptionTierRequired;
        monetization.AllowRental = request.AllowRental ?? false;
        monetization.RentalPrice = request.RentalPrice;
        monetization.RentalPeriodHours = request.RentalPeriodHours ?? 24;
        monetization.AllowPurchase = request.AllowPurchase ?? false;
        monetization.PurchasePrice = request.PurchasePrice;
        monetization.EnableAdSupport = request.EnableAdSupport ?? false;
        monetization.AdRevenueSharePercentage = request.AdRevenueSharePercentage ?? 55;
        monetization.SponsorshipDetails = request.SponsorshipDetails;
        monetization.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(monetization);
    }

    public async Task<VideoMonetizationDto> EnableMonetizationAsync(Guid videoId, EnableMonetizationRequest request, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var monetization = await _dbContext.VideoMonetizations
            .FirstOrDefaultAsync(vm => vm.VideoId == videoId);

        if (monetization != null && monetization.IsActive)
            throw new Exception("Video is already monetized");

        if (monetization == null)
        {
            monetization = new VideoMonetization
            {
                Id = Guid.NewGuid(),
                VideoId = videoId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.VideoMonetizations.Add(monetization);
        }

        // Set monetization properties
        monetization.MonetizationType = request.MonetizationType;
        monetization.Price = request.Price;
        monetization.Currency = request.Currency ?? "USD";
        monetization.IsActive = true;
        monetization.AllowRental = request.AllowRental;
        monetization.RentalPrice = request.RentalPrice;
        monetization.RentalPeriodHours = request.RentalPeriodHours;
        monetization.AllowPurchase = request.AllowPurchase;
        monetization.PurchasePrice = request.PurchasePrice;
        monetization.EnableAdSupport = request.EnableAdSupport;
        monetization.AdRevenueSharePercentage = request.AdRevenueSharePercentage;
        monetization.SubscriptionTierRequired = request.SubscriptionTierRequired;
        monetization.StartDate = DateTimeOffset.UtcNow;
        monetization.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(monetization);
    }

    public async Task<bool> DisableMonetizationAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video belongs to tenant
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var monetization = await _dbContext.VideoMonetizations
            .FirstOrDefaultAsync(vm => vm.VideoId == videoId);

        if (monetization == null)
            return true;

        monetization.IsActive = false;
        monetization.EndDate = DateTimeOffset.UtcNow;
        monetization.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<VideoPurchaseDto> PurchaseVideoAsync(Guid videoId, PurchaseVideoRequest request, Guid userId, Guid tenantId)
    {
        // Verify video exists
        var video = await _dbContext.Videos
            .Include(v => v.Monetization)
            .FirstOrDefaultAsync(v => v.Id == videoId);

        if (video == null)
            throw new Exception("Video not found");

        // Check if monetization allows purchase
        if (video.Monetization == null || !video.Monetization.AllowPurchase || !video.Monetization.PurchasePrice.HasValue)
            throw new Exception("Video is not available for purchase");

        // Check if already purchased
        var existingPurchase = await _dbContext.VideoPurchases
            .FirstOrDefaultAsync(vp => vp.VideoId == videoId && vp.UserId == userId && !vp.IsRefunded);

        if (existingPurchase != null)
            throw new Exception("Video already purchased");

        // Process payment (in production, integrate with Stripe/PayPal)
        var paymentIntentId = Guid.NewGuid().ToString();

        // Create purchase record
        var purchase = new VideoPurchase
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            UserId = userId,
            Price = video.Monetization.PurchasePrice.Value,
            Currency = video.Monetization.Currency,
            PaymentIntentId = paymentIntentId,
            PurchasedAt = DateTimeOffset.UtcNow
        };

        _dbContext.VideoPurchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        // Map to DTO
        var purchaseDto = MapPurchaseToDto(purchase);
        purchaseDto.Video = MapVideoToDto(video);
        
        return purchaseDto;
    }

    public async Task<VideoRentalDto> RentVideoAsync(Guid videoId, RentVideoRequest request, Guid userId, Guid tenantId)
    {
        // Verify video exists
        var video = await _dbContext.Videos
            .Include(v => v.Monetization)
            .FirstOrDefaultAsync(v => v.Id == videoId);

        if (video == null)
            throw new Exception("Video not found");

        // Check if monetization allows rental
        if (video.Monetization == null || !video.Monetization.AllowRental || !video.Monetization.RentalPrice.HasValue)
            throw new Exception("Video is not available for rental");

        // Check if has active rental
        var activeRental = await _dbContext.VideoRentals
            .FirstOrDefaultAsync(vr => vr.VideoId == videoId && vr.UserId == userId && 
                                     vr.IsActive && !vr.IsExpired);

        if (activeRental != null)
            throw new Exception("Video already rented");

        // Process payment (in production, integrate with Stripe/PayPal)
        var paymentIntentId = Guid.NewGuid().ToString();

        // Create rental record
        var rental = new VideoRental
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            UserId = userId,
            Price = video.Monetization.RentalPrice.Value,
            Currency = video.Monetization.Currency,
            PaymentIntentId = paymentIntentId,
            RentedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(video.Monetization.RentalPeriodHours),
            IsActive = true
        };

        _dbContext.VideoRentals.Add(rental);
        await _dbContext.SaveChangesAsync();

        // Map to DTO
        var rentalDto = MapRentalToDto(rental);
        rentalDto.Video = MapVideoToDto(video);
        
        return rentalDto;
    }

    public async Task<bool> CheckVideoAccessAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video exists
        var video = await _dbContext.Videos
            .Include(v => v.Monetization)
            .FirstOrDefaultAsync(v => v.Id == videoId);

        if (video == null)
            return false;

        // Free videos are always accessible
        if (video.IsPublic && (video.Monetization == null || video.Monetization.MonetizationType == MonetizationType.Free))
            return true;

        // Check if user purchased the video
        var purchase = await _dbContext.VideoPurchases
            .FirstOrDefaultAsync(vp => vp.VideoId == videoId && vp.UserId == userId && !vp.IsRefunded);

        if (purchase != null)
            return true;

        // Check if user has active rental
        var rental = await _dbContext.VideoRentals
            .FirstOrDefaultAsync(vr => vr.VideoId == videoId && vr.UserId == userId && 
                                     vr.IsActive && !vr.IsExpired);

        if (rental != null)
            return true;

        // Check subscription access
        if (!string.IsNullOrEmpty(video.Monetization?.SubscriptionTierRequired))
        {
            var subscription = await _dbContext.UserSubscriptions
                .Include(us => us.SubscriptionTier)
                .FirstOrDefaultAsync(us => us.UserId == userId && 
                                         us.Status == SubscriptionStatus.Active &&
                                         us.SubscriptionTier.Name == video.Monetization.SubscriptionTierRequired &&
                                         us.CurrentPeriodEnd > DateTimeOffset.UtcNow);

            if (subscription != null)
                return true;
        }

        return false;
    }

    public async Task<List<VideoPurchaseDto>> GetUserPurchasesAsync(Guid userId, Guid tenantId, int? page = null, int? pageSize = null)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var query = _dbContext.VideoPurchases
            .Include(vp => vp.Video)
            .Where(vp => vp.UserId == userId && !vp.IsRefunded)
            .OrderByDescending(vp => vp.PurchasedAt);

        if (page.HasValue && pageSize.HasValue)
        {
            query = (IOrderedQueryable<VideoPurchase>)query
                .Skip((page.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }

        var purchases = await query.ToListAsync();

        return purchases.Select(p =>
        {
            var dto = MapPurchaseToDto(p);
            dto.Video = MapVideoToDto(p.Video);
            return dto;
        }).ToList();
    }

    public async Task<List<VideoRentalDto>> GetUserRentalsAsync(Guid userId, Guid tenantId, int? page = null, int? pageSize = null)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var query = _dbContext.VideoRentals
            .Include(vr => vr.Video)
            .Where(vr => vr.UserId == userId)
            .OrderByDescending(vr => vr.RentedAt);

        if (page.HasValue && pageSize.HasValue)
        {
            query = (IOrderedQueryable<VideoRental>)query
                .Skip((page.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }

        var rentals = await query.ToListAsync();

        return rentals.Select(r =>
        {
            var dto = MapRentalToDto(r);
            dto.Video = MapVideoToDto(r.Video);
            return dto;
        }).ToList();
    }

    public async Task<RevenueDto> GetVideoRevenueAsync(Guid videoId, Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        // Verify video belongs to user
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.UserId == userId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var start = startDate ?? DateTimeOffset.UtcNow.AddDays(-30);
        var end = endDate ?? DateTimeOffset.UtcNow;

        // Calculate revenue
        var purchaseRevenue = await _dbContext.VideoPurchases
            .Where(vp => vp.VideoId == videoId && vp.PurchasedAt >= start && vp.PurchasedAt <= end && !vp.IsRefunded)
            .SumAsync(vp => vp.Price);

        var rentalRevenue = await _dbContext.VideoRentals
            .Where(vr => vr.VideoId == videoId && vr.RentedAt >= start && vr.RentedAt <= end)
            .SumAsync(vr => vr.Price);

        var adRevenue = await _dbContext.AdRevenues
            .Where(ar => ar.VideoId == videoId && ar.CreatedAt >= start && ar.CreatedAt <= end)
            .SumAsync(ar => ar.Revenue);

        var sponsorshipRevenue = await _dbContext.Sponsorships
            .Where(s => s.VideoId == videoId && s.StartDate >= start && s.StartDate <= end)
            .SumAsync(s => s.Amount);

        var totalPurchases = await _dbContext.VideoPurchases
            .CountAsync(vp => vp.VideoId == videoId && vp.PurchasedAt >= start && vp.PurchasedAt <= end && !vp.IsRefunded);

        var totalRentals = await _dbContext.VideoRentals
            .CountAsync(vr => vr.VideoId == videoId && vr.RentedAt >= start && vr.RentedAt <= end);

        var totalViews = await _dbContext.VideoAnalytics
            .CountAsync(va => va.VideoId == videoId && va.Timestamp >= start && va.Timestamp <= end);

        return new RevenueDto
        {
            VideoId = videoId,
            VideoTitle = video.Title,
            TotalRevenue = purchaseRevenue + rentalRevenue + adRevenue + sponsorshipRevenue,
            PurchaseRevenue = purchaseRevenue,
            RentalRevenue = rentalRevenue,
            AdRevenue = adRevenue,
            SponsorshipRevenue = sponsorshipRevenue,
            TotalPurchases = totalPurchases,
            TotalRentals = totalRentals,
            TotalViews = totalViews,
            Currency = "USD",
            Period = DateOnly.FromDateTime(DateTime.UtcNow)
        };
    }

    public async Task<List<RevenueDto>> GetCreatorRevenueAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var start = startDate ?? DateTimeOffset.UtcNow.AddDays(-30);
        var end = endDate ?? DateTimeOffset.UtcNow;

        // Get user's videos
        var videoIds = await _dbContext.Videos
            .Where(v => v.UserId == userId)
            .Select(v => v.Id)
            .ToListAsync();

        var revenues = new List<RevenueDto>();

        foreach (var videoId in videoIds)
        {
            var revenue = await GetVideoRevenueAsync(videoId, userId, tenantId, start, end);
            revenues.Add(revenue);
        }

        return revenues.OrderByDescending(r => r.TotalRevenue).ToList();
    }

    public async Task<AdRevenueDto> RecordAdRevenueAsync(RecordAdRevenueRequest request, Guid userId, Guid tenantId)
    {
        // Verify video belongs to user
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == request.VideoId && v.UserId == userId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var adRevenue = new AdRevenue
        {
            Id = Guid.NewGuid(),
            VideoId = request.VideoId,
            UserId = userId,
            Revenue = request.Revenue,
            Currency = request.Currency,
            Impressions = request.Impressions,
            Clicks = request.Clicks,
            CTR = request.Impressions > 0 ? (request.Clicks * 100.0) / request.Impressions : 0,
            CPM = request.CPM,
            Date = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow),
            AdNetwork = request.AdNetwork,
            AdType = request.AdType,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.AdRevenues.Add(adRevenue);
        await _dbContext.SaveChangesAsync();

        return MapAdRevenueToDto(adRevenue);
    }

    public async Task<SponsorshipDto> CreateSponsorshipAsync(Guid videoId, CreateSponsorshipRequest request, Guid userId, Guid tenantId)
    {
        // Verify video belongs to user
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.UserId == userId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        // Verify sponsor exists
        var sponsor = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.SponsorId);

        if (sponsor == null)
            throw new Exception("Sponsor not found");

        var sponsorship = new Sponsorship
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            SponsorId = request.SponsorId,
            Amount = request.Amount,
            Currency = request.Currency,
            SponsorName = request.SponsorName,
            SponsorLogo = request.SponsorLogo,
            SponsorUrl = request.SponsorUrl,
            DisplayDurationSeconds = request.DisplayDurationSeconds,
            DisplayPosition = request.DisplayPosition,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Sponsorships.Add(sponsorship);
        await _dbContext.SaveChangesAsync();

        return MapSponsorshipToDto(sponsorship);
    }

    public async Task<bool> CanUserWatchVideoAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        return await CheckVideoAccessAsync(videoId, userId, tenantId);
    }

    private static VideoMonetizationDto MapToDto(VideoMonetization monetization)
    {
        return new VideoMonetizationDto
        {
            Id = monetization.Id,
            VideoId = monetization.VideoId,
            MonetizationType = monetization.MonetizationType,
            Price = monetization.Price,
            Currency = monetization.Currency,
            IsActive = monetization.IsActive,
            StartDate = monetization.StartDate,
            EndDate = monetization.EndDate,
            SubscriptionTierRequired = monetization.SubscriptionTierRequired,
            AllowRental = monetization.AllowRental,
            RentalPrice = monetization.RentalPrice,
            RentalPeriodHours = monetization.RentalPeriodHours,
            AllowPurchase = monetization.AllowPurchase,
            PurchasePrice = monetization.PurchasePrice,
            EnableAdSupport = monetization.EnableAdSupport,
            AdRevenueSharePercentage = monetization.AdRevenueSharePercentage,
            SponsorshipDetails = monetization.SponsorshipDetails,
            Rules = monetization.Rules.Select(r => new MonetizationRuleDto
            {
                Id = r.Id,
                RuleType = r.RuleType,
                RuleValue = r.RuleValue,
                IsActive = r.IsActive
            }).ToList(),
            CreatedAt = monetization.CreatedAt,
            UpdatedAt = monetization.UpdatedAt
        };
    }

    private static VideoPurchaseDto MapPurchaseToDto(VideoPurchase purchase)
    {
        return new VideoPurchaseDto
        {
            Id = purchase.Id,
            VideoId = purchase.VideoId,
            UserId = purchase.UserId,
            Price = purchase.Price,
            Currency = purchase.Currency,
            PaymentIntentId = purchase.PaymentIntentId,
            ReceiptUrl = purchase.ReceiptUrl,
            PurchasedAt = purchase.PurchasedAt,
            IsRefunded = purchase.IsRefunded,
            RefundedAt = purchase.RefundedAt,
            RefundReason = purchase.RefundReason,
            User = new UserDto
            {
                Id = purchase.User.Id,
                Email = purchase.User.Email,
                FirstName = purchase.User.FirstName,
                LastName = purchase.User.LastName,
                AvatarUrl = purchase.User.AvatarUrl
            }
        };
    }

    private static VideoRentalDto MapRentalToDto(VideoRental rental)
    {
        return new VideoRentalDto
        {
            Id = rental.Id,
            VideoId = rental.VideoId,
            UserId = rental.UserId,
            Price = rental.Price,
            Currency = rental.Currency,
            PaymentIntentId = rental.PaymentIntentId,
            RentedAt = rental.RentedAt,
            ExpiresAt = rental.ExpiresAt,
            IsExpired = rental.IsExpired,
            IsActive = rental.IsActive,
            User = new UserDto
            {
                Id = rental.User.Id,
                Email = rental.User.Email,
                FirstName = rental.User.FirstName,
                LastName = rental.User.LastName,
                AvatarUrl = rental.User.AvatarUrl
            }
        };
    }

    public static VideoDto MapVideoToDto(Video video)
    {
        return new VideoDto
        {
            Id = video.Id,
            Title = video.Title,
            Description = video.Description,
            ThumbnailUrl = video.ThumbnailPath,
            DurationSeconds = video.DurationSeconds,
            ViewCount = video.ViewCount,
            IsPublic = video.IsPublic,
            CreatedAt = video.CreatedAt,
            PublishedAt = video.PublishedAt
        };
    }

    private static AdRevenueDto MapAdRevenueToDto(AdRevenue adRevenue)
    {
        return new AdRevenueDto
        {
            Id = adRevenue.Id,
            VideoId = adRevenue.VideoId,
            Revenue = adRevenue.Revenue,
            Currency = adRevenue.Currency,
            Impressions = adRevenue.Impressions,
            Clicks = adRevenue.Clicks,
            CTR = adRevenue.CTR,
            CPM = adRevenue.CPM,
            Date = adRevenue.Date,
            AdNetwork = adRevenue.AdNetwork,
            AdType = adRevenue.AdType
        };
    }

    private static SponsorshipDto MapSponsorshipToDto(Sponsorship sponsorship)
    {
        return new SponsorshipDto
        {
            Id = sponsorship.Id,
            VideoId = sponsorship.VideoId,
            SponsorId = sponsorship.SponsorId,
            Amount = sponsorship.Amount,
            Currency = sponsorship.Currency,
            SponsorName = sponsorship.SponsorName,
            SponsorLogo = sponsorship.SponsorLogo,
            SponsorUrl = sponsorship.SponsorUrl,
            DisplayDurationSeconds = sponsorship.DisplayDurationSeconds,
            DisplayPosition = sponsorship.DisplayPosition,
            IsActive = sponsorship.IsActive,
            StartDate = sponsorship.StartDate,
            EndDate = sponsorship.EndDate,
            CreatedAt = sponsorship.CreatedAt
        };
    }
}
