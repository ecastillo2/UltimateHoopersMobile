using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.DAL;
using DataLayer.DAL.Interface;
using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileRepository _profileRepository;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IProfileRepository profileRepository, ILogger<ProfileController> logger)
        {
            _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all profiles
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProfileViewModelDto>), 200)]
        public async Task<IActionResult> GetProfiles(CancellationToken cancellationToken)
        {
            try
            {
                var profiles = await _profileRepository.GetProfilesAsync(cancellationToken);
                var viewModels = profiles.Select(p => new ProfileViewModelDto(p));

                return Ok(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profiles");
                return StatusCode(500, "An error occurred while retrieving profiles");
            }
        }

        /// <summary>
        /// Get profiles with standard pagination
        /// </summary>
        [HttpGet("paginated")]
        [ProducesResponseType(typeof(PaginatedResultDto<ProfileViewModelDto>), 200)]
        public async Task<IActionResult> GetProfilesPaginated(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var (profiles, totalCount, totalPages) = await _profileRepository
                    .GetProfilesPaginatedAsync(page, pageSize, cancellationToken);

                var viewModels = profiles.Select(p => new ProfileViewModelDto(p)).ToList();


                var result = new PaginatedResultDto<ProfileViewModelDto>
                {
                    Items = viewModels,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated profiles");
                return StatusCode(500, "An error occurred while retrieving paginated profiles");
            }
        }

        /// <summary>
        /// Get profiles with cursor-based pagination for efficient scrolling
        /// <summary>
        /// Get profiles with cursor-based pagination for efficient scrolling
        /// </summary>
        [HttpGet("cursor")]
        [ProducesResponseType(typeof(CursorPaginatedResultDto<ProfileDetailViewModelDto>), 200)]
        public async Task<IActionResult> GetProfilesWithCursor(
            [FromQuery] string cursor = null,
            [FromQuery] int limit = 20,
            [FromQuery] string direction = "next",
            [FromQuery] string sortBy = "Points",
            CancellationToken cancellationToken = default)
        {
            try
            {
                var (profiles, nextCursor) = await _profileRepository
                    .GetProfilesWithCursorAsync(cursor, limit, direction, sortBy, cancellationToken);

                // Create a list to hold our detailed profile view models
                var detailedViewModels = new List<ProfileDetailViewModelDto>();

                // Enrich each profile with additional data
                foreach (var profile in profiles)
                {
                    // Get additional profile data using the profile's ID
                    var profileId = profile.ProfileId;
                    var setting = await _profileRepository.GetProfileSettingsAsync(profileId, cancellationToken);
                    var scoutingReport = await _profileRepository.GetScoutingReportAsync(profileId, cancellationToken);
                    var gameStats = await _profileRepository.GetProfileGameStatisticsAsync(profileId, cancellationToken);

                    // Create a detailed view model with all the additional data
                    var detailedViewModel = new ProfileDetailViewModelDto()
                    {
                        Setting = setting != null ? new Setting(setting) : null,
                        ScoutingReport = scoutingReport != null ? new ScoutingReport(scoutingReport) : null,
                        GameStatistics = gameStats,
                        Profile = profile,
                    };

                    // Add to our list
                    detailedViewModels.Add(detailedViewModel);
                }

                // Create the result with our detailed view models
                var result = new CursorPaginatedResultDto<ProfileDetailViewModelDto>
                {
                    Items = detailedViewModels,
                    NextCursor = nextCursor,
                    HasMore = !string.IsNullOrEmpty(nextCursor),
                    Direction = direction,
                    SortBy = sortBy
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cursor-based profiles");
                return StatusCode(500, "An error occurred while retrieving cursor-based profiles");
            }
        }

        /// <summary>
        /// Get profile by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProfileDetailViewModelDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProfileById(string id, CancellationToken cancellationToken)
        {
            try
            {
                var profile = await _profileRepository.GetProfileByIdAsync(id, cancellationToken);

                if (profile == null)
                    return NotFound();

                // Get additional profile data
                var setting = await _profileRepository.GetProfileSettingsAsync(id, cancellationToken);
                var scoutingReport = await _profileRepository.GetScoutingReportAsync(id, cancellationToken);
                var gameStats = await _profileRepository.GetProfileGameStatisticsAsync(id, cancellationToken);

                var viewModel = new ProfileDetailViewModelDto()
                {
                    Setting = setting != null ? new Setting(setting) : null,
                    ScoutingReport = scoutingReport != null ? new ScoutingReport(scoutingReport) : null,
                    GameStatistics = gameStats,
                    Profile = profile,
                };

                return Ok(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile {ProfileId}", id);
                return StatusCode(500, "An error occurred while retrieving the profile");
            }
        }

        /// <summary>
        /// Get profile by username
        /// </summary>
        [HttpGet("username/{username}")]
        [ProducesResponseType(typeof(ProfileDetailViewModelDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProfileByUsername(string username, CancellationToken cancellationToken)
        {
            try
            {
                var profile = await _profileRepository.GetProfileByUsernameAsync(username, cancellationToken);

                if (profile == null)
                    return NotFound();

                // Get additional profile data
                var setting = await _profileRepository.GetProfileSettingsAsync(profile.ProfileId, cancellationToken);
                var scoutingReport = await _profileRepository.GetScoutingReportAsync(profile.ProfileId, cancellationToken);
                var gameStats = await _profileRepository.GetProfileGameStatisticsAsync(profile.ProfileId, cancellationToken);

                var viewModel = new ProfileDetailViewModelDto()
                {
                    Setting = setting != null ? new Setting(setting) : null,
                    ScoutingReport = scoutingReport != null ? new ScoutingReport(scoutingReport) : null,
                    GameStatistics = gameStats
                };

                return Ok(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile by username {Username}", username);
                return StatusCode(500, "An error occurred while retrieving the profile");
            }
        }

        /// <summary>
        /// Get following profiles for a profile
        /// </summary>
        [HttpGet("{id}/following")]
        [ProducesResponseType(typeof(IEnumerable<ProfileViewModelDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetFollowingProfiles(string id, CancellationToken cancellationToken)
        {
            try
            {
                // First verify profile exists
                var profile = await _profileRepository.GetProfileByIdAsync(id, cancellationToken);

                if (profile == null)
                    return NotFound($"Profile with ID {id} not found");

                var followingProfiles = await _profileRepository.GetFollowingProfilesAsync(id, cancellationToken);
                var viewModels = followingProfiles.Select(p => new ProfileViewModelDto(p));

                return Ok(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving following profiles for {ProfileId}", id);
                return StatusCode(500, "An error occurred while retrieving following profiles");
            }
        }

        /// <summary>
        /// Get follower profiles for a profile
        /// </summary>
        [HttpGet("{id}/followers")]
        [ProducesResponseType(typeof(IEnumerable<ProfileViewModelDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetFollowerProfiles(string id, CancellationToken cancellationToken)
        {
            try
            {
                // First verify profile exists
                var profile = await _profileRepository.GetProfileByIdAsync(id, cancellationToken);

                if (profile == null)
                    return NotFound($"Profile with ID {id} not found");

                var followerProfiles = await _profileRepository.GetFollowerProfilesAsync(id, cancellationToken);
                var viewModels = followerProfiles.Select(p => new ProfileViewModelDto(p));

                return Ok(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving follower profiles for {ProfileId}", id);
                return StatusCode(500, "An error occurred while retrieving follower profiles");
            }
        }

        /// <summary>
        /// Get game history for a profile
        /// </summary>
        [HttpGet("{id}/games")]
        [ProducesResponseType(typeof(IEnumerable<GameViewModelDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProfileGameHistory(string id, CancellationToken cancellationToken)
        {
            try
            {
                // First verify profile exists
                var profile = await _profileRepository.GetProfileByIdAsync(id, cancellationToken);

                if (profile == null)
                    return NotFound($"Profile with ID {id} not found");

                var games = await _profileRepository.GetProfileGameHistoryAsync(id, cancellationToken);
                var viewModels = games.Select(g => new GameViewModelDto(g));

                return Ok(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game history for {ProfileId}", id);
                return StatusCode(500, "An error occurred while retrieving game history");
            }
        }

        /// <summary>
        /// Get game statistics for a profile
        /// </summary>
        [HttpGet("{id}/statistics")]
        [ProducesResponseType(typeof(GameStatistics), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProfileGameStatistics(string id, CancellationToken cancellationToken)
        {
            try
            {
                // First verify profile exists
                var profile = await _profileRepository.GetProfileByIdAsync(id, cancellationToken);

                if (profile == null)
                    return NotFound($"Profile with ID {id} not found");

                var statistics = await _profileRepository.GetProfileGameStatisticsAsync(id, cancellationToken);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game statistics for {ProfileId}", id);
                return StatusCode(500, "An error occurred while retrieving game statistics");
            }
        }

        /// <summary>
        /// Get scouting report for a profile
        /// </summary>
        [HttpGet("{id}/scouting-report")]
        [ProducesResponseType(typeof(ScoutingReportViewModelDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetScoutingReport(string id, CancellationToken cancellationToken)
        {
            try
            {
                // First verify profile exists
                var profile = await _profileRepository.GetProfileByIdAsync(id, cancellationToken);

                if (profile == null)
                    return NotFound($"Profile with ID {id} not found");

                var scoutingReport = await _profileRepository.GetScoutingReportAsync(id, cancellationToken);

                if (scoutingReport == null)
                    return NotFound($"No scouting report found for profile with ID {id}");

                return Ok(new ScoutingReportViewModelDto(scoutingReport));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scouting report for {ProfileId}", id);
                return StatusCode(500, "An error occurred while retrieving the scouting report");
            }
        }

        /// <summary>
        /// Get squad details for a profile
        /// </summary>
        [HttpGet("{id}/squad")]
        [ProducesResponseType(typeof(SquadViewModelDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProfileSquad(string id, CancellationToken cancellationToken)
        {
            try
            {
                // First verify profile exists
                var profile = await _profileRepository.GetProfileByIdAsync(id, cancellationToken);

                if (profile == null)
                    return NotFound($"Profile with ID {id} not found");

                var squad = await _profileRepository.GetProfileSquadAsync(id, cancellationToken);

                if (squad == null)
                    return NotFound($"No squad found for profile with ID {id}");

                return Ok(new SquadViewModelDto(squad));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving squad for {ProfileId}", id);
                return StatusCode(500, "An error occurred while retrieving the squad");
            }
        }

        /// <summary>
        /// Update profile
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProfile(string id, ProfileUpdateModelDto model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != model.ProfileId)
                return BadRequest("Profile ID mismatch");

            try
            {
                var profile = await _profileRepository.GetProfileByIdAsync(id, cancellationToken);

                if (profile == null)
                    return NotFound($"Profile with ID {id} not found");

                // Update profile properties from model
                model.UpdateProfile(profile);

                var success = await _profileRepository.UpdateProfileAsync(profile, cancellationToken);

                if (!success)
                    return StatusCode(500, "Failed to update profile");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile {ProfileId}", id);
                return StatusCode(500, "An error occurred while updating the profile");
            }
        }

        /// <summary>
        /// Update username
        /// </summary>
        [HttpPut("{id}/username")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUsername(string id, [FromBody] UsernameUpdateModelDto model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Verify the profile exists
                var profile = await _profileRepository.GetProfileByIdAsync(id, cancellationToken);

                if (profile == null)
                    return NotFound($"Profile with ID {id} not found");

                // Check if the username is available
                var isAvailable = await _profileRepository.IsUserNameAvailableAsync(model.Username, cancellationToken);

                if (!isAvailable)
                    return BadRequest("Username is already taken");

                var success = await _profileRepository.UpdateProfileUserNameAsync(id, model.Username, cancellationToken);

                if (!success)
                    return StatusCode(500, "Failed to update username");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating username for {ProfileId}", id);
                return StatusCode(500, "An error occurred while updating the username");
            }
        }

        /// <summary>
        /// Update settings
        /// </summary>
        [HttpPut("{id}/settings")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateSettings(string id, [FromBody] SettingUpdateModelDto model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Verify the profile exists
                var profile = await _profileRepository.GetProfileByIdAsync(id, cancellationToken);

                if (profile == null)
                    return NotFound($"Profile with ID {id} not found");

                var setting = await _profileRepository.GetProfileSettingsAsync(id, cancellationToken);

                if (setting == null)
                {
                    // Create new settings if none exist
                    setting = new Setting
                    {
                        SettingId = Guid.NewGuid().ToString(),
                        ProfileId = id
                    };
                }

                // Update settings from model
                model.UpdateSetting(setting);

                var success = await _profileRepository.UpdateSettingAsync(setting, cancellationToken);

                if (!success)
                    return StatusCode(500, "Failed to update settings");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating settings for {ProfileId}", id);
                return StatusCode(500, "An error occurred while updating the settings");
            }
        }

        /// <summary>
        /// Update or create scouting report
        /// </summary>
        [HttpPut("{id}/scouting-report")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpsertScoutingReport(string id, [FromBody] ScoutingReportUpdateModelDto model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Verify the profile exists
                var profile = await _profileRepository.GetProfileByIdAsync(id, cancellationToken);

                if (profile == null)
                    return NotFound($"Profile with ID {id} not found");

                var scoutingReport = await _profileRepository.GetScoutingReportAsync(id, cancellationToken);

                if (scoutingReport == null)
                {
                    // Create new scouting report if none exists
                    scoutingReport = new ScoutingReport
                    {
                        ScoutingReportId = Guid.NewGuid().ToString(),
                        ProfileId = id,
                       
                    };
                }

                // Update scouting report from model
                model.UpdateScoutingReport(scoutingReport);

                var success = await _profileRepository.UpsertScoutingReportAsync(scoutingReport, cancellationToken);

                if (!success)
                    return StatusCode(500, "Failed to update scouting report");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating scouting report for {ProfileId}", id);
                return StatusCode(500, "An error occurred while updating the scouting report");
            }
        }

        /// <summary>
        /// Check if username is available
        /// </summary>
        [HttpGet("checkUsername")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<IActionResult> CheckUsernameAvailability([FromQuery] string username, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return BadRequest("Username is required");

                var isAvailable = await _profileRepository.IsUserNameAvailableAsync(username, cancellationToken);

                return Ok(isAvailable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username availability");
                return StatusCode(500, "An error occurred while checking username availability");
            }
        }
    }
}

