using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.DAL;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProfileService.Models;

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
        [ProducesResponseType(typeof(IEnumerable<ProfileViewModel>), 200)]
        public async Task<IActionResult> GetProfiles(CancellationToken cancellationToken)
        {
            try
            {
                var profiles = await _profileRepository.GetProfilesAsync(cancellationToken);
                var viewModels = profiles.Select(p => new ProfileViewModel(p));

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
        [ProducesResponseType(typeof(PaginatedResult<ProfileViewModel>), 200)]
        public async Task<IActionResult> GetProfilesPaginated(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var (profiles, totalCount, totalPages) = await _profileRepository
                    .GetProfilesPaginatedAsync(page, pageSize, cancellationToken);

                var viewModels = profiles.Select(p => new ProfileViewModel(p)).ToList();

                var result = new PaginatedResult<ProfileViewModel>
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
        /// </summary>
        [HttpGet("cursor")]
        [ProducesResponseType(typeof(CursorPaginatedResult<ProfileViewModel>), 200)]
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

                var viewModels = profiles.Select(p => new ProfileViewModel(p)).ToList();

                var result = new CursorPaginatedResult<ProfileViewModel>
                {
                    Items = viewModels,
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
        [ProducesResponseType(typeof(ProfileDetailViewModel), 200)]
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

                var viewModel = new ProfileDetailViewModel(profile)
                {
                    Setting = setting != null ? new SettingViewModel(setting) : null,
                    ScoutingReport = scoutingReport != null ? new ScoutingReportViewModel(scoutingReport) : null,
                    GameStatistics = gameStats
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
        [ProducesResponseType(typeof(ProfileDetailViewModel), 200)]
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

                var viewModel = new ProfileDetailViewModel(profile)
                {
                    Setting = setting != null ? new SettingViewModel(setting) : null,
                    ScoutingReport = scoutingReport != null ? new ScoutingReportViewModel(scoutingReport) : null,
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
        [ProducesResponseType(typeof(IEnumerable<ProfileViewModel>), 200)]
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
                var viewModels = followingProfiles.Select(p => new ProfileViewModel(p));

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
        [ProducesResponseType(typeof(IEnumerable<ProfileViewModel>), 200)]
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
                var viewModels = followerProfiles.Select(p => new ProfileViewModel(p));

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
        [ProducesResponseType(typeof(IEnumerable<GameViewModel>), 200)]
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
                var viewModels = games.Select(g => new GameViewModel(g));

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
        [ProducesResponseType(typeof(ScoutingReportViewModel), 200)]
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

                return Ok(new ScoutingReportViewModel(scoutingReport));
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
        [ProducesResponseType(typeof(SquadViewModel), 200)]
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

                return Ok(new SquadViewModel(squad));
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
        public async Task<IActionResult> UpdateProfile(string id, ProfileUpdateModel model, CancellationToken cancellationToken)
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
        public async Task<IActionResult> UpdateUsername(string id, [FromBody] UsernameUpdateModel model, CancellationToken cancellationToken)
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
        public async Task<IActionResult> UpdateSettings(string id, [FromBody] SettingUpdateModel model, CancellationToken cancellationToken)
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
        public async Task<IActionResult> UpsertScoutingReport(string id, [FromBody] ScoutingReportUpdateModel model, CancellationToken cancellationToken)
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
                        Status = "Active"
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

// View models for the controller responses

namespace ProfileService.Models
{
    public class ProfileViewModel
    {
        public string ProfileId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string Position { get; set; }
        public string Ranking { get; set; }
        public string StarRating { get; set; }
        public string Bio { get; set; }
        public string ImageURL { get; set; }
        public string PlayerArchetype { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string PlayerNumber { get; set; }
        public string Status { get; set; }
        public int? Points { get; set; }
        public string LastRunDate { get; set; }
        public bool? TopRecord { get; set; }
        public bool? OnSquad { get; set; }

        public ProfileViewModel(Profile profile)
        {
            ProfileId = profile.ProfileId;
            UserId = profile.UserId;
            UserName = profile.UserName;
            Height = profile.Height;
            Weight = profile.Weight;
            Position = profile.Position;
            Ranking = profile.Ranking;
            StarRating = profile.StarRating;
            Bio = profile.Bio;
            ImageURL = profile.ImageURL;
            PlayerArchetype = profile.PlayerArchetype;
            City = profile.City;
            Zip = profile.Zip;
            PlayerNumber = profile.PlayerNumber;
            Status = profile.Status;
            Points = profile.Points;
            LastRunDate = profile.LastRunDate;
            TopRecord = profile.TopRecord;
            OnSquad = profile.OnSquad;
        }
    }

    public class ProfileDetailViewModel : ProfileViewModel
    {
        public SettingViewModel Setting { get; set; }
        public ScoutingReportViewModel ScoutingReport { get; set; }
        public GameStatistics GameStatistics { get; set; }
        public string FollowersCount { get; set; }
        public string FollowingCount { get; set; }

        public ProfileDetailViewModel(Profile profile) : base(profile)
        {
            FollowersCount = profile.FollowersCount;
            FollowingCount = profile.FollowingCount;
        }
    }

    public class GameViewModel
    {
        public string GameId { get; set; }
        public string CourtId { get; set; }
        public string PrivateRunId { get; set; }
        public string CreatedDate { get; set; }
        public string WinProfileIdsStatusString { get; set; }
        public string LoseProfileIdsStatusString { get; set; }
        public string PrivateRunNumber { get; set; }
        public string Location { get; set; }
        public string GameNumber { get; set; }
        public string Status { get; set; }
        public string UserWinOrLose { get; set; }

        public GameViewModel(Game game)
        {
            GameId = game.GameId;
            CourtId = game.CourtId;
            PrivateRunId = game.PrivateRunId;
            CreatedDate = game.CreatedDate;
            WinProfileIdsStatusString = game.WinProfileIdsStatusString;
            LoseProfileIdsStatusString = game.LoseProfileIdsStatusString;
            PrivateRunNumber = game.PrivateRunNumber;
            Location = game.Location;
            GameNumber = game.GameNumber;
            Status = game.Status;
            UserWinOrLose = game.UserWinOrLose;
        }
    }

    public class SettingViewModel
    {
        public string SettingId { get; set; }
        public string ProfileId { get; set; }
        public bool AllowComments { get; set; }
        public bool ShowGameHistory { get; set; }
        public bool AllowEmailNotification { get; set; }

        public SettingViewModel(Setting setting)
        {
            SettingId = setting.SettingId;
            ProfileId = setting.ProfileId;
            AllowComments = setting.AllowComments;
            ShowGameHistory = setting.ShowGameHistory;
            AllowEmailNotification = setting.AllowEmailNotification;
        }
    }

    public class ScoutingReportViewModel
    {
        public string ScoutingReportId { get; set; }
        public string ProfileId { get; set; }
        public string PlayStyle { get; set; }
        public string StrengthOne { get; set; }
        public string StrengthTwo { get; set; }
        public string WeaknessOne { get; set; }
        public string WeaknessTwo { get; set; }
        public string PlayStyleImpactOne { get; set; }
        public string PlayStyleImpactTwo { get; set; }
        public string Comparison { get; set; }
        public string Conclusion { get; set; }
        public string Status { get; set; }
        public string IdealRole { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastUpdated { get; set; }

        public ScoutingReportViewModel(ScoutingReport report)
        {
            ScoutingReportId = report.ScoutingReportId;
            ProfileId = report.ProfileId;
            PlayStyle = report.PlayStyle;
            StrengthOne = report.StrengthOne;
            StrengthTwo = report.StrengthTwo;
            WeaknessOne = report.WeaknessOne;
            WeaknessTwo = report.WeaknessTwo;
            PlayStyleImpactOne = report.PlayStyleImpactOne;
            PlayStyleImpactTwo = report.PlayStyleImpactTwo;
            Comparison = report.Comparison;
            Conclusion = report.Conclusion;
            Status = report.Status;
            IdealRole = report.IdealRole;
            CreatedDate = report.CreatedDate;
            LastUpdated = report.LastUpdated;
        }
    }

    public class SquadViewModel
    {
        public string SquadId { get; set; }
        public string OwnerProfileId { get; set; }
        public string Name { get; set; }

        public SquadViewModel(Squad squad)
        {
            SquadId = squad.SquadId;
            OwnerProfileId = squad.OwnerProfileId;
            Name = squad.Name;
        }
    }

    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    public class CursorPaginatedResult<T>
    {
        public List<T> Items { get; set; }
        public string NextCursor { get; set; }
        public bool HasMore { get; set; }
        public string Direction { get; set; }
        public string SortBy { get; set; }
    }

    public class ProfileUpdateModel
    {
        public string ProfileId { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string Position { get; set; }
        public string Bio { get; set; }
        public string ImageURL { get; set; }
        public string PlayerArchetype { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string PlayerNumber { get; set; }

        public void UpdateProfile(Profile profile)
        {
            profile.Height = Height;
            profile.Weight = Weight;
            profile.Position = Position;
            profile.Bio = Bio;
            profile.ImageURL = ImageURL;
            profile.PlayerArchetype = PlayerArchetype;
            profile.City = City;
            profile.Zip = Zip;
            profile.PlayerNumber = PlayerNumber;
        }
    }

    public class UsernameUpdateModel
    {
        public string Username { get; set; }
    }

    public class SettingUpdateModel
    {
        public bool AllowComments { get; set; }
        public bool ShowGameHistory { get; set; }
        public bool AllowEmailNotification { get; set; }

        public void UpdateSetting(Setting setting)
        {
            setting.AllowComments = AllowComments;
            setting.ShowGameHistory = ShowGameHistory;
            setting.AllowEmailNotification = AllowEmailNotification;
        }
    }

    public class ScoutingReportUpdateModel
    {
        public string PlayStyle { get; set; }
        public string StrengthOne { get; set; }
        public string StrengthTwo { get; set; }
        public string WeaknessOne { get; set; }
        public string WeaknessTwo { get; set; }
        public string PlayStyleImpactOne { get; set; }
        public string PlayStyleImpactTwo { get; set; }
        public string Comparison { get; set; }
        public string Conclusion { get; set; }
        public string IdealRole { get; set; }

        public void UpdateScoutingReport(ScoutingReport report)
        {
            report.PlayStyle = PlayStyle;
            report.StrengthOne = StrengthOne;
            report.StrengthTwo = StrengthTwo;
            report.WeaknessOne = WeaknessOne;
            report.WeaknessTwo = WeaknessTwo;
            report.PlayStyleImpactOne = PlayStyleImpactOne;
            report.PlayStyleImpactTwo = PlayStyleImpactTwo;
            report.Comparison = Comparison;
            report.Conclusion = Conclusion;
            report.IdealRole = IdealRole;
        }
    }
}