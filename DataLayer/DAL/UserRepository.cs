using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Common;
using Domain;
using Messages;

namespace DataLayer.DAL
{
    public class UserRepository : IUserRepository, IDisposable
    {
        private readonly IConfiguration _configuration;
        private HUDBContext _context;
        private EmailMessages _emailMessages;

        public UserRepository(HUDBContext context, IConfiguration configuration)
        {
            this._context = context;
            this._configuration = configuration;
            
        }

        /// <summary>
        /// Delete User
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task DeleteUser(string userId)
        {
            // Find the user by UserId
            var user = await _context.User
                .Where(u => u.UserId == userId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                // Handle the case where the user is not found
                throw new Exception("User not found");
            }

            // Remove or update references to UserId in related tables
            var profile = _context.Profile.Where(p => p.UserId == userId).FirstOrDefault();

            var privateRuns = _context.PrivateRun.Where(pr => pr.ProfileId == profile.ProfileId);

            var privateRunInvites = _context.PrivateRunInvite.Where(pri => pri.ProfileId == profile.ProfileId);

            var followers = _context.Follower.Where(pri => pri.ProfileId == profile.ProfileId).ToList();
            var followeing = _context.Following.Where(pri => pri.ProfileId == profile.ProfileId).ToList();
            var posts = _context.Post.Where(pri => pri.ProfileId == profile.ProfileId).ToList(); ; 
            var history = _context.History.Where(pri => pri.ProfileId == profile.ProfileId).ToList();
            var rating = _context.Rating.Where(pri => pri.ProfileId == profile.ProfileId).ToList();
            var comments = _context.Comment.Where(pri => pri.CommentByUserId == profile.UserId).ToList();
            var likedPost = _context.LikedPost.Where(pri => pri.LikedByProfileId == profile.ProfileId).ToList();
            var savedPosts = _context.SavedPost.Where(pri => pri.SavedByProfileId == profile.ProfileId).ToList();
            var playerComments= _context.PlayerComment.Where(pri => pri.ProfileId == profile.ProfileId).ToList();
            //var squad = _context.Squad.Where(pri => pri.ProfileId == profiles.ProfileId);

            // Remove the user
            _context.User.Remove(user);
            _context.Follower.RemoveRange(followers);
            _context.Following.RemoveRange(followeing);
            _context.Profile.Remove(profile);
            _context.Post.RemoveRange(posts);
            _context.History.RemoveRange(history);
            _context.Rating.RemoveRange(rating);
            _context.Comment.RemoveRange(comments);
            _context.LikedPost.RemoveRange(likedPost);
            _context.SavedPost.RemoveRange(savedPosts);
            _context.PlayerComment.RemoveRange(playerComments);

            // Save changes to the database
            await _context.SaveChangesAsync();
        }
       
        /// <summary>
        /// GetUserById
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<User> GetUserById(string userId)
        {
            using (var context = _context)
            {
                // Fetch the user based on userId
                var model = await (from u in _context.User
                                   where u.UserId == userId
                                   select u).FirstOrDefaultAsync();

                // Return null if the user doesn't exist
                if (model == null)
                {
                    return null;
                }

                // Query the Follower table to get all users who follow the given user, or empty list if none found
                model.Followers = await (from f in _context.Follower
                                         join u in _context.User on f.FollowerUserId equals u.UserId
                                         where f.UserId == userId
                                         select u).ToListAsync() ?? new List<User>();

                // Query the Following table to get all users the given user is following, or empty list if none found
                model.Following = await (from f in _context.Following
                                         join u in _context.User on f.FollowingUserId equals u.UserId
                                         where f.UserId == userId
                                         select u).ToListAsync() ?? new List<User>();

                



                // Query the Post table to get all posts the user has created, or empty list if none found
                model.Profile = await (from p in _context.Profile
                                     where p.UserId == userId
                                     select p).FirstOrDefaultAsync() ?? new Profile();

                // Query the Rating table for the specified ProfileId
                model.Profile.NotificationList = await (from notification in _context.Notification
                                                        where notification.ProfileId == model.Profile.ProfileId
                                                        select notification).ToListAsync() ?? new List<Notification>();

                // Query the Post table to get all posts the user has created, or empty list if none found
                model.Profile.Setting = await (from p in _context.Setting
                                       where p.ProfileId == model.Profile.ProfileId
                                       select p).FirstOrDefaultAsync() ?? new Setting();

                //// Query the Rating table for the specified ProfileId
                var count = await (from rating in context.Rating
                                   where rating.ProfileId == model.Profile.ProfileId
                                   select rating.StarRating).ToListAsync();


                model.Profile.RatedCount = count.Count().ToString();


                // Get followers count
                var followersCount = await context.Follower
                    .Where(f => f.FollowerProfileId == model.Profile.ProfileId)
                    .CountAsync();

                // Get following count
                var followingCount = await context.Following
                    .Where(f => f.ProfileId == model.Profile.ProfileId)
                    .CountAsync();

                // Assign counts to profile properties
                model.Profile.FollowersCount = followersCount > 0 ? followersCount.ToString() : "0";
                model.Profile.FollowingCount = followingCount > 0 ? followingCount.ToString() : "0";


                // Calculate Win/Loss statistics
                var totalGames = await GetTotalGames(model.Profile.ProfileId); // Total games played
                var winPercentage = await GetWinPercentage(model.Profile.ProfileId); // Win percentage
                model.Profile.TotalGames = totalGames.ToString();
                model.Profile.WinPercentage = winPercentage.ToString("F2"); // Formatting the percentage to 2 decimal places


                var data = await GetWinPercentageAndTotalLosses(model.Profile.ProfileId);

                model.Profile.TotalWins = data.totalWins;
                model.Profile.TotalLosses = data.totalLosses;

                return model;
            }
        }

        /// <summary>
        /// GetUserById
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<User>> GetAdminUsers()
        {
            using (var context = _context)
            {
                // Fetch the user based on userId
                var model = await (from u in _context.User
                                   where u.AccessLevel == "Admin"
                                   select u).ToListAsync();

                
                return model;
            }
        }

        /// <summary>
        /// GetUsers
        /// </summary>
        /// <returns></returns>
        public async Task<List<User>> GetUsers()
        {
            var list = await _context.User.ToListAsync();


            foreach(var item in list)
            {
                using (var context = _context)
                {
                    // Fetch the user based on userId
                    var model = await (from u in _context.User
                                       where u.UserId == item.UserId
                                       select u).FirstOrDefaultAsync();

                    // Return null if the user doesn't exist
                    if (model == null)
                    {
                        return null;
                    }

                    // Query the Follower table to get all users who follow the given user, or empty list if none found
                    model.Followers = await (from f in _context.Follower
                                             join u in _context.User on f.FollowerUserId equals u.UserId
                                             where f.UserId == item.UserId
                                             select u).ToListAsync() ?? new List<User>();

                    // Query the Following table to get all users the given user is following, or empty list if none found
                    model.Following = await (from f in _context.Following
                                             join u in _context.User on f.FollowingUserId equals u.UserId
                                             where f.UserId == item.UserId
                                             select u).ToListAsync() ?? new List<User>();

                    // Query the Post table to get all posts the user has created, or empty list if none found
                    model.Posts = await (from p in _context.Post
                                         where p.UserId == item.UserId
                                         select p).ToListAsync() ?? new List<Post>();

                    // Query the Post table to get all posts the user has created, or empty list if none found
                    model.Profile = await (from p in _context.Profile
                                           where p.UserId == item.UserId
                                           select p).FirstOrDefaultAsync() ?? new Profile();

                }
            }

            return list;
        }

        /// <summary>
        /// Insert User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task InsertUser(User user)
        {
            try
            {
                // Check if Profile is initialized
                if (user.Profile == null)
                {
                    user.Profile = new Profile(); // Initialize if null
                }

                Setting setting = new Setting();
                Criteria criteria = new Criteria();
                ScoutingReport scoutingReport = new ScoutingReport();


                //User
                user.UserId = Guid.NewGuid().ToString();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.SignUpDate = DateTime.Now.ToString(); // Store as DateTime
                user.AccessLevel = "Standard";
                user.Status = "Active";

                //Profile
                user.Profile.UserId = user.UserId;
                user.Profile.UserName = user.UserName;
                user.Profile.StarRating = "1";
                user.Profile.Ranking = "1";
                user.Profile.Points = 2;
                user.Profile.TopRecord = false;
                user.Profile.Position = string.Empty;
                user.Profile.Height = string.Empty;
                user.Profile.Height = string.Empty;
                user.Profile.PlayerNumber = UniqueIdNumber.GenerateSixDigit();
                user.Profile.ProfileId = Guid.NewGuid().ToString();
                user.Profile.ImageURL = "https://uhblobstorageaccount.blob.core.windows.net/profileimage/" + user.Profile.ProfileId + ".webp"; // Assuming you're using the UserId as the ImageURL

                //Setting
                setting.SettingId = Guid.NewGuid().ToString();
                setting.ProfileId = user.Profile.ProfileId;
                setting.AllowComments = true;
                setting.ShowGameHistory = true;
                setting.AllowEmailNotification = true;

                //Criteria
                criteria.CriteriaId = Guid.NewGuid().ToString();
                criteria.ProfileId = user.Profile.ProfileId;
                criteria.CompetionLevel = string.Empty;
                criteria.Frequency = string.Empty;
                

                // scoutingReport
                scoutingReport.ScoutingReportId = Guid.NewGuid().ToString();
                scoutingReport.ProfileId = user.Profile.ProfileId.ToString();
                scoutingReport.CreatedDate = DateTime.Now;
                scoutingReport.Status = "Active";

                // Add user and profile to the context
                await _context.User.AddAsync(user);
                await _context.Criteria.AddAsync(criteria);
                await _context.Profile.AddAsync(user.Profile);
                await _context.Setting.AddAsync(setting);
                await _context.ScoutingReport.AddAsync(scoutingReport);

                // Save changes
                await Save();

                await _emailMessages.SignUpEmail(user);
                

            }
            catch (Exception ex)
            {
                // Log the exception

            }
        }

        /// <summary>
        /// Update User Info
        /// </summary>
        /// <param name="user"></param>
        public async Task UpdateUser(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await Save();
        }

        /// <summary>
        /// Update UserEmail
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateUserEmail(User model)
        {
            using (var context = _context)
            {
                var existingItem = context.User.Where(s => s.UserId == model.UserId).FirstOrDefault<User>();

                if (existingItem != null)
                {
                    existingItem.Email = model.Email;

                    context.User.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Update UserName
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateUserName(User model)
        {
            using (var context = _context)
            {
                var existingItem = context.User.Where(s => s.UserId == model.UserId).FirstOrDefault<User>();

                if (existingItem != null)
                {
                    existingItem.FirstName = model.FirstName;
                    existingItem.LastName = model.LastName;
                  

                    context.User.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Update UserName
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateSeg(User model)
        {
            using (var context = _context)
            {
                var existingItem = context.User.Where(s => s.UserId == model.UserId).FirstOrDefault<User>();

                if (existingItem != null)
                {
                    existingItem.SegId = model.SegId;

                    context.User.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Update UserName
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateSubId(User model)
        {
            using (var context = _context)
            {
                var existingItem = context.User.Where(s => s.UserId == model.UserId).FirstOrDefault<User>();

                if (existingItem != null)
                {
                    existingItem.SubId = model.SubId;

                    context.User.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Update Password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdatePassword(User model)
        {
            using (var context = _context)
            {
                var existingItem = context.User.Where(s => s.UserId == model.UserId).FirstOrDefault<User>();

                if (existingItem != null)
                {
                    existingItem.Password = model.Password;

                    context.User.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Update PlayerName
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdatePlayerName(User model)
        {
            using (var context = _context)
            {
                var existingItem = context.User.Where(s => s.UserId == model.UserId).FirstOrDefault<User>();

                if (existingItem != null)
                {
                    existingItem.FirstName = model.FirstName;
                    existingItem.LastName = model.LastName;

                    context.User.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Update Name
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateName(User model)
        {
            using (var context = _context)
            {
                var existingItem = context.User.Where(s => s.UserId == model.UserId).FirstOrDefault<User>();

                if (existingItem != null)
                {
                    existingItem.FirstName = model.FirstName;
                    existingItem.LastName = model.LastName;
                   

                    context.User.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Password Reset
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task PasswordReset(User user)
        {
            User model = _context.User.Single(x => x.UserId == user.UserId);
            model.Password = user.Password;
            model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            _context.User.Update(model);
            await Save();

        }

        /// <summary>
        /// Generate Password
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task GeneratePassword(string userId)
        {
            //Generate temp Password
            int length = 8; // Adjust the length of the password as needed
            string tempPassword = GenerateNewPassword.GenerateRandomPassword(length);

            User model = _context.User.Single(x => x.UserId == userId);
            model.Password = tempPassword;
            model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            _context.User.Update(model);

            //Send email to user
            EmailMessages emailMessages = new EmailMessages(_configuration);
            emailMessages.NewGeneratedPassword(model);

            await Save();

        }

        /// <summary>
        /// Generate Password
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task UpdateLastLoginDate(string userId)
        {

            User model = _context.User.Single(x => x.UserId == userId);
            model.LastLoginDate = DateTime.Now.ToString();

            _context.User.Update(model);
            await Save();

        }


        /// <summary>
        /// Generate Password
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task UnActivateAccount(string userId)
        {

            User model = _context.User.Single(x => x.UserId == userId);
            model.Status = "InActive";

            _context.User.Update(model);
            await Save();

        }

        /// <summary>
        /// Is Email Available
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<bool> IsEmailAvailable(string email)
        {

            bool item = (from u in _context.User
                         where u.Email == email
                         select u).Any();

            return item;
        }

        /// <summary>
        /// Get User By Email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<User> GetUserByEmail(string email)
        {

            User model = (from u in _context.User
                            where u.Email == email
                            select u).FirstOrDefault();

            return model;
        }

        /// <summary>
        /// Update User Info
        /// </summary>
        /// <param name="user"></param>
        public async Task ResetForgottenPassword(User user)
        {

            user.Password = user.Password;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
           
           
            _context.Entry(user).State = EntityState.Modified;
            await Save();
        }

        /// <summary>
        /// Get User Followers By UserId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<User>> GetUserFollowersByUserId(string userId)
        {
            using (var context = _context)
            {
                try
                {
                    // Query the Follower table to get all users who follow the given user
                    var followers = await (from f in context.Follower
                                           join u in context.User on f.FollowerUserId equals u.UserId
                                           where f.UserId == userId
                                           select u).ToListAsync();

                    return followers;
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as needed
                    return null;
                }
            }
        }

        /// <summary>
        /// Get User Following By UserId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<User>> GetUserFollowingByUserId(string userId)
        {
            using (var context = _context)
            {
                try
                {
                    // Query the Following table to get all users the given user is following
                    var following = await (from f in context.Following
                                           join u in context.User on f.FollowingUserId equals u.UserId
                                           where f.UserId == userId
                                           select u).ToListAsync();

                    return following;
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as needed
                    return null;
                }
            }
        }

        /// <summary>
        /// Start Following UserId
        /// </summary>
        /// <param name="following"></param>
        /// <returns></returns>
        public async Task StartFollowingUserId(Following following)
        {
            try
            {

                following.FollowingId = Guid.NewGuid().ToString();

                _context.Following.Add(following);
            }
            catch (Exception ex)
            {

            }
            await Save();
        }

        /// <summary>
        /// Stop Following UserId
        /// </summary>
        /// <param name="following"></param>
        /// <returns></returns>
        public async Task StopFollowingUserId(Following following)
        {
            Following model = (from u in _context.Following
                               where u.FollowingUserId == following.FollowingUserId
                               select u).FirstOrDefault();

            _context.Following.Remove(model);
            await Save();
        }
       
        /// <summary>
        /// GetTotalGames: Helper method to calculate the total games and win percentage for a ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        private async Task<int> GetTotalGames(string profileId)
        {
            var games = await _context.Game.ToListAsync(); // Fetch all games

            int totalGames = games.Count(game =>
            {
                // Check if the profileId is present in WinnersList or LossersList (assuming these are comma-separated strings)
                bool isWinner = !string.IsNullOrEmpty(game.WinProfileIdsStatusString) &&
                                game.WinProfileIdsStatusString.Contains(profileId);
                bool isLoser = !string.IsNullOrEmpty(game.LoseProfileIdsStatusString) &&
                               game.LoseProfileIdsStatusString.Contains(profileId);

                return isWinner || isLoser;
            });

            return totalGames;
        }

        /// <summary>
        /// Get Win Percentage
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        private async Task<double> GetWinPercentage(string profileId)
        {
            var games = await _context.Game.ToListAsync(); // Fetch all games

            // Count wins and losses for the specific ProfileId
            int wins = games.Count(game =>
            {
                return !string.IsNullOrEmpty(game.WinProfileIdsStatusString) &&
                       game.WinProfileIdsStatusString.Contains(profileId);
            });

            int losses = games.Count(game =>
            {
                return !string.IsNullOrEmpty(game.LoseProfileIdsStatusString) &&
                       game.LoseProfileIdsStatusString.Contains(profileId);
            });

            int totalGames = wins + losses;
            if (totalGames == 0)
            {
                return 0; // If no games were played, return 0%
            }

            return (double)wins / totalGames * 100;
        }

        /// <summary>
        /// Get WinPercentage And TotalLosses
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        private async Task<(int totalWins, int totalLosses)> GetWinPercentageAndTotalLosses(string profileId)
        {
            var games = await _context.Game.ToListAsync(); // Fetch all games

            // Count wins and losses for the specific ProfileId
            int wins = games.Count(game =>
            {
                return !string.IsNullOrEmpty(game.WinProfileIdsStatusString) &&
                       game.WinProfileIdsStatusString.Contains(profileId);
            });

            int losses = games.Count(game =>
            {
                return !string.IsNullOrEmpty(game.LoseProfileIdsStatusString) &&
                       game.LoseProfileIdsStatusString.Contains(profileId);
            });

            int totalGames = wins + losses;
            double winPercentage = 0;

            if (totalGames > 0)
            {
                winPercentage = (double)wins / totalGames * 100;
            }

            // Return the win percentage along with the total wins and total losses
            return (wins, losses);
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <returns></returns>
        public Task<int> Save()
        {
            return _context.SaveChangesAsync();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
