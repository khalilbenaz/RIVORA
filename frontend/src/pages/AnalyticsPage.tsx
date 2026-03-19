import { useMemo } from 'react';
import {
  DollarSign,
  Users,
  Activity,
  Zap,
  TrendingUp,
  TrendingDown,
} from 'lucide-react';
import { useTranslation } from 'react-i18next';
import BarChart from '../components/charts/BarChart';
import LineChart from '../components/charts/LineChart';
import DonutChart from '../components/charts/DonutChart';

// Deterministic pseudo-random seeded generator for stable mock data
function seededRandom(seed: number) {
  let s = seed;
  return () => {
    s = (s * 16807 + 0) % 2147483647;
    return (s - 1) / 2147483646;
  };
}

function generateMockData() {
  const rand = seededRandom(42);

  const revenue = Math.round(10000 + rand() * 40000);
  const revenueTrend = rand() > 0.3 ? 'up' : 'down';
  const revenuePct = +(rand() * 15 + 1).toFixed(1);

  const newUsers = Math.round(200 + rand() * 1800);
  const usersTrend = rand() > 0.4 ? 'up' : 'down';
  const usersPct = +(rand() * 20 + 1).toFixed(1);

  const sessions = Math.round(500 + rand() * 4500);
  const sessionsTrend = rand() > 0.5 ? 'up' : 'down';
  const sessionsPct = +(rand() * 12 + 1).toFixed(1);

  const apiCalls = Math.round(5000 + rand() * 45000);
  const apiTrend = rand() > 0.3 ? 'up' : 'down';
  const apiPct = +(rand() * 25 + 1).toFixed(1);

  const days = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
  const apiPerDay = days.map((label) => ({
    label,
    value: Math.round(1000 + rand() * 9000),
  }));

  const usersOverTime = Array.from({ length: 30 }, (_, i) => ({
    label: `D${i + 1}`,
    value: Math.round(100 + rand() * 500 + i * 8),
  }));

  const trafficSources = [
    { label: 'Direct', value: Math.round(200 + rand() * 2000), color: '#3b82f6' },
    { label: 'Organic', value: Math.round(200 + rand() * 1500), color: '#8b5cf6' },
    { label: 'Referral', value: Math.round(100 + rand() * 800), color: '#f59e0b' },
    { label: 'Social', value: Math.round(100 + rand() * 600), color: '#10b981' },
    { label: 'Email', value: Math.round(50 + rand() * 400), color: '#ef4444' },
  ];

  const endpoints = [
    { path: '/api/users', method: 'GET', count: Math.round(2000 + rand() * 8000), avgMs: Math.round(20 + rand() * 180) },
    { path: '/api/products', method: 'GET', count: Math.round(1500 + rand() * 6000), avgMs: Math.round(30 + rand() * 150) },
    { path: '/api/auth/login', method: 'POST', count: Math.round(1000 + rand() * 5000), avgMs: Math.round(50 + rand() * 200) },
    { path: '/api/files', method: 'POST', count: Math.round(500 + rand() * 3000), avgMs: Math.round(100 + rand() * 400) },
    { path: '/api/tenants', method: 'GET', count: Math.round(300 + rand() * 2000), avgMs: Math.round(15 + rand() * 100) },
    { path: '/api/audit', method: 'GET', count: Math.round(200 + rand() * 1500), avgMs: Math.round(40 + rand() * 250) },
  ].sort((a, b) => b.count - a.count);

  return {
    stats: { revenue, revenueTrend, revenuePct, newUsers, usersTrend, usersPct, sessions, sessionsTrend, sessionsPct, apiCalls, apiTrend, apiPct },
    apiPerDay,
    usersOverTime,
    trafficSources,
    endpoints,
  };
}

function TrendBadge({ trend, pct }: { trend: 'up' | 'down'; pct: number }) {
  const isUp = trend === 'up';
  return (
    <span
      className={`inline-flex items-center gap-0.5 rounded-full px-2 py-0.5 text-xs font-medium ${
        isUp ? 'bg-emerald-50 text-emerald-600' : 'bg-red-50 text-red-600'
      }`}
    >
      {isUp ? <TrendingUp size={12} /> : <TrendingDown size={12} />}
      {pct}%
    </span>
  );
}

export default function AnalyticsPage() {
  const { t } = useTranslation();
  const data = useMemo(() => generateMockData(), []);

  const statCards = [
    {
      label: t('analytics.revenue'),
      value: `$${data.stats.revenue.toLocaleString()}`,
      trend: data.stats.revenueTrend as 'up' | 'down',
      pct: data.stats.revenuePct,
      icon: <DollarSign size={20} />,
      color: 'text-emerald-600 bg-emerald-50',
    },
    {
      label: t('analytics.newUsers'),
      value: data.stats.newUsers.toLocaleString(),
      trend: data.stats.usersTrend as 'up' | 'down',
      pct: data.stats.usersPct,
      icon: <Users size={20} />,
      color: 'text-blue-600 bg-blue-50',
    },
    {
      label: t('analytics.activeSessions'),
      value: data.stats.sessions.toLocaleString(),
      trend: data.stats.sessionsTrend as 'up' | 'down',
      pct: data.stats.sessionsPct,
      icon: <Activity size={20} />,
      color: 'text-violet-600 bg-violet-50',
    },
    {
      label: t('analytics.apiCalls'),
      value: data.stats.apiCalls.toLocaleString(),
      trend: data.stats.apiTrend as 'up' | 'down',
      pct: data.stats.apiPct,
      icon: <Zap size={20} />,
      color: 'text-amber-600 bg-amber-50',
    },
  ];

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold text-slate-900">{t('analytics.title')}</h1>

      {/* Row 1: Stat cards */}
      <div className="mb-8 grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {statCards.map((card) => (
          <div
            key={card.label}
            className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm transition-shadow hover:shadow-md"
          >
            <div className="flex items-center justify-between">
              <p className="text-xs font-semibold uppercase tracking-wider text-slate-500">
                {card.label}
              </p>
              <div className={`rounded-lg p-2 ${card.color}`}>{card.icon}</div>
            </div>
            <p className="mt-2 text-3xl font-bold text-slate-900 tabular-nums">{card.value}</p>
            <div className="mt-1">
              <TrendBadge trend={card.trend} pct={card.pct} />
              <span className="ml-1.5 text-xs text-slate-500">{t('analytics.vsLastMonth')}</span>
            </div>
          </div>
        ))}
      </div>

      {/* Row 2: Bar + Line charts */}
      <div className="mb-8 grid grid-cols-1 gap-6 lg:grid-cols-2">
        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <h2 className="mb-4 text-sm font-semibold text-slate-800">
            {t('analytics.apiCallsPerDay')}
          </h2>
          <div className="overflow-x-auto">
            <BarChart data={data.apiPerDay} height={240} color="#3b82f6" />
          </div>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <h2 className="mb-4 text-sm font-semibold text-slate-800">
            {t('analytics.usersOverTime')}
          </h2>
          <div className="overflow-x-auto">
            <LineChart data={data.usersOverTime} height={240} color="#8b5cf6" />
          </div>
        </div>
      </div>

      {/* Row 3: Donut + Table */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <h2 className="mb-4 text-sm font-semibold text-slate-800">
            {t('analytics.trafficBySource')}
          </h2>
          <div className="flex items-center justify-center py-4">
            <DonutChart data={data.trafficSources} size={200} />
          </div>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <h2 className="mb-4 text-sm font-semibold text-slate-800">
            {t('analytics.topEndpoints')}
          </h2>
          <div className="overflow-hidden rounded-lg border border-slate-100">
            <table className="w-full text-sm">
              <thead className="bg-slate-50">
                <tr>
                  <th className="px-3 py-2.5 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Endpoint
                  </th>
                  <th className="px-3 py-2.5 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Method
                  </th>
                  <th className="px-3 py-2.5 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Requests
                  </th>
                  <th className="px-3 py-2.5 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Avg (ms)
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {data.endpoints.map((ep) => (
                  <tr key={ep.path + ep.method} className="transition-colors hover:bg-slate-50">
                    <td className="px-3 py-2 font-mono text-xs text-slate-700">{ep.path}</td>
                    <td className="px-3 py-2">
                      <span className="rounded bg-blue-50 px-1.5 py-0.5 text-xs font-medium text-blue-700">
                        {ep.method}
                      </span>
                    </td>
                    <td className="px-3 py-2 text-right tabular-nums text-slate-600">
                      {ep.count.toLocaleString()}
                    </td>
                    <td className="px-3 py-2 text-right tabular-nums text-slate-600">
                      {ep.avgMs}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}
