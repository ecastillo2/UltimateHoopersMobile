using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class ProfileViewModelDto
    {
        public string ProfileId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string Position { get; set; }
        public string Ranking { get; set; }
        public string StarRating { get; set; }
        public string Bio { get; set; }
        public string ImageURL { get; set; }
        public string PlayerArchetype { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string PlayerNumber { get; set; }
        public string Status { get; set; }
        public int? Points { get; set; }
        public string LastRunDate { get; set; }
        public bool? TopRecord { get; set; }
        public bool? OnSquad { get; set; }

        public ProfileViewModelDto(Profile profile)
        {
            ProfileId = profile.ProfileId;
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
        }
    }
}
