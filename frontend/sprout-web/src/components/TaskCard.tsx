import type { HabitTask } from '../types';

const GRADIENTS = [
  'from-pink-100 to-orange-100',
  'from-yellow-100 to-green-100',
  'from-blue-100 to-purple-100',
  'from-green-100 to-teal-100',
  'from-orange-100 to-red-100',
  'from-purple-100 to-pink-100',
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
      className={`h-full w-full flex flex-col items-center justify-center rounded-3xl bg-gradient-to-br ${gradient} relative overflow-visible select-none`}
    >
      <div className="relative mb-4">
        <span
          className={`text-[96px] leading-none block ${isActive && !completed ? 'animate-float' : 'animate-float-paused'}`}
        >
          {emoji}
        </span>
        {completed && (
          <span className="absolute -top-2 -right-2 text-[48px] drop-shadow-lg">✅</span>
        )}
      </div>
      {completed && (
        <div className="absolute inset-0 bg-green-400/10 rounded-3xl pointer-events-none" />
      )}
      <span className="text-xl font-semibold text-gray-600 text-center px-6">
        {task.label}
      </span>
    </div>
  );
}
