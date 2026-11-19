import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, renderHook, waitFor } from '@testing-library/react';
import { useProgress } from './useProgress';
import { api, ApiError } from '../api/client';
import type { DailyProgress } from '../types';

vi.mock('../api/client', async importOriginal => {
  const actual = await importOriginal<typeof import('../api/client')>();
  return {
    ...actual,
    api: { getToday: vi.fn(), getWeek: vi.fn(), markComplete: vi.fn(), markIncomplete: vi.fn() },
  };
});

const mocked = vi.mocked(api);
const day = (ids: string[] = []): DailyProgress => ({
  date: '2026-05-30',
  completedTaskIds: ids,
  lastUpdated: '2026-05-30T10:00:00Z',
});

async function setup(initial: DailyProgress = day(), onComplete?: (e?: string, all?: boolean) => void) {
  mocked.getToday.mockResolvedValue(initial);
  mocked.getWeek.mockResolvedValue([initial]);
  const hook = renderHook(() => useProgress(onComplete));
  await waitFor(() => expect(hook.result.current.isLoading).toBe(false));
  return hook;
}

describe('useProgress', () => {
  beforeEach(() => {
    vi.resetAllMocks();
    vi.spyOn(console, 'error').mockImplementation(() => {});
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('loads today and the week', async () => {
    const { result } = await setup(day(['a']));

    expect(result.current.today?.completedTaskIds).toEqual(['a']);
    expect(result.current.week).toHaveLength(1);
  });

  it('stops loading and leaves today null when the API fails, so the child view shows all incomplete', async () => {
    mocked.getToday.mockRejectedValue(new Error('down'));
    mocked.getWeek.mockRejectedValue(new Error('down'));

    const { result } = renderHook(() => useProgress());
    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.today).toBeNull();
    expect(result.current.week).toEqual([]);
  });

  it('refreshes every 60 seconds', async () => {
    vi.useFakeTimers();
    mocked.getToday.mockResolvedValue(day());
    mocked.getWeek.mockResolvedValue([day()]);
    renderHook(() => useProgress());
    await vi.advanceTimersByTimeAsync(0);
    const initialCalls = mocked.getToday.mock.calls.length;

    await vi.advanceTimersByTimeAsync(60_000);

    expect(mocked.getToday.mock.calls.length).toBe(initialCalls + 1);
  });

  it('markComplete applies the server response', async () => {
    const { result } = await setup(day());
    mocked.markComplete.mockResolvedValue(day(['a']));

    await act(() => result.current.markComplete('a', ['a', 'b'], '⭐'));

    expect(result.current.today?.completedTaskIds).toEqual(['a']);
  });

  it('markComplete notifies with isAllDone=false while tasks remain', async () => {
    const onComplete = vi.fn();
    const { result } = await setup(day(), onComplete);
    mocked.markComplete.mockResolvedValue(day(['a']));

    await act(() => result.current.markComplete('a', ['a', 'b'], '⭐'));

    expect(onComplete).toHaveBeenCalledWith('⭐', false);
  });

  it('markComplete notifies with isAllDone=true on the last task', async () => {
    const onComplete = vi.fn();
    const { result } = await setup(day(['a']), onComplete);
    mocked.markComplete.mockResolvedValue(day(['a', 'b']));

    await act(() => result.current.markComplete('b', ['a', 'b'], '🎉'));

    expect(onComplete).toHaveBeenCalledWith('🎉', true);
  });

  it('markComplete rolls back and reports a friendly message when saving fails', async () => {
    const { result } = await setup(day());
    mocked.markComplete.mockRejectedValue(new Error('boom'));

    await act(async () => {
      await expect(result.current.markComplete('a', ['a'])).rejects.toThrow('Could not save — try again');
    });

    expect(result.current.today?.completedTaskIds).toEqual([]);
  });

  it('markComplete reports a timeout message for TIMEOUT ApiErrors', async () => {
    const { result } = await setup(day());
    mocked.markComplete.mockRejectedValue(new ApiError('slow', 0, 'TIMEOUT'));

    await act(async () => {
      await expect(result.current.markComplete('a', ['a'])).rejects.toThrow('Connection timeout — try again');
    });
  });

  it('markComplete passes through other ApiError messages', async () => {
    const { result } = await setup(day());
    mocked.markComplete.mockRejectedValue(new ApiError('Server said no', 500, 'INTERNAL_ERROR'));

    await act(async () => {
      await expect(result.current.markComplete('a', ['a'])).rejects.toThrow('Server said no');
    });
  });

  it('rejects a second tap on the same task while the first is in flight', async () => {
    const { result } = await setup(day());
    let resolve!: (d: DailyProgress) => void;
    mocked.markComplete.mockReturnValue(new Promise(r => (resolve = r)));

    let first!: Promise<void>;
    act(() => {
      first = result.current.markComplete('a', ['a']);
    });

    await expect(result.current.markComplete('a', ['a'])).rejects.toThrow('Already marking this task');
    await act(async () => {
      resolve(day(['a']));
      await first;
    });
    expect(mocked.markComplete).toHaveBeenCalledTimes(1);
  });

  it('allows retrying a task after a failed attempt', async () => {
    const { result } = await setup(day());
    mocked.markComplete.mockRejectedValueOnce(new Error('boom')).mockResolvedValueOnce(day(['a']));

    await act(async () => {
      await result.current.markComplete('a', ['a']).catch(() => {});
    });
    await act(() => result.current.markComplete('a', ['a']));

    expect(result.current.today?.completedTaskIds).toEqual(['a']);
  });

  it('markIncomplete applies the server response', async () => {
    const { result } = await setup(day(['a', 'b']));
    mocked.markIncomplete.mockResolvedValue(day(['b']));

    await act(() => result.current.markIncomplete('a'));

    expect(result.current.today?.completedTaskIds).toEqual(['b']);
  });

  it('markIncomplete rolls back on failure', async () => {
    const { result } = await setup(day(['a']));
    mocked.markIncomplete.mockRejectedValue(new Error('boom'));

    await act(async () => {
      await expect(result.current.markIncomplete('a')).rejects.toThrow('Could not save — try again');
    });

    expect(result.current.today?.completedTaskIds).toEqual(['a']);
  });
});
