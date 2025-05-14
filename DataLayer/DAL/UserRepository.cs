using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Common;
using Domain;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataLayer.DAL
{
    /// <summary>
    /// Repository for User-related data operations with optimized database access
    /// </summary>
    public class UserRepository : IUserRepository, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly HUDBContext _context;
        private readonly EmailMessages _emailMessages;

        /// <summary>
        /// Constructor for UserRepository
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="configuration">Application configuration</param>
        public UserRepository(HUDBContext context, IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _emailMessages = new EmailMessages(_configuration);
        }

        /// <summary>
        /// Delete User and all related data with optimized database operations
        /// </summary>
        /// <param name="userId">ID of the user to delete</param>
        public async Task DeleteUser(string userId)
        {
            // Start a transaction to ensure consistency
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Fetch the user with a single query
                var user = await _context.User
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    throw new Exception("User not found");
                }

                // Get the profile in a single query
                var profile = await _context.Profile
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (profile == null)
                {
                    throw new Exception("Profile not found for user");
                }

                // Collect all related entities using the profile ID
                // Use batch operations where possible for better performance
                await DeleteUserRelatedData(profile.ProfileId, user.UserId);

                // Delete the user and profile last (after dependencies)
                _context.Profile.Remove(profile);
                _context.User.Remove(user);

                // Save all changes within the transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // Re-throw to let caller handle
            }
        }

        /// <summary>
        /// Helper method to delete all data related to a user
        /// </summary>
        private async Task DeleteUserRelatedData(string profileId, string userId)
        {
            // Execute removal of related entities in parallel where possible
            // Using separate methods for better organization

            // Remove social connections
            var followersTask = RemoveFollowers(profileId);
            var followingTask = RemoveFollowing(profileId);

            // Remove content
            var postsTask = RemovePosts(profileId);
            var commentsTask = RemoveComments(userId, profileId);
            var historyTask = RemoveHistory(profileId);

            // Remove interactions
            var likesTask = RemoveLikes(profileId);
            var savesTask = RemoveSaves(profileId);
            var ratingsTask = RemoveRatings(profileId);
            var playerCommentsTask = RemovePlayerComments(profileId);

            // Wait for all tasks to complete
            await Task.WhenAll(
                followersTask, followingTask,
                postsTask, commentsTask, historyTask,
                likesTask, savesTask, ratingsTask, playerCommentsTask
            );
        }

        private async Task RemoveFollowers(string profileId)
        {
            var followers = await _context.Follower
                .Where(f => f.ProfileId == profileId)
                .ToListAsync();

            _context.Follower.RemoveRange(followers);
        }

        private async Task RemoveFollowing(string profileId)
        {
            var following = await _context.Following
                .Where(f => f.ProfileId == profileId)
                .ToListAsync();

            _context.Following.RemoveRange(following);
        }

        private async Task RemovePosts(string profileId)
        {
            var posts = await _context.Post
                .Where(p => p.ProfileId == profileId)
                .ToListAsync();

            _context.Post.RemoveRange(posts);
        }

        private async Task RemoveComments(string userId, string profileId)
        {
            var comments = await _context.Comment
                .Where(c => c.CommentByUserId == userId)
                .ToListAsync();

            _context.Comment.RemoveRange(comments);
        }

        private async Task RemoveHistory(string profileId)
        {
            var history = await _context.History
                .Where(h => h.ProfileId == profileId)
                .ToListAsync();

            _context.History.RemoveRange(history);
        }

        private async Task RemoveLikes(string profileId)
        {
            var likes = await _context.LikedPost
                .Where(lp => lp.LikedByProfileId == profileId)
                .ToListAsync();

            _context.LikedPost.RemoveRange(likes);
        }

        private async Task RemoveSaves(string profileId)
        {
            var saves = await _context.SavedPost
                .Where(sp => sp.SavedByProfileId == profileId)
                .ToListAsync();

            _context.SavedPost.RemoveRange(saves);
        }

        private async Task RemoveRatings(string profileId)
        {
            var ratings = await _context.Rating
                .Where(r => r.ProfileId == profileId)
                .ToListAsync();

            _context.Rating.RemoveRange(ratings);
        }

        private async Task RemovePlayerComments(string profileId)
        {
            var playerComments = await _context.PlayerComment
                .Where(pc => pc.ProfileId == profileId)
                .ToListAsync();

            _context.PlayerComment.RemoveRange(playerComments);
        }

        /// <summary>
        /// Get User by ID with optimized queries for related data
        /// </summary>
        /// <param name="userId">ID of the user to retrieve</param>
        /// <returns>User object with related data</returns>
        public async Task<User> GetUserById(string userId)
        {
            try
            {
                // Fetch the user with a single query
                var user = await _context.User
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return null;
                }

                // Run parallel queries to fetch related data for better performance
                var socialDataTask = FetchUserSocialData(userId);
                var profileDataTask = FetchUserProfileData(userId);

                // Wait for both tasks to complete
                await Task.WhenAll(socialDataTask, profileDataTask);

                // Get results
                var socialData = await socialDataTask;
                var profileData = await profileDataTask;

                // Assign data to user object
                user.Followers = socialData.followers;
                user.Following = socialData.following;
                user.Profile = profileData;

                if (user.Profile != null)
                {
                    // Run additional queries in parallel for profile data
                    await EnrichProfileData(user.Profile);
                }

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserById: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Fetch user's social connections (followers and following)
        /// </summary>
        private async Task<(List<User> followers, List<User> following)> FetchUserSocialData(string userId)
        {
            // Run both queries in parallel for better performance
            var followersTask = _context.Follower
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .Join(
                    _context.User,
                    follower => follower.FollowerUserId,
                    user => user.UserId,
                    (follower, user) => user
                )
                .ToListAsync();

            var followingTask = _context.Following
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .Join(
                    _context.User,
                    following => following.FollowingUserId,
                    user => user.UserId,
                    (following, user) => user
                )
                .ToListAsync();

            // Wait for both to complete
            await Task.WhenAll(followersTask, followingTask);

            // Return results
            return (
                await followersTask ?? new List<User>(),
                await followingTask ?? new List<User>()
            );
        }

        /// <summary>
        /// Fetch user's profile data
        /// </summary>
        private async Task<Profile> FetchUserProfileData(string userId)
        {
            var profile = await _context.Profile
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId) ?? new Profile();

            return profile;
        }

        /// <summary>
        /// Enrich profile with additional data (notifications, settings, stats)
        /// </summary>
        private async Task EnrichProfileData(Profile profile)
        {
            // Run multiple queries in parallel for better performance
            var notificationsTask = _context.Notification
                .AsNoTracking()
                .Where(n => n.ProfileId == profile.ProfileId)
                .ToListAsync();

            var settingTask = _context.Setting
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ProfileId == profile.ProfileId);

            var ratingCountTask = _context.Rating
                .AsNoTracking()
                .Where(r => r.ProfileId == profile.ProfileId)
                .CountAsync();

            var followersCountTask = _context.Follower
                .AsNoTracking()
                .Where(f => f.FollowerProfileId == profile.ProfileId)
                .CountAsync();

            var followingCountTask = _context.Following
                .AsNoTracking()
                .Where(f => f.ProfileId == profile.ProfileId)
                .CountAsync();

            // Wait for all tasks to complete
            await Task.WhenAll(
                notificationsTask,
                settingTask,
                ratingCountTask,
                followersCountTask,
                followingCountTask
            );

            // Assign results to profile
            profile.NotificationList = await notificationsTask ?? new List<Notification>();
            profile.Setting = await settingTask ?? new Setting();
            profile.RatedCount = (await ratingCountTask).ToString();
            profile.FollowersCount = (await followersCountTask).ToString();
            profile.FollowingCount = (await followingCountTask).ToString();

            // Calculate game statistics in parallel
            var statsTask = CalculateGameStatistics(profile.ProfileId);
            await statsTask;

            var stats = await statsTask;
            profile.TotalGames = stats.totalGames.ToString();
            profile.WinPercentage = stats.winPercentage.ToString("F2");
            profile.TotalWins = stats.totalWins;
            profile.TotalLosses = stats.totalLosses;
        }

        /// <summary>
        /// Get all admin users
        /// </summary>
        /// <returns>List of admin users</returns>
        public async Task<List<User>> GetAdminUsers()
        {
            try
            {
                return await _context.User
                    .AsNoTracking()
                    .Where(u => u.AccessLevel == "Admin")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching admin users: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all users with their related data
        /// </summary>
        /// <returns>List of all users</returns>
        public async Task<List<User>> GetUsers()
        {
            try
            {
                // Fetch basic user data first
                var users = await _context.User
                    .AsNoTracking()
                    .ToListAsync();

                // For each user, fetch related data (consider batching for large datasets)
                foreach (var user in users)
                {
                    // Get detailed user data, reusing the existing method
                    var detailedUser = await GetUserById(user.UserId);

                    // Copy properties from detailed user to the current user in the list
                    if (detailedUser != null)
                    {
                        user.Followers = detailedUser.Followers;
                        user.Following = detailedUser.Following;
                        user.Posts = detailedUser.Posts;
                        user.Profile = detailedUser.Profile;
                    }
                }

                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all users: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Insert a new user with all related entities
        /// </summary>
        /// <param name="user">User to insert</param>
        public async Task InsertUser(User user)
        {
            // Use a transaction to ensure consistency
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Initialize profile if needed
                if (user.Profile == null)
                {
                    user.Profile = new Profile();
                }

                // Create related entities
                var setting = new Setting
                {
                    SettingId = Guid.NewGuid().ToString(),
                    AllowComments = true,
                    ShowGameHistory = true,
                    AllowEmailNotification = true
                };

                var criteria = new Criteria
                {
                    CriteriaId = Guid.NewGuid().ToString(),
                    CompetionLevel = string.Empty,
                    Frequency = string.Empty
                };

                var scoutingReport = new ScoutingReport
                {
                    ScoutingReportId = Guid.NewGuid().ToString(),
                    CreatedDate = DateTime.Now,
                    Status = "Active"
                };

                // Set user properties
                user.UserId = Guid.NewGuid().ToString();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.SignUpDate = DateTime.Now.ToString();
                user.AccessLevel = "Standard";
                user.Status = "Active";

                // Set profile properties
                user.Profile.UserId = user.UserId;
                user.Profile.UserName = user.UserName;
                user.Profile.StarRating = "1";
                user.Profile.Ranking = "1";
                user.Profile.Points = 2;
                user.Profile.TopRecord = false;
                user.Profile.Position = string.Empty;
                user.Profile.Height = string.Empty;
                user.Profile.PlayerNumber = UniqueIdNumber.GenerateSixDigit();
                user.Profile.ProfileId = Guid.NewGuid().ToString();
                user.Profile.ImageURL = "https://uhblobstorageaccount.blob.core.windows.net/profileimage/" + user.Profile.ProfileId + ".webp";

                // Set related entity properties
                setting.ProfileId = user.Profile.ProfileId;
                criteria.ProfileId = user.Profile.ProfileId;
                scoutingReport.ProfileId = user.Profile.ProfileId.ToString();

                // Add all entities to context
                await _context.User.AddAsync(user);
                await _context.Profile.AddAsync(user.Profile);
                await _context.Setting.AddAsync(setting);
                await _context.Criteria.AddAsync(criteria);
                await _context.ScoutingReport.AddAsync(scoutingReport);

                // Save changes
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                // Send welcome email
                await _emailMessages.SignUpEmail(user);
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await transaction.RollbackAsync();
                Console.WriteLine($"Error inserting user: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update basic user information
        /// </summary>
        /// <param name="user">User with updated information</param>
        public async Task UpdateUser(User user)
        {
            try
            {
                _context.Entry(user).State = EntityState.Modified;
                await Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update user's email
        /// </summary>
        /// <param name="model">User with updated email</param>
        public async Task UpdateUserEmail(User model)
        {
            try
            {
                var user = await _context.User
                    .FirstOrDefaultAsync(u => u.UserId == model.UserId);

                if (user != null)
                {
                    user.Email = model.Email;
                    await Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user email: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update user's name
        /// </summary>
        /// <param name="model">User with updated name</param>
        public async Task UpdateUserName(User model)
        {
            try
            {
                var user = await _context.User
                    .FirstOrDefaultAsync(u => u.UserId == model.UserId);

                if (user != null)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    await Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating username: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update user's segment ID
        /// </summary>
        /// <param name="model">User with updated segment ID</param>
        public async Task UpdateSeg(User model)
        {
            try
            {
                var user = await _context.User
                    .FirstOrDefaultAsync(u => u.UserId == model.UserId);

                if (user != null)
                {
                    user.SegId = model.SegId;
                    await Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating segment ID: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update user's subscription ID
        /// </summary>
        /// <param name="model">User with updated subscription ID</param>
        public async Task UpdateSubId(User model)
        {
            try
            {
                var user = await _context.User
                    .FirstOrDefaultAsync(u => u.UserId == model.UserId);

                if (user != null)
                {
                    user.SubId = model.SubId;
                    await Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating subscription ID: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update user's password
        /// </summary>
        /// <param name="model">User with updated password</param>
        public async Task UpdatePassword(User model)
        {
            try
            {
                var user = await _context.User
                    .FirstOrDefaultAsync(u => u.UserId == model.UserId);

                if (user != null)
                {
                    user.Password = model.Password;
                    await Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating password: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update player's name
        /// </summary>
        /// <param name="model">User with updated player name</param>
        public async Task UpdatePlayerName(User model)
        {
            try
            {
                var user = await _context.User
                    .FirstOrDefaultAsync(u => u.UserId == model.UserId);

                if (user != null)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    await Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating player name: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update user's name
        /// </summary>
        /// <param name="model">User with updated name</param>
        public async Task UpdateName(User model)
        {
            try
            {
                var user = await _context.User
                    .FirstOrDefaultAsync(u => u.UserId == model.UserId);

                if (user != null)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    await Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating name: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Reset user's password
        /// </summary>
        /// <param name="user">User with new password</param>
        public async Task PasswordReset(User user)
        {
            try
            {
                var existingUser = await _context.User
                    .FirstOrDefaultAsync(u => u.UserId == user.UserId);

                if (existingUser != null)
                {
                    existingUser.Password = user.Password;
                    existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
                    await Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting password: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Generate a new password for a user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        public async Task GeneratePassword(string userId)
        {
            try
            {
                // Generate temporary password
                int length = 8;
                string tempPassword = GenerateNewPassword.GenerateRandomPassword(length);

                // Update user's password
                var user = await _context.User
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user != null)
                {
                    user.Password = tempPassword;
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);

                    // Send email with new password
                    var emailMessages = new EmailMessages(_configuration);
                    await emailMessages.NewGeneratedPassword(user);

                    await Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating password: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update user's last login date
        /// </summary>
        /// <param name="userId">ID of the user</param>
        public async Task UpdateLastLoginDate(string userId)
        {
            try
            {
                var user = await _context.User
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user != null)
                {
                    user.LastLoginDate = DateTime.Now.ToString();
                    await Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating last login date: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Deactivate a user's account
        /// </summary>
        /// <param name="userId">ID of the user</param>
        public async Task UnActivateAccount(string userId)
        {
            try
            {
                var user = await _context.User
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user != null)
                {
                    user.Status = "InActive";
                    await Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deactivating account: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if an email is available (not already in use)
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>True if the email is already in use</returns>
        public async Task<bool> IsEmailAvailable(string email)
        {
            try
            {
                return await _context.User
                    .AnyAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking email availability: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get a user by their email address
        /// </summary>
        /// <param name="email">Email to search for</param>
        /// <returns>User with the specified email</returns>
        public async Task<User> GetUserByEmail(string email)
        {
            try
            {
                return await _context.User
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by email: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Reset a forgotten password
        /// </summary>
        /// <param name="user">User with new password</param>
        public async Task ResetForgottenPassword(User user)
        {
            try
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
                _context.Entry(user).State = EntityState.Modified;
                await Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting forgotten password: {ex.Message}");
                throw;
            }
        }

        #region Followers

        /// <summary>
        /// Get all users who follow a specific user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>List of followers</returns>
        public async Task<List<User>> GetUserFollowersByUserId(string userId)
        {
            try
            {
                return await _context.Follower
                    .AsNoTracking()
                    .Where(f => f.UserId == userId)
                    .Join(
                        _context.User,
                        follower => follower.FollowerUserId,
                        user => user.UserId,
                        (follower, user) => user
                    )
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting followers: {ex.Message}");
                return new List<User>();
            }
        }

        /// <summary>
        /// Get all users that a specific user follows
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>List of users being followed</returns>
        public async Task<List<User>> GetUserFollowingByUserId(string userId)
        {
            try
            {
                return await _context.Following
                    .AsNoTracking()
                    .Where(f => f.UserId == userId)
                    .Join(
                        _context.User,
                        following => following.FollowingUserId,
                        user => user.UserId,
                        (following, user) => user
                    )
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting following: {ex.Message}");
                return new List<User>();
            }
        }

        /// <summary>
        /// Start following another user
        /// </summary>
        /// <param name="following">Following relationship to create</param>
        public async Task StartFollowingUserId(Following following)
        {
            try
            {
                following.FollowingId = Guid.NewGuid().ToString();
                await _context.Following.AddAsync(following);
                await Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting follow: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Stop following another user
        /// </summary>
        /// <param name="following">Following relationship to remove</param>
        public async Task StopFollowingUserId(Following following)
        {
            try
            {
                var existingFollowing = await _context.Following
                    .FirstOrDefaultAsync(f => f.FollowingUserId == following.FollowingUserId);

                if (existingFollowing != null)
                {
                    _context.Following.Remove(existingFollowing);
                    await Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping follow: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Game Statistics

        /// <summary>
        /// Calculate game statistics for a profile
        /// </summary>
        /// <param name="profileId">ID of the profile</param>
        /// <returns>Game statistics</returns>
        private async Task<(int totalGames, double winPercentage, int totalWins, int totalLosses)> CalculateGameStatistics(string profileId)
        {
            try
            {
                // Fetch all games in a single query
                var games = await _context.Game
                    .AsNoTracking()
                    .ToListAsync();

                // Calculate wins and losses
                int wins = games.Count(game =>
                    !string.IsNullOrEmpty(game.WinProfileIdsStatusString) &&
                    game.WinProfileIdsStatusString.Contains(profileId));

                int losses = games.Count(game =>
                    !string.IsNullOrEmpty(game.LoseProfileIdsStatusString) &&
                    game.LoseProfileIdsStatusString.Contains(profileId));

                int totalGames = wins + losses;
                double winPercentage = totalGames > 0 ? (double)wins / totalGames * 100 : 0;

                return (totalGames, winPercentage, wins, losses);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating game statistics: {ex.Message}");
                return (0, 0, 0, 0);
            }
        }

        #endregion

        /// <summary>
        /// Save changes to the database
        /// </summary>
        /// <returns>Number of affected rows</returns>
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Dispose of resources
        /// </summary>
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}