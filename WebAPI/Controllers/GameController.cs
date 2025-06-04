using DataLayer.DAL.Interface;
using Domain;
using Domain.DtoModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for managing game-related operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IGameRepository _repository;
        private readonly ILogger<GameController> _logger;

        /// <summary>
        /// Initializes a new instance of the GameController
        /// </summary>
        /// <param name="repository">Game repository</param>
        /// <param name="logger">Logger</param>
        public GameController(IGameRepository repository, ILogger<GameController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

      

        /// <summary>
        /// Get all games
        /// </summary>
        /// <returns>List of all games</returns>
        /// <response code="200">Returns the list of games</response>
        /// <response code="500">If an error occurred while retrieving games</response>
        [HttpGet("GetGames")]
        [ProducesResponseType(typeof(List<Game>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGames()
        {
            try
            {
                var games = await _repository.GetGames();
                return Ok(games);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving games");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving games" });
            }
        }

        /// <summary>
        /// Get a game by ID
        /// </summary>
        /// <param name="gameId">The ID of the game to retrieve</param>
        /// <returns>The game with the specified ID</returns>
        /// <response code="200">Returns the game</response>
        /// <response code="404">If the game was not found</response>
        /// <response code="500">If an error occurred while retrieving the game</response>
        [HttpGet("GetGameById")]
        [ProducesResponseType(typeof(Game), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGameById(string gameId)
        {
            try
            {
                var game = await _repository.GetGameById(gameId);

                if (game == null)
                    return NotFound(new { message = $"Game with ID {gameId} not found" });

                return Ok(game);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game {GameId}", gameId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the game" });
            }
        }

        /// <summary>
        /// Get a game by Client ID
        /// </summary>
        /// <param name="gameId">The ID of the game to retrieve</param>
        /// <returns>The game with the specified ID</returns>
        /// <response code="200">Returns the game</response>
        /// <response code="404">If the game was not found</response>
        /// <response code="500">If an error occurred while retrieving the game</response>
        [HttpGet("GetGameByClientId")]
        [ProducesResponseType(typeof(Game), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGameByClientId(string gameId)
        {
            try
            {
                var game = await _repository.GetGameByClientId(gameId);

                if (game == null)
                    return NotFound(new { message = $"Game with ID {gameId} not found" });

                return Ok(game);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game {GameId}", gameId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the game" });
            }
        }

        /// <summary>
        /// Get games by profile ID
        /// </summary>
        /// <param name="profileId">The ID of the profile</param>
        /// <returns>List of games for the specified profile</returns>
        /// <response code="200">Returns the list of games</response>
        /// <response code="500">If an error occurred while retrieving games</response>
        [HttpGet("GetGamesByProfileId")]
        [ProducesResponseType(typeof(List<Game>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGamesByProfileId(string profileId)
        {
            try
            {
                var games = await _repository.GetGamesByProfileId(profileId);
                return Ok(games);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving games for profile {ProfileId}", profileId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving games for the profile" });
            }
        }

        /// <summary>
        /// Create a new game
        /// </summary>
        /// <param name="game">The game to create</param>
        /// <returns>Result of the creation operation</returns>
        /// <response code="201">If the game was created successfully</response>
        /// <response code="400">If the game data is invalid</response>
        /// <response code="500">If an error occurred while creating the game</response>
        [HttpPost("CreateGame")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateGame([FromBody] Game game)
        {
            try
            {
                if (game == null)
                    return BadRequest(new { message = "Game data is required" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _repository.InsertGame(game);

                return CreatedAtAction(nameof(GetGameById), new { gameId = game.GameId }, game);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating game");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the game" });
            }
        }

        /// <summary>
        /// Update an existing game
        /// </summary>
        /// <param name="game">The game with updated information</param>
        /// <returns>Result of the update operation</returns>
        /// <response code="204">If the game was updated successfully</response>
        /// <response code="400">If the game data is invalid</response>
        /// <response code="404">If the game was not found</response>
        /// <response code="500">If an error occurred while updating the game</response>
        [HttpPut("UpdateGame")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateGame([FromBody] Game game)
        {
            try
            {
                if (game == null)
                    return BadRequest(new { message = "Game data is required" });

                if (string.IsNullOrEmpty(game.GameId))
                    return BadRequest(new { message = "Game ID is required" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingGame = await _repository.GetGameById(game.GameId);
                if (existingGame == null)
                    return NotFound(new { message = $"Game with ID {game.GameId} not found" });

                await _repository.UpdateGame(game);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating game {GameId}", game?.GameId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the game" });
            }
        }

        /// <summary>
        /// Get game history
        /// </summary>
        /// <returns>Game history</returns>
        /// <response code="200">Returns the game history</response>
        /// <response code="500">If an error occurred while retrieving game history</response>
        [HttpGet("GetGameHistory")]
        [ProducesResponseType(typeof(List<Game>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGameHistory()
        {
            try
            {
                var gameHistory = await _repository.GetGameHistory();
                return Ok(gameHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game history");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving game history" });
            }
        }

        /// <summary>
        /// Delete a game
        /// </summary>
        /// <param name="gameId">The ID of the game to delete</param>
        /// <returns>Result of the deletion operation</returns>
        /// <response code="204">If the game was deleted successfully</response>
        /// <response code="404">If the game was not found</response>
        /// <response code="500">If an error occurred while deleting the game</response>
        [HttpDelete("DeleteGame")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteGame(string gameId)
        {
            try
            {
                var existingGame = await _repository.GetGameById(gameId);
                if (existingGame == null)
                    return NotFound(new { message = $"Game with ID {gameId} not found" });

                await _repository.DeleteGame(gameId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting game {GameId}", gameId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the game" });
            }
        }
    }
}