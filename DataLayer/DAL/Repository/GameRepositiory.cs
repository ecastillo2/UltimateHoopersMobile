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

                    //// Fetch winner profiles
                    //game.WinnersList = await context.Profile
                    //    .Where(profile => winningPlayerIds.Contains(profile.ProfileId))
                    //    .ToListAsync();

                    //// Fetch loser profiles
                    //game.LossersList = await context.Profile
                    //    .Where(profile => losingPlayerIds.Contains(profile.ProfileId))
                    //    .ToListAsync();

                    //// Populate the main ProfileList
                    //game.ProfileList = game.WinnersList.Concat(game.LossersList).ToList();

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

                        // Assign winners and losers to the game
                        //game.WinnersList = winnerProfiles;
                        //game.LossersList = loserProfiles;

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

                    // Parse WinProfileIdsStatusString and LoseProfileIdsStatusString
                    foreach (var game in games)
                    {
                        // Deserialize the JSON strings
                        var winProfileStatuses = DeserializeProfileStatuses(game.WinProfileIdsStatusString);
                        var loseProfileStatuses = DeserializeProfileStatuses(game.LoseProfileIdsStatusString);

                        // Extract Profile IDs from statuses
                        var winProfileIds = winProfileStatuses.Select(p => p.ProfileId).ToList();
                        var loseProfileIds = loseProfileStatuses.Select(p => p.ProfileId).ToList();

                        // Fetch profiles from the database
                        game.WinnersList = await context.Profile
                            .Where(profile => winProfileIds.Contains(profile.ProfileId))
                            .ToListAsync();

                        game.LossersList = await context.Profile
                            .Where(profile => loseProfileIds.Contains(profile.ProfileId))
                            .ToListAsync();

                        // Populate the main ProfileList
                        game.ProfileList = game.WinnersList.Concat(game.LossersList).ToList();

                        // Fetch the associated PrivateRun using PrivateRunId
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
                    return null;
                }
            }
        }

        /// <summary>
        /// Deserializes a JSON string into a list of ProfileStatus objects.
        /// </summary>
        /// <param name="jsonString">JSON string representing profile statuses.</param>
        /// <returns>List of ProfileStatus objects.</returns>
        private List<ProfileStatus> DeserializeProfileStatuses(string? jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
                return new List<ProfileStatus>();

            try
            {
                return JsonConvert.DeserializeObject<List<ProfileStatus>>(jsonString) ?? new List<ProfileStatus>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing JSON string: {ex.Message}");
                return new List<ProfileStatus>();
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
                    model.CreatedDate = DateTime.Now.ToString("MM/dd/yyyy");
                    model.GameNumber = UniqueIdNumber.GenerateSixDigit();
                    await context.Game.AddAsync(model);
                }
                catch (Exception ex)
                {

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

                    context.Game.Update(existingItem);
                    await Save();
                }
                else
                {

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



                _context.Game.Remove(obj);
                await Save();
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            throw new NotImplementedException();
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
