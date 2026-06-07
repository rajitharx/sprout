# Skill: Frontend (React 18 + TypeScript + Tailwind v4)

Read this before making any changes to `sprout-web/src/`.

---

## Component Map

```
App.tsx  (view state: 'child' | 'parent')
├── Child View
│   ├── StreakBar          — 7-day completion dots at top
│   ├── TaskCarousel       — swipeable card container + profile header
│   │   └── TaskCard       — single task (emoji + label, gradient bg)
│   └── DoneButton         — large completion button + star animation
│       └── CelebrationOverlay  — full-screen reward (portal)
│
└── Parent View (ParentPanel)
    ├── Child profile editor
    ├── Task list with drag-to-reorder
    ├── Inline edit + delete confirmation
    └── Add task footer
```

---

## View Switching

View state lives only in `App.tsx`. Never lift it elsewhere.

```tsx
const [view, setView] = useState<'child' | 'parent'>('child');
```

The parent panel is accessed via a small SVG gear icon (`opacity-25`) in the top-right corner of the child view — rendered as an inline SVG, not an emoji. Keep it discreet so toddlers don't accidentally open it.

---

## Data Fetching Hooks

All API calls go through hooks, never directly in components.

```typescript
// hooks/useTasks.ts — returns sorted task list
export function useTasks() {
  return { tasks, loading, createTask, updateTask, deleteTask };
}

// hooks/useProgress.ts — fires onAllComplete when every active task is done
export function useProgress(onAllComplete?: () => void) {
  return { today, week, markComplete, markIncomplete };
}
// markComplete(taskId: string, taskIds: string[]) — pass full array of active task IDs
// so stale IDs from soft-deleted tasks don't inflate the completion count.

// hooks/useProfile.ts
export function useProfile() {
  return { profile, loading, updateProfile };
}
```

---

## localStorage Cache

`useTasks` caches the task list after every successful fetch and reads it as a fallback:

```typescript
const CACHE_KEY = 'sprout_tasks_cache';
// on success:
localStorage.setItem(CACHE_KEY, JSON.stringify(tasks));
// on API error:
const cached = localStorage.getItem(CACHE_KEY);
if (cached) setTasks(JSON.parse(cached));
```

Never use `localStorage` as the primary store. Always attempt the API first.

---

## Tailwind Rules

- Use Tailwind utility classes for everything except keyframe animations.
- Inline `style={{}}` only for CSS custom properties (e.g. `--tx`, `--ty` on star/confetti elements).
- Do not install Tailwind plugins.
- Use `100dvh` for full-viewport height (not `100vh`) — accounts for mobile browser chrome.

**Tap target minimums:**

| Context | Minimum |
|---|---|
| Child view — Done button | `min-h-[96px]` full-width |
| Child view — carousel arrows | `min-h-[64px] min-w-[64px]` |
| Child view — any other interactive | `min-h-[64px] min-w-[64px]` |
| Parent view — all interactive | `min-h-[44px] min-w-[44px]` |

---

## TaskCard Gradient Palette

Cycle through these six gradients by task index (`index % 6`). Use 200–300 level shades for vibrancy:

```typescript
const GRADIENTS = [
  'from-rose-200 to-pink-300',
  'from-amber-200 to-yellow-300',
  'from-sky-200 to-indigo-200',
  'from-emerald-200 to-teal-300',
  'from-orange-200 to-amber-300',
  'from-violet-200 to-purple-300',
];
```

When `completed`, override to `'from-emerald-400 to-green-500'` (matches the Done button gradient).

---

## TaskCarousel — Profile Header + Avatar Flash

`TaskCarousel` renders a profile row above the cards:

```tsx
interface Props {
  profile: ChildProfile;
  tasks: HabitTask[];
  completedIds: string[];
  currentIndex: number;
  onIndexChange: (index: number) => void;
  avatarFlash: boolean;   // toggled by App.tsx when stars reach the avatar
}
```

The avatar uses the `key` prop trick to replay the `avatarPop` animation on every flash without toggling a class:

```tsx
<span
  key={avatarFlash ? 'flash' : 'idle'}
  className={`text-5xl leading-none ${avatarFlash ? 'animate-avatar-pop' : ''}`}
>
  {profile.avatar}
</span>
```

---

## Swipe Detection (No Library)

Implement swipe in `TaskCarousel` with pointer events:

```tsx
const SWIPE_THRESHOLD = 50; // px

const handlePointerDown = (e: React.PointerEvent) => {
  startX.current = e.clientX;
};

const handlePointerUp = (e: React.PointerEvent) => {
  const delta = startX.current! - e.clientX;
  if (delta > SWIPE_THRESHOLD) goNext();
  if (delta < -SWIPE_THRESHOLD) goPrev();
};
```

---

## ParentPanel — Drag-to-Reorder

Tasks are reorderable via a grip handle using pointer capture. No drag-and-drop library.

Key points:
- `localTasks` is a local sorted copy of `tasks` prop; synced via `useEffect` when not dragging.
- `dragIndexRef` and `overIndexRef` are refs (not state) to avoid stale closures in pointer handlers.
- `setPointerCapture` on the handle keeps all pointer events routed to it during drag.
- On drop: splice the reordered array, update `sortOrder` for each task, call `onUpdate` for any task whose `sortOrder` changed.
- Drag is disabled while an edit form is open (`editingId !== null`).
- Handle has `style={{ touchAction: 'none' }}` to prevent scroll conflict on touch.

---

## CelebrationOverlay

Render as a React Portal into `document.body` so it covers everything including the gear icon:

```tsx
import { createPortal } from 'react-dom';

return createPortal(
  <div className="fixed inset-0 z-50 ...">...</div>,
  document.body
);
```

Auto-dismiss after 6 seconds. Dismiss button must be large and obvious.

---

## Toast Notifications

A lightweight toast for API errors — no library needed. Show from `App.tsx` state, never from inside child components. Auto-dismiss after 3 seconds.

---

## Emoji Fallback

If a task has an empty or undefined emoji, always fall back to `📋`:

```tsx
const emoji = task.emoji || '📋';
```

---

## Component Checklist

Before marking a component done, verify:
- [ ] Works on 390px wide viewport (iPhone 14 Pro)
- [ ] Child view tap targets ≥ 64px; parent view ≥ 44px
- [ ] No horizontal overflow / scroll
- [ ] Handles empty/loading/error states
- [ ] Uses hook data, not direct `api.client` calls
