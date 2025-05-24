using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Achievement
    {
        [Key]
        public string AchievementId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        // Icon emoji or path to image file
        public string Icon { get; set; }

        // Achievement category (e.g., Offense, Defense, Participation, etc.)
        public string Category { get; set; }

        // Requirement value to unlock (e.g., "10" for "Score 10 three-pointers")
        public int RequirementValue { get; set; }

        // Points awarded for unlocking this achievement
        public int PointValue { get; set; }

        // Difficulty tier (e.g., Bronze, Silver, Gold, Platinum)
        public string Tier { get; set; }

        // If this achievement can be earned multiple times
        public bool IsRepeatable { get; set; }

        // Timestamp for when this achievement was created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Custom background color for display
        public string BackgroundColor { get; set; }

        // For achievements that follow a sequence
        public int? SequenceOrder { get; set; }

        // For grouping related achievements
        public string AchievementGroupId { get; set; }

        // Optional data for custom achievement logic
        public string CustomData { get; set; }
    }

    // Junction table to track which users have which achievements
    public class UserAchievement
    {
        [Key]
        public string UserAchievementId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string ProfileId { get; set; }

        [Required]
        public string AchievementId { get; set; }

        // When this achievement was earned
        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

        // For repeatable achievements, how many times it's been earned
        public int TimesEarned { get; set; } = 1;

        // For achievements with progress tracking (e.g., "7/10 three-pointers")
        public int? Progress { get; set; }

        // For display when showing achievement details
        public string GameId { get; set; }
        public string RunId { get; set; }

        // Navigation properties
        [ForeignKey("ProfileId")]
        public Profile Profile { get; set; }

        [ForeignKey("AchievementId")]
        public Achievement Achievement { get; set; }
    }
}
