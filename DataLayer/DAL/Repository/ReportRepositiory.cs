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
    /// Implementation of the Court repository with optimized query methods
    /// </summary>
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<ReportRepository> _logger;
        private readonly IConfiguration _configuration;
        private bool _disposed = false;

        public ReportRepository(ApplicationContext context, IConfiguration configuration, ILogger<ReportRepository> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<List<Court>> GetCountsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Court
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving Courts");
                throw;
            }
        }

        public async Task<(List<Court> Courts, int TotalCount, int TotalPages)> GetCourtsPaginatedAsync(
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            try
            {
                var totalCount = await _context.Court.CountAsync(cancellationToken);
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var courts = await _context.Court
                    .AsNoTracking()
                    
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return (courts, totalCount, totalPages);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving paginated Courts");
                throw;
            }
        }

        public async Task<(List<Court> Courts, string NextCursor)> GetCourtsWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Default query starting point
                IQueryable<Court> query = _context.Court.AsNoTracking();

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
                var courts = await query.Take(limit + 1).ToListAsync(cancellationToken);

                // Check if we have a next page by fetching limit+1 items
                string nextCursor = null;
                if (courts.Count > limit)
                {
                    // Remove the extra item we retrieved to check for "has next page"
                    var lastItem = courts[limit];
                    courts.RemoveAt(limit);

                    // Create cursor for next page based on last item properties
                    var newCursorData = new CourtCursorData
                    {
                        Id = lastItem.CourtId,
                        Zip = lastItem.Zip,
                        Status = lastItem.Status
                    };

                    var serialized = System.Text.Json.JsonSerializer.Serialize(newCursorData);
                    nextCursor = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(serialized));
                }

                // If we requested previous direction and got results, we need to reverse the order
                if (direction.ToLowerInvariant() == "previous" && courts.Any())
                {
                    courts.Reverse();
                }

                return (courts, nextCursor);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting Courts with cursor");
                throw;
            }
        }

        public async IAsyncEnumerable<Court> StreamAllCourtsAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var batchSize = 100;
            var lastId = string.Empty;

            while (true)
            {
                List<Court> batch;
                try
                {
                    batch = await _context.Court
                        .AsNoTracking()
                        .Where(p => string.Compare(p.CourtId, lastId) > 0)
                        .OrderBy(p => p.CourtId)
                        .Take(batchSize)
                        .ToListAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error streaming Courts");
                    throw;
                }

                if (batch.Count == 0)
                    break;

                foreach (var court in batch)
                {
                    yield return court;
                    lastId = court.CourtId;
                }

                if (batch.Count < batchSize)
                    break;
            }
        }

       
        public async Task<Court> GetCourtByIdAsync(
            string courtId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Court
                    .AsNoTracking()
                   
                    .FirstOrDefaultAsync(p => p.CourtId == courtId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting Court {CourtId}", courtId);
                throw;
            }
        }




        public async Task<bool> UpdateCourtAsync(
            Court court,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _context.Entry(court).State = EntityState.Modified;
                return await SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating Court {CourtId}", court.CourtId);
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

        public Task<int> BatchUpdateCourtsAsync(IEnumerable<Court> Courts, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<Court>> GetCourtsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// Helper class for cursor-based pagination
    /// </summary>
    internal class ReportCursorData
    {
        public string Id { get; set; }
        public string Zip { get; set; }
        public string Status { get; set; }
    }
}