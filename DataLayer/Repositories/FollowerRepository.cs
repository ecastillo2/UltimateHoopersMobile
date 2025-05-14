using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for Follower entity operations
    /// </summary>
    public class FollowerRepository : GenericRepository<Follower>, IFollowerRepository
    {
        public FollowerRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Check if one profile is following another
        /// </summary>
        public async Task<bool> IsFollowingAsync(string profileId, string followerProfileId)
        {
            return await _dbSet.AnyAsync(f =>
                f.ProfileId == profileId &&
                f.FollowerProfileId == followerProfileId);
        }

        /// <summary>
        /// Get the count of followers for a profile
        /// </summary>
        public async Task<int> GetFollowerCountAsync(string profileId)
        {
            return await _dbSet.CountAsync(f => f.ProfileId == profileId);
        }

        /// <summary>
        /// Get profiles that follow a user
        /// </summary>
        public async Task<List<Profile>> GetFollowerProfilesAsync(string profileId)
        {
            return await _dbSet
                .Where(f => f.ProfileId == profileId)
                .Join(_context.Profile,
                    follower => follower.FollowerProfileId,
                    profile => profile.ProfileId,
                    (follower, profile) => profile)
                .ToListAsync();
        }

        /// <summary>
        /// Add a follower
        /// </summary>
        public async Task FollowAsync(string profileId, string followerProfileId)
        {
            // Check if the follower relationship already exists
            if (await IsFollowingAsync(profileId, followerProfileId))
                return;

            // Create new follower record
            var follower = new Follower
            {
                FollowerId = Guid.NewGuid().ToString(),
                ProfileId = profileId,
                FollowerProfileId = followerProfileId,
                FollowDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Status = "Active",
                CreatedDate = DateTime.Now
            };

            await _dbSet.AddAsync(follower);
            await SaveAsync();
        }

        /// <summary>
        /// Remove a follower
        /// </summary>
        public async Task UnfollowAsync(string profileId, string followerProfileId)
        {
            var follower = await _dbSet.FirstOrDefaultAsync(f =>
                f.ProfileId == profileId &&
                f.FollowerProfileId == followerProfileId);

            if (follower != null)
            {
                _dbSet.Remove(follower);
                await SaveAsync();
            }
        }
    }
}