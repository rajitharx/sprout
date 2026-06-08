import { useRef, useState, useEffect } from 'react';
import type { HabitTask, ChildProfile } from '../types';
import { TaskCard } from './TaskCard';

const SWIPE_THRESHOLD = 50;

interface Props {
  profile: ChildProfile;
  tasks: HabitTask[];
  completedIds: string[];
  currentIndex: number;
  onIndexChange: (index: number) => void;
  avatarFlash: boolean;
}

export function TaskCarousel({ profile, tasks, completedIds, currentIndex, onIndexChange, avatarFlash }: Props) {
  const pointerStartX = useRef<number | null>(null);
  const SPACING_PX = 16;
  const viewportRef = useRef<HTMLDivElement | null>(null);
  const [viewportWidth, setViewportWidth] = useState(0);

  useEffect(() => {
    function measure() {
      if (!viewportRef.current) return;
      setViewportWidth(viewportRef.current.clientWidth);
    }
    measure();
    window.addEventListener('resize', measure);
    return () => window.removeEventListener('resize', measure);
  }, []);

  const goNext = () => {
    if (currentIndex < tasks.length - 1) onIndexChange(currentIndex + 1);
  };
  const goPrev = () => {
    if (currentIndex > 0) onIndexChange(currentIndex - 1);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'ArrowRight') {
      e.preventDefault();
      goNext();
    } else if (e.key === 'ArrowLeft') {
      e.preventDefault();
      goPrev();
    }
  };

  const handlePointerDown = (e: React.PointerEvent) => {
    pointerStartX.current = e.clientX;
  };
  const handlePointerUp = (e: React.PointerEvent) => {
    if (pointerStartX.current === null) return;
    const delta = pointerStartX.current - e.clientX;
    if (delta > SWIPE_THRESHOLD) goNext();
    else if (delta < -SWIPE_THRESHOLD) goPrev();
    pointerStartX.current = null;
  };

  const completedCount = completedIds.filter(id => tasks.some(t => t.id === id)).length;

  if (tasks.length === 0) {
    return (
      <div className="flex-1 flex flex-col items-center justify-center gap-4 text-gray-400 px-4">
        <span className="text-[80px] animate-float" aria-hidden="true">🌱</span>
        <p className="text-xl font-bold text-gray-500">No tasks yet</p>
        <p className="text-base text-gray-400">Ask a parent to add some!</p>
      </div>
    );
  }

  return (
    <div className="flex-1 flex flex-col overflow-hidden relative px-4 pb-2">
      {/* Profile header with progress */}
      <div className="flex items-center gap-3 py-3 px-1">
        <span key={avatarFlash ? 'flash' : 'idle'} className={`text-5xl leading-none ${avatarFlash ? 'animate-avatar-pop' : ''}`} aria-hidden="true">
          {profile.avatar}
        </span>
        <div className="flex-1 min-w-0">
          <p className="text-xl font-bold text-gray-700 leading-tight truncate">{profile.name}</p>
          <div className="flex items-center gap-2 mt-1">
            <div className="flex-1 h-1.5 bg-gray-100 rounded-full overflow-hidden">
              <div
                className="h-full bg-orange-400 rounded-full transition-all duration-500"
                style={{ width: tasks.length > 0 ? `${(completedCount / tasks.length) * 100}%` : '0%' }}
                role="progressbar"
                aria-valuenow={completedCount}
                aria-valuemin={0}
                aria-valuemax={tasks.length}
                aria-label="Daily task progress"
              />
            </div>
            <span className="text-xs text-gray-400 font-medium shrink-0" aria-live="polite">{completedCount}/{tasks.length}</span>
          </div>
        </div>
      </div>

      {/* Card viewport */}
      <div
        className="flex-1 overflow-hidden relative cursor-grab active:cursor-grabbing focus:outline-none focus-visible:ring-2 focus-visible:ring-orange-400 rounded-lg"
        onPointerDown={handlePointerDown}
        onPointerUp={handlePointerUp}
        onKeyDown={handleKeyDown}
        ref={viewportRef}
        role="region"
        aria-label="Task carousel, use arrow keys to navigate"
        tabIndex={0}
      >
        <div
          className="flex h-full transition-transform duration-300 ease-in-out"
          style={{
            transform: `translateX(-${currentIndex * (viewportWidth + SPACING_PX)}px)`,
          }}
        >
          {tasks.map((task, i) => (
            <div
              key={task.id}
              className="flex-shrink-0 h-full"
              style={{ width: viewportWidth, marginRight: i === tasks.length - 1 ? 0 : SPACING_PX }}
            >
              <TaskCard
                task={task}
                completed={completedIds.includes(task.id)}
                gradientIndex={i}
                isActive={i === currentIndex}
              />
            </div>
          ))}
        </div>
      </div>

      {/* Navigation */}
      <nav className="flex items-center justify-between mt-3 px-1" aria-label="Task navigation">
        <button
          onClick={goPrev}
          disabled={currentIndex === 0}
          aria-label="Previous task"
          title="Previous task (or press left arrow)"
          className="min-h-[64px] min-w-[64px] flex items-center justify-center text-gray-500 disabled:opacity-20 disabled:cursor-not-allowed rounded-2xl hover:bg-gray-50 active:bg-gray-100 transition-colors cursor-pointer"
        >
          <svg className="w-9 h-9" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M15 18l-6-6 6-6"/>
          </svg>
        </button>

        <div className="flex items-center gap-1" role="tablist" aria-label="Task list">
          {tasks.map((task, i) => (
            <button
              key={i}
              onClick={() => onIndexChange(i)}
              role="tab"
              aria-selected={i === currentIndex}
              aria-label={`Task ${i + 1} of ${tasks.length}${completedIds.includes(task.id) ? ' - completed' : ''}`}
              title={`Go to task ${i + 1}`}
              className="min-h-[44px] min-w-[44px] flex items-center justify-center cursor-pointer focus:outline-none focus-visible:ring-2 focus-visible:ring-orange-400 rounded-full"
            >
              <div
                className={`rounded-full transition-all duration-300 ${
                  i === currentIndex
                    ? 'w-4 h-2.5 bg-orange-400'
                    : completedIds.includes(task.id)
                    ? 'w-2.5 h-2.5 bg-emerald-400'
                    : 'w-2.5 h-2.5 bg-gray-200'
                }`}
                aria-hidden="true"
              />
            </button>
          ))}
        </div>

        <button
          onClick={goNext}
          disabled={currentIndex === tasks.length - 1}
          aria-label="Next task"
          title="Next task (or press right arrow)"
          className="min-h-[64px] min-w-[64px] flex items-center justify-center text-gray-500 disabled:opacity-20 disabled:cursor-not-allowed rounded-2xl hover:bg-gray-50 active:bg-gray-100 transition-colors cursor-pointer"
        >
          <svg className="w-9 h-9" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M9 18l6-6-6-6"/>
          </svg>
        </button>
      </nav>
    </div>
  );
}
