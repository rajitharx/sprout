import type { HabitTask, DailyProgress } from '../types';

async function json<T>(res: Response): Promise<T> {
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
  return res.json() as Promise<T>;
}

export const api = {
  getTasks: () =>
    fetch('/api/tasks').then(r => json<HabitTask[]>(r)),

  createTask: (task: Omit<HabitTask, 'id'>) =>
    fetch('/api/tasks', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(task),
    }).then(r => json<HabitTask>(r)),

  updateTask: (id: string, task: HabitTask) =>
    fetch(`/api/tasks/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(task),
    }).then(r => json<HabitTask>(r)),

  deleteTask: (id: string) =>
    fetch(`/api/tasks/${id}`, { method: 'DELETE' }).then(r => {
      if (!r.ok) throw new Error(`${r.status} ${r.statusText}`);
    }),

  getToday: () =>
    fetch('/api/progress/today').then(r => json<DailyProgress>(r)),

  markComplete: (taskId: string) =>
    fetch(`/api/progress/complete/${taskId}`, { method: 'POST' }).then(r =>
      json<DailyProgress>(r),
    ),

  markIncomplete: (taskId: string) =>
    fetch(`/api/progress/incomplete/${taskId}`, { method: 'POST' }).then(r =>
      json<DailyProgress>(r),
    ),

  getWeek: () =>
    fetch('/api/progress/week').then(r => json<DailyProgress[]>(r)),
};
