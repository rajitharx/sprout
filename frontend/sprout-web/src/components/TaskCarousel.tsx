import { useRef } from 'react';
import type { HabitTask } from '../types';
import { TaskCard } from './TaskCard';

const SWIPE_THRESHOLD = 50;

interface Props {
  tasks: HabitTask[];
  completedIds: string[];
  currentIndex: number;
  onIndexChange: (index: number) => void;
}

export function TaskCarousel({ tasks, completedIds, currentIndex, onIndexChange }: Props) {
  const pointerStartX = useRef<number | null>(null);

  const goNext = () => {
    if (currentIndex < tasks.length - 1) onIndexChange(currentIndex + 1);
  };
  const goPrev = () => {
    if (currentIndex > 0) onIndexChange(currentIndex - 1);
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

  if (tasks.length === 0) {
    return (
      <div className="flex-1 flex flex-col items-center justify-center gap-4 text-gray-400">
        <span className="text-[80px]">🌱</span>
        <p className="text-lg font-medium">No tasks yet</p>
        <p className="text-sm">Ask a parent to add some!</p>
      </div>
    );
  }

  return (
    <div className="flex-1 flex flex-col overflow-hidden relative px-4 pb-2">
      <div
        className="flex-1 overflow-hidden relative cursor-grab active:cursor-grabbing"
        onPointerDown={handlePointerDown}
        onPointerUp={handlePointerUp}
      >
        <div
          className="flex h-full transition-transform duration-300 ease-in-out"
          style={{ transform: `translateX(calc(-${currentIndex * 100}% - ${currentIndex * 16}px))` }}
        >
          {tasks.map((task, i) => (
            <div key={task.id} className="flex-shrink-0 w-full h-full pr-4 last:pr-0">
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

      <div className="flex items-center justify-between mt-3 px-2">
        <button
          onClick={goPrev}
          disabled={currentIndex === 0}
          aria-label="Previous task"
          className="min-h-[64px] min-w-[64px] flex items-center justify-center text-3xl text-gray-400 disabled:opacity-20 rounded-2xl active:bg-gray-100"
        >
          ‹
        </button>

        <div className="flex gap-2">
          {tasks.map((_, i) => (
            <button
              key={i}
              onClick={() => onIndexChange(i)}
              aria-label={`Go to task ${i + 1}`}
              className={`w-2.5 h-2.5 rounded-full transition-colors ${
                i === currentIndex ? 'bg-orange-400' : 'bg-gray-200'
              }`}
            />
          ))}
        </div>

        <button
          onClick={goNext}
          disabled={currentIndex === tasks.length - 1}
          aria-label="Next task"
          className="min-h-[64px] min-w-[64px] flex items-center justify-center text-3xl text-gray-400 disabled:opacity-20 rounded-2xl active:bg-gray-100"
        >
          ›
        </button>
      </div>
    </div>
  );
}
