using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Common;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for Post entity operations
    /// </summary>
    public class PostRepository : GenericRepository<Post>, IPostRepository
    {
        public PostRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get post by ID with related data and relative time calculation
        /// </summary>
        public async Task<Post> GetPostByIdWithDetailsAsync(string postId, string timeZone)
        {
            var post = await _dbSet
                .Include(p => p.PostComments)
                .Include(p => p.ProfileMentions)
                .FirstOrDefaultAsync(p => p.PostId == postId);

            if (post == null)
                return null;

            // Get user/profile info
            post.UserName = await _context.Profiles
                .Where(p => p.ProfileId == post.ProfileId)
                .Select(p => p.UserName)
                .FirstOrDefaultAsync();

            // Calculate relative time
            if (DateTime.TryParse(post.PostedDate, out DateTime postedDate))
            {
                post.RelativeTime = RelativeTime.GetRelativeTime(postedDate, timeZone);
            }

            // Count likes
            post.Likes = await _context.LikedPosts
                .CountAsync(lp => lp.PostId == postId);

            return post;
        }

        /// <summary>
        /// Get paginated posts with profile info and relative time
        /// </summary>
        public async Task<(List<Post> Posts, int TotalCount, int TotalPages)> GetPaginatedPostsAsync(
            int page, int pageSize, string timeZone)
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            // Get total count
            var totalCount = await _dbSet.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Get paginated posts
            var posts = await _dbSet
                .OrderByDescending(p => p.PostedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Load related data
            await LoadPostDetailsAsync(posts, timeZone);

            return (posts, totalCount, totalPages);
        }

        /// <summary>
        /// Get posts by profile ID
        /// </summary>
        public async Task<List<Post>> GetPostsByProfileIdAsync(string profileId, string timeZone)
        {
            var posts = await _dbSet
                .Where(p => p.ProfileId == profileId)
                .OrderByDescending(p => p.PostedDate)
                .ToListAsync();

            await LoadPostDetailsAsync(posts, timeZone);

            return posts;
        }

        /// <summary>
        /// Check if post is liked by profile
        /// </summary>
        public async Task<bool> IsPostLikedByProfileAsync(string postId, string profileId)
        {
            return await _context.LikedPosts
                .AnyAsync(lp => lp.PostId == postId && lp.LikedByProfileId == profileId);
        }

        /// <summary>
        /// Like a post
        /// </summary>
        public async Task LikePostAsync(string postId, string profileId)
        {
            if (!await IsPostLikedByProfileAsync(postId, profileId))
            {
                var likedPost = new LikedPost
                {
                    LikedPostId = Guid.NewGuid().ToString(),
                    PostId = postId,
                    LikedByProfileId = profileId,
                    LikedDate = DateTime.Now.ToString()
                };

                await _context.LikedPosts.AddAsync(likedPost);
                await SaveAsync();
            }
        }

        /// <summary>
        /// Unlike a post
        /// </summary>
        public async Task UnlikePostAsync(string postId, string profileId)
        {
            var likedPost = await _context.LikedPosts
                .FirstOrDefaultAsync(lp => lp.PostId == postId && lp.LikedByProfileId == profileId);

            if (likedPost != null)
            {
                _context.LikedPosts.Remove(likedPost);
                await SaveAsync();
            }
        }

        /// <summary>
        /// Helper method to load post details including profile info and calculate relative time
        /// </summary>
        private async Task LoadPostDetailsAsync(List<Post> posts, string timeZone)
        {
            if (posts == null || !posts.Any())
                return;

            // Get profile IDs from posts
            var profileIds = posts.Select(p => p.ProfileId).Distinct().ToList();

            // Get profiles info in a single query
            var profiles = await _context.Profiles
                .Where(p => profileIds.Contains(p.ProfileId))
                .Select(p => new { p.ProfileId, p.UserName, p.ImageURL })
                .ToDictionaryAsync(p => p.ProfileId);

            // Get post IDs for comment counts
            var postIds = posts.Select(p => p.PostId).ToList();

            // Get comment counts in a single query
            var commentCounts = await _context.PostComments
                .Where(pc => postIds.Contains(pc.PostId))
                .GroupBy(pc => pc.PostId)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.PostId, g => g.Count);

            // Get like counts in a single query
            var likeCounts = await _context.LikedPosts
                .Where(lp => postIds.Contains(lp.PostId))
                .GroupBy(lp => lp.PostId)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.PostId, g => g.Count);

            // Apply data to posts
            foreach (var post in posts)
            {
                // Set profile info
                if (profiles.TryGetValue(post.ProfileId, out var profile))
                {
                    post.UserName = profile.UserName;
                    post.ProfileImageURL = profile.ImageURL;
                }

                // Set comment count
                post.PostCommentCount = commentCounts.TryGetValue(post.PostId, out var commentCount) ? commentCount : 0;

                // Set like count
                post.Likes = likeCounts.TryGetValue(post.PostId, out var likeCount) ? likeCount : 0;

                // Calculate relative time
                if (DateTime.TryParse(post.PostedDate, out DateTime postedDate))
                {
                    post.RelativeTime = RelativeTime.GetRelativeTime(postedDate, timeZone);
                }
            }
        }
    }

    /// <summary>
    /// Interface for Post repository
    /// </summary>
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task<Post> GetPostByIdWithDetailsAsync(string postId, string timeZone);
        Task<(List<Post> Posts, int TotalCount, int TotalPages)> GetPaginatedPostsAsync(int page, int pageSize, string timeZone);
        Task<List<Post>> GetPostsByProfileIdAsync(string profileId, string timeZone);
        Task<bool> IsPostLikedByProfileAsync(string postId, string profileId);
        Task LikePostAsync(string postId, string profileId);
        Task UnlikePostAsync(string postId, string profileId);
    }
}