import { useState, useCallback, useEffect } from 'react';
import { useTasks } from './hooks/useTasks';
import { useProgress } from './hooks/useProgress';
import { useProfile } from './hooks/useProfile';
import { StreakBar } from './components/StreakBar';
import { TaskCarousel } from './components/TaskCarousel';
import { DoneButton } from './components/DoneButton';
import { CelebrationOverlay } from './components/CelebrationOverlay';
import { ParentPanel } from './components/ParentPanel';

type View = 'child' | 'parent';

export function App() {
  const [view, setView] = useState<View>('child');
  const [currentIndex, setCurrentIndex] = useState(0);
  const [showCelebration, setShowCelebration] = useState(false);
  const [toast, setToast] = useState<string | null>(null);
  const [welcomeState, setWelcomeState] = useState<'visible' | 'hiding' | 'hidden'>('visible');

  const showToast = useCallback((msg: string) => {
    setToast(msg);
    setTimeout(() => setToast(null), 3000);
  }, []);

  const handleAllComplete = useCallback(() => {
    setShowCelebration(true);
  }, []);

  const { tasks, loading, createTask, updateTask, deleteTask } = useTasks();
  const { profile, loading: profileLoading, updateProfile } = useProfile();
  const { today, week, markComplete, markIncomplete } = useProgress(handleAllComplete);

  useEffect(() => {
    if (profileLoading) return;
    const dismiss = () => {
      setWelcomeState('hiding');
      setTimeout(() => setWelcomeState('hidden'), 400);
    };
    const timer = setTimeout(dismiss, 2500);
    return () => clearTimeout(timer);
  }, [profileLoading]);

  const completedIds = today?.completedTaskIds ?? [];
  const currentTask = tasks[currentIndex] ?? null;
  const currentTaskCompleted = currentTask ? completedIds.includes(currentTask.id) : false;

  useEffect(() => {
    if (currentIndex >= tasks.length && tasks.length > 0) {
      setCurrentIndex(tasks.length - 1);
    }
  }, [tasks.length, currentIndex]);

  const handleDone = async () => {
    if (!currentTask) return;
    try {
      if (currentTaskCompleted) {
        await markIncomplete(currentTask.id);
      } else {
        await markComplete(currentTask.id, tasks.length);
        if (currentIndex < tasks.length - 1) {
          setTimeout(() => setCurrentIndex(i => i + 1), 400);
        }
      }
    } catch (e) {
      showToast(e instanceof Error ? e.message : 'Something went wrong');
    }
  };

  if (view === 'parent') {
    return (
      <ParentPanel
        tasks={tasks}
        profile={profile}
        onBack={() => setView('child')}
        onCreate={createTask}
        onUpdate={updateTask}
        onDelete={deleteTask}
        onUpdateProfile={updateProfile}
      />
    );
  }

  return (
    <div className="h-[100dvh] flex flex-col bg-white overflow-hidden">
      <div className="relative">
        <StreakBar week={week} />
        <button
          onClick={() => setView('parent')}
          aria-label="Open parent settings"
          className="absolute top-2 right-3 opacity-25 p-2 rounded-lg active:opacity-70 cursor-pointer text-gray-600"
        >
          <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5">
            <circle cx="12" cy="12" r="3"/>
            <path d="M19.4 15a1.65 1.65 0 00.33 1.82l.06.06a2 2 0 010 2.83 2 2 0 01-2.83 0l-.06-.06a1.65 1.65 0 00-1.82-.33 1.65 1.65 0 00-1 1.51V21a2 2 0 01-4 0v-.09A1.65 1.65 0 009 19.4a1.65 1.65 0 00-1.82.33l-.06.06a2 2 0 01-2.83-2.83l.06-.06A1.65 1.65 0 004.68 15a1.65 1.65 0 00-1.51-1H3a2 2 0 010-4h.09A1.65 1.65 0 004.6 9a1.65 1.65 0 00-.33-1.82l-.06-.06a2 2 0 012.83-2.83l.06.06A1.65 1.65 0 009 4.68a1.65 1.65 0 001-1.51V3a2 2 0 014 0v.09a1.65 1.65 0 001 1.51 1.65 1.65 0 001.82-.33l.06-.06a2 2 0 012.83 2.83l-.06.06A1.65 1.65 0 0019.4 9a1.65 1.65 0 001.51 1H21a2 2 0 010 4h-.09a1.65 1.65 0 00-1.51 1z"/>
          </svg>
        </button>
      </div>

      {loading ? (
        <div className="flex-1 flex items-center justify-center">
          <div className="w-48 h-48 rounded-3xl bg-gray-100 animate-pulse" />
        </div>
      ) : (
        <TaskCarousel
          profile={profile}
          tasks={tasks}
          completedIds={completedIds}
          currentIndex={currentIndex}
          onIndexChange={setCurrentIndex}
        />
      )}

      <DoneButton
        onDone={handleDone}
        completed={currentTaskCompleted}
        disabled={loading || !currentTask}
      />

      {showCelebration && (
        <CelebrationOverlay onDismiss={() => setShowCelebration(false)} />
      )}

      {welcomeState !== 'hidden' && (
        <div
          className={`fixed inset-0 flex flex-col items-center justify-center bg-yellow-50 z-50 ${welcomeState === 'hiding' ? 'animate-welcome-out' : 'animate-welcome-in'}`}
          onClick={() => {
            setWelcomeState('hiding');
            setTimeout(() => setWelcomeState('hidden'), 400);
          }}
        >
          <div className="text-8xl mb-6 animate-float">{profile.avatar}</div>
          <p className="text-4xl font-bold text-yellow-600 tracking-tight">Welcome,</p>
          <p className="text-5xl font-black text-yellow-500 mt-1">{profile.name}! 🌱</p>
        </div>
      )}

      {toast && (
        <div className="fixed bottom-4 left-1/2 -translate-x-1/2 bg-gray-800 text-white px-5 py-3 rounded-2xl text-sm font-medium shadow-lg z-40">
          {toast}
        </div>
      )}
    </div>
  );
}
