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
    /// Repository for PostComment entity operations
    /// </summary>
    public class PostCommentRepository : GenericRepository<PostComment>, IPostCommentRepository
    {
        public PostCommentRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get all comments for a post
        /// </summary>
        public async Task<List<PostComment>> GetCommentsByPostIdAsync(string postId, string timeZone)
        {
            var comments = await _dbSet
                .Where(pc => pc.PostId == postId)
                .OrderByDescending(pc => pc.PostCommentDate)
                .ToListAsync();

            // Get profile IDs from comments
            var profileIds = comments.Select(c => c.PostCommentByProfileId).Distinct().ToList();

            // Get profiles in one query
            var profiles = await _context.Profiles
                .Where(p => profileIds.Contains(p.ProfileId))
                .Select(p => new { p.ProfileId, p.UserName, p.ImageURL })
                .ToDictionaryAsync(p => p.ProfileId);

            // Add profile info to comments
            foreach (var comment in comments)
            {
                if (profiles.TryGetValue(comment.PostCommentByProfileId, out var profile))
                {
                    comment.UserName = profile.UserName;
                    comment.ProfileImageURL = profile.ImageURL;
                }

                // Calculate relative time
                if (DateTime.TryParse(comment.PostCommentDate.ToString(), out var commentDate))
                {
                    // Convert DateTime to string before passing it to GetRelativeTime
                    comment.RelativeTime = RelativeTime.GetRelativeTime(commentDate, timeZone);
                }
                else
                {
                    comment.RelativeTime = "Unknown";
                }
            }

            return comments;
        }

        /// <summary>
        /// Get comment count for a post
        /// </summary>
        public async Task<int> GetCommentCountAsync(string postId)
        {
            return await _dbSet.CountAsync(pc => pc.PostId == postId);
        }

        /// <summary>
        /// Add a new comment
        /// </summary>
        public override async Task AddAsync(PostComment comment)
        {
            if (string.IsNullOrEmpty(comment.PostCommentId))
                comment.PostCommentId = Guid.NewGuid().ToString();

            comment.PostCommentDate = DateTime.Now;
            await base.AddAsync(comment);
        }
    }
}