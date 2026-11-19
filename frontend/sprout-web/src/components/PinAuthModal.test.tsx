import { beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { PinAuthModal } from './PinAuthModal';
import { api } from '../api/client';

vi.mock('../api/client', () => ({ api: { validatePin: vi.fn() } }));

const validatePin = vi.mocked(api.validatePin);
const input = () => screen.getByLabelText('PIN code');
const unlock = () => screen.getByRole('button', { name: 'Unlock' });

describe('PinAuthModal', () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('disables Unlock until a PIN is typed', async () => {
    render(<PinAuthModal onSuccess={() => {}} />);
    expect(unlock()).toBeDisabled();

    await userEvent.type(input(), '1');

    expect(unlock()).toBeEnabled();
  });

  it('strips non-digit characters', async () => {
    render(<PinAuthModal onSuccess={() => {}} />);

    await userEvent.type(input(), '1a2b3');

    expect(input()).toHaveValue('123');
  });

  it('limits the PIN to six digits', async () => {
    render(<PinAuthModal onSuccess={() => {}} />);

    await userEvent.type(input(), '12345678');

    expect(input()).toHaveValue('123456');
  });

  it('calls onSuccess when the PIN is valid', async () => {
    validatePin.mockResolvedValue({ valid: true });
    const onSuccess = vi.fn();
    render(<PinAuthModal onSuccess={onSuccess} />);

    await userEvent.type(input(), '1234');
    await userEvent.click(unlock());

    expect(validatePin).toHaveBeenCalledWith('1234');
    await waitFor(() => expect(onSuccess).toHaveBeenCalledTimes(1));
  });

  it('submits on Enter', async () => {
    validatePin.mockResolvedValue({ valid: true });
    const onSuccess = vi.fn();
    render(<PinAuthModal onSuccess={onSuccess} />);

    await userEvent.type(input(), '1234{Enter}');

    await waitFor(() => expect(onSuccess).toHaveBeenCalled());
  });

  it('shows an error and clears the field when the PIN is wrong', async () => {
    validatePin.mockResolvedValue({ valid: false });
    const onSuccess = vi.fn();
    render(<PinAuthModal onSuccess={onSuccess} />);

    await userEvent.type(input(), '0000');
    await userEvent.click(unlock());

    expect(await screen.findByRole('alert')).toHaveTextContent('Incorrect PIN');
    expect(input()).toHaveValue('');
    expect(onSuccess).not.toHaveBeenCalled();
  });

  it('shows a retry message when validation fails', async () => {
    validatePin.mockRejectedValue(new Error('network'));
    const onSuccess = vi.fn();
    render(<PinAuthModal onSuccess={onSuccess} />);

    await userEvent.type(input(), '1234');
    await userEvent.click(unlock());

    expect(await screen.findByRole('alert')).toHaveTextContent('Error validating PIN. Please try again.');
    expect(onSuccess).not.toHaveBeenCalled();
  });

  it('blocks input and shows progress while validating', async () => {
    validatePin.mockReturnValue(new Promise(() => {}));
    render(<PinAuthModal onSuccess={() => {}} />);

    await userEvent.type(input(), '1234');
    await userEvent.click(unlock());

    expect(await screen.findByRole('button', { name: 'Validating...' })).toBeDisabled();
    expect(input()).toBeDisabled();
  });
});
