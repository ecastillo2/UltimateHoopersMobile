using Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPI.ApiClients
{
    /// <summary>
    /// Implementation of Post API client
    /// </summary>
    public class PostApi : IPostApi
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public PostApi(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://ultimatehoopersapi.azurewebsites.net";

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Get all posts
        /// </summary>
        public async Task<List<Post>> GetPostsAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/Post/GetPosts", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<List<Post>>(content, _jsonOptions);
        }

        /// <summary>
        /// Get post by ID
        /// </summary>
        public async Task<Post> GetPostByIdAsync(string postId, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/Post/GetPostById?postId={postId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<Post>(content, _jsonOptions);
        }

        /// <summary>
        /// Create a new post
        /// </summary>
        public async Task<Post> CreatePostAsync(Post post, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(post, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/Post/CreatePost", jsonContent, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<Post>(content, _jsonOptions);
        }

        /// <summary>
        /// Update an existing post
        /// </summary>
        public async Task<bool> UpdatePostAsync(Post post, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(post, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/Post/UpdatePost", jsonContent, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Delete a post
        /// </summary>
        public async Task<bool> DeletePostAsync(string postId, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/Post/DeletePost?postId={postId}", cancellationToken);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Get blogs with optional filtering
        /// </summary>
        public async Task<List<Blog>> GetBlogsAsync(string accessToken, string filter = null, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            string url = $"{_baseUrl}/api/Blog/GetBlogs";
            if (!string.IsNullOrEmpty(filter))
            {
                url += $"?filter={Uri.EscapeDataString(filter)}";
            }

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<List<Blog>>(content, _jsonOptions);
        }
    }
}