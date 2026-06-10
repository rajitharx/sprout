# Sprout

A gamified daily habit tracker for toddlers (ages 3–4). The child taps through emoji-based task cards to complete their morning/evening routine; the parent manages the task list from a discreet panel.

---

## Features

**Child view**
- Welcome overlay on every load — greets the child by name with their avatar
- Customizable child profile with name and avatar emoji
- Swipeable task carousel — one big card per habit, no reading required
- Emoji-first design with giant tap targets, colour-coded gradient cards
- Star collection animation — pressing "I did it!" sends star particles flying up to the profile avatar, which bounces on arrival
- Per-task celebration with task emoji — celebrates each completion with task-specific messaging
- All-done celebration overlay when all tasks are complete with trophy animation
- Completed tasks turn green to match the done button; progress bar and counter update in real time
- Streak bar showing the current week (Monday–Sunday)
- Offline-capable — falls back to `localStorage` cache if the API is unreachable
- Respects `prefers-reduced-motion` — all animations disabled when the system setting is on

**Parent view**
- Unlocked via a discreet tap (no password, no auth complexity)
- Edit child profile (name and avatar emoji)
- Create, edit, reorder, and deactivate tasks
- Each task has a label, emoji, and sort order
- Inline delete confirmation to prevent accidental removal
- All form inputs have visible labels for accessibility

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

### PWA Installation & Icons

Sprout is a Progressive Web App (PWA) installable on iOS and Android home screens.

**To enable PWA installation:**

1. Generate app icons (192x192 and 512x512 PNG):
   - Place in `frontend/sprout-web/public/icon-192.png` and `icon-512.png`
   - Optionally create maskable versions: `icon-maskable-192.png` and `icon-maskable-512.png`
   - Example: Use a warm gradient (coral to orange) with a 🌱 seedling emoji

2. Icons are referenced in `manifest.json` (already configured)

3. Serve the app over HTTPS in production (required for PWA installation)

4. Users can now "Add to Home Screen" on mobile or desktop browsers

### Docker deployment

A single Docker container bundles both frontend and backend for production deployment.

**Build & run locally:**

```bash
docker compose up -d
```

**Deploy to Linux server:**

1. Copy the project to your server:

```bash
rsync -av --exclude=node_modules --exclude=bin --exclude=obj --exclude=.git \
  /path/to/sprout/ user@server:/home/user/sprout/
```

2. On the server:

```bash
cd /home/user/sprout
docker compose up -d
```

The app runs on `http://server:5000` with persistent storage in the `sprout-storage` Docker volume.

**Production deployment checklist:**

- Use a reverse proxy (nginx, Caddy) to serve HTTPS (required for PWA)
- Set `ASPNETCORE_URLS=http://127.0.0.1:5000` to bind to localhost only
- Mount a persistent volume for `Storage__DataPath=/var/sprout-data`
- Configure parent PIN via `ParentAuth__DefaultPin` environment variable
- Enable health checks to monitor the container

**Example Docker run with HTTPS (behind Caddy):**

```bash
docker run -d \
  --name sprout \
  -p 5000:5000 \
  -e ASPNETCORE_URLS=http://127.0.0.1:5000 \
  -e Storage__DataPath=/var/sprout-data \
  -e ParentAuth__DefaultPin=1234 \
  -v sprout-data:/var/sprout-data \
  sprout:latest
```

Then proxy `https://yourdomain.com` → `http://localhost:5000` via Caddy or nginx.

**Deployment files:**

- `Dockerfile` — Multi-stage build (frontend Node → backend .NET)
- `docker-compose.yml` — Container orchestration + volume management
- `.dockerignore` — Build optimization
- `deployment/` — Helper scripts and documentation

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

## Logging & Debugging

The API includes configurable logging for debugging and monitoring. Logging is controlled via `appsettings.json` or environment variables.

### Debug Configuration Options

```json
{
  "Debug": {
    "Enabled": false,
    "LogRequests": false,
    "LogServiceCalls": false,
    "LogExceptions": true
  }
}
```

| Option | Purpose | Default |
|---|---|---|
| `Debug.Enabled` | Master switch to enable debug mode | `false` |
| `Debug.LogRequests` | Log all incoming HTTP requests with response times | `false` |
| `Debug.LogServiceCalls` | Log service-layer operations (task, progress, profile) | `false` |
| `Debug.LogExceptions` | Log unhandled exceptions with full error details | `true` |

### Enable Debug Logging

**Via environment variable (development):**

```bash
cd backend/Sprout.Api
Debug__Enabled=true Debug__LogRequests=true dotnet run
```

**Via appsettings.Development.json (persistent):**

Edit `appsettings.Development.json`:

```json
{
  "Debug": {
    "Enabled": true,
    "LogRequests": true,
    "LogServiceCalls": false,
    "LogExceptions": true
  }
}
```

### Middleware

- **ExceptionHandlingMiddleware** — Catches unhandled exceptions globally and returns a 500 error. Includes error details in the response if `LogExceptions` is enabled.
- **RequestLoggingMiddleware** — Logs request method, path, response status, and elapsed time. Only active if `LogRequests` is enabled.

---

## Running Tests

```bash
cd backend/Sprout.Api.Tests
dotnet test
```

Tests use `WebApplicationFactory` to run the full API in-process against real JSON files written to a temp directory — no mocks for the storage layer.

**Test coverage** — 73 tests including:
- Task CRUD operations and edge cases
- Progress tracking and concurrent mark-complete
- Week calculation edge cases (Sunday→Monday boundary)
- Profile management and validation
- PIN authentication
- Error handling (validation, not found, timeouts)
- Offline fallback behavior

Run with verbose output:

```bash
dotnet test --verbosity=normal
```

---

## Error Handling & Edge Cases

The app is built to handle real-world issues gracefully:

### **Backend Resilience**
- **Input validation** — All endpoints validate request bodies (label/emoji length, required fields, valid sort order)
- **Detailed error responses** — 400 Bad Request returns validation errors with field names; 404 for missing resources; 500 for server errors
- **Concurrent safety** — `SemaphoreSlim` locking prevents data corruption on rapid writes
- **Graceful degradation** — Unhandled exceptions return 500 with sanitized messages (no stack traces exposed)

### **Frontend Resilience**
- **Network timeouts** — 30-second timeout with clear user message: "Request timeout — check your connection"
- **Duplicate prevention** — Marking same task concurrent is blocked; buttons disable during API calls
- **Offline support** — Child view never breaks; falls back to `localStorage` cache if API unreachable
- **Offline indicator** — Banner shows "📡 Offline mode — changes will sync when online"
- **Error recovery** — Form errors dismissed by user; state rolls back on API failure
- **Optimistic updates** — UI updates immediately; state rolls back if save fails

### **Edge Cases Covered**
✅ Empty task list → "No tasks yet" prompt  
✅ API unreachable → Uses localStorage cache  
✅ Network lag → Buttons disabled during request  
✅ Concurrent mark-complete → Prevented with ref-based tracking  
✅ Delete during marking → Safe (ID-based operations)  
✅ All tasks complete → Celebration triggers correctly  
✅ Invalid input (empty name/emoji) → Caught at client and server  
✅ Stale profile/task data → Synced on app load  
✅ Large task lists → Handled efficiently  

---

## Architecture Notes

**Repository pattern** — all data access goes through `ITaskService`, `IProgressService`, and `IProfileService`. The current implementations write JSON files; swapping to SQLite or a cloud DB is a single new class per interface with no changes to endpoints or frontend.

**Concurrent writes** — `JsonProgressService` uses a `SemaphoreSlim(1,1)` to serialise all writes, so rapid taps from the child won't corrupt the progress file.

**Week boundaries** — the streak bar displays the current week starting from Monday and ending on Sunday, ensuring consistent week boundaries across app restarts.

**Child profile** — stored persistently via `IProfileService`, allowing parents to customize the child's name and avatar emoji.

**No router** — view switching between child and parent is a single `useState` in `App.tsx`. No router library needed.

**Animations are pure CSS** — all keyframes live in `index.css` (`float`, `bounce`, `confettiFall`, `pulseGlow`, `ripple`, `starFly`, `avatarPop`). DOM-spawned particles (stars, confetti) are created imperatively and removed after their animation completes. No Framer Motion, GSAP, or confetti packages.

**Per-task & all-complete celebrations** — `useProgress.markComplete` receives the full list of active task IDs and triggers a celebration callback with the task emoji and a flag indicating if all tasks are done. The celebration shows the task emoji for individual completions, then the trophy emoji for the final "All Done!" overlay.

**Offline fallback** — `useTasks`, `useProgress`, and `useProfile` write to `localStorage` on every successful fetch. If the API is unreachable on load, the cached data is used so the child view always renders.
