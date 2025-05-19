using DataLayer;
using DataLayer.DAL;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register the main DbContext
            services.AddDbContext<HUDBContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("UnderGroundhoopersDB"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure()
                ));

            // Register repositories
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IFollowingRepository, FollowingRepository>();
            services.AddScoped<IFollowerRepository, FollowerRepository>();
            services.AddScoped<ISavedPostRepository, SavedPostRepository>();
            services.AddScoped<ILikedPostRepository, LikedPostRepository>();
            services.AddScoped<IPlayerCommentRepository, PlayerCommentRepository>();
            services.AddScoped<IPostCommentRepository, PostCommentRepository>();
            services.AddScoped<IPrivateRunRepository, PrivateRunRepository>();
            services.AddScoped<IPrivateRunInviteRepository, PrivateRunInviteRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IRatingRepository, RatingRepository>();
            services.AddScoped<IErrorExceptionRepository, ErrorExceptionRepository>();
            services.AddScoped<IHistoryRepository, HistoryRepository>();
            services.AddScoped<ISettingRepository, SettingRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<ICourtRepository, CourtRepository>();
            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<IPushSubscriptionRepository, PushSubscriptionRepository>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<IThirdPartyServiceRepository, ThirdPartyServiceRepository>();
            services.AddScoped<IProjectManagementRepository, ProjectManagementRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IScoutingReportRepository, ScoutingReportRepository>();
            services.AddScoped<ISquadRepository, SquadRepository>();
            services.AddScoped<IActivityRepository, ActivityRepository>();

            return services;
        }
    }
}
