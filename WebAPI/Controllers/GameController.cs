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
        /// Get games with cursor-based pagination for efficient scrolling
        /// </summary>
        /// <param name="cursor">Cursor for pagination</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="direction">Direction of scrolling (next/previous)</param>
        /// <param name="sortBy">Field to sort by</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated games result</returns>
        [HttpGet("cursor")]
        [ProducesResponseType(typeof(CursorPaginatedResultDto<GameDetailViewModelDto>), 200)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGamesWithCursor(
            [FromQuery] string cursor = null,
            [FromQuery] int limit = 20,
            [FromQuery] string direction = "next",
            [FromQuery] string sortBy = "CreatedDate",
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate parameters
                if (limit <= 0 || limit > 100)
                    return BadRequest(new { message = "Limit must be between 1 and 100" });

                if (direction != "next" && direction != "previous")
                    return BadRequest(new { message = "Direction must be 'next' or 'previous'" });

                var (games, nextCursor) = await _repository.GetGamesWithCursorAsync(cursor, limit, direction, sortBy, cancellationToken);

                // Map games to detailed view models
                var detailedViewModels = games.Select(game => new GameDetailViewModelDto
                {
                    GameId = game.GameId,
                    GameNumber = game.GameNumber,
                    Status = game.Status,
                    CreatedDate = game.CreatedDate,
                    
                    RunId = game.RunId,
                    CourtId = game.CourtId,
                    ClientId = game.ClientId,
                    ProfileList = game.ProfileList ?? new List<Profile>(),
                    Run = game.Run,
                    Court = game.Court
                }).ToList();

                var result = new CursorPaginatedResultDto<GameDetailViewModelDto>
                {
                    Items = detailedViewModels,
                    NextCursor = nextCursor,
                   // HasMore = !string.IsNullOrEmpty(nextCursor),
                    Direction = direction,
                    SortBy = sortBy,
                    //Count = detailedViewModels.Count
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving games with cursor pagination");
                return StatusCode(500, new { message = "An error occurred while retrieving games" });
            }
        }

        /// <summary>
        /// Get games by profile ID with cursor-based pagination
        /// </summary>
        /// <param name="profileId">Profile ID to filter games by</param>
        /// <param name="cursor">Cursor for pagination</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="direction">Direction of scrolling (next/previous)</param>
        /// <param name="sortBy">Field to sort by</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated games result for the profile</returns>
        [HttpGet("{profileId}/cursor")]
        [ProducesResponseType(typeof(CursorPaginatedResultDto<GameDetailViewModelDto>), 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGamesByProfileIdWithCursor(
            string profileId,
            [FromQuery] string cursor = null,
            [FromQuery] int limit = 20,
            [FromQuery] string direction = "next",
            [FromQuery] string sortBy = "CreatedDate",
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId))
                    return BadRequest(new { message = "Profile ID is required" });

                if (limit <= 0 || limit > 100)
                    return BadRequest(new { message = "Limit must be between 1 and 100" });

                var (games, nextCursor) = await _repository.GetGamesByProfileIdWithCursorAsync(profileId, cursor, limit, direction, sortBy, cancellationToken);

                var detailedViewModels = games.Select(game => new GameDetailViewModelDto
                {
                    GameId = game.GameId,
                    GameNumber = game.GameNumber,
                    Status = game.Status,
                    CreatedDate = game.CreatedDate,
                    //UpdatedDate = game.UpdatedDate,
                    RunId = game.RunId,
                    CourtId = game.CourtId,
                    ClientId = game.ClientId,
                    ProfileList = game.ProfileList ?? new List<Profile>(),
                    Run = game.Run,
                    Court = game.Court
                }).ToList();

                var result = new CursorPaginatedResultDto<GameDetailViewModelDto>
                {
                    Items = detailedViewModels,
                    NextCursor = nextCursor,
                    //HasMore = !string.IsNullOrEmpty(nextCursor),
                    Direction = direction,
                    SortBy = sortBy,
                    //Count = detailedViewModels.Count
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving games by profile ID {ProfileId} with cursor", profileId);
                return StatusCode(500, new { message = "An error occurred while retrieving games for the profile" });
            }
        }

        /// <summary>
        /// Get games by client ID with cursor-based pagination
        /// </summary>
        /// <param name="clientId">Client ID to filter games by</param>
        /// <param name="cursor">Cursor for pagination</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="direction">Direction of scrolling (next/previous)</param>
        /// <param name="sortBy">Field to sort by</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated games result for the client</returns>
        [HttpGet("client/{clientId}/cursor")]
        [ProducesResponseType(typeof(CursorPaginatedResultDto<GameDetailViewModelDto>), 200)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGamesByClientIdWithCursor(
            string clientId,
            [FromQuery] string cursor = null,
            [FromQuery] int limit = 20,
            [FromQuery] string direction = "next",
            [FromQuery] string sortBy = "CreatedDate",
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(clientId))
                    return BadRequest(new { message = "Client ID is required" });

                if (limit <= 0 || limit > 100)
                    return BadRequest(new { message = "Limit must be between 1 and 100" });

                var (games, nextCursor) = await _repository.GetGamesByClientIdWithCursorAsync(clientId, cursor, limit, direction, sortBy, cancellationToken);

                var detailedViewModels = games.Select(game => new GameDetailViewModelDto
                {
                    GameId = game.GameId,
                    GameNumber = game.GameNumber,
                    Status = game.Status,
                    CreatedDate = game.CreatedDate,
                    //UpdatedDate = game.UpdatedDate,
                    RunId = game.RunId,
                    CourtId = game.CourtId,
                    ClientId = game.ClientId,
                    ProfileList = game.ProfileList ?? new List<Profile>(),
                    Run = game.Run,
                    Court = game.Court
                }).ToList();

                var result = new CursorPaginatedResultDto<GameDetailViewModelDto>
                {
                    Items = detailedViewModels,
                    NextCursor = nextCursor,
                   // HasMore = !string.IsNullOrEmpty(nextCursor),
                    Direction = direction,
                    SortBy = sortBy,
                    //Count = detailedViewModels.Count
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving games by client ID {ClientId} with cursor", clientId);
                return StatusCode(500, new { message = "An error occurred while retrieving games for the client" });
            }
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
        /// <response code="400">If the game ID is invalid</response>
        /// <response code="404">If the game was not found</response>
        /// <response code="500">If an error occurred while retrieving the game</response>
        [HttpGet("{gameId}")]
        [ProducesResponseType(typeof(Game), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGameById(string gameId)
        {
            try
            {
                if (string.IsNullOrEmpty(gameId))
                    return BadRequest(new { message = "Game ID is required" });

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
        /// Get games by client ID
        /// </summary>
        /// <param name="clientId">The client ID to filter games by</param>
        /// <returns>List of games with the specified client ID</returns>
        /// <response code="200">Returns the list of games</response>
        /// <response code="400">If the client ID is invalid</response>
        /// <response code="500">If an error occurred while retrieving games</response>
        [HttpGet("{clientId}/GetGameByClientIdAsync")]
        [ProducesResponseType(typeof(List<Game>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGameByClientIdAsync(string clientId)
        {
            try
            {
                if (string.IsNullOrEmpty(clientId))
                    return BadRequest(new { message = "Client ID is required" });

                var games = await _repository.GetGameByClientId(clientId);
                return Ok(games);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving games for client {ClientId}", clientId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving games for the client" });
            }
        }

        /// <summary>
        /// Get games by profile ID
        /// </summary>
        /// <param name="profileId">The ID of the profile</param>
        /// <returns>List of games for the specified profile</returns>
        /// <response code="200">Returns the list of games</response>
        /// <response code="400">If the profile ID is invalid</response>
        /// <response code="500">If an error occurred while retrieving games</response>
        [HttpGet("{profileId}/GetGameByProfileIdAsync")]
        [ProducesResponseType(typeof(List<Game>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGameByProfileIdAsync(string profileId)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId))
                    return BadRequest(new { message = "Profile ID is required" });

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
        [ProducesResponseType(typeof(Game), StatusCodes.Status201Created)]
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
        /// <response code="400">If the game ID is invalid</response>
        /// <response code="404">If the game was not found</response>
        /// <response code="500">If an error occurred while deleting the game</response>
        [HttpDelete("{gameId}/DeleteGameAsync")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteGameAsync(string gameId)
        {
            try
            {
                if (string.IsNullOrEmpty(gameId))
                    return BadRequest(new { message = "Game ID is required" });

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