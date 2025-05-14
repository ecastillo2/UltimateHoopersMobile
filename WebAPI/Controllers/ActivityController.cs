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
    /// Activity Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityRepository _repository;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Activity Controller Constructor
        /// </summary>
        /// <param name="repository">Activity repository</param>
        /// <param name="configuration">Configuration</param>
        public ActivityController(IActivityRepository repository, IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Get All Activities
        /// </summary>
        /// <returns>List of activities</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Activity>>> GetActivities()
        {
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Get Activity By Id
        /// </summary>
        /// <param name="id">Activity ID</param>
        /// <returns>Activity</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Activity>> GetActivity(string id)
        {
            var activity = await _repository.GetByIdAsync(id);

            if (activity == null)
            {
                return NotFound();
            }

            return activity;
        }

        /// <summary>
        /// Create Activity
        /// </summary>
        /// <param name="activity">Activity to create</param>
        /// <returns>Created activity</returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Activity>> CreateActivity(Activity activity)
        {
            try
            {
                if (string.IsNullOrEmpty(activity.ActivityId))
                {
                    activity.ActivityId = Guid.NewGuid().ToString();
                }

                activity.CreatedDate = DateTime.Now;

                await _repository.AddAsync(activity);
                await _repository.SaveAsync();

                return CreatedAtAction(nameof(GetActivity), new { id = activity.ActivityId }, activity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the activity", error = ex.Message });
            }
        }

        /// <summary>
        /// Update Activity
        /// </summary>
        /// <param name="id">Activity ID</param>
        /// <param name="activity">Updated activity data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateActivity(string id, Activity activity)
        {
            if (id != activity.ActivityId)
            {
                return BadRequest();
            }

            try
            {
                _repository.Update(activity);
                await _repository.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // Check if activity exists
                var existingActivity = await _repository.GetByIdAsync(id);
                if (existingActivity == null)
                {
                    return NotFound();
                }

                return StatusCode(500, new { message = "An error occurred while updating the activity", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete Activity
        /// </summary>
        /// <param name="id">Activity ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteActivity(string id)
        {
            var activity = await _repository.GetByIdAsync(id);
            if (activity == null)
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
                return StatusCode(500, new { message = "An error occurred while deleting the activity", error = ex.Message });
            }
        }
    }
}