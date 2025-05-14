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
        private const string TOKEN_KEY = "auth_token";

        public PostService()
        {
            // Create or get the PostApi instance
            // This is a simplified example - in a real app, you might use dependency injection
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
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("No access token available");
            }

            return await _postApi.CreatePostAsync(post, token);
        }

        public async Task<bool> DeletePostAsync(string postId)
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("No access token available");
            }

            return await _postApi.DeletePostAsync(postId, token);
        }

        public async Task<Post> GetPostByIdAsync(string postId)
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("No access token available");
            }

            return await _postApi.GetPostByIdAsync(postId, token);
        }

        public async Task<List<Post>> GetPostsAsync()
        {
            // First try to get the token from the App's global auth token
            var token = App.AuthToken;

            // If it's not available in the global App state, try to get it from secure storage
            if (string.IsNullOrEmpty(token))
            {
                token = await SecureStorage.GetAsync(TOKEN_KEY);
            }

            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("No access token available");
            }

            try
            {
                // Call the API with the retrieved token
                return await _postApi.GetPostsAsync(token);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting posts: {ex.Message}");
                // If we get an exception, it might be due to an expired or invalid token
                // For now, just propagate the exception upward
                throw;
            }
        }

        public async Task<bool> UpdatePostAsync(Post post)
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("No access token available");
            }

            return await _postApi.UpdatePostAsync(post, token);
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

            return token;
        }
    }
}