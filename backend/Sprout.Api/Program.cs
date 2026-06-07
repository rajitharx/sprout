using Sprout.Api.Endpoints;
using Sprout.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISystemClock, SystemClock>();
builder.Services.AddSingleton<ITaskService, JsonTaskService>();
builder.Services.AddSingleton<IProgressService, JsonProgressService>();
builder.Services.AddSingleton<IChildProfileService, JsonChildProfileService>();
builder.Services.AddSingleton<IAuthenticationService, ConfigurationAuthenticationService>();
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
