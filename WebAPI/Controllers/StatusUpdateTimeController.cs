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
    /// StatusUpdateTime Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class StatusUpdateTimeController : Controller
    {
        private readonly IStatusUpdateTimeRepository _repository;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// StatusUpdateTime Controller
        /// </summary>
        /// <param name="repository">StatusUpdateTime repository</param>
        /// <param name="configuration">Configuration</param>
        public StatusUpdateTimeController(IStatusUpdateTimeRepository repository, IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Get StatusUpdateTime
        /// </summary>
        /// <returns>StatusUpdateTime</returns>
        [HttpGet("GetStatusUpdateTime")]
        public async Task<StatusUpdateTime> GetStatusUpdateTime()
        {
            var items = await _repository.GetAllAsync();
            return items.FirstOrDefault();
        }

        /// <summary>
        /// Create or Update StatusUpdateTime
        /// </summary>
        /// <param name="statusUpdateTime">StatusUpdateTime to create or update</param>
        /// <returns>Task</returns>
        [HttpPost("UpdateStatusUpdateTime")]
        public async Task<IActionResult> UpdateStatusUpdateTime([FromBody] StatusUpdateTime statusUpdateTime)
        {
            try
            {
                var existing = await _repository.GetAllAsync();
                var first = existing.FirstOrDefault();

                if (first == null)
                {
                    // Create new
                    statusUpdateTime.StatusUpdateTimeId = Guid.NewGuid().ToString();
                    statusUpdateTime.LastUpdateTime = DateTime.Now;
                    await _repository.AddAsync(statusUpdateTime);
                }
                else
                {
                    // Update existing
                    first.LastUpdateTime = DateTime.Now;
                    _repository.Update(first);
                }

                await _repository.SaveAsync();
                return Ok(new { message = "StatusUpdateTime updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the status update time", error = ex.Message });
            }
        }
    }
}