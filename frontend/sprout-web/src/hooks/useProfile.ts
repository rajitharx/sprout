import { useState, useEffect, useCallback } from 'react';
import { api } from '../api/client';
import type { ChildProfile } from '../types';

const CACHE_KEY = 'sprout_profile_cache';
const DEFAULT_PROFILE: ChildProfile = { name: 'Child', avatar: '👦' };

export function useProfile() {
  const [profile, setProfile] = useState<ChildProfile>(DEFAULT_PROFILE);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.getProfile()
      .then(data => {
        setProfile(data);
        localStorage.setItem(CACHE_KEY, JSON.stringify(data));
      })
      .catch(() => {
        const cached = localStorage.getItem(CACHE_KEY);
        if (cached) setProfile(JSON.parse(cached) as ChildProfile);
      })
      .finally(() => setLoading(false));
  }, []);

  const updateProfile = useCallback(async (newProfile: ChildProfile) => {
    const updated = await api.updateProfile(newProfile);
    setProfile(updated);
    localStorage.setItem(CACHE_KEY, JSON.stringify(updated));
    return updated;
  }, []);

  return { profile, loading, updateProfile };
}
