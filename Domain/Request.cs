using System.ComponentModel.DataAnnotations;

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

    }
}
