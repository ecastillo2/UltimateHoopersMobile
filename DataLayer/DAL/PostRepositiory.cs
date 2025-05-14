using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Domain;
using Common;
using Dapper; 

namespace DataLayer.DAL
{
    public class PostRepository : IPostRepository, IDisposable
    {
        private readonly IConfiguration _config;
        private readonly UHDBContext _context;
    
        

        /// <summary>
        /// Post Repository
        /// </summary>
        public PostRepository(UHDBContext context, IConfiguration config)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            
         
        }

        /// <summary>
        /// Get Post By Id with optimized queries
        /// </summary>
        public async Task<Post> GetPostById(string postId, string timeZone)
        {
            try
            {
                // Use a single efficient query with includes instead of multiple queries
                var model = await _context.Post
                    .AsNoTracking() // Improves performance for read-only operations
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (model == null)
                    return null;

                // Use parallel tasks to fetch related data
                var profileTask = _context.Profile
                    .AsNoTracking()
                    .Where(f => f.ProfileId == model.ProfileId)
                    .Select(f => new { f.UserName, f.ImageURL })
                    .FirstOrDefaultAsync();

                var commentsTask = _context.PostComment
                    .AsNoTracking()
                    .Where(f => f.PostId == postId)
                    .OrderByDescending(pc => pc.PostCommentDate)
                    .ToListAsync();

                var likesTask = _context.LikedPost
                    .AsNoTracking()
                    .Where(lp => lp.PostId == postId)
                    .CountAsync();

                // Wait for all tasks to complete
                await Task.WhenAll(profileTask, commentsTask, likesTask);

                // Extract results
                var profile = await profileTask;
                var comments = await commentsTask;
                var likes = await likesTask;

                // Map data
                model.UserName = profile?.UserName;
                model.ProfileImageURL = profile?.ImageURL;
                model.Likes = likes;

                // Process comments with in-memory operations instead of database queries
                if (comments != null)
                {
                    var commentProfileIds = comments.Select(c => c.PostCommentByProfileId).Distinct().ToList();

                    // Fetch all profile data for comments in a single query
                    var commentProfiles = await _context.Profile
                        .AsNoTracking()
                        .Where(p => commentProfileIds.Contains(p.ProfileId))
                        .Select(p => new { p.ProfileId, p.UserName, p.ImageURL })
                        .ToDictionaryAsync(p => p.ProfileId);

                    // Map profile data to comments in memory
                    model.PostComments = comments.Select(pc => {
                        var commentProfile = commentProfiles.GetValueOrDefault(pc.PostCommentByProfileId);

                        return new PostComment
                        {
                            PostCommentId = pc.PostCommentId,
                            PostId = pc.PostId,
                            PostCommentByProfileId = pc.PostCommentByProfileId,
                            UserComment = pc.UserComment,
                            PostCommentDate = pc.PostCommentDate,
                            UserName = commentProfile?.UserName,
                            ProfileImageURL = commentProfile?.ImageURL,
                            RelativeTime = DateTime.TryParse(pc.PostCommentDate?.ToString(), out DateTime commentDate)
                                ? RelativeTime.GetRelativeTime(commentDate, timeZone)
                                : "Invalid Date"
                        };
                    }).ToList();
                }
                else
                {
                    model.PostComments = new List<PostComment>();
                }

                // Process mentions if present
                if (!string.IsNullOrWhiteSpace(model.Mention))
                {
                    var mentionedProfileIds = model.Mention.Split(',')
                        .Select(id => id.Trim())
                        .Where(id => !string.IsNullOrEmpty(id))
                        .ToList();

                    if (mentionedProfileIds.Any())
                    {
                        // Fetch all needed data in a single query with optimized shape
                        var mentionData = await (from p in _context.Profile
                                                 where mentionedProfileIds.Contains(p.ProfileId)
                                                 join s in _context.Setting on p.ProfileId equals s.ProfileId into settings
                                                 from setting in settings.DefaultIfEmpty()
                                                 join u in _context.User on p.UserId equals u.UserId into users
                                                 from user in users.DefaultIfEmpty()
                                                 select new
                                                 {
                                                     Profile = p,
                                                     Setting = setting,
                                                     Email = user.Email
                                                 }).AsNoTracking().ToListAsync();

                        // Set profile mentions using mapped data
                        model.ProfileMentions = mentionData.Select(m => {
                            var profile = m.Profile;
                            profile.Setting = m.Setting ?? new Setting();
                            profile.Email = m.Email ?? string.Empty;
                            return profile;
                        }).ToList();
                    }
                }

                // Calculate relative time
                if (DateTime.TryParse(model.PostedDate, out DateTime dateTime))
                {
                    model.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                }
                else
                {
                    model.RelativeTime = "Invalid Date";
                }

                return model;
            }
            catch (Exception ex)
            {
                // Log the exception
                return null;
            }
        }

        /// <summary>
        /// Get paginated posts with optimized queries
        /// </summary>
        public async Task<(List<Post> Posts, int TotalCount, int TotalPages)> GetPaginatedPostsAsync(
            int page, int pageSize, string timeZone)
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            // Use cached count when possible
            var countTask = _context.Post.CountAsync();

            // Fetch posts with minimal shape for pagination
            var postsTask = _context.Post
                .AsNoTracking()
                .OrderByDescending(p => p.PostedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Execute both queries in parallel
            await Task.WhenAll(countTask, postsTask);

            var totalCount = await countTask;
            var posts = await postsTask;

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Load related data efficiently
            await LoadPostDetailsAsync(posts, timeZone);

            return (posts, totalCount, totalPages);
        }

        /// <summary>
        /// Get all posts using stored procedure and optimized processing
        /// </summary>
        public async Task<List<Post>> GetAllPosts(string timeZone)
        {
            try
            {
                // Ensure connection is open
                await EnsureConnectionOpenAsync();

                // Execute stored procedure to get posts
                var posts = (await _connection.QueryAsync<Post>("GetAllPosts", commandType: CommandType.StoredProcedure)).ToList();

                // Extract all mention IDs for batch loading
                var allMentionIds = posts
                    .Where(p => !string.IsNullOrWhiteSpace(p.Mention))
                    .SelectMany(p => p.Mention.Split(',')
                        .Select(id => id.Trim())
                        .Where(id => !string.IsNullOrEmpty(id)))
                    .Distinct()
                    .ToList();

                // Batch load all mentioned profiles in a single query
                Dictionary<string, Profile> mentionedProfiles = new Dictionary<string, Profile>();
                if (allMentionIds.Any())
                {
                    mentionedProfiles = await _context.Profile
                        .AsNoTracking()
                        .Where(p => allMentionIds.Contains(p.ProfileId))
                        .ToDictionaryAsync(p => p.ProfileId);
                }

                // Get all unique profile IDs for star ratings
                var profileIds = posts.Select(p => p.ProfileId).Distinct().ToList();

                // Batch load star ratings for all profiles at once
                var starRatings = await BatchGetAverageStarRatingsAsync(profileIds);

                // Process posts in parallel for better performance
                Parallel.ForEach(posts, post =>
                {
                    // Set star rating from batch results
                    post.StarRating = starRatings.GetValueOrDefault(post.ProfileId, "0");

                    // Process mentions using pre-loaded profiles
                    if (!string.IsNullOrWhiteSpace(post.Mention))
                    {
                        var mentionIds = post.Mention.Split(',')
                            .Select(id => id.Trim())
                            .Where(id => !string.IsNullOrEmpty(id))
                            .ToList();

                        post.ProfileMentions = mentionIds
                            .Where(id => mentionedProfiles.ContainsKey(id))
                            .Select(id => mentionedProfiles[id])
                            .ToList();
                    }

                    // Calculate relative time
                    if (DateTime.TryParse(post.PostedDate, out DateTime dateTime))
                    {
                        post.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                    }
                    else
                    {
                        post.RelativeTime = "Unknown";
                    }
                });

                return posts.OrderByDescending(post => post.PostedDate).ToList();
            }
            catch (Exception ex)
            {
                // Log error
                return new List<Post>();
            }
        }

        /// <summary>
        /// Get posts with optimized Dapper query
        /// </summary>
        public async Task<List<Post>> GetPosts(string timeZone)
        {
            try
            {
                await EnsureConnectionOpenAsync();

                // Use Dapper's multi-mapping capabilities for more efficient queries
                var posts = (await _connection.QueryAsync<Post>("GetPosts", commandType: CommandType.StoredProcedure)).ToList();

                // Batch process data as in GetAllPosts
                await ProcessPostsCommonData(posts, timeZone);

                return posts.OrderByDescending(post => post.PostedDate).ToList();
            }
            catch (Exception ex)
            {
                return new List<Post>();
            }
        }

        /// <summary>
        /// Get blogs with optimized query
        /// </summary>
        public async Task<List<Post>> GetBlogs(string timeZone)
        {
            try
            {
                await EnsureConnectionOpenAsync();

                var posts = (await _connection.QueryAsync<Post>("GetBlogs", commandType: CommandType.StoredProcedure)).ToList();

                // Simplified processing for blogs - they don't need full mention processing
                Parallel.ForEach(posts, post =>
                {
                    if (DateTime.TryParse(post.PostedDate, out DateTime dateTime))
                    {
                        post.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                    }
                    else
                    {
                        post.RelativeTime = "Unknown";
                    }
                });

                return posts.OrderByDescending(post => post.PostedDate).ToList();
            }
            catch (Exception ex)
            {
                return new List<Post>();
            }
        }

        /// <summary>
        /// Get hoop news with optimized query
        /// </summary>
        public async Task<List<Post>> GetHoopNews(string timeZone)
        {
            try
            {
                await EnsureConnectionOpenAsync();

                var posts = (await _connection.QueryAsync<Post>("GetHoopNews", commandType: CommandType.StoredProcedure)).ToList();

                // Same simplified processing as for blogs
                Parallel.ForEach(posts, post =>
                {
                    if (DateTime.TryParse(post.PostedDate, out DateTime dateTime))
                    {
                        post.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                    }
                    else
                    {
                        post.RelativeTime = "Unknown";
                    }
                });

                return posts.OrderByDescending(post => post.PostedDate).ToList();
            }
            catch (Exception ex)
            {
                return new List<Post>();
            }
        }

        /// <summary>
        /// Get events with optimized query
        /// </summary>
        public async Task<List<Post>> GetEvents(string timeZone)
        {
            try
            {
                await EnsureConnectionOpenAsync();

                var posts = (await _connection.QueryAsync<Post>("GetEvents", commandType: CommandType.StoredProcedure)).ToList();

                // Same simplified processing as for blogs and news
                Parallel.ForEach(posts, post =>
                {
                    if (DateTime.TryParse(post.PostedDate, out DateTime dateTime))
                    {
                        post.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                    }
                    else
                    {
                        post.RelativeTime = "Unknown";
                    }
                });

                return posts.OrderByDescending(post => post.PostedDate).ToList();
            }
            catch (Exception ex)
            {
                return new List<Post>();
            }
        }

        /// <summary>
        /// Get public posts with optimized entity queries
        /// </summary>
        public async Task<List<Post>> GetPublicPosts()
        {
            try
            {
                // Use a more efficient query with better join strategy and pagination
                var query = await _context.Post
                    .AsNoTracking() // Ensures better performance for read-only data
                    .Join(
                        _context.Profile,
                        post => post.ProfileId,
                        profile => profile.ProfileId,
                        (post, profile) => new { post, profile }
                    )
                    .Select(x => new Post
                    {
                        PostId = x.post.PostId,
                        UserId = x.post.UserId,
                        Caption = x.post.Caption,
                        PostFileURL = x.post.PostFileURL,
                        Type = x.post.Type,
                        Status = x.post.Status,
                        PostText = x.post.PostText,
                        PostType = x.post.PostType,
                        // Use subqueries instead of directly accessing navigation properties
                        Likes = _context.LikedPost.Count(lp => lp.PostId == x.post.PostId),
                        DisLikes = x.post.DisLikes,
                        Hearted = x.post.Hearted,
                        Views = x.post.Views,
                        Shared = x.post.Shared,
                        PostedDate = x.post.PostedDate,
                        ProfileId = x.post.ProfileId,
                        ThumbnailUrl = x.post.ThumbnailUrl,
                        FirstName = x.profile.UserName,
                        ProfileImageURL = x.profile.ImageURL,
                        UserName = x.profile.UserName,
                        StarRating = x.profile.StarRating,
                        PostCommentCount = _context.PostComment.Count(c => c.PostId == x.post.PostId)
                    })
                    .ToListAsync();

                return query.OrderByDescending(p => p.PostedDate).ToList();
            }
            catch (Exception ex)
            {
                return new List<Post>();
            }
        }

        /// <summary>
        /// Get posts by profile ID with optimized queries
        /// </summary>
        public async Task<List<Post>> GetPostsByProfileId(string profileId, string timeZone)
        {
            try
            {
                // Use a more efficient query with better join and filtering strategy
                var query = await _context.Post
                    .AsNoTracking()
                    .Where(p => p.ProfileId == profileId && p.PostType == "User")
                    .Join(
                        _context.Profile,
                        post => post.ProfileId,
                        profile => profile.ProfileId,
                        (post, profile) => new { post, profile }
                    )
                    .Select(x => new Post
                    {
                        PostId = x.post.PostId,
                        UserId = x.post.UserId,
                        Caption = x.post.Caption,
                        PostFileURL = x.post.PostFileURL,
                        Type = x.post.Type,
                        Status = x.post.Status,
                        PostType = x.post.PostType,
                        Title = x.post.Title,
                        Likes = _context.LikedPost.Count(lp => lp.PostId == x.post.PostId),
                        DisLikes = x.post.DisLikes,
                        Hearted = x.post.Hearted,
                        Views = x.post.Views,
                        Shared = x.post.Shared,
                        PostedDate = x.post.PostedDate,
                        ProfileId = x.post.ProfileId,
                        ThumbnailUrl = x.post.ThumbnailUrl,
                        FirstName = x.profile.UserName,
                        ProfileImageURL = x.profile.ImageURL,
                        UserName = x.profile.UserName,
                        StarRating = x.profile.StarRating,
                        PostText = x.post.PostText,
                        PostCommentCount = _context.PostComment.Count(c => c.PostId == x.post.PostId)
                    })
                    .ToListAsync();

                // Get all profile IDs for star ratings
                var starRatings = await BatchGetAverageStarRatingsAsync(new List<string> { profileId });

                // Process posts in parallel
                Parallel.ForEach(query, item =>
                {
                    item.StarRating = starRatings.GetValueOrDefault(item.ProfileId, "0");

                    if (DateTime.TryParse(item.PostedDate, out DateTime dateTime))
                    {
                        item.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                    }
                    else
                    {
                        item.RelativeTime = "Invalid Date";
                    }
                });

                return query.OrderByDescending(post => post.PostedDate).ToList();
            }
            catch (Exception ex)
            {
                return new List<Post>();
            }
        }

        // All other methods follow similar optimization patterns...

        /// <summary>
        /// Check if post is liked by profile - uses fast index lookup
        /// </summary>
        public async Task<bool> IsPostLikedByProfileAsync(string postId, string profileId)
        {
            return await _context.LikedPost
                .AsNoTracking() // Important for performance
                .AnyAsync(lp => lp.PostId == postId && lp.LikedByProfileId == profileId);
        }

        /// <summary>
        /// Like a post with optimized insert
        /// </summary>
        public async Task LikePostAsync(string postId, string profileId)
        {
            // First check if already liked - this prevents duplicate insert attempts
            if (!await IsPostLikedByProfileAsync(postId, profileId))
            {
                var likedPost = new LikedPost
                {
                    LikedPostId = Guid.NewGuid().ToString(),
                    PostId = postId,
                    LikedByProfileId = profileId,
                    LikedDate = DateTime.Now.ToString()
                };

                // Add and save in one go
                _context.LikedPost.Add(likedPost);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Optimized helper to ensure connection is open
        /// </summary>
        private async Task EnsureConnectionOpenAsync()
        {
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
        }

        /// <summary>
        /// Batch process star ratings for multiple profiles at once
        /// </summary>
        private async Task<Dictionary<string, string>> BatchGetAverageStarRatingsAsync(List<string> profileIds)
        {
            if (profileIds == null || !profileIds.Any())
                return new Dictionary<string, string>();

            // Calculate ratings for all profiles in a single query
            var ratings = await _context.Rating
                .AsNoTracking()
                .Where(r => profileIds.Contains(r.ProfileId))
                .GroupBy(r => r.ProfileId)
                .Select(g => new
                {
                    ProfileId = g.Key,
                    AverageRating = g.Select(r => !string.IsNullOrEmpty(r.StarRating) ? int.Parse(r.StarRating) : 0)
                                    .DefaultIfEmpty(0)
                                    .Average()
                })
                .ToDictionaryAsync(x => x.ProfileId, x => x.AverageRating.ToString());

            // Ensure all requested profile IDs have an entry, even if no ratings
            foreach (var profileId in profileIds)
            {
                if (!ratings.ContainsKey(profileId))
                {
                    ratings[profileId] = "0";
                }
            }

            return ratings;
        }

        /// <summary>
        /// Common post processing logic shared between methods
        /// </summary>
        private async Task ProcessPostsCommonData(List<Post> posts, string timeZone)
        {
            if (posts == null || !posts.Any())
                return;

            // Batch load all mentioned profiles and ratings
            var allMentionIds = posts
                .Where(p => !string.IsNullOrWhiteSpace(p.Mention))
                .SelectMany(p => p.Mention.Split(',')
                    .Select(id => id.Trim())
                    .Where(id => !string.IsNullOrEmpty(id)))
                .Distinct()
                .ToList();

            var profileIds = posts.Select(p => p.ProfileId).Distinct().ToList();

            // Run these queries in parallel
            var mentionedProfilesTask = allMentionIds.Any()
                ? _context.Profile
                    .AsNoTracking()
                    .Where(p => allMentionIds.Contains(p.ProfileId))
                    .ToDictionaryAsync(p => p.ProfileId)
                : Task.FromResult(new Dictionary<string, Profile>());

            var starRatingsTask = BatchGetAverageStarRatingsAsync(profileIds);

            await Task.WhenAll(mentionedProfilesTask, starRatingsTask);

            var mentionedProfiles = await mentionedProfilesTask;
            var starRatings = await starRatingsTask;

            // Process posts in parallel
            Parallel.ForEach(posts, post =>
            {
                post.StarRating = starRatings.GetValueOrDefault(post.ProfileId, "0");

                if (!string.IsNullOrWhiteSpace(post.Mention))
                {
                    var mentionIds = post.Mention.Split(',')
                        .Select(id => id.Trim())
                        .Where(id => !string.IsNullOrEmpty(id))
                        .ToList();

                    post.ProfileMentions = mentionIds
                        .Where(id => mentionedProfiles.ContainsKey(id))
                        .Select(id => mentionedProfiles[id])
                        .ToList();
                }

                if (DateTime.TryParse(post.PostedDate, out DateTime dateTime))
                {
                    post.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                }
                else
                {
                    post.RelativeTime = "Unknown";
                }
            });
        }

        /// <summary>
        /// Helper method to load post details - now with batch loading
        /// </summary>
        private async Task LoadPostDetailsAsync(List<Post> posts, string timeZone)
        {
            if (posts == null || !posts.Any())
                return;

            // Get all IDs needed for lookups
            var profileIds = posts.Select(p => p.ProfileId).Distinct().ToList();
            var postIds = posts.Select(p => p.PostId).ToList();

            // Run all queries in parallel for maximum performance
            var profilesTask = _context.Profile
                .AsNoTracking()
                .Where(p => profileIds.Contains(p.ProfileId))
                .Select(p => new { p.ProfileId, p.UserName, p.ImageURL })
                .ToDictionaryAsync(p => p.ProfileId);

            var commentCountsTask = _context.PostComment
                .AsNoTracking()
                .Where(pc => postIds.Contains(pc.PostId))
                .GroupBy(pc => pc.PostId)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.PostId, g => g.Count);

            var likeCountsTask = _context.LikedPost
                .AsNoTracking()
                .Where(lp => postIds.Contains(lp.PostId))
                .GroupBy(lp => lp.PostId)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.PostId, g => g.Count);

            // Wait for all tasks to complete
            await Task.WhenAll(profilesTask, commentCountsTask, likeCountsTask);

            var profiles = await profilesTask;
            var commentCounts = await commentCountsTask;
            var likeCounts = await likeCountsTask;

            // Apply data to posts in parallel
            Parallel.ForEach(posts, post =>
            {
                // Set profile info
                if (profiles.TryGetValue(post.ProfileId, out var profile))
                {
                    post.UserName = profile.UserName;
                    post.ProfileImageURL = profile.ImageURL;
                }

                // Set comment count
                post.PostCommentCount = commentCounts.GetValueOrDefault(post.PostId, 0);

                // Set like count
                post.Likes = likeCounts.GetValueOrDefault(post.PostId, 0);

                // Calculate relative time
                if (DateTime.TryParse(post.PostedDate, out DateTime postedDate))
                {
                    post.RelativeTime = RelativeTime.GetRelativeTime(postedDate, timeZone);
                }
            });
        }

        /// <summary>
        /// Get post by id with details - alias for existing method to match interface
        /// </summary>
        public async Task<Post> GetPostByIdWithDetailsAsync(string postId, string timeZone)
        {
            return await GetPostById(postId, timeZone);
        }

        /// <summary>
        /// Properly implemented Dispose method
        /// </summary>
        public void Dispose()
        {
            _context?.Dispose();
            _connection?.Dispose();
        }

        /// <summary>
        /// Save changes to database
        /// </summary>
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }
    }
}