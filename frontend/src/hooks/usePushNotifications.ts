import { useState, useCallback } from 'react';

export function usePushNotifications() {
  const [permission, setPermission] = useState<NotificationPermission>(
    typeof Notification !== 'undefined' ? Notification.permission : 'default'
  );

  const requestPermission = useCallback(async () => {
    if (!('Notification' in window)) return 'denied' as const;
    const result = await Notification.requestPermission();
    setPermission(result);
    return result;
  }, []);

  const sendNotification = useCallback((title: string, options?: NotificationOptions) => {
    if (permission !== 'granted') return;
    new Notification(title, {
      icon: '/icons/icon-192.svg',
      badge: '/icons/icon-192.svg',
      ...options,
    });
  }, [permission]);

  return { permission, requestPermission, sendNotification };
}
