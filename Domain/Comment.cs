using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Comment
    {
        [Key]
        public string? CommentId { get; set; }
        public string? UserId { get; set; }
        public string? CommentByUserId { get; set; }
        public string? UserComment { get; set; }
        public string? CommentDate { get; set; }

    }
}
