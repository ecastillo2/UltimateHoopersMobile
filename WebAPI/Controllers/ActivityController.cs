
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Domain;
using DataLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Activity Controller
    /// </summary>
    [Route("api/[controller]")]
    public class ActivityController : Controller
    {
        private readonly IActivityRepository _repository;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Activity Controller
        /// </summary>
        /// <param name="repository">Activity repository</param>
        /// <param name="configuration">Configuration</param>
        public ActivityController(IActivityRepository repository, IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Get Activities
        /// </summary>
        /// <returns>List of activities</returns>
        [HttpGet("GetActivitys")]
        //[Authorize]
        public async Task<List<Activity>> GetActivitys()
        {
            return await _repository.GetActivitys();
        }

        /// <summary>
        /// Create Activity
        /// </summary>
        /// <param name="activity">Activity to create</param>
        /// <returns>Task</returns>
        [HttpPost("CreateActivity")]
        public async Task CreateActivity([FromBody] Activity activity)
        {
            try
            {
                await _repository.InsertActivity(activity);
            }
            catch (Exception ex)
            {
                // Log the exception
                // Consider returning an error response instead of silently handling the exception
            }
        }

        /// <summary>
        /// Get Activity By Id
        /// </summary>
        /// <param name="activityId">Activity ID</param>
        /// <returns>Activity</returns>
        [Authorize]
        [HttpGet("GetActivityById")]
        public async Task<Activity> GetActivityById(string activityId)
        {
            try
            {
                return await _repository.GetActivityById(activityId);
            }
            catch (Exception ex)
            {
                throw; // Consider using a more user-friendly error handling approach
            }
        }

        /// <summary>
        /// Delete Activity
        /// </summary>
        /// <param name="activityId">Activity ID</param>
        /// <returns>Result</returns>
        [Authorize]
        [HttpDelete("DeleteActivity")]
        public async Task<IActionResult> DeleteActivity(string activityId)
        {
            try
            {
                await _repository.DeleteActivity(activityId);
                return Ok(new { message = "Activity deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the activity", error = ex.Message });
            }
        }
    }
}