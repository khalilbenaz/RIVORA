import { useState, useEffect, useCallback } from 'react';
import { useAuthStore } from '../store/authStore';

function parseJwtExp(token: string): number | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    const payload = JSON.parse(atob(parts[1]!.replace(/-/g, '+').replace(/_/g, '/')));
    return typeof payload.exp === 'number' ? payload.exp : null;
  } catch {
    return null;
  }
}

const WARNING_BEFORE_MS = 5 * 60 * 1000; // 5 minutes

export function useSessionRefresh() {
  const { token, logout } = useAuthStore();
  const [expiresIn, setExpiresIn] = useState<number | null>(null);
  const [showWarning, setShowWarning] = useState(false);

  const dismiss = useCallback(() => {
    setShowWarning(false);
  }, []);

  useEffect(() => {
    if (!token) {
      setExpiresIn(null);
      setShowWarning(false);
      return;
    }

    const exp = parseJwtExp(token);
    if (!exp) {
      setExpiresIn(null);
      setShowWarning(false);
      return;
    }

    let warningTimer: ReturnType<typeof setTimeout> | undefined;
    let logoutTimer: ReturnType<typeof setTimeout> | undefined;
    let countdownInterval: ReturnType<typeof setInterval> | undefined;

    const setup = () => {
      const now = Date.now();
      const expiresAt = exp * 1000;
      const remaining = expiresAt - now;

      if (remaining <= 0) {
        logout();
        return;
      }

      setExpiresIn(remaining);

      // Countdown ticker (every second)
      countdownInterval = setInterval(() => {
        const left = expiresAt - Date.now();
        if (left <= 0) {
          setExpiresIn(0);
        } else {
          setExpiresIn(left);
        }
      }, 1000);

      // Warning timer
      const timeUntilWarning = remaining - WARNING_BEFORE_MS;
      if (timeUntilWarning <= 0) {
        setShowWarning(true);
      } else {
        warningTimer = setTimeout(() => {
          setShowWarning(true);
        }, timeUntilWarning);
      }

      // Auto-logout timer
      logoutTimer = setTimeout(() => {
        logout();
      }, remaining);
    };

    setup();

    return () => {
      if (warningTimer) clearTimeout(warningTimer);
      if (logoutTimer) clearTimeout(logoutTimer);
      if (countdownInterval) clearInterval(countdownInterval);
    };
  }, [token, logout]);

  return { expiresIn, showWarning, dismiss };
}
