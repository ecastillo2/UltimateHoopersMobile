using Domain;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPI.ApiClients
{
    /// <summary>
    /// Interface for Post API operations
    /// </summary>
    public interface IPostApi
    {
        /// <summary>
        /// Get all posts
        /// </summary>
        Task<List<Post>> GetPostsAsync(string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get post by ID
        /// </summary>
        Task<Post> GetPostByIdAsync(string postId, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new post
        /// </summary>
        Task<Post> CreatePostAsync(Post post, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update an existing post
        /// </summary>
        Task<bool> UpdatePostAsync(Post post, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a post
        /// </summary>
        Task<bool> DeletePostAsync(string postId, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get blogs with optional filtering
        /// </summary>
        Task<List<Blog>> GetBlogsAsync(string accessToken, string filter = null, CancellationToken cancellationToken = default);
    }
}