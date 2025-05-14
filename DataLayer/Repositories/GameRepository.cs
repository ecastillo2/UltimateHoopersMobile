using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Common;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for Game entity operations
    /// </summary>
    public class GameRepository : GenericRepository<Game>, IGameRepository
    {
        public GameRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get game by ID with all related data
        /// </summary>
        public override async Task<Game> GetByIdAsync(object id)
        {
            string gameId = id.ToString();
            var game = await _dbSet
                .FirstOrDefaultAsync(g => g.GameId == gameId);

            if (game == null)
                return null;

            // Load player lists (winners and losers)
            if (!string.IsNullOrEmpty(game.WinProfileIdsStatusString))
            {
                game.WinnersList = await LoadProfilesFromIdsAsync(game.WinProfileIdsStatusString);
            }

            if (!string.IsNullOrEmpty(game.LoseProfileIdsStatusString))
            {
                game.LossersList = await LoadProfilesFromIdsAsync(game.LoseProfileIdsStatusString);
            }

            // Load private run information
            if (!string.IsNullOrEmpty(game.PrivateRunId))
            {
                game.PrivateRun = await _context.PrivateRun
                    .FirstOrDefaultAsync(pr => pr.PrivateRunId == game.PrivateRunId);
            }

            // Load court information
            if (!string.IsNullOrEmpty(game.CourtId))
            {
                game.Court = await _context.Courts
                    .FirstOrDefaultAsync(c => c.CourtId == game.CourtId);
            }

            return game;
        }

        /// <summary>
        /// Get all games with related data
        /// </summary>
        public override async Task<List<Game>> GetAllAsync()
        {
            var games = await _dbSet.ToListAsync();

            // Get all game IDs
            var gameIds = games.Select(g => g.GameId).ToList();

            // Load related data for all games at once
            var privateRunIds = games.Where(g => !string.IsNullOrEmpty(g.PrivateRunId))
                                     .Select(g => g.PrivateRunId)
                                     .Distinct()
                                     .ToList();

            var courtIds = games.Where(g => !string.IsNullOrEmpty(g.CourtId))
                                .Select(g => g.CourtId)
                                .Distinct()
                                .ToList();

            // Fetch related data in a single query for each type
            var privateRuns = await _context.PrivateRun
                .Where(pr => privateRunIds.Contains(pr.PrivateRunId))
                .ToDictionaryAsync(pr => pr.PrivateRunId);

            var courts = await _context.Courts
                .Where(c => courtIds.Contains(c.CourtId))
                .ToDictionaryAsync(c => c.CourtId);

            // Apply related data to games
            foreach (var game in games)
            {
                // Set private run
                if (!string.IsNullOrEmpty(game.PrivateRunId) && privateRuns.TryGetValue(game.PrivateRunId, out var privateRun))
                {
                    game.PrivateRun = privateRun;
                }

                // Set court
                if (!string.IsNullOrEmpty(game.CourtId) && courts.TryGetValue(game.CourtId, out var court))
                {
                    game.Court = court;
                }
            }

            return games;
        }

        /// <summary>
        /// Get games by profile ID
        /// </summary>
        public async Task<List<Game>> GetGamesByProfileIdAsync(string profileId)
        {
            var games = await _dbSet
                .Where(g => g.WinProfileIdsStatusString.Contains(profileId) ||
                            g.LoseProfileIdsStatusString.Contains(profileId))
                .ToListAsync();

            // Load related data for games
            foreach (var game in games)
            {
                // Set user win/lose status
                if (game.WinProfileIdsStatusString.Contains(profileId))
                {
                    game.UserWinOrLose = "Win";
                }
                else if (game.LoseProfileIdsStatusString.Contains(profileId))
                {
                    game.UserWinOrLose = "Lose";
                }

                // Load private run information if available
                if (!string.IsNullOrEmpty(game.PrivateRunId))
                {
                    game.PrivateRun = await _context.PrivateRun
                        .FirstOrDefaultAsync(pr => pr.PrivateRunId == game.PrivateRunId);
                }
            }

            return games;
        }

        /// <summary>
        /// Insert game
        /// </summary>
        public override async Task AddAsync(Game game)
        {
            if (string.IsNullOrEmpty(game.GameId))
                game.GameId = Guid.NewGuid().ToString();

            game.CreatedDate = DateTime.Now.ToString();
            game.GameNumber = UniqueIdNumber.Get8Digits();

            await base.AddAsync(game);
        }

        /// <summary>
        /// Update game
        /// </summary>
        public async Task UpdateGameAsync(Game game)
        {
            var existingGame = await GetByIdAsync(game.GameId);
            if (existingGame == null)
                return;

            // Update properties
            existingGame.WinProfileIdsStatusString = game.WinProfileIdsStatusString;
            existingGame.LoseProfileIdsStatusString = game.LoseProfileIdsStatusString;
            existingGame.Status = game.Status;
            existingGame.Location = game.Location;
            existingGame.PrivateRunId = game.PrivateRunId;
            existingGame.CourtId = game.CourtId;

            _dbSet.Update(existingGame);
            await SaveAsync();
        }

        /// <summary>
        /// Helper method to load profiles from comma-separated ID list
        /// </summary>
        private async Task<List<Profile>> LoadProfilesFromIdsAsync(string idsString)
        {
            if (string.IsNullOrEmpty(idsString))
                return new List<Profile>();

            var ids = idsString.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (!ids.Any())
                return new List<Profile>();

            return await _context.Profile
                .Where(p => ids.Contains(p.ProfileId))
                .ToListAsync();
        }
    }

    /// <summary>
    /// Interface for Game repository
    /// </summary>
    public interface IGameRepository : IGenericRepository<Game>
    {
        Task<List<Game>> GetGamesByProfileIdAsync(string profileId);
        Task UpdateGameAsync(Game game);
    }
}