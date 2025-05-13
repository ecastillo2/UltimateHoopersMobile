using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class History
    {
        [Key]
        public string? HistoryId { get; set; }
        public string? PrivateRunId { get; set; }
        public string? ProfileId { get; set; }
        public string? WinLose { get; set; }
        public string? RunDate { get; set; }
        public string? Points { get; set; }
        public string? Assists { get; set; }
        public string? CreatedDate { get; set; }
        [NotMapped]
        public string? Location { get; set; }
    }
}
