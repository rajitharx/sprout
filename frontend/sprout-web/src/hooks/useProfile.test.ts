import { beforeEach, describe, expect, it, vi } from 'vitest';
import { act, renderHook, waitFor } from '@testing-library/react';
import { useProfile } from './useProfile';
import { api, ApiError } from '../api/client';

vi.mock('../api/client', async importOriginal => {
  const actual = await importOriginal<typeof import('../api/client')>();
  return { ...actual, api: { getProfile: vi.fn(), updateProfile: vi.fn() } };
});

const mocked = vi.mocked(api);
const CACHE = 'sprout_profile_cache';

describe('useProfile', () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('starts with the default profile so the child view always renders', () => {
    mocked.getProfile.mockReturnValue(new Promise(() => {}));

    const { result } = renderHook(() => useProfile());

    expect(result.current.profile).toEqual({ name: 'Child', avatar: '👦' });
    expect(result.current.loading).toBe(true);
  });

  it('uses and caches the server profile', async () => {
    mocked.getProfile.mockResolvedValue({ name: 'Mila', avatar: '🐻' });

    const { result } = renderHook(() => useProfile());
    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.profile.name).toBe('Mila');
    expect(JSON.parse(localStorage.getItem(CACHE)!)).toEqual({ name: 'Mila', avatar: '🐻' });
  });

  it('ignores a server profile with a blank name', async () => {
    mocked.getProfile.mockResolvedValue({ name: ' ', avatar: '🐻' });

    const { result } = renderHook(() => useProfile());
    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.profile).toEqual({ name: 'Child', avatar: '👦' });
  });

  it('falls back to the cached profile when the API fails', async () => {
    localStorage.setItem(CACHE, JSON.stringify({ name: 'Cached', avatar: '🐼' }));
    mocked.getProfile.mockRejectedValue(new Error('down'));

    const { result } = renderHook(() => useProfile());
    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.profile.name).toBe('Cached');
  });

  it('keeps the default when the cache is corrupt', async () => {
    localStorage.setItem(CACHE, '{not json');
    vi.spyOn(console, 'error').mockImplementation(() => {});
    mocked.getProfile.mockRejectedValue(new Error('down'));

    const { result } = renderHook(() => useProfile());
    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.profile.name).toBe('Child');
  });

  it('keeps the default when the cached profile is incomplete', async () => {
    localStorage.setItem(CACHE, JSON.stringify({ name: 'NoAvatar', avatar: '' }));
    mocked.getProfile.mockRejectedValue(new Error('down'));

    const { result } = renderHook(() => useProfile());
    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.profile.name).toBe('Child');
  });

  it('updateProfile rejects blank name or avatar without calling the API', async () => {
    mocked.getProfile.mockResolvedValue({ name: 'Mila', avatar: '🐻' });
    const { result } = renderHook(() => useProfile());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await expect(result.current.updateProfile({ name: '', avatar: '🐻' })).rejects.toThrow('Child name is required');
    await expect(result.current.updateProfile({ name: 'x', avatar: ' ' })).rejects.toThrow('Avatar emoji is required');
    expect(mocked.updateProfile).not.toHaveBeenCalled();
  });

  it('updateProfile stores the server response and updates the cache', async () => {
    mocked.getProfile.mockResolvedValue({ name: 'Mila', avatar: '🐻' });
    mocked.updateProfile.mockResolvedValue({ name: 'Zed', avatar: '🦊' });
    const { result } = renderHook(() => useProfile());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await act(() => result.current.updateProfile({ name: 'Zed', avatar: '🦊' }));

    expect(result.current.profile).toEqual({ name: 'Zed', avatar: '🦊' });
    expect(JSON.parse(localStorage.getItem(CACHE)!).name).toBe('Zed');
  });

  it('updateProfile rolls back and surfaces a friendly message on timeout', async () => {
    mocked.getProfile.mockResolvedValue({ name: 'Mila', avatar: '🐻' });
    mocked.updateProfile.mockRejectedValue(new ApiError('slow', 0, 'TIMEOUT'));
    const { result } = renderHook(() => useProfile());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await act(async () => {
      await expect(result.current.updateProfile({ name: 'Zed', avatar: '🦊' })).rejects.toThrow(
        'Connection timeout — try again',
      );
    });

    expect(result.current.profile.name).toBe('Mila');
  });

  it('updateProfile rolls back when the server returns an invalid profile', async () => {
    mocked.getProfile.mockResolvedValue({ name: 'Mila', avatar: '🐻' });
    mocked.updateProfile.mockResolvedValue({ name: '', avatar: '' });
    const { result } = renderHook(() => useProfile());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await act(async () => {
      await expect(result.current.updateProfile({ name: 'Zed', avatar: '🦊' })).rejects.toThrow(
        'Invalid response from server',
      );
    });

    expect(result.current.profile.name).toBe('Mila');
  });
});
