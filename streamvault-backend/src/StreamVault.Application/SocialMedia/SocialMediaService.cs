using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StreamVault.Application.SocialMedia.DTOs;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;
using System.Text.Json;

namespace StreamVault.Application.SocialMedia;

public class SocialMediaService : ISocialMediaService
{
    private readonly StreamVaultDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SocialMediaService> _logger;
    private readonly HttpClient _httpClient;

    public SocialMediaService(StreamVaultDbContext dbContext, IConfiguration configuration, ILogger<SocialMediaService> logger, HttpClient httpClient)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> ConnectSocialAccountAsync(Guid userId, Guid tenantId, ConnectSocialAccountRequest request)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        // Check if account already connected
        var existingAccount = await _dbContext.SocialAccounts
            .FirstOrDefaultAsync(sa => sa.UserId == userId && sa.Platform.ToLower() == request.Platform.ToLower());

        if (existingAccount != null)
        {
            // Update existing account
            existingAccount.AccessToken = request.AccessToken;
            existingAccount.RefreshToken = request.RefreshToken;
            existingAccount.TokenExpiresAt = request.TokenExpiresAt;
            existingAccount.PlatformUserId = request.UserId;
            existingAccount.Username = request.Username;
            existingAccount.IsActive = true;
            existingAccount.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            // Create new social account
            var socialAccount = new SocialAccount
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Platform = request.Platform.ToLower(),
                AccessToken = request.AccessToken,
                RefreshToken = request.RefreshToken,
                TokenExpiresAt = request.TokenExpiresAt,
                PlatformUserId = request.UserId,
                Username = request.Username,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.SocialAccounts.Add(socialAccount);
        }

        await _dbContext.SaveChangesAsync();

        // Fetch profile information
        await FetchSocialProfileAsync(userId, request.Platform.ToLower());

        return true;
    }

    public async Task<bool> DisconnectSocialAccountAsync(Guid userId, Guid tenantId, string platform)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var account = await _dbContext.SocialAccounts
            .FirstOrDefaultAsync(sa => sa.UserId == userId && sa.Platform.ToLower() == platform.ToLower());

        if (account == null)
            throw new Exception("Social account not found");

        _dbContext.SocialAccounts.Remove(account);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<List<SocialAccountDto>> GetUserSocialAccountsAsync(Guid userId, Guid tenantId)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var accounts = await _dbContext.SocialAccounts
            .Where(sa => sa.UserId == userId)
            .ToListAsync();

        return accounts.Select(MapToSocialAccountDto).ToList();
    }

    public async Task<bool> PostToSocialMediaAsync(Guid userId, Guid tenantId, PostToSocialMediaRequest request)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var results = new List<bool>();

        foreach (var platform in request.Platforms)
        {
            try
            {
                var account = await _dbContext.SocialAccounts
                    .FirstOrDefaultAsync(sa => sa.UserId == userId && sa.Platform.ToLower() == platform.ToLower());

                if (account == null)
                {
                    _logger.LogWarning($"No {platform} account connected for user {userId}");
                    continue;
                }

                // Check if token is valid and refresh if needed
                if (account.TokenExpiresAt.HasValue && account.TokenExpiresAt.Value <= DateTimeOffset.UtcNow)
                {
                    await RefreshTokenAsync(userId, tenantId, platform);
                }

                // Post to platform
                var result = await PostToPlatformAsync(account, request);
                results.Add(result);

                if (result)
                {
                    // Save post record
                    var socialPost = new SocialPost
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Platform = platform.ToLower(),
                        Content = request.Content,
                        VideoUrl = request.VideoUrl,
                        MediaUrls = string.Join(",", request.MediaUrls),
                        Hashtags = string.Join(",", request.Hashtags),
                        Status = PostStatus.Posted.ToString(),
                        PostedAt = DateTimeOffset.UtcNow,
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    _dbContext.SocialPosts.Add(socialPost);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to post to {platform}");
                results.Add(false);
            }
        }

        await _dbContext.SaveChangesAsync();
        return results.Any(r => r);
    }

    public async Task<List<SocialPostDto>> GetSocialPostsAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var posts = await _dbContext.SocialPosts
            .Where(sp => sp.UserId == userId)
            .OrderByDescending(sp => sp.PostedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return posts.Select(MapToSocialPostDto).ToList();
    }

    public async Task<bool> ShareVideoAsync(Guid videoId, Guid userId, Guid tenantId, ShareVideoRequest request)
    {
        // Verify video exists and user has permission
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        // Create share request
        var postRequest = new PostToSocialMediaRequest
        {
            Platforms = request.Platforms,
            Content = $"{request.Title}\n\n{request.Description}",
            VideoUrl = $"{_configuration["BaseUrl"]}/videos/{videoId}",
            Hashtags = request.Hashtags,
            Mentions = request.Mentions,
            SchedulePost = request.SchedulePost,
            ScheduledAt = request.ScheduledAt
        };

        // Add video thumbnail to media
        if (!string.IsNullOrEmpty(video.ThumbnailPath))
        {
            postRequest.MediaUrls.Add(video.ThumbnailPath);
        }

        // Post to social media
        var result = await PostToSocialMediaAsync(userId, tenantId, postRequest);

        if (result)
        {
            // Save share record
            var share = new VideoShare
            {
                Id = Guid.NewGuid(),
                VideoId = videoId,
                UserId = userId,
                Platforms = string.Join(",", request.Platforms),
                ShareStatus = ShareStatus.Shared.ToString(),
                SharedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.VideoShares.Add(share);
            await _dbContext.SaveChangesAsync();
        }

        return result;
    }

    public async Task<List<SharedVideoDto>> GetSharedVideosAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var shares = await _dbContext.VideoShares
            .Include(vs => vs.Video)
            .Where(vs => vs.UserId == userId)
            .OrderByDescending(vs => vs.SharedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return shares.Select(MapToSharedVideoDto).ToList();
    }

    public async Task<SocialAnalyticsDto> GetSocialAnalyticsAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Verify video exists
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.TenantId == tenantId);

        if (video == null)
            throw new Exception("Video not found");

        var shares = await _dbContext.VideoShares
            .Where(vs => vs.VideoId == videoId)
            .Include(vs => vs.SocialPosts)
            .ToListAsync();

        var analytics = new SocialAnalyticsDto
        {
            VideoId = videoId,
            PlatformStats = new Dictionary<string, SocialPlatformStats>()
        };

        foreach (var share in shares)
        {
            foreach (var post in share.SocialPosts)
            {
                if (!analytics.PlatformStats.ContainsKey(post.Platform))
                {
                    analytics.PlatformStats[post.Platform] = new SocialPlatformStats
                    {
                        Platform = post.Platform
                    };
                }

                var stats = analytics.PlatformStats[post.Platform];
                stats.Likes += post.Likes;
                stats.Shares += post.Shares;
                stats.Comments += post.Comments;
                stats.Views += post.Views;
            }
        }

        // Calculate totals
        analytics.TotalLikes = analytics.PlatformStats.Values.Sum(s => s.Likes);
        analytics.TotalShares = analytics.PlatformStats.Values.Sum(s => s.Shares);
        analytics.TotalComments = analytics.PlatformStats.Values.Sum(s => s.Comments);
        analytics.TotalViews = analytics.PlatformStats.Values.Sum(s => s.Views);

        // Calculate engagement rate
        if (analytics.TotalViews > 0)
        {
            analytics.OverallEngagementRate = (double)(analytics.TotalLikes + analytics.TotalComments + analytics.TotalShares) / analytics.TotalViews * 100;
        }

        return analytics;
    }

    public async Task<string> GetAuthUrlAsync(string platform, Guid tenantId, string redirectUri)
    {
        var platformConfig = _configuration.GetSection($"SocialMedia:{platform}");
        if (!platformConfig.Exists())
            throw new Exception($"Platform {platform} not configured");

        var clientId = platformConfig["ClientId"];
        var scopes = platformConfig["Scopes"] ?? "";

        // Generate auth URL based on platform
        return platform.ToLower() switch
        {
            "facebook" => $"https://www.facebook.com/v18.0/dialog/oauth?client_id={clientId}&redirect_uri={redirectUri}&scope={scopes}&response_type=code",
            "twitter" => $"https://twitter.com/i/oauth2/authorize?client_id={clientId}&redirect_uri={redirectUri}&scope={scopes}&response_type=code&code_challenge=challenge&code_challenge_method=plain",
            "instagram" => $"https://api.instagram.com/oauth/authorize?client_id={clientId}&redirect_uri={redirectUri}&scope={scopes}&response_type=code",
            "youtube" => $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientId}&redirect_uri={redirectUri}&scope={scopes}&response_type=code&access_type=offline",
            "tiktok" => $"https://www.tiktok.com/v2/auth/authorize?client_key={clientId}&redirect_uri={redirectUri}&scope={scopes}&response_type=code",
            "linkedin" => $"https://www.linkedin.com/oauth/v2/authorization?client_id={clientId}&redirect_uri={redirectUri}&scope={scopes}&response_type=code",
            _ => throw new Exception($"Unsupported platform: {platform}")
        };
    }

    public async Task<SocialAuthResultDto> AuthenticateAsync(string platform, string code, string state, Guid tenantId)
    {
        var platformConfig = _configuration.GetSection($"SocialMedia:{platform}");
        if (!platformConfig.Exists())
            throw new Exception($"Platform {platform} not configured");

        var clientId = platformConfig["ClientId"];
        var clientSecret = platformConfig["ClientSecret"];
        var redirectUri = platformConfig["RedirectUri"];

        try
        {
            // Exchange code for access token
            var tokenResponse = await ExchangeCodeForTokenAsync(platform, clientId, clientSecret, redirectUri, code);

            // Get user profile
            var profile = await GetUserProfileAsync(platform, tokenResponse.AccessToken);

            return new SocialAuthResultDto
            {
                Success = true,
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                TokenExpiresAt = tokenResponse.ExpiresAt,
                PlatformUserId = profile.Id,
                Username = profile.Username,
                DisplayName = profile.DisplayName,
                AvatarUrl = profile.AvatarUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Authentication failed for {platform}");
            return new SocialAuthResultDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> RefreshTokenAsync(Guid userId, Guid tenantId, string platform)
    {
        var account = await _dbContext.SocialAccounts
            .FirstOrDefaultAsync(sa => sa.UserId == userId && sa.Platform.ToLower() == platform.ToLower());

        if (account == null || string.IsNullOrEmpty(account.RefreshToken))
            return false;

        try
        {
            var platformConfig = _configuration.GetSection($"SocialMedia:{platform}");
            var clientId = platformConfig["ClientId"];
            var clientSecret = platformConfig["ClientSecret"];

            var newToken = await RefreshAccessTokenAsync(platform, clientId, clientSecret, account.RefreshToken);

            account.AccessToken = newToken.AccessToken;
            account.RefreshToken = newToken.RefreshToken;
            account.TokenExpiresAt = newToken.ExpiresAt;
            account.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to refresh token for {platform}");
            return false;
        }
    }

    public async Task<bool> SyncVideosFromSocialAsync(Guid userId, Guid tenantId, string platform)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var account = await _dbContext.SocialAccounts
            .FirstOrDefaultAsync(sa => sa.UserId == userId && sa.Platform.ToLower() == platform.ToLower());

        if (account == null)
            throw new Exception("Social account not found");

        try
        {
            // Fetch videos from platform
            var videos = await FetchVideosFromPlatformAsync(account);

            // Process and save videos
            foreach (var videoData in videos)
            {
                var existingImport = await _dbContext.ImportedVideos
                    .FirstOrDefaultAsync(iv => iv.UserId == userId && iv.OriginalVideoId == videoData.Id && iv.OriginalPlatform == platform);

                if (existingImport == null)
                {
                    // Create video record
                    var video = new Video
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        TenantId = tenantId,
                        Title = videoData.Title,
                        Description = videoData.Description,
                        ThumbnailPath = videoData.ThumbnailUrl,
                        DurationSeconds = videoData.DurationSeconds,
                        ViewCount = videoData.Views,
                        IsPublic = videoData.IsPublic,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow
                    };

                    _dbContext.Videos.Add(video);

                    // Create import record
                    var import = new ImportedVideo
                    {
                        Id = Guid.NewGuid(),
                        VideoId = video.Id,
                        UserId = userId,
                        OriginalPlatform = platform,
                        OriginalVideoId = videoData.Id,
                        OriginalUrl = videoData.Url,
                        OriginalViews = videoData.Views,
                        OriginalLikes = videoData.Likes,
                        OriginalComments = videoData.Comments,
                        ImportStatus = ImportStatus.Completed.ToString(),
                        ImportedAt = DateTimeOffset.UtcNow
                    };

                    _dbContext.ImportedVideos.Add(import);
                }
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to sync videos from {platform}");
            return false;
        }
    }

    public async Task<bool> ImportSocialVideosAsync(Guid userId, Guid tenantId, ImportSocialVideosRequest request)
    {
        // Implementation for importing specific videos
        // Similar to SyncVideosFromSocialAsync but for specific videos
        return true;
    }

    public async Task<List<ImportedVideoDto>> GetImportedVideosAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var imports = await _dbContext.ImportedVideos
            .Include(iv => iv.Video)
            .Where(iv => iv.UserId == userId)
            .OrderByDescending(iv => iv.ImportedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return imports.Select(MapToImportedVideoDto).ToList();
    }

    public async Task<SocialMediaAnalyticsDto> GetSocialMediaAnalyticsAsync(Guid userId, Guid tenantId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var start = startDate ?? DateTimeOffset.UtcNow.AddDays(-30);
        var end = endDate ?? DateTimeOffset.UtcNow;

        var posts = await _dbContext.SocialPosts
            .Where(sp => sp.UserId == userId && sp.PostedAt >= start && sp.PostedAt <= end)
            .ToListAsync();

        var accounts = await _dbContext.SocialAccounts
            .Where(sa => sa.UserId == userId)
            .ToListAsync();

        var analytics = new SocialMediaAnalyticsDto
        {
            Platforms = new Dictionary<string, SocialPlatformAnalytics>(),
            TotalPosts = posts.Count,
            TotalEngagement = posts.Sum(p => p.Likes + p.Comments + p.Shares)
        };

        // Calculate per-platform analytics
        foreach (var account in accounts)
        {
            var platformPosts = posts.Where(p => p.Platform == account.Platform).ToList();
            
            analytics.Platforms[account.Platform] = new SocialPlatformAnalytics
            {
                Platform = account.Platform,
                Posts = platformPosts.Count,
                Followers = account.FollowerCount,
                Following = account.FollowingCount,
                EngagementRate = platformPosts.Any() ? 
                    (double)platformPosts.Sum(p => p.Likes + p.Comments + p.Shares) / platformPosts.Sum(p => p.Views) * 100 : 0
            };
        }

        analytics.AverageEngagementRate = analytics.Platforms.Values.Any() ? 
            analytics.Platforms.Values.Average(p => p.EngagementRate) : 0;

        return analytics;
    }

    public async Task<List<SocialEngagementDto>> GetEngagementMetricsAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Implementation for engagement metrics
        return new List<SocialEngagementDto>();
    }

    public async Task<SocialTrendsDto> GetSocialTrendsAsync(Guid tenantId, string platform)
    {
        // Implementation for social trends
        return new SocialTrendsDto
        {
            TrendingTopics = new List<TrendingTopicDto>(),
            TrendingHashtags = new List<TrendingHashtagDto>(),
            ViralContent = new List<ViralContentDto>(),
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public async Task<Guid> SchedulePostAsync(Guid userId, Guid tenantId, SchedulePostRequest request)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var scheduledPost = new ScheduledPost
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Platforms = string.Join(",", request.Platforms),
            Content = request.Content,
            VideoUrl = request.VideoUrl,
            MediaUrls = string.Join(",", request.MediaUrls),
            Hashtags = string.Join(",", request.Hashtags),
            ScheduledAt = request.ScheduledAt,
            RecurringPost = request.RecurringPost,
            RecurrencePattern = request.RecurrencePattern,
            RecurrenceEnd = request.RecurrenceEnd,
            Status = ScheduledPostStatus.Pending.ToString(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.ScheduledPosts.Add(scheduledPost);
        await _dbContext.SaveChangesAsync();

        return scheduledPost.Id;
    }

    public async Task<bool> UpdateScheduledPostAsync(Guid postId, Guid userId, Guid tenantId, UpdateScheduleRequest request)
    {
        // Implementation for updating scheduled post
        return true;
    }

    public async Task<bool> CancelScheduledPostAsync(Guid postId, Guid userId, Guid tenantId)
    {
        // Implementation for canceling scheduled post
        return true;
    }

    public async Task<List<ScheduledPostDto>> GetScheduledPostsAsync(Guid userId, Guid tenantId, int page = 1, int pageSize = 20)
    {
        // Verify user belongs to tenant
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

        if (user == null)
            throw new Exception("User not found");

        var posts = await _dbContext.ScheduledPosts
            .Where(sp => sp.UserId == userId)
            .OrderBy(sp => sp.ScheduledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return posts.Select(MapToScheduledPostDto).ToList();
    }

    public async Task<List<SocialCommentDto>> GetSocialCommentsAsync(Guid videoId, Guid userId, Guid tenantId, string platform)
    {
        // Implementation for getting social comments
        return new List<SocialCommentDto>();
    }

    public async Task<bool> ReplyToSocialCommentAsync(Guid videoId, Guid userId, Guid tenantId, ReplyToCommentRequest request)
    {
        // Implementation for replying to social comments
        return true;
    }

    public async Task<bool> ModerateSocialCommentAsync(Guid commentId, Guid userId, Guid tenantId, ModerateCommentRequest request)
    {
        // Implementation for moderating social comments
        return true;
    }

    public async Task<bool> TrackHashtagAsync(Guid userId, Guid tenantId, string platform, string hashtag)
    {
        // Implementation for tracking hashtags
        return true;
    }

    public async Task<List<HashtagAnalyticsDto>> GetHashtagAnalyticsAsync(Guid userId, Guid tenantId, string platform)
    {
        // Implementation for hashtag analytics
        return new List<HashtagAnalyticsDto>();
    }

    public async Task<List<TrendingHashtagDto>> GetTrendingHashtagsAsync(Guid tenantId, string platform)
    {
        // Implementation for getting trending hashtags
        return new List<TrendingHashtagDto>();
    }

    public async Task<List<SocialMentionDto>> GetMentionsAsync(Guid userId, Guid tenantId, string platform, int page = 1, int pageSize = 20)
    {
        // Implementation for getting mentions
        return new List<SocialMentionDto>();
    }

    public async Task<bool> RespondToMentionAsync(Guid mentionId, Guid userId, Guid tenantId, RespondToMentionRequest request)
    {
        // Implementation for responding to mentions
        return true;
    }

    public async Task<SocialSentimentDto> GetSentimentAnalysisAsync(Guid videoId, Guid userId, Guid tenantId)
    {
        // Implementation for sentiment analysis
        return new SocialSentimentDto
        {
            OverallScore = 0.5,
            Breakdown = new SentimentBreakdown
            {
                Positive = 0.6,
                Negative = 0.2,
                Neutral = 0.2
            }
        };
    }

    // Private helper methods
    private async Task FetchSocialProfileAsync(Guid userId, string platform)
    {
        var account = await _dbContext.SocialAccounts
            .FirstOrDefaultAsync(sa => sa.UserId == userId && sa.Platform == platform);

        if (account == null)
            return;

        try
        {
            var profile = await GetUserProfileAsync(platform, account.AccessToken);
            
            account.DisplayName = profile.DisplayName;
            account.AvatarUrl = profile.AvatarUrl;
            account.FollowerCount = profile.FollowerCount;
            account.FollowingCount = profile.FollowingCount;
            account.PostCount = profile.PostCount;
            account.LastSyncAt = DateTimeOffset.UtcNow;
            account.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to fetch profile for {platform}");
        }
    }

    private async Task<bool> PostToPlatformAsync(SocialAccount account, PostToSocialMediaRequest request)
    {
        // Implementation would vary by platform
        // This is a placeholder for actual platform API calls
        await Task.Delay(100); // Simulate API call
        return true;
    }

    private async Task<TokenResponse> ExchangeCodeForTokenAsync(string platform, string clientId, string clientSecret, string redirectUri, string code)
    {
        // Implementation for exchanging code for token
        // This would vary by platform
        return new TokenResponse
        {
            AccessToken = "mock_access_token",
            RefreshToken = "mock_refresh_token",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };
    }

    private async Task<UserProfile> GetUserProfileAsync(string platform, string accessToken)
    {
        // Implementation for getting user profile
        // This would vary by platform
        return new UserProfile
        {
            Id = "mock_user_id",
            Username = "mock_username",
            DisplayName = "Mock User",
            AvatarUrl = "https://example.com/avatar.jpg",
            FollowerCount = 1000,
            FollowingCount = 500,
            PostCount = 100
        };
    }

    private async Task<TokenResponse> RefreshAccessTokenAsync(string platform, string clientId, string clientSecret, string refreshToken)
    {
        // Implementation for refreshing access token
        // This would vary by platform
        return new TokenResponse
        {
            AccessToken = "new_access_token",
            RefreshToken = "new_refresh_token",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };
    }

    private async Task<List<VideoData>> FetchVideosFromPlatformAsync(SocialAccount account)
    {
        // Implementation for fetching videos from platform
        // This would vary by platform
        return new List<VideoData>();
    }

    private SocialAccountDto MapToSocialAccountDto(SocialAccount account)
    {
        return new SocialAccountDto
        {
            Id = account.Id,
            UserId = account.UserId,
            Platform = account.Platform,
            PlatformUserId = account.PlatformUserId,
            Username = account.Username,
            DisplayName = account.DisplayName,
            AvatarUrl = account.AvatarUrl,
            IsActive = account.IsActive,
            ConnectedAt = account.CreatedAt,
            LastSyncAt = account.LastSyncAt,
            FollowerCount = account.FollowerCount,
            FollowingCount = account.FollowingCount,
            PostCount = account.PostCount
        };
    }

    private SocialPostDto MapToSocialPostDto(SocialPost post)
    {
        return new SocialPostDto
        {
            Id = post.Id,
            UserId = post.UserId,
            Platform = post.Platform,
            PlatformPostId = post.PlatformPostId,
            Content = post.Content,
            VideoUrl = post.VideoUrl,
            MediaUrls = string.IsNullOrEmpty(post.MediaUrls) ? new List<string>() : post.MediaUrls.Split(',').ToList(),
            Hashtags = string.IsNullOrEmpty(post.Hashtags) ? new List<string>() : post.Hashtags.Split(',').ToList(),
            PostUrl = post.PostUrl,
            PostedAt = post.PostedAt,
            Likes = post.Likes,
            Shares = post.Shares,
            Comments = post.Comments,
            Views = post.Views,
            Status = Enum.Parse<PostStatus>(post.Status)
        };
    }

    private SharedVideoDto MapToSharedVideoDto(VideoShare share)
    {
        return new SharedVideoDto
        {
            Id = share.Id,
            VideoId = share.VideoId,
            VideoTitle = share.Video?.Title ?? "",
            Platform = share.Platforms,
            PostUrl = share.PostUrl,
            SharedAt = share.SharedAt,
            Likes = share.Likes,
            Shares = share.Shares,
            Comments = share.Comments,
            Views = share.Views,
            Status = Enum.Parse<ShareStatus>(share.ShareStatus)
        };
    }

    private ImportedVideoDto MapToImportedVideoDto(ImportedVideo import)
    {
        return new ImportedVideoDto
        {
            Id = import.Id,
            VideoId = import.VideoId,
            OriginalPlatform = import.OriginalPlatform,
            OriginalVideoId = import.OriginalVideoId,
            OriginalUrl = import.OriginalUrl,
            ImportedAt = import.ImportedAt,
            OriginalViews = import.OriginalViews,
            OriginalLikes = import.OriginalLikes,
            OriginalComments = import.OriginalComments,
            Status = Enum.Parse<ImportStatus>(import.ImportStatus)
        };
    }

    private ScheduledPostDto MapToScheduledPostDto(ScheduledPost post)
    {
        return new ScheduledPostDto
        {
            Id = post.Id,
            UserId = post.UserId,
            Platforms = string.IsNullOrEmpty(post.Platforms) ? new List<string>() : post.Platforms.Split(',').ToList(),
            Content = post.Content,
            VideoUrl = post.VideoUrl,
            ScheduledAt = post.ScheduledAt,
            RecurringPost = post.RecurringPost,
            RecurrencePattern = post.RecurrencePattern,
            RecurrenceEnd = post.RecurrenceEnd,
            Status = Enum.Parse<ScheduledPostStatus>(post.Status),
            CreatedAt = post.CreatedAt,
            PostedAt = post.PostedAt,
            ErrorMessage = post.ErrorMessage
        };
    }
}

// Helper classes
internal class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
}

internal class UserProfile
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public int PostCount { get; set; }
}

internal class VideoData
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public int Views { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
    public bool IsPublic { get; set; }
}
