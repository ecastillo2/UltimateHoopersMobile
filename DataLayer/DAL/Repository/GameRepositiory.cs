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
        /// Get Tag By Id
        /// </summary>
        /// <param name="TagId"></param>
        /// <returns></returns>
        public async Task<Game> GetGameById(string GameId)
        {
            using (var context = _context)
            {
                try
                {
                    // Fetch all games
                    var game = await (from model in context.Game
                                                   where model.GameId == GameId
                                                   select model).FirstOrDefaultAsync();

                    // Filter games by checking if the ProfileId exists in win or lose JSON strings
                    var filteredGames = new List<Game>();

                        // Deserialize the JSON strings
                        var profileStatuses = DeserializeProfileStatuses(game.WinProfileIdsStatusString);

                        // Check if ProfileId exists in the winner or loser lists
                        bool isProfileInWinList = profileStatuses.Any(p => p.Status == "W");
                        bool isProfileInLoseList = profileStatuses.Any(p => p.Status == "L");

                        // Fetch profiles for winners and losers
                        var winProfileIds = profileStatuses
                            .Where(p => p.Status == "W")
                            .Select(p => p.ProfileId)
                            .ToList();

                        var loseProfileIds = profileStatuses
                            .Where(p => p.Status == "L")
                            .Select(p => p.ProfileId)
                            .ToList();

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

                     
                    

                    return game;
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
        /// Get Game History
        /// </summary>
        /// <returns></returns>
        public async Task<List<Game>> GetGameHistory()
        {
            using (var context = _context)
            {
                // Get all games from the database
                var games = await context.Game.ToListAsync();
                var profileGames = new List<Game>();



                // Loop through each game
                foreach (var game in games)
                {
                    // Parse the WinProfileIdsStatusString and LoseProfileIdsStatusString into lists of Profile objects
                    List<Profile> winProfiles = string.IsNullOrEmpty(game.WinProfileIdsStatusString)
                        ? new List<Profile>()
                        : JsonConvert.DeserializeObject<List<Profile>>(game.WinProfileIdsStatusString);

                    List<Profile> loseProfiles = string.IsNullOrEmpty(game.LoseProfileIdsStatusString)
                        ? new List<Profile>()
                        : JsonConvert.DeserializeObject<List<Profile>>(game.LoseProfileIdsStatusString);

                    // Check if the ProfileId is present in the winners or losers
                    bool isInWinningTeam = winProfiles.Any();
                    bool isInLosingTeam = loseProfiles.Any();

                    // If the profile is in either the winning or losing list, add the game to the profile's game history
                    if (isInWinningTeam || isInLosingTeam)
                    {
                        // Populate WinnersList
                        game.WinnersList = winProfiles
                            .Select(profile => _context.Profile.FirstOrDefault(p => p.ProfileId == profile.ProfileId))
                            .Where(profile => profile != null)
                            .Select(profile => new Profile
                            {
                                ProfileId = profile.ProfileId,
                                UserId = profile.UserId,
                                UserName = profile.UserName,
                                Height = profile.Height,
                                Weight = profile.Weight,
                                Position = profile.Position,
                                Ranking = profile.Ranking,
                                StarRating = profile.StarRating,
                                QRCode = profile.QRCode,
                                Bio = profile.Bio,
                                ImageURL = profile.ImageURL,
                                PlayerArchetype = profile.PlayerArchetype,
                                City = profile.City,
                                PlayerNumber = profile.PlayerNumber,
                                Points = profile.Points,
                                WinOrLose = "W"
                            }).ToList();

                        // Populate LossersList
                        game.LossersList = loseProfiles
                            .Select(profile => _context.Profile.FirstOrDefault(p => p.ProfileId == profile.ProfileId))
                            .Where(profile => profile != null)
                            .Select(profile => new Profile
                            {
                                ProfileId = profile.ProfileId,
                                UserId = profile.UserId,
                                UserName = profile.UserName,
                                Height = profile.Height,
                                Weight = profile.Weight,
                                Position = profile.Position,
                                Ranking = profile.Ranking,
                                StarRating = profile.StarRating,
                                QRCode = profile.QRCode,
                                Bio = profile.Bio,
                                ImageURL = profile.ImageURL,
                                PlayerArchetype = profile.PlayerArchetype,
                                City = profile.City,
                                PlayerNumber = profile.PlayerNumber,
                                Points = profile.Points,
                                WinOrLose = "L"
                            }).ToList();

                        // Add the game to the result list if it has winners or losers for the profile
                        if (game.WinnersList.Any() || game.LossersList.Any())
                        {
                            profileGames.Add(game);
                        }
                    }
                }
                // Return an empty list if no games found; otherwise, return the list
                return profileGames.Any() ? profileGames : new List<Game>();
            }
            

           
        }

        /// <summary>
        /// Get Games By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        public async Task<List<Game>> GetGamesByProfileId(string profileId)
        {
            using (var context = _context)
            {
                try
                {
                    // Fetch all games
                    var games = await context.Game.ToListAsync();

                    // Filter games by checking if the ProfileId exists in win or lose JSON strings
                    var filteredGames = new List<Game>();

                    foreach (var game in games)
                    {
                        // Deserialize the JSON strings
                        var profileStatuses = DeserializeProfileStatuses(game.WinProfileIdsStatusString);

                        // Check if ProfileId exists in the winner or loser lists
                        bool isProfileInWinList = profileStatuses.Any(p => p.ProfileId == profileId && p.Status == "W");
                        bool isProfileInLoseList = profileStatuses.Any(p => p.ProfileId == profileId && p.Status == "L");

                        // If ProfileId is not in either list, skip this game
                        if (!isProfileInWinList && !isProfileInLoseList)
                        {
                            continue;
                        }

                        // Determine UserWinOrLose
                        game.UserWinOrLose = isProfileInWinList ? "W" : "L";

                        // Fetch profiles for winners and losers
                        var winProfileIds = profileStatuses
                            .Where(p => p.Status == "W")
                            .Select(p => p.ProfileId)
                            .ToList();

                        var loseProfileIds = profileStatuses
                            .Where(p => p.Status == "L")
                            .Select(p => p.ProfileId)
                            .ToList();

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

                        // Add game to the filtered list
                        filteredGames.Add(game);
                    }

                    return filteredGames;
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
