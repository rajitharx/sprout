import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import { TaskCard } from './TaskCard';
import type { HabitTask } from '../types';

const task: HabitTask = { id: '1', label: 'Brush Teeth', emoji: '🪥', sortOrder: 0, isActive: true };

describe('TaskCard', () => {
  it('shows the label and emoji', () => {
    render(<TaskCard task={task} completed={false} gradientIndex={0} isActive />);

    expect(screen.getByRole('img', { name: 'Brush Teeth' })).toBeInTheDocument();
    expect(screen.getByText('🪥')).toBeInTheDocument();
  });

  it('falls back to the clipboard emoji when the task has none', () => {
    render(<TaskCard task={{ ...task, emoji: '' }} completed={false} gradientIndex={0} isActive />);

    expect(screen.getByText('📋')).toBeInTheDocument();
  });

  it('announces completion to assistive tech', () => {
    render(<TaskCard task={task} completed gradientIndex={0} isActive />);

    expect(screen.getByRole('img', { name: 'Brush Teeth - completed' })).toBeInTheDocument();
    expect(screen.getByText('This task is completed')).toBeInTheDocument();
  });

  it('floats only while active and incomplete', () => {
    const { rerender } = render(<TaskCard task={task} completed={false} gradientIndex={0} isActive />);
    expect(screen.getByText('🪥')).toHaveClass('animate-float');

    rerender(<TaskCard task={task} completed gradientIndex={0} isActive />);
    expect(screen.getByText('🪥')).toHaveClass('animate-float-paused');

    rerender(<TaskCard task={task} completed={false} gradientIndex={0} isActive={false} />);
    expect(screen.getByText('🪥')).toHaveClass('animate-float-paused');
  });

  it('wraps the gradient index instead of throwing for large indexes', () => {
    render(<TaskCard task={task} completed={false} gradientIndex={13} isActive />);

    expect(screen.getByRole('img', { name: 'Brush Teeth' })).toHaveClass('from-amber-200');
  });
});
