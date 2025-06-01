using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Domain.DtoModel
{
    public class PostViewModelDto
    {
        // Add a parameterless constructor for JSON deserialization
        [JsonConstructor]
        public PostViewModelDto() { }

        // Existing constructor for mapping from Post
        public PostViewModelDto(Post post)
        {
            PostId = post.PostId;
            UserId = post.UserId;
            Caption = post.Caption;
            PostFileURL = post.PostFileURL;
            Type = post.Type;
            Status = post.Status;
            Likes = post.Likes;
            DisLikes = post.DisLikes;
            Hearted = post.Hearted;
            Views = post.Views;
            Shared = post.Shared;
            PostedDate = post.PostedDate;
            ProfileId = post.ProfileId;
            ThumbnailUrl = post.ThumbnailUrl;
            PostType = post.PostType;
            PostText = post.PostText;
            Title = post.Title;
            Category = post.Category;
            Mention = post.Mention;
            MentionUserNames = post.MentionUserNames;
        }

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
        public DateTime? PostedDate { get; set; }
        public string? ProfileId { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? PostType { get; set; }
        public string? PostText { get; set; }
        public string? Title { get; set; }
        public string Category { get; set; }
        public string? Mention { get; set; }
        public string? MentionUserNames { get; set; }
    }
}
