using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Organization
    {
        [Key]
        public string? OrganizationId { get; set; }
        public string? CompanyName { get; set; }
        public string? InstagramURL { get; set; }
        public string? FacebookURL { get; set; }
        public string? TwitterURL { get; set; }
        public string? YouTubeURL { get; set; }
        public string? DiscordURL { get; set; }

    }
}
