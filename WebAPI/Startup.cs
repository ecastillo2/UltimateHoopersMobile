using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using WebAPI.Middleware;
using WebAPI.Services;
using DataLayer.Repositories;
using DataLayer;

namespace WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add controllers
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Configure JSON serialization
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            // Get connection string from configuration
            var connectionString = Configuration.GetConnectionString("UnderGroundhoopersDB");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection string 'UnderGroundhoopersDB' is not configured");
            }

            // Configure database contexts
            services.AddDbContext<HUDBContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(3);
                    sqlOptions.CommandTimeout(30);
                }));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(3);
                    sqlOptions.CommandTimeout(30);
                }));

            services.AddDbContext<ErrorExceptionContext>(options =>
                options.UseSqlServer(connectionString));

            // Register services
            services.AddScoped<IAuthenticateService, AuthenticationService>();
            services.AddScoped<IPasswordService, PasswordService>();

            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IPostCommentRepository, PostCommentRepository>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IFollowingRepository, FollowingRepository>();
            services.AddScoped<IFollowerRepository, FollowerRepository>();
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<IPrivateRunRepository, PrivateRunRepository>();
            services.AddScoped<IPrivateRunInviteRepository, PrivateRunInviteRepository>();
            services.AddScoped<ICourtRepository, CourtRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddScoped<IErrorExceptionRepository, ErrorExceptionRepository>();

            // Add Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Add CORS policy
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins("https://undergroundhoopers.com", "https://www.undergroundhoopers.com")
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            // Add JWT Authentication
            var jwtSection = Configuration.GetSection("Jwt");
            var jwtKey = jwtSection["Key"];
            var jwtIssuer = jwtSection["Issuer"];
            var jwtAudience = jwtSection["Audience"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Add Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Underground Hoopers API", Version = "v1" });

                // Add JWT Authentication to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // Set the comments path for the Swagger JSON and UI
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });

            // Add memory caching
            services.AddMemoryCache();

            // Add response caching
            services.AddResponseCaching();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Enable Swagger in development only
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Underground Hoopers API V1");
                    c.RoutePrefix = "swagger";
                });
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            // Security headers
            app.UseSecurityHeaders();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Use CORS policy
            app.UseCors();

            // Enable authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Use response caching
            app.UseResponseCaching();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}