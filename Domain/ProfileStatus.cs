using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class ProfileStatus
    {
        [Key]
        public string ProfileId { get; set; }
        public int Points { get; set; }
        public string Team { get; set; }
        public string Status { get; set; }

    }
}
