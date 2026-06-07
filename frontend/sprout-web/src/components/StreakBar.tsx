import type { DailyProgress } from '../types';

const DAY_LABELS = ['M', 'T', 'W', 'T', 'F', 'S', 'S'];

interface Props {
  week: DailyProgress[];
}

export function StreakBar({ week }: Props) {
  const today = new Date().toISOString().slice(0, 10);

  return (
    <div className="flex items-center justify-center gap-2.5 px-4 py-3 h-16">
      {week.map((day, i) => {
        const isToday = day.date === today;
        const isFuture = day.date > today;
        const isDone = day.completedTaskIds.length > 0;

        let dotClass = '';
        if (isDone) {
          dotClass = 'bg-amber-400 shadow-sm';
        } else if (isToday) {
          dotClass = 'bg-white border-2 border-orange-400 animate-pulse-glow';
        } else if (isFuture) {
          dotClass = 'border-2 border-gray-200 bg-gray-50';
        } else {
          dotClass = 'bg-gray-200';
        }

        return (
          <div key={day.date} className="flex flex-col items-center gap-1">
            <div className={`w-8 h-8 rounded-full ${dotClass} flex items-center justify-center`}>
              {isDone && <span className="text-sm leading-none">⭐</span>}
            </div>
            <span className={`text-[11px] font-semibold ${isToday ? 'text-orange-500' : 'text-gray-400'}`}>
              {DAY_LABELS[i]}
            </span>
          </div>
        );
      })}
    </div>
  );
}
