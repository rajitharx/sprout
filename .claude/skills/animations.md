# Skill: Animations & Micro-interactions

Read this before adding, modifying, or debugging any animation in Sprout.

---

## Golden Rule

**No animation libraries.** All animations are pure CSS keyframes defined in `sprout-web/src/index.css`. Never install Framer Motion, GSAP, React Spring, or `canvas-confetti`. DOM-spawned particles (stars, confetti) are created imperatively and removed after their animation — no persistent canvas or library state.

---

## All Keyframes (defined in `index.css`)

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

/* Celebration overlay entry */
@keyframes celebrationPop {
  0%   { transform: scale(0.7); opacity: 0; }
  70%  { transform: scale(1.05); }
  100% { transform: scale(1); opacity: 1; }
}

/* Stars flying from Done button to avatar */
@keyframes starFly {
  0%   { transform: translate(0, 0) scale(1.2); opacity: 1; }
  75%  { opacity: 1; }
  100% { transform: translate(var(--tx), var(--ty)) scale(0.15); opacity: 0; }
}

/* Profile avatar spring-bounce when stars arrive */
@keyframes avatarPop {
  0%   { transform: scale(1)    rotate(0deg); }
  35%  { transform: scale(1.45) rotate(12deg); }
  60%  { transform: scale(0.88) rotate(-6deg); }
  80%  { transform: scale(1.12) rotate(3deg); }
  100% { transform: scale(1)    rotate(0deg); }
}

/* Welcome overlay enter/exit */
@keyframes welcomeFadeIn {
  0%   { opacity: 0; transform: scale(0.9); }
  100% { opacity: 1; transform: scale(1); }
}
@keyframes welcomeFadeOut {
  0%   { opacity: 1; transform: scale(1); }
  100% { opacity: 0; transform: scale(1.05); }
}
```

---

## Named Utility Classes

```css
.animate-float           { animation: float 3s ease-in-out infinite; }
.animate-float-paused    { animation: float 3s ease-in-out infinite; animation-play-state: paused; }
.animate-bounce-trophy   { animation: bounce 0.7s ease-in-out infinite alternate; }
.animate-pulse-glow      { animation: pulseGlow 1.5s ease-in-out infinite; }
.animate-celebration-pop { animation: celebrationPop 0.45s cubic-bezier(0.34, 1.56, 0.64, 1) forwards; }
.animate-avatar-pop      { animation: avatarPop 0.55s cubic-bezier(0.34, 1.56, 0.64, 1) forwards; }
.animate-welcome-in      { animation: welcomeFadeIn 0.5s cubic-bezier(0.34, 1.56, 0.64, 1) forwards; }
.animate-welcome-out     { animation: welcomeFadeOut 0.4s ease-in forwards; }
```

All of these are listed in the `prefers-reduced-motion` block and will be disabled when the system setting is on.

---

## Reduced Motion

Every animation class is suppressed when `prefers-reduced-motion: reduce` is set. The block in `index.css` is the single source of truth — add new classes here:

```css
@media (prefers-reduced-motion: reduce) {
  .animate-float, .animate-float-paused, .animate-bounce-trophy,
  .animate-pulse-glow, .animate-welcome-in, .animate-welcome-out,
  .animate-celebration-pop, .animate-avatar-pop {
    animation: none !important;
    transform: none !important;
  }
}
```

For JS-spawned particles (stars, confetti), check before spawning:

```typescript
if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
```

---

## Star Collection Animation

Triggered when the child presses "I did it!" (not on undo). Stars fly from the button to the profile avatar.

```typescript
const STAR_CHARS = ['⭐', '✨', '⭐', '✨', '🌟', '⭐', '✨', '⭐'];

// targetX/targetY are the screen coords of the profile avatar (approx top-left area)
const targetX = 56;
const targetY = 88;

STAR_CHARS.forEach((char, i) => {
  setTimeout(() => {
    const el = document.createElement('div');
    const startX = originX + (Math.random() - 0.5) * 70; // spread at origin
    el.style.cssText = `
      position: fixed;
      left: ${startX}px; top: ${originY}px;
      font-size: ${18 + Math.floor(Math.random() * 14)}px; line-height: 1;
      --tx: ${targetX - startX}px;
      --ty: ${targetY - originY}px;
      animation: starFly ${(0.5 + Math.random() * 0.22).toFixed(2)}s
        cubic-bezier(0.2, 0.8, 0.4, 1) forwards;
      pointer-events: none; z-index: 100; will-change: transform, opacity;
    `;
    el.textContent = char;
    document.body.appendChild(el);
    setTimeout(() => el.remove(), dur * 1000 + 100);
  }, i * 55); // 55ms stagger between stars
});

// Fire avatarFlash in App.tsx ~620ms after first star spawns
if (onStarsReach) setTimeout(onStarsReach, 620);
```

The `avatarFlash` state in `App.tsx` uses a `key` prop trick to replay `avatarPop` on each flash — changing the `key` forces React to remount the span, restarting the animation:

```tsx
// In App.tsx:
const handleStarsReach = () => {
  setAvatarFlash(true);
  setTimeout(() => setAvatarFlash(false), 600);
};

// In TaskCarousel:
<span key={avatarFlash ? 'flash' : 'idle'}
  className={avatarFlash ? 'animate-avatar-pop' : ''}>
  {profile.avatar}
</span>
```

---

## Confetti System

Confetti is spawned as DOM elements in `CelebrationOverlay`. Never use canvas.

```typescript
function spawnConfetti(container: HTMLElement, count = 60) {
  const COLORS = ['#FF6B9D','#FFD54F','#81D4FA','#A5D6A7','#FF8A65','#CE93D8','#80DEEA'];

  Array.from({ length: count }).forEach((_, i) => {
    setTimeout(() => {
      const el = document.createElement('div');
      const tx  = (Math.random() - 0.5) * 400;
      const rot = (Math.random() - 0.5) * 720;
      const dur = 1.2 + Math.random() * 0.8;
      const size = 8 + Math.random() * 8;

      el.style.cssText = `
        position: absolute;
        width: ${size}px; height: ${size}px;
        left: ${10 + Math.random() * 80}%;
        top: ${Math.random() * 20}%;
        background: ${COLORS[Math.floor(Math.random() * COLORS.length)]};
        border-radius: ${Math.random() > 0.5 ? '50%' : '2px'};
        --tx: ${tx}px; --rot: ${rot}deg;
        animation: confettiFall ${dur}s ease-in forwards;
        pointer-events: none;
      `;
      container.appendChild(el);
      setTimeout(() => el.remove(), dur * 1000 + 100);
    }, i * 25);
  });
}
```

---

## Ripple Effect on Done Button

On button press (`onPointerDown`), create a ripple div at the tap point:

```tsx
const ripple = document.createElement('div');
const size = Math.max(rect.width, rect.height);
ripple.style.cssText = `
  position: absolute;
  width: ${size}px; height: ${size}px;
  left: ${e.clientX - rect.left - size / 2}px;
  top: ${e.clientY - rect.top - size / 2}px;
  background: rgba(255,255,255,0.35);
  border-radius: 50%;
  animation: ripple 0.6s ease-out forwards;
  pointer-events: none;
`;
btn.appendChild(ripple);
setTimeout(() => ripple.remove(), 700);
```

The button needs `position: relative; overflow: hidden` (already in Tailwind classes).

---

## Card Slide Transition

Task carousel transitions use CSS `transform` + `transition`, never JS animation:

```tsx
<div
  className="flex h-full transition-transform duration-300 ease-in-out"
  style={{ transform: `translateX(-${currentIndex * (viewportWidth + SPACING_PX)}px)` }}
>
```

Use `ease-in-out` for slides. Never `linear` for UI transitions.

---

## Streak Dot States

| State | Appearance |
|---|---|
| Completed | Gold filled dot |
| Today / incomplete | Outlined dot with `animate-pulse-glow` |
| Past / missed | Gray filled dot |
| Future | Light gray outlined dot |

---

## Performance Rules

- Animate only `transform` and `opacity` — never `width`, `height`, `top`, `left`, or `margin`.
- Add `will-change: transform` to elements that animate position/scale.
- DOM-spawned elements (stars, confetti) must remove themselves after their animation ends.
- The float animation on non-active `TaskCard` emojis must be paused: use `.animate-float-paused` on inactive cards.
