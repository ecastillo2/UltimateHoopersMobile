using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Criteria
    {
        [Key]
        public string? CriteriaId { get; set; }
        public string? ProfileId { get; set; }
        public string? CompetionLevel { get; set; }
        public string? Frequency { get; set; }
        
    }
}
