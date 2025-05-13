using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class SquadRequest
    {
        [Key]
        public string? SquadRequestId { get; set; }
        public string? SquadId { get; set; }
        public string? ToProfileId { get; set; }
        public string? FromProfileId { get; set; }
        public bool? RequestResponse { get; set; }
        public DateTime? CreatedDate { get; set; }

        [NotMapped]
        public Profile? Profile { get; set; }
    }
}
