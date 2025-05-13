using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class NotificationMessage
    {
        [Key]
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string? Url { get; set; }

    }
}
