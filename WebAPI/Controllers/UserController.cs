using DataLayer.Context;
using DataLayer.DAL;
using DataLayer.DAL.Interface;
using DataLayer.DAL.Repository;
using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IUserRepository _repository;
        private readonly ILogger<UserController> _logger;

        public UserController(ApplicationContext context, IUserRepository repository, ILogger<UserController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }



        /// <summary>
        /// Get users with cursor-based pagination for efficient scrolling
        /// </summary>
        [HttpGet("cursor")]
        [ProducesResponseType(typeof(CursorPaginatedResultDto<UserDetailViewModelDto>), 200)]
        public async Task<IActionResult> GetUsersWithCursor(
            [FromQuery] string cursor = null,
            [FromQuery] int limit = 20,
            [FromQuery] string direction = "next",
            [FromQuery] string sortBy = "Points",
            CancellationToken cancellationToken = default)
        {
            try
            {
                var (users, nextCursor) = await _repository
                    .GetUsersWithCursorAsync(cursor, limit, direction, sortBy, cancellationToken);

                if (users == null || !users.Any())
                {
                    return Ok(new CursorPaginatedResultDto<UserDetailViewModelDto>
                    {
                        Items = new List<UserDetailViewModelDto>(),
                        NextCursor = null,
                        HasMore = false,
                        Direction = direction,
                        SortBy = sortBy
                    });
                }

                var userIds = users.Select(u => u.UserId).Where(id => id != null).ToList();

                var profiles = await _context.Profile
                    .AsNoTracking()
                    .Where(p => userIds.Contains(p.UserId))
                    .ToListAsync(cancellationToken);

                var profileIds = profiles.Select(p => p.ProfileId).ToList();

                var wins = await _context.GameWinningPlayer
                    .AsNoTracking()
                    .Where(g => profileIds.Contains(g.ProfileId))
                    .GroupBy(g => g.ProfileId)
                    .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);

                var losses = await _context.GameLosingPlayer
                    .AsNoTracking()
                    .Where(g => profileIds.Contains(g.ProfileId))
                    .GroupBy(g => g.ProfileId)
                    .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);

                var followers = await _context.Follower
                    .AsNoTracking()
                    .Where(f => profileIds.Contains(f.ProfileId))
                    .ToListAsync(cancellationToken);

                var followings = await _context.Following
                    .AsNoTracking()
                    .Where(f => profileIds.Contains(f.FollowingProfileId))
                    .ToListAsync(cancellationToken);

                var followerIds = followers.Select(f => f.FollowerProfileId).Distinct().ToList();
                var followingIds = followings.Select(f => f.ProfileId).Distinct().ToList();

                var relatedProfiles = await _context.Profile
                    .AsNoTracking()
                    .Where(p => followerIds.Contains(p.ProfileId) || followingIds.Contains(p.ProfileId))
                    .ToListAsync(cancellationToken);

                var relatedDict = relatedProfiles
                    .Where(p => p != null)
                    .ToDictionary(p => p.ProfileId, p => p);

                var detailedViewModels = new List<UserDetailViewModelDto>();

                foreach (var user in users)
                {
                    var profile = profiles.FirstOrDefault(p => p.UserId == user.UserId);

                    if (profile == null)
                    {
                        _logger?.LogWarning("Profile not found for UserId: {UserId}", user.UserId);
                        continue;
                    }

                    profile.TotalWins = wins.TryGetValue(profile.ProfileId, out var winCount) ? winCount : 0;
                    profile.TotalLosses = losses.TryGetValue(profile.ProfileId, out var lossCount) ? lossCount : 0;

                    var thisFollowers = followers
                        .Where(f => f.ProfileId == profile.ProfileId)
                        .Select(f => relatedDict.TryGetValue(f.FollowerProfileId, out var p) ? p : null)
                        .Where(p => p != null)
                        .ToList();

                    var thisFollowings = followings
                        .Where(f => f.FollowingProfileId == profile.ProfileId)
                        .Select(f => relatedDict.TryGetValue(f.ProfileId, out var p) ? p : null)
                        .Where(p => p != null)
                        .ToList();

                    profile.FollowersList = thisFollowers ?? new List<Profile>();
                    profile.FollowingList = thisFollowings ?? new List<Profile>();
                    profile.FollowersCount = profile.FollowersList.Count;
                    profile.FollowingCount = profile.FollowingList.Count;

                    detailedViewModels.Add(new UserDetailViewModelDto
                    {
                        User = user,
                        Profile = profile
                    });
                }

                return Ok(new CursorPaginatedResultDto<UserDetailViewModelDto>
                {
                    Items = detailedViewModels,
                    NextCursor = nextCursor,
                    HasMore = !string.IsNullOrEmpty(nextCursor),
                    Direction = direction,
                    SortBy = sortBy
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cursor-based users");
                return StatusCode(500, "An error occurred while retrieving cursor-based users");
            }
        }



        /// <summary>
        /// Get User by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDetailViewModelDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserById(string id, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _repository.GetUserByIdAsync(id, cancellationToken);

                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving User {UserId}", id);
                return StatusCode(500, "An error occurred while retrieving the User");
            }
        }


        /// <summary>
        /// Get User by ID
        /// </summary>
        [HttpGet("{id}/profile")]
        [ProducesResponseType(typeof(Profile), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProfileByUserId(string id, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _repository.GetProfileByUserId(id, cancellationToken);

                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving User {UserId}", id);
                return StatusCode(500, "An error occurred while retrieving the User");
            }
        }

        /// <summary>
        /// Get User by ID
        /// </summary>
        [HttpGet("{id}/Scoutingreport")]
        [ProducesResponseType(typeof(ScoutingReport), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProfileScoutingReportByUserId(string id, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _repository.GetProfileScoutingReportByUserId(id, cancellationToken);

                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving User {UserId}", id);
                return StatusCode(500, "An error occurred while retrieving the User");
            }
        }

        /// <summary>
        /// Update User
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(string id, UserUpdateModelDto model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != model.UserId)
                return BadRequest("User ID mismatch");

            try
            {
                var user = await _repository.GetUserByIdAsync(id, cancellationToken);

                if (user == null)
                    return NotFound($"User with ID {id} not found");

                // Update User properties from model
                model.UpdateUser(user);

                var success = await _repository.UpdateUserAsync(user, cancellationToken);

                if (!success)
                    return StatusCode(500, "Failed to update User");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, "An error occurred while updating the user");
            }
        }
    }
}