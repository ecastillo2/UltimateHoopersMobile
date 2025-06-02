using Domain;
using Domain.DtoModel;
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
        public async Task<List<Post>> GetPostsAsync(string postType, string accessToken, CancellationToken cancellationToken = default)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Add postType as query parameter and TimeZone header
            var url = $"{_baseUrl}/api/Post/GetPosts?postType={Uri.EscapeDataString(postType)}";

            // Add TimeZone header if needed
            _httpClient.DefaultRequestHeaders.Remove("TimeZone");
            _httpClient.DefaultRequestHeaders.Add("TimeZone", "America/New_York"); // or pass as parameter

            var response = await _httpClient.GetAsync(url, cancellationToken);
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

        public async Task<CursorPaginatedResultDto<PostViewModelDto>> GetPostsWithCursorAsync(string cursor = null, int limit = 20, string direction = "next", string sortBy = "Points", string accessToken = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // Set authentication header if token is provided
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                }

                // Build query string
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(cursor))
                    queryParams.Add($"cursor={Uri.EscapeDataString(cursor)}");

                queryParams.Add($"limit={limit}");
                queryParams.Add($"direction={Uri.EscapeDataString(direction)}");
                queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");

                var queryString = string.Join("&", queryParams);
                var requestUrl = $"{_baseUrl}/api/Post/cursor{(queryParams.Any() ? "?" + queryString : "")}";

                // Make the request
                var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Deserialize the response
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<CursorPaginatedResultDto<PostViewModelDto>>(content, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API request error: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing error: {ex.Message}");
                throw;
            }
        }
    }
}