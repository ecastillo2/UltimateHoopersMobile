// File: WebAPI/DTOs/ProfileDTOs.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    /// <summary>
    /// Request for updating a profile
    /// </summary>
    public class UpdateProfileRequestDto
    {
        public string UserName { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string Position { get; set; }
        public string Bio { get; set; }
        public string PlayerArchetype { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
    }

    /// <summary>
    /// Request for updating profile settings
    /// </summary>
    public class UpdateSettingsRequestDto
    {
        public bool AllowComments { get; set; } = true;
        public bool ShowGameHistory { get; set; } = true;
        public bool AllowEmailNotification { get; set; } = true;
    }

    /// <summary>
    /// Basic profile information
    /// </summary>
    public class ProfileDto
    {
        public string ProfileId { get; set; }
        public string UserName { get; set; }
        public string ImageURL { get; set; }
        public string PlayerArchetype { get; set; }
        public string City { get; set; }
        public string Ranking { get; set; }
        public string StarRating { get; set; }
        public string FollowersCount { get; set; }
        public string FollowingCount { get; set; }
        public string TotalGames { get; set; }
        public string WinPercentage { get; set; }
    }

    /// <summary>
    /// Detailed profile information
    /// </summary>
    public class ProfileDetailDto : ProfileDto
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string Position { get; set; }
        public string Bio { get; set; }
        public string Zip { get; set; }
        public string PlayerNumber { get; set; }
        public int? Points { get; set; }
        public bool? TopRecord { get; set; }
        public string TotalWins { get; set; }
        public string TotalLosses { get; set; }
        public bool IsFollowing { get; set; }
        public SettingsDto Settings { get; set; }
    }

    /// <summary>
    /// Profile settings
    /// </summary>
    public class SettingsDto
    {
        public bool AllowComments { get; set; }
        public bool ShowGameHistory { get; set; }
        public bool AllowEmailNotification { get; set; }
    }

    /// <summary>
    /// Game information
    /// </summary>
    public class GameDto
    {
        public string GameId { get; set; }
        public string GameNumber { get; set; }
        public string CreatedDate { get; set; }
        public string Location { get; set; }
        public string PrivateRunId { get; set; }
        public string CourtId { get; set; }
        public string UserWinOrLose { get; set; }
        public List<ProfileDto> WinnersList { get; set; }
        public List<ProfileDto> LossersList { get; set; }
        public PrivateRunDto PrivateRun { get; set; }
    }

    /// <summary>
    /// Private run information
    /// </summary>
    public class PrivateRunDto
    {
        public string PrivateRunId { get; set; }
        public string PrivateRunNumber { get; set; }
        public string Title { get; set; }
        public string RunDate { get; set; }
        public string RunTime { get; set; }
    }
}