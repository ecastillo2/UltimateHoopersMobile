using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Court
    {
        [Key]
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
    }
}
