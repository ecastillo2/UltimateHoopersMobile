using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.DAL
{
    /// <summary>
    /// Interface for PrivateRun repository operations with consistent cancellation token support
    /// </summary>
    public interface IPrivateRunRepository : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Get all PrivateRuns
        /// </summary>
        Task<List<PrivateRun>> GetPrivateRunsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get PrivateRuns with offset-based pagination
        /// </summary>
        Task<(List<PrivateRun> PrivateRuns, int TotalCount, int TotalPages)> GetPrivateRunsPaginatedAsync(
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get PrivateRuns with cursor-based pagination for efficient scrolling
        /// </summary>
        /// <param name="cursor">The cursor value from the last item in the previous page, null for first page</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="direction">Direction of scrolling: "next" or "previous"</param>
        /// <param name="sortBy">Field to sort by (e.g., "Points", "PlayerNumber", "Status")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Courts and the next cursor value</returns>
        Task<(List<PrivateRun> PrivateRuns, string NextCursor)> GetPrivateRunsWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Stream all PrivateRuns for efficient memory usage with large datasets
        /// </summary>
        IAsyncEnumerable<PrivateRun> StreamAllPrivateRunsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default);


        /// <summary>
        /// Get PrivateRun by ID
        /// </summary>
        Task<PrivateRun> GetPrivateRunByIdAsync(
            string privateRunId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a profile
        /// </summary>
        Task<bool> UpdatePrivateRunAsync(
            PrivateRun privateRun,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Batch update multiple PrivateRun at once
        /// </summary>
        Task<int> BatchUpdatePrivateRunsAsync(
            IEnumerable<PrivateRun> PrivateRuns,
            CancellationToken cancellationToken = default);


        /// <summary>
        /// Save changes to database
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}