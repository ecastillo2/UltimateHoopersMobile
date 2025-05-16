using DataLayer;
using DataLayer.DAL;
using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for managing posts
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostRepository _repository;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor for PostController
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="configuration">Application configuration</param>
        public PostController(HUDBContext context, IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _repository = new PostRepository(context, _configuration);
        }

        /// <summary>
        /// Get posts with cursor-based pagination (optimized for mobile)
        /// </summary>
        /// <param name="cursor">Optional cursor for pagination</param>
        /// <param name="limit">Number of posts to return</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Posts with cursor-based pagination</returns>
        [HttpGet("cursor")]
        [ProducesResponseType(typeof(CursorPaginatedResultDto<Post>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPostsWithCursor(
            [FromQuery] string cursor = null,
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var timeZone = Request.Headers.TryGetValue("TimeZone", out var tz)
                    ? tz.ToString()
                    : "America/New_York";

                var (posts, nextCursor, hasMore) = await _repository.GetPostsWithCursorAsync(
                    cursor,
                    limit,
                    timeZone,
                    cancellationToken);

                return Ok(new CursorPaginatedResultDto<Post>
                {
                    Items = posts,
                    NextCursor = nextCursor,
                    HasMore = hasMore
                });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, new { message = "Request was canceled" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while retrieving the posts. Please try again later.",
                    Error = ex.Message // Consider removing this in production for security
                });
            }
        }

        /// <summary>
        /// Get All Posts
        /// </summary>
        [HttpGet("GetPosts")]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            try
            {
                var timeZone = Request.Headers.TryGetValue("TimeZone", out var tz) ? tz.ToString() : "America/New_York";
                var posts = await _repository.GetPosts(timeZone);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while retrieving the posts. Please try again later.",
                    Error = ex.Message // Consider removing this in production for security
                });
            }
        }

        /// <summary>
        /// Get Blogs
        /// </summary>
        [HttpGet("GetBlogs")]
        public async Task<ActionResult<IEnumerable<Post>>> GetBlogs()
        {
            try
            {
                var timeZone = Request.Headers.TryGetValue("TimeZone", out var tz) ? tz.ToString() : "America/New_York";
                var posts = await _repository.GetBlogs(timeZone);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while retrieving the blogs. Please try again later.",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get posts by profile ID
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <returns>List of posts by the profile</returns>
        [HttpGet("ByProfile/{profileId}")]
        public async Task<ActionResult<IEnumerable<Post>>> GetPostsByProfile(string profileId)
        {
            try
            {
                var timeZone = Request.Headers.TryGetValue("TimeZone", out var tz) ? tz.ToString() : "America/New_York";
                var posts = await _repository.GetPostsByProfileId(profileId, timeZone);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = $"An error occurred while retrieving posts for profile {profileId}.",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get post by ID
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Post with matching ID</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(string id)
        {
            try
            {
                var timeZone = Request.Headers.TryGetValue("TimeZone", out var tz) ? tz.ToString() : "America/New_York";
                var post = await _repository.GetPostById(id, timeZone);

                if (post == null)
                    return NotFound();

                return Ok(post);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = $"An error occurred while retrieving post {id}.",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Create a new post
        /// </summary>
        /// <param name="post">Post to create</param>
        /// <returns>Created post</returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Post>> CreatePost([FromBody] Post post)
        {
            try
            {
                await _repository.InsertPost(post);
                _repository.InvalidateCache(); // Invalidate cache after creating new post

                return CreatedAtAction(nameof(GetPost), new { id = post.PostId }, post);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while creating the post.",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update an existing post
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <param name="post">Updated post</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdatePost(string id, [FromBody] Post post)
        {
            if (id != post.PostId)
                return BadRequest("Post ID mismatch");

            try
            {
                await _repository.UpdatePost(post);
                _repository.InvalidateCache(); // Invalidate cache after updating post
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = $"An error occurred while updating post {id}.",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete a post
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(string id)
        {
            try
            {
                await _repository.DeletePost(id);
                _repository.InvalidateCache(); // Invalidate cache after deleting post
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = $"An error occurred while deleting post {id}.",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update Post Status
        /// </summary>
        [Authorize]
        [HttpGet("UpdatePostStatus")]
        public async Task<IActionResult> UpdatePostStatus(string postId, string status)
        {
            try
            {
                await _repository.UpdatePostStatus(postId, status);
                _repository.InvalidateCache(); // Invalidate cache after updating status
                return Ok(new { message = "Post status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update post status", error = ex.Message });
            }
        }

        /// <summary>
        /// Like a post
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <param name="profileId">Profile ID of the user liking the post</param>
        /// <returns>Success message</returns>
        [HttpPost("{postId}/like")]
        [Authorize]
        public async Task<IActionResult> LikePost(string postId, [FromQuery] string profileId)
        {
            try
            {
                await _repository.LikePostAsync(postId, profileId);
                return Ok(new { message = "Post liked successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while liking the post.",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Unlike a post
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <param name="profileId">Profile ID of the user unliking the post</param>
        /// <returns>Success message</returns>
        [HttpDelete("{postId}/like")]
        [Authorize]
        public async Task<IActionResult> UnlikePost(string postId, [FromQuery] string profileId)
        {
            try
            {
                await _repository.UnlikePostAsync(postId, profileId);
                return Ok(new { message = "Post unliked successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while unliking the post.",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Check if a post is liked by a profile
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <param name="profileId">Profile ID</param>
        /// <returns>True if the post is liked by the profile, false otherwise</returns>
        [HttpGet("{postId}/isLiked")]
        public async Task<ActionResult<bool>> IsPostLiked(string postId, [FromQuery] string profileId)
        {
            try
            {
                var isLiked = await _repository.IsPostLikedByProfileAsync(postId, profileId);
                return Ok(isLiked);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while checking if the post is liked.",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get posts with a specific tag
        /// </summary>
        /// <param name="tagId">Tag ID</param>
        /// <returns>List of posts with the specified tag</returns>
        [HttpGet("ByTag/{tagId}")]
        public async Task<ActionResult<IEnumerable<Post>>> GetPostsByTag(string tagId)
        {
            try
            {
                var timeZone = Request.Headers.TryGetValue("TimeZone", out var tz) ? tz.ToString() : "America/New_York";
                var posts = await _repository.GetPostsWithTagByTagId(tagId, timeZone);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = $"An error occurred while retrieving posts for tag {tagId}.",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get saved posts for a specific profile
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <returns>List of posts saved by the profile</returns>
        [HttpGet("Saved/{profileId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Post>>> GetSavedPosts(string profileId)
        {
            try
            {
                var timeZone = Request.Headers.TryGetValue("TimeZone", out var tz) ? tz.ToString() : "America/New_York";
                var posts = await _repository.GetSavedPostsByProfileId(profileId, timeZone);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = $"An error occurred while retrieving saved posts for profile {profileId}.",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get posts that mention a specific profile
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <returns>List of posts that mention the profile</returns>
        [HttpGet("Mentions/{profileId}")]
        public async Task<ActionResult<IEnumerable<Post>>> GetPostsMentioningProfile(string profileId)
        {
            try
            {
                var timeZone = Request.Headers.TryGetValue("TimeZone", out var tz) ? tz.ToString() : "America/New_York";
                var posts = await _repository.GetPostsMentionProfileId(profileId, timeZone);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = $"An error occurred while retrieving posts mentioning profile {profileId}.",
                    Error = ex.Message
                });
            }
        }
    }

    
}