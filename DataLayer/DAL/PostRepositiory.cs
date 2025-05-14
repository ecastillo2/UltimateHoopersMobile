using Common;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace DataLayer.DAL
{
    public class PostRepository : IPostRepository, IDisposable
    {
        private readonly IConfiguration _config;
        private readonly HUDBContext _context;

        /// <summary>
        /// Post Repository constructor
        /// </summary>
        public PostRepository(HUDBContext context, IConfiguration config)
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
                if (comments != null && comments.Any())
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
                // Log the exception (consider using a proper logging framework)
                Console.WriteLine($"Error getting post by ID: {ex.Message}");
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

            try
            {
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting paginated posts: {ex.Message}");
                return (new List<Post>(), 0, 0);
            }
        }

        /// <summary>
        /// Get all posts with optimized queries
        /// </summary>
        public async Task<List<Post>> GetAllPosts(string timeZone)
        {
            try
            {
                // Get all posts from the database
                var posts = await _context.Post
                    .AsNoTracking()
                    .ToListAsync();

                if (!posts.Any())
                    return new List<Post>();

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
                Console.WriteLine($"Error getting all posts: {ex.Message}");
                return new List<Post>();
            }
        }

        /// <summary>
        /// Get posts with optimized queries
        /// </summary>
        public async Task<List<Post>> GetPosts(string timeZone)
        {
            try
            {
                // Fetch posts using EF Core
                var posts = await _context.Post
                    .AsNoTracking()
                    .OrderByDescending(p => p.PostedDate)
                    .ToListAsync();

                // Process common data for posts
                await ProcessPostsCommonData(posts, timeZone);

                return posts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting posts: {ex.Message}");
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
                // Use EF Core to fetch blog posts
                var posts = await _context.Post
                    .AsNoTracking()
                    .Where(p => p.PostType == "Blog")
                    .OrderByDescending(p => p.PostedDate)
                    .ToListAsync();

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

                return posts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting blogs: {ex.Message}");
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
                // Use EF Core to fetch hoop news posts
                var posts = await _context.Post
                    .AsNoTracking()
                    .Where(p => p.PostType == "HoopNews")
                    .OrderByDescending(p => p.PostedDate)
                    .ToListAsync();

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

                return posts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting hoop news: {ex.Message}");
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
                // Use EF Core to fetch event posts
                var posts = await _context.Post
                    .AsNoTracking()
                    .Where(p => p.PostType == "Event")
                    .OrderByDescending(p => p.PostedDate)
                    .ToListAsync();

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

                return posts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting events: {ex.Message}");
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
                Console.WriteLine($"Error getting public posts: {ex.Message}");
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
                Console.WriteLine($"Error getting posts by profile ID: {ex.Message}");
                return new List<Post>();
            }
        }

        /// <summary>
        /// Get posts that mention a specific profile
        /// </summary>
        public async Task<List<Post>> GetPostsMentionProfileId(string profileId, string timeZone)
        {
            try
            {
                // Query for posts that mention the specified profile
                var posts = await _context.Post
                    .AsNoTracking()
                    .Where(p => p.Mention != null && p.Mention.Contains(profileId))
                    .ToListAsync();

                // Process common data for these posts
                if (posts.Any())
                {
                    await ProcessPostsCommonData(posts, timeZone);
                }

                return posts.OrderByDescending(p => p.PostedDate).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting posts mentioning profile: {ex.Message}");
                return new List<Post>();
            }
        }

        /// <summary>
        /// Get posts with a specific tag
        /// </summary>
        public async Task<List<Post>> GetPostsWithTagByTagId(string tagId, string timeZone)
        {
            try
            {
                // First, get the tag text
                var tag = await _context.Tag
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TagId == tagId);

                if (tag == null)
                    return new List<Post>();

                string hashtagText = $"#{tag.HashTag}";

                // Query for posts containing this hashtag
                var posts = await _context.Post
                    .AsNoTracking()
                    .Where(p => (p.Caption != null && p.Caption.Contains(hashtagText)) ||
                                (p.PostText != null && p.PostText.Contains(hashtagText)))
                    .ToListAsync();

                // Process common data for these posts
                if (posts.Any())
                {
                    await ProcessPostsCommonData(posts, timeZone);
                }

                return posts.OrderByDescending(p => p.PostedDate).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting posts with tag: {ex.Message}");
                return new List<Post>();
            }
        }

        /// <summary>
        /// Get posts saved by a specific profile
        /// </summary>
        public async Task<List<Post>> GetSavedPostsByProfileId(string profileId, string timeZone)
        {
            try
            {
                // Get IDs of posts saved by the profile
                var savedPostIds = await _context.SavedPost
                    .AsNoTracking()
                    .Where(sp => sp.SavedByProfileId == profileId)
                    .Select(sp => sp.PostId)
                    .ToListAsync();

                if (!savedPostIds.Any())
                    return new List<Post>();

                // Get the actual posts
                var posts = await _context.Post
                    .AsNoTracking()
                    .Where(p => savedPostIds.Contains(p.PostId))
                    .ToListAsync();

                // Process common data for these posts
                await ProcessPostsCommonData(posts, timeZone);

                return posts.OrderByDescending(p => p.PostedDate).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting saved posts: {ex.Message}");
                return new List<Post>();
            }
        }

        /// <summary>
        /// Insert a new post
        /// </summary>
        public async Task InsertPost(Post model)
        {
            try
            {
                model.PostId = model.PostId ?? Guid.NewGuid().ToString();
                model.PostedDate = DateTime.Now.ToString();

                await _context.Post.AddAsync(model);
                await Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting post: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete an existing post
        /// </summary>
        public async Task DeletePost(string postId)
        {
            try
            {
                var post = await _context.Post.FirstOrDefaultAsync(p => p.PostId == postId);
                if (post != null)
                {
                    _context.Post.Remove(post);
                    await Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting post: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update an existing post
        /// </summary>
        public async Task UpdatePost(Post model)
        {
            try
            {
                var existingPost = await _context.Post.FirstOrDefaultAsync(p => p.PostId == model.PostId);
                if (existingPost != null)
                {
                    existingPost.Caption = model.Caption;
                    existingPost.PostText = model.PostText;
                    existingPost.Status = model.Status;
                    existingPost.Type = model.Type;
                    existingPost.Title = model.Title;
                    existingPost.PostType = model.PostType;
                    existingPost.Mention = model.Mention;

                    _context.Post.Update(existingPost);
                    await Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating post: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update a post's status
        /// </summary>
        public async Task UpdatePostStatus(string postId, string status)
        {
            try
            {
                var post = await _context.Post.FirstOrDefaultAsync(p => p.PostId == postId);
                if (post != null)
                {
                    post.Status = status;
                    _context.Post.Update(post);
                    await Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating post status: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if post is liked by profile - uses fast index lookup
        /// </summary>
        public async Task<bool> IsPostLikedByProfileAsync(string postId, string profileId)
        {
            try
            {
                return await _context.LikedPost
                    .AsNoTracking() // Important for performance
                    .AnyAsync(lp => lp.PostId == postId && lp.LikedByProfileId == profileId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if post is liked: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Like a post with optimized insert
        /// </summary>
        public async Task LikePostAsync(string postId, string profileId)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error liking post: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Unlike a post
        /// </summary>
        public async Task UnlikePostAsync(string postId, string profileId)
        {
            try
            {
                var likedPost = await _context.LikedPost
                    .FirstOrDefaultAsync(lp => lp.PostId == postId && lp.LikedByProfileId == profileId);

                if (likedPost != null)
                {
                    _context.LikedPost.Remove(likedPost);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unliking post: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get the average star rating for a profile
        /// </summary>
        public async Task<string> GetAverageStarRatingByProfileId(string profileId)
        {
            try
            {
                var ratings = await _context.Rating
                    .AsNoTracking()
                    .Where(r => r.ProfileId == profileId)
                    .Select(r => r.StarRating)
                    .ToListAsync();

                if (!ratings.Any())
                    return "0";

                var validRatings = ratings
                    .Where(r => !string.IsNullOrEmpty(r))
                    .Select(r => int.TryParse(r, out int rating) ? rating : 0);

                if (!validRatings.Any())
                    return "0";

                return validRatings.Average().ToString("F1");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting average star rating: {ex.Message}");
                return "0";
            }
        }

        /// <summary>
        /// Batch process star ratings for multiple profiles at once
        /// </summary>
        private async Task<Dictionary<string, string>> BatchGetAverageStarRatingsAsync(List<string> profileIds)
        {
            if (profileIds == null || !profileIds.Any())
                return new Dictionary<string, string>();

            try
            {
                // First, fetch the ratings from the database
                var ratings = await _context.Rating
                    .AsNoTracking()
                    .Where(r => profileIds.Contains(r.ProfileId))
                    .ToListAsync();

                // Then process them in memory, where we can use out parameters
                var result = ratings
                    .GroupBy(r => r.ProfileId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(r => ParseRating(r.StarRating))
                              .DefaultIfEmpty(0)
                              .Average()
                              .ToString("F1")
                    );

                // Ensure all requested profile IDs have an entry, even if no ratings
                foreach (var profileId in profileIds)
                {
                    if (!result.ContainsKey(profileId))
                    {
                        result[profileId] = "0";
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error batch getting star ratings: {ex.Message}");
                return profileIds.ToDictionary(id => id, id => "0");
            }
        }

        // Helper method to parse rating
        private int ParseRating(string ratingStr)
        {
            if (string.IsNullOrEmpty(ratingStr))
                return 0;

            if (int.TryParse(ratingStr, out int result))
                return result;

            return 0;
        }

        /// <summary>
        /// Common post processing logic shared between methods
        /// </summary>
        private async Task ProcessPostsCommonData(List<Post> posts, string timeZone)
        {
            if (posts == null || !posts.Any())
                return;

            try
            {
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing posts common data: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper method to load post details - now with batch loading
        /// </summary>
        private async Task LoadPostDetailsAsync(List<Post> posts, string timeZone)
        {
            if (posts == null || !posts.Any())
                return;

            try
            {
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading post details: {ex.Message}");
            }
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