using DotaDashboard.Data;
using DotaDashboard.Helpers;
using DotaDashboard.Services.Implementation;
using DotaDashboard.Services.Interfaces;
using DotaDashboard.Workers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();

// Configure PostgreSQL Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var supabaseConnection = builder.Configuration.GetConnectionString("SupabaseConnection");

// Use Supabase if configured (production), otherwise use local PostgreSQL
if (!string.IsNullOrEmpty(supabaseConnection))
{
    connectionString = supabaseConnection;
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString,
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null));
});

// Configure HttpClient for OpenDota API
builder.Services.AddHttpClient("OpenDotaApi", client =>
{
    client.BaseAddress = new Uri("https://api.opendota.com/");
    client.DefaultRequestHeaders.Add("User-Agent", "OpenDotaDashboard/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register RateLimiterService as Singleton
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var rateLimit = config.GetValue("OpenDotaApi:RateLimitPerMinute", 60);
    return new RateLimiterService(rateLimit);
});

// Register application services
builder.Services.AddScoped<IOpenDotaService, OpenDotaService>();
builder.Services.AddScoped<IDataIngestionService, DataIngestionService>();
builder.Services.AddScoped<IAggregateStatsService, AggregateStatsService>();
builder.Services.AddScoped<IJobService, JobService>();

// Register RabbitMQ Helper as Singleton
builder.Services.AddSingleton<RabbitMqHelper>();

// Register Background Worker
builder.Services.AddHostedService<JobWorker>();

var app = builder.Build();

// Apply database migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying database migrations...");

        // Use async migration with increased timeout
        dbContext.Database.SetCommandTimeout(180); // 3 minutes
        await dbContext.Database.MigrateAsync();

        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");

        // Don't throw in production - let app start even if migrations fail
        if (builder.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// Listen on PORT environment variable (Render requirement)
// Only override URLs in production/non-development environments
if (!app.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    app.Urls.Clear(); // Clear any existing URLs
    app.Urls.Add($"http://0.0.0.0:{port}");
}

app.Run();