using Sprout.Api.Endpoints;
using Sprout.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITaskService, JsonTaskService>();
builder.Services.AddSingleton<IProgressService, JsonProgressService>();
builder.Services.AddCors();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors(policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyMethod()
        .AllowAnyHeader());
}

app.UseStaticFiles();

app.MapTaskEndpoints();
app.MapProgressEndpoints();

app.MapFallbackToFile("index.html");

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
