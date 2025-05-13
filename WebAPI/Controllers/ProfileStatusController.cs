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
    /// ProfileStatus Controller
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileStatusController : Controller
    {
        private readonly IProfileStatusRepository _repository;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// ProfileStatus Controller
        /// </summary>
        /// <param name="repository">ProfileStatus repository</param>
        /// <param name="configuration">Configuration</param>
        public ProfileStatusController(IProfileStatusRepository repository, IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Get ProfileStatuses
        /// </summary>
        /// <returns>List of profile statuses</returns>
        [HttpGet("GetProfileStatuses")]
        public async Task<List<ProfileStatus>> GetProfileStatuses()
        {
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Get ProfileStatus By ProfileId
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <returns>ProfileStatus</returns>
        [HttpGet("GetProfileStatusByProfileId")]
        public async Task<ProfileStatus> GetProfileStatusByProfileId(string profileId)
        {
            try
            {
                return await _repository.GetByIdAsync(profileId);
            }
            catch (Exception ex)
            {
                throw; // Consider using a more user-friendly error handling approach
            }
        }

        /// <summary>
        /// Create or Update ProfileStatus
        /// </summary>
        /// <param name="profileStatus">ProfileStatus to create or update</param>
        /// <returns>Task</returns>
        [HttpPost("UpdateProfileStatus")]
        public async Task<IActionResult> UpdateProfileStatus([FromBody] ProfileStatus profileStatus)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(profileStatus.ProfileId);

                if (existing == null)
                {
                    // Create new
                    await _repository.AddAsync(profileStatus);
                }
                else
                {
                    // Update existing
                    existing.Points = profileStatus.Points;
                    existing.Team = profileStatus.Team;
                    existing.Status = profileStatus.Status;
                    _repository.Update(existing);
                }

                await _repository.SaveAsync();
                return Ok(new { message = "ProfileStatus updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the profile status", error = ex.Message });
            }
        }
    }
}