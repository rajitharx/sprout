using Sprout.Api.Services;

namespace Sprout.Api.Endpoints;

public static class ProgressEndpoints
{
    public static void MapProgressEndpoints(this WebApplication app)
    {
        app.MapGet("/api/progress/today", async (IProgressService svc) =>
            Results.Ok(await svc.GetTodayAsync()));

        app.MapPost("/api/progress/complete/{taskId}", async (string taskId, IProgressService svc) =>
            Results.Ok(await svc.MarkCompleteAsync(taskId)));

        app.MapPost("/api/progress/incomplete/{taskId}", async (string taskId, IProgressService svc) =>
            Results.Ok(await svc.MarkIncompleteAsync(taskId)));

        app.MapGet("/api/progress/week", async (IProgressService svc) =>
            Results.Ok(await svc.GetWeekAsync()));
    }
}
