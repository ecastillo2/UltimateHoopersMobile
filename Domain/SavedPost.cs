using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class SavedPost
    {
        [Key]
        public string? SavedPostId { get; set; }
        public string? PostId { get; set; }
        public string? SavedByUserId { get; set; }
        public string? SavedByProfileId { get; set; }
        public string? SavedDate { get; set; }
    }
}
