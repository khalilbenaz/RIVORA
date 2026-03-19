import { Outlet, Navigate, NavLink, Link } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';
import { LayoutDashboard, Settings, LogOut, Menu, X } from 'lucide-react';
import NotificationBell from '../NotificationBell';
import SessionWarning from '../SessionWarning';
import { useState } from 'react';
import MobileNav from './MobileNav';

export default function ClientLayout() {
  const { isAuthenticated, user, logout } = useAuthStore();
  const [mobileOpen, setMobileOpen] = useState(false);

  if (!isAuthenticated) return <Navigate to="/app/login" replace />;

  return (
    <div className="min-h-screen bg-slate-50">
      <SessionWarning />
      {/* Top navbar */}
      <nav className="sticky top-0 z-50 border-b border-slate-200 bg-white/80 backdrop-blur-lg">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-3">
          <div className="flex items-center gap-6">
            <Link to="/app" className="flex items-center gap-2">
              <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-blue-600 text-xs font-bold text-white">R</div>
              <span className="text-lg font-bold text-slate-900">RIVORA</span>
            </Link>
            <div className="hidden items-center gap-1 md:flex">
              <NavLink
                to="/app"
                end
                className={({ isActive }) =>
                  `flex items-center gap-1.5 rounded-lg px-3 py-2 text-sm font-medium transition ${
                    isActive ? 'bg-blue-50 text-blue-700' : 'text-slate-600 hover:bg-slate-100'
                  }`
                }
              >
                <LayoutDashboard size={15} /> Dashboard
              </NavLink>
              <NavLink
                to="/app/settings"
                className={({ isActive }) =>
                  `flex items-center gap-1.5 rounded-lg px-3 py-2 text-sm font-medium transition ${
                    isActive ? 'bg-blue-50 text-blue-700' : 'text-slate-600 hover:bg-slate-100'
                  }`
                }
              >
                <Settings size={15} /> Paramètres
              </NavLink>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <div className="hidden text-sm text-slate-600 md:block">
              {user?.firstName || user?.userName}
            </div>
            <NotificationBell />
            <button
              onClick={logout}
              className="rounded-lg p-2 text-slate-400 transition hover:bg-red-50 hover:text-red-600"
              title="Déconnexion"
            >
              <LogOut size={18} />
            </button>
            <button
              onClick={() => setMobileOpen(!mobileOpen)}
              className="rounded-lg p-2 text-slate-600 md:hidden"
            >
              {mobileOpen ? <X size={20} /> : <Menu size={20} />}
            </button>
          </div>
        </div>

        {/* Mobile nav */}
        {mobileOpen && (
          <div className="border-t border-slate-200 bg-white px-6 py-3 md:hidden">
            <NavLink to="/app" end onClick={() => setMobileOpen(false)} className="block rounded-lg px-3 py-2 text-sm text-slate-700 hover:bg-slate-100">
              Dashboard
            </NavLink>
            <NavLink to="/app/settings" onClick={() => setMobileOpen(false)} className="block rounded-lg px-3 py-2 text-sm text-slate-700 hover:bg-slate-100">
              Paramètres
            </NavLink>
          </div>
        )}
      </nav>

      <main className="mx-auto max-w-6xl px-6 py-8 pb-16 lg:pb-0">
        <Outlet />
      </main>

      <MobileNav />
    </div>
  );
}
