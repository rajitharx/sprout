import { useState } from 'react';
import type { HabitTask, ChildProfile } from '../types';

const EMOJI_OPTIONS = [
  '🪥','🛁','🍎','🥛','👕','🧦','📚','🧸','🙏','🌙',
  '💊','🥦','🏃','🎨','🎵','🌿','💧','🐾','🛏','⭐',
];

function ChevronLeft() {
  return (
    <svg className="w-6 h-6" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
      <path d="M15 18l-6-6 6-6"/>
    </svg>
  );
}

function PencilIcon() {
  return (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7"/>
      <path d="M18.5 2.5a2.121 2.121 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/>
    </svg>
  );
}

function TrashIcon() {
  return (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <polyline points="3 6 5 6 21 6"/>
      <path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a1 1 0 011-1h4a1 1 0 011 1v2"/>
    </svg>
  );
}

interface Props {
  tasks: HabitTask[];
  profile: ChildProfile;
  onBack: () => void;
  onCreate: (task: Omit<HabitTask, 'id'>) => Promise<unknown>;
  onUpdate: (id: string, task: HabitTask) => Promise<unknown>;
  onDelete: (id: string) => Promise<void>;
  onUpdateProfile: (profile: ChildProfile) => Promise<ChildProfile>;
}

export function ParentPanel({ tasks, profile, onBack, onCreate, onUpdate, onDelete, onUpdateProfile }: Props) {
  const [label, setLabel] = useState('');
  const [emoji, setEmoji] = useState('🪥');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editLabel, setEditLabel] = useState('');
  const [editEmoji, setEditEmoji] = useState('');
  const [profileName, setProfileName] = useState(profile.name);
  const [profileAvatar, setProfileAvatar] = useState(profile.avatar);
  const [confirmDeleteId, setConfirmDeleteId] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const handleCreate = async () => {
    if (!label.trim()) return;
    setBusy(true);
    try {
      await onCreate({ label: label.trim(), emoji, sortOrder: tasks.length, isActive: true });
      setLabel('');
    } finally {
      setBusy(false);
    }
  };

  const startEdit = (task: HabitTask) => {
    setEditingId(task.id);
    setEditLabel(task.label);
    setEditEmoji(task.emoji);
    setConfirmDeleteId(null);
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

  const handleDelete = async (id: string) => {
    setBusy(true);
    try {
      await onDelete(id);
      setConfirmDeleteId(null);
    } finally {
      setBusy(false);
    }
  };

  const saveProfile = async () => {
    if (!profileName.trim()) return;
    setBusy(true);
    try {
      await onUpdateProfile({ name: profileName.trim(), avatar: profileAvatar });
    } finally {
      setBusy(false);
    }
  };

  const inputClass = 'w-full border border-gray-200 rounded-xl px-3 py-2.5 text-base focus:outline-none focus:ring-2 focus:ring-orange-300 bg-white';
  const labelClass = 'block text-sm font-medium text-gray-600 mb-1.5';

  return (
    <div className="h-[100dvh] flex flex-col bg-slate-50">
      {/* Header */}
      <div className="flex items-center gap-3 px-4 py-3 bg-white border-b border-gray-100 shadow-sm">
        <button
          onClick={onBack}
          className="min-h-[44px] min-w-[44px] flex items-center justify-center text-gray-600 rounded-xl active:bg-gray-100 transition-colors cursor-pointer"
          aria-label="Back to child view"
        >
          <ChevronLeft />
        </button>
        <h1 className="text-xl font-bold text-gray-800">Manage Tasks</h1>
      </div>

      <div className="flex-1 overflow-y-auto px-4 py-4 space-y-3">
        {/* Child Profile */}
        <div className="bg-white rounded-2xl p-4 shadow-sm space-y-3">
          <h2 className="text-xs font-semibold text-gray-400 uppercase tracking-widest">Child Profile</h2>
          <div className="flex gap-3">
            <div className="shrink-0">
              <label className={labelClass} htmlFor="profile-avatar">Avatar</label>
              <input
                id="profile-avatar"
                value={profileAvatar}
                onChange={e => setProfileAvatar(e.target.value)}
                maxLength={2}
                className="text-3xl text-center border border-gray-200 rounded-xl px-2 py-2 w-16 focus:outline-none focus:ring-2 focus:ring-orange-300 bg-white"
              />
            </div>
            <div className="flex-1 min-w-0">
              <label className={labelClass} htmlFor="profile-name">Name</label>
              <input
                id="profile-name"
                value={profileName}
                onChange={e => setProfileName(e.target.value)}
                placeholder="Child's name"
                className={inputClass}
              />
            </div>
          </div>
          <button
            onClick={saveProfile}
            disabled={busy || !profileName.trim()}
            className="w-full py-2.5 bg-orange-400 text-white font-semibold rounded-xl active:scale-95 transition-transform disabled:opacity-50 cursor-pointer"
          >
            Save Profile
          </button>
        </div>

        {/* Task list */}
        {tasks.map(task => (
          <div key={task.id} className="bg-white rounded-2xl p-4 shadow-sm">
            {editingId === task.id ? (
              <div className="space-y-3">
                <div>
                  <label className={labelClass} htmlFor={`edit-label-${task.id}`}>Task name</label>
                  <div className="flex gap-2 items-center">
                    <span className="text-3xl">{editEmoji}</span>
                    <input
                      id={`edit-label-${task.id}`}
                      value={editLabel}
                      onChange={e => setEditLabel(e.target.value)}
                      className={inputClass}
                      placeholder="Task name"
                    />
                  </div>
                </div>
                <div>
                  <p className={labelClass}>Choose emoji</p>
                  <div className="flex flex-wrap gap-2">
                    {EMOJI_OPTIONS.map(e => (
                      <button
                        key={e}
                        onClick={() => setEditEmoji(e)}
                        className={`text-2xl p-1.5 rounded-lg cursor-pointer ${editEmoji === e ? 'bg-orange-100 ring-2 ring-orange-300' : 'active:bg-gray-100'}`}
                      >
                        {e}
                      </button>
                    ))}
                  </div>
                </div>
                <div className="flex gap-2">
                  <button
                    onClick={() => saveEdit(task)}
                    disabled={busy || !editLabel.trim()}
                    className="flex-1 py-2.5 bg-orange-400 text-white font-semibold rounded-xl active:scale-95 transition-transform disabled:opacity-50 cursor-pointer"
                  >
                    Save
                  </button>
                  <button
                    onClick={() => setEditingId(null)}
                    className="flex-1 py-2.5 bg-gray-100 text-gray-600 font-semibold rounded-xl active:scale-95 transition-transform cursor-pointer"
                  >
                    Cancel
                  </button>
                </div>
              </div>
            ) : (
              <div className="flex items-center gap-3">
                <span className="text-3xl shrink-0">{task.emoji || '📋'}</span>
                <span className="flex-1 text-base font-medium text-gray-700 min-w-0 truncate">{task.label}</span>

                {confirmDeleteId === task.id ? (
                  <div className="flex items-center gap-2 shrink-0">
                    <span className="text-sm text-red-500 font-medium">Delete?</span>
                    <button
                      onClick={() => handleDelete(task.id)}
                      disabled={busy}
                      className="min-h-[44px] px-3 py-1 bg-red-500 text-white text-sm font-semibold rounded-xl active:scale-95 transition-transform disabled:opacity-50 cursor-pointer"
                    >
                      Yes
                    </button>
                    <button
                      onClick={() => setConfirmDeleteId(null)}
                      className="min-h-[44px] px-3 py-1 bg-gray-100 text-gray-600 text-sm font-semibold rounded-xl active:scale-95 transition-transform cursor-pointer"
                    >
                      No
                    </button>
                  </div>
                ) : (
                  <div className="flex items-center gap-1 shrink-0">
                    <button
                      onClick={() => startEdit(task)}
                      aria-label="Edit task"
                      className="min-h-[44px] min-w-[44px] flex items-center justify-center text-gray-400 rounded-xl active:bg-gray-100 transition-colors cursor-pointer"
                    >
                      <PencilIcon />
                    </button>
                    <button
                      onClick={() => setConfirmDeleteId(task.id)}
                      aria-label="Delete task"
                      className="min-h-[44px] min-w-[44px] flex items-center justify-center text-gray-400 rounded-xl active:bg-gray-100 transition-colors cursor-pointer"
                    >
                      <TrashIcon />
                    </button>
                  </div>
                )}
              </div>
            )}
          </div>
        ))}

        {tasks.length === 0 && (
          <div className="flex flex-col items-center gap-2 py-10 text-gray-400">
            <span className="text-5xl">🌱</span>
            <p className="text-base font-medium">No tasks yet. Add one below!</p>
          </div>
        )}
      </div>

      {/* Add task footer */}
      <div className="bg-white border-t border-gray-100 px-4 py-4 space-y-3 shadow-[0_-2px_8px_rgba(0,0,0,0.04)]">
        <h2 className="text-xs font-semibold text-gray-400 uppercase tracking-widest">Add Task</h2>
        <div>
          <label className={labelClass} htmlFor="new-task-label">Task name</label>
          <div className="flex gap-2 items-center">
            <span className="text-3xl shrink-0">{emoji}</span>
            <input
              id="new-task-label"
              value={label}
              onChange={e => setLabel(e.target.value)}
              onKeyDown={e => e.key === 'Enter' && handleCreate()}
              placeholder="e.g. Brush teeth"
              className={inputClass}
            />
          </div>
        </div>
        <div>
          <p className={labelClass}>Choose emoji</p>
          <div className="flex flex-wrap gap-2">
            {EMOJI_OPTIONS.map(e => (
              <button
                key={e}
                onClick={() => setEmoji(e)}
                className={`text-2xl p-1.5 rounded-lg cursor-pointer ${emoji === e ? 'bg-orange-100 ring-2 ring-orange-300' : 'active:bg-gray-100'}`}
              >
                {e}
              </button>
            ))}
          </div>
        </div>
        <button
          onClick={handleCreate}
          disabled={busy || !label.trim()}
          className="w-full py-3 bg-orange-500 text-white font-bold text-lg rounded-2xl active:scale-95 transition-transform disabled:opacity-50 cursor-pointer"
        >
          + Add Task
        </button>
      </div>
    </div>
  );
}
