import { useEffect, useRef } from 'react';
import { createPortal } from 'react-dom';

const COLORS = ['#FF6B9D', '#FFD54F', '#81D4FA', '#A5D6A7', '#FF8A65', '#CE93D8', '#80DEEA'];

function spawnConfetti(container: HTMLElement, count = 100) {
  Array.from({ length: count }).forEach((_, i) => {
    setTimeout(() => {
      const el = document.createElement('div');
      const tx = (Math.random() - 0.5) * 600;
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
        will-change: transform;
      `;

      container.appendChild(el);
      setTimeout(() => el.remove(), dur * 1000 + 100);
    }, i * 25);
  });
}

interface Props {
  onDismiss: () => void;
}

export function CelebrationOverlay({ onDismiss }: Props) {
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (containerRef.current) spawnConfetti(containerRef.current);
    const timer = setTimeout(onDismiss, 6000);
    return () => clearTimeout(timer);
  }, [onDismiss]);

  return createPortal(
    <div
      ref={containerRef}
      className="fixed inset-0 z-50 flex flex-col items-center justify-center bg-gradient-to-br from-yellow-300 to-orange-400 overflow-hidden"
      role="dialog"
      aria-modal="true"
      aria-labelledby="celebration-title"
      aria-describedby="celebration-message"
    >
      <div className="animate-celebration-pop flex flex-col items-center gap-6">
        <span className="text-[120px] leading-none animate-bounce-trophy" aria-hidden="true">🏆</span>
        <h1 id="celebration-title" className="text-5xl font-black text-white drop-shadow-lg">All Done!</h1>
        <p id="celebration-message" className="text-2xl text-yellow-100 font-semibold">Amazing job today! 🌟</p>
        <button
          onClick={onDismiss}
          aria-label="Dismiss celebration and continue"
          className="mt-4 min-h-[64px] min-w-[64px] px-10 py-4 bg-white/30 hover:bg-white/40 rounded-3xl text-white text-2xl font-bold active:scale-95 transition-all focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white"
        >
          ⭐ Yay!
        </button>
      </div>
    </div>,
    document.body,
  );
}
