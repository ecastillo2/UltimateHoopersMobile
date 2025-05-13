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
    /// PostUpdateTime Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class PostUpdateTimeController : Controller
    {
        private readonly IPostUpdateTimeRepository _repository;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// PostUpdateTime Controller
        /// </summary>
        /// <param name="repository">PostUpdateTime repository</param>
        /// <param name="configuration">Configuration</param>
        public PostUpdateTimeController(IPostUpdateTimeRepository repository, IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Get PostUpdateTime
        /// </summary>
        /// <returns>PostUpdateTime</returns>
        [HttpGet("GetPostUpdateTime")]
        public async Task<PostUpdateTime> GetPostUpdateTime()
        {
            var items = await _repository.GetAllAsync();
            return items.FirstOrDefault();
        }

        /// <summary>
        /// Create or Update PostUpdateTime
        /// </summary>
        /// <param name="postUpdateTime">PostUpdateTime to create or update</param>
        /// <returns>Task</returns>
        [HttpPost("UpdatePostUpdateTime")]
        public async Task<IActionResult> UpdatePostUpdateTime([FromBody] PostUpdateTime postUpdateTime)
        {
            try
            {
                var existing = await _repository.GetAllAsync();
                var first = existing.FirstOrDefault();

                if (first == null)
                {
                    // Create new
                    postUpdateTime.PostUpdateTimeId = Guid.NewGuid().ToString();
                    postUpdateTime.PostLastUpdateTime = DateTime.Now;
                    await _repository.AddAsync(postUpdateTime);
                }
                else
                {
                    // Update existing
                    first.PostLastUpdateTime = DateTime.Now;
                    _repository.Update(first);
                }

                await _repository.SaveAsync();
                return Ok(new { message = "PostUpdateTime updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the post update time", error = ex.Message });
            }
        }
    }
}