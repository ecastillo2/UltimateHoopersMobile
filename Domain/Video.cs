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
        public string? VideoNumber { get; set; }
        public string? Title { get; set; }
        public string? VideoThumbnail { get; set; }
        public DateTime? VideoDate { get; set; }
        public DateTime? CreatedDate { get; set; }

    }
}