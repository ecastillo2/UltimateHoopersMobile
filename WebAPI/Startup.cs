using DataLayer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Models;
using WebAPI.Services;
using System.Diagnostics;
using System.Reflection;
using System.Text;


namespace WebAPI
{
    /// <summary>
    /// Startup
    /// </summary>
    public class Startup
    {
        string UnderGroundhoopersConnectionString;
        public IConfiguration Configuration { get; }
        /// <summary>
        /// Startup
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            UnderGroundhoopersConnectionString = Configuration.GetConnectionString("UnderGroundhoopersDB");
        }

        static string XmlCommentsFilePath
        {
            get
            {
                var basePath = AppContext.BaseDirectory;
                var fileName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                return Path.Combine(basePath, fileName);
            }
        }


        public void ConfigureServices(IServiceCollection services)
        {
            // In ConfigureServices() or equivalent
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Your API", Version = "v1" });
            });

            // Enable CORS with the necessary policy
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });
            services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
            services.AddResponseCompression();
            services.AddSession();
            services.AddControllersWithViews();
            services.AddControllers();
            services.AddMvc();
            services.AddSignalRCore();
            // Register the Swagger generator, defining one or more Swagger documents  
            services.AddSwaggerGen(c =>
            {
                c.IncludeXmlComments(XmlCommentsFilePath);

            });
            
            services.AddControllers(options => options.EnableEndpointRouting = false);
            //services.AddControllers();
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            // Add HttpClient and configure the timeout
            services.AddHttpClient("MyApiClient", client =>
            {
                client.Timeout = TimeSpan.FromDays(30); // Set the desired timeout duration in seconds
            });
            //JWT Authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.UTF8.GetBytes(appSettings.Key);

            services.AddAuthentication(au =>
            {

                au.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                au.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(jwt =>
            {

                jwt.RequireHttpsMetadata = false;
                jwt.SaveToken = true;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };


            });
            services.AddDataProtection();
            services.AddScoped<IAuthenticateService, AuthenticateService>();

            services.AddDbContextPool<HUDBContext>(options => options.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<UserContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<PostContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<FollowingContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<FollowerContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<SavedPostContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<LikedPostContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<ProfileContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<CommentContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<PostCommentContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<PlayerCommentContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<PrivateRunContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<PrivateRunInviteContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<TagContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<RatingContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<ErrorExceptionContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<HistoryContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<GameContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<SettingContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<ProductContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<NotificationContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<CourtContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<ContactContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<PushSubscriptionContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<OrganizationContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<StatusUpdateTimeContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<PostUpdateTimeContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<ThirdPartyServiceContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<ProjectManagementContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<OrderContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            
            services.AddDbContextPool<ScoutingReportContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<CriteriaContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<SquadContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<SquadTeamContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));
            services.AddDbContextPool<SquadRequestContext>(opitons => opitons.UseSqlServer(UnderGroundhoopersConnectionString));



            // Register repositories
            services.AddScoped<DataLayer.DAL.IProfileRepository, DataLayer.DAL.ProfileRepository>();
            services.AddScoped<DataLayer.DAL.IUserRepository, DataLayer.DAL.UserRepository>();
            services.AddScoped<DataLayer.DAL.IGameRepository, DataLayer.DAL.GameRepository>();
            services.AddScoped<DataLayer.DAL.IPostRepository, DataLayer.DAL.PostRepository>();
            services.AddScoped<DataLayer.DAL.IFollowingRepository, DataLayer.DAL.FollowingRepository>();
            services.AddScoped<DataLayer.DAL.IFollowerRepository, DataLayer.DAL.FollowerRepository>();
            services.AddScoped<DataLayer.DAL.ISavedPostRepository, DataLayer.DAL.SavedPostRepository>();
            services.AddScoped<DataLayer.DAL.ILikedPostRepository, DataLayer.DAL.LikedPostRepository>();
            services.AddScoped<DataLayer.DAL.IPlayerCommentRepository, DataLayer.DAL.PlayerCommentRepository>();
            services.AddScoped<DataLayer.DAL.IPostCommentRepository, DataLayer.DAL.PostCommentRepository>();
            services.AddScoped<DataLayer.DAL.IPrivateRunRepository, DataLayer.DAL.PrivateRunRepository>();
            services.AddScoped<DataLayer.DAL.IPrivateRunInviteRepository, DataLayer.DAL.PrivateRunInviteRepository>();
            services.AddScoped<DataLayer.DAL.ITagRepository, DataLayer.DAL.TagRepository>();
            services.AddScoped<DataLayer.DAL.IRatingRepository, DataLayer.DAL.RatingRepository>();
            services.AddScoped<DataLayer.DAL.IErrorExceptionRepository, DataLayer.DAL.ErrorExceptionRepository>();
            services.AddScoped<DataLayer.DAL.IHistoryRepository, DataLayer.DAL.HistoryRepository>();
            services.AddScoped<DataLayer.DAL.ISettingRepository, DataLayer.DAL.SettingRepository>();
            services.AddScoped<DataLayer.DAL.IProductRepository, DataLayer.DAL.ProductRepository>();
            services.AddScoped<DataLayer.DAL.INotificationRepository, DataLayer.DAL.NotificationRepository>();
            services.AddScoped<DataLayer.DAL.ICourtRepository, DataLayer.DAL.CourtRepository>();
            services.AddScoped<DataLayer.DAL.IContactRepository, DataLayer.DAL.ContactRepository>();
            services.AddScoped<DataLayer.DAL.IPushSubscriptionRepository, DataLayer.DAL.PushSubscriptionRepository>();
            services.AddScoped<DataLayer.DAL.IOrganizationRepository, DataLayer.DAL.OrganizationRepository>();
            services.AddScoped<DataLayer.DAL.IThirdPartyServiceRepository, DataLayer.DAL.ThirdPartyServiceRepository>();
            services.AddScoped<DataLayer.DAL.IProjectManagementRepository, DataLayer.DAL.ProjectManagementRepository>();
            services.AddScoped<DataLayer.DAL.IOrderRepository, DataLayer.DAL.OrderRepository>();
            services.AddScoped<DataLayer.DAL.IScoutingReportRepository, DataLayer.DAL.ScoutingReportRepository>();
            services.AddScoped<DataLayer.DAL.ISquadRepository, DataLayer.DAL.SquadRepository>();
            services.AddScoped<DataLayer.DAL.IActivityRepository, DataLayer.DAL.ActivityRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseCors(policy =>
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());

            // Configure Swagger
            app.UseSwagger(c =>
            {

            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ultimate Hoopers API v1");
                c.RoutePrefix = "swagger";
            });

            app.UseStaticFiles();
            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseSession();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseResponseCompression();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private class GetTypeInfo
        {
        }
    }
}
