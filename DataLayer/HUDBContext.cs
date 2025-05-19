using Microsoft.EntityFrameworkCore;
using Domain;

namespace DataLayer
{
    public partial class HUDBContext : DbContext
    {
        public HUDBContext()
        {
        
        }

        public HUDBContext(DbContextOptions<HUDBContext> options)
            : base(options)
        {

        }
     
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Post> Post { get; set; }
        public virtual DbSet<Following> Following { get; set; }
        public virtual DbSet<Follower> Follower { get; set; }
        public virtual DbSet<SavedPost> SavedPost { get; set; }
        public virtual DbSet<LikedPost> LikedPost { get; set; }
        public virtual DbSet<Comment> Comment { get; set; }
        public virtual DbSet<PlayerComment> PlayerComment { get; set; }
        public virtual DbSet<PostComment> PostComment { get; set; }
        public virtual DbSet<PrivateRun> PrivateRun { get; set; }
        public virtual DbSet<PrivateRunInvite> PrivateRunInvite { get; set; }
        public virtual DbSet<ErrorException> ErrorException { get; set; }
        public virtual DbSet<Profile> Profile { get; set; }
        public virtual DbSet<Tag> Tag { get; set; }
        public virtual DbSet<Rating> Rating { get; set; }
        public virtual DbSet<History> History { get; set; }
        public virtual DbSet<Game> Game { get; set; }
        public virtual DbSet<Setting> Setting { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<Notification> Notification { get; set; }
        public virtual DbSet<PushSubscription> PushSubscription { get; set; }
        public virtual DbSet<Court> Court { get; set; }
        public virtual DbSet<Contact> Contact { get; set; }
        public virtual DbSet<Organization> Organization { get; set; }
        public virtual DbSet<StatusUpdateTime> StatusUpdateTime { get; set; }
        public virtual DbSet<PostUpdateTime> PostUpdateTime { get; set; }
        public virtual DbSet<ThirdPartyService> ThirdPartyService { get; set; }
        public virtual DbSet<ProjectManagement> ProjectManagement { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<Activity> Activity { get; set; }
        public virtual DbSet<ScoutingReport> ScoutingReport { get; set; }
        public virtual DbSet<Criteria> Criteria { get; set; }
        public virtual DbSet<Squad> Squad { get; set; }
        public virtual DbSet<SquadTeam> SquadTeam { get; set; }
        public virtual DbSet<SquadRequest> SquadRequest { get; set; }
        public virtual DbSet<Message> Message { get; set; }
        public virtual DbSet<Conversation> Conversation { get; set; }
        public virtual DbSet<ConversationParticipant> ConversationParticipant { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }


        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
