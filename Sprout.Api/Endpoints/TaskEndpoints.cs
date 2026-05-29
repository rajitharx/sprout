using Sprout.Api.Models;
using Sprout.Api.Services;

namespace Sprout.Api.Endpoints;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this WebApplication app)
    {
        app.MapGet("/api/tasks", async (ITaskService svc) =>
            Results.Ok(await svc.GetAllAsync()));

        app.MapPost("/api/tasks", async (HabitTask task, ITaskService svc) =>
        {
            var created = await svc.CreateAsync(task);
            return Results.Created($"/api/tasks/{created.Id}", created);
        });

        app.MapPut("/api/tasks/{id}", async (string id, HabitTask task, ITaskService svc) =>
        {
            var updated = await svc.UpdateAsync(id, task);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        app.MapDelete("/api/tasks/{id}", async (string id, ITaskService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}
