using ApiClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
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
// In Program.cs
// In Program.cs where services are being registered
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Website.Services.AuthenticationService>();
// Register API client services
builder.Services.AddApiClientServices(builder.Configuration);

builder.Services.AddHttpClient<IRunApi, RunApi>(client =>
{
 
});
builder.Services.AddHttpClient<IClientApi, ClientApi>(client =>
{

});
builder.Services.AddHttpClient<IUserApi, UserApi>(client =>
{

});
builder.Services.AddHttpClient<IJoinedRunApi, JoinedRunApi>(client =>
{

});

builder.Services.AddHttpClient<IProductApi, ProductApi>(client =>
{

});
builder.Services.AddHttpClient<IProfileApi, ProfileApi>(client =>
{

});

builder.Services.AddHttpClient<IStorageApi, StorageApi>(client =>
{

});
builder.Services.AddHttpClient<IReportApi, ReportApi>(client =>
{

});
builder.Services.AddHttpClient<IPostApi, PostApi>(client =>
{

});
builder.Services.AddHttpClient<IVideoApi, VideoApi>(client =>
{

});


//builder.Services.AddHttpClient<IUserApi, UserApi>(user =>
//{

//});
// In Program.cs
// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // Set session timeout (e.g., 2 hours)
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Use Always in production
});

// Then after building the app, before mapping controllers:

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

// Enable session
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();