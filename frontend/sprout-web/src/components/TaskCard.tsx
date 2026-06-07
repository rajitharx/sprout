import type { HabitTask } from '../types';

const GRADIENTS = [
  'from-rose-200 to-pink-300',
  'from-amber-200 to-yellow-300',
  'from-sky-200 to-indigo-200',
  'from-emerald-200 to-teal-300',
  'from-orange-200 to-amber-300',
  'from-violet-200 to-purple-300',
];

interface Props {
  task: HabitTask;
  completed: boolean;
  gradientIndex: number;
  isActive: boolean;
}

export function TaskCard({ task, completed, gradientIndex, isActive }: Props) {
  const gradient = GRADIENTS[gradientIndex % GRADIENTS.length];
  const emoji = task.emoji || '📋';

  return (
    <div
      className={`h-full w-full flex flex-col items-center justify-center rounded-3xl bg-gradient-to-br ${gradient} relative overflow-visible select-none shadow-md`}
    >
      {completed && (
        <div className="absolute inset-0 bg-emerald-500/15 rounded-3xl pointer-events-none" />
      )}

      <div className="relative mb-6">
        <span
          className={`text-[112px] leading-none block drop-shadow-sm ${isActive && !completed ? 'animate-float' : 'animate-float-paused'}`}
        >
          {emoji}
        </span>
        {completed && (
          <span className="absolute -top-3 -right-4 text-[52px] drop-shadow-lg animate-celebration-pop">
            ✅
          </span>
        )}
      </div>

      <span className={`text-2xl font-bold text-center px-6 leading-snug ${completed ? 'text-emerald-800' : 'text-gray-700'}`}>
        {task.label}
      </span>
    </div>
  );
}
