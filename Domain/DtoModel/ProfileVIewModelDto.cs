using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class ProfileViewModelDto
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public ProfileViewModelDto() { }

        // Existing constructor for mapping from Profile
        public ProfileViewModelDto(Profile profile)
        {
            ProfileId = profile.ProfileId;
            SubscriptionId = profile.SubscriptionId;
            UserId = profile.UserId;
            UserName = profile.UserName;
            Height = profile.Height;
            Weight = profile.Weight;
            Position = profile.Position;
            Ranking = profile.Ranking;
            StarRating = profile.StarRating;
            Bio = profile.Bio;
            ImageURL = profile.ImageURL;
            PlayerArchetype = profile.PlayerArchetype;
            City = profile.City;
            Zip = profile.Zip;
            PlayerNumber = profile.PlayerNumber;
            Status = profile.Status;
            Points = profile.Points;
            LastRunDate = profile.LastRunDate;
            TopRecord = profile.TopRecord;
            OnSquad = profile.OnSquad;
            GameStatistics = profile.GameStatistics;
            PaymentRequired = profile.PaymentRequired;
          

        }

        public string? ProfileId { get; set; }
        public string? SubscriptionId { get; set; }
        public bool? PaymentRequired { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Height { get; set; }
        public string? Weight { get; set; }
        public string? Position { get; set; }
        public int? Ranking { get; set; }
        public int? StarRating { get; set; }
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
        public Profile Profile { get; set; }
        public SettingViewModelDto Setting { get; set; }
        public ScoutingReport ScoutingReport { get; set; }
        public GameStatistics GameStatistics { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }

    }
}
