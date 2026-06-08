import { useState, useEffect, useCallback } from 'react';
import { api, ApiError } from '../api/client';
import type { ChildProfile } from '../types';

const CACHE_KEY = 'sprout_profile_cache';
const DEFAULT_PROFILE: ChildProfile = { name: 'Child', avatar: '👦' };

export function useProfile() {
  const [profile, setProfile] = useState<ChildProfile>(DEFAULT_PROFILE);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.getProfile()
      .then(data => {
        if (data?.name?.trim() && data?.avatar?.trim()) {
          setProfile(data);
          localStorage.setItem(CACHE_KEY, JSON.stringify(data));
        }
      })
      .catch(() => {
        const cached = localStorage.getItem(CACHE_KEY);
        if (cached) {
          try {
            const parsed = JSON.parse(cached) as ChildProfile;
            if (parsed?.name?.trim() && parsed?.avatar?.trim()) {
              setProfile(parsed);
            }
          } catch (e) {
            console.error('Failed to parse cached profile:', e);
          }
        }
      })
      .finally(() => setLoading(false));
  }, []);

  const updateProfile = useCallback(async (newProfile: ChildProfile) => {
    if (!newProfile.name?.trim()) {
      throw new Error('Child name is required');
    }
    if (!newProfile.avatar?.trim()) {
      throw new Error('Avatar emoji is required');
    }

    const previousProfile = profile;
    try {
      setProfile(newProfile);
      const updated = await api.updateProfile(newProfile);
      if (updated?.name?.trim() && updated?.avatar?.trim()) {
        setProfile(updated);
        localStorage.setItem(CACHE_KEY, JSON.stringify(updated));
        return updated;
      }
      throw new Error('Invalid response from server');
    } catch (error) {
      setProfile(previousProfile);
      const message =
        error instanceof ApiError
          ? error.code === 'TIMEOUT'
            ? 'Connection timeout — try again'
            : error.message
          : error instanceof Error
            ? error.message
            : 'Failed to update profile';
      throw new Error(message);
    }
  }, [profile]);

  return { profile, loading, updateProfile };
}
