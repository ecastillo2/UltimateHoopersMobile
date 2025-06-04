using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Domain.DtoModel
{
    public class CourtUpdateModelDto
    {
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

        public void UpdateCourt(Court court)
        {
            court.Name = Name;
            court.Address = Address;
            court.Status = Status;
            court.NumberOfCourts = NumberOfCourts;
            court.State = State;
            court.City = City;
            court.RentalCostPerHour = RentalCostPerHour;
            court.Url = Url;
            court.CourtSize = CourtSize;
            court.CourtType = CourtType;
        }
    }
}
