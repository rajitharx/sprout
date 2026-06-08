const STAR_CHARS = ['⭐', '✨', '⭐', '✨', '🌟', '⭐', '✨', '⭐'];

interface Props {
  onDone: () => void;
  completed: boolean;
  disabled?: boolean;
  onStarsReach?: () => void;
}

export function DoneButton({ onDone, completed, disabled, onStarsReach }: Props) {
  const handlePointerDown = (e: React.PointerEvent<HTMLButtonElement>) => {
    const btn = e.currentTarget;
    const rect = btn.getBoundingClientRect();

    // Ripple
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

    // Stars flying to profile avatar — only on "I did it!" press
    if (completed || disabled) return;
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

    const originX = rect.left + rect.width / 2;
    const originY = rect.top + rect.height / 2;
    // Profile avatar lives near top-left of the carousel area
    const targetX = 56;
    const targetY = 88;

    STAR_CHARS.forEach((char, i) => {
      setTimeout(() => {
        const el = document.createElement('div');
        const startOffsetX = (Math.random() - 0.5) * 70;
        const startX = originX + startOffsetX;
        const tx = targetX - startX;
        const ty = targetY - originY;
        const dur = (0.5 + Math.random() * 0.22).toFixed(2);
        const size = 18 + Math.floor(Math.random() * 14);

        el.style.cssText = `
          position: fixed;
          left: ${startX}px;
          top: ${originY}px;
          font-size: ${size}px;
          line-height: 1;
          --tx: ${tx}px;
          --ty: ${ty}px;
          animation: starFly ${dur}s cubic-bezier(0.2, 0.8, 0.4, 1) forwards;
          pointer-events: none;
          z-index: 100;
          will-change: transform, opacity;
        `;
        el.textContent = char;
        document.body.appendChild(el);
        setTimeout(() => el.remove(), parseFloat(dur) * 1000 + 100);
      }, i * 55);
    });

    // Notify parent when first stars arrive (~600ms)
    if (onStarsReach) setTimeout(onStarsReach, 620);
  };

  const gradient = completed
    ? 'from-emerald-400 to-green-500'
    : 'from-orange-400 to-rose-400';

  return (
    <div className="flex justify-center items-center pb-6 pt-2">
      <button
        onClick={onDone}
        onPointerDown={handlePointerDown}
        disabled={disabled}
        aria-label={completed ? 'Mark task incomplete' : 'Mark task done and move to next'}
        title={completed ? 'Click to mark incomplete' : 'Click to mark complete'}
        className={`relative overflow-hidden min-h-[96px] w-4/5 rounded-3xl bg-gradient-to-r ${gradient} shadow-lg text-white text-4xl font-black flex items-center justify-center gap-3 hover:shadow-xl active:scale-[0.97] transition-all disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white`}
      >
        <span aria-hidden="true">{completed ? '✅' : '☑️'}</span>
        <span className="text-2xl font-bold">{completed ? 'Done!' : 'I did it!'}</span>
      </button>
    </div>
  );
}
