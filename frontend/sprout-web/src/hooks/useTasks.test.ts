import { beforeEach, describe, expect, it, vi } from 'vitest';
import { act, renderHook, waitFor } from '@testing-library/react';
import { useTasks } from './useTasks';
import { api, ApiError } from '../api/client';
import type { HabitTask } from '../types';

vi.mock('../api/client', async importOriginal => {
  const actual = await importOriginal<typeof import('../api/client')>();
  return {
    ...actual,
    api: {
      getTasks: vi.fn(),
      createTask: vi.fn(),
      updateTask: vi.fn(),
      deleteTask: vi.fn(),
    },
  };
});

const task = (id: string, sortOrder = 0): HabitTask => ({
  id,
  label: `Task ${id}`,
  emoji: '⭐',
  sortOrder,
  isActive: true,
});

const mocked = vi.mocked(api);

describe('useTasks', () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('loads tasks and caches them', async () => {
    mocked.getTasks.mockResolvedValue([task('a')]);

    const { result } = renderHook(() => useTasks());
    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.tasks).toEqual([task('a')]);
    expect(JSON.parse(localStorage.getItem('sprout_tasks_cache')!)).toEqual([task('a')]);
    expect(result.current.isOffline).toBe(false);
  });

  it('falls back to cached tasks when the API fails', async () => {
    localStorage.setItem('sprout_tasks_cache', JSON.stringify([task('cached')]));
    mocked.getTasks.mockRejectedValue(new Error('down'));

    const { result } = renderHook(() => useTasks());
    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.tasks).toEqual([task('cached')]);
    expect(result.current.isOffline).toBe(false);
  });

  it('flags offline when the fallback follows a timeout', async () => {
    localStorage.setItem('sprout_tasks_cache', JSON.stringify([task('cached')]));
    mocked.getTasks.mockRejectedValue(new ApiError('timeout', 0, 'TIMEOUT'));

    const { result } = renderHook(() => useTasks());
    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.isOffline).toBe(true);
  });

  it('ends with an empty list and stops loading when API and cache are both unavailable', async () => {
    mocked.getTasks.mockRejectedValue(new Error('down'));

    const { result } = renderHook(() => useTasks());
    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.tasks).toEqual([]);
  });

  it('createTask rejects blank label or emoji without calling the API', async () => {
    mocked.getTasks.mockResolvedValue([]);
    const { result } = renderHook(() => useTasks());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await expect(
      result.current.createTask({ label: '  ', emoji: '⭐', sortOrder: 0, isActive: true }),
    ).rejects.toThrow('Task name and emoji are required');
    await expect(
      result.current.createTask({ label: 'x', emoji: '', sortOrder: 0, isActive: true }),
    ).rejects.toThrow('Task name and emoji are required');
    expect(mocked.createTask).not.toHaveBeenCalled();
  });

  it('createTask inserts in sortOrder and updates the cache', async () => {
    mocked.getTasks.mockResolvedValue([task('a', 0), task('c', 2)]);
    mocked.createTask.mockResolvedValue(task('b', 1));
    const { result } = renderHook(() => useTasks());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await act(() => result.current.createTask({ label: 'b', emoji: '⭐', sortOrder: 1, isActive: true }));

    expect(result.current.tasks.map(t => t.id)).toEqual(['a', 'b', 'c']);
    expect(JSON.parse(localStorage.getItem('sprout_tasks_cache')!).map((t: HabitTask) => t.id)).toEqual([
      'a',
      'b',
      'c',
    ]);
  });

  it('updateTask replaces the task and re-sorts', async () => {
    mocked.getTasks.mockResolvedValue([task('a', 0), task('b', 1)]);
    mocked.updateTask.mockResolvedValue({ ...task('a'), sortOrder: 5, label: 'Renamed' });
    const { result } = renderHook(() => useTasks());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await act(() => result.current.updateTask('a', task('a')));

    expect(result.current.tasks.map(t => t.id)).toEqual(['b', 'a']);
    expect(result.current.tasks[1].label).toBe('Renamed');
  });

  it('updateTask rejects blank fields without calling the API', async () => {
    mocked.getTasks.mockResolvedValue([task('a')]);
    const { result } = renderHook(() => useTasks());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await expect(result.current.updateTask('a', { ...task('a'), label: '' })).rejects.toThrow();
    expect(mocked.updateTask).not.toHaveBeenCalled();
  });

  it('deleteTask removes the task locally and from the cache', async () => {
    mocked.getTasks.mockResolvedValue([task('a'), task('b')]);
    mocked.deleteTask.mockResolvedValue(undefined);
    const { result } = renderHook(() => useTasks());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await act(() => result.current.deleteTask('a'));

    expect(result.current.tasks.map(t => t.id)).toEqual(['b']);
    expect(JSON.parse(localStorage.getItem('sprout_tasks_cache')!)).toHaveLength(1);
  });

  it('deleteTask keeps the task when the API fails', async () => {
    mocked.getTasks.mockResolvedValue([task('a')]);
    mocked.deleteTask.mockRejectedValue(new ApiError('nope', 500));
    const { result } = renderHook(() => useTasks());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await expect(result.current.deleteTask('a')).rejects.toThrow('nope');
    expect(result.current.tasks).toHaveLength(1);
  });
});
