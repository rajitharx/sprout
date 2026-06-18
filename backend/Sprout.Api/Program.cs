using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Sprout.Api.Endpoints;
using Sprout.Api.Services;
using Sprout.DataAccess.Context;
using Sprout.DataAccess.DataMigration;
using Sprout.DataAccess.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISystemClock, SystemClock>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=Sprout;Username=postgres;Password=postgres";

// Only register PostgreSQL if not using an in-memory connection string
var isInMemoryDb = connectionString.Contains("Data Source=:memory:") ||
                   connectionString.Contains("InMemory") ||
                   connectionString.Contains(":memory:");

if (!isInMemoryDb)
{
    builder.Services.AddDbContext<SproutDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Use a stable database name based on the connection string for tests
    var dbName = "SproutDb_" + connectionString.GetHashCode().ToString().Replace("-", "");
    builder.Services.AddDbContext<SproutDbContext>(options =>
        options.UseInMemoryDatabase(dbName));
}

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IProgressRepository, ProgressRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();

builder.Services.AddScoped<ITaskService, DbTaskService>();
builder.Services.AddScoped<IProgressService, DbProgressService>();
builder.Services.AddScoped<IChildProfileService, DbChildProfileService>();
builder.Services.AddSingleton<IAuthenticationService, ConfigurationAuthenticationService>();

builder.Services.ConfigureHttpJsonOptions(opt =>
{
    opt.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddCors();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var debugConfig = app.Configuration.GetSection("Debug");
var debugEnabled = debugConfig.GetValue<bool>("Enabled");

if (debugEnabled)
{
    logger.LogInformation("🔧 Debug mode enabled");
    logger.LogInformation("📋 LogRequests: {LogRequests}", debugConfig.GetValue<bool>("LogRequests"));
    logger.LogInformation("📋 LogServiceCalls: {LogServiceCalls}", debugConfig.GetValue<bool>("LogServiceCalls"));
    logger.LogInformation("📋 LogExceptions: {LogExceptions}", debugConfig.GetValue<bool>("LogExceptions"));
}

// Run database migrations and migrate data from JSON files
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<SproutDbContext>();
        var isRelationalDb = context.Database.IsRelational();

        if (isRelationalDb)
        {
            logger.LogInformation("🗄️ Running database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("✅ Database migrations completed");
        }
        else
        {
            logger.LogInformation("🗄️ Creating database schema (in-memory)...");
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("✅ Database schema created");
        }

        // Migrate data from JSON files to database
        var dataPath = app.Configuration["Storage:DataPath"] ?? "Storage/data";
        var jsonMigration = new JsonToDbMigration(context, dataPath);
        logger.LogInformation("📦 Migrating data from JSON files to database...");
        await jsonMigration.MigrateAsync();
        logger.LogInformation("✅ Data migration completed");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error during database initialization");
        throw;
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (debugConfig.GetValue<bool>("LogRequests"))
{
    app.UseMiddleware<RequestLoggingMiddleware>();
}

if (app.Environment.IsDevelopment())
{
    app.UseCors(policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyMethod()
        .AllowAnyHeader());
}

app.UseStaticFiles();

app.MapAuthenticationEndpoints();
app.MapTaskEndpoints();
app.MapProgressEndpoints();
app.MapChildProfileEndpoints();

app.MapFallbackToFile("index.html");

app.Run();

// Global exception handling middleware
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly bool _logExceptions;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IConfiguration config)
    {
        _next = next;
        _logger = logger;
        _logExceptions = config.GetSection("Debug").GetValue<bool>("LogExceptions");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (_logExceptions)
            {
                _logger.LogError(ex, "❌ Unhandled exception in {Path} {Method}", context.Request.Path, context.Request.Method);
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "An error occurred", message = _logExceptions ? ex.Message : null });
        }
    }
}

// Request logging middleware
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogDebug("→ {Method} {Path}", context.Request.Method, context.Request.Path);

        await _next(context);

        var duration = DateTime.UtcNow - startTime;
        _logger.LogDebug("← {StatusCode} {Path} ({ElapsedMs}ms)", context.Response.StatusCode, context.Request.Path, duration.TotalMilliseconds);
    }
}

// Required for WebApplicationFactory in integration tests
public partial class Program { }
