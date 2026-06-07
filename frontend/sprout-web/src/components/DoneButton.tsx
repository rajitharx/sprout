interface Props {
  onDone: () => void;
  completed: boolean;
  disabled?: boolean;
}

export function DoneButton({ onDone, completed, disabled }: Props) {
  const handlePointerDown = (e: React.PointerEvent<HTMLButtonElement>) => {
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
      background: rgba(255,255,255,0.35);
      border-radius: 50%;
      animation: ripple 0.6s ease-out forwards;
      pointer-events: none;
    `;

    btn.appendChild(ripple);
    setTimeout(() => ripple.remove(), 700);
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
        aria-label={completed ? 'Mark task incomplete' : 'Mark task done'}
        className={`relative overflow-hidden min-h-[96px] w-4/5 rounded-3xl bg-gradient-to-r ${gradient} shadow-lg text-white text-4xl font-black flex items-center justify-center gap-3 active:scale-[0.97] transition-transform disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer`}
      >
        <span>{completed ? '✅' : '☑️'}</span>
        <span className="text-2xl font-bold">{completed ? 'Done!' : 'I did it!'}</span>
      </button>
    </div>
  );
}
