using DataLayer.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebAPI.Services;

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
    // Register repositories
    services.AddScoped<IActivityRepository, ActivityRepository>();
    services.AddScoped<IErrorExceptionRepository, ErrorExceptionRepository>();
    // Register repositories
    services.AddScoped<IActivityRepository, ActivityRepository>();

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

    // Remaining service configuration...
}