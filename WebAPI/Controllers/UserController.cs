using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.DAL;
using Domain;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserController> _logger;

        public UserController(IUnitOfWork unitOfWork, ILogger<UserController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(CancellationToken cancellationToken = default)
        {
            try
            {
                var users = await _unitOfWork.User.GetAllAsync(cancellationToken: cancellationToken);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving users");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _unitOfWork.User.GetByIdAsync(id, cancellationToken);

                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the user");
            }
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user, [FromQuery] string password, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(password))
                    return BadRequest("Password is required");

                if (string.IsNullOrEmpty(user.Email))
                    return BadRequest("Email is required");

                // Check if email is available
                var isEmailAvailable = await _unitOfWork.User.IsEmailAvailableAsync(user.Email, cancellationToken);
                if (!isEmailAvailable)
                    return BadRequest("Email is already in use");

                // Start a transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Create the user with the password
                    var createdUser = await _unitOfWork.User.CreateUserAsync(user, password, cancellationToken);

                    // Create a profile for the user if one doesn't exist
                    if (string.IsNullOrEmpty(createdUser.ProfileId))
                    {
                        var profile = new Profile
                        {
                            ProfileId = Guid.NewGuid().ToString(),
                            UserId = createdUser.UserId,
                            UserName = string.IsNullOrEmpty(user.UserName) ? user.Email.Split('@')[0] : user.UserName,
                            Status = "Active"
                        };

                        // Create a new setting for the profile
                        var setting = new Setting
                        {
                            SettingId = Guid.NewGuid().ToString(),
                            ProfileId = profile.ProfileId,
                            AllowComments = true,
                            ShowGameHistory = true,
                            AllowEmailNotification = true
                        };

                        var success = await _unitOfWork.Profile.UpdateProfileAsync(profile, cancellationToken);
                        if (!success)
                        {
                            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create profile for user");
                        }

                        // Update the user with the profile ID
                        createdUser.ProfileId = profile.ProfileId;
                        _unitOfWork.User.Update(createdUser); // This method doesn't return a Task
                        await _unitOfWork.SaveChangesAsync(cancellationToken); // Save the changes
                    }

                    // Commit the transaction
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    // Remove sensitive data before returning
                    createdUser.Password = null;
                    createdUser.PasswordHash = null;

                    return CreatedAtAction(nameof(GetUser), new { id = createdUser.UserId }, createdUser);
                }
                catch (Exception)
                {
                    // Rollback the transaction if any step fails
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the user");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User user, CancellationToken cancellationToken = default)
        {
            if (id != user.UserId)
                return BadRequest("User ID mismatch");

            try
            {
                var existingUser = await _unitOfWork.User.GetByIdAsync(id, cancellationToken);

                if (existingUser == null)
                    return NotFound();

                // Update only allowed fields
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.City = user.City;
                existingUser.State = user.State;
                existingUser.Zip = user.Zip;
                existingUser.Country = user.Country;
                existingUser.Status = user.Status;

                // Update the user
                _unitOfWork.User.Update(existingUser);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the user");
            }
        }

        [HttpPut("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordModel model, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(model.NewPassword))
                return BadRequest("New password is required");

            try
            {
                var user = await _unitOfWork.User.GetByIdAsync(id, cancellationToken);

                if (user == null)
                    return NotFound();

                // If current password is provided, verify it
                if (!string.IsNullOrEmpty(model.CurrentPassword))
                {
                    var isPasswordValid = _unitOfWork.User.VerifyPassword(user, model.CurrentPassword);
                    if (!isPasswordValid)
                        return BadRequest("Current password is incorrect");
                }

                // Change the password
                await _unitOfWork.User.ChangePasswordAsync(id, model.NewPassword, cancellationToken);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while changing the password");
            }
        }
    }

    public class ChangePasswordModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}