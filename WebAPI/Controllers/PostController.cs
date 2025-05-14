using DataLayer.DAL;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Post Controller
    /// </summary>
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostRepository _repository;
        private readonly IConfiguration _configuration;

        public PostController(IPostRepository repository, IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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

        // Fix other methods similarly, making sure to:
        // 1. Reference _repository instead of repository
        // 2. Return proper ActionResult types
        // 3. Add missing return statements where needed

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
                return Ok(new { message = "Post status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update post status", error = ex.Message });
            }
        }
    }
}