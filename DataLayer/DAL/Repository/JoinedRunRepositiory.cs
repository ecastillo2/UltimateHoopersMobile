using DataLayer.Context;
using DataLayer.DAL.Interface;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataLayer.DAL.Repository
{
    /// <summary>
    /// Repository for JoinedRun operations
    /// </summary>
    public class JoinedRunRepository : IJoinedRunRepository
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<JoinedRunRepository> _logger;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the JoinedRunRepository class
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="logger">Logger</param>
        public JoinedRunRepository(ApplicationContext context, ILogger<JoinedRunRepository> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        /// <summary>
        /// GetJoinedRuns
        /// </summary>
        /// <returns></returns>
        public async Task<List<JoinedRun>> GetJoinedRuns()
        {
            try
            {
                return await _context.JoinedRun
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving joined runs");
                throw;
            }
        }

        /// <summary>
        /// Get joined run by ID
        /// </summary>
        public async Task<JoinedRun> GetJoinedRunById(string joinedRunId)
        {
            try
            {
                return await _context.JoinedRun
                    .AsNoTracking()
                    .FirstOrDefaultAsync(jr => jr.JoinedRunId == joinedRunId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving joined run {JoinedRunId}", joinedRunId);
                throw;
            }
        }

        /// <summary>
        /// Get JoinedRuns By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        public async Task<List<JoinedRun>> GetJoinedRunsByProfileId(string profileId)
        {
            try
            {
                var joinedRuns = await _context.JoinedRun
                    .AsNoTracking()
                    .Where(jr => jr.ProfileId == profileId)
                    .ToListAsync();

                // Enrich joined runs with run data
                foreach (var joinedRun in joinedRuns)
                {
                    // Get the associated run
                    var run = await _context.Run
                        .AsNoTracking()
                        .FirstOrDefaultAsync(r => r.RunId == joinedRun.RunId);

                    // Get associated court if available
                    if (run != null && !string.IsNullOrEmpty(run.CourtId))
                    {
                        var court = await _context.Court
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.CourtId == run.CourtId);

                        run.Court = court;
                    }
                }

                return joinedRuns;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving joined runs for profile {ProfileId}", profileId);
                throw;
            }
        }

        /// <summary>
        /// Get Joined Run Profiles By Run Id Async
        /// </summary>
        /// <param name="runId"></param>
        /// <returns></returns>
        public async Task<List<Profile>> GetJoinedRunProfilesByRunIdAsync(string runId)
        {
            try
            {
                var profiles = await _context.JoinedRun
                    .AsNoTracking()
                    .Where(jr => jr.RunId == runId)
                    .Join(_context.Profile,
                          jr => jr.ProfileId,
                          p => p.ProfileId,
                          (jr, p) => p)
                    .ToListAsync();

                return profiles;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving joined profiles for RunId {RunId}", runId);
                throw;
            }
        }

        /// <summary>
        /// Check if profile is already invited to run
        /// </summary>
        public async Task<bool> IsProfileIdIdAlreadyInvitedToRunInJoinedRuns(string profileId, string runId)
        {
            try
            {
                return await _context.JoinedRun
                    .AnyAsync(jr => jr.ProfileId == profileId && jr.RunId == runId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking if profile {ProfileId} is already invited to run {RunId}", profileId, runId);
                throw;
            }
        }

        /// <summary>
        /// Update player joined run status
        /// </summary>
        public async Task UpdatePlayerJoinedRun(string profileId, string joinedRunId, string status)
        {
            try
            {
                var joinedRun = await _context.JoinedRun
                    .FirstOrDefaultAsync(jr => jr.ProfileId == profileId && jr.JoinedRunId == joinedRunId);

                if (joinedRun == null)
                {
                    _logger?.LogWarning("Joined run not found for profile {ProfileId} and joined run {JoinedRunId}", profileId, joinedRunId);
                    return;
                }

                joinedRun.Status = status;
                _context.JoinedRun.Update(joinedRun);
                await Save();

                // Update associated order if it exists
                var order = await _context.Order
                    .FirstOrDefaultAsync(o => o.ProfileId == profileId && o.JoinedRunId == joinedRunId);

                if (order != null)
                {
                    // Update order status based on accepted invite status
                    if (status == "Accepted")
                    {
                        order.Status = "Completed";
                    }
                    else if (status == "Accepted / Pending")
                    {
                        order.Status = "Pending";
                    }
                    else if (status == "Refund")
                    {
                        order.Status = "Refund";
                    }

                    _context.Order.Update(order);
                    await Save();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating player joined run for profile {ProfileId} and joined run {JoinedRunId}", profileId, joinedRunId);
                throw;
            }
        }

        /// <summary>
        /// UpdatePlayerPresentJoinedRun
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="joinedRunId"></param>
        /// <param name="present"></param>
        /// <returns></returns>
        public async Task UpdatePlayerPresentJoinedRun(string profileId, string joinedRunId, bool present)
        {
            try
            {
                var joinedRun = await _context.JoinedRun
                    .FirstOrDefaultAsync(jr => jr.ProfileId == profileId && jr.JoinedRunId == joinedRunId);

                if (joinedRun == null)
                {
                    _logger?.LogWarning("Joined run not found for profile {ProfileId} and joined run {JoinedRunId}", profileId, joinedRunId);
                    return;
                }

                joinedRun.Present = present;
                _context.JoinedRun.Update(joinedRun);
                await Save();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating player present status for profile {ProfileId} and joined run {JoinedRunId}", profileId, joinedRunId);
                throw;
            }
        }

        /// <summary>
        /// Add Profile To Joined Run Async
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="runId"></param>
        /// <returns></returns>
        public async Task AddProfileToJoinedRunAsync(string profileId, string runId, string status)
        {
            try
            {

                JoinedRun joinedRun = new JoinedRun { 
                
                JoinedRunId = Guid.NewGuid().ToString(),
                    ProfileId = profileId,
                    RunId = runId,
                    Status = status,
                    Present = false,
                    InvitedDate = DateTime.UtcNow,
                };
              
              
                // Add to context and save
                await _context.JoinedRun.AddAsync(joinedRun);
                await Save();

                _logger?.LogInformation("Successfully inserted JoinedRun with ID: {JoinedRunId}", joinedRun.JoinedRunId);
            }
            catch (Exception ex)
            {
                
            }
        }

        /// <summary>
        /// Remove a profile from a run
        /// </summary>
        public async Task<bool> RemoveProfileJoinRunAsync(string profileId, string runId)
        {
            try
            {
                var joinedRun = await _context.JoinedRun
                    .FirstOrDefaultAsync(jr => jr.ProfileId == profileId && jr.RunId == runId);

                if (joinedRun == null)
                {
                    _logger?.LogWarning("Joined run not found for profile {ProfileId} and run {RunId}", profileId, runId);
                    return false;
                }

                _context.JoinedRun.Remove(joinedRun);
                await Save();
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error removing profile {ProfileId} from run {RunId}", profileId, runId);
                throw;
            }
        }

        /// <summary>
        /// Delete a joined run
        /// </summary>
        public async Task DeleteJoinedRun(string joinedRunId)
        {
            try
            {
                var joinedRun = await _context.JoinedRun
                    .FirstOrDefaultAsync(jr => jr.JoinedRunId == joinedRunId);

                if (joinedRun == null)
                {
                    _logger?.LogWarning("Joined run not found with ID {JoinedRunId}", joinedRunId);
                    return;
                }

                _context.JoinedRun.Remove(joinedRun);
                await Save();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting joined run {JoinedRunId}", joinedRunId);
                throw;
            }
        }

        /// <summary>
        /// Clear all joined runs for a specific run
        /// </summary>
        public async Task ClearJoinedRunByRun(string runId)
        {
            try
            {
                var joinedRuns = await _context.JoinedRun
                    .Where(jr => jr.RunId == runId)
                    .ToListAsync();

                if (joinedRuns.Any())
                {
                    _context.JoinedRun.RemoveRange(joinedRuns);
                    await Save();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error clearing joined runs for run {RunId}", runId);
                throw;
            }
        }

        /// <summary>
        /// Get joined runs with player counts for a run
        /// </summary>
        public async Task<(List<Profile> Profiles, int AcceptedCount, int UndecidedCount, int DeclinedCount)> GetJoinedRunsWithCountsByRunId(string runId)
        {
            try
            {
                var joinedRuns = await _context.JoinedRun
                    .AsNoTracking()
                    .Where(jr => jr.RunId == runId)
                    .ToListAsync();

                var profileIds = joinedRuns.Select(jr => jr.ProfileId).ToList();

                var profiles = await _context.Profile
                    .AsNoTracking()
                    .Where(p => profileIds.Contains(p.ProfileId))
                    .ToListAsync();

                // Match profiles with their joined run status
                foreach (var profile in profiles)
                {
                    var joinedRun = joinedRuns.FirstOrDefault(jr => jr.ProfileId == profile.ProfileId);
                    if (joinedRun != null)
                    {
                        profile.Status = joinedRun.Status;
                    }
                }

                // Count by status
                int acceptedCount = joinedRuns.Count(jr => jr.Status == "Accepted");
                int undecidedCount = joinedRuns.Count(jr => jr.Status == "Undecided" || string.IsNullOrEmpty(jr.Status));
                int declinedCount = joinedRuns.Count(jr => jr.Status == "Declined");

                return (profiles, acceptedCount, undecidedCount, declinedCount);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting joined runs with counts for run {RunId}", runId);
                throw;
            }
        }

        /// <summary>
        /// Save changes to the database
        /// </summary>
        public async Task<int> Save()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving changes to database");
                throw;
            }
        }

        /// <summary>
        /// Dispose of resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // No need to dispose _context here as it's injected and managed by DI container
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Dispose of resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// GetRunById
        /// </summary>
        /// <param name="runId"></param>
        /// <returns></returns>
        public async Task<Run> GetRunById(string runId)
        {
            try
            {
                return await _context.Run
                    .AsNoTracking()
                    .FirstOrDefaultAsync(jr => jr.RunId == runId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving joined run {JoinedRunId}", runId);
                throw;
            }
        }
    }

        /// <summary>
    /// Helper class for cursor-based pagination
    /// </summary>
    internal class JoinedRunCursorData
    {
        public string Id { get; set; }
        public string ProfileId { get; set; }
        public string RunId { get; set; }
    }
}