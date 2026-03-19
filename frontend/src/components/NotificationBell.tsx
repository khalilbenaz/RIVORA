import { useState, useRef, useEffect } from 'react';
import { Bell } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useSignalR, Notification } from '../hooks/useSignalR';

function formatRelativeTime(date: Date): string {
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffSec = Math.floor(diffMs / 1000);
  const diffMin = Math.floor(diffSec / 60);
  const diffHour = Math.floor(diffMin / 60);
  const diffDay = Math.floor(diffHour / 24);

  if (diffSec < 60) return `${diffSec}s`;
  if (diffMin < 60) return `${diffMin}m`;
  if (diffHour < 24) return `${diffHour}h`;
  return `${diffDay}d`;
}

function NotificationItem({
  notification,
  onRead,
}: {
  notification: Notification;
  onRead: (id: string) => void;
}) {
  return (
    <button
      onClick={() => onRead(notification.id)}
      className={`w-full text-left px-3 py-2.5 transition-colors hover:bg-slate-50 ${
        notification.read ? 'opacity-60' : ''
      }`}
    >
      <div className="flex items-start justify-between gap-2">
        <span className="inline-block rounded bg-blue-100 px-1.5 py-0.5 text-[10px] font-semibold uppercase text-blue-700">
          {notification.type}
        </span>
        <span className="shrink-0 text-[10px] text-slate-400">
          {formatRelativeTime(notification.timestamp)}
        </span>
      </div>
      <p className="mt-1 truncate text-xs text-slate-600">
        {typeof notification.data === 'string'
          ? notification.data
          : JSON.stringify(notification.data)}
      </p>
    </button>
  );
}

export default function NotificationBell() {
  const { notifications, connected, unreadCount, markAsRead, markAllAsRead } =
    useSignalR();
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);
  const { t } = useTranslation();

  // Close dropdown on outside click
  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  return (
    <div ref={ref} className="relative">
      <button
        onClick={() => setOpen((o) => !o)}
        className="relative rounded-lg p-2 text-slate-400 transition hover:bg-slate-100 hover:text-slate-600"
        aria-label={t('notifications.title', 'Notifications')}
      >
        <Bell size={18} />
        {unreadCount > 0 && (
          <span className="absolute -right-0.5 -top-0.5 flex h-4 min-w-[16px] items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-bold text-white">
            {unreadCount}
          </span>
        )}
        {/* Connection status dot */}
        <span
          className={`absolute bottom-1 right-1 h-2 w-2 rounded-full border border-white ${
            connected ? 'bg-green-500' : 'bg-red-500'
          }`}
        />
      </button>

      {open && (
        <div className="absolute right-0 top-full z-50 mt-2 w-80 rounded-xl border border-slate-200 bg-white shadow-xl">
          <div className="flex items-center justify-between border-b border-slate-100 px-4 py-3">
            <h3 className="text-sm font-semibold text-slate-800">
              {t('notifications.title', 'Notifications')}
            </h3>
            {unreadCount > 0 && (
              <button
                onClick={markAllAsRead}
                className="text-xs font-medium text-blue-600 hover:text-blue-800"
              >
                {t('notifications.markAllRead', 'Mark all as read')}
              </button>
            )}
          </div>

          <div className="max-h-80 overflow-y-auto divide-y divide-slate-100">
            {notifications.length === 0 ? (
              <p className="px-4 py-8 text-center text-xs text-slate-400">
                {t('notifications.empty', 'No notifications yet')}
              </p>
            ) : (
              notifications.map((n) => (
                <NotificationItem
                  key={n.id}
                  notification={n}
                  onRead={markAsRead}
                />
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
}
