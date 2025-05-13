using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.DTOs;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for user profile operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IFollowService _followService;
        private readonly IGameService _gameService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            IProfileService profileService,
            IFollowService followService,
            IGameService gameService,
            ILogger<ProfileController> logger)
        {
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _followService = followService ?? throw new ArgumentNullException(nameof(followService));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all profiles
        /// </summary>
        /// <returns>List of profiles</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProfileDto>), 200)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> GetProfiles()
        {
            try
            {
                var profiles = await _profileService.GetProfilesAsync();
                return Ok(profiles.Select(p => MapToProfileDto(p)).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profiles");
                return StatusCode(500, new ErrorDto { Message = "An error occurred while retrieving profiles" });
            }
        }

        /// <summary>
        /// Get profile by ID
        /// </summary>
        /// <param name="id">Profile ID</param>
        /// <returns>Profile details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProfileDetailDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> GetProfile(string id)
        {
            try
            {
                var profile = await _profileService.GetProfileByIdAsync(id);
                if (profile == null)
                {
                    return NotFound(new ErrorDto { Message = "Profile not found" });
                }

                // Get current user's profile ID from claims
                var currentUserProfileId = User.FindFirst("ProfileId")?.Value;

                // Check if the current user is following this profile
                bool isFollowing = false;
                if (!string.IsNullOrEmpty(currentUserProfileId) && currentUserProfileId != id)
                {
                    isFollowing = await _followService.IsFollowingAsync(currentUserProfileId, id);
                }

                var profileDetail = MapToProfileDetailDto(profile);
                profileDetail.IsFollowing = isFollowing;

                return Ok(profileDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile: {ProfileId}", id);
                return StatusCode(500, new ErrorDto { Message = "An error occurred while retrieving the profile" });
            }
        }

        /// <summary>
        /// Update profile
        /// </summary>
        /// <param name="id">Profile ID</param>
        /// <param name="request">Updated profile data</param>
        /// <returns>Updated profile</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProfileDetailDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 403)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> UpdateProfile(string id, [FromBody] UpdateProfileRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorDto
                {
                    Message = "Invalid request",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .ToList()
                });
            }

            // Get current user's profile ID from claims
            var currentUserProfileId = User.FindFirst("ProfileId")?.Value;

            // Check if the user is authorized to update this profile
            if (currentUserProfileId != id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                var existingProfile = await _profileService.GetProfileByIdAsync(id);
                if (existingProfile == null)
                {
                    return NotFound(new ErrorDto { Message = "Profile not found" });
                }

                // Check if username is being updated and if it's available
                if (!string.IsNullOrEmpty(request.UserName) &&
                    request.UserName != existingProfile.UserName &&
                    await _profileService.IsUserNameAvailableAsync(request.UserName))
                {
                    return BadRequest(new ErrorDto { Message = "Username is already in use" });
                }

                // Update profile
                var profile = new Profile
                {
                    ProfileId = id,
                    UserName = request.UserName ?? existingProfile.UserName,
                    Height = request.Height ?? existingProfile.Height,
                    Weight = request.Weight ?? existingProfile.Weight,
                    Position = request.Position ?? existingProfile.Position,
                    Bio = request.Bio ?? existingProfile.Bio,
                    PlayerArchetype = request.PlayerArchetype ?? existingProfile.PlayerArchetype,
                    City = request.City ?? existingProfile.City,
                    Zip = request.Zip ?? existingProfile.Zip
                };

                var updatedProfile = await _profileService.UpdateProfileAsync(profile);
                return Ok(MapToProfileDetailDto(updatedProfile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile: {ProfileId}", id);
                return StatusCode(500, new ErrorDto { Message = "An error occurred while updating the profile" });
            }
        }

        /// <summary>
        /// Update profile settings
        /// </summary>
        /// <param name="id">Profile ID</param>
        /// <param name="request">Updated settings</param>
        /// <returns>Success message</returns>
        [HttpPut("{id}/settings")]
        [ProducesResponseType(typeof(MessageDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 403)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> UpdateSettings(string id, [FromBody] UpdateSettingsRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorDto
                {
                    Message = "Invalid request",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .ToList()
                });
            }

            // Get current user's profile ID from claims
            var currentUserProfileId = User.FindFirst("ProfileId")?.Value;

            // Check if the user is authorized to update this profile's settings
            if (currentUserProfileId != id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                var profile = await _profileService.GetProfileByIdAsync(id);
                if (profile == null)
                {
                    return NotFound(new ErrorDto { Message = "Profile not found" });
                }

                var settings = new Setting
                {
                    ProfileId = id,
                    AllowComments = request.AllowComments,
                    ShowGameHistory = request.ShowGameHistory,
                    AllowEmailNotification = request.AllowEmailNotification
                };

                await _profileService.UpdateSettingsAsync(settings);
                return Ok(new MessageDto { Message = "Settings updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating settings for profile: {ProfileId}", id);
                return StatusCode(500, new ErrorDto { Message = "An error occurred while updating settings" });
            }
        }

        /// <summary>
        /// Get followers for a profile
        /// </summary>
        /// <param name="id">Profile ID</param>
        /// <returns>List of followers</returns>
        [HttpGet("{id}/followers")]
        [ProducesResponseType(typeof(List<ProfileDto>), 200)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> GetFollowers(string id)
        {
            try
            {
                var profile = await _profileService.GetProfileByIdAsync(id);
                if (profile == null)
                {
                    return NotFound(new ErrorDto { Message = "Profile not found" });
                }

                var followers = await _followService.GetFollowersByProfileIdAsync(id);
                return Ok(followers.Select(p => MapToProfileDto(p)).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving followers for profile: {ProfileId}", id);
                return StatusCode(500, new ErrorDto { Message = "An error occurred while retrieving followers" });
            }
        }

        /// <summary>
        /// Get following for a profile
        /// </summary>
        /// <param name="id">Profile ID</param>
        /// <returns>List of profiles being followed</returns>
        [HttpGet("{id}/following")]
        [ProducesResponseType(typeof(List<ProfileDto>), 200)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> GetFollowing(string id)
        {
            try
            {
                var profile = await _profileService.GetProfileByIdAsync(id);
                if (profile == null)
                {
                    return NotFound(new ErrorDto { Message = "Profile not found" });
                }

                var following = await _followService.GetFollowingByProfileIdAsync(id);
                return Ok(following.Select(p => MapToProfileDto(p)).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving following for profile: {ProfileId}", id);
                return StatusCode(500, new ErrorDto { Message = "An error occurred while retrieving following" });
            }
        }

        /// <summary>
        /// Follow a profile
        /// </summary>
        /// <param name="id">Profile ID to follow</param>
        /// <returns>Success message</returns>
        [HttpPost("{id}/follow")]
        [ProducesResponseType(typeof(MessageDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> FollowProfile(string id)
        {
            // Get current user's profile ID from claims
            var currentUserProfileId = User.FindFirst("ProfileId")?.Value;
            if (string.IsNullOrEmpty(currentUserProfileId))
            {
                return BadRequest(new ErrorDto { Message = "Profile ID not found in token" });
            }

            // Check if user is trying to follow themselves
            if (currentUserProfileId == id)
            {
                return BadRequest(new ErrorDto { Message = "You cannot follow yourself" });
            }

            try
            {
                var profileToFollow = await _profileService.GetProfileByIdAsync(id);
                if (profileToFollow == null)
                {
                    return NotFound(new ErrorDto { Message = "Profile not found" });
                }

                // Check if already following
                var isAlreadyFollowing = await _followService.IsFollowingAsync(currentUserProfileId, id);
                if (isAlreadyFollowing)
                {
                    return BadRequest(new ErrorDto { Message = "You are already following this profile" });
                }

                await _followService.FollowProfileAsync(currentUserProfileId, id);
                return Ok(new MessageDto { Message = "Profile followed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error following profile: {ProfileId}, follower: {FollowerId}", id, currentUserProfileId);
                return StatusCode(500, new ErrorDto { Message = "An error occurred while following the profile" });
            }
        }

        /// <summary>
        /// Unfollow a profile
        /// </summary>
        /// <param name="id">Profile ID to unfollow</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}/follow")]
        [ProducesResponseType(typeof(MessageDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> UnfollowProfile(string id)
        {
            // Get current user's profile ID from claims
            var currentUserProfileId = User.FindFirst("ProfileId")?.Value;
            if (string.IsNullOrEmpty(currentUserProfileId))
            {
                return BadRequest(new ErrorDto { Message = "Profile ID not found in token" });
            }

            try
            {
                var profileToUnfollow = await _profileService.GetProfileByIdAsync(id);
                if (profileToUnfollow == null)
                {
                    return NotFound(new ErrorDto { Message = "Profile not found" });
                }

                // Check if actually following
                var isFollowing = await _followService.IsFollowingAsync(currentUserProfileId, id);
                if (!isFollowing)
                {
                    return BadRequest(new ErrorDto { Message = "You are not following this profile" });
                }

                await _followService.UnfollowProfileAsync(currentUserProfileId, id);
                return Ok(new MessageDto { Message = "Profile unfollowed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unfollowing profile: {ProfileId}, follower: {FollowerId}", id, currentUserProfileId);
                return StatusCode(500, new ErrorDto { Message = "An error occurred while unfollowing the profile" });
            }
        }

        /// <summary>
        /// Get game history for a profile
        /// </summary>
        /// <param name="id">Profile ID</param>
        /// <returns>List of games</returns>
        [HttpGet("{id}/games")]
        [ProducesResponseType(typeof(List<GameDto>), 200)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        [ProducesResponseType(typeof(ErrorDto), 500)]
        public async Task<IActionResult> GetGameHistory(string id)
        {
            try
            {
                var profile = await _profileService.GetProfileByIdAsync(id);
                if (profile == null)
                {
                    return NotFound(new ErrorDto { Message = "Profile not found" });
                }

                // Check if profile settings allow viewing game history
                if (!profile.Setting.ShowGameHistory)
                {
                    var currentUserProfileId = User.FindFirst("ProfileId")?.Value;
                    if (currentUserProfileId != id && !User.IsInRole("Admin"))
                    {
                        return BadRequest(new ErrorDto { Message = "This profile's game history is private" });
                    }
                }

                var games = await _gameService.GetGamesByProfileIdAsync(id);
                return Ok(games.Select(g => MapToGameDto(g, id)).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game history for profile: {ProfileId}", id);
                return StatusCode(500, new ErrorDto { Message = "An error occurred while retrieving game history" });
            }
        }

        // Helper methods for mapping entities to DTOs

        private ProfileDto MapToProfileDto(Profile profile)
        {
            return new ProfileDto
            {
                ProfileId = profile.ProfileId,
                UserName = profile.UserName,
                ImageURL = profile.ImageURL,
                PlayerArchetype = profile.PlayerArchetype,
                City = profile.City,
                Ranking = profile.Ranking,
                StarRating = profile.StarRating,
                FollowersCount = profile.FollowersCount,
                FollowingCount = profile.FollowingCount,
                TotalGames = profile.TotalGames,
                WinPercentage = profile.WinPercentage
            };
        }

        private ProfileDetailDto MapToProfileDetailDto(Profile profile)
        {
            return new ProfileDetailDto
            {
                ProfileId = profile.ProfileId,
                UserId = profile.UserId,
                UserName = profile.UserName,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                Height = profile.Height,
                Weight = profile.Weight,
                Position = profile.Position,
                Ranking = profile.Ranking,
                StarRating = profile.StarRating,
                Bio = profile.Bio,
                ImageURL = profile.ImageURL,
                PlayerArchetype = profile.PlayerArchetype,
                City = profile.City,
                Zip = profile.Zip,
                PlayerNumber = profile.PlayerNumber,
                Points = profile.Points,
                TopRecord = profile.TopRecord,
                FollowersCount = profile.FollowersCount,
                FollowingCount = profile.FollowingCount,
                TotalGames = profile.TotalGames,
                TotalWins = profile.TotalWins?.ToString(),
                TotalLosses = profile.TotalLosses?.ToString(),
                WinPercentage = profile.WinPercentage,
                Settings = new SettingsDto
                {
                    AllowComments = profile.Setting?.AllowComments ?? true,
                    ShowGameHistory = profile.Setting?.ShowGameHistory ?? true,
                    AllowEmailNotification = profile.Setting?.AllowEmailNotification ?? true
                },
                IsFollowing = false // Will be set by the caller if needed
            };
        }

        private GameDto MapToGameDto(Game game, string profileId)
        {
            return new GameDto
            {
                GameId = game.GameId,
                GameNumber = game.GameNumber,
                CreatedDate = game.CreatedDate,
                Location = game.Location,
                PrivateRunId = game.PrivateRunId,
                CourtId = game.CourtId,
                UserWinOrLose = game.UserWinOrLose,
                WinnersList = game.WinnersList?.Select(p => MapToProfileDto(p)).ToList(),
                LossersList = game.LossersList?.Select(p => MapToProfileDto(p)).ToList(),
                PrivateRun = game.PrivateRun != null ? new PrivateRunDto
                {
                    PrivateRunId = game.PrivateRun.PrivateRunId,
                    PrivateRunNumber = game.PrivateRun.PrivateRunNumber,
                    Title = game.PrivateRun.Title,
                    RunDate = game.PrivateRun.RunDate.ToString(),
                    RunTime = game.PrivateRun.RunTime
                } : null
            };
        }
    }
}