using DotaDashboard.Data;
using DotaDashboard.Services.Implementation;
using DotaDashboard.Services.Interfaces;
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

// Register application services
builder.Services.AddScoped<IOpenDotaService, OpenDotaService>();
builder.Services.AddScoped<IDataIngestionService, DataIngestionService>();

var app = builder.Build();

// Apply database migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying database migrations...");
        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
        throw;
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

app.Run();