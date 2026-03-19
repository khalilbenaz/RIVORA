import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useSessionRefresh } from '../useSessionRefresh';
import { useAuthStore } from '../../store/authStore';

// Helper to create a fake JWT with a given exp timestamp
function makeJwt(exp: number): string {
  const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
  const payload = btoa(JSON.stringify({ sub: '123', exp }));
  const signature = 'fake-signature';
  return `${header}.${payload}.${signature}`;
}

describe('useSessionRefresh', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    // Reset auth store
    useAuthStore.setState({ token: null, user: null, isAuthenticated: false });
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('returns null expiresIn when there is no token', () => {
    const { result } = renderHook(() => useSessionRefresh());
    expect(result.current.expiresIn).toBeNull();
    expect(result.current.showWarning).toBe(false);
  });

  it('detects expiration and computes expiresIn', () => {
    const now = Date.now();
    const expInSeconds = Math.floor(now / 1000) + 600; // 10 minutes from now
    const token = makeJwt(expInSeconds);

    useAuthStore.setState({ token, user: { id: '1', userName: 'test', email: 'test@test.com', firstName: null, lastName: null, isActive: true, twoFactorEnabled: false, createdAt: '' } as unknown as import('../../types').User, isAuthenticated: true});

    const { result } = renderHook(() => useSessionRefresh());

    // expiresIn should be roughly 600000ms (10 minutes)
    expect(result.current.expiresIn).toBeGreaterThan(500000);
    expect(result.current.expiresIn).toBeLessThanOrEqual(600000);
    expect(result.current.showWarning).toBe(false);
  });

  it('shows warning when token expires within 5 minutes', () => {
    const now = Date.now();
    const expInSeconds = Math.floor(now / 1000) + 120; // 2 minutes from now
    const token = makeJwt(expInSeconds);

    useAuthStore.setState({ token, user: { id: '1', userName: 'test', email: 'test@test.com', firstName: null, lastName: null, isActive: true, twoFactorEnabled: false, createdAt: '' } as unknown as import('../../types').User, isAuthenticated: true});

    const { result } = renderHook(() => useSessionRefresh());

    expect(result.current.showWarning).toBe(true);
  });

  it('calls logout when token is already expired', () => {
    const now = Date.now();
    const expInSeconds = Math.floor(now / 1000) - 60; // 1 minute ago
    const token = makeJwt(expInSeconds);

    useAuthStore.setState({ token, user: { id: '1', userName: 'test', email: 'test@test.com', firstName: null, lastName: null, isActive: true, twoFactorEnabled: false, createdAt: '' } as unknown as import('../../types').User, isAuthenticated: true});

    renderHook(() => useSessionRefresh());

    // After logout, token should be cleared
    expect(useAuthStore.getState().token).toBeNull();
    expect(useAuthStore.getState().isAuthenticated).toBe(false);
  });

  it('returns null expiresIn for invalid token', () => {
    useAuthStore.setState({ token: 'not-a-jwt', user: null, isAuthenticated: true });

    const { result } = renderHook(() => useSessionRefresh());
    expect(result.current.expiresIn).toBeNull();
  });

  it('dismiss hides the warning', () => {
    const now = Date.now();
    const expInSeconds = Math.floor(now / 1000) + 120;
    const token = makeJwt(expInSeconds);

    useAuthStore.setState({ token, user: { id: '1', userName: 'test', email: 'test@test.com', firstName: null, lastName: null, isActive: true, twoFactorEnabled: false, createdAt: '' } as unknown as import('../../types').User, isAuthenticated: true});

    const { result } = renderHook(() => useSessionRefresh());
    expect(result.current.showWarning).toBe(true);

    act(() => {
      result.current.dismiss();
    });

    expect(result.current.showWarning).toBe(false);
  });
});
