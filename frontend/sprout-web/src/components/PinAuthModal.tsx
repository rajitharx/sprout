import { useState } from 'react';
import { api } from '../api/client';

interface Props {
  onSuccess: () => void;
}

export function PinAuthModal({ onSuccess }: Props) {
  const [pin, setPin] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isValidating, setIsValidating] = useState(false);

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      handleSubmit();
    }
  };

  const handleSubmit = async () => {
    if (pin.length === 0) {
      setError('Please enter a PIN');
      return;
    }

    setIsValidating(true);
    setError(null);
    try {
      const result = await api.validatePin(pin);
      if (result.valid) {
        onSuccess();
      } else {
        setError('Incorrect PIN');
        setPin('');
      }
    } catch (e) {
      setError('Error validating PIN. Please try again.');
      setPin('');
    } finally {
      setIsValidating(false);
    }
  };

  return (
    <div className="fixed inset-0 flex items-center justify-center bg-black/50 z-50">
      <div className="bg-white rounded-3xl p-6 shadow-lg w-80 space-y-6">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-800">Parent Access</h1>
          <p className="text-sm text-gray-600 mt-1">Enter PIN to access settings</p>
        </div>

        <div className="space-y-3">
          <input
            type="password"
            inputMode="numeric"
            value={pin}
            onChange={e => setPin(e.target.value.replace(/[^0-9]/g, ''))}
            onKeyDown={handleKeyDown}
            placeholder="Enter PIN"
            maxLength={6}
            disabled={isValidating}
            className="w-full text-center text-3xl tracking-widest border-2 border-gray-200 rounded-xl px-4 py-3 focus:outline-none focus:border-orange-400 focus:ring-2 focus:ring-orange-200 disabled:opacity-50 bg-white"
          />
          {error && (
            <p className="text-sm text-red-500 font-medium text-center">{error}</p>
          )}
        </div>

        <button
          onClick={handleSubmit}
          disabled={isValidating || pin.length === 0}
          className="w-full py-3 bg-orange-400 text-white font-bold text-lg rounded-xl active:scale-95 transition-transform disabled:opacity-50 cursor-pointer"
        >
          {isValidating ? 'Validating...' : 'Unlock'}
        </button>
      </div>
    </div>
  );
}
