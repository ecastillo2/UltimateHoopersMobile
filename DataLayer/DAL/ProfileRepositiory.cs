using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;
using Newtonsoft.Json;
using Common;



namespace DataLayer.DAL
{
 
    public class ProfileRepository : IProfileRepository, IDisposable
    {
        public IConfiguration _config { get; }
        private readonly HUDBContext _context;
       
        /// <summary>
        /// Profile Repository
        /// </summary>
        /// <param name="context"></param>
        public ProfileRepository(HUDBContext context, IConfiguration config)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Get Profile By Id
        /// </summary>
        /// <param name="ProfileId"></param>
        /// <returns></returns>
        public async Task<Profile?> GetProfileById(string ProfileId)
        {
            using (var context = _context)
            {
                try
                {
                    // Query to join Profile with User and get the Profile with user's details
                    var profile = await (from p in context.Profile
                                         join u in context.User on p.UserId equals u.UserId
                                         where p.ProfileId == ProfileId
                                         select new Profile
                                         {
                                             ProfileId = p.ProfileId,
                                             UserId = p.UserId,
                                             UserName = p.UserName,
                                             Height = p.Height,
                                             Weight = p.Weight,
                                             Position = p.Position,
                                             Ranking = p.Ranking,
                                             StarRating = p.StarRating,
                                             QRCode = p.QRCode,
                                             Bio = p.Bio,
                                             TopRecord = p.TopRecord,
                                             ImageURL = p.ImageURL,
                                             PlayerArchetype = p.PlayerArchetype,
                                             City = p.City,
                                             Zip = p.Zip,
                                             PlayerNumber = p.PlayerNumber,
                                             Points = p.Points,
                                             FirstName = u.FirstName,
                                             LastName = u.LastName,
                                             Email = u.Email,
                                             SegId = u.SegId,
                                             SubId = u.SubId,
                                         }).FirstOrDefaultAsync();

                    if (profile == null)
                        return null;

                    // Fetch notifications for the profile
                    var notifications = await context.Notification
                                                    .Where(notification => notification.ProfileId == ProfileId)
                                                    .ToListAsync();

                    // Fetch the user's settings
                    var setting = await context.Setting
                                               .Where(p => p.ProfileId == ProfileId)
                                               .FirstOrDefaultAsync();

                    // Fetch scouting report for the profile
                    var scoutingReport = await context.ScoutingReport
                                                      .Where(p => p.ProfileId == ProfileId)
                                                      .FirstOrDefaultAsync();

                    // Query to get the squad the user owns
                    var squad = await context.Squad
                                             .Where(p => p.OwnerProfileId == ProfileId)
                                             .FirstOrDefaultAsync();

                    if (squad.SquadId != null)
                    {
                        // Query to get the single squad team associated with the squad
                        var squadTeam = await (from st in _context.SquadTeam
                                               where st.SquadId == squad.SquadId
                                               select st).FirstOrDefaultAsync() ?? new SquadTeam(); // This gets the first squad team

                        // Query to get all profiles associated with the squad
                        var profiles = await (from st in _context.SquadTeam
                                              join p in _context.Profile on st.ProfileId equals p.ProfileId
                                              where st.SquadId == squad.SquadId
                                              select p).ToListAsync() ?? new List<Profile>();

                        // Loop through each profile and set the RequestResponseText based on SquadTeam.RequestResponse
                        foreach (var item in profiles)
                        {
                            // Find the SquadTeam for the current profile (there should be only one SquadTeam in this case)
                            var squadTeamForProfile = squadTeam; // Since there's only one squadTeam, use it directly

                            // Check if squadTeam is not null and then set the RequestResponseText
                            if (squadTeamForProfile != null)
                            {
                                item.RequestResponseText = squadTeamForProfile.RequestResponse.HasValue && squadTeamForProfile.RequestResponse.Value
                                    ? "Accepted"
                                    : "Waitng for Response"; // Customize the text based on your needs
                            }
                        }

                        // Assign the single squadTeam to the Squad
                        profile.Squad = squad;
                        profile.Squad.SquadTeam = squadTeam;
                        profile.Squad.SquadTeam.ProfileList = profiles;// Assign single SquadTeam here
                        //profile.Squad.SquadRequest = squadRequests;
                    }


                    // Get followers and following counts
                    var followersCount = await context.Follower
                        .Where(f => f.FollowerProfileId == ProfileId)
                        .CountAsync();

                    var followingCount = await context.Following
                        .Where(f => f.ProfileId == ProfileId)
                        .CountAsync();

                    // Set counts in profile
                    profile.FollowersCount = followersCount.ToString();
                    profile.FollowingCount = followingCount.ToString();

                    // Retrieve associated PrivateRun list, allowing it to be null if no results
                    var privateRuns = await context.PrivateRun
                                                   .Where(pr => pr.ProfileId == ProfileId)
                                                   .ToListAsync();

                    // Retrieve associated comments
                    var comments = await (from comment in context.PlayerComment
                                          join pr in context.Profile on comment.CommentedProfileId equals pr.ProfileId
                                          where comment.ProfileId == ProfileId
                                          select new PlayerComment
                                          {
                                              PlayerCommentId = comment.PlayerCommentId,
                                              ProfileId = comment.ProfileId,
                                              CommentedProfileId = comment.CommentedProfileId,
                                              Comment = comment.Comment,
                                              DateCommented = comment.DateCommented,
                                              ImageURL = pr.ImageURL, // Assuming you want to add ImageURL here
                                              UserName = pr.UserName
                                          }).ToListAsync();


                    foreach (var item in comments)
                    {
                        // Convert the string to DateTime
                        DateTime dateTime = (DateTime)item.DateCommented;
                        // Get the current time
                        DateTime now = DateTime.Now;

                        // Calculate the difference
                        TimeSpan timeDifference = now - dateTime;

                        // Call the method to get the "ago" string
                        string result = RelativeTime.GetRelativeTime(dateTime, "UTC");

                        item.RelativeTime = result;
                    }

                    profile.ScoutingReport = scoutingReport;
                    // Set related data to profile
                    profile.PrivateRunList = privateRuns.Any() ? privateRuns : null;
                    profile.PlayerCommentList = comments;

                    // Calculate and set Win/Loss statistics
                    var totalGames = await GetTotalGames(ProfileId);
                    var winPercentage = await GetWinPercentage(ProfileId);

                    profile.TotalGames = totalGames.ToString();
                    profile.WinPercentage = winPercentage.ToString("F2");

                    var data = await GetWinPercentageAndTotalLosses(ProfileId);

                    profile.TotalWins = data.totalWins;
                    profile.TotalLosses = data.totalLosses;
                    profile.NotificationList = notifications;
                    profile.Ranking = RankingSuffix.GetOrdinalSuffix(Convert.ToInt32(profile.Ranking));

                    return profile;
                }
                catch (Exception ex)
                {
                    // Log the exception and handle it as needed
                    return null;
                }
            }
        }


        /// <summary>
        /// Get Following Profiles By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        public async Task<List<Profile>> GetFollowingProfilesByProfileId(string profileId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to join Following and Profile tables to get profiles the user is following
                    var profiles = await (from follow in context.Following
                                          join profile in context.Profile
                                          on follow.FollowingProfileId equals profile.ProfileId
                                          where follow.ProfileId == profileId
                                          select new Profile
                                          {
                                              ProfileId = profile.ProfileId,
                                              UserId = profile.UserId,
                                              UserName = profile.UserName,
                                              Height = profile.Height,
                                              Weight = profile.Weight,
                                              Position = profile.Position,
                                              Ranking = profile.Ranking,
                                              StarRating = profile.StarRating,
                                              QRCode = profile.QRCode,
                                              TopRecord = profile.TopRecord,
                                              Bio = profile.Bio,
                                              ImageURL = profile.ImageURL,
                                              PlayerArchetype = profile.PlayerArchetype,
                                              City = profile.City,
                                              Zip = profile.Zip,
                                              PlayerNumber = profile.PlayerNumber,
                                              Points = profile.Points
                                          }).ToListAsync();

                    // Update each profile's StarRating
                    foreach (var profile in profiles)
                    {
                        profile.StarRating = await GetAverageStarRatingByProfileId(profile.ProfileId);
                        profile.Followed = true;
                    }

                    return profiles;
                }
                catch (Exception ex)
                {
                    // Handle the exception or log it as needed
                    return null;
                }
            }
        }

        /// <summary>
        /// Get Follower Profiles By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        public async Task<List<Profile>> GetFollowerProfilesByProfileId(string profileId)
        {
            using (var context = _context)
            {
                try
                {
                    // Use LINQ to join Profile and Following tables to get profiles following the specified profileId
                    var query = await (from follow in context.Follower
                                       join profile in context.Profile
                                       on follow.FollowerProfileId equals profile.ProfileId
                                       where follow.ProfileId == profileId
                                       select profile).ToListAsync();

                    // Update each profile's StarRating
                    foreach (var item in query)
                    {
                        item.StarRating = await GetAverageStarRatingByProfileId(item.ProfileId);
                        item.Followed = false;
                    }

                    

                    return query;
                }
                catch (Exception ex)
                {
                    // Handle the exception or log it as needed
                    return null;
                }
            }
        }

        /// <summary>
        /// Get Profiles
        /// </summary>
        /// <returns></returns>
        public async Task<List<Profile>> GetProfiles()
        {
            using (var context = _context)
            {
                try
                {
                    // Query to join Profile with User and get the Profile with user's details
                    var query = await (from p in _context.Profile
                                          join u in _context.User
                                          on p.UserId equals u.UserId
                                      where u.Status == "Active"
                                          select new Profile
                                          {
                                              ProfileId = p.ProfileId,
                                              UserId = p.UserId,
                                              UserName = p.UserName,
                                              Height = p.Height,
                                              Weight = p.Weight,
                                              Position = p.Position,
                                              Ranking = p.Ranking,
                                              StarRating = p.StarRating,
                                              QRCode = p.QRCode,
                                              Bio = p.Bio,
                                              TopRecord = p.TopRecord,
                                              ImageURL = p.ImageURL,
                                              PlayerArchetype = p.PlayerArchetype,
                                              City = p.City,
                                              Zip = p.Zip,
                                              PlayerNumber = p.PlayerNumber,
                                              Points = p.Points,
                                              Status = u.Status,
                                              FirstName = u.FirstName,
                                              LastName = u.LastName,
                                              Email = u.Email,
                                              FullName = u.FirstName +" "+ u.LastName,
                                              LastLoginDate = u.LastLoginDate,
                                              
                                          }).ToListAsync();




                    foreach (var item in query)
                    {
                        item.Ranking = RankingSuffix.GetOrdinalSuffix(Convert.ToInt32(item.Ranking));
                       
                        //item.StarRating = await GetAverageStarRatingByProfileId(item.ProfileId);

                        // Get followers count
                        var scoutingReport = await context.ScoutingReport
                            .Where(f => f.ProfileId == item.ProfileId)
                            .FirstOrDefaultAsync() ?? new ScoutingReport();



                        // Get followers count
                        var followersCount = await context.Follower
                            .Where(f => f.FollowerProfileId == item.ProfileId)
                            .CountAsync();

                        // Get following count
                        var followingCount = await context.Following
                            .Where(f => f.ProfileId == item.ProfileId)
                            .CountAsync();

                        // Assign counts to profile properties
                        item.FollowersCount = followersCount > 0 ? followersCount.ToString() : "0";
                        item.FollowingCount = followingCount > 0 ? followingCount.ToString() : "0";

                        // Query the Rating table for the specified ProfileId
                        //var count = await  (from rating in context.Rating
                        //                   where rating.ProfileId == item.ProfileId
                        //                   select rating.StarRating).ToListAsync();


                        //item.RatedCount = count.Count().ToString();


                        // Calculate Win/Loss statistics
                        var totalGames = await GetTotalGames(item.ProfileId); // Total games played
                        var winPercentage = await GetWinPercentage(item.ProfileId); // Win percentage
                        item.TotalGames = totalGames.ToString();
                        item.WinPercentage = winPercentage.ToString("F2"); // Formatting the percentage to 2 decimal places
                        item.ScoutingReport = scoutingReport;

                        var data = await GetWinPercentageAndTotalLosses(item.ProfileId);

                        item.TotalWins = data.totalWins;
                        item.TotalLosses = data.totalLosses;

                    }


                    return query;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    return null;
                }
            }
        }

        /// <summary>
        /// Update Profile
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateProfile(Profile model)
        {
            using (var context = _context)
            {
                var existingItem = context.Profile.Where(s => s.ProfileId == model.ProfileId).FirstOrDefault<Profile>();

                if (existingItem != null)
                {
                    existingItem.Height = model.Height;
                    existingItem.Weight = model.Weight;
                    existingItem.Position = model.Position;

                    existingItem.Zip = model.Zip;
                    existingItem.QRCode = model.QRCode;
                    existingItem.Bio = model.Bio;
                    existingItem.PlayerArchetype = model.PlayerArchetype;
                    existingItem.City = model.City;

                    context.Profile.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Update Profile
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateLastRunDate(string ProfileId, string LastRunDate)
        {
            using (var context = _context)
            {
                var existingItem = context.Profile.Where(s => s.ProfileId == ProfileId).FirstOrDefault<Profile>();

                if (existingItem != null)
                {

                    existingItem.LastRunDate = LastRunDate;

                    context.Profile.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Update Profile UserName
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateProfileUserName(Profile model)
        {
            using (var context = _context)
            {
                var existingItem = context.Profile.Where(s => s.ProfileId == model.ProfileId).FirstOrDefault<Profile>();

                if (existingItem != null)
                {
                    existingItem.UserName = model.UserName;

                    context.Profile.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Update Profile UserName
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateSetting(Setting model)
        {
            using (var context = _context)
            {
                var existingItem = context.Setting.Where(s => s.ProfileId == model.ProfileId).FirstOrDefault<Setting>();

                if (existingItem != null)
                {
                    existingItem.AllowComments = model.AllowComments;
                    existingItem.ShowGameHistory = model.ShowGameHistory;

                    context.Setting.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Update Profile UserName
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateWinnerPoints(string profileId)
        {
            using (var context = _context)
            {
                var existingItem = context.Profile.Where(s => s.ProfileId == profileId).FirstOrDefault<Profile>();

                if (existingItem != null)
                {
                    existingItem.Points += 2;

                    context.Profile.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Update Profile UserName
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateSetProfileWithBestRecord(string profileId)
        {
            using (var context = _context)
            {
                var existingItem = context.Profile.Where(s => s.ProfileId == profileId).FirstOrDefault<Profile>();

                if (existingItem != null)
                {
                    existingItem.TopRecord = true;

                    context.Profile.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }


        /// <summary>
        /// Update Profile UserName
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateSetProfileWithBestRecordToFalse(string profileId)
        {
            using (var context = _context)
            {
                var existingItem = context.Profile.Where(s => s.ProfileId == profileId).FirstOrDefault<Profile>();

                if (existingItem != null)
                {
                    existingItem.TopRecord = false;

                    context.Profile.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Is Email Available
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<bool> IsUserNameAvailable(string UserName)
        {

            bool item = (from u in _context.Profile
                         where u.UserName == UserName
                         select u).Any();

            return item;
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
        /// GetAverageStarRatingByProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        public async Task<string> GetAverageStarRatingByProfileId(string profileId)
        {
            using (var context = _context)
            {
                try
                {
                    // Query the Rating table for the specified ProfileId
                    var query = await (from rating in context.Rating
                                       where rating.ProfileId == profileId
                                       select rating.StarRating).ToListAsync();

                    // Convert the StarRating from string to integer and calculate the average
                    var averageRating = query
                        .Where(r => !string.IsNullOrEmpty(r)) // Ensure we only calculate for non-null/non-empty ratings
                        .Select(r => int.Parse(r)) // Convert to integer
                        .DefaultIfEmpty(0)         // If no ratings, return 0 as default
                        .Average();

                    // Return the average as an integer
                    return averageRating.ToString();
                }
                catch (Exception ex)
                {
                    // Handle or log exception as needed
                    return "0"; // Return 0 if any exception occurs
                }
            }
        }

        /// <summary>
        /// GetProfileGameHistory
        /// </summary>
        /// <param name="ProfileId"></param>
        /// <returns></returns>
        public async Task<List<Game>> GetProfileGameHistory(string ProfileId)
        {
            // Get all games from the database
            var games = await _context.Game.ToListAsync();
            var profileGames = new List<Game>();

            // Loop through each game
            foreach (var game in games)
            {
                // Parse the WinProfileIdsStatusString and LoseProfileIdsStatusString into lists of Profile objects
                List<Profile> winProfiles = string.IsNullOrEmpty(game.WinProfileIdsStatusString)
                    ? new List<Profile>()
                    : JsonConvert.DeserializeObject<List<Profile>>(game.WinProfileIdsStatusString);

                List<Profile> loseProfiles = string.IsNullOrEmpty(game.LoseProfileIdsStatusString)
                    ? new List<Profile>()
                    : JsonConvert.DeserializeObject<List<Profile>>(game.LoseProfileIdsStatusString);

                // Check if the ProfileId is present in the winners or losers
                bool isInWinningTeam = winProfiles.Any(profile => profile.ProfileId == ProfileId);
                bool isInLosingTeam = loseProfiles.Any(profile => profile.ProfileId == ProfileId);

                // If the profile is in either the winning or losing list, add the game to the profile's game history
                if (isInWinningTeam || isInLosingTeam)
                {
                    // Populate WinnersList
                    game.WinnersList = winProfiles
                        .Select(profile => _context.Profile.FirstOrDefault(p => p.ProfileId == profile.ProfileId))
                        .Where(profile => profile != null)
                        .Select(profile => new Profile
                        {
                            ProfileId = profile.ProfileId,
                            UserId = profile.UserId,
                            UserName = profile.UserName,
                            Height = profile.Height,
                            Weight = profile.Weight,
                            Position = profile.Position,
                            Ranking = profile.Ranking,
                            StarRating = profile.StarRating,
                            QRCode = profile.QRCode,
                            Bio = profile.Bio,
                            TopRecord = profile.TopRecord,
                            ImageURL = profile.ImageURL,
                            PlayerArchetype = profile.PlayerArchetype,
                            Zip = profile.Zip,
                            City = profile.City,
                            PlayerNumber = profile.PlayerNumber,
                            Points = profile.Points,
                            WinOrLose = "W"
                        }).ToList();

                    // Populate LossersList
                    game.LossersList = loseProfiles
                        .Select(profile => _context.Profile.FirstOrDefault(p => p.ProfileId == profile.ProfileId))
                        .Where(profile => profile != null)
                        .Select(profile => new Profile
                        {
                            ProfileId = profile.ProfileId,
                            UserId = profile.UserId,
                            UserName = profile.UserName,
                            Height = profile.Height,
                            Weight = profile.Weight,
                            Position = profile.Position,
                            Ranking = profile.Ranking,
                            StarRating = profile.StarRating,
                            QRCode = profile.QRCode,
                            TopRecord = profile.TopRecord,
                            Bio = profile.Bio,
                            ImageURL = profile.ImageURL,
                            PlayerArchetype = profile.PlayerArchetype,
                            Zip = profile.Zip,
                            City = profile.City,
                            PlayerNumber = profile.PlayerNumber,
                            Points = profile.Points,
                            WinOrLose = "L"
                        }).ToList();

                    // Add the game to the result list if it has winners or losers for the profile
                    if (game.WinnersList.Any() || game.LossersList.Any())
                    {
                        profileGames.Add(game);
                    }
                }
            }

            // Return an empty list if no games found; otherwise, return the list
            return profileGames.Any() ? profileGames : new List<Game>();
        }

        /// <summary>
        /// Get Total Games: Helper method to calculate the total games and win percentage for a ProfileId
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
        /// Get Win Percentage And TotalLosses
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
            return  (wins, losses);
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <returns></returns>
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}
