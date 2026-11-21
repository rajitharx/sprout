# Sprout — Claude Code Instructions

**Sprout**: gamified daily habit tracker for toddlers (3–4 yrs). Child completes tasks; parent manages them. Every decision must serve both users.

---

## Project Structure

```text
/Sprout
├── .claude/
│   ├── CLAUDE.md (this file)
│   └── skills/ (backend.md, frontend.md, storage.md, animations.md, toddler-ux.md)
├── .github/workflows/ci.yml (backend tests + frontend type-check/build)
├── backend/Sprout.Api/ (.NET 10 Minimal API + Services + Endpoints)
├── backend/Sprout.DataAccess/ (EF Core data access layer + PostgreSQL)
├── backend/Sprout.Api.Tests/ (Integration tests with in-memory EF Core)
├── frontend/sprout-web/ (React 18 + TypeScript + Vite + Tailwind v4)
└── run.sh (starts both servers)
```

---

## Two Users — Design for Both

| User | Needs |
| --- | --- |
| **Child (3–4 yrs)** | Giant tap targets, instant feedback, zero text |
| **Parent** | Quick task management, reliable data persistence |

**Key question for every UI decision:** Can a 3-year-old operate this without help?

---

## Non-Negotiable Constraints

1. **Child view never breaks** — always render (`/`, `View='child'`), even if API unreachable. Fallback to localStorage. Missing emoji? Show 📋. No progress data? Show all incomplete.

2. **Repository Pattern enforced** — all data via repositories (`ITaskRepository` / `IProgressRepository` / `IProfileRepository`) in the `Sprout.DataAccess` layer. Services use repositories; never bypass with direct database I/O or file access.

3. **CSS keyframes only** — animations in `sprout-web/src/index.css`. No Framer Motion, GSAP, or confetti packages. Five keyframes: `float`, `bounce`, `confettiFall`, `pulseGlow`, `ripple`.

4. **Tailwind v4 + TypeScript strict mode** — utilities only; no custom CSS plugins. `noUnusedLocals`/`noUnusedParameters` enforced. Remove unused vars, don't prefix with `_`.

5. **Backend changes require tests** — every endpoint/service/model change must include test cases in `backend/Sprout.Api.Tests/`. Tests must pass before review.

---

## Key Files by Task

| File | Read Before Editing |
| --- | --- |
| Program.cs | Service registration + endpoint mapping + DB migrations |
| Sprout.DataAccess/Context/SproutDbContext.cs | Database schema + entity configuration |
| Sprout.DataAccess/Repositories/ | ITaskRepository, IProgressRepository, IProfileRepository |
| Services/DbProgressService.cs | Week boundaries (Monday–Sunday); maps entities to domain models |
| Services/DbTaskService.cs | Task management via repository layer |
| Services/DbChildProfileService.cs | Child profile persistence |
| api/client.ts | Single source of truth for all API calls |
| App.tsx | View state + data fetching orchestration |
| useProfile.ts | Profile state + cache management |
| StreakBar.tsx | Week display (DAY_LABELS alignment) |
| index.css | All keyframe animations |

---

## Skill Files (Read Before Area-Specific Work)

- Backend → `backend.md` | Frontend → `frontend.md` | Storage → `storage.md` | Animations → `animations.md` | Child UI → `toddler-ux.md`

---

## Commands

```bash
cd backend/Sprout.Api && dotnet run                    # localhost:5000
cd frontend/sprout-web && npm run dev                  # localhost:5173
cd frontend/sprout-web && npm run build && cp -r dist/* ../../backend/Sprout.Api/wwwroot/
./run.sh                                                # both (dev)
```

---

## Do Not

- Use `localStorage` as primary store (fallback only)
- Add router library (single `useState` for view switching)
- Add authentication (parent unlocked via discreet icon)
- Use `position: fixed` in child view (use `100dvh` flex layout)
- Install animation packages or Tailwind plugins
- Rename `ITaskService`, `IProgressService`, `IProfileService`

---

## Decisions Log (keep short; append new ones)

- **Tap targets (2026-11-21):** child-view controls (carousel arrows, Done button) are ≥64px; parent-view controls ≥44px, including preset emoji buttons. No hover `title` tooltips on child-view buttons (touch users never see them).
- **Done button (2026-11-21):** one label with its emoji inside (`⭐ I did it!` / `✅ Done!`); no separate icon span.
- **Parent Add Task (2026-11-21):** the form is NOT a permanent footer. The footer is a single `+ Add Task` bar; tapping opens a bottom-sheet dialog (`absolute` inside the panel root, not `fixed`) so the task list keeps the screen. Sheet closes on success, ✕, backdrop tap or Escape; create errors render inside the sheet. See `frontend.md` → ParentPanel.
- **Workflow:** branch feature work from `main` (not `dev`, which may carry unrelated commits) and PR into `main`. Never push without being asked.
