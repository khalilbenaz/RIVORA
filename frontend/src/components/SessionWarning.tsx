import { useSessionRefresh } from '../hooks/useSessionRefresh';
import { AlertTriangle } from 'lucide-react';

function formatCountdown(ms: number): string {
  const totalSeconds = Math.max(0, Math.floor(ms / 1000));
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;
  return `${minutes}:${seconds.toString().padStart(2, '0')}`;
}

export default function SessionWarning() {
  const { expiresIn, showWarning, dismiss } = useSessionRefresh();

  if (!showWarning || expiresIn === null) return null;

  return (
    <div
      role="alert"
      className="fixed top-0 left-0 right-0 z-[9999] flex items-center justify-center gap-3 bg-amber-400 px-4 py-2.5 text-sm font-medium text-amber-950 shadow-md"
    >
      <AlertTriangle size={16} className="shrink-0" />
      <span>
        Votre session expire dans {formatCountdown(expiresIn)}
      </span>
      <button
        onClick={dismiss}
        className="ml-2 rounded-md bg-amber-600 px-3 py-1 text-xs font-semibold text-white transition hover:bg-amber-700"
      >
        Prolonger la session
      </button>
    </div>
  );
}
