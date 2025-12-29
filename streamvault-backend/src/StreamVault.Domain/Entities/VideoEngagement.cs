using System;
using StreamVault.Domain.Interfaces;
using System.Collections.Generic;

namespace StreamVault.Domain.Entities
{
    public class VideoEngagement : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid VideoId { get; set; }
        public Video Video { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public EngagementType Type { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public enum EngagementType
    {
        Like = 1,
        Dislike = 2,
        Comment = 3,
        Share = 4,
        Download = 5,
        Bookmark = 6,
        Report = 7,
        WatchComplete = 8,
        Skip = 9,
        Pause = 10,
        Resume = 11
    }
}
