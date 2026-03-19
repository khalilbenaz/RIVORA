import { describe, it, expect, beforeEach, vi } from 'vitest';

// Mock matchMedia before importing the store (it runs on import)
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation((query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});

const { useThemeStore } = await import('../themeStore');

describe('themeStore', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.classList.remove('dark');
    useThemeStore.setState({ theme: 'system', resolvedTheme: 'light' });
  });

  it('defaults to system theme', () => {
    const state = useThemeStore.getState();
    expect(state.theme).toBe('system');
  });

  it('setTheme("dark") updates state and localStorage', () => {
    useThemeStore.getState().setTheme('dark');

    const state = useThemeStore.getState();
    expect(state.theme).toBe('dark');
    expect(state.resolvedTheme).toBe('dark');
    expect(localStorage.getItem('rvr_theme')).toBe('dark');
  });

  it('setTheme("light") updates state and localStorage', () => {
    useThemeStore.getState().setTheme('light');

    const state = useThemeStore.getState();
    expect(state.theme).toBe('light');
    expect(state.resolvedTheme).toBe('light');
    expect(localStorage.getItem('rvr_theme')).toBe('light');
  });

  it('applies "dark" class to document.documentElement when dark', () => {
    useThemeStore.getState().setTheme('dark');
    expect(document.documentElement.classList.contains('dark')).toBe(true);
  });

  it('removes "dark" class when switching to light', () => {
    useThemeStore.getState().setTheme('dark');
    expect(document.documentElement.classList.contains('dark')).toBe(true);

    useThemeStore.getState().setTheme('light');
    expect(document.documentElement.classList.contains('dark')).toBe(false);
  });
});
