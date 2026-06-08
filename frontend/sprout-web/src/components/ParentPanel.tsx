import { useState, useEffect, useRef } from 'react';
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

function GripIcon() {
  return (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="currentColor">
      <circle cx="9" cy="7" r="1.5"/><circle cx="15" cy="7" r="1.5"/>
      <circle cx="9" cy="12" r="1.5"/><circle cx="15" cy="12" r="1.5"/>
      <circle cx="9" cy="17" r="1.5"/><circle cx="15" cy="17" r="1.5"/>
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
  const [error, setError] = useState<string | null>(null);

  // Local ordered copy for drag reorder
  const [localTasks, setLocalTasks] = useState<HabitTask[]>([]);
  const [dragIndex, setDragIndex] = useState<number | null>(null);
  const [overIndex, setOverIndex] = useState<number | null>(null);
  const dragIndexRef = useRef<number | null>(null);
  const overIndexRef = useRef<number | null>(null);
  const rowRefs = useRef<(HTMLDivElement | null)[]>([]);

  // Sync from props only when not dragging
  useEffect(() => {
    if (dragIndexRef.current === null) {
      setLocalTasks([...tasks].sort((a, b) => a.sortOrder - b.sortOrder));
    }
  }, [tasks]);

  const handleCreate = async () => {
    if (!label.trim()) return;
    if (!emoji.trim()) {
      setError('Emoji is required');
      return;
    }
    setBusy(true);
    setError(null);
    try {
      await onCreate({ label: label.trim(), emoji, sortOrder: localTasks.length, isActive: true });
      setLabel('');
      setEmoji('🪥');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to create task');
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
    if (!editEmoji.trim()) {
      setError('Emoji is required');
      return;
    }
    setBusy(true);
    setError(null);
    try {
      await onUpdate(task.id, { ...task, label: editLabel.trim(), emoji: editEmoji });
      setEditingId(null);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to update task');
    } finally {
      setBusy(false);
    }
  };

  const handleDelete = async (id: string) => {
    setBusy(true);
    setError(null);
    try {
      await onDelete(id);
      setConfirmDeleteId(null);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to delete task');
    } finally {
      setBusy(false);
    }
  };

  const saveProfile = async () => {
    if (!profileName.trim()) {
      setError('Child name is required');
      return;
    }
    if (!profileAvatar.trim()) {
      setError('Avatar emoji is required');
      return;
    }
    setBusy(true);
    setError(null);
    try {
      await onUpdateProfile({ name: profileName.trim(), avatar: profileAvatar });
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to save profile');
    } finally {
      setBusy(false);
    }
  };

  // Drag handlers — pointer capture keeps events on the handle during touch/mouse drag
  const onHandlePointerDown = (e: React.PointerEvent<HTMLButtonElement>, index: number) => {
    e.currentTarget.setPointerCapture(e.pointerId);
    dragIndexRef.current = index;
    overIndexRef.current = index;
    setDragIndex(index);
    setOverIndex(index);
  };

  const onHandlePointerMove = (e: React.PointerEvent<HTMLButtonElement>) => {
    if (dragIndexRef.current === null) return;
    const y = e.clientY;
    for (let i = 0; i < rowRefs.current.length; i++) {
      const el = rowRefs.current[i];
      if (!el) continue;
      const rect = el.getBoundingClientRect();
      if (y >= rect.top && y < rect.bottom) {
        if (i !== overIndexRef.current) {
          overIndexRef.current = i;
          setOverIndex(i);
        }
        break;
      }
    }
  };

  const onHandlePointerUp = async (e: React.PointerEvent<HTMLButtonElement>) => {
    e.currentTarget.releasePointerCapture(e.pointerId);
    const from = dragIndexRef.current;
    const to = overIndexRef.current;
    dragIndexRef.current = null;
    overIndexRef.current = null;
    setDragIndex(null);
    setOverIndex(null);

    if (from === null || to === null || from === to) return;

    const reordered = [...localTasks];
    const [moved] = reordered.splice(from, 1);
    reordered.splice(to, 0, moved);
    const withOrder = reordered.map((t, i) => ({ ...t, sortOrder: i }));
    setLocalTasks(withOrder);

    setBusy(true);
    try {
      for (const t of withOrder) {
        const orig = tasks.find(o => o.id === t.id);
        if (orig && orig.sortOrder !== t.sortOrder) {
          await onUpdate(t.id, t);
        }
      }
    } finally {
      setBusy(false);
    }
  };

  const inputClass = 'w-full border border-gray-200 rounded-xl px-3 py-2.5 text-base focus:outline-none focus:ring-2 focus:ring-orange-300 bg-white';
  const labelClass = 'block text-sm font-medium text-gray-600 mb-1.5';

  return (
    <div className="h-[100dvh] flex flex-col bg-slate-50">
      {/* Header */}
      <header className="flex items-center gap-3 px-4 py-3 bg-white border-b border-gray-100 shadow-sm">
        <button
          onClick={onBack}
          className="min-h-[44px] min-w-[44px] flex items-center justify-center text-gray-600 rounded-xl hover:bg-gray-100 active:bg-gray-200 transition-colors cursor-pointer focus-visible:outline-2 focus-visible:outline-orange-400"
          aria-label="Back to child view"
          title="Return to child view"
        >
          <ChevronLeft />
        </button>
        <h1 className="text-xl font-bold text-gray-800">Manage Tasks</h1>
      </header>

      {/* Error message */}
      {error && (
        <div className="bg-red-50 border-b border-red-100 px-4 py-3 flex items-center justify-between" role="alert" aria-live="polite">
          <p className="text-red-700 text-sm font-medium">{error}</p>
          <button
            onClick={() => setError(null)}
            className="text-red-400 hover:text-red-600 font-bold text-lg focus-visible:outline-2 focus-visible:outline-red-600"
            aria-label="Dismiss error message"
          >
            ✕
          </button>
        </div>
      )}

      <main className="flex-1 overflow-y-auto px-4 py-4 space-y-3">
        {/* Child Profile */}
        <section className="bg-white rounded-2xl p-4 shadow-sm space-y-3">
          <h2 className="text-xs font-semibold text-gray-400 uppercase tracking-widest">Child Profile</h2>
          <div className="flex gap-3">
            <div className="shrink-0">
              <label className={labelClass} htmlFor="profile-avatar">Avatar <span className="text-red-500" aria-label="required">*</span></label>
              <input
                id="profile-avatar"
                value={profileAvatar}
                onChange={e => setProfileAvatar(e.target.value.slice(-2))}
                maxLength={2}
                aria-required="true"
                disabled={busy}
                className="text-3xl text-center border border-gray-200 rounded-xl px-2 py-2 w-16 focus:outline-none focus:ring-2 focus:ring-orange-300 bg-white disabled:opacity-50"
                placeholder="😊"
                title="Paste or type an emoji for the child's avatar"
              />
            </div>
            <div className="flex-1 min-w-0">
              <label className={labelClass} htmlFor="profile-name">Name <span className="text-red-500" aria-label="required">*</span></label>
              <input
                id="profile-name"
                value={profileName}
                onChange={e => setProfileName(e.target.value)}
                placeholder="Child's name"
                aria-required="true"
                disabled={busy}
                className={inputClass}
              />
            </div>
          </div>
          <button
            onClick={saveProfile}
            disabled={busy || !profileName.trim() || !profileAvatar.trim()}
            className="w-full py-2.5 bg-orange-400 text-white font-semibold rounded-xl hover:bg-orange-500 active:scale-95 transition-all disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-orange-600"
          >
            {busy ? 'Saving...' : 'Save Profile'}
          </button>
        </section>

        {/* Task list */}
        <section aria-label="Tasks list">
          {localTasks.map((task, i) => (
            <div
              key={task.id}
              ref={el => { rowRefs.current[i] = el; }}
              className={`bg-white rounded-2xl shadow-sm transition-all duration-150 ${
                dragIndex === i ? 'opacity-40 scale-[0.98]' : ''
              } ${
                overIndex === i && dragIndex !== null && dragIndex !== i
                  ? 'ring-2 ring-orange-300'
                  : ''
              }`}
            >
              {editingId === task.id ? (
                <div className="p-4 space-y-3">
                  <div>
                    <label className={labelClass} htmlFor={`edit-label-${task.id}`}>Task name <span className="text-red-500" aria-label="required">*</span></label>
                    <div className="flex gap-2 items-center">
                      <span className="text-3xl" aria-hidden="true">{editEmoji}</span>
                      <input
                        id={`edit-label-${task.id}`}
                        value={editLabel}
                        onChange={e => setEditLabel(e.target.value)}
                        className={inputClass}
                        placeholder="Task name"
                        aria-required="true"
                      />
                    </div>
                  </div>
                  <div>
                    <p className={labelClass}>Choose emoji <span className="text-red-500" aria-label="required">*</span></p>
                    <div className="space-y-2">
                      <input
                        type="text"
                        value={editEmoji}
                        onChange={e => setEditEmoji(e.target.value.slice(-2))}
                        maxLength={2}
                        placeholder="Paste or type emoji"
                        disabled={busy}
                        className="w-full text-center text-3xl border border-gray-200 rounded-xl px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-orange-300 bg-white disabled:opacity-50"
                        aria-label="Enter custom emoji or paste emoji here"
                        title="Paste an emoji here or select from presets below"
                      />
                      <div className="flex flex-wrap gap-2" role="group" aria-label="Preset emoji options">
                        {EMOJI_OPTIONS.map(e => (
                          <button
                            key={e}
                            onClick={() => setEditEmoji(e)}
                            disabled={busy}
                            aria-pressed={editEmoji === e}
                            className={`text-2xl p-1.5 rounded-lg cursor-pointer focus-visible:outline-2 focus-visible:outline-orange-400 disabled:opacity-50 disabled:cursor-not-allowed ${editEmoji === e ? 'bg-orange-100 ring-2 ring-orange-300' : 'hover:bg-gray-50 active:bg-gray-100'}`}
                            aria-label={`Select ${e} emoji`}
                          >
                            {e}
                          </button>
                        ))}
                      </div>
                    </div>
                  </div>
                  <div className="flex gap-2">
                    <button
                      onClick={() => saveEdit(task)}
                      disabled={busy || !editLabel.trim() || !editEmoji.trim()}
                      className="flex-1 py-2.5 bg-orange-400 text-white font-semibold rounded-xl hover:bg-orange-500 active:scale-95 transition-all disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-orange-600"
                    >
                      {busy ? 'Saving...' : 'Save'}
                    </button>
                    <button
                      onClick={() => setEditingId(null)}
                      disabled={busy}
                      className="flex-1 py-2.5 bg-gray-100 text-gray-600 font-semibold rounded-xl hover:bg-gray-200 active:scale-95 transition-all disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-gray-400"
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              ) : (
                <div className="flex items-center gap-2 px-4 py-3">
                  {/* Drag handle */}
                  <button
                    aria-label={`Drag to reorder task: ${task.label}`}
                    disabled={busy || editingId !== null}
                    onPointerDown={e => onHandlePointerDown(e, i)}
                    onPointerMove={onHandlePointerMove}
                    onPointerUp={onHandlePointerUp}
                    className="min-h-[44px] min-w-[36px] flex items-center justify-center text-gray-300 disabled:opacity-30 disabled:cursor-not-allowed cursor-grab hover:text-gray-400 active:cursor-grabbing touch-none focus-visible:outline-2 focus-visible:outline-orange-400 rounded"
                    style={{ touchAction: 'none' }}
                  >
                    <GripIcon />
                  </button>

                  <span className="text-3xl shrink-0" aria-hidden="true">{task.emoji || '📋'}</span>
                  <span className="flex-1 text-base font-medium text-gray-700 min-w-0 truncate">{task.label}</span>

                  {confirmDeleteId === task.id ? (
                    <div className="flex items-center gap-2 shrink-0">
                      <span className="text-sm text-red-500 font-medium">Delete?</span>
                      <button
                        onClick={() => handleDelete(task.id)}
                        disabled={busy}
                        className="min-h-[44px] px-3 py-1 bg-red-500 text-white text-sm font-semibold rounded-xl hover:bg-red-600 active:scale-95 transition-all disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer focus-visible:outline-2 focus-visible:outline-offset-1 focus-visible:outline-red-600"
                      >
                        Yes
                      </button>
                      <button
                        onClick={() => setConfirmDeleteId(null)}
                        className="min-h-[44px] px-3 py-1 bg-gray-100 text-gray-600 text-sm font-semibold rounded-xl hover:bg-gray-200 active:scale-95 transition-all cursor-pointer focus-visible:outline-2 focus-visible:outline-offset-1 focus-visible:outline-gray-400"
                      >
                        No
                      </button>
                    </div>
                  ) : (
                    <div className="flex items-center gap-1 shrink-0">
                      <button
                        onClick={() => startEdit(task)}
                        aria-label={`Edit task: ${task.label}`}
                        title="Edit task"
                        className="min-h-[44px] min-w-[44px] flex items-center justify-center text-gray-400 rounded-xl hover:bg-gray-100 hover:text-gray-600 active:bg-gray-200 transition-colors cursor-pointer focus-visible:outline-2 focus-visible:outline-orange-400"
                      >
                        <PencilIcon />
                      </button>
                      <button
                        onClick={() => setConfirmDeleteId(task.id)}
                        aria-label={`Delete task: ${task.label}`}
                        title="Delete task"
                        className="min-h-[44px] min-w-[44px] flex items-center justify-center text-gray-400 rounded-xl hover:bg-gray-100 hover:text-gray-600 active:bg-gray-200 transition-colors cursor-pointer focus-visible:outline-2 focus-visible:outline-orange-400"
                      >
                        <TrashIcon />
                      </button>
                    </div>
                  )}
                </div>
              )}
            </div>
          ))}

          {localTasks.length === 0 && (
            <div className="flex flex-col items-center gap-2 py-10 text-gray-400">
              <span className="text-5xl" aria-hidden="true">🌱</span>
              <p className="text-base font-medium">No tasks yet. Add one below!</p>
            </div>
          )}
        </section>
      </main>

      {/* Add task footer */}
      <footer className="bg-white border-t border-gray-100 px-4 py-4 space-y-3 shadow-[0_-2px_8px_rgba(0,0,0,0.04)]">
        <h2 className="text-xs font-semibold text-gray-400 uppercase tracking-widest">Add Task</h2>
        <div>
          <label className={labelClass} htmlFor="new-task-label">Task name <span className="text-red-500" aria-label="required">*</span></label>
          <div className="flex gap-2 items-center">
            <span className="text-3xl shrink-0" aria-hidden="true">{emoji}</span>
            <input
              id="new-task-label"
              value={label}
              onChange={e => setLabel(e.target.value)}
              onKeyDown={e => e.key === 'Enter' && !busy && handleCreate()}
              placeholder="e.g. Brush teeth"
              className={inputClass}
              aria-required="true"
              disabled={busy}
            />
          </div>
        </div>
        <div>
          <p className={labelClass}>Choose emoji <span className="text-red-500" aria-label="required">*</span></p>
          <div className="space-y-2">
            <input
              type="text"
              value={emoji}
              onChange={e => setEmoji(e.target.value.slice(-2))}
              maxLength={2}
              placeholder="Paste or type emoji"
              disabled={busy}
              className="w-full text-center text-3xl border border-gray-200 rounded-xl px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-orange-300 bg-white disabled:opacity-50"
              aria-label="Enter custom emoji or paste emoji here"
              title="Paste an emoji here or select from presets below"
            />
            <div className="flex flex-wrap gap-2" role="group" aria-label="Preset emoji options">
              {EMOJI_OPTIONS.map(e => (
                <button
                  key={e}
                  onClick={() => setEmoji(e)}
                  disabled={busy}
                  aria-pressed={emoji === e}
                  className={`text-2xl p-1.5 rounded-lg cursor-pointer focus-visible:outline-2 focus-visible:outline-orange-400 disabled:opacity-50 disabled:cursor-not-allowed ${emoji === e ? 'bg-orange-100 ring-2 ring-orange-300' : 'hover:bg-gray-50 active:bg-gray-100'}`}
                  aria-label={`Select ${e} emoji`}
                >
                  {e}
                </button>
              ))}
            </div>
          </div>
        </div>
        <button
          onClick={handleCreate}
          disabled={busy || !label.trim() || !emoji.trim()}
          className="w-full py-3 bg-orange-500 text-white font-bold text-lg rounded-2xl hover:bg-orange-600 active:scale-95 transition-all disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-orange-700"
        >
          {busy ? 'Adding...' : '+ Add Task'}
        </button>
      </footer>
    </div>
  );
}
