import { useState, useCallback, useEffect } from 'react';
import { useTasks } from './hooks/useTasks';
import { useProgress } from './hooks/useProgress';
import { useProfile } from './hooks/useProfile';
import { StreakBar } from './components/StreakBar';
import { TaskCarousel } from './components/TaskCarousel';
import { DoneButton } from './components/DoneButton';
import { CelebrationOverlay } from './components/CelebrationOverlay';
import { ParentPanel } from './components/ParentPanel';
import { PinAuthModal } from './components/PinAuthModal';

type View = 'child' | 'parent';

export function App() {
  const [view, setView] = useState<View>('child');
  const [currentIndex, setCurrentIndex] = useState(0);
  const [showCelebration, setShowCelebration] = useState(false);
  const [celebrationEmoji, setCelebrationEmoji] = useState<string | undefined>();
  const [isAllDone, setIsAllDone] = useState(false);
  const [toast, setToast] = useState<string | null>(null);
  const [welcomeState, setWelcomeState] = useState<'visible' | 'hiding' | 'hidden'>('visible');
  const [avatarFlash, setAvatarFlash] = useState(false);
  const [isPinAuthenticated, setIsPinAuthenticated] = useState(false);

  const showToast = useCallback((msg: string) => {
    setToast(msg);
    setTimeout(() => setToast(null), 3000);
  }, []);

  const handleTaskComplete = useCallback((taskEmoji?: string, allDone?: boolean) => {
    setCelebrationEmoji(taskEmoji);
    setIsAllDone(allDone ?? false);
    setShowCelebration(true);
  }, []);

  const handleStarsReach = useCallback(() => {
    setAvatarFlash(true);
    setTimeout(() => setAvatarFlash(false), 600);
  }, []);

  const { tasks, loading, isOffline, createTask, updateTask, deleteTask } = useTasks();
  const { profile, loading: profileLoading, updateProfile } = useProfile();
  const { today, week, markComplete, markIncomplete, markingIds } = useProgress(handleTaskComplete);
  const isMarking = markingIds.size > 0;

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
    if (!currentTask || isMarking) return;
    try {
      if (currentTaskCompleted) {
        await markIncomplete(currentTask.id);
      } else {
        await markComplete(currentTask.id, tasks.map(t => t.id), currentTask.emoji);
        if (currentIndex < tasks.length - 1) {
          setTimeout(() => setCurrentIndex(i => i + 1), 400);
        }
      }
    } catch (e) {
      showToast(e instanceof Error ? e.message : 'Something went wrong');
    }
  };

  if (view === 'parent') {
    if (!isPinAuthenticated) {
      return (
        <>
          <PinAuthModal onSuccess={() => setIsPinAuthenticated(true)} />
          <div className="h-[100dvh] flex flex-col bg-white overflow-hidden opacity-50 pointer-events-none">
            <StreakBar week={week} />
          </div>
        </>
      );
    }

    return (
      <ParentPanel
        tasks={tasks}
        profile={profile}
        onBack={() => {
          setView('child');
          setIsPinAuthenticated(false);
        }}
        onCreate={createTask}
        onUpdate={updateTask}
        onDelete={deleteTask}
        onUpdateProfile={updateProfile}
      />
    );
  }

  return (
    <div className="h-[100dvh] flex flex-col bg-white overflow-hidden">
      <header className="relative">
        <StreakBar week={week} />
        <button
          onClick={() => {
            setIsPinAuthenticated(false);
            setView('parent');
          }}
          aria-label="Open parent settings and task management"
          title="Parent Settings"
          className="absolute top-2 right-3 opacity-25 p-2 rounded-lg hover:opacity-50 active:opacity-70 cursor-pointer text-gray-600 transition-opacity"
        >
          <svg className="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" aria-hidden="true">
            <circle cx="12" cy="12" r="3"/>
            <path d="M19.4 15a1.65 1.65 0 00.33 1.82l.06.06a2 2 0 010 2.83 2 2 0 01-2.83 0l-.06-.06a1.65 1.65 0 00-1.82-.33 1.65 1.65 0 00-1 1.51V21a2 2 0 01-4 0v-.09A1.65 1.65 0 009 19.4a1.65 1.65 0 00-1.82.33l-.06.06a2 2 0 01-2.83-2.83l.06-.06A1.65 1.65 0 004.68 15a1.65 1.65 0 00-1.51-1H3a2 2 0 010-4h.09A1.65 1.65 0 004.6 9a1.65 1.65 0 00-.33-1.82l-.06-.06a2 2 0 012.83-2.83l.06.06A1.65 1.65 0 009 4.68a1.65 1.65 0 001-1.51V3a2 2 0 014 0v.09a1.65 1.65 0 001 1.51 1.65 1.65 0 001.82-.33l.06-.06a2 2 0 012.83 2.83l-.06.06A1.65 1.65 0 0019.4 9a1.65 1.65 0 001.51 1H21a2 2 0 010 4h-.09a1.65 1.65 0 00-1.51 1z"/>
          </svg>
        </button>
      </header>

      <main className="flex-1 overflow-hidden">
        {loading ? (
          <div className="flex h-full items-center justify-center" role="status" aria-label="Loading tasks">
            <div className="w-48 h-48 rounded-3xl bg-gray-100 animate-pulse" />
          </div>
        ) : (
          <TaskCarousel
            profile={profile}
            tasks={tasks}
            completedIds={completedIds}
            currentIndex={currentIndex}
            onIndexChange={setCurrentIndex}
            avatarFlash={avatarFlash}
          />
        )}
      </main>

      <DoneButton
        onDone={handleDone}
        completed={currentTaskCompleted}
        disabled={loading || !currentTask || isMarking}
        onStarsReach={handleStarsReach}
      />

      {showCelebration && (
        <CelebrationOverlay
          onDismiss={() => setShowCelebration(false)}
          taskEmoji={celebrationEmoji}
          isAllDone={isAllDone}
        />
      )}

      {welcomeState !== 'hidden' && (
        <div
          role="dialog"
          aria-modal="true"
          aria-label="Welcome message"
          className={`fixed inset-0 flex flex-col items-center justify-center bg-yellow-50 z-50 ${welcomeState === 'hiding' ? 'animate-welcome-out' : 'animate-welcome-in'}`}
          onClick={() => {
            setWelcomeState('hiding');
            setTimeout(() => setWelcomeState('hidden'), 400);
          }}
          onKeyDown={(e) => {
            if (e.key === 'Enter' || e.key === ' ') {
              e.preventDefault();
              setWelcomeState('hiding');
              setTimeout(() => setWelcomeState('hidden'), 400);
            }
          }}
          tabIndex={0}
        >
          <div className="text-8xl mb-6 animate-float" aria-hidden="true">{profile.avatar}</div>
          <h1 className="text-4xl font-bold text-yellow-600 tracking-tight">Welcome,</h1>
          <p className="text-5xl font-black text-yellow-500 mt-1">{profile.name}! 🌱</p>
          <p className="sr-only">Click or press Enter to continue</p>
        </div>
      )}

      {toast && (
        <div
          role="status"
          aria-live="polite"
          aria-atomic="true"
          className="fixed bottom-4 left-1/2 -translate-x-1/2 bg-gray-800 text-white px-5 py-3 rounded-2xl text-sm font-medium shadow-lg z-40"
        >
          {toast}
        </div>
      )}

      {isOffline && (
        <div
          role="alert"
          aria-live="polite"
          className="fixed top-0 left-0 right-0 bg-orange-100 text-orange-800 px-4 py-2 text-center text-sm font-medium z-50"
        >
          📡 Offline mode — changes will sync when online
        </div>
      )}
    </div>
  );
}
