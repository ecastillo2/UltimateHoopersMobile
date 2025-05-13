using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Document
    {
        [Key]
        public string? DocumentId { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? Url { get; set; }
        public string? IdNumber { get; set; }

    }
}
