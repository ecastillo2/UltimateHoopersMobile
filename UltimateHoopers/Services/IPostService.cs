// Services/IPostService.cs
using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;
//using UltimateHoopers.Models;

namespace UltimateHoopers.Services
{
    public interface IPostService
    {
        Task<List<Post>> GetPostsAsync();
        Task<Post> GetPostByIdAsync(string postId);
        Task<Post> CreatePostAsync(Post post);
        Task<bool> UpdatePostAsync(Post post);
        Task<bool> DeletePostAsync(string postId);
    }
}