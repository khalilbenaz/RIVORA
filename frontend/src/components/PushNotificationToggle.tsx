import { Bell, BellOff, BellRing } from 'lucide-react';
import { usePushNotifications } from '../hooks/usePushNotifications';

export default function PushNotificationToggle() {
  const { permission, requestPermission } = usePushNotifications();

  if (permission === 'granted') {
    return (
      <div className="flex items-center gap-3 rounded-lg border border-emerald-200 bg-emerald-50 p-4">
        <BellRing size={20} className="text-emerald-600" />
        <div className="flex-1">
          <h3 className="font-medium text-slate-900">Notifications push</h3>
          <p className="text-sm text-slate-500">Les notifications sont activees.</p>
        </div>
        <span className="rounded-full bg-emerald-100 px-3 py-1 text-xs font-semibold text-emerald-700">
          Actif
        </span>
      </div>
    );
  }

  if (permission === 'denied') {
    return (
      <div className="flex items-center gap-3 rounded-lg border border-slate-200 bg-slate-50 p-4">
        <BellOff size={20} className="text-slate-400" />
        <div className="flex-1">
          <h3 className="font-medium text-slate-900">Notifications push</h3>
          <p className="text-sm text-slate-500">
            Bloquees dans les parametres du navigateur. Veuillez modifier les autorisations de votre navigateur pour activer les notifications.
          </p>
        </div>
        <span className="rounded-full bg-slate-200 px-3 py-1 text-xs font-semibold text-slate-500">
          Bloque
        </span>
      </div>
    );
  }

  return (
    <div className="flex items-center gap-3 rounded-lg border border-slate-200 p-4">
      <Bell size={20} className="text-slate-400" />
      <div className="flex-1">
        <h3 className="font-medium text-slate-900">Notifications push</h3>
        <p className="text-sm text-slate-500">
          Recevez des notifications en temps reel, meme lorsque l'onglet n'est pas au premier plan.
        </p>
      </div>
      <button
        onClick={requestPermission}
        className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700"
      >
        Activer
      </button>
    </div>
  );
}
