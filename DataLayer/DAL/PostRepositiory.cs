using Common;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataLayer.DAL
{
    public class PostRepository : IPostRepository, IDisposable
    {
        private readonly IConfiguration _config;
        private readonly HUDBContext _context;
        private static readonly object _cacheLock = new object();
        private static Dictionary<string, object> _cacheItems = new Dictionary<string, object>();
        private static DateTime _cacheLastRefreshed = DateTime.MinValue;
        private const int CACHE_EXPIRATION_MINUTES = 10;

        /// <summary>
        /// Post Repository constructor
        /// </summary>
        public PostRepository(HUDBContext context, IConfiguration config)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Get posts with cursor-based pagination optimized for mobile scrolling
        /// </summary>
        public async Task<(List<Post> Posts, string NextCursor, bool HasMore)> GetPostsWithCursorAsync(
            string cursor = null,
            int limit = 10,
            string timeZone = "America/New_York",
            CancellationToken cancellationToken = default)
        {
            if (limit < 1) limit = 10;
            if (limit > 50) limit = 50; // Cap maximum items to prevent performance issues

            try
            {
                // Parse cursor if provided
                DateTime? cursorDate = null;
                string cursorPostId = null;

                if (!string.IsNullOrEmpty(cursor))
                {
                    try
                    {
                        var decodedCursor = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
                        var parts = decodedCursor.Split('|');

                        if (parts.Length == 2)
                        {
                            if (DateTime.TryParse(parts[0], out DateTime parsedDate))
                            {
                                cursorDate = parsedDate;
                            }
                            cursorPostId = parts[1];
                        }
                    }
                    catch
                    {
                        // If cursor parsing fails, start from the beginning
                        cursorDate = null;
                        cursorPostId = null;
                    }
                }

                // Build query
                IQueryable<Post> query = _context.Post
                    .AsNoTracking()
                    .Where(p => p.Status == "Active" && p.PostType == "User");

                // Apply cursor-based pagination
                if (cursorDate.HasValue && !string.IsNullOrEmpty(cursorPostId))
                {
                    // When cursor provided, get posts that are older than cursor
                    // (for posts with same date, use ID for deterministic ordering)
                    query = query.Where(p =>
                        (DateTime.Parse(p.PostedDate) < cursorDate) ||
                        (DateTime.Parse(p.PostedDate) == cursorDate && string.Compare(p.PostId, cursorPostId) < 0));
                }

                // Order by date descending (newest first), then by ID for stability
                query = query.OrderByDescending(p => p.PostedDate)
                             .ThenByDescending(p => p.PostId);

                // Get one extra item to determine if there are more results
                var posts = await query.Take(limit + 1).ToListAsync(cancellationToken);

                bool hasMore = posts.Count > limit;
                string nextCursor = null;

                // If we have more results, prepare next cursor and remove the extra item
                if (hasMore)
                {
                    var lastPost = posts[limit - 1];

                    // Create cursor from last returned post (date|id)
                    var cursorValue = $"{DateTime.Parse(lastPost.PostedDate)}|{lastPost.PostId}";
                    nextCursor = Convert.ToBase64String(Encoding.UTF8.GetBytes(cursorValue));

                    // Remove the extra item
                    posts.RemoveAt(limit);
                }

                // Enrich posts with essential details
                await EnrichPostsWithBasicDetailsAsync(posts, timeZone);

                return (posts, nextCursor, hasMore);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPostsWithCursorAsync: {ex.Message}");
                return (new List<Post>(), null, false);
            }
        }

        /// <summary>
        /// Invalidate cache to force reload on next request
        /// </summary>
        public void InvalidateCache()
        {
            lock (_cacheLock)
            {
                _cacheItems.Clear();
                _cacheLastRefreshed = DateTime.MinValue;
            }
        }

        /// <summary>
        /// Enrich posts with essential details for display
        /// </summary>
        private async Task EnrichPostsWithBasicDetailsAsync(List<Post> posts, string timeZone)
        {
            if (posts == null || !posts.Any())
                return;

            try
            {
                // Get all IDs needed for batch lookups
                var profileIds = posts.Select(p => p.ProfileId).Distinct().ToList();
                var postIds = posts.Select(p => p.PostId).ToList();

                // Run queries in parallel for better performance
                var profilesTask = _context.Profile
                    .AsNoTracking()
                    .Where(p => profileIds.Contains(p.ProfileId))
                    .Select(p => new { p.ProfileId, p.UserName, p.ImageURL })
                    .ToDictionaryAsync(p => p.ProfileId);

                var likesTask = _context.LikedPost
                    .AsNoTracking()
                    .Where(lp => postIds.Contains(lp.PostId))
                    .GroupBy(lp => lp.PostId)
                    .Select(g => new { PostId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(g => g.PostId, g => g.Count);

                var commentsTask = _context.PostComment
                    .AsNoTracking()
                    .Where(pc => postIds.Contains(pc.PostId))
                    .GroupBy(pc => pc.PostId)
                    .Select(g => new { PostId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(g => g.PostId, g => g.Count);

                // Wait for all tasks to complete
                await Task.WhenAll(profilesTask, likesTask, commentsTask);

                var profiles = await profilesTask;
                var likes = await likesTask;
                var comments = await commentsTask;

                // Enrich posts with retrieved data
                foreach (var post in posts)
                {
                    // Set profile info
                    if (profiles.TryGetValue(post.ProfileId, out var profile))
                    {
                        post.UserName = profile.UserName;
                        post.ProfileImageURL = profile.ImageURL;
                    }

                    // Set like count
                    post.Likes = likes.GetValueOrDefault(post.PostId, 0);

                    // Set comment count
                    post.PostCommentCount = comments.GetValueOrDefault(post.PostId, 0);

                    // Calculate relative time
                    if (DateTime.TryParse(post.PostedDate, out DateTime dateTime))
                    {
                        post.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                    }
                    else
                    {
                        post.RelativeTime = "Unknown";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enriching posts: {ex.Message}");
            }
        }

        // Implementation of existing methods...
        // [Keep all existing methods from the original PostRepository]

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
                await EnrichPostsWithBasicDetailsAsync(posts, timeZone);

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
        public async Task<List<Post>> GetPosts(string timeZone)
        {
            try
            {
                // Check cache first
                string cacheKey = $"AllPosts_{timeZone}";
                if (TryGetFromCache(cacheKey, out List<Post> cachedPosts))
                {
                    return cachedPosts;
                }

                // Fetch posts using EF Core, filtering for PostType = "User"
                var posts = await _context.Post
                    .AsNoTracking()
                    .Where(p => p.PostType == "User" && p.Status == "Active")
                    .OrderByDescending(p => p.PostedDate)
                    .ToListAsync();

                // Process common data for posts
                await EnrichPostsWithBasicDetailsAsync(posts, timeZone);

                // Cache the results
                AddToCache(cacheKey, posts);

                return posts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting posts: {ex.Message}");
                return new List<Post>();
            }
        }

        /// <summary>
        /// Get posts by profile ID
        /// </summary>
        public async Task<List<Post>> GetPostsByProfileId(string profileId, string timeZone)
        {
            try
            {
                // Cache key for this specific profile's posts
                string cacheKey = $"ProfilePosts_{profileId}_{timeZone}";
                if (TryGetFromCache(cacheKey, out List<Post> cachedPosts))
                {
                    return cachedPosts;
                }

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

                // Process posts in parallel
                Parallel.ForEach(query, item =>
                {
                    if (DateTime.TryParse(item.PostedDate, out DateTime dateTime))
                    {
                        item.RelativeTime = RelativeTime.GetRelativeTime(dateTime, timeZone);
                    }
                    else
                    {
                        item.RelativeTime = "Invalid Date";
                    }
                });

                var result = query.OrderByDescending(post => post.PostedDate).ToList();

                // Cache the results
                AddToCache(cacheKey, result);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting posts by profile ID: {ex.Message}");
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
                // Cache key for blogs
                string cacheKey = $"Blogs_{timeZone}";
                if (TryGetFromCache(cacheKey, out List<Post> cachedPosts))
                {
                    return cachedPosts;
                }

                // Use EF Core to fetch blog posts
                var posts = await _context.Post
                    .AsNoTracking()
                    .Where(p => p.PostType == "Blog" && p.Status == "Active")
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

                // Cache the results
                AddToCache(cacheKey, posts);

                return posts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting blogs: {ex.Message}");
                return new List<Post>();
            }
        }

        /// <summary>
        /// Cache helpers
        /// </summary>
        private bool TryGetFromCache<T>(string key, out T value)
        {
            lock (_cacheLock)
            {
                // Check if cache is expired
                if ((DateTime.Now - _cacheLastRefreshed).TotalMinutes > CACHE_EXPIRATION_MINUTES)
                {
                    _cacheItems.Clear();
                    _cacheLastRefreshed = DateTime.Now;
                    value = default;
                    return false;
                }

                // Try to get from cache
                if (_cacheItems.TryGetValue(key, out object cachedValue) && cachedValue is T typedValue)
                {
                    value = typedValue;
                    return true;
                }

                value = default;
                return false;
            }
        }

        private void AddToCache<T>(string key, T value)
        {
            lock (_cacheLock)
            {
                _cacheItems[key] = value;
                _cacheLastRefreshed = DateTime.Now;
            }
        }

        // Keep the rest of your existing methods here...

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

                    // Invalidate any cached posts since counts have changed
                    InvalidateCache();
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

                    // Invalidate any cached posts since counts have changed
                    InvalidateCache();
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

                // Invalidate cache when a new post is added
                InvalidateCache();
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

                    // Invalidate cache when a post is deleted
                    InvalidateCache();
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

                    // Invalidate cache when a post is updated
                    InvalidateCache();
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

                    // Invalidate cache when a post status is updated
                    InvalidateCache();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating post status: {ex.Message}");
                throw;
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
        /// Get hoop news with optimized query
        /// </summary>
        public async Task<List<Post>> GetHoopNews(string timeZone)
        {
            try
            {
                // Cache key for hoop news
                string cacheKey = $"HoopNews_{timeZone}";
                if (TryGetFromCache(cacheKey, out List<Post> cachedPosts))
                {
                    return cachedPosts;
                }

                // Use EF Core to fetch hoop news posts
                var posts = await _context.Post
                    .AsNoTracking()
                    .Where(p => p.PostType == "HoopNews" && p.Status == "Active")
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

                // Cache the results
                AddToCache(cacheKey, posts);

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
                // Cache key for events
                string cacheKey = $"Events_{timeZone}";
                if (TryGetFromCache(cacheKey, out List<Post> cachedPosts))
                {
                    return cachedPosts;
                }

                // Use EF Core to fetch event posts
                var posts = await _context.Post
                    .AsNoTracking()
                    .Where(p => p.PostType == "Event" && p.Status == "Active")
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

                // Cache the results
                AddToCache(cacheKey, posts);

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
                // Cache key for public posts
                string cacheKey = "PublicPosts";
                if (TryGetFromCache(cacheKey, out List<Post> cachedPosts))
                {
                    return cachedPosts;
                }

                // Use a more efficient query with better join strategy and pagination
                var query = await _context.Post
                    .AsNoTracking() // Ensures better performance for read-only data
                    .Where(p => p.Status == "Active")
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

                var result = query.OrderByDescending(p => p.PostedDate).ToList();

                // Cache the results
                AddToCache(cacheKey, result);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting public posts: {ex.Message}");
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
                    .Where(p => p.Mention != null && p.Mention.Contains(profileId) && p.Status == "Active")
                    .ToListAsync();

                // Process common data for these posts
                if (posts.Any())
                {
                    await EnrichPostsWithBasicDetailsAsync(posts, timeZone);
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
                    .Where(p => p.Status == "Active")
                    .ToListAsync();

                // Process common data for these posts
                if (posts.Any())
                {
                    await EnrichPostsWithBasicDetailsAsync(posts, timeZone);
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
                    .Where(p => savedPostIds.Contains(p.PostId) && p.Status == "Active")
                    .ToListAsync();

                // Process common data for these posts
                await EnrichPostsWithBasicDetailsAsync(posts, timeZone);

                return posts.OrderByDescending(p => p.PostedDate).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting saved posts: {ex.Message}");
                return new List<Post>();
            }
        }

        /// <summary>
        /// Get all posts including drafts and archived
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

                // Process posts with basic details
                await EnrichPostsWithBasicDetailsAsync(posts, timeZone);

                return posts.OrderByDescending(post => post.PostedDate).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all posts: {ex.Message}");
                return new List<Post>();
            }
        }

        /// <summary>
        /// Save changes to database
        /// </summary>
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Properly implemented Dispose method
        /// </summary>
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}