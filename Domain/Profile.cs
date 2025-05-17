using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class Profile
    {
        [Key]
        public string? ProfileId { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Height { get; set; }
        public string? Weight { get; set; }
        public string? Position { get; set; }
        public string? Ranking { get; set; }
        public string? StarRating { get; set; }
        public string? QRCode { get; set; }
        public string? Bio { get; set; }
        public string? ImageURL { get; set; }
        public string? PlayerArchetype { get; set; }
        public string? City { get; set; }
        public string? Zip { get; set; }
        public string? PlayerNumber { get; set; }
        public string? Status { get; set; }
        public int? Points { get; set; }
        public string? LastRunDate { get; set; }
        public bool? TopRecord { get; set; }
        public bool? OnSquad { get; set; }
       

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
        public string? RatedCount { get; set; }

        [NotMapped]
        public string? FollowersCount { get; set; }

        [NotMapped]
        public string? FollowingCount { get; set; }
        [NotMapped]
        public string? Email { get; set; }

        [NotMapped]
        public List<PrivateRun>? PrivateRunList { get; set; }

        [NotMapped]
        public List<PrivateRunInvite>? PrivateRunInviteList { get; set; }
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
        public string? WinPercentage { get; set; }
        [NotMapped]
        public string? Team { get; set; }

        [NotMapped]
        public string? TotalGames { get; set; }
        [NotMapped]
        public int? TotalWins { get; set; }
        [NotMapped]
        public int? TotalLosses { get; set; }
       
        public Setting? Setting { get; set; }
        [NotMapped]
        public ScoutingReport? ScoutingReport { get; set; }
        [NotMapped]
        public GameStatistics GameStatistics { get; set; }
        [NotMapped]
        public string? AcceptedInvite { get; set; }
        [NotMapped]
        public string? FullName { get; set; }




    }
}