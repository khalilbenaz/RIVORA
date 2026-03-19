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
    useAuthStore.setState({ token: null, user: null, isAuthenticated: false });
  });

  it('starts with no authentication', () => {
    const state = useAuthStore.getState();
    expect(state.token).toBeNull();
    expect(state.user).toBeNull();
    expect(state.isAuthenticated).toBe(false);
  });

  it('login sets token, user, and isAuthenticated', () => {
    useAuthStore.getState().login('test-token', mockUser);

    const state = useAuthStore.getState();
    expect(state.token).toBe('test-token');
    expect(state.user).toEqual(mockUser);
    expect(state.isAuthenticated).toBe(true);
  });

  it('login persists to localStorage', () => {
    useAuthStore.getState().login('test-token', mockUser);

    expect(localStorage.getItem('rvr_token')).toBe('test-token');
    expect(JSON.parse(localStorage.getItem('rvr_user')!)).toEqual(mockUser);
  });

  it('logout clears token, user, and isAuthenticated', () => {
    useAuthStore.getState().login('test-token', mockUser);
    useAuthStore.getState().logout();

    const state = useAuthStore.getState();
    expect(state.token).toBeNull();
    expect(state.user).toBeNull();
    expect(state.isAuthenticated).toBe(false);
  });

  it('logout removes from localStorage', () => {
    useAuthStore.getState().login('test-token', mockUser);
    useAuthStore.getState().logout();

    expect(localStorage.getItem('rvr_token')).toBeNull();
    expect(localStorage.getItem('rvr_user')).toBeNull();
  });

  it('loadFromStorage restores auth state', () => {
    localStorage.setItem('rvr_token', 'stored-token');
    localStorage.setItem('rvr_user', JSON.stringify(mockUser));

    useAuthStore.getState().loadFromStorage();

    const state = useAuthStore.getState();
    expect(state.token).toBe('stored-token');
    expect(state.user).toEqual(mockUser);
    expect(state.isAuthenticated).toBe(true);
  });

  it('loadFromStorage does nothing when storage is empty', () => {
    useAuthStore.getState().loadFromStorage();

    const state = useAuthStore.getState();
    expect(state.token).toBeNull();
    expect(state.isAuthenticated).toBe(false);
  });

  it('loadFromStorage handles invalid JSON gracefully', () => {
    localStorage.setItem('rvr_token', 'some-token');
    localStorage.setItem('rvr_user', 'not-valid-json');

    useAuthStore.getState().loadFromStorage();

    const state = useAuthStore.getState();
    expect(state.token).toBeNull();
    expect(state.isAuthenticated).toBe(false);
    // Should also clean up bad data
    expect(localStorage.getItem('rvr_token')).toBeNull();
  });
});
