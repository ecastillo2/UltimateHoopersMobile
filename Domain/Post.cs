using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class Post
    {
        [Key]
        public string? PostId { get; set; }
        public string? UserId { get; set; }
        public string? Caption { get; set; }
        public string? PostFileURL { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public int? Likes { get; set; }
        public int? DisLikes { get; set; }
        public int? Hearted { get; set; }
        public int? Views { get; set; }
        public string? Shared { get; set; }
        public string? PostedDate { get; set; }
        public string? ProfileId { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? PostType { get; set; }
        public string? PostText { get; set; }
        public string? Title { get; set; }
        public string? Category { get; set; }
        public string? Mention { get; set; }
        public string? TagId { get; set; }
        public string? MentionUserNames { get; set; }
        [NotMapped]
        public List<Profile>? ProfileMentions { get; set; }
        [NotMapped]
        public List<PostComment>? PostComments { get; set; }

        [NotMapped]
        
        public string? FirstName { get; set; }

        [NotMapped]
      
        public string? LastName { get; set; }

        [NotMapped]
       
        public string? ProfileImageURL { get; set; }

        [NotMapped]
       
        public string? UserName { get; set; }

        [NotMapped]

        public string? RelativeTime { get; set; }
        [NotMapped]

        public bool? SavedPost { get; set; }

        [NotMapped]
        public bool? LikedPost { get; set; }
        [NotMapped]
        public int? PostCommentCount { get; set; }

        [NotMapped]
        public string? StarRating { get; set; }
    }
}
