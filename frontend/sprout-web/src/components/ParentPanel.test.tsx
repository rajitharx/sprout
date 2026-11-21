import { describe, it, expect, vi } from 'vitest';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ParentPanel } from './ParentPanel';

function setup(onCreate = vi.fn().mockResolvedValue({})) {
  render(
    <ParentPanel
      tasks={[{ id: '1', label: 'Brush teeth', emoji: '🪥', sortOrder: 0, isActive: true }]}
      profile={{ name: 'Sam', avatar: '🐻' }}
      onBack={vi.fn()}
      onCreate={onCreate}
      onUpdate={vi.fn()}
      onDelete={vi.fn()}
      onUpdateProfile={vi.fn()}
    />,
  );
  return onCreate;
}

describe('ParentPanel add-task sheet', () => {
  it('keeps the form hidden until Add Task is tapped', () => {
    setup();
    expect(screen.queryByRole('dialog')).toBeNull();
    expect(screen.getByText('Brush teeth')).toBeTruthy();
  });

  it('opens the sheet, creates a task and closes', async () => {
    const user = userEvent.setup();
    const onCreate = setup();
    await user.click(screen.getByRole('button', { name: '+ Add Task' }));
    await user.type(screen.getByLabelText(/Task name/), 'Wash hands');
    await user.click(within(screen.getByRole('dialog')).getByRole('button', { name: '+ Add Task' }));
    await waitFor(() => expect(onCreate).toHaveBeenCalledWith(expect.objectContaining({ label: 'Wash hands' })));
    await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull());
  });

  it('closes via cancel without creating', async () => {
    const user = userEvent.setup();
    const onCreate = setup();
    await user.click(screen.getByRole('button', { name: '+ Add Task' }));
    await user.click(screen.getByRole('button', { name: 'Cancel adding task' }));
    expect(screen.queryByRole('dialog')).toBeNull();
    expect(onCreate).not.toHaveBeenCalled();
  });
});
