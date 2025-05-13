using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.DTOs;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for post operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IProfileService _profileService;
        private readonly IStorageService _storageService;
        private readonly Common.Services.IFileProcessorService _fileProcessor;
        private readonly ILogger<PostController> _logger;

        public PostController(
            IPostService postService,
            IProfileService profileService,
            IStorageService storageService,
            Common.Services.IFileProcessorService fileProcessor,
            ILogger<PostController> logger)
        {
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _fileProcessor = fileProcessor ?? throw new ArgumentNullException(nameof(fileProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get paginated posts
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="timeZone">User's timezone for relative time calculation</param>
        /// <returns>Paginated list of posts</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<PostDto>), 200)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> GetPosts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string timeZone = "UTC")
        {
            try
            {
                var result = await _postService.GetPostsAsync(page, pageSize, timeZone);

                // Get profile ID from claim if authenticated
                var profileId = User.FindFirst("ProfileId")?.Value;

                // Check saved and liked status for each post if user is authenticated
                if (!string.IsNullOrEmpty(profileId))
                {
                    var savedPosts = await _postService.GetSavedPostsByProfileIdAsync(profileId);
                    var likedPosts = await _postService.GetLikedPostsByProfileIdAsync(profileId);

                    foreach (var post in result.Items)
                    {
                        post.IsSaved = savedPosts.Any(sp => sp.PostId == post.PostId);
                        post.IsLiked = likedPosts.Any(lp => lp.PostId == post.PostId);
                    }
                }

                return Ok(new PagedResultDto<PostDto>
                {
                    Items = result.Items.Select(MapToPostDto).ToList(),
                    Page = result.Page,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount,
                    TotalPages = result.TotalPages,
                    HasPreviousPage = result.HasPreviousPage,
                    HasNextPage = result.HasNextPage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts");
                return StatusCode(500, new ErrorDto { Message = "An error occurred while retrieving posts" });
            }
        }

        /// <summary>
        /// Get a specific post by ID
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <param name="timeZone">User's timezone for relative time calculation</param>
        /// <returns>Post details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PostDetailDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> GetPost(string id, [FromQuery] string timeZone = "UTC")
        {
            try
            {
                var post = await _postService.GetPostByIdAsync(id, timeZone);
                if (post == null)
                {
                    return NotFound(new ErrorDto { Message = "Post not found" });
                }

                // Get profile ID from claim if authenticated
                var profileId = User.FindFirst("ProfileId")?.Value;

                // Check saved and liked status if user is authenticated
                if (!string.IsNullOrEmpty(profileId))
                {
                    var isSaved = await _postService.IsPostSavedByProfileAsync(id, profileId);
                    var isLiked = await _postService.IsPostLikedByProfileAsync(id, profileId);

                    post.SavedPost = isSaved;
                    post.LikedPost = isLiked;
                }

                return Ok(MapToPostDetailDto(post));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post: {PostId}", id);
                return StatusCode(500, new ErrorDto { Message = "An error occurred while retrieving the post" });
            }
        }

        /// <summary>
        /// Create a new post
        /// </summary>
        /// <returns>Created post</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(PostDetailDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorDto
                {
                    Message = "Invalid request",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .ToList()
                });
            }

            // Get user ID and profile ID from claims
            var userId = User.FindFirst("UserId")?.Value;
            var profileId = User.FindFirst("ProfileId")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(profileId))
            {
                return BadRequest(new ErrorDto { Message = "User or profile ID not found in token" });
            }

            try
            {
                // Create post object
                var post = new Post
                {
                    PostId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    ProfileId = profileId,
                    Caption = request.Caption,
                    PostText = request.PostText,
                    Type = request.File != null ? _fileProcessor.CheckFileType(request.File) : "text",
                    Status = "Active",
                    PostType = request.PostType ?? "User",
                    Title = request.Title,
                    Category = request.Category,
                    Mention = request.Mention,
                    PostedDate = DateTime.UtcNow.ToString("o")
                };

                // Process file if uploaded
                if (request.File != null && request.File.Length > 0)
                {
                    var fileType = _fileProcessor.CheckFileType(request.File);
                    if (fileType == "image" || fileType == "video")
                    {
                        // Process file based on type
                        if (fileType == "image")
                        {
                            // Convert image to WebP format
                            var webpBytes = await _fileProcessor.ConvertToWebPAsync(fileType, request.File);
                            var blobUrl = await _storageService.UploadFileAsync(webpBytes, $"{post.PostId}.webp", "postfile");
                            post.PostFileURL = blobUrl;
                        }
                        else if (fileType == "video")
                        {
                            // Upload video (conversion will be handled by the storage service)
                            var videoStream = request.File.OpenReadStream();
                            var blobUrl = await _storageService.UploadFileAsync(
                                videoStream,
                                $"{post.PostId}.mp4",
                                "postfile");
                            post.PostFileURL = blobUrl;

                            // Generate thumbnail
                            var thumbnailUrl = await _storageService.GenerateVideoThumbnailAsync(
                                request.File,
                                $"{post.PostId}.png",
                                "postthumbnail");
                            post.ThumbnailUrl = thumbnailUrl;
                        }
                    }
                    else
                    {
                        return BadRequest(new ErrorDto { Message = fileType }); // Return error message from file type check
                    }
                }

                // Create post
                var createdPost = await _postService.CreatePostAsync(post);

                // Generate post detail DTO
                var postDetail = MapToPostDetailDto(createdPost);

                return CreatedAtAction(nameof(GetPost), new { id = post.PostId }, postDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                return StatusCode(500, new ErrorDto { Message = "An error occurred while creating the post" });
            }
        }

        /// <summary>
        /// Update a post
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <param name="request">Updated post data</param>
        /// <returns>Updated post</returns>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(PostDetailDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 403)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> UpdatePost(string id, [FromBody] UpdatePostRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorDto
                {
                    Message = "Invalid request",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .ToList()
                });
            }

            try
            {
                // Get existing post
                var existingPost = await _postService.GetPostByIdAsync(id, "UTC");
                if (existingPost == null)
                {
                    return NotFound(new ErrorDto { Message = "Post not found" });
                }

                // Check if user has permission to update the post
                var userId = User.FindFirst("UserId")?.Value;
                var profileId = User.FindFirst("ProfileId")?.Value;

                if (existingPost.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Update post
                var updatedPost = new Post
                {
                    PostId = id,
                    Caption = request.Caption ?? existingPost.Caption,
                    PostText = request.PostText ?? existingPost.PostText,
                    Status = request.Status ?? existingPost.Status,
                    PostType = request.PostType ?? existingPost.PostType,
                    Title = request.Title ?? existingPost.Title,
                    Category = request.Category ?? existingPost.Category,
                    Mention = request.Mention ?? existingPost.Mention
                };

                await _postService.UpdatePostAsync(updatedPost);

                // Get updated post
                var post = await _postService.GetPostByIdAsync(id, "UTC");

                // Check saved and liked status if user is authenticated
                if (!string.IsNullOrEmpty(profileId))
                {
                    var isSaved = await _postService.IsPostSavedByProfileAsync(id, profileId);
                    var isLiked = await _postService.IsPostLikedByProfileAsync(id, profileId);

                    post.SavedPost = isSaved;
                    post.LikedPost = isLiked;
                }

                return Ok(MapToPostDetailDto(post));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post: {PostId}", id);
                return StatusCode(500, new ErrorDto { Message = "An error occurred while updating the post" });
            }
        }

        /// <summary>
        /// Delete a post
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(MessageDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 403)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> DeletePost(string id)
        {
            try
            {
                // Get existing post
                var existingPost = await _postService.GetPostByIdAsync(id, "UTC");
                if (existingPost == null)
                {
                    return NotFound(new ErrorDto { Message = "Post not found" });
                }

                // Check if user has permission to delete the post
                var userId = User.FindFirst("UserId")?.Value;

                if (existingPost.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Delete post
                await _postService.DeletePostAsync(id);

                return Ok(new MessageDto { Message = "Post deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post: {PostId}", id);
                return StatusCode(500, new ErrorDto { Message = "An error occurred while deleting the post" });
            }
        }

        /// <summary>
        /// Get posts by profile ID
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="timeZone">User's timezone for relative time calculation</param>
        /// <returns>Paginated list of posts by profile</returns>
        [HttpGet("profile/{profileId}")]
        [ProducesResponseType(typeof(PagedResultDto<PostDto>), 200)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> GetPostsByProfile(
            string profileId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string timeZone = "UTC")
        {
            try
            {
                // Check if profile exists
                var profile = await _profileService.GetProfileByIdAsync(profileId);
                if (profile == null)
                {
                    return NotFound(new ErrorDto { Message = "Profile not found" });
                }

                var result = await _postService.GetPostsByProfileIdAsync(profileId, page, pageSize, timeZone);

                // Get current user's profile ID from claims if authenticated
                var currentProfileId = User.FindFirst("ProfileId")?.Value;

                // Check saved and liked status for each post if user is authenticated
                if (!string.IsNullOrEmpty(currentProfileId))
                {
                    var savedPosts = await _postService.GetSavedPostsByProfileIdAsync(currentProfileId);
                    var likedPosts = await _postService.GetLikedPostsByProfileIdAsync(currentProfileId);

                    foreach (var post in result.Items)
                    {
                        post.SavedPost = savedPosts.Any(sp => sp.PostId == post.PostId);
                        post.LikedPost = likedPosts.Any(lp => lp.PostId == post.PostId);
                    }
                }

                return Ok(new PagedResultDto<PostDto>
                {
                    Items = result.Items.Select(MapToPostDto).ToList(),
                    Page = result.Page,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount,
                    TotalPages = result.TotalPages,
                    HasPreviousPage = result.HasPreviousPage,
                    HasNextPage = result.HasNextPage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts for profile: {ProfileId}", profileId);
                return StatusCode(500, new ErrorDto { Message = "An error occurred while retrieving posts" });
            }
        }

        /// <summary>
        /// Like a post
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Success message</returns>
        [HttpPost("{id}/like")]
        [Authorize]
        [ProducesResponseType(typeof(MessageDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> LikePost(string id)
        {
            // Get profile ID from claims
            var profileId = User.FindFirst("ProfileId")?.Value;
            if (string.IsNullOrEmpty(profileId))
            {
                return BadRequest(new ErrorDto { Message = "Profile ID not found in token" });
            }

            try
            {
                // Check if post exists
                var post = await _postService.GetPostByIdAsync(id, "UTC");
                if (post == null)
                {
                    return NotFound(new ErrorDto { Message = "Post not found" });
                }

                // Check if already liked
                var isLiked = await _postService.IsPostLikedByProfileAsync(id, profileId);
                if (isLiked)
                {
                    return BadRequest(new ErrorDto { Message = "Post already liked" });
                }

                // Like post
                await _postService.LikePostAsync(id, profileId);

                return Ok(new MessageDto { Message = "Post liked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking post: {PostId}, profile: {ProfileId}", id, profileId);
                return StatusCode(500, new ErrorDto { Message = "An error occurred while liking the post" });
            }
        }

        /// <summary>
        /// Unlike a post
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}/like")]
        [Authorize]
        [ProducesResponseType(typeof(MessageDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> UnlikePost(string id)
        {
            // Get profile ID from claims
            var profileId = User.FindFirst("ProfileId")?.Value;
            if (string.IsNullOrEmpty(profileId))
            {
                return BadRequest(new ErrorDto { Message = "Profile ID not found in token" });
            }

            try
            {
                // Check if post exists
                var post = await _postService.GetPostByIdAsync(id, "UTC");
                if (post == null)
                {
                    return NotFound(new ErrorDto { Message = "Post not found" });
                }

                // Check if already liked
                var isLiked = await _postService.IsPostLikedByProfileAsync(id, profileId);
                if (!isLiked)
                {
                    return BadRequest(new ErrorDto { Message = "Post not liked" });
                }

                // Unlike post
                await _postService.UnlikePostAsync(id, profileId);

                return Ok(new MessageDto { Message = "Post unliked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unliking post: {PostId}, profile: {ProfileId}", id, profileId);
                return StatusCode(500, new ErrorDto { Message = "An error occurred while unliking the post" });
            }
        }

        /// <summary>
        /// Save a post
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Success message</returns>
        [HttpPost("{id}/save")]
        [Authorize]
        [ProducesResponseType(typeof(MessageDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> SavePost(string id)
        {
            // Get profile ID from claims
            var profileId = User.FindFirst("ProfileId")?.Value;
            if (string.IsNullOrEmpty(profileId))
            {
                return BadRequest(new ErrorDto { Message = "Profile ID not found in token" });
            }

            try
            {
                // Check if post exists
                var post = await _postService.GetPostByIdAsync(id, "UTC");
                if (post == null)
                {
                    return NotFound(new ErrorDto { Message = "Post not found" });
                }

                // Check if already saved
                var isSaved = await _postService.IsPostSavedByProfileAsync(id, profileId);
                if (isSaved)
                {
                    return BadRequest(new ErrorDto { Message = "Post already saved" });
                }

                // Save post
                await _postService.SavePostAsync(id, profileId);

                return Ok(new MessageDto { Message = "Post saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving post: {PostId}, profile: {ProfileId}", id, profileId);
                return StatusCode(500, new ErrorDto { Message = "An error occurred while saving the post" });
            }
        }

        /// <summary>
        /// Unsave a post
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}/save")]
        [Authorize]
        [ProducesResponseType(typeof(MessageDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> UnsavePost(string id)
        {
            // Get profile ID from claims
            var profileId = User.FindFirst("ProfileId")?.Value;
            if (string.IsNullOrEmpty(profileId))
            {
                return BadRequest(new ErrorDto { Message = "Profile ID not found in token" });
            }

            try
            {
                // Check if post exists
                var post = await _postService.GetPostByIdAsync(id, "UTC");
                if (post == null)
                {
                    return NotFound(new ErrorDto { Message = "Post not found" });
                }

                // Check if saved
                var isSaved = await _postService.IsPostSavedByProfileAsync(id, profileId);
                if (!isSaved)
                {
                    return BadRequest(new ErrorDto { Message = "Post not saved" });
                }

                // Unsave post
                await _postService.UnsavePostAsync(id, profileId);

                return Ok(new MessageDto { Message = "Post unsaved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsaving post: {PostId}, profile: {ProfileId}", id, profileId);
                return StatusCode(500, new ErrorDto { Message = "An error occurred while unsaving the post" });
            }
        }

        // Helper methods for mapping entities to DTOs

        private PostDto MapToPostDto(Post post)
        {
            return new PostDto
            {
                PostId = post.PostId,
                Caption = post.Caption,
                PostFileURL = post.PostFileURL,
                Type = post.Type,
                Status = post.Status,
                PostType = post.PostType,
                Title = post.Title,
                Category = post.Category,
                Likes = post.Likes,
                PostedDate = post.PostedDate,
                RelativeTime = post.RelativeTime,
                ThumbnailUrl = post.ThumbnailUrl,
                ProfileId = post.ProfileId,
                UserName = post.UserName,
                ProfileImageURL = post.ProfileImageURL,
                PostCommentCount = post.PostCommentCount,
                IsSaved = post.SavedPost ?? false,
                IsLiked = post.LikedPost ?? false
            };
        }

        private PostDetailDto MapToPostDetailDto(Post post)
        {
            return new PostDetailDto
            {
                PostId = post.PostId,
                UserId = post.UserId,
                Caption = post.Caption,
                PostFileURL = post.PostFileURL,
                Type = post.Type,
                Status = post.Status,
                PostType = post.PostType,
                Title = post.Title,
                Category = post.Category,
                PostText = post.PostText,
                Likes = post.Likes,
                PostedDate = post.PostedDate,
                RelativeTime = post.RelativeTime,
                ThumbnailUrl = post.ThumbnailUrl,
                ProfileId = post.ProfileId,
                UserName = post.UserName,
                ProfileImageURL = post.ProfileImageURL,
                PostCommentCount = post.PostCommentCount,
                Mention = post.Mention,
                MentionUserNames = post.MentionUserNames,
                IsSaved = post.SavedPost ?? false,
                IsLiked = post.LikedPost ?? false,
                Comments = post.PostComments?.Select(pc => new PostCommentDto
                {
                    PostCommentId = pc.PostCommentId,
                    PostId = pc.PostId,
                    PostCommentByProfileId = pc.PostCommentByProfileId,
                    UserComment = pc.UserComment,
                    PostCommentDate = pc.PostCommentDate.ToString(),
                    RelativeTime = pc.RelativeTime,
                    UserName = pc.UserName,
                    ProfileImageURL = pc.ProfileImageURL
                }).ToList(),
                Mentions = post.ProfileMentions?.Select(p => new ProfileMentionDto
                {
                    ProfileId = p.ProfileId,
                    UserName = p.UserName,
                    ImageURL = p.ImageURL
                }).ToList()
            };
        }
    }
}