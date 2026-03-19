import { NavLink } from 'react-router-dom';
import { LayoutDashboard, Settings, Bell, LogOut } from 'lucide-react';
import { useAuthStore } from '../../store/authStore';
import { useSignalR } from '../../hooks/useSignalR';

export default function MobileNav() {
  const logout = useAuthStore((s) => s.logout);
  const { unreadCount } = useSignalR();

  const baseCls =
    'flex flex-col items-center gap-0.5 text-[10px] font-medium transition-colors';
  const activeCls = 'text-blue-600';
  const inactiveCls = 'text-slate-400 hover:text-slate-600';

  return (
    <nav className="fixed bottom-0 inset-x-0 z-50 border-t border-slate-200 bg-white shadow-[0_-1px_3px_rgba(0,0,0,0.05)] lg:hidden">
      <div className="flex items-center justify-around py-2">
        <NavLink
          to="/app"
          end
          className={({ isActive }) =>
            `${baseCls} ${isActive ? activeCls : inactiveCls}`
          }
        >
          <LayoutDashboard size={20} />
          <span>Dashboard</span>
        </NavLink>

        <NavLink
          to="/app/settings"
          className={({ isActive }) =>
            `${baseCls} ${isActive ? activeCls : inactiveCls}`
          }
        >
          <Settings size={20} />
          <span>Settings</span>
        </NavLink>

        <NavLink
          to="/app"
          end
          className={() => `${baseCls} ${inactiveCls} relative`}
          onClick={(e) => {
            e.preventDefault();
            // Notification bell is informational; navigating to dashboard
          }}
        >
          <div className="relative">
            <Bell size={20} />
            {unreadCount > 0 && (
              <span className="absolute -right-1.5 -top-1 flex h-3.5 min-w-[14px] items-center justify-center rounded-full bg-red-500 px-0.5 text-[9px] font-bold text-white">
                {unreadCount}
              </span>
            )}
          </div>
          <span>Notifications</span>
        </NavLink>

        <button
          onClick={logout}
          className={`${baseCls} ${inactiveCls}`}
        >
          <LogOut size={20} />
          <span>Logout</span>
        </button>
      </div>
    </nav>
  );
}
