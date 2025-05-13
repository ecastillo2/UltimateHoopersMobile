using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Contact
    {
        [Key]
        public string? ContactId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public string? CreatedDate { get; set; }

    }
}
