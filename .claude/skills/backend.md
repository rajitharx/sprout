# Skill: Backend (.NET 10 Minimal API)

Read this before making any changes to `Sprout.Api/`.

---

## Architecture

```
HTTP Request
    ↓
Endpoints/  (TaskEndpoints.cs / ProgressEndpoints.cs)
    ↓
Services/   (ITaskService / IProgressService)
    ↓
JsonTaskService / JsonProgressService
    ↓
Storage/data/tasks.json + progress.json
```

Endpoints call services. Services call the JSON layer. Nothing else calls the JSON layer.

---

## Endpoints Convention

Register endpoints as extension methods in `Endpoints/`, never inline in `Program.cs`:

```csharp
// Endpoints/TaskEndpoints.cs
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
```

---

## Service Interface Rules

Never change the signatures of `ITaskService` or `IProgressService` without updating both the JSON implementation and any tests. The interfaces are the DB-migration contract.

```csharp
// IProgressService.cs — do not modify signatures
public interface IProgressService
{
    Task<DailyProgress> GetTodayAsync();
    Task<DailyProgress> MarkCompleteAsync(string taskId);
    Task<DailyProgress> MarkIncompleteAsync(string taskId);
    Task<List<DailyProgress>> GetWeekAsync();
}
```

---

## File I/O Safety

`JsonProgressService` and `JsonTaskService` both use a `SemaphoreSlim(1,1)`. Always follow this pattern when reading or writing:

```csharp
await _lock.WaitAsync();
try
{
    var json = await File.ReadAllTextAsync(_path);
    var data = JsonSerializer.Deserialize<List<T>>(json, _options) ?? [];
    // mutate data
    await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(data, _options));
}
finally
{
    _lock.Release();
}
```

Never `await` inside the `try` block without the semaphore being held. Never skip the `finally` release — it deadlocks the app.

---

## Date Handling

Progress records use ISO date strings (`"2025-06-01"`) as keys, not `DateTime` objects. Always derive today's key with:

```csharp
var today = DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd");
```

Use local time (not UTC) because this is a household app — the parent's device timezone is always correct.

---

## Adding a New Endpoint

1. Add the method signature to the relevant interface (`ITaskService` or `IProgressService`).
2. Implement it in the JSON service class.
3. Register the route in the relevant `Endpoints/` class.
4. Test manually with `curl` or the Vite dev server.

Never add a route without a corresponding interface method.

---

## CORS

CORS is configured in `Program.cs` for development only:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseCors(policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyMethod()
        .AllowAnyHeader());
}
```

Do not widen CORS in production. In production, the API serves the built React app as static files, so CORS is not needed.

---

## Error Responses

Use `Results.*` helpers — never throw unhandled exceptions:

| Situation | Return |
|---|---|
| Resource not found | `Results.NotFound()` |
| Invalid input | `Results.BadRequest("reason")` |
| Success with body | `Results.Ok(data)` |
| Created | `Results.Created(location, data)` |
| Success no body | `Results.NoContent()` |

---

## Seed Data

`JsonTaskService` seeds one default task on startup if `tasks.json` is empty or missing:

```csharp
if (!data.Any())
{
    data.Add(new HabitTask
    {
        Id = "default-1",
        Label = "Brush Teeth",
        Emoji = "🪥",
        SortOrder = 0,
        IsActive = true
    });
    await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(data, _options));
}
```

Do not remove this seed — it ensures the child view always has something to show on a fresh install.
