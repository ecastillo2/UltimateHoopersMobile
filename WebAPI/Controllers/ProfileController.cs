using DataLayer;
using DataLayer.DAL;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Profile Controller with improved performance, error handling, and validation
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly HUDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProfileController> _logger;

        /// <summary>
        /// Profile Controller constructor with dependency injection
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="logger">Logger instance</param>
        public ProfileController(
            HUDBContext context,
            IConfiguration configuration,
            ILogger<ProfileController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all profiles - simplified fallback implementation
        /// </summary>
        /// <returns>List of profiles</returns>
        [HttpGet("GetProfiles")]
        [ProducesResponseType(typeof(List<Profile>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Profile>>> GetProfiles()
        {
            try
            {
                _logger.LogInformation("Fetching all profiles");

                // Direct query to database - simplified to ensure it works
                var profiles = await _context.Profile
                    .AsNoTracking()
                    .Take(100) // Limit to first 100 profiles for performance
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} profiles", profiles.Count);

                return Ok(profiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profiles");
                return StatusCode(500, new { message = "An error occurred while retrieving profiles" });
            }
        }

        /// <summary>
        /// Get profile by ID
        /// </summary>
        /// <param name="profileId">The profile ID to retrieve</param>
        /// <returns>The profile</returns>
        [HttpGet("GetProfileById")]
        [ProducesResponseType(typeof(Profile), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Profile>> GetProfileById(string profileId)
        {
            if (string.IsNullOrEmpty(profileId))
            {
                return BadRequest(new { message = "Profile ID is required" });
            }

            try
            {
                _logger.LogInformation("Fetching profile with ID: {ProfileId}", profileId);

                // Direct query to get profile with essential information
                var profile = await _context.Profile
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ProfileId == profileId);

                if (profile == null)
                {
                    _logger.LogWarning("Profile not found: {ProfileId}", profileId);
                    return NotFound(new { message = $"Profile with ID {profileId} not found" });
                }

                // Get user information with a separate query
                var user = await _context.User
                    .AsNoTracking()
                    .Where(u => u.UserId == profile.UserId)
                    .Select(u => new {
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.Status,
                        u.LastLoginDate
                    })
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    profile.FirstName = user.FirstName;
                    profile.LastName = user.LastName;
                    profile.Email = user.Email;
                    profile.Status = user.Status;
                    profile.LastLoginDate = user.LastLoginDate;
                }

                _logger.LogInformation("Successfully retrieved profile: {ProfileId}", profileId);

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile {ProfileId}", profileId);
                return StatusCode(500, new { message = "An error occurred while retrieving the profile" });
            }
        }

        /// <summary>
        /// Get follower profiles by profile ID
        /// </summary>
        /// <param name="profileId">The profile ID to get followers for</param>
        /// <returns>List of follower profiles</returns>
        [HttpGet("GetFollowerProfilesByProfileId")]
        [Authorize]
        [ProducesResponseType(typeof(List<Profile>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Profile>>> GetFollowerProfilesByProfileId(string profileId)
        {
            if (string.IsNullOrEmpty(profileId))
            {
                return BadRequest(new { message = "Profile ID is required" });
            }

            try
            {
                _logger.LogInformation("Fetching followers for profile: {ProfileId}", profileId);

                // Get follower profile IDs
                var followerIds = await _context.Follower
                    .AsNoTracking()
                    .Where(f => f.ProfileId == profileId)
                    .Select(f => f.FollowerProfileId)
                    .ToListAsync();

                if (!followerIds.Any())
                {
                    return Ok(new List<Profile>());
                }

                // Get follower profiles
                var followers = await _context.Profile
                    .AsNoTracking()
                    .Where(p => followerIds.Contains(p.ProfileId))
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} followers for profile: {ProfileId}",
                    followers.Count, profileId);

                return Ok(followers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving followers for profile {ProfileId}", profileId);
                return StatusCode(500, new { message = "An error occurred while retrieving followers" });
            }
        }

        /// <summary>
        /// Get following profiles by profile ID
        /// </summary>
        /// <param name="profileId">The profile ID to get following profiles for</param>
        /// <returns>List of following profiles</returns>
        [HttpGet("GetFollowingProfilesByProfileId")]
        [Authorize]
        [ProducesResponseType(typeof(List<Profile>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Profile>>> GetFollowingProfilesByProfileId(string profileId)
        {
            if (string.IsNullOrEmpty(profileId))
            {
                return BadRequest(new { message = "Profile ID is required" });
            }

            try
            {
                _logger.LogInformation("Fetching following profiles for profile: {ProfileId}", profileId);

                // Get following profile IDs
                var followingIds = await _context.Following
                    .AsNoTracking()
                    .Where(f => f.ProfileId == profileId)
                    .Select(f => f.FollowingProfileId)
                    .ToListAsync();

                if (!followingIds.Any())
                {
                    return Ok(new List<Profile>());
                }

                // Get following profiles
                var following = await _context.Profile
                    .AsNoTracking()
                    .Where(p => followingIds.Contains(p.ProfileId))
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} following profiles for profile: {ProfileId}",
                    following.Count, profileId);

                return Ok(following);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving following profiles for profile {ProfileId}", profileId);
                return StatusCode(500, new { message = "An error occurred while retrieving following profiles" });
            }
        }

        /// <summary>
        /// Get profile game history
        /// </summary>
        /// <param name="profileId">The profile ID to get game history for</param>
        /// <returns>List of games</returns>
        [HttpGet("GetProfileGameHistory")]
        [Authorize]
        [ProducesResponseType(typeof(List<Game>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Game>>> GetProfileGameHistory(string profileId)
        {
            if (string.IsNullOrEmpty(profileId))
            {
                return BadRequest(new { message = "Profile ID is required" });
            }

            try
            {
                _logger.LogInformation("Fetching game history for profile: {ProfileId}", profileId);

                // Get games where the profile was a winner or loser
                var games = await _context.Game
                    .AsNoTracking()
                    .Where(g =>
                        (!string.IsNullOrEmpty(g.WinProfileIdsStatusString) && g.WinProfileIdsStatusString.Contains(profileId)) ||
                        (!string.IsNullOrEmpty(g.LoseProfileIdsStatusString) && g.LoseProfileIdsStatusString.Contains(profileId)))
                    .OrderByDescending(g => g.CreatedDate)
                    .Take(20) // Limit to most recent 20 games
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} games for profile: {ProfileId}",
                    games.Count, profileId);

                return Ok(games);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game history for profile {ProfileId}", profileId);
                return StatusCode(500, new { message = "An error occurred while retrieving game history" });
            }
        }

        /// <summary>
        /// Update profile
        /// </summary>
        /// <param name="profile">The profile to update</param>
        /// <returns>Success or error response</returns>
        [HttpPut("UpdateProfile")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProfile([FromBody] Profile profile)
        {
            if (profile == null)
            {
                return BadRequest(new { message = "Profile is required" });
            }

            if (string.IsNullOrEmpty(profile.ProfileId))
            {
                return BadRequest(new { message = "Profile ID is required" });
            }

            try
            {
                _logger.LogInformation("Updating profile: {ProfileId}", profile.ProfileId);

                // Get existing profile
                var existingProfile = await _context.Profile
                    .FirstOrDefaultAsync(p => p.ProfileId == profile.ProfileId);

                if (existingProfile == null)
                {
                    _logger.LogWarning("Profile not found for update: {ProfileId}", profile.ProfileId);
                    return NotFound(new { message = $"Profile with ID {profile.ProfileId} not found" });
                }

                // Update only provided fields
                if (profile.Height != null) existingProfile.Height = profile.Height;
                if (profile.Weight != null) existingProfile.Weight = profile.Weight;
                if (profile.Position != null) existingProfile.Position = profile.Position;
                if (profile.Zip != null) existingProfile.Zip = profile.Zip;
                if (profile.QRCode != null) existingProfile.QRCode = profile.QRCode;
                if (profile.Bio != null) existingProfile.Bio = profile.Bio;
                if (profile.PlayerArchetype != null) existingProfile.PlayerArchetype = profile.PlayerArchetype;
                if (profile.City != null) existingProfile.City = profile.City;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated profile: {ProfileId}", profile.ProfileId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile {ProfileId}", profile.ProfileId);
                return StatusCode(500, new { message = "An error occurred while updating the profile" });
            }
        }

        /// <summary>
        /// Update profile username
        /// </summary>
        /// <param name="profile">The profile with updated username</param>
        /// <returns>Success or error response</returns>
        [HttpPut("UpdateProfileUserName")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProfileUserName([FromBody] Profile profile)
        {
            if (profile == null)
            {
                return BadRequest(new { message = "Profile is required" });
            }

            if (string.IsNullOrEmpty(profile.ProfileId))
            {
                return BadRequest(new { message = "Profile ID is required" });
            }

            if (string.IsNullOrEmpty(profile.UserName))
            {
                return BadRequest(new { message = "Username is required" });
            }

            try
            {
                _logger.LogInformation("Updating username for profile: {ProfileId}", profile.ProfileId);

                // Check if the username is already taken by another profile
                var existingProfile = await _context.Profile
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.UserName == profile.UserName && p.ProfileId != profile.ProfileId);

                if (existingProfile != null)
                {
                    _logger.LogWarning("Username {UserName} is already taken by profile {ExistingProfileId}",
                        profile.UserName, existingProfile.ProfileId);
                    return Conflict(new { message = "Username is already taken" });
                }

                // Get the profile to update
                var profileToUpdate = await _context.Profile
                    .FirstOrDefaultAsync(p => p.ProfileId == profile.ProfileId);

                if (profileToUpdate == null)
                {
                    _logger.LogWarning("Profile not found for username update: {ProfileId}", profile.ProfileId);
                    return NotFound(new { message = $"Profile with ID {profile.ProfileId} not found" });
                }

                // Update username
                profileToUpdate.UserName = profile.UserName;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated username for profile: {ProfileId}", profile.ProfileId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating username for profile {ProfileId}", profile.ProfileId);
                return StatusCode(500, new { message = "An error occurred while updating the username" });
            }
        }

        /// <summary>
        /// Update profile settings
        /// </summary>
        /// <param name="setting">The settings to update</param>
        /// <returns>Success or error response</returns>
        [HttpPut("UpdateSetting")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSetting([FromBody] Setting setting)
        {
            if (setting == null)
            {
                return BadRequest(new { message = "Setting is required" });
            }

            if (string.IsNullOrEmpty(setting.ProfileId))
            {
                return BadRequest(new { message = "Profile ID is required" });
            }

            try
            {
                _logger.LogInformation("Updating settings for profile: {ProfileId}", setting.ProfileId);

                // Check if the setting exists
                var existingSetting = await _context.Setting
                    .FirstOrDefaultAsync(s => s.ProfileId == setting.ProfileId);

                if (existingSetting != null)
                {
                    // Update existing settings
                    existingSetting.AllowComments = setting.AllowComments;
                    existingSetting.ShowGameHistory = setting.ShowGameHistory;
                    existingSetting.AllowEmailNotification = setting.AllowEmailNotification;

                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Create new settings
                    setting.SettingId = Guid.NewGuid().ToString();
                    await _context.Setting.AddAsync(setting);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Successfully updated settings for profile: {ProfileId}", setting.ProfileId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating settings for profile {ProfileId}", setting.ProfileId);
                return StatusCode(500, new { message = "An error occurred while updating the settings" });
            }
        }

        /// <summary>
        /// Update last run date for a profile
        /// </summary>
        /// <param name="profileId">The profile ID to update</param>
        /// <param name="lastRunDate">The new last run date</param>
        /// <returns>Success or error response</returns>
        [HttpPut("UpdateLastRunDate")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLastRunDate(
            [Required][FromQuery] string profileId,
            [Required][FromQuery] string lastRunDate)
        {
            if (string.IsNullOrEmpty(profileId))
            {
                return BadRequest(new { message = "Profile ID is required" });
            }

            if (string.IsNullOrEmpty(lastRunDate))
            {
                return BadRequest(new { message = "Last run date is required" });
            }

            try
            {
                _logger.LogInformation("Updating last run date for profile: {ProfileId}", profileId);

                // Get the profile to update
                var profile = await _context.Profile
                    .FirstOrDefaultAsync(p => p.ProfileId == profileId);

                if (profile == null)
                {
                    _logger.LogWarning("Profile not found for last run date update: {ProfileId}", profileId);
                    return NotFound(new { message = $"Profile with ID {profileId} not found" });
                }

                // Update last run date
                profile.LastRunDate = lastRunDate;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated last run date for profile: {ProfileId}", profileId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last run date for profile {ProfileId}", profileId);
                return StatusCode(500, new { message = "An error occurred while updating the last run date" });
            }
        }

        /// <summary>
        /// Update winner points for a profile
        /// </summary>
        /// <param name="profileId">The profile ID to update</param>
        /// <returns>Success or error response</returns>
        [HttpPut("UpdateWinnerPoints")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateWinnerPoints([Required][FromQuery] string profileId)
        {
            if (string.IsNullOrEmpty(profileId))
            {
                return BadRequest(new { message = "Profile ID is required" });
            }

            try
            {
                _logger.LogInformation("Updating winner points for profile: {ProfileId}", profileId);

                // Get the profile to update
                var profile = await _context.Profile
                    .FirstOrDefaultAsync(p => p.ProfileId == profileId);

                if (profile == null)
                {
                    _logger.LogWarning("Profile not found for winner points update: {ProfileId}", profileId);
                    return NotFound(new { message = $"Profile with ID {profileId} not found" });
                }

                // Update points
                profile.Points = (profile.Points ?? 0) + 2;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated winner points for profile: {ProfileId}", profileId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating winner points for profile {ProfileId}", profileId);
                return StatusCode(500, new { message = "An error occurred while updating the winner points" });
            }
        }

        /// <summary>
        /// Set profile with best record flag
        /// </summary>
        /// <param name="profileId">The profile ID to update</param>
        /// <returns>Success or error response</returns>
        [HttpPut("UpdateSetProfileWithBestRecord")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSetProfileWithBestRecord([Required][FromQuery] string profileId)
        {
            if (string.IsNullOrEmpty(profileId))
            {
                return BadRequest(new { message = "Profile ID is required" });
            }

            try
            {
                _logger.LogInformation("Setting best record flag for profile: {ProfileId}", profileId);

                // Get the profile to update
                var profile = await _context.Profile
                    .FirstOrDefaultAsync(p => p.ProfileId == profileId);

                if (profile == null)
                {
                    _logger.LogWarning("Profile not found for best record flag update: {ProfileId}", profileId);
                    return NotFound(new { message = $"Profile with ID {profileId} not found" });
                }

                // Update flag
                profile.TopRecord = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully set best record flag for profile: {ProfileId}", profileId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting best record flag for profile {ProfileId}", profileId);
                return StatusCode(500, new { message = "An error occurred while updating the best record flag" });
            }
        }

        /// <summary>
        /// Set profile with best record flag to false
        /// </summary>
        /// <param name="profileId">The profile ID to update</param>
        /// <returns>Success or error response</returns>
        [HttpPut("UpdateSetProfileWithBestRecordToFalse")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSetProfileWithBestRecordToFalse([Required][FromQuery] string profileId)
        {
            if (string.IsNullOrEmpty(profileId))
            {
                return BadRequest(new { message = "Profile ID is required" });
            }

            try
            {
                _logger.LogInformation("Setting best record flag to false for profile: {ProfileId}", profileId);

                // Get the profile to update
                var profile = await _context.Profile
                    .FirstOrDefaultAsync(p => p.ProfileId == profileId);

                if (profile == null)
                {
                    _logger.LogWarning("Profile not found for best record flag update: {ProfileId}", profileId);
                    return NotFound(new { message = $"Profile with ID {profileId} not found" });
                }

                // Update flag
                profile.TopRecord = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully set best record flag to false for profile: {ProfileId}", profileId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting best record flag to false for profile {ProfileId}", profileId);
                return StatusCode(500, new { message = "An error occurred while updating the best record flag" });
            }
        }

        /// <summary>
        /// Check if a username is available
        /// </summary>
        /// <param name="userName">The username to check</param>
        /// <returns>True if the username is available, false if it's already taken</returns>
        [HttpGet("IsUserNameAvailable")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> IsUserNameAvailable([Required][FromQuery] string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest(new { message = "Username is required" });
            }

            try
            {
                _logger.LogInformation("Checking if username is available: {UserName}", userName);

                // Check if the username exists
                bool exists = await _context.Profile
                    .AsNoTracking()
                    .AnyAsync(p => p.UserName == userName);

                _logger.LogInformation("Username {UserName} is {Result}",
                    userName, exists ? "taken" : "available");

                // Return true if available (not exists)
                return Ok(!exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username availability: {UserName}", userName);
                return StatusCode(500, new { message = "An error occurred while checking username availability" });
            }
        }

        /// <summary>
        /// Check if an email is available
        /// </summary>
        /// <param name="email">The email to check</param>
        /// <returns>True if the email is available, false if it's already taken</returns>
        [HttpGet("IsEmailAvailable")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> IsEmailAvailable(
            [Required][FromQuery][EmailAddress] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            try
            {
                _logger.LogInformation("Checking if email is available: {Email}", email);

                // Check if the email exists
                bool exists = await _context.User
                    .AsNoTracking()
                    .AnyAsync(u => u.Email == email);

                _logger.LogInformation("Email {Email} is {Result}",
                    email, exists ? "taken" : "available");

                // Return true if available (not exists)
                return Ok(!exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email availability: {Email}", email);
                return StatusCode(500, new { message = "An error occurred while checking email availability" });
            }
        }
    }
}