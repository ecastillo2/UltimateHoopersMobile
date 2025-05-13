using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class PostComment
    {
        [Key]
        public string? PostCommentId { get; set; }
        public string? PostId { get; set; }
        public string? PostCommentByProfileId { get; set; }
        public string? UserComment { get; set; }
        public DateTime? PostCommentDate { get; set; }

        [NotMapped]
        public string? RelativeTime { get; set; }

        [NotMapped]
        public string? ProfileImageURL { get; set; }

        [NotMapped]
        public string? UserName { get; set; }
    }
}
