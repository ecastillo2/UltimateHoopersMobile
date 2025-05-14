using Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.ApiClients;

namespace UltimateHoopers.Services
{
    public class PostService : IPostService
    {
        private readonly IPostApi _postApi;
        public PostService()
        {
            // Create or get the PostApi instance
            // This is a simplified example - in a real app, you might use dependency injection
            var httpClient = new HttpClient();
            var configuration = new ConfigurationBuilder().Build(); // Or get from your app's configuration
            _postApi = new PostApi(httpClient, configuration);
        }
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

        public async Task<List<Post>> GetPostsAsync()
        {

            var accessToken = await SecureStorage.GetAsync("access_token");

            if (string.IsNullOrEmpty(accessToken))
            {
                throw new UnauthorizedAccessException("No access token available");
            }

            // Since you're already awaiting _postApi.GetPostsAsync, it returns List<Post>, not Task<List<Post>>
            // So just return that result directly
            return await _postApi.GetPostsAsync(accessToken);

        }

        public Task<bool> UpdatePostAsync(Post post)
        {
            return Task.FromResult(true);
        }
    }
}