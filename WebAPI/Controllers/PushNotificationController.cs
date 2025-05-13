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
    /// PushNotification Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class PushNotificationController : Controller
    {
        private readonly IPushNotificationRepository _repository;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// PushNotification Controller
        /// </summary>
        /// <param name="repository">PushNotification repository</param>
        /// <param name="configuration">Configuration</param>
        public PushNotificationController(IPushNotificationRepository repository, IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Get PushNotifications
        /// </summary>
        /// <returns>List of push notifications</returns>
        [HttpGet("GetPushNotifications")]
        public async Task<List<PushNotification>> GetPushNotifications()
        {
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Get PushNotification By Id
        /// </summary>
        /// <param name="pushNotificationId">PushNotification ID</param>
        /// <returns>PushNotification</returns>
        [HttpGet("GetPushNotificationById")]
        public async Task<PushNotification> GetPushNotificationById(string pushNotificationId)
        {
            try
            {
                return await _repository.GetByIdAsync(pushNotificationId);
            }
            catch (Exception ex)
            {
                throw; // Consider using a more user-friendly error handling approach
            }
        }

        /// <summary>
        /// Get PushNotifications By Type
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <returns>List of push notifications</returns>
        [HttpGet("GetPushNotificationsByType")]
        public async Task<List<PushNotification>> GetPushNotificationsByType(string type)
        {
            try
            {
                return await _repository.FindAsync(pn => pn.Type == type);
            }
            catch (Exception ex)
            {
                throw; // Consider using a more user-friendly error handling approach
            }
        }

        /// <summary>
        /// Create PushNotification
        /// </summary>
        /// <param name="pushNotification">PushNotification to create</param>
        /// <returns>Task</returns>
        [HttpPost("CreatePushNotification")]
        public async Task<IActionResult> CreatePushNotification([FromBody] PushNotification pushNotification)
        {
            try
            {
                if (string.IsNullOrEmpty(pushNotification.PushNotificationId))
                {
                    pushNotification.PushNotificationId = Guid.NewGuid().ToString();
                }

                await _repository.AddAsync(pushNotification);
                await _repository.SaveAsync();
                return Ok(new { message = "PushNotification created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the push notification", error = ex.Message });
            }
        }

        /// <summary>
        /// Update PushNotification
        /// </summary>
        /// <param name="pushNotification">PushNotification to update</param>
        /// <returns>Result</returns>
        [HttpPost("UpdatePushNotification")]
        public async Task<IActionResult> UpdatePushNotification([FromBody] PushNotification pushNotification)
        {
            try
            {
                _repository.Update(pushNotification);
                await _repository.SaveAsync();
                return Ok(new { message = "PushNotification updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the push notification", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete PushNotification
        /// </summary>
        /// <param name="pushNotificationId">PushNotification ID</param>
        /// <returns>Result</returns>
        [HttpDelete("DeletePushNotification")]
        public async Task<IActionResult> DeletePushNotification(string pushNotificationId)
        {
            try
            {
                await _repository.DeleteByIdAsync(pushNotificationId);
                await _repository.SaveAsync();
                return Ok(new { message = "PushNotification deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the push notification", error = ex.Message });
            }
        }
    }
}