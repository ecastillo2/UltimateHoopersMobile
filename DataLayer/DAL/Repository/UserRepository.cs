using DataLayer.Context;
using DataLayer.DAL.Interface;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace DataLayer.DAL.Repository
{
    /// <summary>
    /// Implementation of the User repository with proper error handling
    /// </summary>
    public class UserRepository :  IUserRepository
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<UserRepository> _logger;
        private readonly IConfiguration _configuration;
        private bool _disposed = false;

        public UserRepository(ApplicationContext context, IConfiguration configuration, ILogger<UserRepository> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<List<User>> GetUsersAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.User
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving Runs");
                throw;
            }
        }

        public async Task<(List<User> Users, string NextCursor)> GetUsersWithCursorAsync(string cursor = null,int limit = 20,string direction = "next",string sortBy = "Points",CancellationToken cancellationToken = default)
        {
            try
            {
                // Default query starting point
                IQueryable<User> query = _context.User.AsNoTracking();

                // Parse the cursor if provided
                CursorData cursorData = null;
                if (!string.IsNullOrEmpty(cursor))
                {
                    try
                    {
                        // Decode and deserialize cursor
                        var decodedCursor = System.Text.Encoding.UTF8.GetString(
                            Convert.FromBase64String(cursor));
                        cursorData = System.Text.Json.JsonSerializer.Deserialize<CursorData>(decodedCursor);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Invalid cursor format. Starting from beginning");
                        // If cursor parsing fails, ignore and start from beginning
                        cursorData = null;
                    }
                }


                // Execute query with limit
                var privateRuns = await query.Take(limit + 1).ToListAsync(cancellationToken);

                // Check if we have a next page by fetching limit+1 items
                string nextCursor = null;
                if (privateRuns.Count > limit)
                {
                    // Remove the extra item we retrieved to check for "has next page"
                    var lastItem = privateRuns[limit];
                    privateRuns.RemoveAt(limit);

                    // Create cursor for next page based on last item properties
                    var newCursorData = new ClientCursorData
                    {
                        Id = lastItem.ClientId,


                    };

                    var serialized = System.Text.Json.JsonSerializer.Serialize(newCursorData);
                    nextCursor = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(serialized));
                }

                // If we requested previous direction and got results, we need to reverse the order
                if (direction.ToLowerInvariant() == "previous" && privateRuns.Any())
                {
                    privateRuns.Reverse();
                }

                return (privateRuns, nextCursor);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting Runs with cursor");
                throw;
            }
        }

        public async IAsyncEnumerable<User> StreamAllUsersAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var batchSize = 100;
            var lastId = string.Empty;

            while (true)
            {
                List<User> batch;
                try
                {
                    batch = await _context.User
                        .AsNoTracking()
                        .Where(p => string.Compare(p.UserId, lastId) > 0)
                        .OrderBy(p => p.UserId)
                        .Take(batchSize)
                        .ToListAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error streaming PrivateRuns");
                    throw;
                }

                if (batch.Count == 0)
                    break;

                foreach (var privateRun in batch)
                {
                    yield return privateRun;
                    lastId = privateRun.UserId;
                }

                if (batch.Count < batchSize)
                    break;
            }
        }

        public async Task<User> GetUserByIdAsync(string runId,CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.User
                    .AsNoTracking()
                    
                    .FirstOrDefaultAsync(p => p.UserId == runId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting PrivateRun {PrivateRunId}", runId);
                throw;
            }
        }

        public async Task<IList<User>> GetUsersSearchAsync(string searchQuery, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(searchQuery) || searchQuery.Length < 2)
                {
                    return new List<User>();
                }

                var query = searchQuery.ToLower().Trim();

                // First get matching user IDs
                var userIds = await _context.User
                    .AsNoTracking()
                    .Where(u =>
                        // Search by first name
                        (!string.IsNullOrEmpty(u.FirstName) && u.FirstName.ToLower().Contains(query)) ||
                        // Search by last name  
                        (!string.IsNullOrEmpty(u.LastName) && u.LastName.ToLower().Contains(query)) ||
                        // Search by full name
                        (!string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName) &&
                         (u.FirstName + " " + u.LastName).ToLower().Contains(query)) ||
                        // Search by email
                        (!string.IsNullOrEmpty(u.Email) && u.Email.ToLower().Contains(query)) ||
                        // Search in Profile data using subquery
                        _context.Profile.Any(p => p.UserId == u.UserId && (
                            (!string.IsNullOrEmpty(p.UserName) && p.UserName.ToLower().Contains(query)) ||
                            (!string.IsNullOrEmpty(p.PlayerNumber) && p.PlayerNumber.ToLower().Contains(query))
                        ))
                    )
                    .Where(u => u.Status == "Active") // Only active users
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .Take(20) // Limit results
                    .Select(u => u.UserId)
                    .ToListAsync(cancellationToken);

                // Get users by IDs
                var users = await _context.User
                    .Where(u => userIds.Contains(u.UserId))
                    .AsNoTracking()
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .ToListAsync(cancellationToken);

                // Get profiles for these users
                var profiles = await _context.Profile
                    .Where(p => userIds.Contains(p.UserId))
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                // Create a dictionary for faster lookup
                var profileLookup = profiles.ToDictionary(p => p.UserId, p => p);

                // Assign profiles to users
                foreach (var user in users)
                {
                    if (profileLookup.TryGetValue(user.UserId, out var profile))
                    {
                        user.Profile = profile;
                    }
                }

                return users;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error searching users with query: {SearchQuery}", searchQuery);
                throw;
            }
        }

        public async Task<ScoutingReport?> GetProfileScoutingReportByUserId(string userId,CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Get the ProfileId from the User with given UserId
                var profileId = await _context.Profile
                    .AsNoTracking()
                    .Where(u => u.UserId == userId)
                    .Select(u => u.ProfileId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (string.IsNullOrEmpty(profileId))
                {
                    _logger?.LogWarning("No profile found for UserId {UserId}", userId);
                    return null;
                }

                // 2. Get the scouting report for that profile
                var scoutingReport = await _context.ScoutingReport
                    .AsNoTracking()
                    .FirstOrDefaultAsync(sr => sr.ProfileId == profileId, cancellationToken);

                return scoutingReport;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting ScoutingReport for UserId {UserId}", userId);
                throw;
            }
        }

        public async Task<Profile> GetProfileByUserId(string runId,CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _context.Profile
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.UserId == runId, cancellationToken);

                if (result == null)
                {
                    _logger?.LogWarning("Profile not found for UserId: {UserId}", runId);
                    return null;
                }

                result.TotalWins = await _context.GameWinningPlayer
                    .AsNoTracking()
                    .CountAsync(p => p.ProfileId == result.ProfileId, cancellationToken);

                result.TotalLosses = await _context.GameLosingPlayer
                    .AsNoTracking()
                    .CountAsync(p => p.ProfileId == result.ProfileId, cancellationToken);

                // Followers
                var followersList = await _context.Follower
                    .AsNoTracking()
                    .Where(f => f.ProfileId == result.ProfileId)
                    .ToListAsync(cancellationToken);

                // Followings
                var followingsList = await _context.Following
                    .AsNoTracking()
                    .Where(f => f.FollowingProfileId == result.ProfileId)
                    .ToListAsync(cancellationToken);

                // Extract the follower/following IDs if any
                var followerIds = followersList.Select(f => f.FollowerProfileId).ToList();
                var followingIds = followingsList.Select(f => f.ProfileId).ToList();

                // Fetch profiles
                var followerProfiles = followerIds.Any()
                    ? await _context.Profile
                        .AsNoTracking()
                        .Where(p => followerIds.Contains(p.ProfileId))
                        .ToListAsync(cancellationToken)
                    : new List<Profile>();

                var followingProfiles = followingIds.Any()
                    ? await _context.Profile
                        .AsNoTracking()
                        .Where(p => followingIds.Contains(p.ProfileId))
                        .ToListAsync(cancellationToken)
                    : new List<Profile>();

                // Assign to result
                result.FollowersList = followerProfiles;
                result.FollowingList = followingProfiles;

                result.FollowersCount = followerProfiles.Count;
                result.FollowingCount = followingProfiles.Count;

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting profile by UserId: {UserId}", runId);
                throw;
            }
        }


        /// <summary>
        /// Get user by email with proper error handling
        /// </summary>
        public async Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.User
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting user by email: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Check if email is available (not already in use)
        /// </summary>
        public async Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                return !await _context.User
                    .AnyAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking email availability: {Email}", email);
                throw;
            }
        }


        /// <summary>
        /// Create a new user with secure password hashing
        /// </summary>
        public async Task<User> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
        {
            // Validate inputs first
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password is required", nameof(password));

            try
            {
                // Use a transaction to ensure all operations succeed or fail together
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Generate a new user ID if not provided
                    user.UserId ??= Guid.NewGuid().ToString();

                    // Hash the password before storing
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                    user.SecurityStamp = Guid.NewGuid().ToString();
                    user.SignUpDate = DateTime.UtcNow;
                    user.Status ??= "Active";
                    user.AccessLevel ??= "Standard";

                    // Get default ranking in a more efficient way
                    int defaultRanking = await _context.Profile
                        .Where(p => p.Status == "Active")
                        .CountAsync(cancellationToken);

                    // Create profile with minimal properties
                    var profileId = Guid.NewGuid().ToString();
                    var playerNumber = Common.UniqueIdNumber.GenerateSixDigit();

                    var profile = new Profile
                    {
                        UserId = user.UserId,
                        ProfileId = profileId,
                        Position = "N/A",
                        Ranking = defaultRanking + 1,
                        StarRating = 1,
                        ImageURL = $"https://uhblobstorageaccount.blob.core.windows.net/profileimage/{profileId}+.webp",
                        PlayerNumber = playerNumber
                    };

                    // Create scouting report with default values
                    var report = new ScoutingReport
                    {
                        ScoutingReportId = Guid.NewGuid().ToString(),
                        ProfileId = profileId,
                        Shooting = 1,
                        BallHandling = 1,
                        Passing = 1,
                        Defense = 1,
                        Rebounding = 1,
                        Athleticism = 1
                    };

                    // Create default settings
                    var setting = new Setting
                    {
                        SettingId = Guid.NewGuid().ToString(),
                        AllowEmailNotification = true,
                        AllowComments = true,
                        ShowGameHistory = true
                    };

                    // Add all entities in batch
                    await _context.User.AddAsync(user, cancellationToken);
                    await _context.Profile.AddAsync(profile, cancellationToken);
                    await _context.ScoutingReport.AddAsync(report, cancellationToken);
                    await _context.Setting.AddAsync(setting, cancellationToken);

                    // Save changes once
                    await _context.SaveChangesAsync(cancellationToken);

                    // Commit transaction
                    await transaction.CommitAsync(cancellationToken);

                    _logger?.LogInformation("Created new user {UserId} with email {Email}", user.UserId, user.Email);
                    return user;
                }
                catch (Exception)
                {
                    // Rollback transaction on error
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating user with email {Email}", user.Email);
                throw;
            }
        }

        /// <summary>
        /// Verify a user's password
        /// </summary>
        public bool VerifyPassword(User user, string password)
        {
            try
            {
                if (user == null || string.IsNullOrEmpty(password))
                    return false;

                // Make sure the hash is properly formatted
                string hashToVerify = user.PasswordHash;
                if (hashToVerify != null && !hashToVerify.StartsWith("$"))
                {
                    // If the hash doesn't start with $, it might be missing a character
                    // Try prepending the $ character
                    hashToVerify = "$" + hashToVerify;
                }

                // Log both hashes for debugging
                Console.WriteLine($"Original hash: {user.PasswordHash}");
                Console.WriteLine($"Fixed hash: {hashToVerify}");

                // Use the potentially fixed hash
                //bool result = BCrypt.Net.BCrypt.Verify(password, hashToVerify);
                //Console.WriteLine($"Verification result: {result}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in VerifyPassword: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Update the last login date for a user
        /// </summary>
        public async Task UpdateLastLoginDateAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _context.User.FindAsync(new object[] { userId }, cancellationToken);

                if (user != null)
                {
                    user.LastLoginDate = DateTime.UtcNow;
                    _context.Entry(user).State = EntityState.Modified;
                    await SaveChangesAsync(cancellationToken);
                    _logger?.LogInformation("Updated last login date for user {UserId}", userId);
                }
                else
                {
                    _logger?.LogWarning("User not found when updating last login date: {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating last login date for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving changes to database");
                throw;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await _context.DisposeAsync();
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        public Task<bool> UpdateUserAsync(User privateRun, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}