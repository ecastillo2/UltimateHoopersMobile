using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain;

namespace ApiClient.Interfaces
{
    /// <summary>
    /// Interface for the Post API client
    /// </summary>
    public interface IPostApi
    {
        /// <summary>
        /// Gets all posts
        /// </summary>
        /// <param name="timeZone">The timezone for date formatting</param>
        /// <param name="token">Authentication token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of posts</returns>
        Task<List<Post>> GetPostsAsync(string timeZone, string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets blog posts
        /// </summary>
        /// <param name="timeZone">The timezone for date formatting</param>
        /// <param name="token">Authentication token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of blog posts</returns>
        Task<List<Post>> GetBlogsAsync(string timeZone, string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets news posts
        /// </summary>
        /// <param name="timeZone">The timezone for date formatting</param>
        /// <param name="token">Authentication token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of news posts</returns>
        Task<List<Post>> GetNewsAsync(string timeZone, string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a post by ID
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <param name="timeZone">The timezone for date formatting</param>
        /// <param name="token">Authentication token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The post</returns>
        Task<Post> GetPostByIdAsync(string postId, string timeZone, string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new post
        /// </summary>
        /// <param name="post">The post to create</param>
        /// <param name="token">Authentication token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful</returns>
        Task<bool> CreatePostAsync(Post post, string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing post
        /// </summary>
        /// <param name="post">The post to update</param>
        /// <param name="token">Authentication token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful</returns>
        Task<bool> UpdatePostAsync(Post post, string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets posts by profile ID
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <param name="timeZone">The timezone for date formatting</param>
        /// <param name="token">Authentication token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of posts</returns>
        Task<List<Post>> GetPostsByProfileIdAsync(string profileId, string timeZone, string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets saved posts by profile ID
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <param name="timeZone">The timezone for date formatting</param>
        /// <param name="token">Authentication token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of saved posts</returns>
        Task<List<Post>> GetSavedPostsByProfileIdAsync(string profileId, string timeZone, string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets posts mentioning a profile
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <param name="timeZone">The timezone for date formatting</param>
        /// <param name="token">Authentication token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of posts</returns>
        Task<List<Post>> GetPostsMentionProfileIdAsync(string profileId, string timeZone, string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a post
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <param name="token">Authentication token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result of the delete operation</returns>
        Task<ApiResult> DeletePostAsync(string postId, string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a post's status
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <param name="status">New status</param>
        /// <param name="token">Authentication token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task UpdatePostStatusAsync(string postId, string status, string token, CancellationToken cancellationToken = default);
    }
}