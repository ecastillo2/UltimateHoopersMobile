using Microsoft.AspNetCore.Mvc;
using Domain;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer.Repositories;
using System;
using System.Linq;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Post Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostRepository _repository;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Post Controller Constructor
        /// </summary>
        /// <param name="repository">Post repository</param>
        /// <param name="configuration">Configuration</param>
        public PostController(IPostRepository repository, IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Get All Posts
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of posts</returns>
        [HttpGet]
        public async Task<ActionResult<object>> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var timeZone = Request.Headers.TryGetValue("TimeZone", out var tz) ? tz.ToString() : "America/New_York";
            var (posts, totalCount, totalPages) = await _repository.GetPaginatedPostsAsync(page, pageSize, timeZone);

            return new
            {
                Posts = posts,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };
        }

        /// <summary>
        /// Get Post By Id
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Post with details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(string id)
        {
            var timeZone = Request.Headers.TryGetValue("TimeZone", out var tz) ? tz.ToString() : "America/New_York";
            var post = await _repository.GetPostByIdWithDetailsAsync(id, timeZone);

            if (post == null)
            {
                return NotFound();
            }

            return post;
        }

        /// <summary>
        /// Get Posts By Profile Id
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <returns>List of posts by profile</returns>
        [HttpGet("profile/{profileId}")]
        public async Task<ActionResult<IEnumerable<Post>>> GetPostsByProfile(string profileId)
        {
            var timeZone = Request.Headers.TryGetValue("TimeZone", out var tz) ? tz.ToString() : "America/New_York";
            return await _repository.GetPostsByProfileIdAsync(profileId, timeZone);
        }

        /// <summary>
        /// Create Post
        /// </summary>
        /// <param name="post">Post to create</param>
        /// <returns>Created post</returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Post>> CreatePost(Post post)
        {
            try
            {
                if (string.IsNullOrEmpty(post.PostId))
                {
                    post.PostId = Guid.NewGuid().ToString();
                }

                post.PostedDate = DateTime.Now.ToString();

                await _repository.AddAsync(post);
                await _repository.SaveAsync();

                return CreatedAtAction(nameof(GetPost), new { id = post.PostId }, post);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the post", error = ex.Message });
            }
        }

        /// <summary>
        /// Update Post
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <param name="post">Updated post data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdatePost(string id, Post post)
        {
            if (id != post.PostId)
            {
                return BadRequest();
            }

            try
            {
                _repository.Update(post);
                await _repository.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // Check if post exists
                var timeZone = Request.Headers.TryGetValue("TimeZone", out var tz) ? tz.ToString() : "America/New_York";
                var existingPost = await _repository.GetPostByIdWithDetailsAsync(id, timeZone);
                if (existingPost == null)
                {
                    return NotFound();
                }

                return StatusCode(500, new { message = "An error occurred while updating the post", error = ex.Message });
            }
        }

        /// <summary>
        /// Like a Post
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <param name="profileId">Profile ID liking the post</param>
        /// <returns>No content if successful</returns>
        [HttpPost("{id}/like")]
        [Authorize]
        public async Task<IActionResult> LikePost(string id, [FromBody] string profileId)
        {
            try
            {
                await _repository.LikePostAsync(id, profileId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while liking the post", error = ex.Message });
            }
        }

        /// <summary>
        /// Unlike a Post
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <param name="profileId">Profile ID unliking the post</param>
        /// <returns>No content if successful</returns>
        [HttpPost("{id}/unlike")]
        [Authorize]
        public async Task<IActionResult> UnlikePost(string id, [FromBody] string profileId)
        {
            try
            {
                await _repository.UnlikePostAsync(id, profileId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while unliking the post", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete Post
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(string id)
        {
            var timeZone = Request.Headers.TryGetValue("TimeZone", out var tz) ? tz.ToString() : "America/New_York";
            var post = await _repository.GetPostByIdWithDetailsAsync(id, timeZone);
            if (post == null)
            {
                return NotFound();
            }

            try
            {
                await _repository.DeleteByIdAsync(id);
                await _repository.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the post", error = ex.Message });
            }
        }


    }
}