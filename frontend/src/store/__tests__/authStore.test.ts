import { describe, it, expect, beforeEach } from 'vitest';
import { useAuthStore } from '../authStore';
import type { User } from '../../types';

const mockUser: User = {
  id: '1',
  userName: 'admin',
  email: 'admin@rivora.io',
  firstName: 'Admin',
  lastName: 'User',
  isActive: true,
  emailConfirmed: true,
  twoFactorEnabled: false,
  createdAt: '2025-01-01T00:00:00Z',
};

describe('authStore', () => {
  beforeEach(() => {
    localStorage.clear();
    useAuthStore.setState({ token: null, refreshToken: null, user: null, isAuthenticated: false });
  });

  it('starts with no authentication', () => {
    const state = useAuthStore.getState();
    expect(state.token).toBeNull();
    expect(state.refreshToken).toBeNull();
    expect(state.user).toBeNull();
    expect(state.isAuthenticated).toBe(false);
  });

  it('login sets token, refreshToken, user, and isAuthenticated', () => {
    useAuthStore.getState().login('test-token', 'test-refresh', mockUser);

    const state = useAuthStore.getState();
    expect(state.token).toBe('test-token');
    expect(state.refreshToken).toBe('test-refresh');
    expect(state.user).toEqual(mockUser);
    expect(state.isAuthenticated).toBe(true);
  });

  it('login persists only user to localStorage (not tokens)', () => {
    useAuthStore.getState().login('test-token', 'test-refresh', mockUser);

    // Tokens must NOT be in localStorage (XSS protection)
    expect(localStorage.getItem('rvr_token')).toBeNull();
    // User display data is OK in localStorage
    expect(JSON.parse(localStorage.getItem('rvr_user')!)).toEqual(mockUser);
  });

  it('logout clears token, refreshToken, user, and isAuthenticated', () => {
    useAuthStore.getState().login('test-token', 'test-refresh', mockUser);
    useAuthStore.getState().logout();

    const state = useAuthStore.getState();
    expect(state.token).toBeNull();
    expect(state.refreshToken).toBeNull();
    expect(state.user).toBeNull();
    expect(state.isAuthenticated).toBe(false);
  });

  it('logout removes user from localStorage', () => {
    useAuthStore.getState().login('test-token', 'test-refresh', mockUser);
    useAuthStore.getState().logout();

    expect(localStorage.getItem('rvr_user')).toBeNull();
  });

  it('setToken updates the in-memory token', () => {
    useAuthStore.getState().setToken('new-token');

    const state = useAuthStore.getState();
    expect(state.token).toBe('new-token');
    // Should NOT persist to localStorage
    expect(localStorage.getItem('rvr_token')).toBeNull();
  });

  it('clearAuth clears state and localStorage', () => {
    useAuthStore.getState().login('test-token', 'test-refresh', mockUser);
    useAuthStore.getState().clearAuth();

    const state = useAuthStore.getState();
    expect(state.token).toBeNull();
    expect(state.refreshToken).toBeNull();
    expect(state.user).toBeNull();
    expect(state.isAuthenticated).toBe(false);
    expect(localStorage.getItem('rvr_user')).toBeNull();
  });

  it('loadFromStorage restores user but not auth state', () => {
    localStorage.setItem('rvr_user', JSON.stringify(mockUser));

    useAuthStore.getState().loadFromStorage();

    const state = useAuthStore.getState();
    expect(state.user).toEqual(mockUser);
    // isAuthenticated should be false — a token refresh is needed
    expect(state.isAuthenticated).toBe(false);
    expect(state.token).toBeNull();
  });

  it('loadFromStorage does nothing when storage is empty', () => {
    useAuthStore.getState().loadFromStorage();

    const state = useAuthStore.getState();
    expect(state.token).toBeNull();
    expect(state.isAuthenticated).toBe(false);
  });

  it('loadFromStorage handles invalid JSON gracefully', () => {
    localStorage.setItem('rvr_user', 'not-valid-json');

    useAuthStore.getState().loadFromStorage();

    const state = useAuthStore.getState();
    expect(state.user).toBeNull();
    expect(state.isAuthenticated).toBe(false);
    // Should clean up bad data
    expect(localStorage.getItem('rvr_user')).toBeNull();
  });
});
