var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Add model binding and validation
    options.ModelValidatorProviders.Clear();
});

// Add session support for authentication demo
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true; // Security: prevent XSS
    options.Cookie.IsEssential = true; // Required for GDPR compliance
    options.Cookie.Name = "TaskFlowPro.Session";
    options.Cookie.SameSite = SameSiteMode.Strict; // CSRF protection
});

// Add anti-forgery token support
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
    options.SuppressXFrameOptionsHeader = false;
});

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add memory cache for performance
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware (must be before UseAuthorization)
app.UseSession();

// Add anti-forgery middleware
app.UseAntiforgery();

app.UseAuthorization();

// Map static assets
app.MapStaticAssets();

// Configure routing
app.MapControllerRoute(
    name: "dashboard",
    pattern: "dashboard/{role}",
    defaults: new { controller = "Home", action = "Dashboard" },
    constraints: new { role = @"^(admin|manager|user)$" });

app.MapControllerRoute(
    name: "auth",
    pattern: "auth/{action}",
    defaults: new { controller = "Home" },
    constraints: new { action = @"^(login|logout)$" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}));

// Custom middleware for session management
app.Use(async (context, next) =>
{
    // Log session activity
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var sessionId = context.Session.Id;
    var path = context.Request.Path;

    logger.LogInformation($"Session {sessionId} accessing {path} at {DateTime.Now}");

    await next();
});

app.Run();