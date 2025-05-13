// DataLayer/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    /// <summary>
    /// Unified application database context that replaces multiple individual contexts
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Entity sets
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Court> Courts { get; set; }
        public DbSet<Criteria> Criteria { get; set; }
        public DbSet<ErrorException> ErrorExceptions { get; set; }
        public DbSet<Follower> Followers { get; set; }
        public DbSet<Following> Following { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<LikedPost> LikedPosts { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<PlayerComment> PlayerComments { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<PostUpdateTime> PostUpdateTimes { get; set; }
        public DbSet<PrivateRun> PrivateRuns { get; set; }
        public DbSet<PrivateRunInvite> PrivateRunInvites { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<ProjectManagement> ProjectManagement { get; set; }
        public DbSet<PushSubscription> PushSubscriptions { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<SavedPost> SavedPosts { get; set; }
        public DbSet<ScoutingReport> ScoutingReports { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Squad> Squads { get; set; }
        public DbSet<SquadTeam> SquadTeams { get; set; }
        public DbSet<SquadRequest> SquadRequests { get; set; }
        public DbSet<StatusUpdateTime> StatusUpdateTimes { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ThirdPartyService> ThirdPartyServices { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity relationships and constraints here
            // Example:
            modelBuilder.Entity<User>()
                .HasOne(u => u.Profile)
                .WithOne()
                .HasForeignKey<Profile>(p => p.UserId);

            modelBuilder.Entity<Post>()
                .HasOne<Profile>()
                .WithMany()
                .HasForeignKey(p => p.ProfileId);

            modelBuilder.Entity<PostComment>()
                .HasOne<Post>()
                .WithMany(p => p.PostComments)
                .HasForeignKey(pc => pc.PostId);

            // Add more configurations as needed
        }
    }
}