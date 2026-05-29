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
      className={`h-full w-full flex flex-col items-center justify-center rounded-3xl bg-gradient-to-br ${gradient} relative overflow-hidden select-none`}
    >
      {completed && (
        <div className="absolute inset-0 bg-green-400/20 rounded-3xl flex items-center justify-center">
          <span className="text-[64px]">✅</span>
        </div>
      )}
      <span
        className={`text-[96px] leading-none mb-4 ${isActive && !completed ? 'animate-float' : 'animate-float-paused'}`}
      >
        {emoji}
      </span>
      <span className="text-xl font-semibold text-gray-600 text-center px-6">
        {task.label}
      </span>
    </div>
  );
}
