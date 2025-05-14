// Services/PostService.cs
using Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//using UltimateHoopers.Models;

namespace UltimateHoopers.Services
{
    public class PostService : IPostService
    {
        public Task<Post> CreatePostAsync(Post post)
        {
            // Return a mock response for now
            return Task.FromResult(post);
        }

        public Task<bool> DeletePostAsync(string postId)
        {
            return Task.FromResult(true);
        }

        public Task<Post> GetPostByIdAsync(string postId)
        {
            return Task.FromResult(new Post { PostId = postId, Caption = "Sample Post" });
        }

        public Task<List<Post>> GetPostsAsync()
        {
            // Return mock data for now
            var posts = new List<Post>
            {
                new Post {
                    PostId = Guid.NewGuid().ToString(),
                    Caption = "Sample Post 1",
                    UserName = "john_doe",
                    Likes = 32,
                    PostedDate = DateTime.Now.AddHours(-2).ToString(),
                    RelativeTime = "2 hours ago"
                },
                new Post {
                    PostId = Guid.NewGuid().ToString(),
                    Caption = "Sample Post 2",
                    UserName = "jane_smith",
                    Likes = 47,
                    PostedDate = DateTime.Now.AddHours(-5).ToString(),
                    RelativeTime = "5 hours ago"
                }
            };

            return Task.FromResult(posts);
        }

        public Task<bool> UpdatePostAsync(Post post)
        {
            return Task.FromResult(true);
        }
    }
}