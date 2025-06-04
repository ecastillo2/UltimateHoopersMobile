using Common;
using DataLayer.Context;
using DataLayer.DAL.Interface;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.Json;

namespace DataLayer.DAL.Repository
{
    public class GameRepository : IGameRepository, IDisposable
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<GameRepository> _logger;
        private readonly IConfiguration _configuration;
        private bool _disposed = false;

        public GameRepository(ApplicationContext context, IConfiguration configuration, ILogger<GameRepository> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        /// <summary>
        /// Get Games With Cursor-based Pagination
        /// </summary>
        /// <param name="cursor">Base64 encoded cursor for pagination</param>
        /// <param name="limit">Number of items to return</param>
        /// <param name="direction">Direction of pagination (next/previous)</param>
        /// <param name="sortBy">Field to sort by</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Games and next cursor</returns>
        public async Task<(List<Game> Games, string NextCursor)> GetGamesWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "CreatedDate",
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate limit
                if (limit <= 0) limit = 20;
                if (limit > 100) limit = 100; // Maximum limit

                // Start with base query
                var query = _context.Game.AsNoTracking();

                // Parse cursor if provided
                GameCursorData cursorData = null;
                if (!string.IsNullOrEmpty(cursor))
                {
                    try
                    {
                        var decodedCursor = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
                        cursorData = System.Text.Json.JsonSerializer.Deserialize<GameCursorData>(decodedCursor);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Invalid cursor format. Starting from beginning");
                        cursorData = null;
                    }
                }

                // Apply cursor filtering based on direction
                if (cursorData != null)
                {
                    if (direction.ToLowerInvariant() == "next")
                    {
                        switch (sortBy.ToLowerInvariant())
                        {
                            case "createddate":
                                if (DateTime.TryParse(cursorData.CreatedDate, out var createdDate))
                                {
                                    query = query.Where(g => g.CreatedDate < createdDate ||
                                        (g.CreatedDate == createdDate && g.GameId.CompareTo(cursorData.Id) < 0));
                                }
                                break;
                            default:
                                query = query.Where(g => g.GameId.CompareTo(cursorData.Id) > 0);
                                break;
                        }
                    }
                    else if (direction.ToLowerInvariant() == "previous")
                    {
                        switch (sortBy.ToLowerInvariant())
                        {
                            case "createddate":
                                if (DateTime.TryParse(cursorData.CreatedDate, out var createdDate))
                                {
                                    query = query.Where(g => g.CreatedDate > createdDate ||
                                        (g.CreatedDate == createdDate && g.GameId.CompareTo(cursorData.Id) > 0));
                                }
                                break;
                            default:
                                query = query.Where(g => g.GameId.CompareTo(cursorData.Id) < 0);
                                break;
                        }
                    }
                }

                // Apply sorting
                switch (sortBy.ToLowerInvariant())
                {
                    case "createddate":
                        query = direction.ToLowerInvariant() == "previous"
                            ? query.OrderBy(g => g.CreatedDate).ThenBy(g => g.GameId)
                            : query.OrderByDescending(g => g.CreatedDate).ThenByDescending(g => g.GameId);
                        break;
                    default:
                        query = direction.ToLowerInvariant() == "previous"
                            ? query.OrderBy(g => g.GameId)
                            : query.OrderByDescending(g => g.GameId);
                        break;
                }

                // Execute query with limit + 1 to check for next page
                var games = await query.Take(limit + 1).ToListAsync(cancellationToken);

                // Check if we have a next page
                string nextCursor = null;
                if (games.Count > limit)
                {
                    var lastItem = games[limit];
                    games.RemoveAt(limit);

                    // Create cursor for next page
                    var newCursorData = new GameCursorData
                    {
                        Id = lastItem.GameId,
                        CreatedDate = lastItem.CreatedDate?.ToString("O") // ISO 8601 format
                    };

                    var serialized = System.Text.Json.JsonSerializer.Serialize(newCursorData);
                    nextCursor = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(serialized));
                }

                // Reverse for previous direction
                if (direction.ToLowerInvariant() == "previous" && games.Any())
                {
                    games.Reverse();
                }

                // Populate game details
                var populatedGames = await PopulateGameDetails(games);

                return (populatedGames, nextCursor);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting games with cursor");
                throw;
            }
        }

        /// <summary>
        /// Get Games by ProfileId with Cursor-based Pagination
        /// </summary>
        public async Task<(List<Game> Games, string NextCursor)> GetGamesByProfileIdWithCursorAsync(
            string profileId,
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "CreatedDate",
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId))
                    return (new List<Game>(), null);

                // Find all games where the profile participated
                var winningGameIds = await _context.GameWinningPlayer
                    .Where(wp => wp.ProfileId == profileId)
                    .Select(wp => wp.GameId)
                    .ToListAsync(cancellationToken);

                var losingGameIds = await _context.GameLosingPlayer
                    .Where(lp => lp.ProfileId == profileId)
                    .Select(lp => lp.GameId)
                    .ToListAsync(cancellationToken);

                var allGameIds = winningGameIds.Concat(losingGameIds).Distinct().ToList();

                if (!allGameIds.Any())
                    return (new List<Game>(), null);

                // Use base cursor method with additional filtering
                var query = _context.Game.AsNoTracking().Where(g => allGameIds.Contains(g.GameId));

                // Apply cursor logic similar to GetGamesWithCursorAsync but with the filtered query
                GameCursorData cursorData = null;
                if (!string.IsNullOrEmpty(cursor))
                {
                    try
                    {
                        var decodedCursor = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
                        cursorData = System.Text.Json.JsonSerializer.Deserialize<GameCursorData>(decodedCursor);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Invalid cursor format for profile games");
                        cursorData = null;
                    }
                }

                // Apply cursor filtering and sorting (same logic as GetGamesWithCursorAsync)
                if (cursorData != null)
                {
                    if (direction.ToLowerInvariant() == "next")
                    {
                        query = query.Where(g => g.GameId.CompareTo(cursorData.Id) > 0);
                    }
                    else if (direction.ToLowerInvariant() == "previous")
                    {
                        query = query.Where(g => g.GameId.CompareTo(cursorData.Id) < 0);
                    }
                }

                query = direction.ToLowerInvariant() == "previous"
                    ? query.OrderBy(g => g.GameId)
                    : query.OrderByDescending(g => g.GameId);

                var games = await query.Take(limit + 1).ToListAsync(cancellationToken);

                string nextCursor = null;
                if (games.Count > limit)
                {
                    var lastItem = games[limit];
                    games.RemoveAt(limit);

                    var newCursorData = new GameCursorData
                    {
                        Id = lastItem.GameId,
                        CreatedDate = lastItem.CreatedDate?.ToString("O")
                    };

                    var serialized = System.Text.Json.JsonSerializer.Serialize(newCursorData);
                    nextCursor = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(serialized));
                }

                if (direction.ToLowerInvariant() == "previous" && games.Any())
                {
                    games.Reverse();
                }

                var populatedGames = await PopulateGameDetails(games);
                return (populatedGames, nextCursor);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching games by profile ID '{ProfileId}' with cursor", profileId);
                return (new List<Game>(), null);
            }
        }

        /// <summary>
        /// Get Games by ClientId with Cursor-based Pagination
        /// </summary>
        public async Task<(List<Game> Games, string NextCursor)> GetGamesByClientIdWithCursorAsync(
            string clientId,
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "CreatedDate",
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(clientId))
                    return (new List<Game>(), null);

                var query = _context.Game.AsNoTracking().Where(g => g.ClientId == clientId);

                // Apply cursor logic
                GameCursorData cursorData = null;
                if (!string.IsNullOrEmpty(cursor))
                {
                    try
                    {
                        var decodedCursor = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
                        cursorData = System.Text.Json.JsonSerializer.Deserialize<GameCursorData>(decodedCursor);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Invalid cursor format for client games");
                        cursorData = null;
                    }
                }

                if (cursorData != null)
                {
                    if (direction.ToLowerInvariant() == "next")
                    {
                        query = query.Where(g => g.GameId.CompareTo(cursorData.Id) > 0);
                    }
                    else if (direction.ToLowerInvariant() == "previous")
                    {
                        query = query.Where(g => g.GameId.CompareTo(cursorData.Id) < 0);
                    }
                }

                query = direction.ToLowerInvariant() == "previous"
                    ? query.OrderBy(g => g.GameId)
                    : query.OrderByDescending(g => g.GameId);

                var games = await query.Take(limit + 1).ToListAsync(cancellationToken);

                string nextCursor = null;
                if (games.Count > limit)
                {
                    var lastItem = games[limit];
                    games.RemoveAt(limit);

                    var newCursorData = new GameCursorData
                    {
                        Id = lastItem.GameId,
                        CreatedDate = lastItem.CreatedDate?.ToString("O")
                    };

                    var serialized = System.Text.Json.JsonSerializer.Serialize(newCursorData);
                    nextCursor = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(serialized));
                }

                if (direction.ToLowerInvariant() == "previous" && games.Any())
                {
                    games.Reverse();
                }

                var populatedGames = await PopulateGameDetails(games);
                return (populatedGames, nextCursor);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching games by client ID '{ClientId}' with cursor", clientId);
                return (new List<Game>(), null);
            }
        }

        /// <summary>
        /// Get Game By Id
        /// </summary>
        public async Task<Game> GetGameById(string gameId)
        {
            try
            {
                if (string.IsNullOrEmpty(gameId))
                    return null;

                var game = await _context.Game
                    .Where(model => model.GameId == gameId)
                    .FirstOrDefaultAsync();

                if (game == null)
                    return null;

                var games = await PopulateGameDetails(new List<Game> { game });
                return games.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching game by ID: {GameId}", gameId);
                return null;
            }
        }

        /// <summary>
        /// Get all games by Client ID
        /// </summary>
        public async Task<List<Game>> GetGameByClientId(string clientId)
        {
            try
            {
                if (string.IsNullOrEmpty(clientId))
                    return new List<Game>();

                var games = await _context.Game
                    .Where(model => model.ClientId == clientId)
                    .ToListAsync();

                if (!games.Any())
                    return new List<Game>();

                return await PopulateGameDetails(games);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching games by client ID '{ClientId}'", clientId);
                return new List<Game>();
            }
        }

        /// <summary>
        /// Helper method to populate game details efficiently
        /// </summary>
        private async Task<List<Game>> PopulateGameDetails(List<Game> games)
        {
            try
            {
                if (!games.Any())
                    return games;

                var gameIds = games.Select(g => g.GameId).ToList();

                // Get all winning and losing players in batch
                var allWinningPlayers = await _context.GameWinningPlayer
                    .Where(wp => gameIds.Contains(wp.GameId))
                    .ToListAsync();

                var allLosingPlayers = await _context.GameLosingPlayer
                    .Where(lp => gameIds.Contains(lp.GameId))
                    .ToListAsync();

                // Get all unique profile IDs
                var allProfileIds = allWinningPlayers.Select(wp => wp.ProfileId)
                    .Concat(allLosingPlayers.Select(lp => lp.ProfileId))
                    .Distinct()
                    .ToList();

                // Fetch all profiles at once
                var allProfiles = await _context.Profile
                    .Where(p => allProfileIds.Contains(p.ProfileId))
                    .ToListAsync();

                // Get runs and courts in batch
                var runIds = games.Where(g => !string.IsNullOrEmpty(g.RunId))
                    .Select(g => g.RunId)
                    .Distinct()
                    .ToList();

                var courtIds = games.Where(g => !string.IsNullOrEmpty(g.CourtId))
                    .Select(g => g.CourtId)
                    .Distinct()
                    .ToList();

                var allRuns = runIds.Any() ? await _context.Run
                    .Where(r => runIds.Contains(r.RunId))
                    .ToListAsync() : new List<Run>();

                var allCourts = courtIds.Any() ? await _context.Court
                    .Where(c => courtIds.Contains(c.CourtId))
                    .ToListAsync() : new List<Court>();

                // Process each game
                foreach (var game in games)
                {
                    game.ProfileList = new List<Profile>();

                    var winningPlayerIds = allWinningPlayers
                        .Where(wp => wp.GameId == game.GameId)
                        .Select(wp => wp.ProfileId)
                        .ToList();

                    var losingPlayerIds = allLosingPlayers
                        .Where(lp => lp.GameId == game.GameId)
                        .Select(lp => lp.ProfileId)
                        .ToList();

                    // Create winner profiles
                    var winnerProfiles = allProfiles
                        .Where(profile => winningPlayerIds.Contains(profile.ProfileId))
                        .Select(profile => CreateProfileCopy(profile, "W"))
                        .ToList();

                    // Create loser profiles
                    var loserProfiles = allProfiles
                        .Where(profile => losingPlayerIds.Contains(profile.ProfileId))
                        .Select(profile => CreateProfileCopy(profile, "L"))
                        .ToList();

                    game.ProfileList = winnerProfiles.Concat(loserProfiles).ToList();

                    // Attach run and court
                    if (!string.IsNullOrEmpty(game.RunId))
                    {
                        game.Run = allRuns.FirstOrDefault(run => run.RunId == game.RunId);
                    }

                    if (!string.IsNullOrEmpty(game.CourtId))
                    {
                        game.Court = allCourts.FirstOrDefault(court => court.CourtId == game.CourtId);
                    }
                }

                return games;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error populating game details");
                return games;
            }
        }

        /// <summary>
        /// Helper method to create a copy of Profile with WinOrLose marker
        /// </summary>
        private Profile CreateProfileCopy(Profile original, string winOrLose)
        {
            return new Profile
            {
                ProfileId = original.ProfileId,
                UserName = original.UserName,
                ImageURL = original.ImageURL,

                // Add any other Profile properties as needed
                WinOrLose = winOrLose
            };
        }

        /// <summary>
        /// Get Game History - Optimized
        /// </summary>
        public async Task<List<Game>> GetGameHistory()
        {
            try
            {
                var games = await _context.Game.ToListAsync();
                if (!games.Any())
                    return new List<Game>();

                return await PopulateGameDetails(games);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching game history");
                return new List<Game>();
            }
        }

        /// <summary>
        /// Get Games By ProfileId - Optimized
        /// </summary>
        public async Task<List<Game>> GetGamesByProfileId(string profileId)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId))
                    return new List<Game>();

                var winningGameIds = await _context.GameWinningPlayer
                    .Where(wp => wp.ProfileId == profileId)
                    .Select(wp => wp.GameId)
                    .ToListAsync();

                var losingGameIds = await _context.GameLosingPlayer
                    .Where(lp => lp.ProfileId == profileId)
                    .Select(lp => lp.GameId)
                    .ToListAsync();

                var allGameIds = winningGameIds.Concat(losingGameIds).Distinct().ToList();
                if (!allGameIds.Any())
                    return new List<Game>();

                var games = await _context.Game
                    .Where(g => allGameIds.Contains(g.GameId))
                    .ToListAsync();

                return await PopulateGameDetails(games);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching games for profile {ProfileId}", profileId);
                return new List<Game>();
            }
        }

        /// <summary>
        /// Get all Games - Optimized
        /// </summary>
        public async Task<List<Game>> GetGames()
        {
            try
            {
                var games = await _context.Game.ToListAsync();
                if (!games.Any())
                    return new List<Game>();

                return await PopulateGameDetails(games);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching all games");
                return new List<Game>();
            }
        }

        /// <summary>
        /// Insert Game
        /// </summary>
        public async Task InsertGame(Game model)
        {
            try
            {
                if (model == null)
                    throw new ArgumentNullException(nameof(model));

                model.GameId = Guid.NewGuid().ToString();
                model.CreatedDate = DateTime.UtcNow;
                model.GameNumber = UniqueIdNumber.GenerateSixDigit();

                await _context.Game.AddAsync(model);
                await Save();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error inserting game");
                throw;
            }
        }

        /// <summary>
        /// Update Game
        /// </summary>
        public async Task UpdateGame(Game model)
        {
            try
            {
                if (model == null)
                    throw new ArgumentNullException(nameof(model));

                var existingItem = await _context.Game
                    .Where(s => s.GameId == model.GameId)
                    .FirstOrDefaultAsync();

                if (existingItem != null)
                {
                    existingItem.Status = model.Status;
                    existingItem.RunId = model.RunId;
                    existingItem.CourtId = model.CourtId;
                    existingItem.ClientId = model.ClientId;
                 

                    _context.Game.Update(existingItem);
                    await Save();
                }
                else
                {
                    throw new ArgumentException($"Game with ID {model.GameId} not found");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating game");
                throw;
            }
        }

        /// <summary>
        /// Delete Game
        /// </summary>
        public async Task DeleteGame(string gameId)
        {
            try
            {
                if (string.IsNullOrEmpty(gameId))
                    throw new ArgumentException("GameId cannot be null or empty");

                var game = await _context.Game
                    .Where(u => u.GameId == gameId)
                    .FirstOrDefaultAsync();

                if (game != null)
                {
                    // Delete related records
                    var winningPlayers = _context.GameWinningPlayer.Where(wp => wp.GameId == gameId);
                    var losingPlayers = _context.GameLosingPlayer.Where(lp => lp.GameId == gameId);

                    _context.GameWinningPlayer.RemoveRange(winningPlayers);
                    _context.GameLosingPlayer.RemoveRange(losingPlayers);
                    _context.Game.Remove(game);

                    await Save();
                }
                else
                {
                    throw new ArgumentException($"Game with ID {gameId} not found");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting game");
                throw;
            }
        }

        /// <summary>
        /// Save changes to database
        /// </summary>
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Dispose pattern implementation
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Helper class for cursor-based pagination
        /// </summary>
        internal class GameCursorData
        {
            public string Id { get; set; }
            public string CreatedDate { get; set; }
            public string Category { get; set; }
            public string Points { get; set; }
        }

        /// <summary>
        /// Base cursor data class
        /// </summary>
        internal class CursorData
        {
            public string Id { get; set; }
        }
    }
}