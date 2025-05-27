using DataLayer.DAL;
using DataLayer.DAL.Interface;
using DataLayer.DAL.Repository;
using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IUserRepository _repository;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository repository, ILogger<UserController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get clients with cursor-based pagination for efficient scrolling
        /// </summary>
        [HttpGet("clients/cursor")]
        [ProducesResponseType(typeof(CursorPaginatedResultDto<UserDetailViewModelDto>), 200)]
        public async Task<IActionResult> GetClientsWithCursor(
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

                // Create a list to hold our detailed profile view models
                var detailedViewModels = new List<UserDetailViewModelDto>();

                // Enrich each profile with additional data
                foreach (var item in users)
                {
                    // Get additional profile data using the profile's ID
                    var user = item;
                    var profile = await _repository.GetProfileByUserId(item.UserId, cancellationToken);

                    // Create a detailed view model with all the additional data
                    var detailedViewModel = new UserDetailViewModelDto()
                    {
                        User = user,
                        Profile = profile,
                    };

                    // Add to our list
                    detailedViewModels.Add(detailedViewModel);
                }

                var result = new CursorPaginatedResultDto<UserDetailViewModelDto>
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
                _logger.LogError(ex, "Error retrieving cursor-based clients");
                return StatusCode(500, "An error occurred while retrieving cursor-based clients");
            }
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

                // Create a list to hold our detailed profile view models
                var detailedViewModels = new List<UserDetailViewModelDto>();

                // Enrich each profile with additional data
                foreach (var item in users)
                {
                    // Get additional profile data using the profile's ID
                    var user = item;
                    var profile = await _repository.GetProfileByUserId(item.UserId, cancellationToken);

                    // Create a detailed view model with all the additional data
                    var detailedViewModel = new UserDetailViewModelDto()
                    {
                        User = user,
                        Profile = profile,
                    };

                    // Add to our list
                    detailedViewModels.Add(detailedViewModel);
                }

                var result = new CursorPaginatedResultDto<UserDetailViewModelDto>
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