import type { HabitTask, DailyProgress, ChildProfile } from '../types';

const API_TIMEOUT = 30_000; // 30 seconds

export class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
    public code?: string,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

async function fetchWithTimeout(input: RequestInfo, init?: RequestInit): Promise<Response> {
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), API_TIMEOUT);

  try {
    return await fetch(input, { ...init, signal: controller.signal });
  } catch (e) {
    if (e instanceof Error && e.name === 'AbortError') {
      throw new ApiError('Request timeout — check your connection', 0, 'TIMEOUT');
    }
    throw e;
  } finally {
    clearTimeout(timeoutId);
  }
}

async function json<T>(res: Response): Promise<T> {
  if (!res.ok) {
    const errorData = await res.json().catch(() => ({}));
    const isOffline = errorData?.offline === true;
    const code = isOffline ? 'OFFLINE' : errorData?.error?.code;
    const message = errorData?.error?.message || (isOffline ? 'Offline mode - using cached data' : `${res.status} ${res.statusText}`);
    throw new ApiError(message, res.status, code);
  }
  return res.json() as Promise<T>;
}

export const api = {
  getTasks: () =>
    fetchWithTimeout('/api/tasks').then(r => json<HabitTask[]>(r)),

  createTask: (task: Omit<HabitTask, 'id'>) =>
    fetchWithTimeout('/api/tasks', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(task),
    }).then(r => json<HabitTask>(r)),

  updateTask: (id: string, task: HabitTask) =>
    fetchWithTimeout(`/api/tasks/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(task),
    }).then(r => json<HabitTask>(r)),

  deleteTask: (id: string) =>
    fetchWithTimeout(`/api/tasks/${id}`, { method: 'DELETE' }).then(r => {
      if (!r.ok) throw new ApiError(`${r.status} ${r.statusText}`, r.status);
    }),

  getToday: () =>
    fetchWithTimeout('/api/progress/today').then(r => json<DailyProgress>(r)),

  markComplete: (taskId: string) =>
    fetchWithTimeout(`/api/progress/complete/${taskId}`, { method: 'POST' }).then(r =>
      json<DailyProgress>(r),
    ),

  markIncomplete: (taskId: string) =>
    fetchWithTimeout(`/api/progress/incomplete/${taskId}`, { method: 'POST' }).then(r =>
      json<DailyProgress>(r),
    ),

  getWeek: () =>
    fetchWithTimeout('/api/progress/week').then(r => json<DailyProgress[]>(r)),

  getProfile: () =>
    fetchWithTimeout('/api/profile').then(r => json<ChildProfile>(r)),

  updateProfile: (profile: ChildProfile) =>
    fetchWithTimeout('/api/profile', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(profile),
    }).then(r => json<ChildProfile>(r)),

  validatePin: (pin: string) =>
    fetchWithTimeout('/api/auth/validate-pin', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ pin }),
    }).then(r => json<{ valid: boolean }>(r)),
};
