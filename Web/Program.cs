using ApiClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using WebAPI.ApiClients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HttpClient factory
builder.Services.AddHttpClient();

// Register HttpContextAccessor and AuthenticationService
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Website.Services.AuthenticationService>();

// Register API client services
builder.Services.AddApiClientServices(builder.Configuration);

// Register HttpClient services
builder.Services.AddHttpClient<IRunApi, RunApi>(client => { });
builder.Services.AddHttpClient<IClientApi, ClientApi>(client => { });
builder.Services.AddHttpClient<IUserApi, UserApi>(client => { });
builder.Services.AddHttpClient<IJoinedRunApi, JoinedRunApi>(client => { });
builder.Services.AddHttpClient<IProductApi, ProductApi>(client => { });
builder.Services.AddHttpClient<IProfileApi, ProfileApi>(client => { });
builder.Services.AddHttpClient<IStorageApi, StorageApi>(client => { });
builder.Services.AddHttpClient<IReportApi, ReportApi>(client => { });
builder.Services.AddHttpClient<IPostApi, PostApi>(client => { });
builder.Services.AddHttpClient<IVideoApi, VideoApi>(client => { });
builder.Services.AddHttpClient<IGameApi, GameApi>(client => { });

// Configure session options for remember me functionality
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Use SameAsRequest for development
    options.Cookie.Name = "UH_Session";
});

// Configure data protection for cookie encryption (optional but recommended)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"./keys"))
    .SetApplicationName("UltimateHoopers");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable session middleware (must be before UseAuthorization)
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();