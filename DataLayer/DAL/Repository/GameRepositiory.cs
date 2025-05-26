using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;
using Newtonsoft.Json;
using Common;
using DataLayer.DAL.Interface;
using DataLayer.Context;

namespace DataLayer.DAL.Repository
{
    public class GameRepository : IGameRepository, IDisposable
    {
        public IConfiguration Configuration { get; }
        private ApplicationContext _context;

        public GameRepository(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get Game By Id
        /// </summary>
        /// <param name="GameId">The ID of the game to retrieve</param>
        /// <returns>The game with the specified ID, including winners and losers</returns>
        public async Task<Game> GetGameById(string GameId)
        {
            using (var context = _context)
            {
                try
                {
                    // Fetch the game
                    var game = await context.Game
                        .Where(model => model.GameId == GameId)
                        .FirstOrDefaultAsync();

                    if (game == null)
                        return null;

                    // Get winning player IDs
                    var winningPlayerIds = await context.GameWinningPlayer
                        .Where(wp => wp.GameId == GameId)
                        .Select(wp => wp.ProfileId)
                        .ToListAsync();

                    // Get losing player IDs
                    var losingPlayerIds = await context.GameLosingPlayer
                        .Where(lp => lp.GameId == GameId)
                        .Select(lp => lp.ProfileId)
                        .ToListAsync();

                    // Initialize the lists if they're null
                    game.ProfileList = new List<Profile>();

                    // Fetch winner profiles
                    var winnerProfiles = await context.Profile
                        .Where(profile => winningPlayerIds.Contains(profile.ProfileId))
                        .ToListAsync();

                    // Mark winners
                    foreach (var profile in winnerProfiles)
                    {
                        profile.WinOrLose = "W";
                    }

                    // Fetch loser profiles
                    var loserProfiles = await context.Profile
                        .Where(profile => losingPlayerIds.Contains(profile.ProfileId))
                        .ToListAsync();

                    // Mark losers
                    foreach (var profile in loserProfiles)
                    {
                        profile.WinOrLose = "L";
                    }

                    // Populate the main ProfileList with both winners and losers
                    game.ProfileList = winnerProfiles.Concat(loserProfiles).ToList();

                    // Fetch the associated Run using RunId
                    if (!string.IsNullOrEmpty(game.RunId))
                    {
                        game.Run = await context.Run
                            .FirstOrDefaultAsync(run => run.RunId == game.RunId);
                    }

                    return game;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    Console.WriteLine($"Error fetching game: {ex.Message}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Get Game History
        /// </summary>
        /// <returns>List of games with player information</returns>
        public async Task<List<Game>> GetGameHistory()
        {
            using (var context = _context)
            {
                try
                {
                    // Get all games from the database
                    var games = await context.Game.ToListAsync();
                    var profileGames = new List<Game>();

                    // Loop through each game to populate winners and losers
                    foreach (var game in games)
                    {
                        // Get winning player IDs for this game
                        var winningPlayerIds = await context.GameWinningPlayer
                            .Where(wp => wp.GameId == game.GameId)
                            .Select(wp => wp.ProfileId)
                            .ToListAsync();

                        // Get losing player IDs for this game
                        var losingPlayerIds = await context.GameLosingPlayer
                            .Where(lp => lp.GameId == game.GameId)
                            .Select(lp => lp.ProfileId)
                            .ToListAsync();

                        // Skip games with no players
                        if (!winningPlayerIds.Any() && !losingPlayerIds.Any())
                            continue;

                        // Fetch profiles for winners and mark them with "W"
                        var winnerProfiles = await context.Profile
                            .Where(p => winningPlayerIds.Contains(p.ProfileId))
                            .ToListAsync();

                        // Set WinOrLose property for each winner
                        foreach (var profile in winnerProfiles)
                        {
                            profile.WinOrLose = "W";
                        }

                        // Fetch profiles for losers and mark them with "L"
                        var loserProfiles = await context.Profile
                            .Where(p => losingPlayerIds.Contains(p.ProfileId))
                            .ToListAsync();

                        // Set WinOrLose property for each loser
                        foreach (var profile in loserProfiles)
                        {
                            profile.WinOrLose = "L";
                        }

                        // Initialize the lists if they're null
                        game.ProfileList = new List<Profile>();

                        // Populate the combined ProfileList
                        game.ProfileList = winnerProfiles.Concat(loserProfiles).ToList();

                        // Fetch the associated Run if available
                        if (!string.IsNullOrEmpty(game.RunId))
                        {
                            game.Run = await context.Run
                                .FirstOrDefaultAsync(r => r.RunId == game.RunId);
                        }

                        // Add the game to our result list
                        profileGames.Add(game);
                    }

                    return profileGames;
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"Error fetching game history: {ex.Message}");
                    return new List<Game>();
                }
            }
        }

        /// <summary>
        /// Get Games By ProfileId
        /// </summary>
        /// <param name="profileId">The profile ID to get games for</param>
        /// <returns>List of games the profile participated in</returns>
        public async Task<List<Game>> GetGamesByProfileId(string profileId)
        {
            using (var context = _context)
            {
                try
                {
                    // Find all games where the profile is a winner
                    var winningGameIds = await context.GameWinningPlayer
                        .Where(wp => wp.ProfileId == profileId)
                        .Select(wp => wp.GameId)
                        .ToListAsync();

                    // Find all games where the profile is a loser
                    var losingGameIds = await context.GameLosingPlayer
                        .Where(lp => lp.ProfileId == profileId)
                        .Select(lp => lp.GameId)
                        .ToListAsync();

                    // Combine IDs to get all games the profile participated in
                    var allGameIds = winningGameIds.Concat(losingGameIds).Distinct().ToList();

                    if (!allGameIds.Any())
                        return new List<Game>();

                    // Fetch the games
                    var games = await context.Game
                        .Where(g => allGameIds.Contains(g.GameId))
                        .ToListAsync();

                    // Process each game to add player information
                    var processedGames = new List<Game>();

                    foreach (var game in games)
                    {
                        // Determine if the requested profile is a winner or loser
                        bool isWinner = winningGameIds.Contains(game.GameId);

                      

                        // Get all winning players for this game
                        var winningPlayerIds = await context.GameWinningPlayer
                            .Where(wp => wp.GameId == game.GameId)
                            .Select(wp => wp.ProfileId)
                            .ToListAsync();

                        // Get all losing players for this game
                        var losingPlayerIds = await context.GameLosingPlayer
                            .Where(lp => lp.GameId == game.GameId)
                            .Select(lp => lp.ProfileId)
                            .ToListAsync();

                        // Fetch winner profiles
                        var winnerProfiles = await context.Profile
                            .Where(p => winningPlayerIds.Contains(p.ProfileId))
                            .ToListAsync();

                        // Mark winners
                        foreach (var profile in winnerProfiles)
                        {
                            profile.WinOrLose = "W";
                        }

                        // Fetch loser profiles
                        var loserProfiles = await context.Profile
                            .Where(p => losingPlayerIds.Contains(p.ProfileId))
                            .ToListAsync();

                        // Mark losers
                        foreach (var profile in loserProfiles)
                        {
                            profile.WinOrLose = "L";
                        }

                        // Initialize and populate the ProfileList
                        game.ProfileList = new List<Profile>();
                        game.ProfileList = winnerProfiles.Concat(loserProfiles).ToList();

                        // Fetch the associated Run if available
                        if (!string.IsNullOrEmpty(game.RunId))
                        {
                            game.Run = await context.Run
                                .FirstOrDefaultAsync(r => r.RunId == game.RunId);
                        }

                        processedGames.Add(game);
                    }

                    return processedGames;
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"Error fetching games for profile {profileId}: {ex.Message}");
                    return new List<Game>();
                }
            }
        }

        /// <summary>
        /// Get Games
        /// </summary>
        /// <returns></returns>
        public async Task<List<Game>> GetGames()
        {
            using (var context = _context)
            {
                try
                {
                    // Fetch all games
                    var games = await context.Game.ToListAsync();

                    // Process each game to add player information
                    foreach (var game in games)
                    {
                        // Get winning player IDs for this game
                        var winningPlayerIds = await context.GameWinningPlayer
                            .Where(wp => wp.GameId == game.GameId)
                            .Select(wp => wp.ProfileId)
                            .ToListAsync();

                        // Get losing player IDs for this game
                        var losingPlayerIds = await context.GameLosingPlayer
                            .Where(lp => lp.GameId == game.GameId)
                            .Select(lp => lp.ProfileId)
                            .ToListAsync();

                        // Fetch winner profiles
                        var winnerProfiles = await context.Profile
                            .Where(profile => winningPlayerIds.Contains(profile.ProfileId))
                            .ToListAsync();

                        // Mark winners
                        foreach (var profile in winnerProfiles)
                        {
                            profile.WinOrLose = "W";
                        }

                        // Fetch loser profiles
                        var loserProfiles = await context.Profile
                            .Where(profile => losingPlayerIds.Contains(profile.ProfileId))
                            .ToListAsync();

                        // Mark losers
                        foreach (var profile in loserProfiles)
                        {
                            profile.WinOrLose = "L";
                        }

                        // Initialize and populate the ProfileList
                        game.ProfileList = new List<Profile>();
                        game.ProfileList = winnerProfiles.Concat(loserProfiles).ToList();

                        // Fetch the associated Run using RunId
                        if (!string.IsNullOrEmpty(game.RunId))
                        {
                            game.Run = await context.Run
                                .FirstOrDefaultAsync(run => run.RunId == game.RunId);
                        }
                    }

                    return games;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    Console.WriteLine($"Error fetching games: {ex.Message}");
                    return new List<Game>();
                }
            }
        }

        /// <summary>
        /// Insert Game
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertGame(Game model)
        {
            using (var context = _context)
            {
                try
                {
                    model.GameId = Guid.NewGuid().ToString();
                    model.CreatedDate = DateTime.UtcNow;
                    model.GameNumber = UniqueIdNumber.GenerateSixDigit();
                    await context.Game.AddAsync(model);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error inserting game: {ex.Message}");
                }
                await Save();
            }
        }

        /// <summary>
        /// Update Game
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateGame(Game model)
        {
            using (var context = _context)
            {
                var existingItem = context.Game.Where(s => s.GameId == model.GameId).FirstOrDefault<Game>();

                if (existingItem != null)
                {
                    // Update properties from model to existingItem
                    existingItem.Status = model.Status;
                    existingItem.RunId = model.RunId;
                    existingItem.CourtId = model.CourtId;
                    // Add other properties as needed

                    context.Game.Update(existingItem);
                    await Save();
                }
            }
        }

        /// <summary>
        /// Delete Game
        /// </summary>
        /// <param name="GameId"></param>
        /// <returns></returns>
        public async Task DeleteGame(string GameId)
        {
            using (var context = _context)
            {
                Game obj = (from u in context.Game
                            where u.GameId == GameId
                            select u).FirstOrDefault();

                if (obj != null)
                {
                    _context.Game.Remove(obj);
                    await Save();
                }
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _context?.Dispose();
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <returns></returns>
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }
    }
}