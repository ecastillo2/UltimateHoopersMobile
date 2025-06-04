using System.Runtime.CompilerServices;
using DataLayer.Context;
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
    public class VideoRepository : IVideoRepository
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<VideoRepository> _logger;
        private readonly IConfiguration _configuration;
        private bool _disposed = false;
        private readonly string _blobBaseUrl;

        public VideoRepository(ApplicationContext context, IConfiguration configuration, ILogger<VideoRepository> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _blobBaseUrl = configuration["BlobStorage:VideoBaseUrl"] ?? "https://ultimatehoopersapi.azurewebsites.net";
            _logger = logger;
        }

        /// <summary>
        /// Get Video Async
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<Video>> GetVideosAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Video
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving Runs");
                throw;
            }
        }

        /// <summary>
        /// Get Clients Paginated Async
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<(List<Video> Videos, int TotalCount, int TotalPages)> GetVideosPaginatedAsync(int page = 1,int pageSize = 20,CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            try
            {
                var totalCount = await _context.Video.CountAsync(cancellationToken);
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var products = await _context.Video
                    .AsNoTracking()

                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return (products, totalCount, totalPages);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving paginated Clients");
                throw;
            }
        }

        /// <summary>
        /// Get Products With CursorAsync
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="limit"></param>
        /// <param name="direction"></param>
        /// <param name="sortBy"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<(List<Video> Videos, string NextCursor)> GetVideosWithCursorAsync(string cursor = null,int limit = 20,string direction = "next",string sortBy = "Points",CancellationToken cancellationToken = default)
        {
            try
            {
                // Default query starting point
                IQueryable<Video> query = _context.Video.AsNoTracking();

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
                    var newCursorData = new VideoCursorData
                    {
                        Id = lastItem.VideoId,


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

        /// <summary>
        /// Get Product By Id Async
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Video> GetVideoByIdAsync(string videoId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Video
                    .AsNoTracking()

                    .FirstOrDefaultAsync(p => p.VideoId == videoId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting Video {VideoId}", videoId);
                throw;
            }
        }



        /// <summary>
        /// Get Product By Id Async
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> DeleteVideoAsync(string videoId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(videoId))
            {
                _logger?.LogWarning("DeleteVideoAsync called with null or empty VideoId");
                return false;
            }

            try
            {
                // Find the product with related entities
                var video = await _context.Video
                    //.Include(p => p.Reviews) // Include related entities if any
                    //.Include(p => p.Categories)
                    .FirstOrDefaultAsync(p => p.VideoId == videoId, cancellationToken);

                if (video == null)
                {
                    _logger?.LogWarning("Product with ID {ProductId} not found for deletion", videoId);
                    return false;
                }

                // Delete related entities first (if cascade delete is not configured)
                //if (product.Reviews?.Any() == true)
                //{
                //    _context.Reviews.RemoveRange(product.Reviews);
                //}

                // Remove the product
                _context.Video.Remove(video);

                // Save all changes - EF Core automatically wraps this in a transaction
                var rowsAffected = await _context.SaveChangesAsync(cancellationToken);

                if (rowsAffected > 0)
                {
                    _logger?.LogInformation("Successfully deleted Video {VideoId}", videoId);
                    return true;
                }
                else
                {
                    _logger?.LogWarning("No rows affected when deleting Video {VideoId}", videoId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting product {ProductId}", videoId);
                throw;
            }
        }


        /// <summary>
        /// Insert Product
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task InsertVideo(Video model, CancellationToken cancellationToken = default)
        {
            try
            {

                model.VideoURL = $"{_blobBaseUrl}{model.VideoId}.mp4";
                model.CreatedDate = DateTime.UtcNow;

                // Add to context and save
                await _context.Video.AddAsync(model);
                await SaveChangesAsync();

                _logger?.LogInformation("Successfully inserted Product with ID: {JoinedRunId}", model.VideoId);
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Update Client Async
        /// </summary>
        /// <param name="privateRun"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> UpdateVideoAsync(Video video, CancellationToken cancellationToken = default)
        {
            try
            {
                // Retrieve the existing product from database
                var existingVideo = await _context.Video
                    .FirstOrDefaultAsync(p => p.VideoId == video.VideoId, cancellationToken);

                if (existingVideo == null)
                {
                    return false;
                }

                // Update only the fields you want to allow updates for
                existingVideo.Status = video.Status;
               

                // Don't update sensitive fields like CreatedDate, CreatedBy, etc.
                // existingProduct.CreatedDate = Video.CreatedDate; // DON'T update

                return await SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating Video {VideoId}", video.VideoId);
                throw;
            }
        }

        /// <summary>
        /// Save Changes Async
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

        public Task<int> BatchUpdateVideosAsync(IEnumerable<Video> videos, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }


        public IAsyncEnumerable<Video> StreamAllVideosAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }


        #endregion
    }

    /// <summary>
    /// Helper class for cursor-based pagination
    /// </summary>
    internal class VideoCursorData
    {
        public string Id { get; set; }
        public string Category { get; set; }
        public string points { get; set; }
    }
}