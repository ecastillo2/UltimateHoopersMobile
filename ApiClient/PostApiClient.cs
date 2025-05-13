using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiClient.Configuration;
using ApiClient.Core;
using ApiClient.Interfaces;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiClient.Services
{
    /// <summary>
    /// Implementation of the Post API client
    /// </summary>
    public class PostApiClient : BaseApiClient, IPostApi
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostApiClient"/> class
        /// </summary>
        /// <param name="httpClient">The HTTP client</param>
        /// <param name="logger">The logger</param>
        /// <param name="options">The API client options</param>
        public PostApiClient(
            HttpClient httpClient,
            ILogger<PostApiClient> logger,
            IOptions<ApiClientOptions> options)
            : base(httpClient, logger)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(options.Value.TimeoutSeconds);
        }

        /// <inheritdoc/>
        public async Task<List<Post>> GetPostsAsync(string timeZone, string token, CancellationToken cancellationToken = default)
        {
            string endpoint = $"api/Post/GetPosts?timeZone={Uri.EscapeDataString(timeZone)}";
            return await GetAsync<List<Post>>(endpoint, token, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<List<Post>> GetBlogsAsync(string timeZone, string token, CancellationToken cancellationToken = default)
        {
            string endpoint = $"api/Post/GetBlogs?timeZone={Uri.EscapeDataString(timeZone)}";
            return await GetAsync<List<Post>>(endpoint, token, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<List<Post>> GetNewsAsync(string timeZone, string token, CancellationToken cancellationToken = default)
        {
            string endpoint = $"api/Post/GetNews?timeZone={Uri.EscapeDataString(timeZone)}";
            return await GetAsync<List<Post>>(endpoint, token, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Post> GetPostByIdAsync(string postId, string timeZone, string token, CancellationToken cancellationToken = default)
        {
            string endpoint = $"api/Post/GetPostById?postId={Uri.EscapeDataString(postId)}&timeZone={Uri.EscapeDataString(timeZone)}";
            return await GetAsync<Post>(endpoint, token, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> CreatePostAsync(Post post, string token, CancellationToken cancellationToken = default)
        {
            string endpoint = "api/Post/CreatePost/";
            await PostAsync<object>(endpoint, post, token, cancellationToken);
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdatePostAsync(Post post, string token, CancellationToken cancellationToken = default)
        {
            string endpoint = "api/Post/UpdatePost";
            await PostAsync<object>(endpoint, post, token, cancellationToken);
            return true;
        }

        /// <inheritdoc/>
        public async Task<List<Post>> GetPostsByProfileIdAsync(string profileId, string timeZone, string token, CancellationToken cancellationToken = default)
        {
            string endpoint = $"api/Post/GetPostsByProfileId?profileId={Uri.EscapeDataString(profileId)}&timeZone={Uri.EscapeDataString(timeZone)}";
            return await GetAsync<List<Post>>(endpoint, token, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<List<Post>> GetSavedPostsByProfileIdAsync(string profileId, string timeZone, string token, CancellationToken cancellationToken = default)
        {
            string endpoint = $"api/Post/GetSavedPostsByProfileId?profileId={Uri.EscapeDataString(profileId)}&timeZone={Uri.EscapeDataString(timeZone)}";
            return await GetAsync<List<Post>>(endpoint, token, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<List<Post>> GetPostsMentionProfileIdAsync(string profileId, string timeZone, string token, CancellationToken cancellationToken = default)
        {
            string endpoint = $"api/Post/GetPostsMentionProfileId?profileId={Uri.EscapeDataString(profileId)}&timeZone={Uri.EscapeDataString(timeZone)}";
            return await GetAsync<List<Post>>(endpoint, token, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ApiResult> DeletePostAsync(string postId, string token, CancellationToken cancellationToken = default)
        {
            try
            {
                string endpoint = $"api/Post/DeletePost?postId={Uri.EscapeDataString(postId)}";
                var result = await DeleteAsync<JsonResult>(endpoint, token, cancellationToken);
                return ApiResult.Succeeded("Post deleted successfully", result);
            }
            catch (ApiException ex)
            {
                return ApiResult.Failed(ex.Message);
            }
        }

        /// <inheritdoc/>
        public async Task UpdatePostStatusAsync(string postId, string status, string token, CancellationToken cancellationToken = default)
        {
            string endpoint = $"api/Post/UpdatePostStatus?postId={Uri.EscapeDataString(postId)}&status={Uri.EscapeDataString(status)}";
            await GetAsync<object>(endpoint, token, cancellationToken);
        }
    }
}