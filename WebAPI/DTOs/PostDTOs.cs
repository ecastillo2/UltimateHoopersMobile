// File: WebAPI/DTOs/PostDTOs.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebAPI.DTOs
{
    /// <summary>
    /// Request for creating a new post
    /// </summary>
    public class CreatePostRequestDto
    {
        public string Caption { get; set; }
        public string PostText { get; set; }
        public string PostType { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Mention { get; set; }
        public IFormFile File { get; set; }
    }

    /// <summary>
    /// Request for updating a post
    /// </summary>
    public class UpdatePostRequestDto
    {
        public string Caption { get; set; }
        public string PostText { get; set; }
        public string Status { get; set; }
        public string PostType { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Mention { get; set; }
    }

    /// <summary>
    /// Basic post information
    /// </summary>
    public class PostDto
    {
        public string PostId { get; set; }
        public string Caption { get; set; }
        public string PostFileURL { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string PostType { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public int? Likes { get; set; }
        public string PostedDate { get; set; }
        public string RelativeTime { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ProfileId { get; set; }
        public string UserName { get; set; }
        public string ProfileImageURL { get; set; }
        public int? PostCommentCount { get; set; }
        public bool IsSaved { get; set; }
        public bool IsLiked { get; set; }
    }

    /// <summary>
    /// Detailed post information including comments
    /// </summary>
    public class PostDetailDto : PostDto
    {
        public string UserId { get; set; }
        public string PostText { get; set; }
        public string Mention { get; set; }
        public string MentionUserNames { get; set; }
        public List<PostCommentDto> Comments { get; set; }
        public List<ProfileMentionDto> Mentions { get; set; }
    }

    /// <summary>
    /// Post comment information
    /// </summary>
    public class PostCommentDto
    {
        public string PostCommentId { get; set; }
        public string PostId { get; set; }
        public string PostCommentByProfileId { get; set; }
        public string UserComment { get; set; }
        public string PostCommentDate { get; set; }
        public string RelativeTime { get; set; }
        public string UserName { get; set; }
        public string ProfileImageURL { get; set; }
    }

    /// <summary>
    /// Profile mention information
    /// </summary>
    public class ProfileMentionDto
    {
        public string ProfileId { get; set; }
        public string UserName { get; set; }
        public string ImageURL { get; set; }
    }
}
