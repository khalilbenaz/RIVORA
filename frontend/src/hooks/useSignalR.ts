import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '../store/authStore';

export interface Notification {
  id: string;
  type: string;
  data: unknown;
  timestamp: Date;
  read: boolean;
}

export function useSignalR() {
  const token = useAuthStore((s) => s.token);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [connected, setConnected] = useState(false);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    if (!token) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/kba', { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on('ReceiveNotification', (type: string, data: unknown) => {
      setNotifications((prev) => [
        {
          id: crypto.randomUUID(),
          type,
          data,
          timestamp: new Date(),
          read: false,
        },
        ...prev.slice(0, 49), // Keep max 50
      ]);

      // Trigger browser push notification when page is not focused
      if (
        typeof Notification !== 'undefined' &&
        Notification.permission === 'granted' &&
        document.visibilityState === 'hidden'
      ) {
        const payload = data as Record<string, string> | null;
        const title = type === 'chat.message'
          ? `New message from ${payload?.senderName ?? 'someone'}`
          : 'RIVORA Notification';
        const body = type === 'chat.message'
          ? (payload?.content ?? '')
          : (typeof data === 'string' ? data : JSON.stringify(data));

        new Notification(title, {
          body,
          icon: '/icons/icon-192.svg',
          badge: '/icons/icon-192.svg',
          tag: type, // Prevents duplicate notifications of same type
        });
      }
    });

    connection.onclose(() => setConnected(false));
    connection.onreconnected(() => setConnected(true));

    connection.start()
      .then(() => setConnected(true))
      .catch(() => setConnected(false));

    connectionRef.current = connection;

    return () => {
      connection.stop();
    };
  }, [token]);

  const markAsRead = (id: string) => {
    setNotifications((prev) =>
      prev.map((n) => (n.id === id ? { ...n, read: true } : n))
    );
  };

  const markAllAsRead = () => {
    setNotifications((prev) => prev.map((n) => ({ ...n, read: true })));
  };

  const unreadCount = notifications.filter((n) => !n.read).length;

  return { notifications, connected, unreadCount, markAsRead, markAllAsRead };
}
