using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Domain;
using Domain.DtoModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace UltimateHoopers.Services
{
    public class PostService : IPostService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PostService> _logger;
        private const string TOKEN_KEY = "auth_token";
        private readonly string _baseUrl;

        // Constructor with proper DI
        public PostService(HttpClient httpClient, IConfiguration configuration, ILogger<PostService> logger = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;

            // Get base URL from configuration or use default
            _baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://ultimatehoopersapi.azurewebsites.net/";

            // Set base address for HttpClient if not already set
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                LogInfo($"Set HttpClient.BaseAddress to {_baseUrl}");
            }

            // Set default headers
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            LogInfo($"PostService initialized with base URL: {_baseUrl}");
        }

        // Simplified constructor for non-DI scenarios
        public PostService()
        {
            _httpClient = new HttpClient();
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ApiSettings:BaseUrl"] = "https://ultimatehoopersapi.azurewebsites.net/"
                })
                .Build();

            _baseUrl = _configuration["ApiSettings:BaseUrl"];

            // Set base address and headers
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            LogInfo($"PostService initialized with base URL: {_baseUrl} (non-DI constructor)");
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Implementation logic
                throw new NotImplementedException("CreatePostAsync not implemented yet");
            }
            catch (Exception ex)
            {
                LogError("Error creating post", ex);
                throw;
            }
        }

        public async Task<bool> DeletePostAsync(string postId)
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Implementation logic
                throw new NotImplementedException("DeletePostAsync not implemented yet");
            }
            catch (Exception ex)
            {
                LogError($"Error deleting post {postId}", ex);
                throw;
            }
        }

        public async Task<Post> GetPostByIdAsync(string postId)
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Implementation logic
                throw new NotImplementedException("GetPostByIdAsync not implemented yet");
            }
            catch (Exception ex)
            {
                LogError($"Error getting post {postId}", ex);
                throw;
            }
        }

        public async Task<List<Post>> GetPostsAsync()
        {
            try
            {
                LogInfo("GetPostsAsync called");

                // Get token (first from App state, then from secure storage)
                var token = await GetTokenAsync();
                LogInfo($"Token retrieved: {!string.IsNullOrEmpty(token)}");

                if (string.IsNullOrEmpty(token))
                {
                    LogError("No access token available", null);
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Create the request URL for the paginated API endpoint
                string apiUrl = "api/Post/GetPostsWithCursor";

                // Create a JSON payload for the request
                var requestData = new
                {
                    cursor = (string)null,
                    limit = 50,
                    direction = "next",
                    sortBy = "Date"
                };

                // Serialize the request data
                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");

                // Add authorization header
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                LogInfo($"Sending request to {apiUrl}");

                // Make the HTTP request
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);

                // Check if request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    LogInfo($"Received successful response with length: {jsonResponse?.Length ?? 0}");

                    // Deserialize the paginated result
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var result = JsonSerializer.Deserialize<PaginatedResultDto<Post>>(jsonResponse, options);

                    if (result != null && result.Items != null)
                    {
                        LogInfo($"Successfully deserialized {result.Items.Count} posts");
                        return new List<Post>(result.Items);
                    }
                    else
                    {
                        LogInfo("No posts found in the response");
                        return new List<Post>();
                    }
                }
                else
                {
                    // Log the error response
                    string errorContent = await response.Content.ReadAsStringAsync();
                    LogError($"Error response from API: {response.StatusCode}, {errorContent}", null);

                    // Check for specific status codes
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedAccessException("Authentication failed. Please log in again.");
                    }

                    throw new HttpRequestException($"Error retrieving posts: {response.StatusCode}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                LogError("Unauthorized access", null);
                throw; // Rethrow to be handled by the caller
            }
            catch (Exception ex)
            {
                LogError("Error getting posts", ex);

#if DEBUG
                // For development and testing, return mock data if API call fails
                LogInfo("Returning mock posts for testing due to API error");
                return CreateMockPosts();
#else
                throw;
#endif
            }
        }

        public async Task<bool> UpdatePostAsync(Post post)
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Implementation logic
                throw new NotImplementedException("UpdatePostAsync not implemented yet");
            }
            catch (Exception ex)
            {
                LogError($"Error updating post {post?.PostId}", ex);
                throw;
            }
        }

        private async Task<string> GetTokenAsync()
        {
            try
            {
                // First try to get the token from the App's global auth token
                var token = App.AuthToken;
                LogInfo($"App.AuthToken: {!string.IsNullOrEmpty(token)}");

                // If it's not available in the global App state, try to get it from secure storage
                if (string.IsNullOrEmpty(token))
                {
                    token = await SecureStorage.GetAsync(TOKEN_KEY);
                    LogInfo($"SecureStorage token: {!string.IsNullOrEmpty(token)}");
                }

                // For development, provide a fallback token
#if DEBUG
                if (string.IsNullOrEmpty(token))
                {
                    token = "development-token";
                    LogInfo("Using development fallback token");
                }
#endif

                return token;
            }
            catch (Exception ex)
            {
                LogError("Error retrieving token", ex);
                return null;
            }
        }

        private void LogInfo(string message)
        {
            if (_logger != null)
            {
                _logger.LogInformation(message);
            }

            Debug.WriteLine($"[PostService] INFO: {message}");
        }

        private void LogError(string message, Exception ex)
        {
            if (_logger != null && ex != null)
            {
                _logger.LogError(ex, message);
            }
            else if (_logger != null)
            {
                _logger.LogError(message);
            }

            if (ex != null)
            {
                Debug.WriteLine($"[PostService] ERROR: {message}: {ex.Message}");
                Debug.WriteLine($"[PostService] Stack trace: {ex.StackTrace}");
            }
            else
            {
                Debug.WriteLine($"[PostService] ERROR: {message}");
            }
        }

        // Mock data for development and testing
        private List<Post> CreateMockPosts()
        {
            LogInfo("Creating mock posts for testing");

            return new List<Post>
            {
                new Post
                {
                    PostId = "1",
                    UserName = "michael_johnson",
                    Caption = "Looking for players to join our game this Sunday at Downtown Court. We need 2-3 more players. All skill levels welcome! #basketball #pickup #sunday",
                    PostFileURL = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4",
                    ThumbnailUrl = "https://peach.blender.org/wp-content/uploads/bbb-splash.png",
                    PostType = "video",
                    Likes = 32,
                    ProfileImageURL = "https://randomuser.me/api/portraits/men/32.jpg",
                    RelativeTime = "2 hours ago",
                    PostCommentCount = 12,
                    LikedPost = false,
                    SavedPost = false
                },
                new Post
                {
                    PostId = "2",
                    UserName = "sarah_thompson",
                    Caption = "Just finished my first training session with Coach Williams. His shooting drills are incredible! My three-point percentage has already improved. #basketball #training #threepointer",
                    PostFileURL = "https://images.unsplash.com/photo-1518626413561-907586085645?q=80&w=1000&auto=format&fit=crop",
                    PostType = "image",
                    Likes = 55,
                    ProfileImageURL = "https://randomuser.me/api/portraits/women/32.jpg",
                    RelativeTime = "5 hours ago",
                    PostCommentCount = 8,
                    LikedPost = true,
                    SavedPost = false
                },
                new Post
                {
                    PostId = "3",
                    UserName = "basketball_highlights",
                    Caption = "Check out this amazing dunk from last night's game! Who says white men can't jump? 🏀🔥 #basketball #dunk #highlights",
                    PostFileURL = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4",
                    ThumbnailUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/images/ElephantsDream.jpg",
                    PostType = "video",
                    Likes = 128,
                    ProfileImageURL = "https://randomuser.me/api/portraits/men/85.jpg",
                    RelativeTime = "1 day ago",
                    PostCommentCount = 24,
                    LikedPost = false,
                    SavedPost = true
                },
                new Post
                {
                    PostId = "4",
                    UserName = "photo_expert",
                    Caption = "Basketball court at sunset. Perfect lighting for a game! 🏀 #basketball #sunset #court",
                    PostFileURL = "https://images.unsplash.com/photo-1505666287802-931dc83d1b52?q=80&w=1000&auto=format&fit=crop",
                    PostType = "image",
                    Likes = 19,
                    ProfileImageURL = "https://randomuser.me/api/portraits/women/65.jpg",
                    RelativeTime = "3 hours ago",
                    PostCommentCount = 2,
                    LikedPost = false,
                    SavedPost = false
                }
            };
        }
    }
}