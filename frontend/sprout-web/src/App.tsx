import { useState, useCallback, useEffect } from 'react';
import { useTasks } from './hooks/useTasks';
import { useProgress } from './hooks/useProgress';
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

  const showToast = useCallback((msg: string) => {
    setToast(msg);
    setTimeout(() => setToast(null), 3000);
  }, []);

  const handleAllComplete = useCallback(() => {
    setShowCelebration(true);
  }, []);

  const { tasks, loading, createTask, updateTask, deleteTask } = useTasks();
  const { today, week, markComplete, markIncomplete } = useProgress(handleAllComplete);

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
        onBack={() => setView('child')}
        onCreate={createTask}
        onUpdate={updateTask}
        onDelete={deleteTask}
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
          className="absolute top-2 right-3 text-xs opacity-30 p-2 rounded-lg active:opacity-60"
        >
          ⚙️
        </button>
      </div>

      {loading ? (
        <div className="flex-1 flex items-center justify-center">
          <div className="w-48 h-48 rounded-3xl bg-gray-100 animate-pulse" />
        </div>
      ) : (
        <TaskCarousel
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

      {toast && (
        <div className="fixed bottom-4 left-1/2 -translate-x-1/2 bg-gray-800 text-white px-5 py-3 rounded-2xl text-sm font-medium shadow-lg z-40">
          {toast}
        </div>
      )}
    </div>
  );
}
