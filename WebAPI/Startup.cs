// WebAPI/Startup.cs (Modified)
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using WebAPI.Services;
using WebAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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

            // Add DataLayer services (uses the unified ApplicationDbContext and repositories)
            services.AddDataLayer(Configuration);

            // Register application services
            services.AddScoped<IAuthenticateService, AuthenticationService>();
            services.AddScoped<IPasswordService, PasswordService>();

            // Add authentication
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

            // Add health checks
            services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>("database", HealthStatus.Unhealthy)
                .AddCheck("api_health", () => HealthCheckResult.Healthy());

            // Add CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAppClient", builder =>
                {
                    builder.WithOrigins(
                            Configuration.GetSection("AllowedOrigins").Get<string[]>() ??
                            new[] { "https://undergroundhoopers.com", "https://www.undergroundhoopers.com" })
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            // Configure Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Underground Hoopers API",
                    Version = "v1",
                    Description = "API for Underground Hoopers basketball application",
                    Contact = new OpenApiContact
                    {
                        Name = "Support",
                        Email = "support@undergroundhoopers.com"
                    }
                });

                // Include XML comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Add security definition for JWT
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
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
            });

            // Add HttpClient services
            services.AddHttpClient();

            // Add memory cache
            services.AddMemoryCache();

            // Add response caching
            services.AddResponseCaching();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "UndergroundHoopers API v1"));
            }
            else
            {
                // Use exception handler middleware in production
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            // Use security headers middleware
            app.UseSecurityHeaders();

            // Configure caching headers
            app.Use(async (context, next) =>
            {
                // Configure static files to be cached for 7 days
                if (context.Request.Path.StartsWithSegments("/static") ||
                    context.Request.Path.Value.Contains(".webp") ||
                    context.Request.Path.Value.Contains(".png") ||
                    context.Request.Path.Value.Contains(".jpg"))
                {
                    context.Response.Headers.Add("Cache-Control", "public,max-age=604800");
                }

                await next();
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            // Enable CORS
            app.UseCors("AllowAppClient");

            // Enable response caching
            app.UseResponseCaching();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // Configure health checks
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(
                            new
                            {
                                status = report.Status.ToString(),
                                checks = report.Entries.Select(e => new
                                {
                                    name = e.Key,
                                    status = e.Value.Status.ToString(),
                                    description = e.Value.Description
                                }),
                                duration = report.TotalDuration
                            });
                        await context.Response.WriteAsync(result);
                    }
                });
            });
        }
    }
}