using Common;
using Common.Extensions;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataLayer.DAL
{
    /// <summary>
    /// Highly optimized implementation of ProfileRepository with advanced performance enhancements
    /// </summary>
    public class ProfileRepository : IProfileRepository, IDisposable
    {
        private readonly HUDBContext _context;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private bool _disposed = false;
        private readonly SemaphoreSlim _cacheSemaphore = new SemaphoreSlim(1, 1);
        private int _failureCount = 0;
        private bool _circuitOpen = false;
        private DateTime _circuitResetTime = DateTime.MinValue;

        // Cache keys and durations with more granular control
        private const string PROFILE_CACHE_KEY_PREFIX = "Profile_";
        private const string FOLLOWERS_CACHE_KEY_PREFIX = "ProfileFollowers_";
        private const string FOLLOWING_CACHE_KEY_PREFIX = "ProfileFollowing_";
        private const string GAME_STATS_CACHE_KEY_PREFIX = "ProfileGameStats_";
        private const string ALL_PROFILES_CACHE_KEY = "AllProfiles_";
        private const string STAR_RATINGS_CACHE_KEY_PREFIX = "StarRatings_";
        private const string CACHE_INITIALIZED_KEY = "ProfileCacheInitialized";

        // Differentiated cache durations based on data volatility
        private static readonly TimeSpan PROFILE_CACHE_DURATION = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan SOCIAL_CACHE_DURATION = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan GAME_STATS_CACHE_DURATION = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan STAR_RATINGS_CACHE_DURATION = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan DB_QUERY_TIMEOUT = TimeSpan.FromSeconds(30);

        // Cache options with sliding expiration for memory optimization
        private static readonly MemoryCacheEntryOptions PROFILE_CACHE_OPTIONS = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(PROFILE_CACHE_DURATION)
            .SetSlidingExpiration(TimeSpan.FromMinutes(3));

        private static readonly MemoryCacheEntryOptions SOCIAL_CACHE_OPTIONS = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(SOCIAL_CACHE_DURATION)
            .SetSlidingExpiration(TimeSpan.FromMinutes(1));

        private static readonly MemoryCacheEntryOptions GAME_STATS_CACHE_OPTIONS = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(GAME_STATS_CACHE_DURATION)
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));

        private static readonly MemoryCacheEntryOptions STAR_RATINGS_CACHE_OPTIONS = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(STAR_RATINGS_CACHE_DURATION)
            .SetSlidingExpiration(TimeSpan.FromMinutes(10));

        // Optimized parallel processing configuration
        private static readonly ParallelOptions PARALLEL_OPTIONS = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2)
        };

        // Compiled queries for frequently used operations
        private static readonly Func<HUDBContext, string, Task<Profile>> GetProfileByIdQuery =
            EF.CompileAsyncQuery((HUDBContext context, string profileId) =>
                context.Profile
                    .AsNoTracking()
                    .FirstOrDefault(p => p.ProfileId == profileId));

        private static readonly Func<HUDBContext, List<string>, Task<Dictionary<string, int>>> GetFollowerCountsQuery =
            EF.CompileAsyncQuery((HUDBContext context, List<string> ids) =>
                context.Follower
                    .AsNoTracking()
                    .Where(f => ids.Contains(f.FollowerProfileId))
                    .GroupBy(f => f.FollowerProfileId)
                    .Select(g => new { ProfileId = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.ProfileId, x => x.Count));

        private static readonly Func<HUDBContext, List<string>, Task<Dictionary<string, int>>> GetFollowingCountsQuery =
            EF.CompileAsyncQuery((HUDBContext context, List<string> ids) =>
                context.Following
                    .AsNoTracking()
                    .Where(f => ids.Contains(f.ProfileId))
                    .GroupBy(f => f.ProfileId)
                    .Select(g => new { ProfileId = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.ProfileId, x => x.Count));

        private static readonly Func<HUDBContext, int, int, Task<List<Profile>>> GetPaginatedProfilesQuery =
            EF.CompileAsyncQuery((HUDBContext context, int skip, int take) =>
                context.Profile
                    .AsNoTracking()
                    .OrderBy(p => p.UserName)
                    .Skip(skip)
                    .Take(take)
                    .ToList());

        /// <summary>
        /// Constructor that takes dependencies via DI
        /// </summary>
        public ProfileRepository(HUDBContext context, IConfiguration config, IMemoryCache cache = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _cache = cache; // Optional - fallback gracefully if not provided

            // Initialize cache asynchronously if available
            if (_cache != null && !_cache.TryGetValue(CACHE_INITIALIZED_KEY, out _))
            {
                _ = Task.Run(async () =>
                {
                    await WarmUpCacheAsync(CancellationToken.None);
                    _cache.Set(CACHE_INITIALIZED_KEY, true, TimeSpan.FromHours(1));
                });
            }
        }

        /// <summary>
        /// Warm up cache with frequently accessed data
        /// </summary>
        private async Task WarmUpCacheAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Get total count for pagination
                int totalCount = await _context.Profile
                    .AsNoTracking()
                    .CountAsync(cancellationToken);

                // Cache the count
                _cache?.Set("ProfilesTotalCount", totalCount, TimeSpan.FromMinutes(10));

                // Pre-cache first page of profiles
                await GetProfilesPaginated(1, 20, cancellationToken);

                // Get some of the most active profiles for pre-caching
                var topProfileIds = await _context.Profile
                    .AsNoTracking()
                    .OrderByDescending(p => p.LastLoginDate)
                    .Take(10)
                    .Select(p => p.ProfileId)
                    .ToListAsync(cancellationToken);

                // Pre-load these profiles in parallel
                var tasks = topProfileIds.Select(id => GetProfileById(id, cancellationToken)).ToList();
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cache warm-up error: {ex.Message}");
                // Non-critical error, continue without cache warming
            }
        }

        /// <summary>
        /// Create a linked cancellation token with timeout
        /// </summary>
        private CancellationTokenSource CreateTimeoutSource(
            TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var timeoutSource = new CancellationTokenSource(timeout);
            var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
                timeoutSource.Token, cancellationToken);
            return linkedSource;
        }

        /// <summary>
        /// Execute operation with circuit breaker pattern for resilience
        /// </summary>
        private async Task<T> ExecuteWithCircuitBreaker<T>(
            Func<Task<T>> operation,
            T fallbackValue,
            CancellationToken cancellationToken)
        {
            // Check if circuit is open
            if (_circuitOpen)
            {
                if (DateTime.UtcNow < _circuitResetTime)
                {
                    return fallbackValue;
                }

                _circuitOpen = false;
                _failureCount = 0;
            }

            try
            {
                // Set timeout for the operation
                using var timeoutSource = CreateTimeoutSource(DB_QUERY_TIMEOUT, cancellationToken);
                return await operation().WithCancellation(timeoutSource.Token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                await _cacheSemaphore.WaitAsync(cancellationToken);
                try
                {
                    _failureCount++;
                    if (_failureCount >= 5)
                    {
                        _circuitOpen = true;
                        _circuitResetTime = DateTime.UtcNow.AddMinutes(1);
                    }
                }
                finally
                {
                    _cacheSemaphore.Release();
                }

                Console.WriteLine($"Circuit breaker caught exception: {ex.Message}");
                return fallbackValue;
            }
        }

        /// <summary>
        /// Get or create cache entry with double-checked locking to prevent cache stampede
        /// </summary>
        private async Task<T> GetOrCreateCacheAsync<T>(
            string cacheKey,
            Func<Task<T>> dataFactory,
            MemoryCacheEntryOptions options,
            CancellationToken cancellationToken)
        {
            // Quick check without lock
            if (_cache != null && _cache.TryGetValue(cacheKey, out T cachedValue))
            {
                return cachedValue;
            }

            // Use cache or just execute if no cache available
            if (_cache == null)
            {
                return await dataFactory();
            }

            // Get or create semaphore for this key
            var lockObj = _cache.GetOrCreate($"{cacheKey}_lock", entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromSeconds(10));
                return new SemaphoreSlim(1, 1);
            });

            await lockObj.WaitAsync(cancellationToken);
            try
            {
                // Check cache again after acquiring lock
                if (_cache.TryGetValue(cacheKey, out cachedValue))
                {
                    return cachedValue;
                }

                // Execute data factory
                var result = await dataFactory();

                if (result != null)
                {
                    _cache.Set(cacheKey, result, options);
                }

                return result;
            }
            finally
            {
                lockObj.Release();
            }
        }

        /// <summary>
        /// Get profiles with pagination and optimized caching for large datasets
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Paginated list of profiles</returns>
        public async Task<(List<Profile> Profiles, int TotalCount, int TotalPages)> GetProfilesPaginated(
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            // Validate and normalize page parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100; // Limit maximum page size for performance

            // Generate a deterministic cache key based on pagination parameters
            string cacheKey = $"Profiles_Page{page}_Size{pageSize}";

            return await GetOrCreateCacheAsync(
                cacheKey,
                async () => await FetchPaginatedProfilesAsync(page, pageSize, cancellationToken),
                PROFILE_CACHE_OPTIONS,
                cancellationToken);
        }

        /// <summary>
        /// Internal method to fetch paginated profiles from the database
        /// </summary>
        private async Task<(List<Profile> Profiles, int TotalCount, int TotalPages)> FetchPaginatedProfilesAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            return await ExecuteWithCircuitBreaker(
                async () =>
                {
                    // Execute count query separately - use cache for repeated count operations
                    int totalCount;
                    string countCacheKey = "ProfilesTotalCount";

                    if (_cache != null && _cache.TryGetValue(countCacheKey, out int cachedCount))
                    {
                        totalCount = cachedCount;
                    }
                    else
                    {
                        totalCount = await _context.Profile
                            .AsNoTracking()
                            .CountAsync(cancellationToken);

                        // Cache the count for 10 minutes - count changes less frequently
                        _cache?.Set(countCacheKey, totalCount, TimeSpan.FromMinutes(10));
                    }

                    // Calculate total pages once
                    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                    // Skip/Take calculation
                    int skip = (page - 1) * pageSize;

                    // Optimized query for profiles with minimal columns selected
                    var profilesQuery = _context.Profile
                        .AsNoTracking()
                        .Join(_context.User,
                            p => p.UserId,
                            u => u.UserId,
                            (profile, user) => new Profile
                            {
                                ProfileId = profile.ProfileId,
                                UserId = profile.UserId,
                                UserName = profile.UserName,
                                Height = profile.Height,
                                Weight = profile.Weight,
                                Position = profile.Position,
                                Ranking = profile.Ranking,
                                StarRating = profile.StarRating,
                                Bio = profile.Bio,
                                ImageURL = profile.ImageURL,
                                PlayerArchetype = profile.PlayerArchetype,
                                City = profile.City,
                                Zip = profile.Zip,
                                PlayerNumber = profile.PlayerNumber,
                                Points = profile.Points,
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                Email = user.Email,
                                LastLoginDate = user.LastLoginDate,
                                Status = user.Status
                            })
                        .OrderBy(p => p.UserName);

                    // Add SQL Server index hints for large datasets
                    if (_context.Database.IsSqlServer())
                    {
                        profilesQuery = (IOrderedQueryable<Profile>)profilesQuery.TagWith("OPTION(OPTIMIZE FOR UNKNOWN)");
                    }

                    // Apply pagination with Skip/Take
                    var profiles = await profilesQuery
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync(cancellationToken);

                    // If no profiles found, return early
                    if (!profiles.Any())
                    {
                        return (new List<Profile>(), totalCount, totalPages);
                    }

                    // Get all profile IDs for batch operations
                    var profileIds = profiles.Select(p => p.ProfileId).ToList();

                    // Execute all necessary count queries in parallel
                    Task<Dictionary<string, int>> followerCountsTask = GetFollowerCountsAsync(profileIds, cancellationToken);
                    Task<Dictionary<string, int>> followingCountsTask = GetFollowingCountsAsync(profileIds, cancellationToken);
                    Task<Dictionary<string, string>> starRatingsTask = BatchGetAverageStarRatingsAsync(profileIds, cancellationToken);

                    // Wait for all parallel tasks to complete
                    await Task.WhenAll(followerCountsTask, followingCountsTask, starRatingsTask);

                    // Extract results
                    var followerCounts = await followerCountsTask;
                    var followingCounts = await followingCountsTask;
                    var starRatings = await starRatingsTask;

                    // Apply data to profiles efficiently
                    if (profiles.Count > 50)
                    {
                        Parallel.ForEach(profiles, PARALLEL_OPTIONS, profile =>
                        {
                            EnrichProfile(profile, followerCounts, followingCounts, starRatings);
                        });
                    }
                    else
                    {
                        foreach (var profile in profiles)
                        {
                            EnrichProfile(profile, followerCounts, followingCounts, starRatings);
                        }
                    }

                    return (profiles, totalCount, totalPages);
                },
                (new List<Profile>(), 0, 0),
                cancellationToken);
        }

        /// <summary>
        /// Efficient way to get follower counts for multiple profiles using compiled query
        /// </summary>
        private async Task<Dictionary<string, int>> GetFollowerCountsAsync(
            List<string> profileIds,
            CancellationToken cancellationToken = default)
        {
            if (profileIds == null || !profileIds.Any())
                return new Dictionary<string, int>();

            // Create a deterministic cache key
            string cacheKey = $"{FOLLOWERS_CACHE_KEY_PREFIX}{string.Join("_", profileIds.Take(3))}_{profileIds.Count}";

            return await GetOrCreateCacheAsync(
                cacheKey,
                async () =>
                {
                    // Execute the compiled query
                    var followerCounts = await GetFollowerCountsQuery(_context, profileIds);

                    // Add missing profiles with zero counts
                    foreach (var profileId in profileIds)
                    {
                        if (!followerCounts.ContainsKey(profileId))
                        {
                            followerCounts[profileId] = 0;
                        }
                    }

                    return followerCounts;
                },
                SOCIAL_CACHE_OPTIONS,
                cancellationToken);
        }

        /// <summary>
        /// Efficient way to get following counts for multiple profiles using compiled query
        /// </summary>
        private async Task<Dictionary<string, int>> GetFollowingCountsAsync(
            List<string> profileIds,
            CancellationToken cancellationToken = default)
        {
            if (profileIds == null || !profileIds.Any())
                return new Dictionary<string, int>();

            // Create a deterministic cache key
            string cacheKey = $"{FOLLOWING_CACHE_KEY_PREFIX}{string.Join("_", profileIds.Take(3))}_{profileIds.Count}";

            return await GetOrCreateCacheAsync(
                cacheKey,
                async () =>
                {
                    // Execute the compiled query
                    var followingCounts = await GetFollowingCountsQuery(_context, profileIds);

                    // Add missing profiles with zero counts
                    foreach (var profileId in profileIds)
                    {
                        if (!followingCounts.ContainsKey(profileId))
                        {
                            followingCounts[profileId] = 0;
                        }
                    }

                    return followingCounts;
                },
                SOCIAL_CACHE_OPTIONS,
                cancellationToken);
        }

        /// <summary>
        /// Helper method to enrich a profile with related data
        /// </summary>
        private void EnrichProfile(
            Profile profile,
            Dictionary<string, int> followerCounts,
            Dictionary<string, int> followingCounts,
            Dictionary<string, string> starRatings)
        {
            // Apply follower count
            profile.FollowersCount = followerCounts.TryGetValue(profile.ProfileId, out var followers)
                ? followers.ToString() : "0";

            // Apply following count
            profile.FollowingCount = followingCounts.TryGetValue(profile.ProfileId, out var following)
                ? following.ToString() : "0";

            // Apply star rating
            profile.StarRating = starRatings.GetValueOrDefault(profile.ProfileId, "0");

            // Apply ranking formatting if provided and valid
            if (!string.IsNullOrEmpty(profile.Ranking) && int.TryParse(profile.Ranking, out int ranking))
            {
                profile.Ranking = RankingSuffix.GetOrdinalSuffix(ranking);
            }
        }

        /// <summary>
        /// Get profile by ID with optimized query and caching
        /// </summary>
        public async Task<Profile> GetProfileById(string profileId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(profileId))
                return null;

            // Use cache with optimized access pattern
            string cacheKey = $"{PROFILE_CACHE_KEY_PREFIX}{profileId}";

            return await GetOrCreateCacheAsync(
                cacheKey,
                () => FetchProfileByIdAsync(profileId, cancellationToken),
                PROFILE_CACHE_OPTIONS,
                cancellationToken);
        }

        /// <summary>
        /// Internal method to fetch a profile by ID
        /// </summary>
        private async Task<Profile> FetchProfileByIdAsync(string profileId, CancellationToken cancellationToken)
        {
            return await ExecuteWithCircuitBreaker(
                async () =>
                {
                    // Use compiled query for better performance
                    var profile = await GetProfileByIdQuery(_context, profileId);

                    if (profile == null)
                        return null;

                    // Now fetch user information
                    var user = await _context.User
                        .AsNoTracking()
                        .Where(u => u.UserId == profile.UserId)
                        .Select(u => new
                        {
                            u.FirstName,
                            u.LastName,
                            u.Email,
                            u.SegId,
                            u.SubId,
                            u.Status,
                            u.LastLoginDate
                        })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (user != null)
                    {
                        profile.FirstName = user.FirstName;
                        profile.LastName = user.LastName;
                        profile.Email = user.Email;
                        profile.SegId = user.SegId;
                        profile.SubId = user.SubId;
                        profile.Status = user.Status;
                        profile.LastLoginDate = user.LastLoginDate;
                    }

                    // Run parallel tasks to fetch related data - only fetch what we need
                    var tasks = new List<Task>();

                    // Setting and scouting report tasks
                    var settingTask = _context.Setting
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.ProfileId == profile.ProfileId, cancellationToken);
                    tasks.Add(settingTask);

                    var scoutingReportTask = _context.ScoutingReport
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.ProfileId == profile.ProfileId, cancellationToken);
                    tasks.Add(scoutingReportTask);

                    // Social counts tasks - use the batch count methods for better performance
                    var singleProfileId = new List<string> { profile.ProfileId };
                    var followerCountsTask = GetFollowerCountsAsync(singleProfileId, cancellationToken);
                    tasks.Add(followerCountsTask);

                    var followingCountsTask = GetFollowingCountsAsync(singleProfileId, cancellationToken);
                    tasks.Add(followingCountsTask);

                    // Notifications task - limit to recent for performance
                    var notificationsTask = _context.Notification
                        .AsNoTracking()
                        .Where(n => n.ProfileId == profile.ProfileId)
                        .OrderByDescending(n => n.CreatedDate)
                        .Take(15) // Limit to most recent 15 notifications for performance
                        .ToListAsync(cancellationToken);
                    tasks.Add(notificationsTask);

                    // Game statistics task - potentially from cache
                    var gameStatsTask = GetGameStatisticsAsync(profile.ProfileId, cancellationToken);
                    tasks.Add(gameStatsTask);

                    // Execute all tasks in parallel
                    await Task.WhenAll(tasks);

                    // Assign results to the profile
                    profile.Setting = await settingTask;
                    profile.ScoutingReport = await scoutingReportTask;

                    // Get follower count from batch operation
                    var followerCounts = await followerCountsTask;
                    profile.FollowersCount = followerCounts.TryGetValue(profile.ProfileId, out int followers)
                        ? followers.ToString() : "0";

                    // Get following count from batch operation
                    var followingCounts = await followingCountsTask;
                    profile.FollowingCount = followingCounts.TryGetValue(profile.ProfileId, out int following)
                        ? following.ToString() : "0";

                    profile.NotificationList = await notificationsTask;

                    // Apply ranking formatting if valid
                    if (!string.IsNullOrEmpty(profile.Ranking) && int.TryParse(profile.Ranking, out int ranking))
                    {
                        profile.Ranking = RankingSuffix.GetOrdinalSuffix(ranking);
                    }

                    // Get game statistics
                    var stats = await gameStatsTask;
                    profile.TotalGames = stats.totalGames.ToString();
                    profile.WinPercentage = stats.winPercentage.ToString("F2");
                    profile.TotalWins = stats.totalWins;
                    profile.TotalLosses = stats.totalLosses;

                    return profile;
                },
                null,
                cancellationToken);
        }

        /// <summary>
        /// Get profile by ID with timeout
        /// </summary>
        public async Task<Profile> GetProfileByIdWithTimeout(
            string profileId,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            using var linkedSource = CreateTimeoutSource(timeout, cancellationToken);
            try
            {
                return await GetProfileById(profileId, linkedSource.Token);
            }
            catch (OperationCanceledException) when (linkedSource.Token.IsCancellationRequested
                                                    && !cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine($"Query timeout for profile {profileId}");
                return null;
            }
        }

        /// <summary>
        /// Get game statistics for a profile with caching and query optimization
        /// </summary>
        private async Task<(int totalGames, double winPercentage, int totalWins, int totalLosses)> GetGameStatisticsAsync(
            string profileId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(profileId))
                return (0, 0, 0, 0);

            // Check cache with optimized access pattern
            string cacheKey = $"{GAME_STATS_CACHE_KEY_PREFIX}{profileId}";

            return await GetOrCreateCacheAsync(
                cacheKey,
                () => FetchGameStatisticsAsync(profileId, cancellationToken),
                GAME_STATS_CACHE_OPTIONS,
                cancellationToken);
        }

        /// <summary>
        /// Internal method to fetch game statistics
        /// </summary>
        private async Task<(int totalGames, double winPercentage, int totalWins, int totalLosses)> FetchGameStatisticsAsync(
            string profileId,
            CancellationToken cancellationToken)
        {
            return await ExecuteWithCircuitBreaker(
                async () =>
                {
                    // Use database-specific optimizations
                    if (_context.Database.IsSqlServer())
                    {
                        // Parallel queries for wins and losses with SQL optimization hints
                        var winsQuery = _context.Game
                            .AsNoTracking()
                            .Where(g => !string.IsNullOrEmpty(g.WinProfileIdsStatusString) && g.WinProfileIdsStatusString.Contains(profileId))
                            .TagWith("OPTION(RECOMPILE, OPTIMIZE FOR UNKNOWN)");

                        var lossesQuery = _context.Game
                            .AsNoTracking()
                            .Where(g => !string.IsNullOrEmpty(g.LoseProfileIdsStatusString) && g.LoseProfileIdsStatusString.Contains(profileId))
                            .TagWith("OPTION(RECOMPILE, OPTIMIZE FOR UNKNOWN)");

                        // Execute both queries in parallel
                        var winsTask = winsQuery.CountAsync(cancellationToken);
                        var lossesTask = lossesQuery.CountAsync(cancellationToken);

                        await Task.WhenAll(winsTask, lossesTask);

                        int wins = await winsTask;
                        int losses = await lossesTask;
                        int totalGames = wins + losses;
                        double winPercentage = totalGames > 0 ? (double)wins / totalGames * 100 : 0;

                        return (totalGames, winPercentage, wins, losses);
                    }
                    else
                    {
                        // For other database providers - more generic approach
                        // Pre-filter games to reduce memory usage
                        var relevantGames = await _context.Game
                            .AsNoTracking()
                            .Where(g =>
                                (!string.IsNullOrEmpty(g.WinProfileIdsStatusString) && g.WinProfileIdsStatusString.Contains(profileId)) ||
                                (!string.IsNullOrEmpty(g.LoseProfileIdsStatusString) && g.LoseProfileIdsStatusString.Contains(profileId)))
                            .ToListAsync(cancellationToken);

                        int wins = relevantGames.Count(game =>
                            !string.IsNullOrEmpty(game.WinProfileIdsStatusString) &&
                            game.WinProfileIdsStatusString.Contains(profileId));

                        int losses = relevantGames.Count(game =>
                            !string.IsNullOrEmpty(game.LoseProfileIdsStatusString) &&
                            game.LoseProfileIdsStatusString.Contains(profileId));

                        int totalGames = wins + losses;
                        double winPercentage = totalGames > 0 ? (double)wins / totalGames * 100 : 0;

                        return (totalGames, winPercentage, wins, losses);
                    }
                },
                (0, 0, 0, 0),
                cancellationToken);
        }

        /// <summary>
        /// Update profile with optimized database access and cache invalidation
        /// </summary>
        public async Task UpdateProfile(Profile model, CancellationToken cancellationToken = default)
        {
            if (model == null || string.IsNullOrEmpty(model.ProfileId))
                throw new ArgumentNullException(nameof(model));

            try
            {
                // Track which properties were updated for selective cache invalidation
                bool significantChange = false;

                // Do a single database operation using SQL for efficiency if using SQL Server
                if (_context.Database.IsSqlServer())
                {
                    var updateSql = new System.Text.StringBuilder("UPDATE Profile SET ");
                    var parameters = new List<object>();
                    int paramIndex = 0;

                    if (model.Height != null)
                    {
                        updateSql.Append($"Height = {{{paramIndex++}}}, ");
                        parameters.Add(model.Height);
                        significantChange = true;
                    }

                    if (model.Weight != null)
                    {
                        updateSql.Append($"Weight = {{{paramIndex++}}}, ");
                        parameters.Add(model.Weight);
                        significantChange = true;
                    }

                    if (model.Position != null)
                    {
                        updateSql.Append($"Position = {{{paramIndex++}}}, ");
                        parameters.Add(model.Position);
                        significantChange = true;
                    }

                    if (model.Zip != null)
                    {
                        updateSql.Append($"Zip = {{{paramIndex++}}}, ");
                        parameters.Add(model.Zip);
                    }

                    if (model.QRCode != null)
                    {
                        updateSql.Append($"QRCode = {{{paramIndex++}}}, ");
                        parameters.Add(model.QRCode);
                    }

                    if (model.Bio != null)
                    {
                        updateSql.Append($"Bio = {{{paramIndex++}}}, ");
                        parameters.Add(model.Bio);
                        significantChange = true;
                    }

                    if (model.PlayerArchetype != null)
                    {
                        updateSql.Append($"PlayerArchetype = {{{paramIndex++}}}, ");
                        parameters.Add(model.PlayerArchetype);
                        significantChange = true;
                    }

                    if (model.City != null)
                    {
                        updateSql.Append($"City = {{{paramIndex++}}}, ");
                        parameters.Add(model.City);
                    }

                    // Remove trailing comma and space
                    if (parameters.Count > 0)
                    {
                        updateSql.Length -= 2;
                        updateSql.Append($" WHERE ProfileId = {{{paramIndex}}}");
                        parameters.Add(model.ProfileId);

                        await _context.Database.ExecuteSqlRawAsync(
                            updateSql.ToString(), parameters.ToArray());
                    }
                }
                else
                {
                    // Get existing profile first 
                    var existingItem = await _context.Profile
                        .FirstOrDefaultAsync(s => s.ProfileId == model.ProfileId, cancellationToken);

                    if (existingItem != null)
                    {
                        // Only update the properties that were provided
                        if (model.Height != null)
                        {
                            existingItem.Height = model.Height;
                            significantChange = true;
                        }

                        if (model.Weight != null)
                        {
                            existingItem.Weight = model.Weight;
                            significantChange = true;
                        }

                        if (model.Position != null)
                        {
                            existingItem.Position = model.Position;
                            significantChange = true;
                        }

                        if (model.Zip != null)
                        {
                            existingItem.Zip = model.Zip;
                        }

                        if (model.QRCode != null)
                        {
                            existingItem.QRCode = model.QRCode;
                        }

                        if (model.Bio != null)
                        {
                            existingItem.Bio = model.Bio;
                            significantChange = true;
                        }

                        if (model.PlayerArchetype != null)
                        {
                            existingItem.PlayerArchetype = model.PlayerArchetype;
                            significantChange = true;
                        }

                        if (model.City != null)
                        {
                            existingItem.City = model.City;
                        }

                        // Save changes
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }

                // Invalidate cache with appropriate strategy
                if (significantChange)
                {
                    // Significant change warrants more aggressive invalidation
                    InvalidateProfileCaches(model.ProfileId);
                }
                else
                {
                    // Minor change - just invalidate the profile cache
                    string profileCacheKey = $"{PROFILE_CACHE_KEY_PREFIX}{model.ProfileId}";
                    _cache?.Remove(profileCacheKey);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile: {ex.Message}, ProfileId: {model.ProfileId}");
                throw; // Rethrow to allow calling code to handle the exception
            }
        }

        /// <summary>
        /// Batch update multiple profiles at once
        /// </summary>
        public async Task BatchUpdateProfiles(
            List<Profile> profiles,
            CancellationToken cancellationToken = default)
        {
            if (profiles == null || !profiles.Any())
                return;

            try
            {
                // Group updates by profile ID to avoid duplicates
                var profileUpdates = profiles
                    .GroupBy(p => p.ProfileId)
                    .Select(g => g.First())
                    .ToList();

                // Use bulk operations for SQL Server
                if (_context.Database.IsSqlServer() && profileUpdates.Count > 10)
                {
                    // Build batch SQL update
                    foreach (var batch in profileUpdates.Chunk(50))
                    {
                        // Create SQL for this batch
                        var parameters = new Dictionary<string, object>();
                        var sqlBuilder = new System.Text.StringBuilder();

                        for (int i = 0; i < batch.Length; i++)
                        {
                            var profile = batch[i];
                            sqlBuilder.Append($"UPDATE Profile SET ");

                            bool hasUpdates = false;
                            if (profile.Bio != null)
                            {
                                sqlBuilder.Append($"Bio = @Bio{i}, ");
                                parameters[$"@Bio{i}"] = profile.Bio;
                                hasUpdates = true;
                            }

                            if (profile.Height != null)
                            {
                                sqlBuilder.Append($"Height = @Height{i}, ");
                                parameters[$"@Height{i}"] = profile.Height;
                                hasUpdates = true;
                            }

                            if (profile.Weight != null)
                            {
                                sqlBuilder.Append($"Weight = @Weight{i}, ");
                                parameters[$"@Weight{i}"] = profile.Weight;
                                hasUpdates = true;
                            }

                            // Remove trailing comma if we added any updates
                            if (hasUpdates)
                            {
                                sqlBuilder.Length -= 2; // Remove last comma and space
                                sqlBuilder.Append($" WHERE ProfileId = @ProfileId{i};");
                                parameters[$"@ProfileId{i}"] = profile.ProfileId;
                            }
                        }

                        if (sqlBuilder.Length > 0)
                        {
                            // Execute the batch update
                            var sql = sqlBuilder.ToString();
                            await _context.Database.ExecuteSqlRawAsync(sql, parameters.Values.ToArray());
                        }
                    }
                }
                else
                {
                    // For non-SQL Server or smaller batches, use standard EF approach with batching
                    foreach (var batch in profileUpdates.Chunk(50))
                    {
                        foreach (var profile in batch)
                        {
                            await UpdateProfile(profile, cancellationToken);
                        }
                    }
                }

                // Invalidate cache for each profile
                foreach (var profile in profileUpdates)
                {
                    InvalidateProfileCaches(profile.ProfileId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in batch update profiles: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get followers efficiently with batch loading and caching
        /// </summary>
        public async Task<List<Profile>> GetFollowerProfilesByProfileId(
            string profileId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(profileId))
                return new List<Profile>();

            // Use optimized cache access
            string cacheKey = $"{FOLLOWERS_CACHE_KEY_PREFIX}{profileId}";

            return await GetOrCreateCacheAsync(
                cacheKey,
                () => FetchFollowerProfilesAsync(profileId, cancellationToken),
                SOCIAL_CACHE_OPTIONS,
                cancellationToken);
        }

        /// <summary>
        /// Internal method to fetch follower profiles
        /// </summary>
        private async Task<List<Profile>> FetchFollowerProfilesAsync(
            string profileId,
            CancellationToken cancellationToken)
        {
            return await ExecuteWithCircuitBreaker(
                async () =>
                {
                    // Get follower profile IDs in one optimized query
                    var followerProfileIds = await _context.Follower
                        .AsNoTracking()
                        .Where(f => f.ProfileId == profileId)
                        .Select(f => f.FollowerProfileId)
                        .ToListAsync(cancellationToken);

                    if (!followerProfileIds.Any())
                        return new List<Profile>();

                    // Fetch all profiles in a single efficient query with only needed fields
                    var profiles = await _context.Profile
                        .AsNoTracking()
                        .Where(p => followerProfileIds.Contains(p.ProfileId))
                        .Join(_context.User,
                            p => p.UserId,
                            u => u.UserId,
                            (p, u) => new Profile
                            {
                                ProfileId = p.ProfileId,
                                UserId = p.UserId,
                                UserName = p.UserName,
                                Height = p.Height,
                                Weight = p.Weight,
                                Position = p.Position,
                                Ranking = p.Ranking,
                                StarRating = p.StarRating,
                                Bio = p.Bio,
                                ImageURL = p.ImageURL,
                                PlayerArchetype = p.PlayerArchetype,
                                City = p.City,
                                Zip = p.Zip,
                                PlayerNumber = p.PlayerNumber,
                                Points = p.Points,
                                FirstName = u.FirstName,
                                LastName = u.LastName,
                                Status = u.Status
                            })
                        .ToListAsync(cancellationToken);

                    // Get star ratings in batch
                    var starRatings = await BatchGetAverageStarRatingsAsync(
                        profiles.Select(p => p.ProfileId).ToList(), cancellationToken);

                    // Apply ratings and flags
                    foreach (var profile in profiles)
                    {
                        profile.StarRating = starRatings.GetValueOrDefault(profile.ProfileId, "0");
                        profile.Followed = false;

                        // Apply ranking formatting if valid
                        if (!string.IsNullOrEmpty(profile.Ranking) && int.TryParse(profile.Ranking, out int ranking))
                        {
                            profile.Ranking = RankingSuffix.GetOrdinalSuffix(ranking);
                        }
                    }

                    return profiles;
                },
                new List<Profile>(),
                cancellationToken);
        }

        /// <summary>
        /// Get following profiles efficiently with batch loading and caching
        /// </summary>
        public async Task<List<Profile>> GetFollowingProfilesByProfileId(
            string profileId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(profileId))
                return new List<Profile>();

            // Use optimized cache access
            string cacheKey = $"{FOLLOWING_CACHE_KEY_PREFIX}{profileId}";

            return await GetOrCreateCacheAsync(
                cacheKey,
                () => FetchFollowingProfilesAsync(profileId, cancellationToken),
                SOCIAL_CACHE_OPTIONS,
                cancellationToken);
        }

        /// <summary>
        /// Internal method to fetch following profiles
        /// </summary>
        private async Task<List<Profile>> FetchFollowingProfilesAsync(
            string profileId,
            CancellationToken cancellationToken)
        {
            return await ExecuteWithCircuitBreaker(
                async () =>
                {
                    // Get following profile IDs in one efficient query
                    var followingProfileIds = await _context.Following
                        .AsNoTracking()
                        .Where(f => f.ProfileId == profileId)
                        .Select(f => f.FollowingProfileId)
                        .ToListAsync(cancellationToken);

                    if (!followingProfileIds.Any())
                        return new List<Profile>();

                    // Fetch all profiles in a single query with only needed fields
                    var profiles = await _context.Profile
                        .AsNoTracking()
                        .Where(p => followingProfileIds.Contains(p.ProfileId))
                        .Join(_context.User,
                            p => p.UserId,
                            u => u.UserId,
                            (p, u) => new Profile
                            {
                                ProfileId = p.ProfileId,
                                UserId = p.UserId,
                                UserName = p.UserName,
                                Height = p.Height,
                                Weight = p.Weight,
                                Position = p.Position,
                                Ranking = p.Ranking,
                                StarRating = p.StarRating,
                                Bio = p.Bio,
                                ImageURL = p.ImageURL,
                                PlayerArchetype = p.PlayerArchetype,
                                City = p.City,
                                Zip = p.Zip,
                                PlayerNumber = p.PlayerNumber,
                                Points = p.Points,
                                FirstName = u.FirstName,
                                LastName = u.LastName,
                                Status = u.Status
                            })
                        .ToListAsync(cancellationToken);

                    // Get star ratings in batch
                    var starRatings = await BatchGetAverageStarRatingsAsync(
                        profiles.Select(p => p.ProfileId).ToList(), cancellationToken);

                    // Apply ratings and flags
                    foreach (var profile in profiles)
                    {
                        profile.StarRating = starRatings.GetValueOrDefault(profile.ProfileId, "0");
                        profile.Followed = true; // These are profiles the user is following

                        // Apply ranking formatting
                        if (!string.IsNullOrEmpty(profile.Ranking) && int.TryParse(profile.Ranking, out int ranking))
                        {
                            profile.Ranking = RankingSuffix.GetOrdinalSuffix(ranking);
                        }
                    }

                    return profiles;
                },
                new List<Profile>(),
                cancellationToken);
        }

        /// <summary>
        /// Get star ratings for multiple profiles in one batch operation with caching
        /// </summary>
        private async Task<Dictionary<string, string>> BatchGetAverageStarRatingsAsync(
            List<string> profileIds,
            CancellationToken cancellationToken = default)
        {
            if (profileIds == null || !profileIds.Any())
                return new Dictionary<string, string>();

            // Create a deterministic multi-profile cache key
            string cacheKey = $"{STAR_RATINGS_CACHE_KEY_PREFIX}{string.Join("_", profileIds.Take(3))}_{profileIds.Count}";

            return await GetOrCreateCacheAsync(
                cacheKey,
                () => FetchStarRatingsAsync(profileIds, cancellationToken),
                STAR_RATINGS_CACHE_OPTIONS,
                cancellationToken);
        }

        /// <summary>
        /// Internal method to fetch star ratings
        /// </summary>
        private async Task<Dictionary<string, string>> FetchStarRatingsAsync(
            List<string> profileIds,
            CancellationToken cancellationToken)
        {
            return await ExecuteWithCircuitBreaker(
                async () =>
                {
                    // Optimize query to fetch all ratings at once
                    var ratings = await _context.Rating
                        .AsNoTracking()
                        .Where(r => profileIds.Contains(r.ProfileId))
                        .Select(r => new { r.ProfileId, r.StarRating })
                        .ToListAsync(cancellationToken);

                    // Process ratings efficiently
                    var result = new Dictionary<string, string>();
                    var groupedRatings = ratings
                        .Where(r => !string.IsNullOrEmpty(r.StarRating))
                        .GroupBy(r => r.ProfileId);

                    foreach (var group in groupedRatings)
                    {
                        var validRatings = group
                            .Select(r => int.TryParse(r.StarRating, out int val) ? val : 0)
                            .Where(v => v > 0)
                            .ToList();

                        result[group.Key] = validRatings.Any()
                            ? validRatings.Average().ToString("F1")
                            : "0.0";
                    }

                    // Add missing profiles with zero rating
                    foreach (var profileId in profileIds)
                    {
                        if (!result.ContainsKey(profileId))
                        {
                            result[profileId] = "0.0";
                        }
                    }

                    return result;
                },
                profileIds.ToDictionary(id => id, id => "0"),
                cancellationToken);
        }

        /// <summary>
        /// Get all profiles efficiently with streaming capability for large datasets
        /// </summary>
        public async Task<List<Profile>> GetProfiles(CancellationToken cancellationToken = default)
        {
            // Try to get from cache for better performance
            if (_cache != null && _cache.TryGetValue(ALL_PROFILES_CACHE_KEY, out List<Profile> cachedProfiles))
            {
                return cachedProfiles;
            }

            try
            {
                // Reuse the paginated method with a larger page size
                var (profiles, _, _) = await GetProfilesPaginated(1, 1000, cancellationToken);

                // Cache the result with appropriate duration
                _cache?.Set(ALL_PROFILES_CACHE_KEY, profiles, PROFILE_CACHE_OPTIONS);

                return profiles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProfiles: {ex.Message}");
                return new List<Profile>();
            }
        }

        

        /// <summary>
        /// Get game history for a profile with optimized queries and caching
        /// </summary>
        public async Task<List<Game>> GetProfileGameHistory(
            string profileId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(profileId))
                return new List<Game>();

            // Use optimized cache access
            string cacheKey = $"ProfileGameHistory_{profileId}";

            return await GetOrCreateCacheAsync(
                cacheKey,
                () => FetchProfileGameHistoryAsync(profileId, cancellationToken),
                GAME_STATS_CACHE_OPTIONS,
                cancellationToken);
        }

        /// <summary>
        /// Internal method to fetch profile game history
        /// </summary>
        private async Task<List<Game>> FetchProfileGameHistoryAsync(
            string profileId,
            CancellationToken cancellationToken)
        {
            return await ExecuteWithCircuitBreaker(
                async () =>
                {
                    // Optimized query to find relevant games in a single operation
                    var games = await _context.Game
                        .AsNoTracking()
                        .Where(g =>
                            (!string.IsNullOrEmpty(g.WinProfileIdsStatusString) && g.WinProfileIdsStatusString.Contains(profileId)) ||
                            (!string.IsNullOrEmpty(g.LoseProfileIdsStatusString) && g.LoseProfileIdsStatusString.Contains(profileId)))
                        .OrderByDescending(g => g.CreatedDate) // Most recent first
                        .Take(20) // Limit to most recent 20 games for performance
                        .ToListAsync(cancellationToken);

                    if (!games.Any())
                        return new List<Game>();

                    // Extract all profile IDs that need to be loaded
                    var allProfileIds = new HashSet<string>();
                    foreach (var game in games)
                    {
                        // Extract profile IDs from game data
                        ExtractProfileIds(game.WinProfileIdsStatusString, allProfileIds);
                        ExtractProfileIds(game.LoseProfileIdsStatusString, allProfileIds);
                    }

                    // Fetch all needed profiles in a single query
                    var profiles = await _context.Profile
                        .AsNoTracking()
                        .Where(p => allProfileIds.Contains(p.ProfileId))
                        .Select(p => new Profile
                        {
                            ProfileId = p.ProfileId,
                            UserId = p.UserId,
                            UserName = p.UserName,
                            ImageURL = p.ImageURL,
                            Height = p.Height,
                            Weight = p.Weight,
                            Position = p.Position,
                            Ranking = p.Ranking,
                            StarRating = p.StarRating,
                            QRCode = p.QRCode,
                            Bio = p.Bio,
                            PlayerArchetype = p.PlayerArchetype,
                            City = p.City,
                            PlayerNumber = p.PlayerNumber,
                        })
                        .ToListAsync(cancellationToken);

                    // Create a dictionary for quick lookups
                    var profilesDict = profiles.ToDictionary(p => p.ProfileId);

                    // Process each game using in-memory operations - use parallel for efficiency
                    if (games.Count > 5)
                    {
                        Parallel.ForEach(games, PARALLEL_OPTIONS, game =>
                        {
                            ProcessGameProfiles(game, profilesDict, profileId);
                        });
                    }
                    else
                    {
                        foreach (var game in games)
                        {
                            ProcessGameProfiles(game, profilesDict, profileId);
                        }
                    }

                    return games;
                },
                new List<Game>(),
                cancellationToken);
        }

        /// <summary>
        /// Process game profiles for better code organization
        /// </summary>
        private void ProcessGameProfiles(Game game, Dictionary<string, Profile> profilesDict, string currentProfileId)
        {
            // Parse winners and losers
            game.WinnersList = ExtractProfiles(game.WinProfileIdsStatusString, profilesDict, "W");
            game.LossersList = ExtractProfiles(game.LoseProfileIdsStatusString, profilesDict, "L");

            // Combine lists
            game.ProfileList = new List<Profile>();
            game.ProfileList.AddRange(game.WinnersList);
            game.ProfileList.AddRange(game.LossersList);

            // Set user win/lose status
            if (game.WinProfileIdsStatusString?.Contains(currentProfileId) == true)
            {
                game.UserWinOrLose = "W";
            }
            else if (game.LoseProfileIdsStatusString?.Contains(currentProfileId) == true)
            {
                game.UserWinOrLose = "L";
            }
        }

        /// <summary>
        /// Helper method to extract profile IDs from a JSON-like string
        /// </summary>
        private void ExtractProfileIds(string profileIdsString, HashSet<string> profileIds)
        {
            if (string.IsNullOrEmpty(profileIdsString))
                return;

            try
            {
                // Use Json.NET for efficient deserialization
                var profileStatuses = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProfileStatus>>(profileIdsString);
                if (profileStatuses != null)
                {
                    foreach (var status in profileStatuses)
                    {
                        profileIds.Add(status.ProfileId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting profile IDs: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper method to extract profiles from a JSON-like string
        /// </summary>
        private List<Profile> ExtractProfiles(string profileIdsString, Dictionary<string, Profile> profilesDict, string winOrLose)
        {
            var result = new List<Profile>();

            if (string.IsNullOrEmpty(profileIdsString))
                return result;

            try
            {
                // Deserialize the string
                var profileStatuses = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProfileStatus>>(profileIdsString);
                if (profileStatuses != null)
                {
                    // For each profile status, find the corresponding profile and add to result
                    foreach (var status in profileStatuses)
                    {
                        if (profilesDict.TryGetValue(status.ProfileId, out var profile))
                        {
                            // Create a clone with minimal properties to avoid modifying cached profiles
                            var clonedProfile = new Profile
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
                                ImageURL = profile.ImageURL,
                                PlayerArchetype = profile.PlayerArchetype,
                                City = profile.City,
                                PlayerNumber = profile.PlayerNumber,
                                WinOrLose = winOrLose
                            };

                            result.Add(clonedProfile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting profiles: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Update profile's last run date with cache invalidation
        /// </summary>
        public async Task UpdateLastRunDate(string profileId, string lastRunDate, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(profileId))
                return;

            try
            {
                // Use direct update for better performance
                if (_context.Database.IsSqlServer())
                {
                    // Execute SQL to update directly without loading the entity
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE Profile SET LastRunDate = {0} WHERE ProfileId = {1}",
                        lastRunDate, profileId);
                }
                else
                {
                    // For other databases, use standard EF Core approach
                    var profile = await _context.Profile
                        .FirstOrDefaultAsync(p => p.ProfileId == profileId, cancellationToken);

                    if (profile != null)
                    {
                        profile.LastRunDate = lastRunDate;
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }

                // Invalidate profile cache
                string cacheKey = $"{PROFILE_CACHE_KEY_PREFIX}{profileId}";
                _cache?.Remove(cacheKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating last run date: {ex.Message}, ProfileId: {profileId}");
                throw;
            }
        }

        /// <summary>
        /// Update winner points with optimized database access
        /// </summary>
        public async Task UpdateWinnerPoints(string profileId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(profileId))
                return;

            try
            {
                // Use direct SQL for better performance with SQL Server
                if (_context.Database.IsSqlServer())
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE Profile SET Points = ISNULL(Points, 0) + 2 WHERE ProfileId = {0}",
                        profileId);
                }
                else
                {
                    // For other databases, use standard EF Core approach
                    var profile = await _context.Profile
                        .FirstOrDefaultAsync(p => p.ProfileId == profileId, cancellationToken);

                    if (profile != null)
                    {
                        profile.Points = (profile.Points ?? 0) + 2;
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }

                // Invalidate profile and related caches
                InvalidateProfileCaches(profileId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating winner points: {ex.Message}, ProfileId: {profileId}");
                throw;
            }
        }

        /// <summary>
        /// Update top record flag with optimized database access
        /// </summary>
        public async Task UpdateSetProfileWithBestRecord(string profileId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(profileId))
                return;

            try
            {
                // Use direct SQL for better performance with SQL Server
                if (_context.Database.IsSqlServer())
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE Profile SET TopRecord = 1 WHERE ProfileId = {0}",
                        profileId);
                }
                else
                {
                    // For other databases, use standard EF Core approach
                    var profile = await _context.Profile
                        .FirstOrDefaultAsync(p => p.ProfileId == profileId, cancellationToken);

                    if (profile != null)
                    {
                        profile.TopRecord = true;
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }

                // Invalidate profile and related caches
                InvalidateProfileCaches(profileId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating top record flag: {ex.Message}, ProfileId: {profileId}");
                throw;
            }
        }

        /// <summary>
        /// Update top record flag to false with optimized database access
        /// </summary>
        public async Task UpdateSetProfileWithBestRecordToFalse(string profileId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(profileId))
                return;

            try
            {
                // Use direct SQL for better performance with SQL Server
                if (_context.Database.IsSqlServer())
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE Profile SET TopRecord = 0 WHERE ProfileId = {0}",
                        profileId);
                }
                else
                {
                    // For other databases, use standard EF Core approach
                    var profile = await _context.Profile
                        .FirstOrDefaultAsync(p => p.ProfileId == profileId, cancellationToken);

                    if (profile != null)
                    {
                        profile.TopRecord = false;
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }

                // Invalidate profile and related caches
                InvalidateProfileCaches(profileId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating top record flag: {ex.Message}, ProfileId: {profileId}");
                throw;
            }
        }

        /// <summary>
        /// Update profile username with validation and cache invalidation
        /// </summary>
        public async Task UpdateProfileUserName(Profile model, CancellationToken cancellationToken = default)
        {
            if (model == null || string.IsNullOrEmpty(model.ProfileId) || string.IsNullOrEmpty(model.UserName))
                throw new ArgumentException("Profile ID and username are required");

            try
            {
                // First check if the username is already taken by another profile
                bool isUserNameTaken = await _context.Profile
                    .AsNoTracking()
                    .AnyAsync(p => p.UserName == model.UserName && p.ProfileId != model.ProfileId, cancellationToken);

                if (isUserNameTaken)
                {
                    throw new InvalidOperationException("Username is already taken");
                }

                // Update the username using the most efficient method
                if (_context.Database.IsSqlServer())
                {
                    int rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE Profile SET UserName = {0} WHERE ProfileId = {1}",
                        model.UserName, model.ProfileId);

                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException("Profile not found");
                    }
                }
                else
                {
                    var profile = await _context.Profile
                        .FirstOrDefaultAsync(p => p.ProfileId == model.ProfileId, cancellationToken);

                    if (profile != null)
                    {
                        profile.UserName = model.UserName;
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        throw new InvalidOperationException("Profile not found");
                    }
                }

                // Invalidate all relevant caches
                InvalidateAllProfileCaches(model.ProfileId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile username: {ex.Message}, ProfileId: {model.ProfileId}");
                throw;
            }
        }

        /// <summary>
        /// Update profile settings with optimized database access
        /// </summary>
        public async Task UpdateSetting(Setting model, CancellationToken cancellationToken = default)
        {
            if (model == null || string.IsNullOrEmpty(model.ProfileId))
                throw new ArgumentException("Profile ID is required");

            try
            {
                // Check if setting exists first
                var setting = await _context.Setting
                    .FirstOrDefaultAsync(s => s.ProfileId == model.ProfileId, cancellationToken);

                if (setting != null)
                {
                    // Update existing setting
                    setting.AllowComments = model.AllowComments;
                    setting.ShowGameHistory = model.ShowGameHistory;
                    setting.AllowEmailNotification = model.AllowEmailNotification;

                    await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    // Create new setting
                    model.SettingId = Guid.NewGuid().ToString();
                    await _context.Setting.AddAsync(model, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                // Invalidate profile cache
                string cacheKey = $"{PROFILE_CACHE_KEY_PREFIX}{model.ProfileId}";
                _cache?.Remove(cacheKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile settings: {ex.Message}, ProfileId: {model.ProfileId}");
                throw;
            }
        }

        /// <summary>
        /// Check if email is available
        /// </summary>
        public async Task<bool> IsEmailAvailable(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            try
            {
                // Optimize query - use AsNoTracking and Any for best performance
                return await _context.User
                    .AsNoTracking()
                    .AnyAsync(u => u.Email == email, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking email availability: {ex.Message}, Email: {email}");
                return false;
            }
        }

        /// <summary>
        /// Check if username is available
        /// </summary>
        public async Task<bool> IsUserNameAvailable(string userName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userName))
                return false;

            try
            {
                // Optimize query - use AsNoTracking and Any for best performance
                return await _context.Profile
                    .AsNoTracking()
                    .AnyAsync(p => p.UserName == userName, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking username availability: {ex.Message}, UserName: {userName}");
                return false;
            }
        }

        /// <summary>
        /// Save changes to database
        /// </summary>
        public async Task<int> Save(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Helper method to invalidate profile-related caches
        /// </summary>
        private void InvalidateProfileCaches(string profileId)
        {
            if (_cache == null) return;

            // Invalidate profile cache
            string profileCacheKey = $"{PROFILE_CACHE_KEY_PREFIX}{profileId}";
            _cache.Remove(profileCacheKey);

            // Invalidate game statistics cache
            string gameStatsCacheKey = $"{GAME_STATS_CACHE_KEY_PREFIX}{profileId}";
            _cache.Remove(gameStatsCacheKey);

            // Invalidate game history cache
            string gameHistoryCacheKey = $"ProfileGameHistory_{profileId}";
            _cache.Remove(gameHistoryCacheKey);
        }

        /// <summary>
        /// Helper method to invalidate all caches related to a profile
        /// </summary>
        private void InvalidateAllProfileCaches(string profileId)
        {
            if (_cache == null) return;

            // Invalidate direct profile cache
            InvalidateProfileCaches(profileId);

            // Invalidate all profiles cache
            _cache.Remove(ALL_PROFILES_CACHE_KEY);

            // Invalidate followers/following caches
            _cache.Remove($"{FOLLOWERS_CACHE_KEY_PREFIX}{profileId}");
            _cache.Remove($"{FOLLOWING_CACHE_KEY_PREFIX}{profileId}");

            // Invalidate paginated caches that might include this profile
            var cacheKeys = new List<string>();

            // Find cache entries that might contain this profile
            var entries = _cache.GetType().GetField("_entries",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_cache);

            if (entries is IDictionary<object, object> entriesDict)
            {
                foreach (var key in entriesDict.Keys)
                {
                    var keyString = key.ToString();
                    if (keyString != null &&
                        (keyString.StartsWith("Profiles_Page") ||
                         keyString.StartsWith(STAR_RATINGS_CACHE_KEY_PREFIX)))
                    {
                        cacheKeys.Add(keyString);
                    }
                }

                // Remove all affected cache entries
                foreach (var key in cacheKeys)
                {
                    _cache.Remove(key);
                }
            }
            else
            {
                // Fallback to removing common page caches if reflection approach fails
                for (int i = 1; i <= 5; i++) // First 5 pages are most likely to be affected
                {
                    _cache.Remove($"Profiles_Page{i}_Size20");
                }
            }
        }

        /// <summary>
        /// Stream all profiles for more efficient memory usage with large datasets
        /// </summary>
        public async IAsyncEnumerable<Profile> StreamAllProfiles(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Define batch size for efficient processing
            const int batchSize = 100;
            int currentPage = 0;
            bool hasMore = true;

            while (hasMore && !cancellationToken.IsCancellationRequested)
            {
                // Get one page at a time
                // Make sure to use OrderBy to return IOrderedQueryable<Profile>
                var profiles = await _context.Profile
                    .AsNoTracking()
                    .OrderBy(p => p.ProfileId) // This ensures we get an IOrderedQueryable<Profile>
                    .Skip(currentPage * batchSize)
                    .Take(batchSize)
                    .ToListAsync(cancellationToken);

                // Check if we've reached the end
                if (profiles.Count == 0)
                {
                    hasMore = false;
                    continue;
                }

                // Return profiles one by one
                foreach (var profile in profiles)
                {
                    yield return profile;
                }

                // Move to next page
                currentPage++;
            }
        }

        /// <summary>
        /// Properly implemented Dispose pattern
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _cacheSemaphore?.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Implementation of IProfileRepository interface without CancellationToken
        /// </summary>
        public Task<List<Profile>> GetProfiles()
        {
            return GetProfiles(CancellationToken.None);
        }

        public Task<List<Profile>> GetFollowingProfilesByProfileId(string profileId)
        {
            return GetFollowingProfilesByProfileId(profileId, CancellationToken.None);
        }

        public Task<List<Profile>> GetFollowerProfilesByProfileId(string profileId)
        {
            return GetFollowerProfilesByProfileId(profileId, CancellationToken.None);
        }

        public Task<Profile> GetProfileById(string profileId)
        {
            return GetProfileById(profileId, CancellationToken.None);
        }

        public Task<List<Game>> GetProfileGameHistory(string profileId)
        {
            return GetProfileGameHistory(profileId, CancellationToken.None);
        }

        public Task UpdateProfile(Profile model)
        {
            return UpdateProfile(model, CancellationToken.None);
        }

        public Task UpdateWinnerPoints(string profileId)
        {
            return UpdateWinnerPoints(profileId, CancellationToken.None);
        }

        public Task UpdateSetProfileWithBestRecord(string profileId)
        {
            return UpdateSetProfileWithBestRecord(profileId, CancellationToken.None);
        }

        public Task UpdateSetProfileWithBestRecordToFalse(string profileId)
        {
            return UpdateSetProfileWithBestRecordToFalse(profileId, CancellationToken.None);
        }

        public Task UpdateLastRunDate(string profileId, string lastRunDate)
        {
            return UpdateLastRunDate(profileId, lastRunDate, CancellationToken.None);
        }

        public Task UpdateProfileUserName(Profile model)
        {
            return UpdateProfileUserName(model, CancellationToken.None);
        }

        public Task UpdateSetting(Setting model)
        {
            return UpdateSetting(model, CancellationToken.None);
        }

        public Task<bool> IsUserNameAvailable(string userName)
        {
            return IsUserNameAvailable(userName, CancellationToken.None);
        }

        public Task<bool> IsEmailAvailable(string email)
        {
            return IsEmailAvailable(email, CancellationToken.None);
        }

        public Task<int> Save()
        {
            return Save(CancellationToken.None);
        }
    }

}
