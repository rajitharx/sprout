export interface HabitTask {
  id: string;
  label: string;
  emoji: string;
  sortOrder: number;
  isActive: boolean;
}

export interface DailyProgress {
  date: string;
  completedTaskIds: string[];
  lastUpdated: string;
}

export interface ChildProfile {
  name: string;
  avatar: string;
}
