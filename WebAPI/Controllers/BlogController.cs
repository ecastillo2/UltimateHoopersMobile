using Microsoft.AspNetCore.Mvc;
using Domain;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer.Repositories;
using System;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Blog Controller
    /// </summary>
    [Route("api/[controller]")]
    public class BlogController : Controller
    {
        private readonly IBlogRepository _repository;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Blog Controller
        /// </summary>
        /// <param name="repository">Blog repository</param>
        /// <param name="configuration">Configuration</param>
        public BlogController(IBlogRepository repository, IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Get Blogs
        /// </summary>
        /// <returns>List of blogs</returns>
        [HttpGet("GetBlogs")]
        public async Task<List<Blog>> GetBlogs()
        {
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Get Blog By Id
        /// </summary>
        /// <param name="blogId">Blog ID</param>
        /// <returns>Blog</returns>
        [HttpGet("GetBlogById")]
        public async Task<Blog> GetBlogById(string blogId)
        {
            try
            {
                return await _repository.GetByIdAsync(blogId);
            }
            catch (Exception ex)
            {
                throw; // Consider using a more user-friendly error handling approach
            }
        }

        /// <summary>
        /// Create Blog
        /// </summary>
        /// <param name="blog">Blog to create</param>
        /// <returns>Task</returns>
        [HttpPost("CreateBlog")]
        [Authorize]
        public async Task<IActionResult> CreateBlog([FromBody] Blog blog)
        {
            try
            {
                await _repository.AddAsync(blog);
                await _repository.SaveAsync();
                return Ok(new { message = "Blog created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the blog", error = ex.Message });
            }
        }

        /// <summary>
        /// Update Blog
        /// </summary>
        /// <param name="blog">Blog to update</param>
        /// <returns>Result</returns>
        [HttpPost("UpdateBlog")]
        [Authorize]
        public async Task<IActionResult> UpdateBlog([FromBody] Blog blog)
        {
            try
            {
                _repository.Update(blog);
                await _repository.SaveAsync();
                return Ok(new { message = "Blog updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the blog", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete Blog
        /// </summary>
        /// <param name="blogId">Blog ID</param>
        /// <returns>Result</returns>
        [HttpDelete("DeleteBlog")]
        [Authorize]
        public async Task<IActionResult> DeleteBlog(string blogId)
        {
            try
            {
                await _repository.DeleteByIdAsync(blogId);
                await _repository.SaveAsync();
                return Ok(new { message = "Blog deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the blog", error = ex.Message });
            }
        }
    }
}