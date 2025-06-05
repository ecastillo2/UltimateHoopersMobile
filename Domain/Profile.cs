using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class Profile
    {
        [Key]
        public string? ProfileId { get; set; }
        public string? SubscriptionId { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Height { get; set; }
        public string? Weight { get; set; }
        public string? Position { get; set; }
        public int? Ranking { get; set; }
        public int? StarRating { get; set; }
        public string? QRCode { get; set; }
        public string? Bio { get; set; }
        public string? ImageURL { get; set; }
        public string? PlayerArchetype { get; set; }
        public string? City { get; set; }
        public string? Zip { get; set; }
        public string? PlayerNumber { get; set; }
        public string? Status { get; set; }
        public int? Points { get; set; }
        public DateTime? LastRunDate { get; set; }
        public bool? TopRecord { get; set; }
        public bool? OnSquad { get; set; }
        public bool? PaymentRequired { get; set; }


        [NotMapped]
        public Squad? Squad { get; set; }
        [NotMapped]
        public SquadTeam? SquadTeam { get; set; }

        [NotMapped]
        public string? LastLoginDate { get; set; }
        [NotMapped]
        public string? RequestResponseText { get; set; }
        [NotMapped]
        public string? SubId { get; set; }
        [NotMapped]
        public string? SegId { get; set; }
        
        [NotMapped]
        public string? FirstName { get; set; }

        [NotMapped]
        public string? LastName { get; set; }

        [NotMapped]
        public bool? Followed { get; set; }

        [NotMapped]
        public int? RatedCount { get; set; }

        [NotMapped]
        public int? FollowersCount { get; set; }

        [NotMapped]
        public int? FollowingCount { get; set; }
        [NotMapped]
        public string? Email { get; set; }

        [NotMapped]
        public List<Run>? RunList { get; set; }

        [NotMapped]
        public List<JoinedRun>? JoinedRunList { get; set; }
        [NotMapped]
        public List<Notification>? NotificationList { get; set; }

        [NotMapped]
        public List<PlayerComment>? PlayerCommentList { get; set; }
        [NotMapped]
        public string? InviteStatus { get; set; }

        [NotMapped]
        public string? WinOrLose { get; set; }
        [NotMapped]
        public string? PointsScored { get; set; }

        [NotMapped]
        public double? WinPercentage { get; set; }
        [NotMapped]
        public string? Team { get; set; }

        [NotMapped]
        public int? TotalGames { get; set; }
        [NotMapped]
        public int? TotalWins { get; set; }
        [NotMapped]
        public int? TotalLosses { get; set; }
       
        public Setting? Setting { get; set; }
       
        public ScoutingReport? ScoutingReport { get; set; }
        [NotMapped]
        public GameStatistics GameStatistics { get; set; }
        [NotMapped]
        public string? AcceptedInvite { get; set; }
        [NotMapped]
        public string? FullName { get; set; }
        [NotMapped]
        public List<Profile>? FollowersList { get; set; }

        [NotMapped]
        public List<Profile>? FollowingList { get; set; }
        [NotMapped]
        public Subscription? Subscription { get; set; }




    }
}