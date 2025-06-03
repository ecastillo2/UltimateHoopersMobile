using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class Video
    {
        [Key]
        public string? VideoId { get; set; }
        public string? ClientId { get; set; }
        public string? VideoURL { get; set; }
        public string? VideoName { get; set; }
        public string? Status { get; set; }
        public DateTime? VideoDate { get; set; }

    }
}