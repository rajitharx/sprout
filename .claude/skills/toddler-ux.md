# Skill: Toddler UX Design

Read this before making any change to the child-facing UI (`View = 'child'`).

---

## The User

**Age:** 3–4 years old.
**Device:** iPad or Android tablet held with two hands, occasionally one hand.
**Literacy:** Zero. Cannot read labels, error messages, or button text.
**Attention span:** 10–20 seconds before losing interest if nothing happens.
**Motor precision:** Low. Fingers are chubby, taps are imprecise, often multi-finger.
**Emotional state:** Expects delight. Frustration comes fast if feedback is absent or confusing.

---

## The One Question

Before every UI decision in the child view, ask:

> **"Can a 3-year-old operate this without any help from a parent?"**

If the answer is "maybe" or "no", redesign before shipping.

---

## Tap Target Rules

| Element | Minimum size |
|---|---|
| Done button | `min-h-[96px]`, full-width (`w-4/5`) |
| Carousel arrow | `min-h-[64px] min-w-[64px]` |
| Streak dot (decorative) | Not interactive — no minimum |
| Any other interactive element in child view | `min-h-[64px] min-w-[64px]` |

Use `touch-action: manipulation` on all interactive elements to eliminate the 300ms tap delay.

---

## No Text Dependency

The child view must be fully operable with zero text comprehension:

- Task identity = emoji (96px+), not label
- Action = one large button with a checkmark emoji (✅)
- Completion = full-screen celebration (unmistakable)
- Progress = colored dots (gold = done, gray = not done)

The label under the emoji (`text-2xl font-black`) is for parents watching — keep it visible but visually secondary to the emoji.

---

## Instant Feedback

Every tap must produce visible + haptic feedback within **100ms**. No exceptions.

| Tap | Immediate response (< 100ms) | Delayed response |
|---|---|---|
| Done button | Scale down 4%, ripple starts | Celebration overlay (300ms) |
| Carousel swipe | Card begins sliding | Snap to next card (300ms) |
| Carousel arrow | Card begins sliding | Snap to next card (300ms) |

If the API call fails, the UI must still respond immediately (optimistic update). Roll back silently if the API confirms failure.

---

## Optimistic Updates

When the child taps Done, update the local state immediately — don't wait for the API response:

```typescript
const markComplete = async (taskId: string) => {
  // 1. Update UI immediately
  setToday(prev => ({
    ...prev!,
    completedTaskIds: [...(prev?.completedTaskIds ?? []), taskId],
  }));

  // 2. Trigger celebration immediately
  setShowCelebration(true);

  // 3. Sync to backend (fire and forget from UX perspective)
  try {
    await api.markComplete(taskId);
  } catch {
    // silently roll back
    setToday(prev => ({
      ...prev!,
      completedTaskIds: prev!.completedTaskIds.filter(id => id !== taskId),
    }));
    showToast('Could not save — try again');
  }
};
```

---

## Celebration Must Be Unmistakable

The reward moment is the entire point of the app. It must feel genuinely exciting, not polite.

Checklist for `CelebrationOverlay`:
- [ ] Covers 100% of the screen — no peeking of the task card behind it
- [ ] Bright, warm full-screen background (gold/yellow gradient)
- [ ] Trophy emoji at `text-[120px]` minimum, bouncing
- [ ] At least 60 confetti pieces in 7+ distinct colors
- [ ] Text is large (`text-5xl font-black`) and white
- [ ] Auto-dismisses after 6 seconds — child should not need to find the dismiss button
- [ ] Dismiss button is large and obvious (⭐ Yay!) for when they want to go again immediately

---

## No Dead Ends

The child view must never reach a state where the child is stuck with nothing to interact with:

| Situation | Correct behavior |
|---|---|
| All tasks complete | Show "All Done! 🎉" celebration, then a static "all done" state with gold dots |
| No tasks configured | Show a friendly placeholder emoji (🌱) — parent must add tasks |
| API offline | Show cached tasks from localStorage, all marked as not done |
| Loading | Show skeleton pulse on the card area — never a spinner (toddlers don't understand spinners) |

---

## Layout — Never Break These Rules

```
┌─────────────────────────────┐  ← top of 100dvh
│  StreakBar (fixed height)   │  32px
│─────────────────────────────│
│                             │
│      TaskCard               │  flex-1 (fills remaining space)
│     (emoji + label)         │
│                             │
│─────────────────────────────│
│    DoneButton               │  min-h-[96px], fixed to bottom
└─────────────────────────────┘  ← bottom of 100dvh
```

- Use `h-[100dvh] flex flex-col` on the child view root.
- `StreakBar` and `DoneButton` have fixed heights.
- `TaskCard` area uses `flex-1` to fill the middle.
- **Nothing scrolls.** If content overflows, reduce font size or padding — never add a scrollbar.

---

## Colors & Visual Language

Toddlers respond to warm, saturated, high-contrast colors:

- Task card backgrounds: soft pastel gradients (not white)
- Done button: coral-to-orange gradient (`from-pink-400 to-orange-400`)
- Completed state: green gradient (`from-green-400 to-emerald-400`)
- Celebration: yellow/gold (`from-yellow-300 to-orange-400`)
- Streak complete: `bg-yellow-400` (gold star)

Avoid: gray, dark backgrounds, muted palettes, anything that looks "empty" or "broken" to a child.

---

## Sound (Future Phase 2)

When adding sound:
- Use Web Audio API — no libraries
- Button click: short (< 200ms) high-pitched tone
- Celebration: 2–3 second ascending fanfare
- All sounds must have a parent-controlled mute toggle (stored in localStorage)
- Never autoplay sound without a user gesture — browser policy blocks it
