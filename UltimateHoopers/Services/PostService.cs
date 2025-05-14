using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace UltimateHoopers.Services
{
    public class PostService : IPostService
    {
        private readonly ILogger<PostService> _logger;
        private const string TOKEN_KEY = "auth_token";

        // Constructor with minimal dependencies
        public PostService(ILogger<PostService> logger = null)
        {
            _logger = logger;
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            try
            {
                // Implementation would be here
                return post;
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
                // Implementation would be here
                return true;
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
                // Implementation would be here
                return null;
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
                // Create a list to hold the posts
                IList<Post> posts = new List<Post>();

                // Add posts with your Azure Blob Storage URLs - using minimal data to test robustness

                // Post 1: Just ID and URL
                posts.Add(new Post
                {
                    PostId = "1",
                    PostFileURL = "https://uhblobstorageaccount.blob.core.windows.net/postfile/c3c7c0b7-e279-42a7-af86-a862f42d349a.webp"
                    // Everything else null
                });

                // Post 2: ID, URL, and type
                posts.Add(new Post
                {
                    PostId = "2",
                    PostFileURL = "https://uhblobstorageaccount.blob.core.windows.net/postfile/92a4bc09-eace-48e5-a2f4-73561d3451b9.mp4",
                    PostType = "video"
                    // Everything else null
                });

                // Post 3: Most fields but some nulls
                posts.Add(new Post
                {
                    PostId = "3",
                    UserName = "Regular User",
                    Caption = "Testing with a regular image URL",
                    PostFileURL = "https://images.unsplash.com/photo-1546519638-68e109498ffc?q=80&w=1000&auto=format&fit=crop",
                    PostType = "image",
                    Likes = 42,
                    // RelativeTime is null
                    ProfileImageURL = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?q=80&w=1000&auto=format&fit=crop",
                    // LikedPost is null
                    SavedPost = true,
                    PostCommentCount = 5
                });

                // Post 4: Empty strings instead of nulls
                posts.Add(new Post
                {
                    PostId = "4",
                    UserName = "",
                    Caption = "",
                    PostFileURL = "https://images.unsplash.com/photo-1505666287802-931d7a78bde2?q=80&w=1000&auto=format&fit=crop",
                    PostType = "",
                    Likes = 0,
                    RelativeTime = "",
                    ProfileImageURL = "",
                    LikedPost = false,
                    SavedPost = false,
                    PostCommentCount = 0
                });

                // Post 5: Just whitespace in string fields
                posts.Add(new Post
                {
                    PostId = "5",
                    UserName = "   ",
                    Caption = "  ",
                    PostFileURL = "https://images.unsplash.com/photo-1566492031773-4f4e44671857?q=80&w=1000&auto=format&fit=crop",
                    PostType = " ",
                    Likes = null,
                    RelativeTime = "  ",
                    ProfileImageURL = "  ",
                    LikedPost = null,
                    SavedPost = null,
                    PostCommentCount = null
                });

                // Post 6: First and last name but no username
                posts.Add(new Post
                {
                    PostId = "6",
                    FirstName = "John",
                    LastName = "Doe",
                    // UserName is null
                    Caption = "Testing name fallback",
                    PostFileURL = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?q=80&w=1000&auto=format&fit=crop",
                    // PostType is null - should be detected from URL
                    Likes = 12,
                    // All other fields null
                });

                // Post 7: A complete post with all fields
                posts.Add(new Post
                {
                    PostId = "7",
                    UserName = "Complete User",
                    FirstName = "Complete",
                    LastName = "User",
                    Caption = "This post has all fields populated",
                    PostFileURL = "https://uhblobstorageaccount.blob.core.windows.net/postfile/c3c7c0b7-e279-42a7-af86-a862f42d349a.webp",
                    ThumbnailUrl = "https://uhblobstorageaccount.blob.core.windows.net/postfile/c3c7c0b7-e279-42a7-af86-a862f42d349a.webp",
                    PostType = "image",
                    PostedDate = DateTime.Now.AddDays(-2).ToString(),
                    RelativeTime = "2 days ago",
                    ProfileImageURL = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?q=80&w=1000&auto=format&fit=crop",
                    Likes = 87,
                    LikedPost = true,
                    SavedPost = true,
                    PostCommentCount = 15
                });

                // Log what we're returning
                Console.WriteLine($"GetPostsAsync returning {posts.Count} posts");
                foreach (var post in posts)
                {
                    Console.WriteLine($"Post: {post.PostId}, Type: {post.PostType ?? "null"}, URL: {post.PostFileURL}");
                }

                // Convert to List<Post> for return
                return new List<Post>(posts);
            }
            catch (Exception ex)
            {
                LogError("Error getting posts", ex);

                // For development/testing, return mock data if API fails
                Console.WriteLine("Returning fallback mock posts due to error");
                return CreateMockPosts();
            }
        }

        public async Task<bool> UpdatePostAsync(Post post)
        {
            try
            {
                // Implementation would be here
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error updating post {post?.PostId}", ex);
                throw;
            }
        }

        private async Task<string> GetTokenAsync()
        {
            // Implementation would be here
            return "sample-token";
        }

        private void LogError(string message, Exception ex)
        {
            if (_logger != null)
            {
                _logger.LogError(ex, message);
            }
            else
            {
                Console.WriteLine($"{message}: {ex.Message}");
            }
        }

        // Mock data for fallback
        private List<Post> CreateMockPosts()
        {
            return new List<Post>
            {
                new Post
                {
                    PostId = "fallback-1",
                    UserName = "Fallback User",
                    Caption = "Fallback post with a regular image",
                    PostFileURL = "https://images.unsplash.com/photo-1505666287802-931d7a78bde2?q=80&w=1000&auto=format&fit=crop",
                    PostType = "image",
                    Likes = 32,
                    ProfileImageURL = "https://images.unsplash.com/photo-1566492031773-4f4e44671857?q=80&w=1000&auto=format&fit=crop",
                    RelativeTime = "2 hours ago",
                    PostCommentCount = 12,
                    LikedPost = false,
                    SavedPost = false
                },
                // Add more fallback posts if needed
            };
        }
    }
}