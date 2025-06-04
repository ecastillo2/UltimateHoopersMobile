using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Domain.DtoModel
{
    public class CourtViewModelDto
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public CourtViewModelDto() { }

        // Existing constructor for mapping from Profile
        public CourtViewModelDto(Court court)
        {
            CourtId = court.CourtId;
            ImageURL = court.ImageURL;
            Name = court.Name;
            Latitude = court.Latitude;
            Longitude = court.Longitude;
            Address = court.Address;
            City = court.City;
            State = court.State;
            Zip = court.Zip;
            Status = court.ImageURL;
            NumberOfCourts = court.NumberOfCourts;
            RentalCostPerHour = court.RentalCostPerHour;
            Url = court.Url;
            CourtSize = court.CourtSize;
            CourtNumber = court.CourtNumber;
            CourtType = court.CourtType;

        }

        public string? CourtId { get; set; }
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
