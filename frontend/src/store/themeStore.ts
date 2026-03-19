import { create } from 'zustand';

type Theme = 'light' | 'dark' | 'system';

interface ThemeState {
  theme: Theme;
  setTheme: (theme: Theme) => void;
  resolvedTheme: 'light' | 'dark';
}

function getSystemTheme(): 'light' | 'dark' {
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
}

function applyTheme(theme: Theme) {
  const resolved = theme === 'system' ? getSystemTheme() : theme;
  document.documentElement.classList.toggle('dark', resolved === 'dark');
  return resolved;
}

export const useThemeStore = create<ThemeState>((set) => ({
  theme: (localStorage.getItem('rvr_theme') as Theme) || 'system',
  resolvedTheme: applyTheme((localStorage.getItem('rvr_theme') as Theme) || 'system'),
  setTheme: (theme) => {
    localStorage.setItem('rvr_theme', theme);
    const resolved = applyTheme(theme);
    set({ theme, resolvedTheme: resolved });
  },
}));
