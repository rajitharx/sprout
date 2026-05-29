import { useState } from 'react';
import type { HabitTask } from '../types';

const EMOJI_OPTIONS = [
  '🪥','🛁','🍎','🥛','👕','🧦','📚','🧸','🙏','🌙',
  '💊','🥦','🏃','🎨','🎵','🌿','💧','🐾','🛏','⭐',
];

interface Props {
  tasks: HabitTask[];
  onBack: () => void;
  onCreate: (task: Omit<HabitTask, 'id'>) => Promise<unknown>;
  onUpdate: (id: string, task: HabitTask) => Promise<unknown>;
  onDelete: (id: string) => Promise<void>;
}

export function ParentPanel({ tasks, onBack, onCreate, onUpdate, onDelete }: Props) {
  const [label, setLabel] = useState('');
  const [emoji, setEmoji] = useState('🪥');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editLabel, setEditLabel] = useState('');
  const [editEmoji, setEditEmoji] = useState('');
  const [busy, setBusy] = useState(false);

  const handleCreate = async () => {
    if (!label.trim()) return;
    setBusy(true);
    try {
      await onCreate({
        label: label.trim(),
        emoji,
        sortOrder: tasks.length,
        isActive: true,
      });
      setLabel('');
    } finally {
      setBusy(false);
    }
  };

  const startEdit = (task: HabitTask) => {
    setEditingId(task.id);
    setEditLabel(task.label);
    setEditEmoji(task.emoji);
  };

  const saveEdit = async (task: HabitTask) => {
    if (!editLabel.trim()) return;
    setBusy(true);
    try {
      await onUpdate(task.id, { ...task, label: editLabel.trim(), emoji: editEmoji });
      setEditingId(null);
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="h-[100dvh] flex flex-col bg-gray-50">
      <div className="flex items-center gap-3 px-4 py-4 bg-white border-b border-gray-100">
        <button
          onClick={onBack}
          className="min-h-[44px] min-w-[44px] flex items-center justify-center text-2xl rounded-xl active:bg-gray-100"
          aria-label="Back to child view"
        >
          ←
        </button>
        <h1 className="text-xl font-bold text-gray-800">Manage Tasks</h1>
      </div>

      <div className="flex-1 overflow-y-auto px-4 py-4 space-y-3">
        {tasks.map(task => (
          <div key={task.id} className="bg-white rounded-2xl p-4 shadow-sm">
            {editingId === task.id ? (
              <div className="space-y-3">
                <div className="flex gap-2">
                  <span className="text-3xl">{editEmoji}</span>
                  <input
                    value={editLabel}
                    onChange={e => setEditLabel(e.target.value)}
                    className="flex-1 border border-gray-200 rounded-xl px-3 py-2 text-base focus:outline-none focus:ring-2 focus:ring-orange-300"
                    placeholder="Task name"
                  />
                </div>
                <div className="flex flex-wrap gap-2">
                  {EMOJI_OPTIONS.map(e => (
                    <button
                      key={e}
                      onClick={() => setEditEmoji(e)}
                      className={`text-2xl p-1 rounded-lg ${editEmoji === e ? 'bg-orange-100 ring-2 ring-orange-300' : ''}`}
                    >
                      {e}
                    </button>
                  ))}
                </div>
                <div className="flex gap-2">
                  <button
                    onClick={() => saveEdit(task)}
                    disabled={busy}
                    className="flex-1 py-2 bg-orange-400 text-white font-semibold rounded-xl active:scale-95 transition-transform disabled:opacity-50"
                  >
                    Save
                  </button>
                  <button
                    onClick={() => setEditingId(null)}
                    className="flex-1 py-2 bg-gray-100 text-gray-600 font-semibold rounded-xl active:scale-95 transition-transform"
                  >
                    Cancel
                  </button>
                </div>
              </div>
            ) : (
              <div className="flex items-center gap-3">
                <span className="text-3xl">{task.emoji || '📋'}</span>
                <span className="flex-1 text-base font-medium text-gray-700">{task.label}</span>
                <button
                  onClick={() => startEdit(task)}
                  aria-label="Edit task"
                  className="min-h-[44px] min-w-[44px] flex items-center justify-center text-xl text-gray-400 rounded-xl active:bg-gray-100"
                >
                  ✏️
                </button>
                <button
                  onClick={() => onDelete(task.id)}
                  aria-label="Delete task"
                  className="min-h-[44px] min-w-[44px] flex items-center justify-center text-xl text-gray-400 rounded-xl active:bg-gray-100"
                >
                  🗑️
                </button>
              </div>
            )}
          </div>
        ))}

        {tasks.length === 0 && (
          <p className="text-center text-gray-400 py-8">No tasks yet. Add one below!</p>
        )}
      </div>

      <div className="bg-white border-t border-gray-100 px-4 py-4 space-y-3">
        <p className="text-sm font-semibold text-gray-500 uppercase tracking-wide">Add Task</p>
        <div className="flex gap-2">
          <span className="text-3xl">{emoji}</span>
          <input
            value={label}
            onChange={e => setLabel(e.target.value)}
            onKeyDown={e => e.key === 'Enter' && handleCreate()}
            placeholder="Task name (e.g. Brush teeth)"
            className="flex-1 border border-gray-200 rounded-xl px-3 py-2 text-base focus:outline-none focus:ring-2 focus:ring-orange-300"
          />
        </div>
        <div className="flex flex-wrap gap-2">
          {EMOJI_OPTIONS.map(e => (
            <button
              key={e}
              onClick={() => setEmoji(e)}
              className={`text-2xl p-1 rounded-lg ${emoji === e ? 'bg-orange-100 ring-2 ring-orange-300' : ''}`}
            >
              {e}
            </button>
          ))}
        </div>
        <button
          onClick={handleCreate}
          disabled={busy || !label.trim()}
          className="w-full py-3 bg-orange-400 text-white font-bold text-lg rounded-2xl active:scale-95 transition-transform disabled:opacity-50"
        >
          + Add Task
        </button>
      </div>
    </div>
  );
}
