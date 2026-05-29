# Sprout — Claude Code Instructions

You are building and maintaining **Sprout**, a gamified daily habit tracker for toddlers (target user: 3–4 years old). The child completes tasks; the parent manages them. Every decision must serve those two distinct users.

---

## Project Layout

```text
/Sprout
├── .claude/                  ← you are here
│   ├── CLAUDE.md             ← this file (always read first)
│   └── skills/
│       ├── backend.md
│       ├── frontend.md
│       ├── storage.md
│       ├── animations.md
│       └── toddler-ux.md
│
├── .github/
│   └── workflows/
│       └── ci.yml            ← backend (build+test) + frontend (type-check+build)
│
├── backend/
│   ├── Sprout.Api/           ← .NET 10 Minimal API
│   │   ├── Program.cs
│   │   ├── Endpoints/
│   │   ├── Models/
│   │   ├── Services/
│   │   └── Storage/data/     ← tasks.json, progress.json, profile.json
│   └── Sprout.Api.Tests/
│
├── frontend/
│   └── sprout-web/           ← React 18 + TypeScript + Vite + Tailwind v4
│       ├── src/
│       │   ├── api/client.ts
│       │   ├── components/   ← TaskCarousel, TaskCard, DoneButton,
│       │   │                    CelebrationOverlay, StreakBar, ParentPanel
│       │   ├── hooks/        ← useTasks, useProgress, useProfile
│       │   └── types/
│       └── public/manifest.json
│
└── run.sh                    ← starts both servers (dev only)
```

---

## Two Users — Always Design for Both

| User | Context | Needs |
| --- | --- | --- |
| **Child (3–4 yrs)** | Holds tablet with both hands, can't read, short attention span | Giant tap targets, instant feedback, zero text dependency |
| **Parent** | Uses app briefly morning/evening | Quick task management, trust that data is saved |

When making any UI decision, ask: *"Can a 3-year-old operate this without help?"* If the answer is no, redesign it.

---

## Core Principles

### Never Break the Child View

The child-facing UI (`/` route, `View = 'child'`) must always render, even if:

- The API is unreachable (fall back to `localStorage` cache)
- A task has a missing emoji (show 📋 as fallback)
- Progress data fails to load (show all tasks as incomplete)

### Repository Pattern — Never Bypass It

All data access goes through `ITaskService` / `IProgressService` / `IProfileService`. Never read or write JSON files directly from endpoints or components. This makes the future DB migration a single new service class.

### No Animation Libraries

All animations are pure CSS keyframes defined in `sprout-web/src/index.css`. Do not install Framer Motion, GSAP, or similar. The five core keyframes (`float`, `bounce`, `confettiFall`, `pulseGlow`, `ripple`) cover all needs.

### Tailwind v4 Only

Use Tailwind utility classes. Do not write custom CSS except for the keyframe animations in `index.css`. Do not install Tailwind plugins.

### TypeScript Strict Mode

`tsconfig.json` enables `noUnusedLocals` and `noUnusedParameters`. Every import and parameter must be used or CI fails. Remove unused variables rather than prefixing with `_`.

### Test-Driven Backend Changes

**Every backend API code change must include relevant test cases.** When modifying or adding endpoints, services, or data models:

- Add or update tests in `backend/Sprout.Api.Tests/` to cover the new/changed behavior
- Tests must pass before code review — CI validates this
- Tests serve as contracts: document what the API guarantees, catch regressions, and enable safe refactors
- For service changes: test both happy path and error cases
- For endpoint changes: test request/response shapes, status codes, and validation

Test updates are not optional, even for small changes.

---

## Key Files — Read Before Editing

| File | Purpose |
| --- | --- |
| `backend/Sprout.Api/Program.cs` | Service registration + endpoint mapping |
| `backend/Sprout.Api/Services/JsonProgressService.cs` | File I/O with SemaphoreSlim — be careful with locking; week calculation starts from Monday |
| `backend/Sprout.Api/Services/JsonProfileService.cs` | Child profile persistence |
| `frontend/sprout-web/src/api/client.ts` | Single source of truth for all API calls |
| `frontend/sprout-web/src/App.tsx` | View state (`child` \| `parent`), data fetching orchestration |
| `frontend/sprout-web/src/hooks/useProfile.ts` | Child profile state and cache management |
| `frontend/sprout-web/src/components/StreakBar.tsx` | Weekly progress display (Monday–Sunday); directly maps DAY_LABELS to week data |
| `frontend/sprout-web/src/index.css` | All keyframe animations live here |

---

## Skill Files

Before working on any area, read the relevant skill file:

- **Backend changes** → read `.claude/skills/backend.md`
- **Frontend / component work** → read `.claude/skills/frontend.md`
- **Storage / data model changes** → read `.claude/skills/storage.md`
- **Any animation or celebration UI** → read `.claude/skills/animations.md`
- **Any child-facing UI** → read `.claude/skills/toddler-ux.md`

---

## Commands

```bash
# Backend
cd backend/Sprout.Api && dotnet run           # starts on http://localhost:5000

# Frontend
cd frontend/sprout-web && npm run dev          # starts on http://localhost:5173

# Build for production (outputs to backend/Sprout.Api/wwwroot)
cd frontend/sprout-web && npm run build && cp -r dist/* ../../backend/Sprout.Api/wwwroot/

# Run both (dev)
./run.sh
```

---

## What Not to Do

- Do not use `localStorage` as the primary store — it's a fallback cache only
- Do not add a router library — view switching is a single `useState`
- Do not add authentication — parent panel is unlocked via a discreet icon tap
- Do not use `position: fixed` inside the child view — use `100dvh` flex layout
- Do not install confetti or animation npm packages
- Do not rename `ITaskService`, `IProgressService`, or `IProfileService` — other code depends on these interfaces
