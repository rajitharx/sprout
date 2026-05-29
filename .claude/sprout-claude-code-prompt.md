# Sprout — Claude Code Build Prompt

## Project Overview

Build **Sprout**, a gamified daily habit tracker for toddlers (target user: 3.5 years old). The app must be visually engaging, purely icon/emoji driven (no reading required), and deliver a satisfying reward experience when a task is completed. The parent manages tasks; the child interacts with the completion UI.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 10 Minimal API (C#) |
| Frontend | React 18 + TypeScript, Vite |
| Styling | Tailwind CSS v4 |
| Storage | JSON flat-file (local filesystem via .NET) — DB-ready abstraction layer |
| Communication | REST (fetch) |
| Deployment target | Single machine / localhost, PWA-ready |

---

## Solution Structure

```
/Sprout
├── Sprout.Api/              # .NET 10 Minimal API
│   ├── Program.cs
│   ├── Endpoints/
│   │   ├── TaskEndpoints.cs
│   │   └── ProgressEndpoints.cs
│   ├── Models/
│   │   ├── HabitTask.cs
│   │   └── DailyProgress.cs
│   ├── Services/
│   │   ├── ITaskService.cs
│   │   ├── IProgressService.cs
│   │   ├── JsonTaskService.cs
│   │   └── JsonProgressService.cs
│   ├── Storage/
│   │   └── data/               # JSON files live here
│   │       ├── tasks.json
│   │       └── progress.json
│   └── Sprout.Api.csproj
│
└── sprout-web/              # React + Vite frontend
    ├── src/
    │   ├── main.tsx
    │   ├── App.tsx
    │   ├── api/
    │   │   └── client.ts       # All fetch calls
    │   ├── components/
    │   │   ├── TaskCard.tsx
    │   │   ├── DoneButton.tsx
    │   │   ├── CelebrationOverlay.tsx
    │   │   ├── StreakBar.tsx
    │   │   ├── TaskCarousel.tsx
    │   │   └── ParentPanel.tsx
    │   ├── hooks/
    │   │   ├── useTasks.ts
    │   │   └── useProgress.ts
    │   └── types/
    │       └── index.ts
    ├── public/
    │   └── manifest.json       # PWA manifest
    ├── index.html
    ├── vite.config.ts
    └── package.json
```

---

## Backend — .NET 10 Minimal API

### Data Models

```csharp
// Models/HabitTask.cs
public record HabitTask
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Label { get; init; } = "";       // e.g. "Brush Teeth"
    public string Emoji { get; init; } = "🪥";    // displayed large on screen
    public int SortOrder { get; init; } = 0;
    public bool IsActive { get; init; } = true;
}

// Models/DailyProgress.cs
public record DailyProgress
{
    public string Date { get; init; } = "";        // ISO date: "2025-06-01"
    public List<string> CompletedTaskIds { get; init; } = [];
    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;
}
```

### Storage Layer (JSON flat-file)

Implement a repository pattern behind interfaces so swapping to a real DB later requires only a new implementation, not changes to endpoints or business logic.

```csharp
// Services/ITaskService.cs
public interface ITaskService
{
    Task<List<HabitTask>> GetAllAsync();
    Task<HabitTask?> GetByIdAsync(string id);
    Task<HabitTask> CreateAsync(HabitTask task);
    Task<HabitTask?> UpdateAsync(string id, HabitTask task);
    Task<bool> DeleteAsync(string id);
}

// Services/IProgressService.cs
public interface IProgressService
{
    Task<DailyProgress> GetTodayAsync();
    Task<DailyProgress> MarkCompleteAsync(string taskId);
    Task<DailyProgress> MarkIncompleteAsync(string taskId);
    Task<List<DailyProgress>> GetWeekAsync();   // last 7 days
}
```

`JsonTaskService` reads/writes `Storage/data/tasks.json`.
`JsonProgressService` reads/writes `Storage/data/progress.json` as an array of `DailyProgress` objects (one entry per calendar day). Both services must:
- Create the JSON files with empty arrays on first run if they do not exist.
- Use `System.Text.Json` with `JsonSerializerOptions { WriteIndented = true }`.
- Wrap all file I/O in a `SemaphoreSlim(1,1)` to prevent concurrent write corruption.

### API Endpoints

Register all endpoints in `Program.cs` via extension methods in `Endpoints/`.

```
GET    /api/tasks                  → List<HabitTask>  (active tasks ordered by SortOrder)
POST   /api/tasks                  → HabitTask        (create)
PUT    /api/tasks/{id}             → HabitTask        (update label/emoji/order/active)
DELETE /api/tasks/{id}             → 204              (soft-delete: set IsActive=false)

GET    /api/progress/today         → DailyProgress
POST   /api/progress/complete/{taskId}   → DailyProgress
POST   /api/progress/incomplete/{taskId} → DailyProgress
GET    /api/progress/week          → List<DailyProgress>  (last 7 calendar days)
```

### CORS & Configuration

- Enable CORS to allow `http://localhost:5173` (Vite dev server) in development.
- In production, serve the built React app as static files from `wwwroot/` using `app.UseStaticFiles()` and `app.MapFallbackToFile("index.html")`.
- Configure the storage path via `appsettings.json`:

```json
{
  "Storage": {
    "DataPath": "Storage/data"
  }
}
```

- Bind on `http://localhost:5000` in development.

---

## Frontend — React + TypeScript + Tailwind

### Shared Types

```typescript
// src/types/index.ts
export interface HabitTask {
  id: string;
  label: string;
  emoji: string;
  sortOrder: number;
  isActive: boolean;
}

export interface DailyProgress {
  date: string;
  completedTaskIds: string[];
  lastUpdated: string;
}
```

### API Client

```typescript
// src/api/client.ts
const BASE = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

export const api = {
  getTasks: (): Promise<HabitTask[]> => ...,
  getToday: (): Promise<DailyProgress> => ...,
  getWeek: (): Promise<DailyProgress[]> => ...,
  markComplete: (taskId: string): Promise<DailyProgress> => ...,
  markIncomplete: (taskId: string): Promise<DailyProgress> => ...,
  createTask: (task: Partial<HabitTask>): Promise<HabitTask> => ...,
  updateTask: (id: string, task: Partial<HabitTask>): Promise<HabitTask> => ...,
  deleteTask: (id: string): Promise<void> => ...,
};
```

### App Shell & Routing

No router library needed. Use a single `view` state in `App.tsx`:

```typescript
type View = 'child' | 'parent';
```

- **Child view** (default): full-screen, touch-friendly task carousel.
- **Parent view**: management panel accessible via a small discreet settings icon (⚙️) in the top-right corner. No password needed for MVP.

---

## Child View — UI Requirements

### Layout

Full-viewport, mobile-first. No scrolling. Everything fits within 100dvh.

```
┌─────────────────────────────┐
│  [streak bar — 7 dots]      │  ← top, always visible
│                             │
│   ← [task card] →           │  ← center, swipeable carousel
│                             │
│      [DONE button]          │  ← bottom third, very large
└─────────────────────────────┘
```

### TaskCard Component

- Soft pastel gradient background per card (cycle through a fixed palette of 6 warm gradients based on task index).
- Emoji rendered at `text-9xl` (96px+), centered, with a gentle floating CSS animation (`@keyframes float`).
- Task label displayed below emoji at `text-2xl font-black`, but keep it subtle — the emoji is the star.
- Card fills 85% of viewport width, rounded corners (`rounded-3xl`), soft drop shadow.

### TaskCarousel Component

- Swipeable horizontally using pointer events (implement lightweight swipe detection — no library).
- Show dot indicators below the done button (one dot per task, active dot highlighted).
- Show left/right chevron arrows on the card edges for non-touch devices.
- Animate card transition with a smooth slide (`transform: translateX`, CSS transition 300ms ease).

### DoneButton Component

- Occupies the full bottom section; minimum tap target `96px` tall, `80%` wide, centered.
- Default state: bright gradient button (coral-to-orange), white bold label "✅ I Did It!", large text (`text-2xl font-black`).
- Completed state: green gradient, "⭐ Done!", disabled but still visually visible.
- Press animation: scale down 4% on `:active`, spring back.
- On press: emit a ripple effect from the tap point.

### CelebrationOverlay Component

Triggered when the current task is marked complete. Covers the full screen.

- Warm golden/yellow full-screen background with animated gradient shift.
- Large bouncing trophy emoji (🏆) at center.
- Congratulations text: "Amazing!!" (large, bold, white).
- Sub-text specific to the task: e.g. "You brushed your teeth! 🦷✨"
- Animated confetti: spawn 60 `div` elements with randomized colors, positions, rotation, and `fall` keyframe animation. Remove elements after animation completes.
- "⭐ Yay!" button to dismiss and return to task card.
- Auto-dismiss after 6 seconds if not tapped.
- All tasks complete state: show a special "All Done! 🎉" variant with extra confetti.

### StreakBar Component

- 7 dots representing the current calendar week (Mon–Sun).
- Completed days: filled gold star ⭐, pulsing glow animation.
- Today (not yet complete): pulsing empty circle.
- Past days (incomplete): muted gray dot.
- Future days: faint outline only.
- Derive state from the `week` progress array returned by `/api/progress/week`.

---

## Parent View — Management Panel

Slide-up panel (or full-screen on mobile). Access via ⚙️ icon.

### Features

1. **Task list** — see all active tasks with emoji and label.
2. **Add task** — emoji picker (grid of 20 common kid-friendly emojis) + text label input + save button.
3. **Edit task** — tap any task to edit its emoji or label inline.
4. **Reorder tasks** — drag handle (≡) to change carousel order.
5. **Delete task** — swipe-to-delete or trash icon with a confirmation tap.
6. **Today's progress summary** — read-only view showing which tasks are done today with checkmarks.
7. **Reset today** — button to un-mark all tasks for today (for testing/mistakes).

### Emoji Picker

Preset grid of at least 20 emojis covering common toddler routines:
`🪥 🛁 🧼 👕 👟 🎒 🍽️ 🥛 🛌 📚 🧸 🚽 🧹 🌿 💊 🥦 🧃 🏃 🎨 🤝`

---

## Animations & Micro-interactions

Implement all animations with pure CSS keyframes — no animation libraries.

```css
@keyframes float {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-14px); }
}

@keyframes bounce {
  0%, 100% { transform: translateY(0) rotate(-4deg); }
  50% { transform: translateY(-20px) rotate(4deg); }
}

@keyframes confettiFall {
  0%   { transform: translate(0, 0) rotate(0deg); opacity: 1; }
  100% { transform: translate(var(--tx), 110vh) rotate(var(--rot)); opacity: 0; }
}

@keyframes pulseGlow {
  0%, 100% { box-shadow: 0 0 0 0 rgba(255, 213, 79, 0.6); }
  50%       { box-shadow: 0 0 0 8px rgba(255, 213, 79, 0); }
}

@keyframes ripple {
  0%   { transform: scale(0); opacity: 0.5; }
  100% { transform: scale(6); opacity: 0; }
}
```

---

## PWA Setup

Add `public/manifest.json`:

```json
{
  "name": "Sprout",
  "short_name": "Sprout",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#FFF8E7",
  "theme_color": "#FF6B9D",
  "icons": [
    { "src": "/icon-192.png", "sizes": "192x192", "type": "image/png" },
    { "src": "/icon-512.png", "sizes": "512x512", "type": "image/png" }
  ]
}
```

Add the manifest link and `<meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1">` to `index.html`. Generate placeholder PNG icons (solid coral circle with 🪥 emoji).

---

## Seed Data

On first run (if `tasks.json` is empty), the backend should seed one default task:

```json
[
  {
    "id": "default-1",
    "label": "Brush Teeth",
    "emoji": "🪥",
    "sortOrder": 0,
    "isActive": true
  }
]
```

---

## Error Handling

- API calls that fail should surface a toast notification (bottom-center, 3s auto-dismiss) — never break the UI.
- If the API is unreachable, the child view should still render (cached last-known tasks from `localStorage`).
- Use a simple `useProgress` hook that polls `/api/progress/today` every 60 seconds to stay in sync (useful if multiple devices are used).

---

## Development Setup Instructions

Generate a `README.md` with exact commands to:

1. Run the backend: `cd Sprout.Api && dotnet run`
2. Run the frontend: `cd sprout-web && npm install && npm run dev`
3. Build for production and copy the `dist/` output into `Sprout.Api/wwwroot/`
4. Run everything from a single command using a `Makefile` or `run.sh`

---

## Quality Checklist

Before finishing, verify:

- [ ] Tapping Done on every task in the carousel triggers celebration and marks it complete via the API.
- [ ] Refreshing the page restores completed state (data comes from backend).
- [ ] The streak bar reflects the correct days based on `progress.json` data.
- [ ] Adding/editing/deleting tasks in the parent panel is reflected immediately in the child carousel.
- [ ] On a real mobile device (or Chrome DevTools mobile emulation), no element is cut off, no scrolling is required in child view, and tap targets are at minimum 64px.
- [ ] `tasks.json` and `progress.json` are created automatically on first run.
- [ ] CORS is configured so the Vite dev server can reach the API without errors.
- [ ] The app works offline for the child view (reads from `localStorage` cache) if the backend is down.
