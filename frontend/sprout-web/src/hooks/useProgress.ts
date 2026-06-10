import { useState, useEffect, useCallback, useRef } from 'react';
import { api, ApiError } from '../api/client';
import type { DailyProgress } from '../types';

export function useProgress(onTaskComplete?: (taskEmoji?: string, isAllDone?: boolean) => void) {
  const [today, setToday] = useState<DailyProgress | null>(null);
  const [week, setWeek] = useState<DailyProgress[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const prevCompletedCount = useRef(0);
  const markingRef = useRef<Set<string>>(new Set());

  const fetchAll = useCallback(async () => {
    try {
      const [t, w] = await Promise.all([api.getToday(), api.getWeek()]);
      setToday(t);
      setWeek(w);
      return t;
    } catch (e) {
      console.error('Failed to fetch progress:', e);
      return null;
    }
  }, []);

  useEffect(() => {
    fetchAll().finally(() => setIsLoading(false));
    const interval = setInterval(() => fetchAll().catch(() => {}), 60_000);
    return () => clearInterval(interval);
  }, [fetchAll]);

  const markComplete = useCallback(
    async (taskId: string, taskIds: string[], taskEmoji?: string) => {
      if (markingRef.current.has(taskId)) {
        throw new Error('Already marking this task');
      }

      markingRef.current.add(taskId);
      const previousState = today;

      try {
        setToday(prev => {
          if (!prev) return prev;
          if (prev.completedTaskIds.includes(taskId)) return prev;
          const next = { ...prev, completedTaskIds: [...prev.completedTaskIds, taskId] };
          const activeCompleted = next.completedTaskIds.filter(id => taskIds.includes(id));
          const isAllDone = activeCompleted.length === taskIds.length;
          if (onTaskComplete) {
            onTaskComplete(taskEmoji, isAllDone);
          }
          prevCompletedCount.current = activeCompleted.length;
          return next;
        });

        const updated = await api.markComplete(taskId);
        setToday(updated);
      } catch (error) {
        setToday(previousState);
        const message =
          error instanceof ApiError
            ? error.code === 'TIMEOUT'
              ? 'Connection timeout — try again'
              : error.message
            : 'Could not save — try again';
        throw new Error(message);
      } finally {
        markingRef.current.delete(taskId);
      }
    },
    [today, onTaskComplete],
  );

  const markIncomplete = useCallback(async (taskId: string) => {
    if (markingRef.current.has(taskId)) {
      throw new Error('Already marking this task');
    }

    markingRef.current.add(taskId);
    const previousState = today;

    try {
      setToday(prev =>
        prev
          ? { ...prev, completedTaskIds: prev.completedTaskIds.filter(id => id !== taskId) }
          : prev,
      );

      const updated = await api.markIncomplete(taskId);
      setToday(updated);
    } catch (error) {
      setToday(previousState);
      const message =
        error instanceof ApiError
          ? error.code === 'TIMEOUT'
            ? 'Connection timeout — try again'
            : error.message
          : 'Could not save — try again';
      throw new Error(message);
    } finally {
      markingRef.current.delete(taskId);
    }
  }, [today]);

  return { today, week, isLoading, markComplete, markIncomplete, markingIds: markingRef.current };
}
