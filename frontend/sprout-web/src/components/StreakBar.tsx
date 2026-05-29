import type { DailyProgress } from '../types';

const DAY_LABELS = ['M', 'T', 'W', 'T', 'F', 'S', 'S'];

interface Props {
  week: DailyProgress[];
}

export function StreakBar({ week }: Props) {
  const today = new Date().toISOString().slice(0, 10);
  const todayDayIndex = (new Date().getDay() + 6) % 7; // Mon=0

  return (
    <div className="flex items-center justify-center gap-2 px-4 py-2 h-12">
      {week.map((day, i) => {
        const isToday = day.date === today;
        const isFuture = day.date > today;
        const isDone = day.completedTaskIds.length > 0;
        const dayOfWeek = DAY_LABELS[(todayDayIndex - (6 - i) + 7) % 7];

        let dotClass = '';
        if (isDone) {
          dotClass = 'bg-yellow-400 text-yellow-900';
        } else if (isToday) {
          dotClass = 'border-2 border-yellow-400 animate-pulse-glow';
        } else if (isFuture) {
          dotClass = 'border border-gray-200';
        } else {
          dotClass = 'bg-gray-200';
        }

        return (
          <div key={day.date} className="flex flex-col items-center gap-0.5">
            <div className={`w-6 h-6 rounded-full ${dotClass} flex items-center justify-center`}>
              {isDone && <span className="text-[10px]">⭐</span>}
            </div>
            <span className="text-[10px] text-gray-400 font-medium">{dayOfWeek}</span>
          </div>
        );
      })}
    </div>
  );
}
