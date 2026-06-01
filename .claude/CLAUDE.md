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
├── backend/Sprout.Api/ (.NET 10 Minimal API + Services + Storage/data/)
├── backend/Sprout.Api.Tests/
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

2. **Repository Pattern enforced** — all data via `ITaskService` / `IProgressService` / `IProfileService`. Never bypass with direct file I/O.

3. **CSS keyframes only** — animations in `sprout-web/src/index.css`. No Framer Motion, GSAP, or confetti packages. Five keyframes: `float`, `bounce`, `confettiFall`, `pulseGlow`, `ripple`.

4. **Tailwind v4 + TypeScript strict mode** — utilities only; no custom CSS plugins. `noUnusedLocals`/`noUnusedParameters` enforced. Remove unused vars, don't prefix with `_`.

5. **Backend changes require tests** — every endpoint/service/model change must include test cases in `backend/Sprout.Api.Tests/`. Tests must pass before review.

---

## Key Files by Task

| File | Read Before Editing |
| --- | --- |
| Program.cs | Service registration + endpoint mapping |
| JsonProgressService.cs | SemaphoreSlim locking; week = Monday–Sunday |
| JsonProfileService.cs | Child profile persistence |
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
