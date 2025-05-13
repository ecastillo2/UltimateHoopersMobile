using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for Following entity operations
    /// </summary>
    public class FollowingRepository : GenericRepository<Following>, IFollowingRepository
    {
        public FollowingRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Check if one profile is following another
        /// </summary>
        public async Task<bool> IsFollowingAsync(string profileId, string followingProfileId)
        {
            return await _dbSet.AnyAsync(f =>
                f.ProfileId == profileId &&
                f.FollowingProfileId == followingProfileId);
        }

        /// <summary>
        /// Get the count of profiles a user is following
        /// </summary>
        public async Task<int> GetFollowingCountAsync(string profileId)
        {
            return await _dbSet.CountAsync(f => f.ProfileId == profileId);
        }

        /// <summary>
        /// Get profiles that a user is following
        /// </summary>
        public async Task<List<Profile>> GetFollowingProfilesAsync(string profileId)
        {
            return await _dbSet
                .Where(f => f.ProfileId == profileId)
                .Join(_context.Profiles,
                    following => following.FollowingProfileId,
                    profile => profile.ProfileId,
                    (following, profile) => profile)
                .ToListAsync();
        }

        /// <summary>
        /// Follow a profile
        /// </summary>
        public async Task FollowAsync(string profileId, string followingProfileId)
        {
            // Check if the following relationship already exists
            if (await IsFollowingAsync(profileId, followingProfileId))
                return;

            // Create new following record
            var following = new Following
            {
                FollowingId = Guid.NewGuid().ToString(),
                ProfileId = profileId,
                FollowingProfileId = followingProfileId,
                FollowDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Status = "Active",
                CreatedDate = DateTime.Now
            };

            await _dbSet.AddAsync(following);
            await SaveAsync();
        }

        /// <summary>
        /// Unfollow a profile
        /// </summary>
        public async Task UnfollowAsync(string profileId, string followingProfileId)
        {
            var following = await _dbSet.FirstOrDefaultAsync(f =>
                f.ProfileId == profileId &&
                f.FollowingProfileId == followingProfileId);

            if (following != null)
            {
                _dbSet.Remove(following);
                await SaveAsync();
            }
        }
    }
}