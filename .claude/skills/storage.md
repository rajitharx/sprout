# Skill: Storage (JSON Flat-File Layer)

Read this before touching any data model, service, or the `Storage/data/` folder.

---

## File Locations

```
Sprout.Api/
└── Storage/
    └── data/
        ├── tasks.json      ← array of HabitTask
        ├── progress.json   ← array of DailyProgress (one per calendar day)
        └── profile.json    ← single ChildProfile object (name + avatar emoji)
```

The path is configurable via `appsettings.json`:

```json
{
  "Storage": {
    "DataPath": "Storage/data"
  }
}
```

Both files are created automatically on first run if they do not exist.

---

## Data Shapes

### tasks.json

```json
[
  {
    "id": "default-1",
    "label": "Brush Teeth",
    "emoji": "🪥",
    "sortOrder": 0,
    "isActive": true
  },
  {
    "id": "a3f9c2b1-...",
    "label": "Put Shoes On",
    "emoji": "👟",
    "sortOrder": 1,
    "isActive": true
  }
]
```

### progress.json

```json
[
  {
    "date": "2025-06-01",
    "completedTaskIds": ["default-1"],
    "lastUpdated": "2025-06-01T07:34:12Z"
  },
  {
    "date": "2025-06-02",
    "completedTaskIds": ["default-1", "a3f9c2b1-..."],
    "lastUpdated": "2025-06-02T08:01:55Z"
  }
]
```

One object per calendar day. If a day has no completions, no entry is needed (treat missing = zero completions).

---

## Soft Delete

Tasks are never hard-deleted from `tasks.json`. Deleting a task sets `isActive: false`.

**Stale ID implication:** Because `completedTaskIds` in `progress.json` stores raw IDs forever, a soft-deleted task's ID can remain in historical progress records. When checking whether all tasks are complete, you must filter `completedTaskIds` against only the currently active task IDs — not just compare counts. See `useProgress.markComplete` which takes `taskIds: string[]` for this reason.

```csharp
// JsonTaskService.DeleteAsync
var task = data.FirstOrDefault(t => t.Id == id);
if (task is null) return false;
var index = data.IndexOf(task);
data[index] = task with { IsActive = false };
```

`GetAllAsync` filters to only `IsActive == true` tasks. This preserves historical progress references.

---

## Concurrency Safety

Both services use a `static readonly SemaphoreSlim` (one per file) to prevent race conditions when two requests arrive simultaneously:

```csharp
private static readonly SemaphoreSlim _lock = new(1, 1);

public async Task<...> SomeWriteAsync(...)
{
    await _lock.WaitAsync();
    try
    {
        // read → mutate → write
    }
    finally
    {
        _lock.Release(); // ALWAYS release, even on exception
    }
}
```

Read-only operations (`GetAllAsync`, `GetTodayAsync`) do not require the lock.

---

## Migrating to a Real Database

When the time comes to move from JSON to a proper DB (SQLite, Postgres, etc.):

1. Create `SqliteTaskService : ITaskService` and `SqliteProgressService : IProgressService`.
2. Register the new implementations in `Program.cs` (replace `JsonTaskService`/`JsonProgressService`).
3. No changes to endpoints, hooks, or components.

The JSON files can be used to seed the new database as a one-time migration script.

---

## Modifying the Data Model

### Adding a field to `HabitTask`

1. Add the property to `Models/HabitTask.cs` with a default value.
2. Existing JSON records without the field will deserialize with the default (System.Text.Json handles missing properties gracefully).
3. Update `ITaskService` if the new field needs to be set/returned differently.
4. Update `types/index.ts` in the frontend to match.

### Adding a field to `DailyProgress`

Same process. Be careful: `progress.json` may have thousands of records over time — always ensure the new field has a sensible default that works for all historical entries.

---

## Serialization Settings

Both services share the same options:

```csharp
private static readonly JsonSerializerOptions _options = new()
{
    WriteIndented = true,
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};
```

`PropertyNameCaseInsensitive = true` ensures the frontend's camelCase JSON round-trips correctly with C#'s PascalCase properties.

---

## Storage Checklist

Before shipping any storage change:
- [ ] New fields have default values compatible with existing JSON files
- [ ] Write operations are inside the semaphore lock
- [ ] File is created if missing (don't assume it exists)
- [ ] `tasks.json` shape matches `HabitTask` C# record
- [ ] `progress.json` shape matches `DailyProgress` C# record
- [ ] Frontend `types/index.ts` updated to match any model change
