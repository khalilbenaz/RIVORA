import { NavLink } from 'react-router-dom';
import {
  LayoutDashboard,
  Users,
  Package,
  ScrollText,
  Heart,
  Building2,
  Shield,
  Globe2,
  MessageCircle,
  FolderOpen,
  FolderPlus,
  BarChart3,
  Calendar,
  StickyNote,
  Activity,
  Workflow,
  Code2,
  Columns3,
  LogOut,
  Menu,
  X,
} from 'lucide-react';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '../store/authStore';
import LanguageSwitcher from './LanguageSwitcher';
import NotificationBell from './NotificationBell';
import ThemeToggle from './ThemeToggle';

const navItems = [
  { to: '/', icon: LayoutDashboard, labelKey: 'nav.dashboard', end: true },
  { to: '/users', icon: Users, labelKey: 'nav.users' },
  { to: '/products', icon: Package, labelKey: 'nav.products' },
  { to: '/tenants', icon: Building2, labelKey: 'nav.tenants' },
  { to: '/audit', icon: ScrollText, labelKey: 'nav.audit' },
  { to: '/health', icon: Heart, labelKey: 'nav.health' },
  { to: '/roles', icon: Shield, labelKey: 'nav.roles' },
  { to: '/webhooks', icon: Globe2, labelKey: 'nav.webhooks' },
  { to: '/chat', icon: MessageCircle, labelKey: 'nav.chat' },
  { to: '/files', icon: FolderOpen, labelKey: 'nav.files' },
  { to: '/analytics', icon: BarChart3, labelKey: 'nav.analytics' },
  { to: '/calendar', icon: Calendar, labelKey: 'nav.calendar' },
  { to: '/notes', icon: StickyNote, labelKey: 'nav.notes' },
  { to: '/activity', icon: Activity, labelKey: 'nav.activity' },
  { to: '/flows', icon: Workflow, labelKey: 'nav.flows' },
  { to: '/projects', icon: FolderPlus, labelKey: 'nav.projects' },
  { to: '/generator', icon: Code2, labelKey: 'nav.generator' },
  { to: '/kanban', icon: Columns3, labelKey: 'nav.kanban' },
];

export default function Sidebar() {
  const [open, setOpen] = useState(false);
  const logout = useAuthStore((s) => s.logout);
  const user = useAuthStore((s) => s.user);
  const { t } = useTranslation();

  return (
    <>
      <button
        onClick={() => setOpen(true)}
        className="fixed top-3 left-3 z-50 rounded-lg bg-slate-800 p-2 text-white shadow-lg lg:hidden"
        aria-label="Open menu"
      >
        <Menu size={20} />
      </button>

      {open && (
        <div
          className="fixed inset-0 z-40 bg-black/40 lg:hidden"
          onClick={() => setOpen(false)}
        />
      )}

      <aside
        className={`fixed inset-y-0 left-0 z-50 flex w-64 flex-col bg-slate-900 text-white transition-transform duration-200 lg:translate-x-0 ${
          open ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        <div className="flex items-center justify-between border-b border-slate-700 px-4 py-4">
          <h1 className="bg-gradient-to-r from-blue-400 to-violet-400 bg-clip-text text-xl font-bold text-transparent">
            RIVORA
          </h1>
          <div className="flex items-center gap-2">
            <NotificationBell />
            <ThemeToggle />
            <LanguageSwitcher />
            <button
              onClick={() => setOpen(false)}
              className="rounded p-1 text-slate-400 hover:text-white lg:hidden"
            >
              <X size={18} />
            </button>
          </div>
        </div>

        <nav className="flex-1 overflow-y-auto py-4">
          <ul className="space-y-1 px-3">
            {navItems.map(({ to, icon: Icon, labelKey, end }) => (
              <li key={to}>
                <NavLink
                  to={to}
                  end={end}
                  onClick={() => setOpen(false)}
                  className={({ isActive }) =>
                    `flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors ${
                      isActive
                        ? 'bg-blue-600/20 text-blue-400'
                        : 'text-slate-400 hover:bg-slate-800 hover:text-slate-200'
                    }`
                  }
                >
                  <Icon size={18} />
                  {t(labelKey)}
                </NavLink>
              </li>
            ))}
          </ul>
        </nav>

        <div className="border-t border-slate-700 px-4 py-3">
          <div className="mb-2 text-xs text-slate-500">
            {t('common.connectedAs')} <span className="text-slate-300">{user?.userName}</span>
          </div>
          <button
            onClick={logout}
            className="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-sm text-slate-400 transition-colors hover:bg-slate-800 hover:text-red-400"
          >
            <LogOut size={16} />
            {t('common.logout')}
          </button>
        </div>
      </aside>
    </>
  );
}
