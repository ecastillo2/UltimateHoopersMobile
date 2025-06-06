using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class Request
    {
        [Key]
        public string? RequestId { get; set; }
        public string? RunId { get; set; }
        public string? ProfileId { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedDate { get; set; }

        [NotMapped]
        public Run? Run { get; set; }

        [NotMapped]
        public Profile? Profile { get; set; }

        [NotMapped]
        public IList<Profile>? ProfileList { get; set; }

    }
}
