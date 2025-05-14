using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using DataLayer.Repositories;

namespace DataLayer.Repositories
{
    /// <summary>
    /// Repository for Profile entity operations
    /// </summary>
    public class ProfileRepository : GenericRepository<Profile>, IProfileRepository
    {
        public ProfileRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get profile by ID with full details
        /// </summary>
        public override async Task<Profile> GetByIdAsync(object id)
        {
            string profileId = id.ToString();
            var profile = await _dbSet
                .Include(p => p.Setting)
                .Include(p => p.ScoutingReport)
                .FirstOrDefaultAsync(p => p.ProfileId == profileId);

            if (profile == null)
                return null;

            // Get user details
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.UserId == profile.UserId);

            if (user != null)
            {
                profile.FirstName = user.FirstName;
                profile.LastName = user.LastName;
                profile.Email = user.Email;
            }

            // Get follower/following counts
            profile.FollowersCount = (await GetFollowerCountAsync(profileId)).ToString();
            profile.FollowingCount = (await GetFollowingCountAsync(profileId)).ToString();

            // Calculate game statistics
            await CalculateGameStatisticsAsync(profile);

            return profile;
        }

        /// <summary>
        /// Get all profiles with basic info
        /// </summary>
        public override async Task<List<Profile>> GetAllAsync()
        {
            var profiles = await _dbSet
                .Where(p => p.UserId != null)
                .ToListAsync();

            // Get all user IDs
            var userIds = profiles.Select(p => p.UserId).ToList();

            // Get all users in one query
            var users = await _context.User
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId);

            // Get all profile IDs
            var profileIds = profiles.Select(p => p.ProfileId).ToList();

            // Get follower counts in one query
            var followerCounts = await _context.Followers
                .Where(f => profileIds.Contains(f.FollowerProfileId))
                .GroupBy(f => f.FollowerProfileId)
                .Select(g => new { ProfileId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.ProfileId, g => g.Count);

            // Get following counts in one query
            var followingCounts = await _context.Following
                .Where(f => profileIds.Contains(f.ProfileId))
                .GroupBy(f => f.ProfileId)
                .Select(g => new { ProfileId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.ProfileId, g => g.Count);

            // Apply user info and counts to profiles
            foreach (var profile in profiles)
            {
                if (users.TryGetValue(profile.UserId, out var user))
                {
                    profile.FirstName = user.FirstName;
                    profile.LastName = user.LastName;
                    profile.FullName = $"{user.FirstName} {user.LastName}";
                }

                profile.FollowersCount = followerCounts.TryGetValue(profile.ProfileId, out var followerCount)
                    ? followerCount.ToString() : "0";

                profile.FollowingCount = followingCounts.TryGetValue(profile.ProfileId, out var followingCount)
                    ? followingCount.ToString() : "0";
            }

            return profiles;
        }

        /// <summary>
        /// Get profiles following a user
        /// </summary>
        public async Task<List<Profile>> GetFollowingProfilesAsync(string profileId)
        {
            return await _context.Following
                .Where(f => f.ProfileId == profileId)
                .Join(_dbSet,
                    following => following.FollowingProfileId,
                    profile => profile.ProfileId,
                    (following, profile) => profile)
                .ToListAsync();
        }

        /// <summary>
        /// Get profiles that follow a user
        /// </summary>
        public async Task<List<Profile>> GetFollowerProfilesAsync(string profileId)
        {
            return await _context.Followers
                .Where(f => f.ProfileId == profileId)
                .Join(_dbSet,
                    follower => follower.FollowerProfileId,
                    profile => profile.ProfileId,
                    (follower, profile) => profile)
                .ToListAsync();
        }

        /// <summary>
        /// Check if username is available
        /// </summary>
        public async Task<bool> IsUserNameAvailableAsync(string userName)
        {
            return !await _dbSet.AnyAsync(p => p.UserName == userName);
        }

        /// <summary>
        /// Update profile settings
        /// </summary>
        public async Task UpdateSettingsAsync(Setting setting)
        {
            var existingSetting = await _context.Setting
                .FirstOrDefaultAsync(s => s.ProfileId == setting.ProfileId);

            if (existingSetting != null)
            {
                existingSetting.AllowComments = setting.AllowComments;
                existingSetting.ShowGameHistory = setting.ShowGameHistory;
                existingSetting.AllowEmailNotification = setting.AllowEmailNotification;

                _context.Setting.Update(existingSetting);
            }
            else
            {
                setting.SettingId = Guid.NewGuid().ToString();
                await _context.Setting.AddAsync(setting);
            }

            await SaveAsync();
        }

        /// <summary>
        /// Helper method to get follower count
        /// </summary>
        private async Task<int> GetFollowerCountAsync(string profileId)
        {
            return await _context.Followers
                .CountAsync(f => f.FollowerProfileId == profileId);
        }

        /// <summary>
        /// Helper method to get following count
        /// </summary>
        private async Task<int> GetFollowingCountAsync(string profileId)
        {
            return await _context.Following
                .CountAsync(f => f.ProfileId == profileId);
        }

        /// <summary>
        /// Helper method to calculate game statistics
        /// </summary>
        private async Task CalculateGameStatisticsAsync(Profile profile)
        {
            var games = await _context.Games.ToListAsync();

            // Count wins and losses
            int wins = 0;
            int losses = 0;

            foreach (var game in games)
            {
                if (string.IsNullOrEmpty(game.WinProfileIdsStatusString))
                    continue;

                // Check if profile is in win list
                if (game.WinProfileIdsStatusString.Contains(profile.ProfileId))
                    wins++;
                // Check if profile is in lose list
                else if (game.LoseProfileIdsStatusString?.Contains(profile.ProfileId) == true)
                    losses++;
            }

            // Set statistics
            profile.TotalGames = (wins + losses).ToString();
            profile.TotalWins = wins;
            profile.TotalLosses = losses;

            // Calculate win percentage
            double winPercentage = (wins + losses) > 0
                ? (double)wins / (wins + losses) * 100
                : 0;

            profile.WinPercentage = winPercentage.ToString("F2");
        }
    }
}