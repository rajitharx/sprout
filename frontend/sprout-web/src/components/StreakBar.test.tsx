import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { StreakBar } from './StreakBar';
import type { DailyProgress } from '../types';

const DATES = [
  '2026-05-25',
  '2026-05-26',
  '2026-05-27',
  '2026-05-28',
  '2026-05-29',
  '2026-05-30',
  '2026-05-31',
];

const week = (done: string[] = []): DailyProgress[] =>
  DATES.map(date => ({
    date,
    completedTaskIds: done.includes(date) ? ['t'] : [],
    lastUpdated: '',
  }));

describe('StreakBar', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-05-28T12:00:00Z'));
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('renders seven days labelled Monday to Sunday', () => {
    render(<StreakBar week={week()} />);

    expect(screen.getAllByRole('img')).toHaveLength(7);
    expect(screen.getAllByText('M')).toHaveLength(1);
    expect(screen.getAllByText('T')).toHaveLength(2);
  });

  it('marks completed, today, missed and upcoming days', () => {
    render(<StreakBar week={week(['2026-05-25'])} />);

    expect(screen.getByLabelText('Monday - completed')).toBeInTheDocument();
    expect(screen.getByLabelText('Tuesday - not completed')).toBeInTheDocument();
    expect(screen.getByLabelText('Thursday - today')).toBeInTheDocument();
    expect(screen.getByLabelText('Friday - upcoming')).toBeInTheDocument();
  });

  it('shows a star on completed days only', () => {
    render(<StreakBar week={week(['2026-05-25', '2026-05-26'])} />);

    expect(screen.getAllByText('⭐')).toHaveLength(2);
  });

  it('shows today as completed once it has progress', () => {
    render(<StreakBar week={week(['2026-05-28'])} />);

    expect(screen.getByLabelText('Thursday - completed')).toBeInTheDocument();
  });

  it('renders an empty bar without crashing when there is no week data', () => {
    render(<StreakBar week={[]} />);

    expect(screen.getByRole('navigation', { name: 'Weekly progress' })).toBeInTheDocument();
    expect(screen.queryAllByRole('img')).toHaveLength(0);
  });
});
