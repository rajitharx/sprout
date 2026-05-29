# Sprout — Claude Code Instructions

You are building and maintaining **Sprout**, a gamified daily habit tracker for toddlers (target user: 3–4 years old). The child completes tasks; the parent manages them. Every decision must serve those two distinct users.

---

## Project Layout

```
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
├── Sprout.Api/               ← .NET 10 Minimal API
│   ├── Program.cs
│   ├── Endpoints/
│   ├── Models/
│   ├── Services/
│   └── Storage/data/         ← tasks.json, progress.json
│
└── sprout-web/               ← React 18 + TypeScript + Vite + Tailwind v4
    ├── src/
    │   ├── api/client.ts
    │   ├── components/
    │   ├── hooks/
    │   └── types/
    └── public/manifest.json
```

---

## Two Users — Always Design for Both

| User | Context | Needs |
|---|---|---|
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
All data access goes through `ITaskService` / `IProgressService`. Never read or write JSON files directly from endpoints or components. This makes the future DB migration a single new service class.

### No Animation Libraries
All animations are pure CSS keyframes defined in `sprout-web/src/index.css`. Do not install Framer Motion, GSAP, or similar. The five core keyframes (`float`, `bounce`, `confettiFall`, `pulseGlow`, `ripple`) cover all needs.

### Tailwind v4 Only
Use Tailwind utility classes. Do not write custom CSS except for the keyframe animations in `index.css`. Do not install Tailwind plugins.

---

## Key Files — Read Before Editing

| File | Purpose |
|---|---|
| `Sprout.Api/Program.cs` | Service registration + endpoint mapping |
| `Sprout.Api/Services/JsonProgressService.cs` | File I/O with SemaphoreSlim — be careful with locking |
| `sprout-web/src/api/client.ts` | Single source of truth for all API calls |
| `sprout-web/src/App.tsx` | View state (`child` \| `parent`), data fetching orchestration |
| `sprout-web/src/index.css` | All keyframe animations live here |

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
cd Sprout.Api && dotnet run           # starts on http://localhost:5000

# Frontend
cd sprout-web && npm run dev          # starts on http://localhost:5173

# Build for production (outputs to Sprout.Api/wwwroot)
cd sprout-web && npm run build && cp -r dist/* ../Sprout.Api/wwwroot/

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
- Do not rename `ITaskService` or `IProgressService` — other code depends on these interfaces
