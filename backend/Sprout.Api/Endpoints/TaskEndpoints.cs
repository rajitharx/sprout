using Sprout.Api.Models;
using Sprout.Api.Services;

namespace Sprout.Api.Endpoints;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this WebApplication app)
    {
        app.MapGet("/api/tasks", async (ITaskService svc) =>
        {
            try
            {
                var tasks = await svc.GetAllAsync();
                return Results.Ok(tasks);
            }
            catch
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        });

        app.MapPost("/api/tasks", async (HabitTask task, ITaskService svc) =>
        {
            var errors = TaskValidator.Validate(task);
            if (errors is not null)
            {
                return Results.BadRequest(ApiErrors.ValidationError(errors));
            }

            try
            {
                var created = await svc.CreateAsync(task);
                return Results.Created($"/api/tasks/{created.Id}", created);
            }
            catch
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        });

        app.MapPut("/api/tasks/{id}", async (string id, HabitTask task, ITaskService svc) =>
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Results.BadRequest(ApiErrors.ValidationError("id", "Task ID is required."));
            }

            var errors = TaskValidator.Validate(task);
            if (errors is not null)
            {
                return Results.BadRequest(ApiErrors.ValidationError(errors));
            }

            try
            {
                var updated = await svc.UpdateAsync(id, task);
                return updated is null ? Results.NotFound(ApiErrors.NotFound("Task")) : Results.Ok(updated);
            }
            catch
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        });

        app.MapDelete("/api/tasks/{id}", async (string id, ITaskService svc) =>
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Results.BadRequest(ApiErrors.ValidationError("id", "Task ID is required."));
            }

            try
            {
                var deleted = await svc.DeleteAsync(id);
                return deleted ? Results.NoContent() : Results.NotFound(ApiErrors.NotFound("Task"));
            }
            catch
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        });
    }
}
