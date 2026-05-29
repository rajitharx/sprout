# Skill: Frontend (React 18 + TypeScript + Tailwind v4)

Read this before making any changes to `sprout-web/src/`.

---

## Component Map

```
App.tsx  (view state: 'child' | 'parent')
├── Child View
│   ├── StreakBar          — 7-day completion dots at top
│   ├── TaskCarousel       — swipeable card container
│   │   └── TaskCard       — single task (emoji + label)
│   └── DoneButton         — large completion button
│       └── CelebrationOverlay  — full-screen reward (portal)
│
└── Parent View (ParentPanel)
    ├── Task list + edit
    ├── Emoji picker
    └── Progress summary
```

---

## View Switching

View state lives only in `App.tsx`. Never lift it elsewhere.

```tsx
const [view, setView] = useState<'child' | 'parent'>('child');
```

The parent panel is accessed via a small ⚙️ icon (`text-xs opacity-30`) in the top-right corner of the child view. Keep it discreet — toddlers should not accidentally open it.

---

## Data Fetching Hooks

All API calls go through hooks, never directly in components:

```typescript
// hooks/useTasks.ts
export function useTasks() {
  const [tasks, setTasks] = useState<HabitTask[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refresh = async () => { ... };

  useEffect(() => { refresh(); }, []);

  return { tasks, loading, error, refresh };
}

// hooks/useProgress.ts
export function useProgress() {
  const [today, setToday] = useState<DailyProgress | null>(null);
  const [week, setWeek] = useState<DailyProgress[]>([]);

  // Poll every 60s for multi-device sync
  useEffect(() => {
    const id = setInterval(refreshToday, 60_000);
    return () => clearInterval(id);
  }, []);

  return { today, week, markComplete, markIncomplete, refresh };
}
```

---

## localStorage Cache

`useTasks` must cache the task list to `localStorage` after every successful fetch, and read from it as a fallback when the API is unreachable:

```typescript
const CACHE_KEY = 'sprout_tasks_cache';

// on success:
localStorage.setItem(CACHE_KEY, JSON.stringify(tasks));

// on error:
const cached = localStorage.getItem(CACHE_KEY);
if (cached) setTasks(JSON.parse(cached));
```

Never use `localStorage` as the primary store. Always attempt the API first.

---

## Tailwind Rules

- Use Tailwind utility classes for everything except keyframe animations.
- Do not write `style={{}}` inline styles except for CSS custom properties (e.g. `--tx`, `--rot` on confetti elements).
- Do not install Tailwind plugins.
- Minimum tap target for any interactive element: `min-h-[64px] min-w-[64px]`.
- Use `100dvh` for full-viewport height (not `100vh`) — accounts for mobile browser chrome.

---

## TaskCard Gradient Palette

Cycle through these six gradients by task index (`index % 6`):

```typescript
const GRADIENTS = [
  'from-pink-100 to-orange-100',
  'from-yellow-100 to-green-100',
  'from-blue-100 to-purple-100',
  'from-green-100 to-teal-100',
  'from-orange-100 to-red-100',
  'from-purple-100 to-pink-100',
];
```

Apply as: `className={`bg-gradient-to-br ${GRADIENTS[index % 6]}`}`

---

## Swipe Detection (No Library)

Implement swipe in `TaskCarousel` with pointer events:

```tsx
const SWIPE_THRESHOLD = 50; // px

const handlePointerDown = (e: React.PointerEvent) => {
  startX.current = e.clientX;
};

const handlePointerUp = (e: React.PointerEvent) => {
  const delta = e.clientX - startX.current;
  if (delta < -SWIPE_THRESHOLD) goNext();
  if (delta > SWIPE_THRESHOLD) goPrev();
};
```

Attach `onPointerDown` and `onPointerUp` to the carousel container. Do not use `onTouchStart/End` — pointer events work for both touch and mouse.

---

## CelebrationOverlay

Render as a React Portal into `document.body` so it covers everything including the parent ⚙️ icon:

```tsx
import { createPortal } from 'react-dom';

return createPortal(
  <div className="fixed inset-0 z-50 ...">...</div>,
  document.body
);
```

Auto-dismiss after 6 seconds:

```tsx
useEffect(() => {
  if (!visible) return;
  const id = setTimeout(onDismiss, 6000);
  return () => clearTimeout(id);
}, [visible]);
```

---

## Toast Notifications

A lightweight toast for API errors — no library needed:

```tsx
// components/Toast.tsx
export function Toast({ message, onDone }: { message: string; onDone: () => void }) {
  useEffect(() => {
    const id = setTimeout(onDone, 3000);
    return () => clearTimeout(id);
  }, []);

  return (
    <div className="fixed bottom-6 left-1/2 -translate-x-1/2 bg-gray-800 text-white
                    text-sm font-semibold px-5 py-3 rounded-full shadow-lg z-50">
      {message}
    </div>
  );
}
```

Show it from `App.tsx` state — never from inside child components.

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
- [ ] All tap targets ≥ 64px
- [ ] No horizontal overflow / scroll
- [ ] Handles empty/loading/error states
- [ ] Uses hook data, not direct `api.client` calls
