using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.DAL.Interface
{
    /// <summary>
    /// Interface for Court repository operations with consistent cancellation token support
    /// </summary>
    public interface IReportRepository : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Get all Courts
        /// </summary>
        Task<List<Court>> GetCourtsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Courts with offset-based pagination
        /// </summary>
        Task<(List<Court> Courts, int TotalCount, int TotalPages)> GetCourtsPaginatedAsync(
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Courts with cursor-based pagination for efficient scrolling
        /// </summary>
        /// <param name="cursor">The cursor value from the last item in the previous page, null for first page</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="direction">Direction of scrolling: "next" or "previous"</param>
        /// <param name="sortBy">Field to sort by (e.g., "Points", "PlayerNumber", "Status")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Courts and the next cursor value</returns>
        Task<(List<Court> Courts, string NextCursor)> GetCourtsWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Stream all Courts for efficient memory usage with large datasets
        /// </summary>
        IAsyncEnumerable<Court> StreamAllCourtsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default);


        /// <summary>
        /// Get Court by ID
        /// </summary>
        Task<Court> GetCourtByIdAsync(
            string courtId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a profile
        /// </summary>
        Task<bool> UpdateCourtAsync(
            Court court,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Batch update multiple profiles at once
        /// </summary>
        Task<int> BatchUpdateCourtsAsync(
            IEnumerable<Court> Courts,
            CancellationToken cancellationToken = default);

       
        /// <summary>
        /// Save changes to database
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}