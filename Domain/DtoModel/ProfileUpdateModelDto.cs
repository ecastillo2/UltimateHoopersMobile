using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class ProfileUpdateModelDto
    {
        public string? ProfileId { get; set; }
        public string? Height { get; set; }
        public string? Weight { get; set; }
        public string? Position { get; set; }
        public string? Bio { get; set; }
        public string? ImageURL { get; set; }
        public string? PlayerArchetype { get; set; }
        public string? City { get; set; }
        public string? Zip { get; set; }
        public string? PlayerNumber { get; set; }

        public void UpdateProfile(Profile profile)
        {
            profile.Height = Height;
            profile.Weight = Weight;
            profile.Position = Position;
            profile.Bio = Bio;
            profile.ImageURL = ImageURL;
            profile.PlayerArchetype = PlayerArchetype;
            profile.City = City;
            profile.Zip = Zip;
            profile.PlayerNumber = PlayerNumber;
        }
    }
}
