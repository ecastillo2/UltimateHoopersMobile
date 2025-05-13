// File: WebAPI/Startup.cs (partial)
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using WebAPI.Extensions;
using System;
using System.IO;
using System.Reflection;
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

            // Add database contexts and repositories
            services.AddDataServices(Configuration);

            // Add common services
            services.AddCommonServices(Configuration);

            // Add authentication and authorization
            services.AddAuthenticationServices(Configuration);

            // Add health checks
            services.AddHealthChecks()
                .AddDbContextCheck<HUDBContext>();

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
                // Use custom exception handling in production
                app.UseGlobalExceptionHandler();
                app.UseHsts();
            }

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

            // Enable response caching
            app.UseResponseCaching();

            // Set up CORS
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}