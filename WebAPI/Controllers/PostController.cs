// File: WebAPI/Controllers/PostControllerImproved.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DataLayer.DAL;
using Domain;
using DataLayer.EFCoreExtensions;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for managing posts
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostRepository _postRepository;
        private readonly ILogger<PostController> _logger;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public PostController(
            IPostRepository postRepository,
            ILogger<PostController> logger)
        {
            _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all posts with pagination and filtering
        /// </summary>
        /// <param name="timeZone">Timezone for calculating relative times</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of posts per page</param>
        /// <param name="status">Filter by status (optional)</param>
        /// <returns>List of posts</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<Post>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "timeZone", "page", "pageSize", "status" })]
        public async Task<IActionResult> GetPosts(
            [FromQuery] string timeZone = "UTC",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string status = null)
        {
            try
            {
                _logger.LogInformation(
                    "Getting posts (Page: {Page}, PageSize: {PageSize}, Status: {Status})",
                    page, pageSize, status ?? "All");

                // Get posts with pagination and optional filtering
                var posts = await _postRepository.GetPostsPagedAsync(timeZone, page, pageSize, status);

                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving posts" });
            }
        }

        /// <summary>
        /// Get post by ID
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <param name="timeZone">Timezone for calculating relative times</param>
        /// <returns>Post details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Post))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResponseCache(Duration = 30, VaryByQueryKeys = new[] { "timeZone" })]
        public async Task<IActionResult> GetPost(string id, [FromQuery] string timeZone = "UTC")
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Post ID is required");
            }

            try
            {
                _logger.LogInformation("Getting post with ID: {PostId}", id);

                var post = await _postRepository.GetPostById(id, timeZone);

                if (post == null)
                {
                    return NotFound();
                }

                return Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post with ID: {PostId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the post" });
            }
        }

        /// <summary>
        /// Create a new post
        /// </summary>
        /// <param name="model">Post data</param>
        /// <returns>Created post details</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePost([FromBody] PostCreateModel model)
        {
            if (model == null)
            {
                return BadRequest("Post data is required");
            }

            // Validate model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Get user ID from claims
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                // Create new post
                var post = new Post
                {
                    PostId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    ProfileId = model.ProfileId,
                    Caption = model.Caption,
                    PostText = model.PostText,
                    Type = model.Type,
                    PostType = model.PostType,
                    Title = model.Title,
                    Category = model.Category,
                    Mention = model.Mention,
                    Status = "Active",
                    Likes = 0,
                    DisLikes = 0,
                    Hearted = 0,
                    Views = 0
                };

                _logger.LogInformation("Creating new post for user: {UserId}", userId);

                await _postRepository.InsertPost(post);

                // Return created post
                return CreatedAtAction(nameof(GetPost), new { id = post.PostId }, post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the post" });
            }
        }

        /// <summary>
        /// Update an existing post
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <param name="model">Updated post data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePost(string id, [FromBody] PostUpdateModel model)
        {
            if (string.IsNullOrEmpty(id) || model == null)
            {
                return BadRequest("Post ID and update data are required");
            }

            // Validate model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Get user ID from claims
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                // Get existing post
                var existingPost = await _postRepository.GetPostById(id, "UTC");
                if (existingPost == null)
                {
                    return NotFound();
                }

                // Check if user has permission to update the post
                if (existingPost.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Update post
                var post = new Post
                {
                    PostId = id,
                    Caption = model.Caption,
                    PostText = model.PostText,
                    Type = model.Type,
                    Status = model.Status,
                    PostType = model.PostType,
                    Title = model.Title,
                    Category = model.Category,
                    Mention = model.Mention
                };

                _logger.LogInformation("Updating post: {PostId}", id);

                await _postRepository.UpdatePost(post);

                // Return no content on successful update
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post: {PostId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the post" });
            }
        }

        /// <summary>
        /// Update post status
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <param name="status">New status</param>
        /// <returns>No content if successful</returns>
        [HttpPatch("{id}/status")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePostStatus(string id, [FromQuery] string status)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(status))
            {
                return BadRequest("Post ID and status are required");
            }

            try
            {
                // Get user ID from claims
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                // Get existing post
                var existingPost = await _postRepository.GetPostById(id, "UTC");
                if (existingPost == null)
                {
                    return NotFound();
                }

                // Check if user has permission to update the post
                if (existingPost.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                _logger.LogInformation("Updating post status: {PostId}, Status: {Status}", id, status);

                await _postRepository.UpdatePostStatus(id, status);

                // Return no content on successful update
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post status: {PostId}, Status: {Status}", id, status);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the post status" });
            }
        }

        /// <summary>
        /// Delete a post
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePost(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Post ID is required");
            }

            try
            {
                // Get user ID from claims
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                // Get existing post
                var existingPost = await _postRepository.GetPostById(id, "UTC");
                if (existingPost == null)
                {
                    return NotFound();
                }

                // Check if user has permission to delete the post
                if (existingPost.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                _logger.LogInformation("Deleting post: {PostId}", id);

                await _postRepository.DeletePost(id);

                // Return no content on successful delete
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post: {PostId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the post" });
            }
        }

        /// <summary>
        /// Get posts by profile ID
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <param name="timeZone">Timezone for calculating relative times</param>
        /// <returns>List of posts by profile</returns>
        [HttpGet("profile/{profileId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Post>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "timeZone" })]
        public async Task<IActionResult> GetPostsByProfileId(string profileId, [FromQuery] string timeZone = "UTC")
        {
            if (string.IsNullOrEmpty(profileId))
            {
                return BadRequest("Profile ID is required");
            }

            try
            {
                _logger.LogInformation("Getting posts for profile: {ProfileId}", profileId);

                var posts = await _postRepository.GetPostsByProfileId(profileId, timeZone);

                if (posts == null || posts.Count == 0)
                {
                    // Return empty list instead of 404 for consistency
                    return Ok(new List<Post>());
                }

                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts for profile: {ProfileId}", profileId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving posts for the profile" });
            }
        }

        /// <summary>
        /// Get posts with a specific tag
        /// </summary>
        /// <param name="tagId">Tag ID</param>
        /// <param name="timeZone">Timezone for calculating relative times</param>
        /// <returns>List of posts with the tag</returns>
        [HttpGet("tag/{tagId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Post>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "timeZone" })]
        public async Task<IActionResult> GetPostsByTagId(string tagId, [FromQuery] string timeZone = "UTC")
        {
            if (string.IsNullOrEmpty(tagId))
            {
                return BadRequest("Tag ID is required");
            }

            try
            {
                _logger.LogInformation("Getting posts with tag: {TagId}", tagId);

                var posts = await _postRepository.GetPostsWithTagByTagId(tagId, timeZone);

                if (posts == null || posts.Count == 0)
                {