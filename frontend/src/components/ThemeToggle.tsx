import { Sun, Moon, Monitor } from 'lucide-react';
import { useThemeStore } from '../store/themeStore';

const options = [
  { value: 'light' as const, icon: Sun, label: 'Light' },
  { value: 'dark' as const, icon: Moon, label: 'Dark' },
  { value: 'system' as const, icon: Monitor, label: 'System' },
];

export default function ThemeToggle() {
  const { theme, setTheme } = useThemeStore();

  return (
    <div className="flex items-center rounded-lg bg-slate-800 p-0.5">
      {options.map(({ value, icon: Icon, label }) => (
        <button
          key={value}
          onClick={() => setTheme(value)}
          aria-label={label}
          className={`rounded-md p-1.5 transition-colors ${
            theme === value
              ? 'bg-slate-600 text-white'
              : 'text-slate-400 hover:text-slate-200'
          }`}
        >
          <Icon size={14} />
        </button>
      ))}
    </div>
  );
}
