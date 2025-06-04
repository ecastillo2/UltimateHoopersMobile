using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain
{
    public class Court
    {

        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public Court() { }

        // Existing constructor for mapping from ScoutingReport
        public Court(Court court)
        {
            CourtId = court.CourtId;
            ClientId = court.ClientId;
            ProfileId = court.ProfileId;
            ImageURL = court.ImageURL;
            Name = court.Name;
            Latitude = court.Latitude;
            Longitude = court.Longitude;
            Address = court.Address;
            City = court.City;
            Zip = court.Zip;
            Status = court.Status;
            NumberOfCourts = court.NumberOfCourts;
            Url = court.Url;
            RentalCostPerHour = court.RentalCostPerHour;
            CourtSize = court.CourtSize;
            CourtNumber = court.CourtNumber;
            CourtType = court.CourtType;
        }


        [Key]
        public string? CourtId { get; set; }
        public string? ClientId { get; set; }
        public string? ProfileId { get; set; }
        public string? ImageURL { get; set; }
        public string? Name { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public string? Status { get; set; }
        public int? NumberOfCourts { get; set; }
        public decimal? RentalCostPerHour { get; set; }
        public string? Url { get; set; }
        public string? CourtSize { get; set; }
        public string? CourtNumber { get; set; }
        public string? CourtType { get; set; }
    }
}
