import { useState, useEffect, useCallback } from 'react';
import { api, ApiError } from '../api/client';
import type { HabitTask } from '../types';

const CACHE_KEY = 'sprout_tasks_cache';

export function useTasks() {
  const [tasks, setTasks] = useState<HabitTask[]>([]);
  const [loading, setLoading] = useState(true);
  const [isOffline, setIsOffline] = useState(false);

  useEffect(() => {
    api.getTasks()
      .then(data => {
        setTasks(data);
        localStorage.setItem(CACHE_KEY, JSON.stringify(data));
        setIsOffline(false);
      })
      .catch(error => {
        const cached = localStorage.getItem(CACHE_KEY);
        if (cached) {
          setTasks(JSON.parse(cached) as HabitTask[]);
          if (error instanceof ApiError && error.code === 'TIMEOUT') {
            setIsOffline(true);
          }
        }
      })
      .finally(() => setLoading(false));
  }, []);

  const createTask = useCallback(
    async (task: Omit<HabitTask, 'id'>) => {
      if (!task.label?.trim() || !task.emoji?.trim()) {
        throw new Error('Task name and emoji are required');
      }
      const created = await api.createTask(task);
      setTasks(prev => {
        const next = [...prev, created].sort((a, b) => a.sortOrder - b.sortOrder);
        localStorage.setItem(CACHE_KEY, JSON.stringify(next));
        return next;
      });
      return created;
    },
    [],
  );

  const updateTask = useCallback(
    async (id: string, task: HabitTask) => {
      if (!task.label?.trim() || !task.emoji?.trim()) {
        throw new Error('Task name and emoji are required');
      }
      const updated = await api.updateTask(id, task);
      setTasks(prev => {
        const next = prev
          .map(t => (t.id === id ? updated : t))
          .sort((a, b) => a.sortOrder - b.sortOrder);
        localStorage.setItem(CACHE_KEY, JSON.stringify(next));
        return next;
      });
      return updated;
    },
    [],
  );

  const deleteTask = useCallback(async (id: string) => {
    await api.deleteTask(id);
    setTasks(prev => {
      const next = prev.filter(t => t.id !== id);
      localStorage.setItem(CACHE_KEY, JSON.stringify(next));
      return next;
    });
  }, []);

  return { tasks, loading, isOffline, createTask, updateTask, deleteTask };
}
