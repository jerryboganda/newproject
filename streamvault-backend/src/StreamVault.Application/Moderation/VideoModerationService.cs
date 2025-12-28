using Microsoft.EntityFrameworkCore;
using StreamVault.Application.Moderation.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;

namespace StreamVault.Application.Moderation;

public class VideoModerationService : IVideoModerationService
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly ILogger<VideoModerationService> _logger;

    public VideoModerationService(StreamVaultDbContext dbContext, ILogger<VideoModerationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ModerationResultDto> ModerateVideoAsync(Guid videoId, Guid tenantId)
    {
        var video = await _dbContext.Videos
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var flags = new List<ModerationFlagDto>();
        var categoryScores = new Dictionary<string, double>();

        // Perform automated moderation checks
        var titleFlags = await CheckTitleContent(video.Title);
        flags.AddRange(titleFlags);

        var descriptionFlags = await CheckDescriptionContent(video.Description);
        flags.AddRange(descriptionFlags);

        var creatorFlags = await CheckCreatorHistory(video.User, tenantId);
        flags.AddRange(creatorFlags);

        // Calculate overall scores
        categoryScores["title"] = titleFlags.Any() ? titleFlags.Average(f => f.Confidence) : 0.1;
        categoryScores["description"] = descriptionFlags.Any() ? descriptionFlags.Average(f => f.Confidence) : 0.1;
        categoryScores["creator"] = creatorFlags.Any() ? creatorFlags.Average(f => f.Confidence) : 0.1;

        var overallScore = categoryScores.Values.Average();
        var requiresHumanReview = overallScore > 0.7 || flags.Any(f => f.Severity == "high" || f.Severity == "critical");

        var status = requiresHumanReview ? ModerationStatus.UnderReview :
                    overallScore > 0.8 ? ModerationStatus.AutoRejected :
                    overallScore < 0.3 ? ModerationStatus.AutoApproved :
                    ModerationStatus.Pending;

        // Store moderation flags in database
        foreach (var flag in flags)
        {
            _dbContext.ModerationFlags.Add(new ModerationFlag
            {
                Id = Guid.NewGuid(),
                VideoId = videoId,
                Type = flag.Type,
                Severity = flag.Severity,
                Description = flag.Description,
                Confidence = flag.Confidence,
                Timestamp = flag.Timestamp,
                Evidence = flag.Evidence,
                IsAutoDetected = true,
                CreatedAt = DateTimeOffset.UtcNow,
                IsResolved = false
            });
        }

        // Update video moderation status
        video.ModerationStatus = status.ToString();
        video.RequiresModeration = requiresHumanReview;
        video.ModeratedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return new ModerationResultDto
        {
            VideoId = videoId,
            Status = status,
            ConfidenceScore = overallScore,
            Flags = flags,
            Summary = GenerateModerationSummary(flags, overallScore),
            RequiresHumanReview = requiresHumanReview,
            ModeratedAt = DateTimeOffset.UtcNow,
            DetectedIssues = flags.Select(f => f.Type).Distinct().ToList(),
            CategoryScores = categoryScores
        };
    }

    public async Task<List<ModerationFlagDto>> GetVideoFlagsAsync(Guid videoId, Guid tenantId)
    {
        var flags = await _dbContext.ModerationFlags
            .Where(mf => mf.VideoId == videoId)
            .OrderByDescending(mf => mf.CreatedAt)
            .ToListAsync();

        return flags.Select(f => new ModerationFlagDto
        {
            Id = f.Id,
            VideoId = f.VideoId,
            Type = f.Type,
            Severity = f.Severity,
            Description = f.Description,
            Confidence = f.Confidence,
            Timestamp = f.Timestamp,
            Evidence = f.Evidence,
            IsAutoDetected = f.IsAutoDetected,
            CreatedAt = f.CreatedAt,
            IsResolved = f.IsResolved,
            ResolvedAt = f.ResolvedAt,
            ResolvedBy = f.ResolvedBy
        }).ToList();
    }

    public async Task<bool> ApproveVideoAsync(Guid videoId, Guid tenantId, Guid moderatorId)
    {
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        video.ModerationStatus = ModerationStatus.Approved.ToString();
        video.RequiresModeration = false;
        video.ModeratedAt = DateTimeOffset.UtcNow;
        video.ModeratedBy = moderatorId;

        // Resolve all flags
        var flags = await _dbContext.ModerationFlags
            .Where(mf => mf.VideoId == videoId && !mf.IsResolved)
            .ToListAsync();

        foreach (var flag in flags)
        {
            flag.IsResolved = true;
            flag.ResolvedAt = DateTimeOffset.UtcNow;
            flag.ResolvedBy = moderatorId.ToString();
        }

        // Log moderation action
        _dbContext.ModerationActions.Add(new ModerationAction
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            ModeratorId = moderatorId,
            Action = ModerationAction.Approved.ToString(),
            Reason = "Approved by moderator",
            Timestamp = DateTimeOffset.UtcNow,
            IsAutomated = false
        });

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectVideoAsync(Guid videoId, Guid tenantId, Guid moderatorId, string reason)
    {
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        video.ModerationStatus = ModerationStatus.Rejected.ToString();
        video.RequiresModeration = false;
        video.ModeratedAt = DateTimeOffset.UtcNow;
        video.ModeratedBy = moderatorId;
        video.ModerationReason = reason;

        // Resolve all flags
        var flags = await _dbContext.ModerationFlags
            .Where(mf => mf.VideoId == videoId && !mf.IsResolved)
            .ToListAsync();

        foreach (var flag in flags)
        {
            flag.IsResolved = true;
            flag.ResolvedAt = DateTimeOffset.UtcNow;
            flag.ResolvedBy = moderatorId.ToString();
        }

        // Log moderation action
        _dbContext.ModerationActions.Add(new ModerationAction
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            ModeratorId = moderatorId,
            Action = ModerationAction.Rejected.ToString(),
            Reason = reason,
            Timestamp = DateTimeOffset.UtcNow,
            IsAutomated = false
        });

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReportVideoAsync(Guid videoId, Guid userId, Guid tenantId, ReportVideoRequest request)
    {
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        // Check if user already reported this video
        var existingReport = await _dbContext.VideoReports
            .FirstOrDefaultAsync(vr => vr.VideoId == videoId && vr.ReporterId == userId);

        if (existingReport != null)
            throw new Exception("You have already reported this video");

        // Create report
        var report = new VideoReport
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            ReporterId = userId,
            Reason = request.Reason,
            Category = request.Category,
            Description = request.Description,
            Timestamp = request.Timestamp,
            Evidence = request.Evidence?.ToString(),
            Status = ReportStatus.Pending.ToString(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.VideoReports.Add(report);

        // Update video report count and potentially flag for review
        video.ReportCount = (video.ReportCount ?? 0) + 1;

        var totalReports = await _dbContext.VideoReports
            .CountAsync(vr => vr.VideoId == videoId && vr.Status != ReportStatus.Dismissed.ToString());

        if (totalReports >= 3) // Threshold for automatic review
        {
            video.RequiresModeration = true;
            video.ModerationStatus = ModerationStatus.Flagged.ToString();
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<List<ReportedVideoDto>> GetReportedVideosAsync(Guid tenantId, int page = 1, int pageSize = 20)
    {
        var reportedVideos = await _dbContext.VideoReports
            .Include(vr => vr.Video)
                .ThenInclude(v => v.User)
            .Include(vr => vr.Reporter)
            .Where(vr => vr.Video.TenantId == tenantId)
            .GroupBy(vr => vr.VideoId)
            .Select(g => new
            {
                Video = g.First().Video,
                ReportCount = g.Count(),
                Reports = g.ToList()
            })
            .OrderByDescending(g => g.ReportCount)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return reportedVideos.Select(rv => new ReportedVideoDto
        {
            VideoId = rv.Video.Id,
            Title = rv.Video.Title,
            ThumbnailUrl = rv.Video.ThumbnailPath ?? "",
            CreatorName = $"{rv.Video.User.FirstName} {rv.Video.User.LastName}",
            ReportCount = rv.ReportCount,
            Reports = rv.Reports.Select(r => new VideoReportDto
            {
                Id = r.Id,
                ReporterId = r.ReporterId,
                ReporterName = $"{r.Reporter.FirstName} {r.Reporter.LastName}",
                Reason = r.Reason,
                Category = r.Category,
                Description = r.Description,
                Timestamp = r.Timestamp,
                ReportedAt = r.CreatedAt,
                IsReviewed = r.Status != ReportStatus.Pending.ToString(),
                Status = Enum.Parse<ReportStatus>(r.Status)
            }).ToList(),
            CreatedAt = rv.Video.CreatedAt,
            Status = Enum.Parse<ModerationStatus>(rv.Video.ModerationStatus ?? "Pending"),
            IsUnderReview = rv.Video.RequiresModeration,
            ReviewerId = rv.Video.ModeratedBy?.ToString(),
            SeverityScore = CalculateSeverityScore(rv.Reports)
        }).ToList();
    }

    public async Task<List<PendingModerationDto>> GetPendingModerationAsync(Guid tenantId, int page = 1, int pageSize = 20)
    {
        var pendingVideos = await _dbContext.Videos
            .Include(v => v.User)
            .Include(v => v.ModerationFlags)
            .Where(v => v.TenantId == tenantId && 
                       (v.RequiresModeration == true || 
                        v.ModerationStatus == ModerationStatus.Pending.ToString() ||
                        v.ModerationStatus == ModerationStatus.UnderReview.ToString()))
            .OrderBy(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return pendingVideos.Select(v => new PendingModerationDto
        {
            VideoId = v.Id,
            Title = v.Title,
            ThumbnailUrl = v.ThumbnailPath ?? "",
            CreatorName = $"{v.User.FirstName} {v.User.LastName}",
            CreatorEmail = v.User.Email,
            UploadedAt = v.CreatedAt,
            DurationSeconds = v.DurationSeconds,
            PriorityScore = CalculatePriorityScore(v),
            FlagReasons = v.ModerationFlags.Select(f => f.Type).Distinct().ToList(),
            ReportCount = v.ReportCount ?? 0,
            IsAutoFlagged = v.ModerationFlags.Any(f => f.IsAutoDetected),
            QueueType = GetQueueType(v)
        }).ToList();
    }

    public async Task<ModerationStatsDto> GetModerationStatsAsync(Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        var start = startDate ?? DateTimeOffset.UtcNow.AddDays(-30);
        var end = endDate ?? DateTimeOffset.UtcNow;

        var videos = await _dbContext.Videos
            .Where(v => v.TenantId == tenantId && 
                       v.CreatedAt >= start && v.CreatedAt <= end)
            .ToListAsync();

        var stats = new ModerationStatsDto
        {
            TotalVideosModerated = videos.Count,
            AutoApproved = videos.Count(v => v.ModerationStatus == ModerationStatus.AutoApproved.ToString()),
            AutoRejected = videos.Count(v => v.ModerationStatus == ModerationStatus.AutoRejected.ToString()),
            ManuallyReviewed = videos.Count(v => v.ModeratedBy.HasValue),
            PendingReview = videos.Count(v => v.RequiresModeration == true),
            AverageReviewTime = await CalculateAverageReviewTime(tenantId, start, end),
            AccuracyRate = await CalculateModerationAccuracy(tenantId, start, end)
        };

        // Get flag categories
        var flags = await _dbContext.ModerationFlags
            .Include(mf => mf.Video)
            .Where(mf => mf.Video.TenantId == tenantId &&
                        mf.CreatedAt >= start && mf.CreatedAt <= end)
            .ToListAsync();

        stats.FlagCategories = flags
            .GroupBy(f => f.Type)
            .ToDictionary(g => g.Key, g => g.Count());

        // Get daily stats
        stats.DailyStats = await GetDailyModerationStats(tenantId, start, end);

        // Get moderator performance
        stats.ModeratorPerformance = await GetModeratorPerformance(tenantId, start, end);

        return stats;
    }

    public async Task<bool> UpdateModerationSettingsAsync(Guid tenantId, ModerationSettingsDto settings)
    {
        var existingSettings = await _dbContext.ModerationSettings
            .FirstOrDefaultAsync(ms => ms.TenantId == tenantId);

        if (existingSettings == null)
        {
            existingSettings = new ModerationSetting
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId
            };
            _dbContext.ModerationSettings.Add(existingSettings);
        }

        // Update settings
        existingSettings.EnableAutoModeration = settings.EnableAutoModeration;
        existingSettings.AutoApprovalThreshold = settings.AutoApprovalThreshold;
        existingSettings.AutoRejectionThreshold = settings.AutoRejectionThreshold;
        existingSettings.RestrictedCategories = string.Join(",", settings.RestrictedCategories);
        existingSettings.BannedWords = string.Join(",", settings.BannedWords);
        existingSettings.RequireManualReviewForNewCreators = settings.RequireManualReviewForNewCreators;
        existingSettings.NewCreatorThreshold = settings.NewCreatorThreshold;
        existingSettings.EnableCopyrightDetection = settings.EnableCopyrightDetection;
        existingSettings.EnableAdultContentDetection = settings.EnableAdultContentDetection;
        existingSettings.EnableViolenceDetection = settings.EnableViolenceDetection;
        existingSettings.EnableHateSpeechDetection = settings.EnableHateSpeechDetection;
        existingSettings.MaxVideoLength = settings.MaxVideoLength;
        existingSettings.EnableAppealsProcess = settings.EnableAppealsProcess;
        existingSettings.AppealReviewTimeHours = settings.AppealReviewTimeHours;
        existingSettings.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<ModerationSettingsDto> GetModerationSettingsAsync(Guid tenantId)
    {
        var settings = await _dbContext.ModerationSettings
            .FirstOrDefaultAsync(ms => ms.TenantId == tenantId);

        if (settings == null)
        {
            // Return default settings
            return new ModerationSettingsDto();
        }

        return new ModerationSettingsDto
        {
            EnableAutoModeration = settings.EnableAutoModeration,
            AutoApprovalThreshold = settings.AutoApprovalThreshold,
            AutoRejectionThreshold = settings.AutoRejectionThreshold,
            RestrictedCategories = settings.RestrictedCategories?.Split(',').ToList() ?? new List<string>(),
            BannedWords = settings.BannedWords?.Split(',').ToList() ?? new List<string>(),
            RequireManualReviewForNewCreators = settings.RequireManualReviewForNewCreators,
            NewCreatorThreshold = settings.NewCreatorThreshold,
            EnableCopyrightDetection = settings.EnableCopyrightDetection,
            EnableAdultContentDetection = settings.EnableAdultContentDetection,
            EnableViolenceDetection = settings.EnableViolenceDetection,
            EnableHateSpeechDetection = settings.EnableHateSpeechDetection,
            MaxVideoLength = settings.MaxVideoLength,
            EnableAppealsProcess = settings.EnableAppealsProcess,
            AppealReviewTimeHours = settings.AppealReviewTimeHours
        };
    }

    public async Task<List<ModerationActionDto>> GetModerationHistoryAsync(Guid videoId, Guid tenantId)
    {
        var actions = await _dbContext.ModerationActions
            .Include(ma => ma.Moderator)
            .Where(ma => ma.VideoId == videoId)
            .OrderByDescending(ma => ma.Timestamp)
            .ToListAsync();

        return actions.Select(a => new ModerationActionDto
        {
            Id = a.Id,
            VideoId = a.VideoId,
            ModeratorId = a.ModeratorId,
            ModeratorName = $"{a.Moderator.FirstName} {a.Moderator.LastName}",
            Action = Enum.Parse<ModerationAction>(a.Action),
            Reason = a.Reason,
            Notes = a.Notes,
            Timestamp = a.Timestamp,
            IsAutomated = a.IsAutomated,
            AutomationRule = a.AutomationRule
        }).ToList();
    }

    public async Task<bool> AppealModerationDecisionAsync(Guid videoId, Guid userId, Guid tenantId, string appealReason)
    {
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        // Check if user is the video owner
        if (video.UserId != userId)
            throw new Exception("Only the video owner can appeal");

        // Check if appeals are enabled
        var settings = await GetModerationSettingsAsync(tenantId);
        if (!settings.EnableAppealsProcess)
            throw new Exception("Appeals are not enabled");

        // Check if there's already an appeal
        var existingAppeal = await _dbContext.ModerationAppeals
            .FirstOrDefaultAsync(ma => ma.VideoId == videoId);

        if (existingAppeal != null)
            throw new Exception("An appeal has already been submitted for this video");

        // Create appeal
        var appeal = new ModerationAppeal
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            UserId = userId,
            OriginalDecision = video.ModerationStatus ?? "",
            AppealReason = appealReason,
            Status = AppealStatus.Pending.ToString(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.ModerationAppeals.Add(appeal);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<List<AutoModeratedContentDto>> GetAutoModeratedContentAsync(Guid tenantId, int page = 1, int pageSize = 20)
    {
        var content = await _dbContext.ModerationFlags
            .Include(mf => mf.Video)
                .ThenInclude(v => v.User)
            .Where(mf => mf.Video.TenantId == tenantId && 
                        mf.IsAutoDetected && 
                        !mf.IsResolved)
            .GroupBy(mf => mf.VideoId)
            .Select(g => new
            {
                Video = g.First().Video,
                Flags = g.ToList(),
                DetectionType = g.First().Type,
                Confidence = g.Average(f => f.Confidence)
            })
            .OrderByDescending(g => g.Confidence)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return content.Select(c => new AutoModeratedContentDto
        {
            VideoId = c.Video.Id,
            Title = c.Video.Title,
            CreatorName = $"{c.Video.User.FirstName} {c.Video.User.LastName}",
            DetectionType = c.DetectionType,
            Confidence = c.Confidence,
            Description = string.Join("; ", c.Flags.Select(f => f.Description)),
            DetectedAt = c.Flags.Min(f => f.CreatedAt),
            IsUnderReview = c.Video.RequiresModeration,
            ReviewerId = c.Video.ModeratedBy?.ToString(),
            Status = c.Video.ModerationStatus ?? "Pending"
        }).ToList();
    }

    private async Task<List<ModerationFlagDto>> CheckTitleContent(string title)
    {
        var flags = new List<ModerationFlagDto>();

        // Check for banned words
        var bannedWords = new[] { "spam", "clickbait", "misleading", "fake" };
        foreach (var word in bannedWords)
        {
            if (title.ToLower().Contains(word))
            {
                flags.Add(new ModerationFlagDto
                {
                    Id = Guid.NewGuid(),
                    Type = "banned_content",
                    Severity = "medium",
                    Description = $"Title contains banned word: {word}",
                    Confidence = 0.8,
                    IsAutoDetected = true,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }

        // Check for excessive caps or special characters
        if (title.Count(char.IsUpper) > title.Length * 0.5)
        {
            flags.Add(new ModerationFlagDto
            {
                Id = Guid.NewGuid(),
                Type = "formatting",
                Severity = "low",
                Description = "Title contains excessive capitalization",
                Confidence = 0.6,
                IsAutoDetected = true,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        return flags;
    }

    private async Task<List<ModerationFlagDto>> CheckDescriptionContent(string? description)
    {
        var flags = new List<ModerationFlagDto>();

        if (string.IsNullOrEmpty(description))
            return flags;

        // Check for suspicious links
        if (description.Contains("bit.ly") || description.Contains("t.co"))
        {
            flags.Add(new ModerationFlagDto
            {
                Id = Guid.NewGuid(),
                Type = "suspicious_links",
                Severity = "medium",
                Description = "Description contains shortened links",
                Confidence = 0.7,
                IsAutoDetected = true,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        return flags;
    }

    private async Task<List<ModerationFlagDto>> CheckCreatorHistory(User creator, Guid tenantId)
    {
        var flags = new List<ModerationFlagDto>();

        // Check creator's video count
        var videoCount = await _dbContext.Videos
            .CountAsync(v => v.UserId == creator.Id && v.TenantId == tenantId);

        if (videoCount < 3) // New creator
        {
            flags.Add(new ModerationFlagDto
            {
                Id = Guid.NewGuid(),
                Type = "new_creator",
                Severity = "low",
                Description = "Video from new creator - requires review",
                Confidence = 0.5,
                IsAutoDetected = true,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        // Check creator's moderation history
        var rejectedCount = await _dbContext.Videos
            .CountAsync(v => v.UserId == creator.Id && 
                        v.TenantId == tenantId && 
                        v.ModerationStatus == ModerationStatus.Rejected.ToString());

        if (rejectedCount > 2)
        {
            flags.Add(new ModerationFlagDto
            {
                Id = Guid.NewGuid(),
                Type = "creator_history",
                Severity = "high",
                Description = $"Creator has {rejectedCount} previously rejected videos",
                Confidence = 0.9,
                IsAutoDetected = true,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        return flags;
    }

    private string GenerateModerationSummary(List<ModerationFlagDto> flags, double overallScore)
    {
        if (!flags.Any())
            return "No issues detected - video appears to comply with guidelines";

        var highSeverityFlags = flags.Where(f => f.Severity == "high" || f.Severity == "critical").Count();
        var flagTypes = flags.Select(f => f.Type).Distinct().ToList();

        var summary = highSeverityFlags > 0 
            ? $"High priority: {highSeverityFlags} critical issues detected"
            : $"Moderate: {flags.Count} issues detected including {string.Join(", ", flagTypes.Take(3))}";

        return summary;
    }

    private double CalculateSeverityScore(List<VideoReport> reports)
    {
        var score = 0.0;
        foreach (var report in reports)
        {
            score += report.Category.ToLower() switch
            {
                "violence" => 0.9,
                "harassment" => 0.9,
                "hate_speech" => 0.95,
                "copyright" => 0.8,
                "spam" => 0.6,
                "misinformation" => 0.8,
                _ => 0.5
            };
        }
        return Math.Min(1.0, score / reports.Count);
    }

    private double CalculatePriorityScore(Video video)
    {
        var score = 0.0;

        // Base score on flags
        if (video.ModerationFlags.Any())
        {
            var maxSeverity = video.ModerationFlags.Max(f => f.Severity switch
            {
                "critical" => 1.0,
                "high" => 0.8,
                "medium" => 0.6,
                "low" => 0.4,
                _ => 0.2
            });
            score += maxSeverity;
        }

        // Add score for report count
        score += (video.ReportCount ?? 0) * 0.1;

        // Add score for video age (newer videos get higher priority)
        var ageInHours = (DateTimeOffset.UtcNow - video.CreatedAt).TotalHours;
        if (ageInHours < 24) score += 0.3;
        else if (ageInHours < 168) score += 0.2;

        return Math.Min(1.0, score);
    }

    private string GetQueueType(Video video)
    {
        var priorityScore = CalculatePriorityScore(video);
        
        return priorityScore switch
        {
            >= 0.8 => "priority",
            >= 0.5 => "standard",
            _ => "low"
        };
    }

    private async Task<double> CalculateAverageReviewTime(Guid tenantId, DateTimeOffset start, DateTimeOffset end)
    {
        var actions = await _dbContext.ModerationActions
            .Include(ma => ma.Video)
            .Where(ma => ma.Video.TenantId == tenantId &&
                        ma.Timestamp >= start && ma.Timestamp <= end)
            .ToListAsync();

        if (!actions.Any()) return 0;

        var reviewTimes = new List<double>();
        foreach (var action in actions)
        {
            var video = action.Video;
            if (video.CreatedAt.HasValue)
            {
                var reviewTime = (action.Timestamp - video.CreatedAt.Value).TotalHours;
                reviewTimes.Add(reviewTime);
            }
        }

        return reviewTimes.Any() ? reviewTimes.Average() : 0;
    }

    private async Task<double> CalculateModerationAccuracy(Guid tenantId, DateTimeOffset start, DateTimeOffset end)
    {
        // Simplified accuracy calculation
        // In production, this would compare automated decisions with human reviews
        return 0.85; // Placeholder
    }

    private async Task<List<DailyModerationStatsDto>> GetDailyModerationStats(Guid tenantId, DateTimeOffset start, DateTimeOffset end)
    {
        var stats = new List<DailyModerationStatsDto>();
        var current = start.Date;

        while (current <= end.Date)
        {
            var dayStart = new DateTimeOffset(current, TimeSpan.Zero);
            var dayEnd = dayStart.AddDays(1);

            var dayVideos = await _dbContext.Videos
                .Where(v => v.TenantId == tenantId &&
                           v.CreatedAt >= dayStart && v.CreatedAt < dayEnd)
                .ToListAsync();

            stats.Add(new DailyModerationStatsDto
            {
                Date = DateOnly.FromDateTime(current),
                VideosProcessed = dayVideos.Count,
                Approved = dayVideos.Count(v => v.ModerationStatus == ModerationStatus.Approved.ToString() || 
                                             v.ModerationStatus == ModerationStatus.AutoApproved.ToString()),
                Rejected = dayVideos.Count(v => v.ModerationStatus == ModerationStatus.Rejected.ToString() || 
                                             v.ModerationStatus == ModerationStatus.AutoRejected.ToString()),
                Escalated = dayVideos.Count(v => v.ModerationStatus == ModerationStatus.UnderReview.ToString()),
                AverageProcessingTime = 2.5 // Placeholder
            });

            current = current.AddDays(1);
        }

        return stats;
    }

    private async Task<List<ModeratorPerformanceDto>> GetModeratorPerformance(Guid tenantId, DateTimeOffset start, DateTimeOffset end)
    {
        var moderators = await _dbContext.ModerationActions
            .Include(ma => ma.Moderator)
            .Where(ma => ma.Moderator.TenantId == tenantId &&
                        ma.Timestamp >= start && ma.Timestamp <= end)
            .GroupBy(ma => ma.ModeratorId)
            .Select(g => new
            {
                ModeratorId = g.Key,
                Moderator = g.First().Moderator,
                Actions = g.ToList()
            })
            .ToListAsync();

        return moderators.Select(m => new ModeratorPerformanceDto
        {
            ModeratorId = m.ModeratorId,
            ModeratorName = $"{m.Moderator.FirstName} {m.Moderator.LastName}",
            VideosReviewed = m.Actions.Count,
            ActionsTaken = m.Actions.Count(a => a.Action != ModerationAction.Approved.ToString()),
            AverageTimePerVideo = 5.2, // Placeholder
            AccuracyRate = 0.92, // Placeholder
            AppealsReceived = 0, // Would need to calculate from appeals table
            AppealsUpheld = 0
        }).ToList();
    }
}
