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
    /// Implementation of the PrivateRun repository with optimized query methods
    /// </summary>
    public class RunRepository : IRunRepository
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<RunRepository> _logger;
        private readonly IConfiguration _configuration;
        private bool _disposed = false;

        public RunRepository(ApplicationContext context, IConfiguration configuration, ILogger<RunRepository> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<List<Run>> GetRunsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Run
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving Runs");
                throw;
            }
        }

        public async Task<(List<Run> Runs, int TotalCount, int TotalPages)> GetRunsPaginatedAsync(
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            try
            {
                var totalCount = await _context.Run.CountAsync(cancellationToken);
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var privateRuns = await _context.Run
                    .AsNoTracking()

                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return (privateRuns, totalCount, totalPages);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving paginated Runs");
                throw;
            }
        }

        public async Task<(List<Run> Runs, string NextCursor)> GetRunsWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Default query starting point
                IQueryable<Run> query = _context.Run.AsNoTracking();

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


                // Execute query with limit
                var privateRuns = await query.Take(limit + 1).ToListAsync(cancellationToken);

                // Check if we have a next page by fetching limit+1 items
                string nextCursor = null;
                if (privateRuns.Count > limit)
                {
                    // Remove the extra item we retrieved to check for "has next page"
                    var lastItem = privateRuns[limit];
                    privateRuns.RemoveAt(limit);

                    // Create cursor for next page based on last item properties
                    var newCursorData = new RunCursorData
                    {
                        Id = lastItem.RunId,
                        
                        Status = lastItem.Status
                    };

                    var serialized = System.Text.Json.JsonSerializer.Serialize(newCursorData);
                    nextCursor = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(serialized));
                }

                // If we requested previous direction and got results, we need to reverse the order
                if (direction.ToLowerInvariant() == "previous" && privateRuns.Any())
                {
                    privateRuns.Reverse();
                }

                return (privateRuns, nextCursor);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting Runs with cursor");
                throw;
            }
        }

        public async IAsyncEnumerable<Run> StreamAllRunsAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var batchSize = 100;
            var lastId = string.Empty;

            while (true)
            {
                List<Run> batch;
                try
                {
                    batch = await _context.Run
                        .AsNoTracking()
                        .Where(p => string.Compare(p.RunId, lastId) > 0)
                        .OrderBy(p => p.RunId)
                        .Take(batchSize)
                        .ToListAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error streaming PrivateRuns");
                    throw;
                }

                if (batch.Count == 0)
                    break;

                foreach (var privateRun in batch)
                {
                    yield return privateRun;
                    lastId = privateRun.RunId;
                }

                if (batch.Count < batchSize)
                    break;
            }
        }


        public async Task<Run> GetRunByIdAsync(
            string runId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Run
                    .AsNoTracking()

                    .FirstOrDefaultAsync(p => p.RunId == runId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting PrivateRun {PrivateRunId}", runId);
                throw;
            }
        }


        public async Task<Court> GetCourtAsync(
            string courtId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Court
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.CourtId == courtId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting scouting report for Court {CourtId}", courtId);
                throw;
            }
        }


        public async Task<List<Profile>> GetJoinedRunAsync(
      string privateRunId,
      CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await (from pri in _context.JoinedRun
                                    join p in _context.Profile on pri.ProfileId equals p.ProfileId
                                    where pri.RunId == privateRunId
                                    select p)  // Just select the Profile objects directly
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                return result;  // Since we're selecting Profile entities, just return them
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting profiles for PrivateRunInvite {PrivateRunInviteId}", privateRunId);
                throw;
            }
        }

        public async Task<bool> UpdateRunAsync(
            Run run,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _context.Entry(run).State = EntityState.Modified;
                return await SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating PrivateRun {PrivateRunId}", run.RunId);
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

        public Task<int> BatchUpdateRunsAsync(IEnumerable<Run> PrivateRuns, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<Profile>> GetRunInviteAsync(string privateRunId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// Helper class for cursor-based pagination
    /// </summary>
    internal class RunCursorData
    {
        public string Id { get; set; }
        public string Zip { get; set; }
        public string Status { get; set; }
    }
}