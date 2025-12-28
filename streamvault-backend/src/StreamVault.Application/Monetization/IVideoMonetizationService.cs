using StreamVault.Application.Monetization.DTOs;
using StreamVault.Domain.Entities;

namespace StreamVault.Application.Monetization;

public interface IVideoMonetizationService
{
    Task<VideoMonetizationDto> GetVideoMonetizationAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<VideoMonetizationDto> UpdateVideoMonetizationAsync(Guid videoId, UpdateVideoMonetizationRequest request, Guid userId, Guid tenantId);
    Task<VideoMonetizationDto> EnableMonetizationAsync(Guid videoId, EnableMonetizationRequest request, Guid userId, Guid tenantId);
    Task<bool> DisableMonetizationAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<VideoPurchaseDto> PurchaseVideoAsync(Guid videoId, PurchaseVideoRequest request, Guid userId, Guid tenantId);
    Task<VideoRentalDto> RentVideoAsync(Guid videoId, RentVideoRequest request, Guid userId, Guid tenantId);
    Task<bool> CheckVideoAccessAsync(Guid videoId, Guid userId, Guid tenantId);
    Task<List<VideoPurchaseDto>> GetUserPurchasesAsync(Guid userId, Guid tenantId, int? page = null, int? pageSize = null);
    Task<List<VideoRentalDto>> GetUserRentalsAsync(Guid userId, Guid tenantId, int? page = null, int? pageSize = null);
    Task<RevenueDto> GetVideoRevenueAsync(Guid videoId, Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
    Task<List<RevenueDto>> GetCreatorRevenueAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
    Task<AdRevenueDto> RecordAdRevenueAsync(RecordAdRevenueRequest request, Guid userId, Guid tenantId);
    Task<SponsorshipDto> CreateSponsorshipAsync(Guid videoId, CreateSponsorshipRequest request, Guid userId, Guid tenantId);
    Task<bool> CanUserWatchVideoAsync(Guid videoId, Guid userId, Guid tenantId);
}
