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
        public DbSet<Activity> Activity { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Contact> Contact { get; set; }
        public DbSet<Court> Courts { get; set; }
        public DbSet<Criteria> Criteria { get; set; }
        public DbSet<ErrorException> ErrorExceptions { get; set; }
        public DbSet<Follower> Followers { get; set; }
        public DbSet<Following> Following { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<LikedPost> LikedPost { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<Organization> Organization { get; set; }
        public DbSet<PlayerComment> PlayerComment { get; set; }
        public DbSet<Post> Post { get; set; }
        public DbSet<PostComment> PostComment { get; set; }
        public DbSet<PostUpdateTime> PostUpdateTime { get; set; }
        public DbSet<PrivateRun> PrivateRun { get; set; }
        public DbSet<PrivateRunInvite> PrivateRunInvites{ get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Profile> Profile { get; set; }
        public DbSet<ProjectManagement> ProjectManagement { get; set; }
        public DbSet<PushSubscription> PushSubscription { get; set; }
        public DbSet<Rating> Rating { get; set; }
        public DbSet<SavedPost> SavedPost { get; set; }
        public DbSet<ScoutingReport> ScoutingReport { get; set; }
        public DbSet<Setting> Setting { get; set; }
        public DbSet<Squad> Squad { get; set; }
        public DbSet<SquadTeam> SquadTeam { get; set; }
        public DbSet<SquadRequest> SquadRequest { get; set; }
        public DbSet<StatusUpdateTime> StatusUpdateTime { get; set; }
        public DbSet<Tag> Tag { get; set; }
        public DbSet<ThirdPartyService> ThirdPartyService { get; set; }
        public DbSet<User> User { get; set; }

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