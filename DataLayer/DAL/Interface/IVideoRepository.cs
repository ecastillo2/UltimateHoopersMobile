using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.DAL.Interface
{
    /// <summary>
    /// Interface for Video repository operations with consistent cancellation token support
    /// </summary>
    public interface IVideoRepository : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Get all Videos
        /// </summary>
        Task<List<Video>> GetVideosAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Videos with offset-based pagination
        /// </summary>
        Task<(List<Video> Videos, int TotalCount, int TotalPages)> GetVideosPaginatedAsync(
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Videos with cursor-based pagination for efficient scrolling
        /// </summary>
        /// <param name="cursor">The cursor value from the last item in the previous page, null for first page</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="direction">Direction of scrolling: "next" or "previous"</param>
        /// <param name="sortBy">Field to sort by (e.g., "Points", "PlayerNumber", "Status")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Courts and the next cursor value</returns>
        Task<(List<Video> Videos, string NextCursor)> GetVideosWithCursorAsync(
            string cursor = null,
            int limit = 20,
            string direction = "next",
            string sortBy = "Points",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Stream all Videos for efficient memory usage with large datasets
        /// </summary>
        IAsyncEnumerable<Video> StreamAllVideosAsync([EnumeratorCancellation] CancellationToken cancellationToken = default);


        /// <summary>
        /// Get Video by ID
        /// </summary>
        Task<Video> GetVideoByIdAsync(string videoId,CancellationToken cancellationToken = default);

        /// <summary>
        /// Insert Video
        /// </summary>
        /// <param name="video"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task InsertVideo(Video video, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Video by ID
        /// </summary>
        Task<bool> DeleteVideoAsync(string videoId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update Video Async
        /// </summary>
        /// <param name="video"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> UpdateVideoAsync(Video video, CancellationToken cancellationToken = default);

        /// <summary>
        /// Batch update multiple Run at once
        /// </summary>
        Task<int> BatchUpdateVideosAsync(IEnumerable<Video> video, CancellationToken cancellationToken = default);


        /// <summary>
        /// Save changes to database
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}