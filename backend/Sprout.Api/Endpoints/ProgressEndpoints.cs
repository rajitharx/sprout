using Sprout.Api.Models;
using Sprout.Api.Services;

namespace Sprout.Api.Endpoints;

public static class ProgressEndpoints
{
    public static void MapProgressEndpoints(this WebApplication app)
    {
        app.MapGet("/api/progress/today", async (IProgressService svc) =>
        {
            try
            {
                var today = await svc.GetTodayAsync();
                return Results.Ok(today);
            }
            catch
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        });

        app.MapPost("/api/progress/complete/{taskId}", async (string taskId, IProgressService svc) =>
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                return Results.BadRequest(ApiErrors.ValidationError("taskId", "Task ID is required."));
            }

            try
            {
                var result = await svc.MarkCompleteAsync(taskId);
                return Results.Ok(result);
            }
            catch
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        });

        app.MapPost("/api/progress/incomplete/{taskId}", async (string taskId, IProgressService svc) =>
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                return Results.BadRequest(ApiErrors.ValidationError("taskId", "Task ID is required."));
            }

            try
            {
                var result = await svc.MarkIncompleteAsync(taskId);
                return Results.Ok(result);
            }
            catch
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        });

        app.MapGet("/api/progress/week", async (IProgressService svc) =>
        {
            try
            {
                var week = await svc.GetWeekAsync();
                return Results.Ok(week);
            }
            catch
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        });
    }
}
