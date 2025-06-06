using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.DAL.Interface
{
    /// <summary>
    /// Interface for PrivateRun repository operations with consistent cancellation token support
    /// </summary>
    public interface IRunRepository : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Get all PrivateRuns
        /// </summary>
        Task<List<Run>> GetRunsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get PrivateRuns with offset-based pagination
        /// </summary>
        Task<(List<Run> Runs, int TotalCount, int TotalPages)> GetRunsPaginatedAsync(int page = 1,int pageSize = 20,CancellationToken cancellationToken = default);

        /// <summary>
        /// Get PrivateRuns with cursor-based pagination for efficient scrolling
        /// </summary>
        /// <param name="cursor">The cursor value from the last item in the previous page, null for first page</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="direction">Direction of scrolling: "next" or "previous"</param>
        /// <param name="sortBy">Field to sort by (e.g., "Points", "PlayerNumber", "Status")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Courts and the next cursor value</returns>
        Task<(List<Run> Runs, string NextCursor)> GetRunsWithCursorAsync(string cursor = null,int limit = 20,string direction = "next",string sortBy = "Points",CancellationToken cancellationToken = default);

        /// <summary>
        /// Stream all PrivateRuns for efficient memory usage with large datasets
        /// </summary>
        IAsyncEnumerable<Run> StreamAllRunsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default);


        /// <summary>
        /// Get PrivateRun by ID
        /// </summary>
        Task<Run> GetRunByIdAsync(string privateRunId,CancellationToken cancellationToken = default);

        /// <summary>
        /// Get PrivateRun by ID
        /// </summary>
        Task<Court> GetCourtAsync(string courtId,CancellationToken cancellationToken = default);

        /// <summary>
        /// Get PrivateRun by ID
        /// </summary>
        Task<List<Profile>> GetJoinedRunAsync(string privateRunId,CancellationToken cancellationToken = default);

        /// <summary>
        /// Get PrivateRun by ID
        /// </summary>
        Task<List<Request>> GetRunRequestsAsync(string RunId,CancellationToken cancellationToken = default);

        /// <summary>
        /// Update a profile
        /// </summary>
        Task<bool> UpdateRunAsync(Run privateRun,CancellationToken cancellationToken = default);

        /// <summary>
        /// Batch update multiple Run at once
        /// </summary>
        Task<int> BatchUpdateRunsAsync(IEnumerable<Run> PrivateRuns,CancellationToken cancellationToken = default);


        /// <summary>
        /// Save changes to database
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}