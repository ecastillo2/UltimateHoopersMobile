using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.Context;
using DataLayer.DAL.Context;
using DataLayer.DAL.Interface;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataLayer.DAL.Repository
{
    /// <summary>
    /// Implementation of the profile repository with optimized query methods
    /// </summary>
    public class ProfileRepository : IProfileRepository
    {
        private readonly HUDBContext _context;
        private readonly ILogger<ProfileRepository> _logger;
        private readonly IConfiguration _configuration;
        private bool _disposed = false;

        public ProfileRepository(HUDBContext context, IConfiguration configuration, ILogger<ProfileRepository> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

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

        public async Task<(List<Profile> Profiles, int TotalCount, int TotalPages)> GetProfilesPaginatedAsync(
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
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

        public async Task<(List<Profile> Profiles, string NextCursor)> GetProfilesWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            CancellationToken cancellationToken = default)
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

        public async IAsyncEnumerable<Profile> StreamAllProfilesAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
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

        public async Task<List<Profile>> GetFollowingProfilesAsync(
            string profileId,
            CancellationToken cancellationToken = default)
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

        public async Task<List<Profile>> GetFollowerProfilesAsync(
            string profileId,
            CancellationToken cancellationToken = default)
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

        public async Task<Profile> GetProfileByIdAsync(
            string profileId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Profile
                    .AsNoTracking()
                    .Include(p => p.Setting)
                    .FirstOrDefaultAsync(p => p.ProfileId == profileId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting profile {ProfileId}", profileId);
                throw;
            }
        }

        public async Task<Profile> GetProfileByUserIdAsync(
            string userId,
            CancellationToken cancellationToken = default)
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

        public async Task<Profile> GetProfileByUsernameAsync(
            string username,
            CancellationToken cancellationToken = default)
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

        public async Task<Profile> GetProfileWithTimeoutAsync(
            string profileId,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
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

        public async Task<List<Game>> GetProfileGameHistoryAsync(
            string profileId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Find all games where this profile participated
                // This is more complex as we need to check both winner and loser strings
                var query = _context.Game
                    .AsNoTracking()
                    .Where(g =>
                        g.WinProfileIdsStatusString.Contains(profileId) ||
                        g.LoseProfileIdsStatusString.Contains(profileId))
                    .OrderByDescending(g => g.CreatedDate);

                return await query.ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting game history for profile {ProfileId}", profileId);
                throw;
            }
        }

        public async Task<GameStatistics> GetProfileGameStatisticsAsync(
            string profileId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var games = await GetProfileGameHistoryAsync(profileId, cancellationToken);

                // Add null checks for WinProfileIdsStatusString and LoseProfileIdsStatusString
                int wins = games.Count(g => g.WinProfileIdsStatusString != null &&
                                            g.WinProfileIdsStatusString.Contains(profileId));

                int losses = games.Count(g => g.LoseProfileIdsStatusString != null &&
                                              g.LoseProfileIdsStatusString.Contains(profileId));

                return new GameStatistics
                {
                    TotalGames = games.Count,
                    WinPercentage = games.Count > 0 ? (double)wins / games.Count * 100 : 0,
                    TotalWins = wins.ToString(),
                    TotalLosses = losses.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating game statistics for profile {ProfileId}", profileId);
                throw;
            }
        }

        public async Task<ScoutingReport> GetScoutingReportAsync(
            string profileId,
            CancellationToken cancellationToken = default)
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

        public async Task<Squad> GetProfileSquadAsync(
            string profileId,
            CancellationToken cancellationToken = default)
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

        public async Task<Setting> GetProfileSettingsAsync(
            string profileId,
            CancellationToken cancellationToken = default)
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

        public async Task<bool> UpdateProfileAsync(
            Profile profile,
            CancellationToken cancellationToken = default)
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

        public async Task<int> BatchUpdateProfilesAsync(
            IEnumerable<Profile> profiles,
            CancellationToken cancellationToken = default)
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

        public async Task<bool> UpdateProfilePointsAsync(
            string profileId,
            int points,
            CancellationToken cancellationToken = default)
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

        public async Task<bool> SetTopRecordStatusAsync(
            string profileId,
            bool hasTopRecord,
            CancellationToken cancellationToken = default)
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

        public async Task<bool> UpdateLastRunDateAsync(
            string profileId,
            string lastRunDate,
            CancellationToken cancellationToken = default)
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

        public async Task<bool> UpdateProfileUserNameAsync(
            string profileId,
            string newUserName,
            CancellationToken cancellationToken = default)
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

        public async Task<bool> UpdateSettingAsync(
            Setting setting,
            CancellationToken cancellationToken = default)
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

        public async Task<bool> UpsertScoutingReportAsync(
            ScoutingReport scoutingReport,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var existingReport = await _context.ScoutingReport
                    .FirstOrDefaultAsync(s => s.ProfileId == scoutingReport.ProfileId, cancellationToken);

                if (existingReport != null)
                {
                    // Update existing report
                    existingReport.PlayStyle = scoutingReport.PlayStyle;
                    existingReport.StrengthOne = scoutingReport.StrengthOne;
                    existingReport.StrengthTwo = scoutingReport.StrengthTwo;
                    existingReport.WeaknessOne = scoutingReport.WeaknessOne;
                    existingReport.WeaknessTwo = scoutingReport.WeaknessTwo;
                    existingReport.PlayStyleImpactOne = scoutingReport.PlayStyleImpactOne;
                    existingReport.PlayStyleImpactTwo = scoutingReport.PlayStyleImpactTwo;
                    existingReport.Comparison = scoutingReport.Comparison;
                    existingReport.Conclusion = scoutingReport.Conclusion;
                    existingReport.IdealRole = scoutingReport.IdealRole;
                    existingReport.Status = scoutingReport.Status;
                    existingReport.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    // Add new report
                    scoutingReport.CreatedDate = DateTime.UtcNow;
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

        public async Task<bool> IsUserNameAvailableAsync(
            string userName,
            CancellationToken cancellationToken = default)
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