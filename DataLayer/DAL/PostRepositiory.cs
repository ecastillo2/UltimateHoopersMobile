using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;
using System.Linq;
using System.Data;
using Common;
using Microsoft.Data.SqlClient;
//using static Org.BouncyCastle.Math.EC.ECCurve;


namespace DataLayer.DAL
{
    public class PostRepository : IPostRepository, IDisposable
    {
        public IConfiguration _config { get; }
        private HUDBContext _context;
        private readonly string _connectionString;
        /// <summary>
        /// Post Repository
        /// </summary>
        /// <param name="context"></param>
        public PostRepository(HUDBContext context, IConfiguration config)
        {
            this._context = context;
            _config = config;
            _connectionString = _config.GetConnectionString("HBDB_ConnectionString");
        }

        /// <summary>
        /// Get Post By Id
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public async Task<Post> GetPostById(string postId, string timeZone)
        {
            try
            {
                // Query for the Post with the matching PostId
                var model = await _context.Post
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                // Return null if the post doesn't exist
                if (model == null)
                {
                    return null;
                }

                // Query for the PostComments associated with the PostId
                model.UserName = await _context.Profile
                    .Where(f => f.ProfileId == model.ProfileId)
                    .Select(f => f.UserName) // Select only the UserName
                    .FirstOrDefaultAsync(); // Get the first match or null

                model.PostComments = await _context.PostComment
    .Where(f => f.PostId == postId)
    .OrderByDescending(pc => pc.PostCommentDate) // Sorting by date descending
    .Select(pc => new PostComment
    {
        PostCommentId = pc.PostCommentId,
        PostId = pc.PostId,
        PostCommentByProfileId = pc.PostCommentByProfileId,
        UserComment = pc.UserComment,
        PostCommentDate = pc.PostCommentDate,
        RelativeTime = pc.RelativeTime,

        // Join with Profile table to get UserName and ProfileImageURL
        UserName = _context.Profile
            .Where(p => p.ProfileId == pc.PostCommentByProfileId)
            .Select(p => p.UserName)
            .FirstOrDefault(),

        ProfileImageURL = _context.Profile
            .Where(p => p.ProfileId == pc.PostCommentByProfileId)
            .Select(p => p.ImageURL)
            .FirstOrDefault()
    })
    .ToListAsync() ?? new List<PostComment>();

                // Get the count of likes for the current post
                model.Likes = await _context.LikedPost
                    .CountAsync(lp => lp.PostId == postId);

                // Query for the ProfileMentions associated with the PostId (allow it to be null)
                model.ProfileMentions = await _context.Profile
                    .Where(p => p.UserId != null && p.UserId == model.UserId) // Adjust the condition based on your requirements
                    .ToListAsync(); // This could return an empty list or null, depending on the condition

                // If there are mentions in the post, handle them
                if (!string.IsNullOrWhiteSpace(model.Mention))
                {
                    // Split the mention field to get a list of ProfileIds
                    var mentionedProfileIds = model.Mention.Split(',')
                        .Select(id => id.Trim())
                        .Where(id => !string.IsNullOrEmpty(id)) // Ensure no empty IDs
                        .ToList();

                    if (mentionedProfileIds.Any())
                    {
                        // Fetch the profiles associated with the mentioned IDs
                        model.ProfileMentions = await _context.Profile
                            .Where(p => mentionedProfileIds.Contains(p.ProfileId))
                            .ToListAsync();

                        // Fetch settings for all mentioned profiles in one query
                        var settings = await _context.Setting
                            .Where(s => mentionedProfileIds.Contains(s.ProfileId))
                            .ToDictionaryAsync(s => s.ProfileId);

                        // Fetch users based on UserId from Profile table
                        var userIds = model.ProfileMentions.Select(p => p.UserId).ToHashSet(); // Extract unique UserIds
                        var users = await _context.User
                            .Where(u => userIds.Contains(u.UserId))
                            .ToDictionaryAsync(u => u.UserId);

                        // Assign settings and user email to each profile in a single loop
                        foreach (var profile in model.ProfileMentions)
                        {
                            profile.Setting = settings.TryGetValue(profile.ProfileId, out var setting) ? setting : new Setting();
                            profile.Email = users.TryGetValue(profile.UserId, out var user) ? user.Email : string.Empty;
                        }
                    }
                }

                // Convert PostedDate (string) to DateTime if necessary
                if (DateTime.TryParse(model.PostedDate, out DateTime dateTime))
                {
                    // Get the current time
                    DateTime now = DateTime.Now;

                    // Call the method to get the "ago" string (e.g., "5 minutes ago")
                    model.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                }
                else
                {
                    model.RelativeTime = "Invalid Date"; // Handle potential parse failure
                }

                foreach(var item in model.PostComments)
                {
                    // Convert PostedDate (string) to DateTime if necessary
                    if (DateTime.TryParse(item.PostCommentDate.ToString(), out DateTime dateTimex))
                    {
                        // Get the current time
                        DateTime now = DateTime.Now;

                        // Call the method to get the "ago" string (e.g., "5 minutes ago")
                        item.RelativeTime = RelativeTime.GetRelativeTime(dateTimex, timeZone);
                    }
                    else
                    {
                        item.RelativeTime = "Invalid Date"; // Handle potential parse failure
                    }
                }
               

                return model;
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                return null;
            }
        }

        /// <summary>
        /// Get Posts
        /// </summary>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        public async Task<List<Post>> GetAllPosts(string timeZone)
        {
            using (var connection = new SqlConnection(_connectionString)) // Get database connection
            {
                try
                {
                    // Open Connection
                    if (connection.State == ConnectionState.Closed)
                        await connection.OpenAsync();

                    // Execute Stored Procedure
                    var posts = (await connection.QueryAsync<Post>("GetAllPosts", commandType: CommandType.StoredProcedure)).ToList();

                    // Process Mentions
                    foreach (var post in posts)
                    {
                        if (!string.IsNullOrWhiteSpace(post.Mention))
                        {
                            var mentionedProfileIds = post.Mention.Split(',')
                                .Select(id => id.Trim())
                                .Where(id => !string.IsNullOrEmpty(id))
                                .ToList();

                            if (mentionedProfileIds.Any())
                            {
                                post.ProfileMentions = await _context.Profile
                                    .Where(p => mentionedProfileIds.Contains(p.ProfileId))
                                    .ToListAsync();
                            }
                        }

                        // Fetch Star Rating (Assuming you have a method for this)
                        post.StarRating = await GetAverageStarRatingByProfileId(post.ProfileId);

                        // Convert Date to Relative Time
                        if (DateTime.TryParse(post.PostedDate, out DateTime dateTime))
                        {
                            post.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                        }
                        else
                        {
                            post.RelativeTime = "Unknown"; // Handle parsing errors
                        }
                    }

                    return posts.OrderByDescending(post => post.PostedDate).ToList();
                }
                catch (Exception ex)
                {
                    // Log error (optional)
                    return new List<Post>();
                }
            }
        }

        /// <summary>
        /// Get Posts
        /// </summary>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        public async Task<List<Post>> GetPosts(string timeZone)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    // Open Connection
                    if (connection.State == ConnectionState.Closed)
                        await connection.OpenAsync();

                    // Fetch posts using Dapper with a stored procedure
                    var posts = (await connection.QueryAsync<Post>("GetPosts", commandType: CommandType.StoredProcedure)).ToList();

                    // Get all mentioned profile IDs in one go
                    var allMentionedProfileIds = posts
                        .Where(p => !string.IsNullOrWhiteSpace(p.Mention))
                        .SelectMany(p => p.Mention.Split(',').Select(id => id.Trim()))
                        .Where(id => !string.IsNullOrEmpty(id))
                        .Distinct()
                        .ToList();

                    // Fetch all mentioned profiles in one query
                    var profileMentions = new List<Profile>();
                    if (allMentionedProfileIds.Any())
                    {
                        profileMentions = await _context.Profile
                            .Where(p => allMentionedProfileIds.Contains(p.ProfileId))
                            .ToListAsync();
                    }

                    // Use Parallel.ForEach for concurrent processing
                    Parallel.ForEach(posts, post =>
                    {
                        // Map profile mentions
                        if (!string.IsNullOrWhiteSpace(post.Mention))
                        {
                            var mentionedIds = post.Mention.Split(',')
                                .Select(id => id.Trim())
                                .Where(id => !string.IsNullOrEmpty(id))
                                .ToList();

                            post.ProfileMentions = profileMentions
                                .Where(p => mentionedIds.Contains(p.ProfileId))
                                .ToList();
                        }

                        // Fetch Star Rating (optimize with caching if frequently accessed)
                        post.StarRating = GetAverageStarRatingByProfileId(post.ProfileId).Result;

                        // Convert Date to Relative Time
                        if (DateTime.TryParse(post.PostedDate, out DateTime dateTime))
                        {
                            post.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                        }
                        else
                        {
                            post.RelativeTime = "Unknown";
                        }
                    });

                    // Sort only once, at the end
                    return posts.OrderByDescending(post => post.PostedDate).ToList();
                }
                catch (Exception ex)
                {
                    // Log error (optional)
                    return new List<Post>();
                }
            }
        }


        /// <summary>
        /// Get Posts
        /// </summary>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        public async Task<List<Post>> GetBlogs(string timeZone)
        {
            using (var connection = new SqlConnection(_connectionString)) // Get database connection
            {
                try
                {
                    // Open Connection
                    if (connection.State == ConnectionState.Closed)
                        await connection.OpenAsync();

                    // Execute Stored Procedure
                    var posts = (await connection.QueryAsync<Post>("GetBlogs", commandType: CommandType.StoredProcedure)).ToList();

                    // Process Mentions
                    foreach (var post in posts)
                    {
                        
                        // Convert Date to Relative Time
                        if (DateTime.TryParse(post.PostedDate, out DateTime dateTime))
                        {
                            post.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                        }
                        else
                        {
                            post.RelativeTime = "Unknown"; // Handle parsing errors
                        }
                    }

                    return posts.OrderByDescending(post => post.PostedDate).ToList();
                }
                catch (Exception ex)
                {
                    // Log error (optional)
                    return new List<Post>();
                }
            }
        }

        /// <summary>
        /// Get Posts
        /// </summary>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        public async Task<List<Post>> GetHoopNews(string timeZone)
        {
            using (var connection = new SqlConnection(_connectionString)) // Get database connection
            {
                try
                {
                    // Open Connection
                    if (connection.State == ConnectionState.Closed)
                        await connection.OpenAsync();

                    // Execute Stored Procedure
                    var posts = (await connection.QueryAsync<Post>("GetHoopNews", commandType: CommandType.StoredProcedure)).ToList();

                    // Process Mentions
                    foreach (var post in posts)
                    {

                        // Convert Date to Relative Time
                        if (DateTime.TryParse(post.PostedDate, out DateTime dateTime))
                        {
                            post.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                        }
                        else
                        {
                            post.RelativeTime = "Unknown"; // Handle parsing errors
                        }
                    }

                    return posts.OrderByDescending(post => post.PostedDate).ToList();
                }
                catch (Exception ex)
                {
                    // Log error (optional)
                    return new List<Post>();
                }
            }
        }

        /// <summary>
        /// Get Posts
        /// </summary>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        public async Task<List<Post>> GetEvents(string timeZone)
        {
            using (var connection = new SqlConnection(_connectionString)) // Get database connection
            {
                try
                {
                    // Open Connection
                    if (connection.State == ConnectionState.Closed)
                        await connection.OpenAsync();

                    // Execute Stored Procedure
                    var posts = (await connection.QueryAsync<Post>("GetEvents", commandType: CommandType.StoredProcedure)).ToList();

                    // Process Mentions
                    foreach (var post in posts)
                    {

                        // Convert Date to Relative Time
                        if (DateTime.TryParse(post.PostedDate, out DateTime dateTime))
                        {
                            post.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                        }
                        else
                        {
                            post.RelativeTime = "Unknown"; // Handle parsing errors
                        }
                    }

                    return posts.OrderByDescending(post => post.PostedDate).ToList();
                }
                catch (Exception ex)
                {
                    // Log error (optional)
                    return new List<Post>();
                }
            }
        }

        /// <summary>
        /// Get Posts
        /// </summary>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        public async Task<List<Post>> GetPublicPosts()
        {
            using (var context = _context)
            {
                try
                {
                    // LINQ query to join Post and Profile tables
                    var query = await (from post in context.Post
                                       join profile in context.Profile
                                       on post.ProfileId equals profile.ProfileId
                                       select new Post
                                       {
                                           PostId = post.PostId,
                                           UserId = post.UserId,
                                           Caption = post.Caption,
                                           PostFileURL = post.PostFileURL,
                                           Type = post.Type,
                                           Status = post.Status,
                                           PostText = post.PostText,
                                           PostType = post.PostType,
                                           // Count the number of likes for each post
                                           Likes = context.LikedPost
                                                           .Count(lp => lp.PostId == post.PostId),
                                           DisLikes = post.DisLikes,
                                           Hearted = post.Hearted,
                                           Views = post.Views,
                                           Shared = post.Shared,
                                           PostedDate = post.PostedDate,
                                           ProfileId = post.ProfileId,
                                           ThumbnailUrl = post.ThumbnailUrl,
                                           FirstName = profile.UserName, // Assuming UserName represents FirstName
                                           ProfileImageURL = profile.ImageURL,
                                           UserName = profile.UserName,
                                           StarRating = profile.StarRating,
                                           // Count the number of comments for each post
                                           PostCommentCount = context.PostComment
                                                                     .Where(c => c.PostId == post.PostId)
                                                                     .Count()
                                       }).ToListAsync();



                    // Order the query by PostedDate in descending order
                    query = query.OrderByDescending(post => post.PostedDate).ToList();
                    return query;
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as needed
                    return null;
                }
            }
        }

        /// <summary>
        /// Get Posts By ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        public async Task<List<Post>> GetPostsByProfileId(string profileId, string timeZone)
        {
            using (var context = _context)
            {
                try
                {
                    // LINQ query to join Post and Profile tables
                    var query = await (from post in context.Post
                                       join profile in context.Profile
                                       on post.ProfileId equals profile.ProfileId // Correct join condition
                                       where post.ProfileId == profileId && post.PostType == "User"  // Filter by the given profileId
                                       select new Post
                                       {
                                           PostId = post.PostId,
                                           UserId = post.UserId,
                                           Caption = post.Caption,
                                           PostFileURL = post.PostFileURL,
                                           Type = post.Type,
                                           Status = post.Status,
                                           PostType = post.PostType,
                                           Title = post.Title,
                                           // Count the number of likes for each post
                                           Likes = context.LikedPost
                                                   .Count(lp => lp.PostId == post.PostId),
                                           DisLikes = post.DisLikes,
                                           Hearted = post.Hearted,
                                           Views = post.Views,
                                           Shared = post.Shared,
                                           PostedDate = post.PostedDate,
                                           ProfileId = post.ProfileId,
                                           ThumbnailUrl = post.ThumbnailUrl,
                                           FirstName = profile.UserName, // Assuming UserName represents FirstName
                                           ProfileImageURL = profile.ImageURL,
                                           UserName = profile.UserName,
                                           StarRating = profile.StarRating,
                                           PostText = post.PostText,
                                           // Count the number of comments for each post
                                           PostCommentCount = context.PostComment
                                                             .Where(c => c.PostId == post.PostId)
                                                             .Count()
                                       }).ToListAsync();

                    foreach (var item in query)
                    {
                        item.StarRating = await GetAverageStarRatingByProfileId(item.ProfileId);
                        // Convert PostedDate (string) to DateTime if necessary
                        if (DateTime.TryParse(item.PostedDate, out DateTime dateTime))
                        {
                            // Get the current time
                            DateTime now = DateTime.Now;

                            // Call the method to get the "ago" string (e.g., "5 minutes ago")
                            item.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                        }
                        else
                        {
                            item.RelativeTime = "Invalid Date"; // Handle potential parse failure
                        }
                    }

                    // Sort the posts by PostedDate in descending order
                    query = query.OrderByDescending(post => post.PostedDate).ToList();

                    return query;
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine(ex.Message); // Replace with your logging mechanism
                    return null; // Return null or handle the error appropriately
                }
            }
        }

        /// <summary>
        /// Get Posts saved by a specific ProfileId
        /// </summary>
        /// <param name="profileId">The ID of the profile saving the posts</param>
        /// <param name="timeZone">The timezone to calculate relative time</param>
        /// <returns>List of saved posts</returns>
        public async Task<List<Post>> GetSavedPostsByProfileId(string profileId, string timeZone)
        {
            using (var context = _context)
            {
                try
                {
                    // LINQ query to get saved posts for the specified profile
                    var savedPosts = await (from saved in context.SavedPost
                                            join post in context.Post
                                            on saved.PostId equals post.PostId
                                            join profile in context.Profile
                                            on post.ProfileId equals profile.ProfileId
                                            where saved.SavedByProfileId == profileId
                                            select new Post
                                            {
                                                PostId = post.PostId,
                                                UserId = post.UserId,
                                                Caption = post.Caption,
                                                PostFileURL = post.PostFileURL,
                                                Type = post.Type,
                                                Status = post.Status,
                                                PostType = post.PostType,
                                                Likes = context.LikedPost
                                                   .Count(lp => lp.PostId == post.PostId),
                                                DisLikes = post.DisLikes,
                                                Hearted = post.Hearted,
                                                Views = post.Views,
                                                Shared = post.Shared,
                                                PostedDate = post.PostedDate,
                                                ProfileId = post.ProfileId,
                                                ThumbnailUrl = post.ThumbnailUrl,
                                                UserName = profile.UserName,
                                                ProfileImageURL = profile.ImageURL,
                                                StarRating = profile.StarRating,
                                                // Use a subquery to get PostCommentCount
                                                PostCommentCount = context.PostComment
                                                                         .Where(c => c.PostId == post.PostId)
                                                                         .Count(),
                                                SavedPost = true // Mark as saved
                                            }).ToListAsync();

                    // Loop through the saved posts to calculate additional properties
                    foreach (var item in savedPosts)
                    {
                        item.StarRating = await GetAverageStarRatingByProfileId(item.ProfileId);

                        // Convert PostedDate (string) to DateTime if necessary
                        if (DateTime.TryParse(item.PostedDate, out DateTime dateTime))
                        {
                            // Get the current time
                            DateTime now = DateTime.Now;

                            // Call the method to get the "ago" string (e.g., "5 minutes ago")
                            item.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                        }
                        else
                        {
                            item.RelativeTime = "Invalid Date"; // Handle potential parse failure
                        }
                    }

                    // Sort the saved posts by PostedDate in descending order
                    savedPosts = savedPosts.OrderByDescending(post => post.PostedDate).ToList();

                    return savedPosts;
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine(ex.Message); // Replace with your logging mechanism
                    return null; // Return null or handle the error appropriately
                }
            }
        }

        /// <summary>
        /// Get Posts Mentioning ProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        public async Task<List<Post>> GetPostsMentionProfileId(string profileId, string timeZone)
        {
            var posts = new List<Post>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("GetPostsMentionProfileId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ProfileId", profileId);
                        command.Parameters.AddWithValue("@TimeZone", timeZone);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var post = new Post
                                {
                                    PostId = reader["PostId"].ToString(),
                                    UserId = reader["UserId"].ToString(),
                                    Caption = reader["Caption"]?.ToString(),
                                    PostFileURL = reader["PostFileURL"]?.ToString(),
                                    Type = reader["Type"]?.ToString(),
                                    Status = reader["Status"]?.ToString(),
                                    Likes = reader["Likes"] != DBNull.Value ? Convert.ToInt32(reader["Likes"]) : 0,
                                    DisLikes = reader["DisLikes"] != DBNull.Value ? Convert.ToInt32(reader["DisLikes"]) : 0,
                                    Hearted = reader["Hearted"] != DBNull.Value ? Convert.ToInt32(reader["Hearted"]) : 0,
                                    Views = reader["Views"] != DBNull.Value ? Convert.ToInt32(reader["Views"]) : 0,
                                    PostText = reader["PostText"]?.ToString(),
                                    ProfileId = reader["ProfileId"].ToString(),
                                    ThumbnailUrl = reader["ThumbnailUrl"]?.ToString(),
                                    PostType = reader["PostType"]?.ToString(),
                                    FirstName = reader["FirstName"]?.ToString(),
                                    ProfileImageURL = reader["ProfileImageURL"]?.ToString(),
                                    UserName = reader["UserName"]?.ToString(),
                                    StarRating = reader["StarRating"]?.ToString(),
                                    Mention = reader["Mention"]?.ToString(),
                                    PostedDate = reader["PostedDate"]?.ToString(),
                                    PostCommentCount = reader["PostCommentCount"] != DBNull.Value ? Convert.ToInt32(reader["PostCommentCount"]) : 0
                                };

                                // Get relative time
                                if (DateTime.TryParse(post.PostedDate, out DateTime dateTime))
                                {
                                    post.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                                }
                                else
                                {
                                    post.RelativeTime = "Invalid Date";
                                }

                                posts.Add(post);
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Database error: {sqlEx.Message}");
                // Log the exception if using a logging framework
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                // Log the exception if needed
            }

            return posts;
        }

        /// <summary>
        /// Get Posts With Specific Hashtag
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        public async Task<List<Post>> GetPostsWithTagByTagId(string tagId, string timeZone)
        {
            using (var context = _context)
            {
                try
                {
                    // Retrieve the tag object based on tagId
                    var tag = await context.Tag
                                           .Where(t => t.TagId == tagId)
                                           .FirstOrDefaultAsync();

                    // Check if the tag exists and contains a valid hashtag
                    if (tag == null || string.IsNullOrEmpty(tag.HashTag))
                    {
                        return new List<Post>(); // Return empty list if the tag or hashtag is not found
                    }

                    // LINQ query to retrieve posts with the specified hashtag in the caption
                    var query = await (from post in context.Post
                                       where post.Caption.Contains("#" + tag.HashTag) // Check if the caption contains the hashtag
                                       select new Post
                                       {
                                           PostId = post.PostId,
                                           UserId = post.UserId,
                                           Caption = post.Caption,
                                           PostFileURL = post.PostFileURL,
                                           Type = post.Type,
                                           Status = post.Status,
                                           Likes = post.Likes,
                                           DisLikes = post.DisLikes,
                                           Hearted = post.Hearted,
                                           Views = post.Views,
                                           Shared = post.Shared,
                                           PostedDate = post.PostedDate,
                                           ProfileId = post.ProfileId,
                                           ThumbnailUrl = post.ThumbnailUrl,
                                           FirstName = context.Profile
                                                              .Where(p => p.ProfileId == post.ProfileId)
                                                              .Select(p => p.UserName)
                                                              .FirstOrDefault(),
                                           ProfileImageURL = context.Profile
                                                                    .Where(p => p.ProfileId == post.ProfileId)
                                                                    .Select(p => p.ImageURL)
                                                                    .FirstOrDefault(),
                                           UserName = context.Profile
                                                             .Where(p => p.ProfileId == post.ProfileId)
                                                             .Select(p => p.UserName)
                                                             .FirstOrDefault(),
                                           StarRating = context.Profile
                                                               .Where(p => p.ProfileId == post.ProfileId)
                                                               .Select(p => p.StarRating)
                                                               .FirstOrDefault(),
                                           // Calculate PostCommentCount
                                           PostCommentCount = context.PostComment
                                                                     .Count(c => c.PostId == post.PostId)
                                       }).ToListAsync();

                    foreach (var item in query)
                    {

                        item.StarRating = await GetAverageStarRatingByProfileId(item.ProfileId);

                        // Convert PostedDate (string) to DateTime if necessary
                        if (DateTime.TryParse(item.PostedDate, out DateTime dateTime))
                        {
                            // Call the method to get the "ago" string (e.g., "5 minutes ago")
                            item.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                        }
                        else
                        {
                            item.RelativeTime = "Invalid Date"; // Handle potential parse failure
                        }
                    }

                    // Sort the posts by PostedDate in descending order
                    query = query.OrderByDescending(post => post.PostedDate).ToList();

                    return query;
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine(ex.Message); // Replace with your logging mechanism
                    return null; // Return null or handle the error appropriately
                }
            }
        }


        /// <summary>
        /// Insert Post
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertPost(Post model)
        {
            using (var context = _context)
            {
                try
                {
                    string fileType = string.Empty;

                    if (model.Type == "image")
                    {
                        fileType = ".webp";
                        model.PostFileURL = "https://uhblobstorageaccount.blob.core.windows.net/postfile/" + model.PostId + fileType;
                    }
                    if (model.Type == "video")
                    {
                        fileType = ".mp4";
                        model.PostFileURL = "https://uhblobstorageaccount.blob.core.windows.net/postfile/" + model.PostId + fileType;

                        model.ThumbnailUrl = "https://uhblobstorageaccount.blob.core.windows.net/postthumbnail/" + model.PostId + ".png";
                    }

                    model.PostedDate = DateTime.Now.ToString();

                    await context.Post.AddAsync(model);

                }
                catch (Exception ex)
                {

                }
                await Save();
            }
        }

        /// <summary>
        /// Update Post
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdatePost(Post model)
        {
            using (var context = _context)
            {
                var existingItem = context.Post.Where(s => s.PostId == model.PostId).FirstOrDefault<Post>();

                if (existingItem != null)
                {
                    existingItem.Caption = model.Caption;
                    existingItem.Type = model.Type;
                    existingItem.Status = model.Status;
                    existingItem.Likes = model.Likes;
                    existingItem.DisLikes = model.DisLikes;
                    existingItem.Hearted = model.Hearted;
                    existingItem.Views = model.Views;
                    existingItem.Shared = model.Shared;
                    existingItem.PostType = model.PostType;
                    existingItem.Title = model.Title;
                    existingItem.PostText = model.PostText;
                    existingItem.Category = model.Category;

                    context.Post.Update(existingItem);
                    await Save();
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Update PostStatus
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task UpdatePostStatus(string postId, string status)
        {
            using (var context = _context)
            {
                // Find the existing post by PostId
                var existingPost = context.Post.FirstOrDefault(p => p.PostId == postId);

                if (existingPost != null)
                {
                    // Update the status field
                    existingPost.Status = status;

                    // Optionally, you can add updates to other fields if needed:
                    // existingPost.Caption = "new caption";
                    // existingPost.Type = "new type";

                    // Mark the entity as updated
                    context.Post.Update(existingPost);

                    // Save changes asynchronously
                    await context.SaveChangesAsync();
                }
                else
                {
                    // Handle case where the post is not found, e.g., logging or throwing an exception
                    throw new Exception($"Post with PostId {postId} not found.");
                }
            }
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
        /// Delete Post
        /// </summary>
        /// <param name="PostId"></param>
        /// <returns></returns>
        public async Task DeletePost(string postId)
        {
            using (var context = _context)
            {
                // Fetch the post
                var post = await context.Post.FirstOrDefaultAsync(p => p.PostId == postId);

                // Get all comments associated with the post
                var postComments = context.PostComment.Where(pc => pc.PostId == postId).ToList();


                Post objSavedPost = (from u in context.Post
                                     where u.PostId == postId
                                     select u).FirstOrDefault();



                _context.Post.Remove(post);
                // Remove all the comments
                _context.PostComment.RemoveRange(postComments);
                _context.Post.Remove(objSavedPost);
                await Save();
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <returns></returns>
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }


    }
}
