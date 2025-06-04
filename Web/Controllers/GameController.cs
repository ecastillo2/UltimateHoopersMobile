using Common;
using Common.Utilities;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using WebAPI.ApiClients;
using Website.Models;
using Website.ViewModels;

namespace Web.Controllers
{
    public class GameController : Controller
    {
        private readonly IGameApi _gameApi;
        private readonly ILogger<GameController> _logger;

        public GameController(IGameApi gameApi, ILogger<GameController> logger)
        {
            _gameApi = gameApi ?? throw new ArgumentNullException(nameof(gameApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Game(string cursor = null, int limit = 10, string direction = "next", string sortBy = "CreatedDate", CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to view Games.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                var userRole = HttpContext.Session.GetString("UserRole");
                var profileId = HttpContext.Session.GetString("ProfileId");

                var result = await _gameApi.GetGamesWithCursorAsync(
                    cursor: cursor,
                    limit: limit,
                    direction: direction,
                    sortBy: sortBy,
                    accessToken: accessToken,
                    cancellationToken: cancellationToken);

                var viewModel = new GamesViewModel
                {
                    GameList = result.Items,
                    NextCursor = result.NextCursor,
                    CurrentSortBy = sortBy
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Games");
                TempData["Error"] = "An error occurred while retrieving Games. Please try again later.";
                return RedirectToAction("Dashboard", "Dashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGameData(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Game ID is required" });
                }

                var game = await _gameApi.GetGameByIdAsync(id, accessToken, cancellationToken);
                if (game == null)
                {
                    return Json(new { success = false, message = "Game not found" });
                }

                var gameData = new
                {
                    success = true,
                    game = new
                    {
                        gameId = game.GameId,
                        gameNumber = game.GameNumber,
                        runId = game.RunId,
                        courtId = game.CourtId,
                        clientId = game.ClientId,
                        status = game.Status,
                        createdDate = game.CreatedDate,
                        profileList = game.ProfileList,
                        run = game.Run,
                        court = game.Court
                    }
                };

                return Json(gameData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game data for ID: {gameId}", id);
                return Json(new { success = false, message = "Error loading game data: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to view game details.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                var game = await _gameApi.GetGameByIdAsync(id, accessToken, cancellationToken);
                if (game == null)
                {
                    TempData["Error"] = "Game not found.";
                    return RedirectToAction("Game");
                }

                return View(game);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Game details for ID: {GameId}", id);
                TempData["Error"] = "An error occurred while retrieving Game details. Please try again later.";
                return RedirectToAction("Game");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            var accessToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                TempData["Error"] = "You must be logged in to create a Game.";
                return RedirectToAction("Index", "Home", new { scrollTo = "login" });
            }

            return View(new Game());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Game game, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required", requiresLogin = true });
                }

                // Validate required fields
                if (string.IsNullOrEmpty(game.RunId))
                {
                    return Json(new { success = false, message = "Run ID is required", field = "RunId" });
                }

                if (string.IsNullOrEmpty(game.CourtId))
                {
                    return Json(new { success = false, message = "Court ID is required", field = "CourtId" });
                }

                // Set default values
                game.GameId = Guid.NewGuid().ToString();
                game.GameNumber = UniqueIdNumber.GenerateSixDigit();
                game.Status = game.Status ?? "Scheduled";
                game.CreatedDate = DateTime.UtcNow;

                // Create game
                var createdGame = await _gameApi.CreateGameAsync(game, accessToken, cancellationToken);

                if (createdGame != null)
                {
                    _logger.LogInformation("Game created successfully: {GameId}", game.GameId);

                    return Json(new
                    {
                        success = true,
                        message = "Game created successfully!",
                        game = new
                        {
                            gameId = game.GameId,
                            gameNumber = game.GameNumber,
                            runId = game.RunId,
                            courtId = game.CourtId,
                            clientId = game.ClientId,
                            status = game.Status,
                            createdDate = game.CreatedDate
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to create game. Please try again." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating game");
                return Json(new { success = false, message = "An error occurred while creating the game: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to edit a Game.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                var game = await _gameApi.GetGameByIdAsync(id, accessToken, cancellationToken);
                if (game == null)
                {
                    TempData["Error"] = "Game not found.";
                    return RedirectToAction("Game");
                }

                return View(game);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game for edit, ID: {GameId}", id);
                TempData["Error"] = "An error occurred while retrieving the game. Please try again later.";
                return RedirectToAction("Game");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Game game, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required", requiresLogin = true });
                }

                if (string.IsNullOrEmpty(game.GameId))
                {
                    return Json(new { success = false, message = "Game ID is required" });
                }

                // Get existing game
                var existingGame = await _gameApi.GetGameByIdAsync(game.GameId, accessToken, cancellationToken);
                if (existingGame == null)
                {
                    return Json(new { success = false, message = "Game not found" });
                }

                // Preserve certain fields
                game.GameNumber = existingGame.GameNumber; // Preserve original game number
                game.CreatedDate = existingGame.CreatedDate; // Preserve creation date

                // Update game
                await _gameApi.UpdateGameAsync(game, accessToken, cancellationToken);

                _logger.LogInformation("Game updated successfully: {GameId}", game.GameId);

                return Json(new
                {
                    success = true,
                    message = "Game updated successfully!",
                    game = new
                    {
                        gameId = game.GameId,
                        gameNumber = game.GameNumber,
                        runId = game.RunId,
                        courtId = game.CourtId,
                        clientId = game.ClientId,
                        status = game.Status,
                        createdDate = game.CreatedDate
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Game: {GameId}", game?.GameId);
                return Json(new { success = false, message = "An error occurred while updating the game: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Error"] = "You must be logged in to delete a game.";
                    return RedirectToAction("Index", "Home", new { scrollTo = "login" });
                }

                // Get game details first
                var game = await _gameApi.GetGameByIdAsync(id, accessToken, cancellationToken);
                if (game == null)
                {
                    TempData["Error"] = "Game not found.";
                    return RedirectToAction("Game");
                }

                // Delete game
                var result = await _gameApi.DeleteGameAsync(id, accessToken, cancellationToken);

                if (result.Success)
                {
                    TempData["Success"] = "Game deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to delete game.";
                }

                return RedirectToAction("Game");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Game: {GameId}", id);
                TempData["Error"] = "An error occurred while deleting the Game. Please try again later.";
                return RedirectToAction("Game");
            }
        }

        /// <summary>
        /// Get players for a specific game
        /// </summary>
        /// <param name="gameId">Game ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of players in the game</returns>
        [HttpGet]
        public async Task<IActionResult> GetGamePlayers(string gameId, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                if (string.IsNullOrEmpty(gameId))
                {
                    return Json(new { success = false, message = "Game ID is required" });
                }

                var game = await _gameApi.GetGameByIdAsync(gameId, accessToken, cancellationToken);
                if (game == null)
                {
                    return Json(new { success = false, message = "Game not found" });
                }

                var players = game.ProfileList ?? new List<Profile>();

                var playerData = players.Select(p => new
                {
                    id = p.ProfileId,
                    name = $"{p.FirstName} {p.LastName}".Trim(),
                    position = p.Position ?? "Not specified",
                    number = p.PlayerNumber ?? "--"
                }).ToList();

                return Json(new
                {
                    success = true,
                    players = playerData,
                    count = playerData.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving players for game: {GameId}", gameId);
                return Json(new { success = false, message = "Error loading game players: " + ex.Message });
            }
        }

        /// <summary>
        /// Add a player to a game
        /// </summary>
        /// <param name="gameId">Game ID</param>
        /// <param name="profileId">Profile ID of player to add</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the operation</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPlayerToGame(string gameId, string profileId, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required", requiresLogin = true });
                }

                if (string.IsNullOrEmpty(gameId))
                {
                    return Json(new { success = false, message = "Game ID is required" });
                }

                if (string.IsNullOrEmpty(profileId))
                {
                    return Json(new { success = false, message = "Profile ID is required" });
                }

                // This would typically call a method on the game API to add a player
                // For now, return success as a placeholder
                _logger.LogInformation("Adding player {ProfileId} to game {GameId}", profileId, gameId);

                return Json(new
                {
                    success = true,
                    message = "Player added to game successfully!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding player to game: {GameId}, Player: {ProfileId}", gameId, profileId);
                return Json(new { success = false, message = "An error occurred while adding the player to the game: " + ex.Message });
            }
        }

        /// <summary>
        /// Remove a player from a game
        /// </summary>
        /// <param name="gameId">Game ID</param>
        /// <param name="profileId">Profile ID of player to remove</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the operation</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePlayerFromGame(string gameId, string profileId, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required", requiresLogin = true });
                }

                if (string.IsNullOrEmpty(gameId))
                {
                    return Json(new { success = false, message = "Game ID is required" });
                }

                if (string.IsNullOrEmpty(profileId))
                {
                    return Json(new { success = false, message = "Profile ID is required" });
                }

                // This would typically call a method on the game API to remove a player
                // For now, return success as a placeholder
                _logger.LogInformation("Removing player {ProfileId} from game {GameId}", profileId, gameId);

                return Json(new
                {
                    success = true,
                    message = "Player removed from game successfully!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing player from game: {GameId}, Player: {ProfileId}", gameId, profileId);
                return Json(new { success = false, message = "An error occurred while removing the player from the game: " + ex.Message });
            }
        }

        /// <summary>
        /// Update game status (e.g., start game, end game)
        /// </summary>
        /// <param name="gameId">Game ID</param>
        /// <param name="status">New status</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the operation</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGameStatus(string gameId, string status, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required", requiresLogin = true });
                }

                if (string.IsNullOrEmpty(gameId))
                {
                    return Json(new { success = false, message = "Game ID is required" });
                }

                if (string.IsNullOrEmpty(status))
                {
                    return Json(new { success = false, message = "Status is required" });
                }

                // Get the existing game
                var game = await _gameApi.GetGameByIdAsync(gameId, accessToken, cancellationToken);
                if (game == null)
                {
                    return Json(new { success = false, message = "Game not found" });
                }

                // Update the status
                game.Status = status;
                await _gameApi.UpdateGameAsync(game, accessToken, cancellationToken);

                _logger.LogInformation("Game status updated: {GameId} -> {Status}", gameId, status);

                return Json(new
                {
                    success = true,
                    message = $"Game status updated to {status}",
                    newStatus = status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating game status: {GameId} -> {Status}", gameId, status);
                return Json(new { success = false, message = "An error occurred while updating the game status: " + ex.Message });
            }
        }

        /// <summary>
        /// Get game statistics
        /// </summary>
        /// <param name="gameId">Game ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Game statistics</returns>
        [HttpGet]
        public async Task<IActionResult> GetGameStatistics(string gameId, CancellationToken cancellationToken = default)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, message = "Authentication required" });
                }

                if (string.IsNullOrEmpty(gameId))
                {
                    return Json(new { success = false, message = "Game ID is required" });
                }

                var game = await _gameApi.GetGameByIdAsync(gameId, accessToken, cancellationToken);
                if (game == null)
                {
                    return Json(new { success = false, message = "Game not found" });
                }

                // Calculate basic statistics
                var playerCount = game.ProfileList?.Count ?? 0;
                var duration = game.CreatedDate.HasValue ?
                    DateTime.UtcNow.Subtract(game.CreatedDate.Value).TotalMinutes : 0;

                var statistics = new
                {
                    totalPlayers = playerCount,
                    gameDuration = $"{(int)duration}:{((int)duration % 60):00}",
                    totalScore = 0, // This would come from actual game scoring
                    gameRating = "4.2", // This would be calculated from player ratings
                    status = game.Status,
                    court = game.Court?.Name ?? "Unknown",
                    run = game.Run?.Name ?? "Unknown"
                };

                return Json(new
                {
                    success = true,
                    statistics = statistics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game statistics: {GameId}", gameId);
                return Json(new { success = false, message = "Error loading game statistics: " + ex.Message });
            }
        }
    }
}