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
        public async Task<ActionResult<User>> CreateUser([FromBody] User user, CancellationToken cancellationToken = default)
        {
            try
            {
                // Start a transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Generate a new ID
                user.UserId = Guid.NewGuid().ToString();

                // Set additional properties
                user.SignUpDate = DateTime.Now.ToString();
                user.Status = "Active";
                user.AccessLevel = "Standard";

                // Create a profile for the user
                var profile = new Profile
                {
                    ProfileId = Guid.NewGuid().ToString(),
                    UserId = user.UserId,
                    UserName = user.UserName ?? user.FirstName,
                    Status = "Active"
                };

                // Add both entities
                await _unitOfWork.User.AddAsync(user, cancellationToken);
                await _unitOfWork.Profile.AddAsync(profile, cancellationToken);

                // Commit the transaction
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error creating user");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the user");
            }
        }

        // Add other actions for updating, deleting, etc.
    }
}