import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import {
  UserPlus,
  UserMinus,
  Package,
  Edit,
  Building2,
  LogIn,
  ShieldAlert,
  Globe,
  Filter,
} from 'lucide-react';
import type { LucideIcon } from 'lucide-react';

interface ActivityItem {
  id: string;
  type: string;
  title: string;
  description: string;
  user: string;
  timestamp: string;
}

const ACTIVITY_TYPES: Record<string, { icon: LucideIcon; colorClass: string; dotClass: string }> = {
  'user.created': { icon: UserPlus, colorClass: 'text-blue-500 bg-blue-100 dark:bg-blue-900/40', dotClass: 'bg-blue-500' },
  'user.deleted': { icon: UserMinus, colorClass: 'text-red-500 bg-red-100 dark:bg-red-900/40', dotClass: 'bg-red-500' },
  'product.created': { icon: Package, colorClass: 'text-green-500 bg-green-100 dark:bg-green-900/40', dotClass: 'bg-green-500' },
  'product.updated': { icon: Edit, colorClass: 'text-amber-500 bg-amber-100 dark:bg-amber-900/40', dotClass: 'bg-amber-500' },
  'tenant.created': { icon: Building2, colorClass: 'text-violet-500 bg-violet-100 dark:bg-violet-900/40', dotClass: 'bg-violet-500' },
  'login.success': { icon: LogIn, colorClass: 'text-green-500 bg-green-100 dark:bg-green-900/40', dotClass: 'bg-green-500' },
  'login.failed': { icon: ShieldAlert, colorClass: 'text-red-500 bg-red-100 dark:bg-red-900/40', dotClass: 'bg-red-500' },
  'webhook.sent': { icon: Globe, colorClass: 'text-blue-500 bg-blue-100 dark:bg-blue-900/40', dotClass: 'bg-blue-500' },
};

const USERS = ['alice', 'bob', 'charlie', 'diana', 'eric', 'fiona'];

function generateMockActivities(): ActivityItem[] {
  const types = Object.keys(ACTIVITY_TYPES);
  const descriptions: Record<string, string[]> = {
    'user.created': ['New user "john_doe" registered via admin panel', 'User "sarah_k" created from API', 'Batch import: user "dev_ops_1" added'],
    'user.deleted': ['User "temp_user" removed by admin', 'Inactive account "old_acc" purged', 'User "test_123" deleted on request'],
    'product.created': ['Product "Enterprise Plan" added to catalog', 'New SKU "widget-pro" created', 'Product "Starter Kit" published'],
    'product.updated': ['Price updated for "Enterprise Plan"', 'Description changed for "widget-pro"', 'Stock level adjusted for "Starter Kit"'],
    'tenant.created': ['Tenant "acme-corp" provisioned', 'New workspace "dev-team" created', 'Tenant "beta-client" onboarded'],
    'login.success': ['Admin login from 192.168.1.42', 'SSO login via Google', 'API key authentication successful'],
    'login.failed': ['Failed login attempt for "admin" from 10.0.0.1', 'Brute force detected: 5 failed attempts', 'Invalid 2FA code for "alice"'],
    'webhook.sent': ['Webhook delivered to https://hooks.example.com', 'Event "user.created" sent to Slack', 'Notification webhook triggered'],
  };

  const items: ActivityItem[] = [];
  for (let i = 0; i < 20; i++) {
    const type = types[i % types.length]!;
    const descs = descriptions[type]!;
    const hoursAgo = Math.random() * 48;
    items.push({
      id: `act-${i}`,
      type,
      title: type.replace('.', ': '),
      description: descs[i % descs.length]!,
      user: USERS[i % USERS.length]!,
      timestamp: new Date(Date.now() - hoursAgo * 3600000).toISOString(),
    });
  }
  return items.sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());
}

function relativeTime(dateStr: string): string {
  const diff = Date.now() - new Date(dateStr).getTime();
  const secs = Math.floor(diff / 1000);
  if (secs < 60) return 'just now';
  const mins = Math.floor(secs / 60);
  if (mins < 60) return `${mins}m ago`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  return `${days}d ago`;
}

function getInitials(name: string): string {
  return name.slice(0, 2).toUpperCase();
}

const AVATAR_COLORS = [
  'bg-blue-500', 'bg-green-500', 'bg-amber-500', 'bg-violet-500', 'bg-pink-500', 'bg-cyan-500',
];

export default function ActivityPage() {
  const { t } = useTranslation();
  const [allActivities] = useState<ActivityItem[]>(generateMockActivities);
  const [visibleCount, setVisibleCount] = useState(10);
  const [filterType, setFilterType] = useState<string>('all');

  const filtered = useMemo(() => {
    if (filterType === 'all') return allActivities;
    return allActivities.filter((a) => a.type === filterType);
  }, [allActivities, filterType]);

  const visible = filtered.slice(0, visibleCount);
  const hasMore = visibleCount < filtered.length;

  return (
    <div>
      {/* Header */}
      <div className="mb-6 flex flex-wrap items-center justify-between gap-4">
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">{t('activity.title')}</h1>
        <div className="flex items-center gap-2">
          <Filter size={16} className="text-slate-400" />
          <select
            value={filterType}
            onChange={(e) => { setFilterType(e.target.value); setVisibleCount(10); }}
            className="rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-slate-600 dark:bg-slate-700 dark:text-white"
          >
            <option value="all">{t('activity.allTypes')}</option>
            {Object.keys(ACTIVITY_TYPES).map((type) => (
              <option key={type} value={type}>{type}</option>
            ))}
          </select>
        </div>
      </div>

      {/* Timeline */}
      <div className="relative">
        {/* Vertical line */}
        <div className="absolute left-6 top-0 bottom-0 w-0.5 bg-slate-200 dark:bg-slate-700" />

        <div className="space-y-0">
          {visible.map((item) => {
            const config = ACTIVITY_TYPES[item.type] ?? ACTIVITY_TYPES['webhook.sent']!;
            const Icon = config.icon;
            const avatarColor = AVATAR_COLORS[USERS.indexOf(item.user) % AVATAR_COLORS.length];

            return (
              <div key={item.id} className="relative flex gap-4 pb-6 pl-12">
                {/* Timeline dot */}
                <div className={`absolute left-[18px] top-1.5 h-3 w-3 rounded-full ${config.dotClass} ring-4 ring-white dark:ring-slate-900`} />

                {/* Content card */}
                <div className="flex-1 rounded-xl border border-slate-200 bg-white p-4 shadow-sm transition hover:shadow-md dark:border-slate-700 dark:bg-slate-800">
                  <div className="flex items-start gap-3">
                    {/* Icon */}
                    <div className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-lg ${config.colorClass}`}>
                      <Icon size={18} />
                    </div>
                    {/* Details */}
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2">
                        <span className="text-sm font-semibold text-slate-800 dark:text-slate-100">{item.title}</span>
                        <span className="text-xs text-slate-400">{relativeTime(item.timestamp)}</span>
                      </div>
                      <p className="mt-0.5 text-sm text-slate-600 dark:text-slate-300">{item.description}</p>
                    </div>
                    {/* User avatar */}
                    <div
                      className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-xs font-bold text-white ${avatarColor}`}
                      title={item.user}
                    >
                      {getInitials(item.user)}
                    </div>
                  </div>
                </div>
              </div>
            );
          })}
        </div>

        {visible.length === 0 && (
          <div className="py-12 text-center text-slate-400">{t('activity.noActivity')}</div>
        )}

        {/* Load more */}
        {hasMore && (
          <div className="mt-4 flex justify-center">
            <button
              onClick={() => setVisibleCount((c) => c + 10)}
              className="rounded-lg border border-slate-300 px-6 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-100 dark:border-slate-600 dark:text-slate-300 dark:hover:bg-slate-700"
            >
              {t('activity.loadMore')}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
