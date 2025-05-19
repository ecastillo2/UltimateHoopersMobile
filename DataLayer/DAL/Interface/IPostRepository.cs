using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.DAL.Interface
{
    /// <summary>
    /// Repository interface for Post-related data operations
    /// </summary>
    public interface IPostRepository : IDisposable
    {
        /// <summary>
        /// Get all posts with timezone for relative time calculation
        /// </summary>
        Task<List<Post>> GetPosts(string timezone);

        /// <summary>
        /// Get posts with cursor-based pagination optimized for mobile scrolling
        /// </summary>
        /// <param name="cursor">Optional cursor for pagination</param>
        /// <param name="limit">Number of posts to return</param>
        /// <param name="timeZone">Timezone for relative time calculation</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tuple containing posts, next cursor, and whether more posts exist</returns>
        Task<(List<Post> Posts, string NextCursor, bool HasMore)> GetPostsWithCursorAsync(
            string cursor = null,
            int limit = 10,
            string timeZone = "America/New_York",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidate the posts cache
        /// </summary>
        void InvalidateCache();

        /// <summary>
        /// Get all posts including draft and archived posts
        /// </summary>
        Task<List<Post>> GetAllPosts(string timezone);

        /// <summary>
        /// Get blog-type posts
        /// </summary>
        Task<List<Post>> GetBlogs(string timezone);

        /// <summary>
        /// Get news-type posts
        /// </summary>
        Task<List<Post>> GetHoopNews(string timezone);

        /// <summary>
        /// Get event-type posts
        /// </summary>
        Task<List<Post>> GetEvents(string timezone);

        /// <summary>
        /// Get all publicly visible posts
        /// </summary>
        Task<List<Post>> GetPublicPosts();

        /// <summary>
        /// Get posts created by a specific profile
        /// </summary>
        Task<List<Post>> GetPostsByProfileId(string profileId, string timezone);

        /// <summary>
        /// Get posts that mention a specific profile
        /// </summary>
        Task<List<Post>> GetPostsMentionProfileId(string profileId, string timezone);

        /// <summary>
        /// Get posts with a specific tag
        /// </summary>
        Task<List<Post>> GetPostsWithTagByTagId(string tagId, string timezone);

        /// <summary>
        /// Get posts saved by a specific profile
        /// </summary>
        Task<List<Post>> GetSavedPostsByProfileId(string profileId, string timezone);

        /// <summary>
        /// Get a post by its ID with all related data
        /// </summary>
        Task<Post> GetPostById(string postId, string timeZone);

        /// <summary>
        /// Get a post by its ID with all details (alias for GetPostById)
        /// </summary>
        Task<Post> GetPostByIdWithDetailsAsync(string postId, string timeZone);

        /// <summary>
        /// Insert a new post
        /// </summary>
        Task InsertPost(Post model);

        /// <summary>
        /// Delete an existing post
        /// </summary>
        Task DeletePost(string postId);

        /// <summary>
        /// Update an existing post
        /// </summary>
        Task UpdatePost(Post model);

        /// <summary>
        /// Update a post's status
        /// </summary>
        Task UpdatePostStatus(string postId, string status);

        /// <summary>
        /// Save changes to the database
        /// </summary>
        Task<int> Save();

        /// <summary>
        /// Get paginated posts with total count and pages
        /// </summary>
        Task<(List<Post> Posts, int TotalCount, int TotalPages)> GetPaginatedPostsAsync(
            int page, int pageSize, string timeZone);

        /// <summary>
        /// Check if a post is liked by a specific profile
        /// </summary>
        Task<bool> IsPostLikedByProfileAsync(string postId, string profileId);

        /// <summary>
        /// Like a post
        /// </summary>
        Task LikePostAsync(string postId, string profileId);

        /// <summary>
        /// Unlike a post
        /// </summary>
        Task UnlikePostAsync(string postId, string profileId);

        /// <summary>
        /// Get the average star rating for a profile
        /// </summary>
        Task<string> GetAverageStarRatingByProfileId(string profileId);
    }
}