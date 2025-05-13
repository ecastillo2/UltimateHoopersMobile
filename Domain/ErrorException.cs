using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class ErrorException
    {
        [Key]
        public string? ErrorExceptionId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? DetailMessage { get; set; }
        public string? UserId { get; set; }
        public string? ProfileId { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
