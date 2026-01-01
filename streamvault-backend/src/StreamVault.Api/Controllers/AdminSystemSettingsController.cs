using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamVault.Api.Services;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Api.Controllers;

[ApiController]
[Route("api/v1/admin/system-settings")]
[Authorize(Roles = "SuperAdmin")]
public class AdminSystemSettingsController : ControllerBase
{
    private readonly StreamVaultDbContext _db;
    private readonly AuditLogger _audit;

    public AdminSystemSettingsController(StreamVaultDbContext db, AuditLogger audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<ActionResult<SystemSettingsDto>> Get(CancellationToken cancellationToken)
    {
        var settings = await _db.SystemSettings.AsNoTracking().OrderBy(x => x.CreatedAt).FirstOrDefaultAsync(cancellationToken);
        if (settings == null)
        {
            settings = new SystemSettings
            {
                Id = Guid.NewGuid(),
                AllowNewRegistrations = true,
                RequireEmailVerification = true,
                DefaultSubscriptionPlanId = Guid.Empty,
                MaxFileSizeMB = 2048,
                SupportedVideoFormats = new List<string> { "mp4", "mov", "webm" },
                MaintenanceMode = false,
                MaintenanceMessage = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.SystemSettings.Add(settings);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Ok(Map(settings));
    }

    [HttpPut]
    public async Task<ActionResult<SystemSettingsDto>> Put([FromBody] UpdateSystemSettingsRequest request, CancellationToken cancellationToken)
    {
        var existing = await _db.SystemSettings.OrderBy(x => x.CreatedAt).FirstOrDefaultAsync(cancellationToken);

        var oldValues = existing == null
            ? null
            : new Dictionary<string, object>
            {
                ["allowNewRegistrations"] = existing.AllowNewRegistrations,
                ["requireEmailVerification"] = existing.RequireEmailVerification,
                ["defaultSubscriptionPlanId"] = existing.DefaultSubscriptionPlanId,
                ["maxFileSizeMB"] = existing.MaxFileSizeMB,
                ["supportedVideoFormats"] = existing.SupportedVideoFormats,
                ["maintenanceMode"] = existing.MaintenanceMode,
                ["maintenanceMessage"] = existing.MaintenanceMessage ?? string.Empty
            };

        if (existing == null)
        {
            existing = new SystemSettings
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };
            _db.SystemSettings.Add(existing);
        }

        existing.AllowNewRegistrations = request.AllowNewRegistrations;
        existing.RequireEmailVerification = request.RequireEmailVerification;
        existing.DefaultSubscriptionPlanId = request.DefaultSubscriptionPlanId ?? Guid.Empty;
        existing.MaxFileSizeMB = Math.Clamp(request.MaxFileSizeMB, 1, 1024 * 1024);
        existing.SupportedVideoFormats = request.SupportedVideoFormats?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().TrimStart('.')).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            ?? new List<string> { "mp4" };
        existing.MaintenanceMode = request.MaintenanceMode;
        existing.MaintenanceMessage = string.IsNullOrWhiteSpace(request.MaintenanceMessage) ? null : request.MaintenanceMessage.Trim();
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        await _audit.TryLogAsync(
            HttpContext,
            action: "admin.system_settings.update",
            entityType: nameof(SystemSettings),
            entityId: existing.Id,
            oldValues: oldValues,
            newValues: new Dictionary<string, object>
            {
                ["allowNewRegistrations"] = existing.AllowNewRegistrations,
                ["requireEmailVerification"] = existing.RequireEmailVerification,
                ["defaultSubscriptionPlanId"] = existing.DefaultSubscriptionPlanId,
                ["maxFileSizeMB"] = existing.MaxFileSizeMB,
                ["supportedVideoFormats"] = existing.SupportedVideoFormats,
                ["maintenanceMode"] = existing.MaintenanceMode,
                ["maintenanceMessage"] = existing.MaintenanceMessage ?? string.Empty
            },
            cancellationToken: cancellationToken);

        return Ok(Map(existing));
    }

    private static SystemSettingsDto Map(SystemSettings s) => new(
        s.Id,
        s.AllowNewRegistrations,
        s.RequireEmailVerification,
        s.DefaultSubscriptionPlanId == Guid.Empty ? null : s.DefaultSubscriptionPlanId,
        s.MaxFileSizeMB,
        s.SupportedVideoFormats,
        s.MaintenanceMode,
        s.MaintenanceMessage,
        s.CreatedAt,
        s.UpdatedAt);

    public sealed record SystemSettingsDto(
        Guid Id,
        bool AllowNewRegistrations,
        bool RequireEmailVerification,
        Guid? DefaultSubscriptionPlanId,
        int MaxFileSizeMB,
        IReadOnlyList<string> SupportedVideoFormats,
        bool MaintenanceMode,
        string? MaintenanceMessage,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public sealed class UpdateSystemSettingsRequest
    {
        public bool AllowNewRegistrations { get; set; }
        public bool RequireEmailVerification { get; set; }
        public Guid? DefaultSubscriptionPlanId { get; set; }
        public int MaxFileSizeMB { get; set; }
        public List<string>? SupportedVideoFormats { get; set; }
        public bool MaintenanceMode { get; set; }
        public string? MaintenanceMessage { get; set; }
    }
}
