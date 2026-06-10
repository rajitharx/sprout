# Sprout — Development Roadmap

**Current Status**: MVP complete with core features. Tracking next phases for polish, testing, and deployment.

---

## ✅ Completed (Sprint 0–4)

### Backend (Sprout.Api)
- [x] .NET 10 Minimal API structure with `Program.cs` endpoint registration
- [x] Repository Pattern: `ITaskService`, `IProgressService`, `IChildProfileService` interfaces
- [x] JSON-based persistence with `SemaphoreSlim` file locking for concurrency safety
- [x] All task endpoints: `GET /api/tasks`, `POST`, `PUT`, `DELETE`
- [x] All progress endpoints: `GET /api/progress/today`, `/week`, `POST /mark-complete`, `/mark-incomplete`
- [x] Child profile endpoints: `GET /api/profile`, `PUT /api/profile`
- [x] CORS configuration for development (localhost:5173)
- [x] Static file serving and SPA fallback (`MapFallbackToFile`)
- [x] System clock abstraction for testable date handling
- [x] Integration test suite (xUnit + WebApplicationFactory)
  - Task service and endpoint tests ✅
  - Progress service and endpoint tests ✅
  - Profile service tests ✅
  - Authentication service and endpoint tests ✅
- [x] Parent authentication (PIN)
  - `IAuthenticationService` with configuration-based PIN validation
  - PIN stored in `appsettings.json` (default: 1234)
  - `/api/auth/validate-pin` endpoint
  - `PinAuthModal` component with numeric input
  - PIN validation on parent panel access

### Frontend (sprout-web)
- [x] React 18 + TypeScript + Vite + Tailwind v4 setup
- [x] View routing: `View = 'child' | 'parent'` state in `App.tsx`
- [x] Child view layout: 100dvh flex, no scroll
- [x] Task carousel with swipe detection and dot indicators
- [x] Task card component with emoji animation (float keyframe)
- [x] Done button with scale animation and ripple effect
- [x] Celebration overlay with confetti spawn and auto-dismiss
- [x] Streak bar showing Mon–Sun week with star/circle/muted dot states
- [x] Parent panel with:
  - Task list display
  - Task creation with emoji picker (20-emoji grid)
  - Task editing inline
  - Task deletion with confirmation
  - Child profile editing (name + avatar emoji)
  - Progress summary for today
- [x] Custom hooks: `useTasks`, `useProgress`, `useProfile`
- [x] API client (`api/client.ts`) — single source of truth
- [x] Error handling: toast notifications on API failure
- [x] Offline fallback: localStorage cache for tasks/progress
- [x] All CSS keyframes implemented:
  - `float` (emoji animation) ✅
  - `bounce` (trophy in celebration) ✅
  - `confettiFall` (confetti animation) ✅
  - `pulseGlow` (streak bar star pulse) ✅
  - `ripple` (done button tap effect) ✅
  - `celebrationPop` (celebration entry) ✅

### Deployment & Infrastructure
- [x] Docker setup (`Dockerfile`, `docker-compose.yml`)
- [x] Deployment directory with scripts
- [x] Frontend build output in `wwwroot/`
- [x] Production-ready .NET configuration

---

## 🔶 Phase 1: Polish & Completeness (In Progress / Next)

### PWA & Web App
- [ ] **Populate `manifest.json`** with proper PWA metadata
  - App name, short name, icons array (192x192, 512x512)
  - Display mode: `standalone`
  - Background/theme colors
  - Start URL and scope
- [ ] **Generate PWA icons**
  - 192x192 and 512x512 PNG icons (coral/warm gradient with 🌱 emoji or app mark)
  - Place in `frontend/sprout-web/public/`
- [ ] **Service worker registration** (optional for MVP+)
  - Cache-first strategy for static assets
  - Network-first for API calls
  - Offline mode: serve fallback UI
- [ ] **Web app launch screen testing**
  - iOS: test "Add to Home Screen" flow
  - Android: test standalone mode and app appearance

### Accessibility & UX Polish
- [ ] **Aria labels & semantic HTML**
  - Add `aria-label` to all interactive elements
  - Use `<button>` not `<div>` for buttons
  - Ensure keyboard navigation (Tab, Enter) for parent panel
  - Color contrast checks (WCAG AA minimum)
- [ ] **Touch target sizing verification**
  - Ensure all tap targets ≥ 96px (Done button already ✅)
  - Carousel navigation chevrons
  - Parent panel buttons
- [ ] **Error states & edge cases**
  - Empty task list → show helpful prompt in child view
  - API unreachable → graceful degradation with localStorage
  - All tasks completed → verify celebration triggers (currently only shows on last task)
  - Network lag → disable buttons during API calls

### Testing & Quality
- [ ] **Frontend test suite** (optional but recommended)
  - Component snapshot tests (React Testing Library or Vitest)
  - Hook tests (`useTasks`, `useProgress`, `useProfile`)
  - API client mocking
  - Carousel gesture simulation
- [ ] **Backend test expansion**
  - Edge case: concurrent mark-complete on same task
  - Edge case: delete task while it's being completed
  - Edge case: date boundary (end of week, month rollover)
  - Performance: large task lists (50+ tasks)
- [ ] **End-to-end test flow**
  - Child completes a task → parent sees updated progress
  - Parent creates task → child carousel reflects immediately
  - Parent edits profile → child view shows new avatar
  - Offline → complete task → come back online → sync confirmed
- [ ] **Browser/device testing**
  - iPad (3–4 year old typical device)
  - Chrome DevTools mobile emulation
  - iOS Safari
  - Android Chrome

### Documentation
- [ ] **API documentation** (OpenAPI/Swagger optional)
  - Endpoint list, request/response schemas, examples
  - Or: inline `/api/docs` endpoint
- [ ] **Deployment guide**
  - Docker build & run instructions
  - Environment variables (`ASPNETCORE_URLS`, etc.)
  - Data persistence (volume mounts)
  - Reverse proxy setup (if behind nginx/Caddy)
- [ ] **Developer quick-start**
  - Architecture overview (service layer, data flow)
  - How to add a new task property
  - How to swap JSON storage for a real database
  - Testing guidelines

---

## 🟡 Phase 2: Features & Enhancements (Post-MVP)

### Child View Enhancements
- [ ] **Task-specific celebration variant**
  - Celebrate the task itself: "You brushed your teeth! 🦷✨"
  - Instead of generic "All Done!" on single-task completion
  - Show emoji from the completed task in the celebration
- [ ] **Sound effects** (optional)
  - Task complete "ding" sound
  - Confetti spawn sound
  - Parent can toggle audio in settings
- [ ] **Difficulty levels or streaks**
  - Show current streak count (days in a row)
  - Streak-based rewards or visual indicators
  - Reset streak on missed day (next sprint?)
- [ ] **Swipe gesture improvements**
  - Velocity-based snap (swipe past 30% → snap to next)
  - Drag-to-dismiss task (swipe up to skip, confirm?)
  - Chevron arrows fade on longer task lists

### Parent Panel Enhancements
- [x] **Parent auth (PIN)**
  - PIN unlock for parent panel ✅
  - Currently stored in configuration file (appsettings.json)
  - **TODO (Phase 3 - DB Migration):** Move PIN validation to database
    - Store PIN securely in database (hashed with salt)
    - Update `IAuthenticationService` to read from DB instead of config
    - Allow parent to change PIN in settings
    - Support parent-specific PIN per child profile
- [ ] **Drag-to-reorder tasks**
  - Visual drag handle (≡) beside each task
  - Update `sortOrder` on drop
- [ ] **Reset today button**
  - Un-mark all tasks for the day
  - Confirmation dialog ("Really reset?")
- [ ] **Weekly & monthly analytics**
  - Chart: completion rate by day
  - Trend: which tasks are often skipped?
  - Export progress data as CSV or JSON
- [ ] **Schedule tasks by time of day**
  - Morning routine (6am–9am)
  - Evening routine (6pm–8pm)
  - Mark due/overdue in child view
- [ ] **Parental controls (future)**
  - Lock task editing after certain time
  - Child can't re-enter parent view after panel closes

### Notifications & Reminders
- [ ] **Push notifications** (web or native)
  - Remind parent: "Tasks not yet completed" (10am, 6pm)
  - Celebrate with parent: "All done today! 🎉"
  - Mobile app: use OS notifications
- [ ] **Flexible scheduling**
  - Parent sets up routine schedules
  - Optional: recurring tasks by weekday

### Data & Sync
- [ ] **Multi-device sync** (optional)
  - Cloud sync: Firebase, Supabase, or custom server
  - Conflict resolution: last-write-wins or merge
  - Offline queue: queue changes, sync when online
- [ ] **Data export/backup**
  - JSON export of all tasks and progress history
  - CSV report for parent use
  - Import from backup file

---

## 🔴 Phase 3: Scaling & Deployment (Future)

### Backend Scaling
- [ ] **Database migration**
  - Swap `IProgressService` for PostgreSQL/SQLite implementation
  - Keep identical interface → no frontend changes
  - Migration script from `progress.json` to DB
  - Indexes on date, taskId, childId
  - **PIN validation database schema:**
    - `parent_auth` table with: `id`, `parent_pin_hash`, `parent_pin_salt`, `created_at`
    - Update `IAuthenticationService` to use database-backed PIN validation
    - Implement PIN hash verification (bcrypt or similar)
- [ ] **Multi-child support**
  - Add `ChildProfile` lookup by ID
  - Progress tied to child (childId foreign key)
  - Parent manages multiple children
  - Each child can have separate PIN (optional)
- [ ] **Real-time sync** (optional)
  - WebSocket or Server-Sent Events
  - Live updates: child completes → parent sees immediately
  - Eliminates 60s polling interval

### Frontend Scaling
- [ ] **Multiple children carousel** (parent view)
  - Select child, see their task carousel
  - Switch between children quickly
- [ ] **Customizable themes**
  - Parent chooses color palette (warm, cool, rainbow)
  - Child view adapts (not just gradient per task)
  - Dark mode option
- [ ] **Localization**
  - Parent labels in EN, ES, FR, etc.
  - Emoji descriptions in child view
  - RTL support for Arabic/Hebrew

### Deployment at Scale
- [ ] **CI/CD pipeline**
  - GitHub Actions: run tests on PR
  - Auto-deploy to staging on merge
  - Manual approval for production
- [ ] **Monitoring & logging**
  - Application Insights or similar
  - Error tracking (Sentry)
  - Performance metrics
- [ ] **Load testing**
  - Benchmark: 1000 concurrent children
  - API response time under load
  - Database connection pooling

---

## 📋 Known Issues & TODOs

### Current Bugs / Edge Cases
- [x] **Celebration triggers only on last task** ✅
  - Now: shows per-task celebration with task emoji, then all-done screen
  - Modified: `useProgress`, `CelebrationOverlay`, `App.tsx`
  - Status: Complete

- [x] **Streak bar week calculation** ✅
  - Verified: Mon–Sun alignment is correct
  - Added edge case tests: Sunday→Monday boundary, Saturday completion
  - Status: 73/73 tests passing

- [x] **localStorage fallback stale data** ✅
  - Mitigation implemented: "offline mode" banner shown when offline (App.tsx line 191-199)
  - Sync happens when back online via 60s polling interval
  - Status: Complete

- [x] **Manifest.json is empty** ✅
  - Populated with proper PWA metadata
  - Includes app name, icons array, display mode, theme colors, shortcuts
  - Note: Icon files (192x192, 512x512 PNG) need to be generated and placed in `public/`
  - Status: Complete (icons pending)

### Performance Considerations
- [ ] **Large task lists** (20+ tasks)
  - Carousel may lag on slow devices
  - Consider: virtualization or pagination
- [ ] **Frequent progress polling**
  - Currently 60s interval
  - Optimize: debounce API calls, use service worker cache
- [ ] **Confetti performance**
  - 100 confetti elements on low-end devices
  - Consider: reduce count on mobile, use CSS-only animation

---

## 🎯 Success Criteria by Phase

### Phase 1 (Polish & Completeness)
- [ ] PWA installable on iOS and Android
- [ ] All accessibility checks pass (WCAG AA)
- [ ] Empty task list shows helpful UI
- [ ] API failure gracefully degrades
- [ ] Test coverage ≥ 80%
- [ ] README includes deployment guide
- [ ] No console errors on Chrome, Safari, Firefox

### Phase 2 (Features)
- [ ] Parent can reorder tasks
- [ ] Task-specific celebration shows task emoji
- [ ] Weekly analytics visible in parent panel
- [ ] Reset today button works and confirms
- [ ] Sound effects toggle in settings

### Phase 3 (Scaling)
- [ ] Support 3+ children per account
- [ ] Database swap tested with zero schema change
- [ ] Real-time sync under 100ms latency
- [ ] Handles 1000 concurrent users
- [ ] Deployment documented for production

---

## 📅 Suggested Sprint Planning

| Sprint | Focus | Estimate |
| --- | --- | --- |
| **Sprint 4** | Phase 1: PWA + Accessibility | 2–3 weeks |
| **Sprint 5** | Phase 1: Testing + Docs | 1–2 weeks |
| **Sprint 6** | Phase 2: Task enhancements (reorder, per-task celebrate) | 2 weeks |
| **Sprint 7** | Phase 2: Parent analytics + notifications (optional) | 2 weeks |
| **Sprint 8+** | Phase 3: Multi-child, DB, real-time (post-launch) | 4+ weeks |

---

## 🚀 Launch Readiness Checklist

Before shipping to production:
- [ ] Phase 1 complete: PWA, accessibility, error handling
- [ ] No critical bugs in Phase 1 test suite
- [ ] Deployment scripts tested end-to-end
- [ ] Parent and child flows verified on real iPad/phone
- [ ] Data persistence confirmed (restart API, data remains)
- [ ] Offline mode tested (complete tasks, come back online, sync)
- [ ] API response times acceptable (<200ms p95)
- [ ] Documentation deployed (README + architecture guide)
- [ ] Monitoring & error tracking configured
- [ ] Stakeholder sign-off on child and parent UX

---

## 📝 Notes

- **Single-source-of-truth**: All API calls go through `api/client.ts` → easy to swap backend later
- **Repository Pattern**: Services abstract storage → swap JSON for DB, Redis, etc. with no endpoint changes
- **CSS-only animations**: No dependency on animation libraries → lightweight, predictable
- **Offline-first mindset**: localStorage fallback ensures child view never breaks
- **Toddler-centric design**: Every decision asks "Can a 3-year-old use this?"

---

*Last updated: 2026-06-10*

---

## Recent Fixes (Sprint 5)

✅ **Per-task celebration display** — Shows task emoji on individual task completion, trophy on "All Done!"  
✅ **Manifest.json populated** — Full PWA metadata with app name, icons, display mode, theme colors, shortcuts  
✅ **Edge case tests added** — Sunday→Monday boundary testing + Saturday completion verification  
✅ **README enhanced** — Added PWA installation guide, production deployment checklist, Docker HTTPS examples  
✅ **All 73 tests passing** — Including new edge case coverage for week calculation
