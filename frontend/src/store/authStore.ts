import { create } from 'zustand';
import type { User } from '../types';

interface AuthState {
  token: string | null;
  refreshToken: string | null;
  user: User | null;
  isAuthenticated: boolean;
  login: (token: string, refreshToken: string, user: User) => void;
  logout: () => void;
  setToken: (token: string) => void;
  clearAuth: () => void;
  loadFromStorage: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  token: null,
  refreshToken: null,
  user: null,
  isAuthenticated: false,

  login: (token, refreshToken, user) => {
    // Security: access token stored in memory only — not persisted to prevent XSS token theft
    localStorage.setItem('rvr_user', JSON.stringify(user));
    set({ token, refreshToken, user, isAuthenticated: true });
  },

  logout: () => {
    // TODO: call server endpoint to clear the HttpOnly refresh token cookie
    localStorage.removeItem('rvr_user');
    set({ token: null, refreshToken: null, user: null, isAuthenticated: false });
  },

  setToken: (token) => {
    set({ token });
  },

  clearAuth: () => {
    localStorage.removeItem('rvr_user');
    set({ token: null, refreshToken: null, user: null, isAuthenticated: false });
  },

  loadFromStorage: () => {
    // Only user display data is persisted — tokens are memory-only.
    // If a user record exists the app should trigger a token refresh flow
    // via the refresh token cookie to rehydrate the access token.
    const userJson = localStorage.getItem('rvr_user');
    if (userJson) {
      try {
        const user = JSON.parse(userJson) as User;
        // isAuthenticated stays false until a new access token is obtained
        set({ user, isAuthenticated: false });
      } catch {
        localStorage.removeItem('rvr_user');
      }
    }
  },
}));
