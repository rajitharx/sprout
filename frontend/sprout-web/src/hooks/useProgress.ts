import { useState, useEffect, useCallback, useRef } from 'react';
import { api } from '../api/client';
import type { DailyProgress } from '../types';

export function useProgress(onAllComplete?: () => void) {
  const [today, setToday] = useState<DailyProgress | null>(null);
  const [week, setWeek] = useState<DailyProgress[]>([]);
  const prevCompletedCount = useRef(0);

  const fetchAll = useCallback(async () => {
    const [t, w] = await Promise.all([api.getToday(), api.getWeek()]);
    setToday(t);
    setWeek(w);
    return t;
  }, []);

  useEffect(() => {
    fetchAll().catch(() => {});
    const interval = setInterval(() => fetchAll().catch(() => {}), 60_000);
    return () => clearInterval(interval);
  }, [fetchAll]);

  const markComplete = useCallback(async (taskId: string, totalTasks: number) => {
    setToday(prev => {
      if (!prev) return prev;
      if (prev.completedTaskIds.includes(taskId)) return prev;
      const next = { ...prev, completedTaskIds: [...prev.completedTaskIds, taskId] };
      if (next.completedTaskIds.length === totalTasks && onAllComplete) {
        onAllComplete();
      }
      prevCompletedCount.current = next.completedTaskIds.length;
      return next;
    });
    try {
      const updated = await api.markComplete(taskId);
      setToday(updated);
    } catch {
      setToday(prev =>
        prev
          ? { ...prev, completedTaskIds: prev.completedTaskIds.filter(id => id !== taskId) }
          : prev,
      );
      throw new Error('Could not save — try again');
    }
  }, [onAllComplete]);

  const markIncomplete = useCallback(async (taskId: string) => {
    setToday(prev =>
      prev
        ? { ...prev, completedTaskIds: prev.completedTaskIds.filter(id => id !== taskId) }
        : prev,
    );
    try {
      const updated = await api.markIncomplete(taskId);
      setToday(updated);
    } catch {
      setToday(prev =>
        prev ? { ...prev, completedTaskIds: [...prev.completedTaskIds, taskId] } : prev,
      );
      throw new Error('Could not save — try again');
    }
  }, []);

  return { today, week, markComplete, markIncomplete };
}
