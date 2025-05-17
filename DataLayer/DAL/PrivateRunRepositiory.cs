using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataLayer.DAL
{
    /// <summary>
    /// Implementation of the PrivateRun repository with optimized query methods
    /// </summary>
    public class PrivateRunRepository : IPrivateRunRepository
    {
        private readonly HUDBContext _context;
        private readonly ILogger<PrivateRunRepository> _logger;
        private readonly IConfiguration _configuration;
        private bool _disposed = false;

        public PrivateRunRepository(HUDBContext context, IConfiguration configuration, ILogger<PrivateRunRepository> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<List<PrivateRun>> GetPrivateRunsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.PrivateRun
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving PrivateRuns");
                throw;
            }
        }

        public async Task<(List<PrivateRun> PrivateRuns, int TotalCount, int TotalPages)> GetPrivateRunsPaginatedAsync(
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            try
            {
                var totalCount = await _context.PrivateRun.CountAsync(cancellationToken);
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var privateRuns = await _context.PrivateRun
                    .AsNoTracking()

                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return (privateRuns, totalCount, totalPages);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving paginated PrivateRuns");
                throw;
            }
        }

        public async Task<(List<PrivateRun> PrivateRuns, string NextCursor)> GetPrivateRunsWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Default query starting point
                IQueryable<PrivateRun> query = _context.PrivateRun.AsNoTracking();

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
                    var newCursorData = new PrivateRunCursorData
                    {
                        Id = lastItem.PrivateRunId,
                        
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
                _logger?.LogError(ex, "Error getting PrivateRuns with cursor");
                throw;
            }
        }

        public async IAsyncEnumerable<PrivateRun> StreamAllPrivateRunsAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var batchSize = 100;
            var lastId = string.Empty;

            while (true)
            {
                List<PrivateRun> batch;
                try
                {
                    batch = await _context.PrivateRun
                        .AsNoTracking()
                        .Where(p => string.Compare(p.PrivateRunId, lastId) > 0)
                        .OrderBy(p => p.PrivateRunId)
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
                    lastId = privateRun.PrivateRunId;
                }

                if (batch.Count < batchSize)
                    break;
            }
        }


        public async Task<PrivateRun> GetPrivateRunByIdAsync(
            string privateRunId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.PrivateRun
                    .AsNoTracking()

                    .FirstOrDefaultAsync(p => p.PrivateRunId == privateRunId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting PrivateRun {PrivateRunId}", privateRunId);
                throw;
            }
        }




        public async Task<bool> UpdatePrivateRunAsync(
            PrivateRun privateRun,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _context.Entry(privateRun).State = EntityState.Modified;
                return await SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating PrivateRun {PrivateRunId}", privateRun.PrivateRunId);
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

        public Task<int> BatchUpdatePrivateRunsAsync(IEnumerable<PrivateRun> PrivateRuns, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// Helper class for cursor-based pagination
    /// </summary>
    internal class PrivateRunCursorData
    {
        public string Id { get; set; }
        public string Zip { get; set; }
        public string Status { get; set; }
    }
}