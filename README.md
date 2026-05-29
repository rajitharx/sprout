# Sprout

A gamified daily habit tracker for toddlers (ages 3–4). The child taps through emoji-based task cards to complete their morning/evening routine; the parent manages the task list from a discreet panel.

---

## Features

**Child view**
- Customizable child profile with name and avatar emoji
- Swipeable task carousel — one big card per habit, no reading required
- Emoji-first design with giant tap targets
- Celebration overlay when all tasks are complete
- Streak bar showing the current week (Monday–Sunday)
- Offline-capable — falls back to `localStorage` cache if the API is unreachable

**Parent view**
- Unlocked via a discreet tap (no password, no auth complexity)
- Edit child profile (name and avatar emoji)
- Create, edit, reorder, and deactivate tasks
- Each task has a label, emoji, and sort order

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend API | .NET 10 — Minimal API (C#) |
| Storage | JSON flat files (upgrade path: swap service, keep interfaces) |
| Frontend | React 18 + TypeScript + Vite |
| Styling | Tailwind CSS v4 |
| Animations | Pure CSS keyframes — no animation libraries |
| Tests | xUnit + `WebApplicationFactory` integration tests |

---

## Project Structure

```
sprout/
├── backend/
│   ├── Sprout.Api/               .NET 10 Minimal API
│   │   ├── Endpoints/            TaskEndpoints.cs, ProgressEndpoints.cs, ProfileEndpoints.cs
│   │   ├── Models/               HabitTask.cs, DailyProgress.cs, ChildProfile.cs
│   │   ├── Services/             ITaskService, IProgressService, IProfileService + JSON implementations
│   │   └── Storage/data/         tasks.json, progress.json, profile.json (runtime data, gitignored)
│   └── Sprout.Api.Tests/         Integration tests
│
├── frontend/
│   └── sprout-web/               React + TypeScript + Vite
│       └── src/
│           ├── api/client.ts     All API calls — single source of truth
│           ├── components/       TaskCard, TaskCarousel, ParentPanel, CelebrationOverlay, StreakBar, DoneButton
│           ├── hooks/            useTasks.ts, useProgress.ts, useProfile.ts
│           └── types/            Shared TypeScript types
│
└── Sprout.slnx                   .NET solution file
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org)

### Run in development

**Terminal 1 — API** (http://localhost:5000)

```bash
cd backend/Sprout.Api
dotnet run
```

**Terminal 2 — Frontend** (http://localhost:5173)

```bash
cd frontend/sprout-web
npm install
npm run dev
```

The Vite dev server proxies API calls to `localhost:5000` automatically via the CORS allowlist in `Program.cs`.

### Production build

```bash
cd frontend/sprout-web
npm run build
cp -r dist/* ../../backend/Sprout.Api/wwwroot/
cd ../../backend/Sprout.Api
dotnet run
```

The API serves the built frontend as static files and falls back to `index.html` for all non-API routes.

---

## API Reference

### Tasks

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/tasks` | List all tasks |
| `POST` | `/api/tasks` | Create a task |
| `PUT` | `/api/tasks/{id}` | Update a task |
| `DELETE` | `/api/tasks/{id}` | Delete a task |

**Task object**

```json
{
  "id": "uuid",
  "label": "Brush teeth",
  "emoji": "🪥",
  "sortOrder": 0,
  "isActive": true
}
```

### Progress

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/progress/today` | Today's completed task IDs |
| `POST` | `/api/progress/complete/{taskId}` | Mark a task complete |
| `POST` | `/api/progress/incomplete/{taskId}` | Unmark a task |
| `GET` | `/api/progress/week` | Current week's progress (Monday–Sunday) |

**Progress object**

```json
{
  "date": "2026-05-29",
  "completedTaskIds": ["uuid-1", "uuid-2"],
  "lastUpdated": "2026-05-29T07:30:00Z"
}
```

### Child Profile

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/profile` | Get the child's profile |
| `PUT` | `/api/profile` | Update the child's profile |

**Child Profile object**

```json
{
  "name": "Child",
  "avatar": "👦"
}
```

---

## Configuration

The storage path is configurable via `appsettings.json` or an environment variable:

```json
{
  "Storage": {
    "DataPath": "Storage/data"
  }
}
```

Override for a persistent volume in production:

```bash
Storage__DataPath=/var/sprout-data dotnet run
```

---

## Running Tests

```bash
cd backend/Sprout.Api.Tests
dotnet test
```

Tests use `WebApplicationFactory` to run the full API in-process against real JSON files written to a temp directory — no mocks for the storage layer.

---

## Architecture Notes

**Repository pattern** — all data access goes through `ITaskService`, `IProgressService`, and `IProfileService`. The current implementations write JSON files; swapping to SQLite or a cloud DB is a single new class per interface with no changes to endpoints or frontend.

**Concurrent writes** — `JsonProgressService` uses a `SemaphoreSlim(1,1)` to serialise all writes, so rapid taps from the child won't corrupt the progress file.

**Week boundaries** — the streak bar displays the current week starting from Monday and ending on Sunday, ensuring consistent week boundaries across app restarts.

**Child profile** — stored persistently via `IProfileService`, allowing parents to customize the child's name and avatar emoji.

**No router** — view switching between child and parent is a single `useState` in `App.tsx`. No router library needed.

**Offline fallback** — `useTasks`, `useProgress`, and `useProfile` write to `localStorage` on every successful fetch. If the API is unreachable on load, the cached data is used so the child view always renders.
