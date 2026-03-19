import { create } from 'zustand';
import type { User } from '../types';

interface AuthState {
  token: string | null;
  user: User | null;
  isAuthenticated: boolean;
  login: (token: string, user: User) => void;
  logout: () => void;
  loadFromStorage: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  token: null,
  user: null,
  isAuthenticated: false,

  login: (token, user) => {
    localStorage.setItem('rvr_token', token);
    localStorage.setItem('rvr_user', JSON.stringify(user));
    set({ token, user, isAuthenticated: true });
  },

  logout: () => {
    localStorage.removeItem('rvr_token');
    localStorage.removeItem('rvr_user');
    set({ token: null, user: null, isAuthenticated: false });
  },

  loadFromStorage: () => {
    const token = localStorage.getItem('rvr_token');
    const userJson = localStorage.getItem('rvr_user');
    if (token && userJson) {
      try {
        const user = JSON.parse(userJson) as User;
        set({ token, user, isAuthenticated: true });
      } catch {
        localStorage.removeItem('rvr_token');
        localStorage.removeItem('rvr_user');
      }
    }
  },
}));
