using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class PlayerComment
    {
        [Key]
        public string? PlayerCommentId { get; set; }
        public string? ProfileId { get; set; }
        public string? CommentedProfileId { get; set; }
        public string? Comment { get; set; }
        public DateTime? DateCommented { get; set; }

        [NotMapped]
        public string? ImageURL { get; set; }

        [NotMapped]
        public string? RelativeTime { get; set; }
        

        [NotMapped]

        public string? ProfileImageURL { get; set; }

        [NotMapped]

        public string? UserName { get; set; }
    }
}
