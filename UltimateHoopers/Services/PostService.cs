using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using WebAPI.ApiClients;

namespace UltimateHoopers.Services
{
    public class PostService : IPostService
    {
        private readonly IPostApi _postApi;
        private readonly ILogger<PostService> _logger;
        private const string TOKEN_KEY = "auth_token";

        // Constructor with proper DI
        public PostService(HttpClient httpClient, IConfiguration configuration, ILogger<PostService> logger = null)
        {
            // Create PostApi with the provided dependencies
            _postApi = new PostApi(httpClient, configuration);
            _logger = logger;
        }

        // Simplified constructor for non-DI scenarios
        public PostService()
        {
            // Create PostApi with default dependencies
            var httpClient = new HttpClient();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ApiSettings:BaseUrl"] = "https://ultimatehoopersapi.azurewebsites.net/"
                })
                .Build();

            _postApi = new PostApi(httpClient, configuration);
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

                return await _postApi.CreatePostAsync(post, token);
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

                return await _postApi.DeletePostAsync(postId, token);
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

                return await _postApi.GetPostByIdAsync(postId, token);
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
                // Get token (first from App state, then from secure storage)
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }

                // Call the API with the retrieved token
                var paginatedResult = await _postApi.GetPostsWithCursorAsync(
                    cursor: null,
                    limit: 50, // Request a larger batch
                    direction: "next",
                    sortBy: "Date",
                    accessToken: token);
                // Convert ProfileViewModelDto to Profile objects
                if (paginatedResult != null && paginatedResult.Items != null)
                {
                    // Create a list to hold the converted profiles
                    var posts = new List<Post>();

                    foreach (var item in paginatedResult.Items)
                    {
                        // Map properties from the DTO to a new Profile object
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
                        };

                        posts.Add(post);
                    }

                    return posts;
                }

                // Return empty list if no results
                return new List<Post>();
            }
            catch (Exception ex)
            {
                LogError("Error getting profiles with cursor", ex);

                // For development, provide mock data if API fails
#if DEBUG
                return null;
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

                return await _postApi.UpdatePostAsync(post, token);
            }
            catch (Exception ex)
            {
                LogError($"Error updating post {post?.PostId}", ex);
                throw;
            }
        }

        private async Task<string> GetTokenAsync()
        {
            // First try to get the token from the App's global auth token
            var token = App.AuthToken;

            // If it's not available in the global App state, try to get it from secure storage
            if (string.IsNullOrEmpty(token))
            {
                token = await SecureStorage.GetAsync(TOKEN_KEY);
            }

            // For development, provide a fallback token
#if DEBUG
            if (string.IsNullOrEmpty(token))
            {
                token = "development-token";
            }
#endif

            return token;
        }

        private void LogError(string message, Exception ex)
        {
            if (_logger != null)
            {
                _logger.LogError(ex, message);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"{message}: {ex.Message}");
            }
        }

        // Mock data for development and testing
        private List<Post> CreateMockPosts()
        {
            return new List<Post>
            {
                new Post
                {
                    PostId = "1",
                    UserName = "michael_johnson",
                    Caption = "Looking for players to join our game this Sunday at Downtown Court. We need 2-3 more players. All skill levels welcome! #basketball #pickup #sunday",
                    PostFileURL = "https://uhblobstorageaccount.blob.core.windows.net/postfile/92a4bc09-eace-48e5-a2f4-73561d3451b9.mp4",
                    PostType = "video",
                    Likes = 32,
                    ThumbnailUrl = "https://uhblobstorageaccount.blob.core.windows.net/postthumbnail/92a4bc09-eace-48e5-a2f4-73561d3451b9.png",
                    ProfileImageURL = "https://images.unsplash.com/photo-1566492031773-4f4e44671857?q=80&w=1000&auto=format&fit=crop",
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
                    PostFileURL = "https://uhblobstorageaccount.blob.core.windows.net/postfile/bd2f7d77-88db-4997-807f-b122ef7bbd0d.webp",
                    PostType = "image",
                    Likes = 55,
                    ProfileImageURL = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?q=80&w=1000&auto=format&fit=crop",
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
                    PostFileURL = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1518020382113-a7e8fc38eac9?q=80&w=1000&auto=format&fit=crop",
                    PostType = "video",
                    Likes = 128,
                    ProfileImageURL = "https://images.unsplash.com/photo-1518020382113-a7e8fc38eac9?q=80&w=1000&auto=format&fit=crop",
                    RelativeTime = "1 day ago",
                    PostCommentCount = 24,
                    LikedPost = false,
                    SavedPost = true
                },
                new Post
                {
                    PostId = "4",
                    UserName = "webp_tester",
                    Caption = "Testing WEBP image format - this is a high quality but smaller file size format! #webp #basketball",
                    PostFileURL = "https://www.gstatic.com/webp/gallery/4.webp",
                    PostType = "image",
                    Likes = 19,
                    ProfileImageURL = "https://www.gstatic.com/webp/gallery/5.webp",
                    RelativeTime = "3 hours ago",
                    PostCommentCount = 2,
                    LikedPost = false,
                    SavedPost = false
                }
            };
        }
    }
}