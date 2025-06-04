using DataLayer.Context;
using DataLayer.DAL.Context;
using DataLayer.DAL.Interface;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataLayer.DAL.Repository
{
    /// <summary>
    /// Implementation of the profile repository with optimized query methods
    /// </summary>
    public class ProfileRepository : IProfileRepository
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<ProfileRepository> _logger;
        private readonly IConfiguration _configuration;
        private bool _disposed = false;

        public ProfileRepository(ApplicationContext context, IConfiguration configuration, ILogger<ProfileRepository> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        /// <summary>
        /// Get Profiles Async
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<Profile>> GetProfilesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Profile
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving profiles");
                throw;
            }
        }

        /// <summary>
        /// Get Profiles Paginated Async
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<(List<Profile> Profiles, int TotalCount, int TotalPages)> GetProfilesPaginatedAsync(int page = 1,int pageSize = 20,CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            try
            {
                var totalCount = await _context.Profile.CountAsync(cancellationToken);
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var profiles = await _context.Profile
                    .AsNoTracking()
                    .OrderByDescending(p => p.Points)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return (profiles, totalCount, totalPages);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving paginated profiles");
                throw;
            }
        }

        /// <summary>
        /// Get Profiles With Cursor Async
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="limit"></param>
        /// <param name="direction"></param>
        /// <param name="sortBy"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<(List<Profile> Profiles, string NextCursor)> GetProfilesWithCursorAsync(string cursor = null,int limit = 20,string direction = "next",string sortBy = "Points",CancellationToken cancellationToken = default)
        {
            try
            {
                // Default query starting point
                IQueryable<Profile> query = _context.Profile.AsNoTracking();

                // Parse the cursor if provided
                CursorData cursorData = null;
                if (!string.IsNullOrEmpty(cursor))
                {
                    try
                    {
                        // Decode and deserialize cursor
                        var decodedCursor = System.Text.Encoding.UTF8.GetString(
                            Convert.FromBase64String(cursor));
                        cursorData = System.Text.Json.JsonSerializer.Deserialize<CursorData>(decodedCursor);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Invalid cursor format. Starting from beginning");
                        // If cursor parsing fails, ignore and start from beginning
                        cursorData = null;
                    }
                }

                // Apply filtering based on cursor and direction
                if (cursorData != null)
                {
                    switch (sortBy.ToLowerInvariant())
                    {
                        case "points":
                            if (direction.ToLowerInvariant() == "next")
                            {
                                query = query.Where(p => p.Points < cursorData.Points ||
                                    p.Points == cursorData.Points && p.ProfileId.CompareTo(cursorData.Id) < 0);
                                query = query.OrderByDescending(p => p.Points).ThenByDescending(p => p.ProfileId);
                            }
                            else // previous
                            {
                                query = query.Where(p => p.Points > cursorData.Points ||
                                    p.Points == cursorData.Points && p.ProfileId.CompareTo(cursorData.Id) > 0);
                                query = query.OrderBy(p => p.Points).ThenBy(p => p.ProfileId);
                            }
                            break;

                        case "playernumber":
                            if (direction.ToLowerInvariant() == "next")
                            {
                                query = query.Where(p => p.PlayerNumber.CompareTo(cursorData.PlayerNumber) > 0 ||
                                    p.PlayerNumber == cursorData.PlayerNumber && p.ProfileId.CompareTo(cursorData.Id) < 0);
                                query = query.OrderBy(p => p.PlayerNumber).ThenByDescending(p => p.ProfileId);
                            }
                            else // previous
                            {
                                query = query.Where(p => p.PlayerNumber.CompareTo(cursorData.PlayerNumber) < 0 ||
                                    p.PlayerNumber == cursorData.PlayerNumber && p.ProfileId.CompareTo(cursorData.Id) > 0);
                                query = query.OrderByDescending(p => p.PlayerNumber).ThenBy(p => p.ProfileId);
                            }
                            break;

                        case "username":
                            if (direction.ToLowerInvariant() == "next")
                            {
                                query = query.Where(p => p.UserName.CompareTo(cursorData.UserName) > 0 ||
                                    p.UserName == cursorData.UserName && p.ProfileId.CompareTo(cursorData.Id) < 0);
                                query = query.OrderBy(p => p.UserName).ThenByDescending(p => p.ProfileId);
                            }
                            else // previous
                            {
                                query = query.Where(p => p.UserName.CompareTo(cursorData.UserName) < 0 ||
                                    p.UserName == cursorData.UserName && p.ProfileId.CompareTo(cursorData.Id) > 0);
                                query = query.OrderByDescending(p => p.UserName).ThenBy(p => p.ProfileId);
                            }
                            break;

                        case "status":
                            if (direction.ToLowerInvariant() == "next")
                            {
                                query = query.Where(p => p.Status.CompareTo(cursorData.Status) > 0 ||
                                    p.Status == cursorData.Status && p.ProfileId.CompareTo(cursorData.Id) < 0);
                                query = query.OrderBy(p => p.Status).ThenByDescending(p => p.ProfileId);
                            }
                            else // previous
                            {
                                query = query.Where(p => p.Status.CompareTo(cursorData.Status) < 0 ||
                                    p.Status == cursorData.Status && p.ProfileId.CompareTo(cursorData.Id) > 0);
                                query = query.OrderByDescending(p => p.Status).ThenBy(p => p.ProfileId);
                            }
                            break;

                        default:
                            // Default to Points
                            if (direction.ToLowerInvariant() == "next")
                            {
                                query = query.Where(p => p.Points < cursorData.Points ||
                                    p.Points == cursorData.Points && p.ProfileId.CompareTo(cursorData.Id) < 0);
                                query = query.OrderByDescending(p => p.Points).ThenByDescending(p => p.ProfileId);
                            }
                            else // previous
                            {
                                query = query.Where(p => p.Points > cursorData.Points ||
                                    p.Points == cursorData.Points && p.ProfileId.CompareTo(cursorData.Id) > 0);
                                query = query.OrderBy(p => p.Points).ThenBy(p => p.ProfileId);
                            }
                            break;
                    }
                }
                else
                {
                    // For first page with no cursor
                    switch (sortBy.ToLowerInvariant())
                    {
                        case "points":
                            query = query.OrderByDescending(p => p.Points).ThenByDescending(p => p.ProfileId);
                            break;
                        case "playernumber":
                            query = query.OrderBy(p => p.PlayerNumber).ThenByDescending(p => p.ProfileId);
                            break;
                        case "username":
                            query = query.OrderBy(p => p.UserName).ThenByDescending(p => p.ProfileId);
                            break;
                        case "status":
                            query = query.OrderBy(p => p.Status).ThenByDescending(p => p.ProfileId);
                            break;
                        default:
                            query = query.OrderByDescending(p => p.Points).ThenByDescending(p => p.ProfileId);
                            break;
                    }
                }

                // Execute query with limit
                var profiles = await query.Take(limit + 1).ToListAsync(cancellationToken);

                // Check if we have a next page by fetching limit+1 items
                string nextCursor = null;
                if (profiles.Count > limit)
                {
                    // Remove the extra item we retrieved to check for "has next page"
                    var lastItem = profiles[limit];
                    profiles.RemoveAt(limit);

                    // Create cursor for next page based on last item properties
                    var newCursorData = new CursorData
                    {
                        Id = lastItem.ProfileId,
                        Points = lastItem.Points,
                        PlayerNumber = lastItem.PlayerNumber,
                        UserName = lastItem.UserName,
                        Status = lastItem.Status
                    };

                    var serialized = System.Text.Json.JsonSerializer.Serialize(newCursorData);
                    nextCursor = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(serialized));
                }

                // If we requested previous direction and got results, we need to reverse the order
                if (direction.ToLowerInvariant() == "previous" && profiles.Any())
                {
                    profiles.Reverse();
                }

                return (profiles, nextCursor);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting profiles with cursor");
                throw;
            }
        }

        /// <summary>
        /// Stream All Profiles Async
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<Profile> StreamAllProfilesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var batchSize = 100;
            var lastId = string.Empty;

            while (true)
            {
                List<Profile> batch;
                try
                {
                    batch = await _context.Profile
                        .AsNoTracking()
                        .Where(p => string.Compare(p.ProfileId, lastId) > 0)
                        .OrderBy(p => p.ProfileId)
                        .Take(batchSize)
                        .ToListAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error streaming profiles");
                    throw;
                }

                if (batch.Count == 0)
                    break;

                foreach (var profile in batch)
                {
                    yield return profile;
                    lastId = profile.ProfileId;
                }

                if (batch.Count < batchSize)
                    break;
            }
        }

        /// <summary>
        /// Get Following Profiles Async
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<Profile>> GetFollowingProfilesAsync(string profileId,CancellationToken cancellationToken = default)
        {
            try
            {
                var followingIds = await _context.Following
                    .AsNoTracking()
                    .Where(f => f.ProfileId == profileId)
                    .Select(f => f.FollowingProfileId)
                    .ToListAsync(cancellationToken);

                if (followingIds.Count == 0)
                    return new List<Profile>();

                return await _context.Profile
                    .AsNoTracking()
                    .Where(p => followingIds.Contains(p.ProfileId))
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting profiles that {ProfileId} is following", profileId);
                throw;
            }
        }

        /// <summary>
        /// Get Follower Profiles Async
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<Profile>> GetFollowerProfilesAsync(string profileId,CancellationToken cancellationToken = default)
        {
            try
            {
                var followerIds = await _context.Follower
                    .AsNoTracking()
                    .Where(f => f.ProfileId == profileId)
                    .Select(f => f.FollowerProfileId)
                    .ToListAsync(cancellationToken);

                if (followerIds.Count == 0)
                    return new List<Profile>();

                return await _context.Profile
                    .AsNoTracking()
                    .Where(p => followerIds.Contains(p.ProfileId))
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting profiles that follow {ProfileId}", profileId);
                throw;
            }
        }

        /// <summary>
        /// Get Profile By Id Async
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Profile> GetProfileByIdAsync(string profileId,CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _context.Profile
                    .AsNoTracking()
                    .Include(p => p.Setting)
                    .FirstOrDefaultAsync(p => p.ProfileId == profileId, cancellationToken);
                 

                result.TotalWins = await _context.GameWinningPlayer
                    .AsNoTracking()
                    .CountAsync(p => p.ProfileId == result.ProfileId, cancellationToken);

                result.TotalLosses = await _context.GameLosingPlayer
                    .AsNoTracking()
                    .CountAsync(p => p.ProfileId == result.ProfileId, cancellationToken);


                #region Follow/Following

                //Followers
                var followers = await _context.Follower
                    .AsNoTracking()
                    .Where(f => f.ProfileId == result.ProfileId) // who follows this user
                    .ToListAsync(cancellationToken);

                //Followings
                var followings = await _context.Following
                    .AsNoTracking()
                    .Where(f => f.FollowingProfileId == result.ProfileId) // who follows this user
                    .ToListAsync(cancellationToken);

                // Extract the follower IDs
                var followerIds = followers.Select(f => f.FollowerProfileId).ToList();

                // Extract the follower IDs
                var followingIds = followings.Select(f => f.ProfileId).ToList();

                // Get the matching profile records
                var followerProfiles = await _context.Profile
                    .AsNoTracking()
                    .Where(p => followerIds.Contains(p.ProfileId))
                    .ToListAsync(cancellationToken);

                // Get the matching profile records
                var followingProfiles = await _context.Profile
                    .AsNoTracking()
                    .Where(p => followingIds.Contains(p.ProfileId))
                    .ToListAsync(cancellationToken);

                result.FollowersList = followerProfiles;
                result.FollowingList = followingProfiles;

                result.FollowersCount = followerProfiles.Count();
                result.FollowingCount = followingProfiles.Count();
                #endregion

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting profile {ProfileId}", profileId);
                throw;
            }
        }

        /// <summary>
        /// Get Client By User Id Async
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Client> GetClientByUserIdAsync(string userId,CancellationToken cancellationToken = default)
        {
            try
            {
                // This approach uses a join operation to get the client in a single query
                return await _context.User
                    .AsNoTracking()
                    .Where(u => u.UserId == userId)
                    .Join(
                        _context.Client,
                        user => user.ClientId,
                        client => client.ClientId,
                        (user, client) => client
                    )
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting client for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get Courts By ClientId Async
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<Court>> GetCourtsByClientIdAsync(string clientId,CancellationToken cancellationToken = default)
        {
            try
            {
                // Using join to verify the client exists before returning courts
                // This will only return courts if the client exists
                return await _context.Client
                    .AsNoTracking()
                    .Where(c => c.ClientId == clientId)
                    .Join(
                        _context.Court,
                        client => client.ClientId,
                        court => court.ClientId,
                        (client, court) => court
                    )
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting courts for client {ClientId}", clientId);
                throw;
            }
        }

        /// <summary>
        /// Get Users By ClientId Async
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<User>> GetUsersByClientIdAsync(string clientId,CancellationToken cancellationToken = default)
        {
            try
            {
                // Using join to verify the client exists before returning courts
                // This will only return courts if the client exists
                return await _context.Client
                    .AsNoTracking()
                    .Where(c => c.ClientId == clientId)
                    .Join(
                        _context.User,
                        client => client.ClientId,
                        court => court.ClientId,
                        (client, court) => court
                    )
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting courts for client {ClientId}", clientId);
                throw;
            }
        }

        /// <summary>
        /// Get Profile By UserId Async
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Profile> GetProfileByUserIdAsync(string userId,CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Profile
                    .AsNoTracking()
                    .Include(p => p.Setting)
                    .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting profile by user ID {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get Profile By Username Async
        /// </summary>
        /// <param name="username"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Profile> GetProfileByUsernameAsync(string username,CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Profile
                    .AsNoTracking()
                    .Include(p => p.Setting)
                    .FirstOrDefaultAsync(p => p.UserName == username, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting profile by username {Username}", username);
                throw;
            }
        }

        /// <summary>
        /// Get Profile With TimeouAsync
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public async Task<Profile> GetProfileWithTimeoutAsync(string profileId,TimeSpan timeout,CancellationToken cancellationToken = default)
        {
            using var timeoutCts = new CancellationTokenSource(timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, timeoutCts.Token);

            try
            {
                return await _context.Profile
                    .AsNoTracking()
                    .Include(p => p.Setting)
                    .FirstOrDefaultAsync(p => p.ProfileId == profileId, linkedCts.Token);
            }
            catch (OperationCanceledException ex) when (timeoutCts.Token.IsCancellationRequested)
            {
                _logger?.LogWarning("Request to get profile {ProfileId} timed out after {Timeout}", profileId, timeout);
                throw new TimeoutException($"The operation timed out after {timeout}.", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting profile with timeout {ProfileId}", profileId);
                throw;
            }
        }

        /// <summary>
        /// Get Profile Game History Async
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<Game>> GetProfileGameHistoryAsync(string profileId,CancellationToken cancellationToken = default)
        {
            try
            {
                // First, find all games where the profile is either a winner or loser
                var winningGames = await _context.GameWinningPlayer
                    .AsNoTracking()
                    .Where(wp => wp.ProfileId == profileId)
                    .Select(wp => wp.GameId)
                    .ToListAsync(cancellationToken);

                var losingGames = await _context.GameLosingPlayer
                    .AsNoTracking()
                    .Where(lp => lp.ProfileId == profileId)
                    .Select(lp => lp.GameId)
                    .ToListAsync(cancellationToken);

                // Combine the IDs and get unique game IDs
                var gameIds = winningGames.Concat(losingGames).Distinct().ToList();

                if (!gameIds.Any())
                    return new List<Game>();

                // Fetch all the games
                var games = await _context.Game
                    .AsNoTracking()
                    .Where(g => gameIds.Contains(g.GameId))
                    .OrderByDescending(g => g.CreatedDate)
                    .ToListAsync(cancellationToken);

                // Now populate winners and losers for each game
                foreach (var game in games)
                {
                    // Get winning players
                    var winningPlayerIds = await _context.GameWinningPlayer
                        .AsNoTracking()
                        .Where(wp => wp.GameId == game.GameId)
                        .Select(wp => wp.ProfileId)
                        .ToListAsync(cancellationToken);

                    // Get losing players
                    var losingPlayerIds = await _context.GameLosingPlayer
                        .AsNoTracking()
                        .Where(lp => lp.GameId == game.GameId)
                        .Select(lp => lp.ProfileId)
                        .ToListAsync(cancellationToken);



                    // Fetch associated Run if available
                    if (!string.IsNullOrEmpty(game.RunId))
                    {
                        game.Run = await _context.Run
                            .AsNoTracking()
                            .FirstOrDefaultAsync(r => r.RunId == game.RunId, cancellationToken);
                    }
                }

                return games;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting game history for profile {ProfileId}", profileId);
                throw;
            }
        }

        /// <summary>
        /// Get Profile Game Statistics Async
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<GameStatistics> GetProfileGameStatisticsAsync(string profileId,CancellationToken cancellationToken = default)
        {
            try
            {
                // Count wins
                int wins = await _context.GameWinningPlayer
                    .AsNoTracking()
                    .CountAsync(wp => wp.ProfileId == profileId, cancellationToken);

                // Count losses
                int losses = await _context.GameLosingPlayer
                    .AsNoTracking()
                    .CountAsync(lp => lp.ProfileId == profileId, cancellationToken);

                // Calculate total games and win percentage
                int totalGames = wins + losses;
                double winPercentage = totalGames > 0 ? (double)wins / totalGames * 100 : 0;

                return new GameStatistics
                {
                    TotalGames = totalGames,
                    WinPercentage = winPercentage,
                    TotalWins = wins,
                    TotalLosses = losses
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating game statistics for profile {ProfileId}", profileId);
                throw;
            }
        }

        /// <summary>
        /// Get ScoutingReport Async
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ScoutingReport> GetScoutingReportAsync(string profileId,CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.ScoutingReport
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.ProfileId == profileId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting scouting report for profile {ProfileId}", profileId);
                throw;
            }
        }

        /// <summary>
        /// Get Scouting Report By Id Async
        /// </summary>
        /// <param name="scoutingReportId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ScoutingReport> GetScoutingReportByIdAsync(string scoutingReportId,CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.ScoutingReport
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.ScoutingReportId == scoutingReportId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting scouting report for ScoutingReport {ScoutingReportId}", scoutingReportId);
                throw;
            }
        }

        /// <summary>
        /// Update Client Async
        /// </summary>
        /// <param name="privateRun"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> UpdateScoutingReportAsync(ScoutingReport model,CancellationToken cancellationToken = default)
        {
            try
            {
                // Retrieve the existing product from database
                var existingProduct = await _context.ScoutingReport
                    .FirstOrDefaultAsync(p => p.ScoutingReportId == model.ScoutingReportId, cancellationToken);

                if (existingProduct == null)
                {
                    return false;
                }

                // Update only the fields you want to allow updates for
                existingProduct.PrimaryPosition = model.PrimaryPosition;
                existingProduct.SecondaryPosition = model.SecondaryPosition;
                existingProduct.PlayingStyle = model.PlayingStyle;
                existingProduct.Shooting = model.Shooting;
                existingProduct.BallHandling = model.BallHandling;
                existingProduct.Passing = model.Passing;
                existingProduct.Defense = model.Defense;
                existingProduct.Rebounding = model.Rebounding;
                existingProduct.Athleticism = model.Athleticism;
                existingProduct.Strengths = model.Strengths;
                existingProduct.AreasforImprovement = model.AreasforImprovement;
                existingProduct.AdditionalNotes = model.AdditionalNotes;
                existingProduct.LastUpdated = DateTime.UtcNow;

                // Don't update sensitive fields like CreatedDate, CreatedBy, etc.
                // existingProduct.CreatedDate = product.CreatedDate; // DON'T update

                return await SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating Product {ScoutingReportId}", model.ScoutingReportId);
                throw;
            }
        }

        public async Task<Squad> GetProfileSquadAsync(string profileId,CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if profile owns a squad
                var ownedSquad = await _context.Squad
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.OwnerProfileId == profileId, cancellationToken);

                if (ownedSquad != null)
                    return ownedSquad;

                // Check if profile is in a squad team
                var squadTeam = await _context.SquadTeam
                    .AsNoTracking()
                    .FirstOrDefaultAsync(st => st.ProfileId == profileId, cancellationToken);

                if (squadTeam == null)
                    return null;

                return await _context.Squad
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SquadId == squadTeam.SquadId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting squad for profile {ProfileId}", profileId);
                throw;
            }
        }

        public async Task<Setting> GetProfileSettingsAsync(string profileId,CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Setting
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.ProfileId == profileId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting settings for profile {ProfileId}", profileId);
                throw;
            }
        }

        public async Task<bool> UpdateProfileAsync(Profile profile,CancellationToken cancellationToken = default)
        {
            try
            {
                _context.Entry(profile).State = EntityState.Modified;
                return await SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating profile {ProfileId}", profile.ProfileId);
                throw;
            }
        }

        public async Task<int> BatchUpdateProfilesAsync(IEnumerable<Profile> profiles,CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var profile in profiles)
                {
                    _context.Entry(profile).State = EntityState.Modified;
                }

                return await SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error batch updating profiles");
                throw;
            }
        }

        public async Task<bool> UpdateProfilePointsAsync(string profileId,int points,CancellationToken cancellationToken = default)
        {
            try
            {
                var profile = await _context.Profile
                    .FindAsync(new object[] { profileId }, cancellationToken);

                if (profile == null)
                    return false;

                profile.Points += points;

                return await SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating points for profile {ProfileId}", profileId);
                throw;
            }
        }

        public async Task<bool> SetTopRecordStatusAsync(string profileId,bool hasTopRecord,CancellationToken cancellationToken = default)
        {
            try
            {
                var profile = await _context.Profile
                    .FindAsync(new object[] { profileId }, cancellationToken);

                if (profile == null)
                    return false;

                profile.TopRecord = hasTopRecord;

                return await SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error setting top record status for profile {ProfileId}", profileId);
                throw;
            }
        }

        public async Task<bool> UpdateLastRunDateAsync(string profileId,DateTime lastRunDate,CancellationToken cancellationToken = default)
        {
            try
            {
                var profile = await _context.Profile
                    .FindAsync(new object[] { profileId }, cancellationToken);

                if (profile == null)
                    return false;

                profile.LastRunDate = lastRunDate;

                return await SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating last run date for profile {ProfileId}", profileId);
                throw;
            }
        }

        public async Task<bool> UpdateProfileUserNameAsync(string profileId,string newUserName,CancellationToken cancellationToken = default)
        {
            try
            {
                // First check if the username is available
                bool isAvailable = await IsUserNameAvailableAsync(newUserName, cancellationToken);
                if (!isAvailable)
                    return false;

                var profile = await _context.Profile
                    .FindAsync(new object[] { profileId }, cancellationToken);

                if (profile == null)
                    return false;

                profile.UserName = newUserName;

                return await SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating username for profile {ProfileId}", profileId);
                throw;
            }
        }

        public async Task<bool> UpdateSettingAsync(Setting setting,CancellationToken cancellationToken = default)
        {
            try
            {
                _context.Entry(setting).State = EntityState.Modified;

                return await SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating settings for profile {ProfileId}", setting.ProfileId);
                throw;
            }
        }

        public async Task<bool> UpsertScoutingReportAsync(ScoutingReport scoutingReport,CancellationToken cancellationToken = default)
        {
            try
            {
                var existingReport = await _context.ScoutingReport
                    .FirstOrDefaultAsync(s => s.ProfileId == scoutingReport.ProfileId, cancellationToken);

                if (existingReport != null)
                {
                    // Update existing report
                    existingReport.PrimaryPosition = scoutingReport.PrimaryPosition;
                    existingReport.SecondaryPosition = scoutingReport.SecondaryPosition;
                    existingReport.PlayingStyle = scoutingReport.PlayingStyle;
                    existingReport.Shooting = scoutingReport.Shooting;
                    existingReport.BallHandling = scoutingReport.BallHandling;
                    existingReport.Passing = scoutingReport.Passing;
                    existingReport.Defense = scoutingReport.Defense;
                    existingReport.Rebounding = scoutingReport.Rebounding;
                    existingReport.Athleticism = scoutingReport.Athleticism;
                    existingReport.Strengths = scoutingReport.Strengths;
                    existingReport.AreasforImprovement = scoutingReport.AreasforImprovement;
                    existingReport.AdditionalNotes = scoutingReport.AdditionalNotes;
                    existingReport.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    // Add new report
              
                    scoutingReport.LastUpdated = DateTime.UtcNow;
                    await _context.ScoutingReport.AddAsync(scoutingReport, cancellationToken);
                }

                return await SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error upserting scouting report for profile {ProfileId}", scoutingReport.ProfileId);
                throw;
            }
        }

        public async Task<bool> IsUserNameAvailableAsync(string userName,CancellationToken cancellationToken = default)
        {
            try
            {
                return !await _context.Profile
                    .AnyAsync(p => p.UserName.ToLower() == userName.ToLower(), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking username availability for {Username}", userName);
                throw;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving changes to database");
                throw;
            }
        }

        #region IDisposable and IAsyncDisposable Implementation

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await _context.DisposeAsync();
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }

    /// <summary>
    /// Helper class for cursor-based pagination
    /// </summary>
    internal class CursorData
    {
        public string Id { get; set; }
        public int? Points { get; set; }
        public string PlayerNumber { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
    }
}