using Domain;
using Domain.DtoModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebAPI.ApiClients;

namespace UltimateHoopers.Services
{
    public class PostService : IPostService
    {
        private readonly IPostApi _postApi;
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
            _postApi = new PostApi(httpClient, configuration);

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
            _postApi = new PostApi(_httpClient, _configuration);
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

                // Call the API with the retrieved token
                var paginatedResult = await _postApi.GetPostsWithCursorAsync(
                    cursor: null,
                    limit: 50, // Request a larger batch
                    direction: "next",
                    sortBy: "Date",
                    accessToken: token);

                LogInfo($"API call completed. Result: {(paginatedResult != null ? "Success" : "Null")}");
                LogInfo($"Items count: {paginatedResult?.Items?.Count ?? 0}");

                // Convert items to Post objects
                if (paginatedResult != null && paginatedResult.Items != null && paginatedResult.Items.Count > 0)
                {
                    // Create a list to hold the converted posts
                    var posts = new List<Post>();

                    foreach (var item in paginatedResult.Items)
                    {
                        try
                        {
                            // Map properties from the DTO to a new Post object
                            var post = new Post
                            {
                                PostId = item.PostId,
                                UserId = item.UserId,
                                Caption = item.Caption,
                                PostFileURL = item.PostFileURL,
                                Type = item.Type,
                                Status = item.Status,
                                Likes = item.Likes,
                                DisLikes = item.DisLikes,
                                Hearted = item.Hearted,
                                Views = item.Views,
                                Shared = item.Shared,
                                PostedDate = item.PostedDate,
                                ProfileId = item.ProfileId,
                                ThumbnailUrl = item.ThumbnailUrl,
                                PostType = item.PostType,
                                PostText = item.PostText,
                                Title = item.Title,
                                Category = item.Category,
                                Mention = item.Mention,
                                MentionUserNames = item.MentionUserNames,

                                // Add these properties for the UI
                                UserName = "Unknown User",
                                RelativeTime = "2 hours ago",
                                ProfileImageURL = item.ThumbnailUrl,
                                PostCommentCount =  0,
                                LikedPost =  false,
                                SavedPost =  false
                            };

                            posts.Add(post);
                            LogInfo($"Added post: {post.PostId}");
                        }
                        catch (Exception ex)
                        {
                            LogError($"Error mapping post {item.PostId}", ex);
                        }
                    }

                    LogInfo($"Returning {posts.Count} posts from API");
                    return posts;
                }
                else
                {
                    LogInfo("No posts returned from API, returning empty list");
                    return new List<Post>();
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

        // Helper method to format relative time
        private string FormatRelativeTime(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr))
                return "Recently";

            if (DateTime.TryParse(dateStr, out DateTime date))
            {
                TimeSpan diff = DateTime.Now - date;

                if (diff.TotalMinutes < 1)
                    return "Just now";
                if (diff.TotalMinutes < 60)
                    return $"{(int)diff.TotalMinutes} minutes ago";
                if (diff.TotalHours < 24)
                    return $"{(int)diff.TotalHours} hours ago";
                if (diff.TotalDays < 7)
                    return $"{(int)diff.TotalDays} days ago";

                return date.ToString("MMM dd, yyyy");
            }

            return "Recently";
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
                    PostFileURL = "https://uhblobstorageaccount.blob.core.windows.net/postfile/a12a3e62-4f16-45b3-9da7-a8f5e957d658.mp4",
                    ThumbnailUrl = "https://uhblobstorageaccount.blob.core.windows.net/postthumbnail/a12a3e62-4f16-45b3-9da7-a8f5e957d658.png",
                    PostType = "video",
                    Likes = 32,
                    ProfileImageURL = "https://uhblobstorageaccount.blob.core.windows.net/postthumbnail/a12a3e62-4f16-45b3-9da7-a8f5e957d658.png",
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