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
    /// Profile Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileRepository _repository;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Profile Controller Constructor
        /// </summary>
        /// <param name="repository">Profile repository</param>
        /// <param name="configuration">Configuration</param>
        public ProfileController(IProfileRepository repository, IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Get All Profiles
        /// </summary>
        /// <returns>List of profiles</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Profile>>> GetProfiles()
        {
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Get Profile By Id
        /// </summary>
        /// <param name="id">Profile ID</param>
        /// <returns>Profile</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Profile>> GetProfile(string id)
        {
            var profile = await _repository.GetByIdAsync(id);

            if (profile == null)
            {
                return NotFound();
            }

            return profile;
        }

        /// <summary>
        /// Get Followers of a Profile
        /// </summary>
        /// <param name="id">Profile ID</param>
        /// <returns>List of follower profiles</returns>
        [HttpGet("{id}/followers")]
        public async Task<ActionResult<IEnumerable<Profile>>> GetFollowers(string id)
        {
            var profile = await _repository.GetByIdAsync(id);
            if (profile == null)
            {
                return NotFound();
            }

            return await _repository.GetFollowerProfilesAsync(id);
        }

        /// <summary>
        /// Get Profiles that a Profile is Following
        /// </summary>
        /// <param name="id">Profile ID</param>
        /// <returns>List of following profiles</returns>
        [HttpGet("{id}/following")]
        public async Task<ActionResult<IEnumerable<Profile>>> GetFollowing(string id)
        {
            var profile = await _repository.GetByIdAsync(id);
            if (profile == null)
            {
                return NotFound();
            }

            return await _repository.GetFollowingProfilesAsync(id);
        }

        /// <summary>
        /// Check if Username is Available
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>True if username is available</returns>
        [HttpGet("username-available")]
        public async Task<ActionResult<bool>> IsUsernameAvailable([FromQuery] string username)
        {
            return await _repository.IsUserNameAvailableAsync(username);
        }

        /// <summary>
        /// Create Profile
        /// </summary>
        /// <param name="profile">Profile to create</param>
        /// <returns>Created profile</returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Profile>> CreateProfile(Profile profile)
        {
            try
            {
                if (string.IsNullOrEmpty(profile.ProfileId))
                {
                    profile.ProfileId = Guid.NewGuid().ToString();
                }

                profile.CreatedDate = DateTime.Now;

                await _repository.AddAsync(profile);
                await _repository.SaveAsync();

                return CreatedAtAction(nameof(GetProfile), new { id = profile.ProfileId }, profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the profile", error = ex.Message });
            }
        }

        /// <summary>
        /// Update Profile
        /// </summary>
        /// <param name="id">Profile ID</param>
        /// <param name="profile">Updated profile data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(string id, Profile profile)
        {
            if (id != profile.ProfileId)
            {
                return BadRequest();
            }

            try
            {
                profile.LastModifiedDate = DateTime.Now;
                _repository.Update(profile);
                await _repository.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // Check if profile exists
                var existingProfile = await _repository.GetByIdAsync(id);
                if (existingProfile == null)
                {
                    return NotFound();
                }

                return StatusCode(500, new { message = "An error occurred while updating the profile", error = ex.Message });
            }
        }

        /// <summary>
        /// Update Profile Settings
        /// </summary>
        /// <param name="id">Profile ID</param>
        /// <param name="setting">Settings to update</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}/settings")]
        [Authorize]
        public async Task<IActionResult> UpdateSettings(string id, Setting setting)
        {
            if (id != setting.ProfileId)
            {
                return BadRequest();
            }

            try
            {
                await _repository.UpdateSettingsAsync(setting);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the settings", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete Profile
        /// </summary>
        /// <param name="id">Profile ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProfile(string id)
        {
            var profile = await _repository.GetByIdAsync(id);
            if (profile == null)
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
                return StatusCode(500, new { message = "An error occurred while deleting the profile", error = ex.Message });
            }
        }
    }
}