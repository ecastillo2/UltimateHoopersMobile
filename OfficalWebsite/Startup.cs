using ApiClient;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net.Http.Headers;
using WebAPI.ApiClients;

namespace OfficalWebsite
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
            // Register IHttpClientFactory
            services.AddHttpClient();
            // Register WebApiService with HttpClient
            services.AddHttpClient<WebApiService>()
                    .ConfigureHttpClient(client =>
                    {
                        // Optionally, you can set headers, default values, etc.
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    });
            services.AddHttpClient<IPostApi, PostApi>();
            services.AddControllersWithViews();

            // Enable CORS with the necessary policy
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 524288000; // 100 MB
            });

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Set the timeout as needed
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true; // Make the session cookie essential
            });
            services.AddHttpContextAccessor();


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie("TmiginScheme", options =>
            {
                options.LoginPath = "/Home/Index";
                options.LogoutPath = "/Home/Index";
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.SlidingExpiration = true;
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 524288000; // 100 MB (adjust as needed)
            });

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 524288000; // 100 MB in bytes
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                await next();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            // Apply the CORS policy
            // Use CORS policy
            app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader());
            app.UseRouting();
            app.UseSession();
            //app.UseMiddleware<TokenValidationMiddleware>();
            app.UseAuthentication(); // Ensure authentication middleware is used
            app.UseAuthorization();
            app.Use(async (context, next) =>
            {
                string CurrentUserIDSession = context.Session.GetString("UserId");
                if (!context.Request.Path.Value.Contains("/Home/Index"))
                {
                    //if (string.IsNullOrEmpty(CurrentUserIDSession))
                    //{
                    //    var path = $"/Home/Index?ReturnUrl={context.Request.Path}";
                    //    context.Response.Redirect(path);
                    //    return;
                    //}

                }
                await next();
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
