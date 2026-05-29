# Skill: Animations & Micro-interactions

Read this before adding, modifying, or debugging any animation in Sprout.

---

## Golden Rule

**No animation libraries.** All animations are pure CSS keyframes defined in `sprout-web/src/index.css`. Never install Framer Motion, GSAP, React Spring, or `canvas-confetti`. The five core keyframes cover every animation need in this app.

---

## Core Keyframes (defined in `index.css`)

```css
/* Emoji on task card — gentle hover */
@keyframes float {
  0%, 100% { transform: translateY(0px); }
  50%       { transform: translateY(-14px); }
}

/* Trophy on celebration screen */
@keyframes bounce {
  0%, 100% { transform: translateY(0px) rotate(-4deg); }
  50%       { transform: translateY(-20px) rotate(4deg); }
}

/* Confetti pieces falling */
@keyframes confettiFall {
  0%   { transform: translate(0, 0) rotate(0deg); opacity: 1; }
  100% { transform: translate(var(--tx), 110vh) rotate(var(--rot)); opacity: 0; }
}

/* Streak dot — today's incomplete day */
@keyframes pulseGlow {
  0%, 100% { box-shadow: 0 0 0 0px rgba(255, 213, 79, 0.6); }
  50%       { box-shadow: 0 0 0 8px rgba(255, 213, 79, 0); }
}

/* Done button press feedback */
@keyframes ripple {
  0%   { transform: scale(0); opacity: 0.5; }
  100% { transform: scale(6); opacity: 0; }
}
```

Apply via Tailwind's arbitrary `[animation:...]` or add named utility classes in `index.css`:

```css
.animate-float    { animation: float 3s ease-in-out infinite; }
.animate-bounce-trophy { animation: bounce 0.7s ease-in-out infinite alternate; }
.animate-pulse-glow    { animation: pulseGlow 1.5s ease-in-out infinite; }
```

---

## Confetti System

Confetti is spawned as DOM elements in `CelebrationOverlay`. Never use canvas.

### Spawn function

```typescript
function spawnConfetti(container: HTMLElement, count = 60) {
  const COLORS = [
    '#FF6B9D', '#FFD54F', '#81D4FA',
    '#A5D6A7', '#FF8A65', '#CE93D8', '#80DEEA',
  ];

  Array.from({ length: count }).forEach((_, i) => {
    setTimeout(() => {
      const el = document.createElement('div');
      const tx  = (Math.random() - 0.5) * 400;
      const rot = (Math.random() - 0.5) * 720;
      const dur = 1.2 + Math.random() * 0.8;
      const size = 8 + Math.random() * 8;

      el.style.cssText = `
        position: absolute;
        width: ${size}px;
        height: ${size}px;
        left: ${10 + Math.random() * 80}%;
        top: ${Math.random() * 20}%;
        background: ${COLORS[Math.floor(Math.random() * COLORS.length)]};
        border-radius: ${Math.random() > 0.5 ? '50%' : '2px'};
        --tx: ${tx}px;
        --rot: ${rot}deg;
        animation: confettiFall ${dur}s ease-in forwards;
        pointer-events: none;
      `;

      container.appendChild(el);
      setTimeout(() => el.remove(), dur * 1000 + 100);
    }, i * 25); // stagger spawn
  });
}
```

Call `spawnConfetti(containerRef.current)` when the overlay becomes visible. Clean up is automatic via `el.remove()` after each animation ends.

### All-tasks-complete variant

Spawn 100 pieces (not 60) and use a larger spread (`tx * 600`).

---

## Ripple Effect on Done Button

On button press, create a ripple div positioned at the tap point:

```tsx
const handlePress = (e: React.PointerEvent<HTMLButtonElement>) => {
  const btn = e.currentTarget;
  const rect = btn.getBoundingClientRect();
  const ripple = document.createElement('div');
  const size = Math.max(rect.width, rect.height);

  ripple.style.cssText = `
    position: absolute;
    width: ${size}px;
    height: ${size}px;
    left: ${e.clientX - rect.left - size / 2}px;
    top: ${e.clientY - rect.top - size / 2}px;
    background: rgba(255,255,255,0.4);
    border-radius: 50%;
    animation: ripple 0.6s ease-out forwards;
    pointer-events: none;
  `;

  btn.style.position = 'relative';
  btn.style.overflow = 'hidden';
  btn.appendChild(ripple);
  setTimeout(() => ripple.remove(), 700);
};
```

---

## Card Slide Transition

Task carousel transitions use CSS `transform` + `transition`, never JS animation:

```tsx
// In TaskCarousel
<div
  className="flex transition-transform duration-300 ease-in-out"
  style={{ transform: `translateX(calc(-${currentIndex * 100}% - ${currentIndex * gap}px))` }}
>
  {tasks.map(task => <TaskCard key={task.id} task={task} />)}
</div>
```

Use `ease-in-out` for slides. Never use `linear` for UI transitions.

---

## Celebration Overlay Entry

The overlay enters with a scale + opacity pop:

```css
/* index.css */
@keyframes celebrationPop {
  0%   { transform: scale(0.7); opacity: 0; }
  70%  { transform: scale(1.05); }
  100% { transform: scale(1); opacity: 1; }
}

.animate-celebration-pop {
  animation: celebrationPop 0.45s cubic-bezier(0.34, 1.56, 0.64, 1) forwards;
}
```

Apply `animate-celebration-pop` to the overlay root div when it becomes visible.

---

## Streak Dot States

| State | Class | Animation |
|---|---|---|
| Completed | `bg-yellow-400 text-yellow-900` | none (static gold) |
| Today / incomplete | `border-2 border-yellow-400` | `animate-pulse-glow` |
| Past / missed | `bg-gray-200` | none |
| Future | `border border-gray-200` | none |

---

## Performance Rules

- All `animation` properties must include `will-change: transform` on elements that animate position/scale.
- Confetti elements are removed from the DOM after their animation — never accumulate stale elements.
- The float animation on `TaskCard` emoji must pause when the card is not the active slide: add `animation-play-state: paused` on non-active cards.
- Never animate `width`, `height`, `top`, `left`, or `margin` — always use `transform` equivalents.
