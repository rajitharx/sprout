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
  const gradient = completed
    ? 'from-emerald-400 to-green-500'
    : GRADIENTS[gradientIndex % GRADIENTS.length];
  const emoji = task.emoji || '📋';

  return (
    <div
      className={`h-full w-full flex flex-col items-center justify-center rounded-3xl bg-gradient-to-br ${gradient} relative overflow-visible select-none shadow-md`}
    >
      <div className="mb-6">
        <span
          className={`text-[112px] leading-none block drop-shadow-sm ${isActive && !completed ? 'animate-float' : 'animate-float-paused'}`}
        >
          {emoji}
        </span>
      </div>

      <span className={`text-2xl font-bold text-center px-6 leading-snug ${completed ? 'text-emerald-900' : 'text-gray-700'}`}>
        {task.label}
      </span>
    </div>
  );
}
