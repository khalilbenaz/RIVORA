import { useState, useEffect, useRef } from 'react';
import { healthApi } from '../api/health';
import StatCard from '../components/StatCard';
import Badge from '../components/Badge';
import Spinner from '../components/Spinner';
import { RefreshCw, Heart } from 'lucide-react';
import type { HealthCheck } from '../types';

export default function HealthPage() {
  const [health, setHealth] = useState<HealthCheck | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [autoRefresh, setAutoRefresh] = useState(false);
  const [lastUpdate, setLastUpdate] = useState<Date | null>(null);
  const interval = useRef<ReturnType<typeof setInterval>>(undefined);

  const fetchHealth = async () => {
    setLoading(true);
    try {
      const res = await healthApi.check();
      setHealth(res.data);
      setError(false);
    } catch {
      setError(true);
    } finally {
      setLoading(false);
      setLastUpdate(new Date());
    }
  };

  useEffect(() => {
    void fetchHealth();
    //run once on mount
  }, []);

  useEffect(() => {
    if (autoRefresh) {
      interval.current = setInterval(() => void fetchHealth(), 30000);
    }
    return () => clearInterval(interval.current);
    //fetchHealth is stable (no deps)
  }, [autoRefresh]);

  const entries = health ? Object.entries(health.entries) : [];
  const allHealthy = entries.every(([, e]) => e.status === 'Healthy');

  if (loading && !health) return <Spinner />;

  return (
    <div>
      <div className="mb-6 flex flex-wrap items-center justify-between gap-4">
        <h1 className="text-2xl font-bold text-slate-900">Health Dashboard</h1>
        <div className="flex items-center gap-3">
          <label className="flex items-center gap-2 text-sm text-slate-600">
            <input
              type="checkbox"
              checked={autoRefresh}
              onChange={(e) => setAutoRefresh(e.target.checked)}
              className="accent-blue-500"
            />
            Auto-refresh (30s)
          </label>
          <button
            onClick={() => void fetchHealth()}
            disabled={loading}
            className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700 disabled:opacity-60"
          >
            <RefreshCw size={14} className={loading ? 'animate-spin' : ''} />
            Rafraîchir
          </button>
        </div>
      </div>

      <div className="mb-8 grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
        <StatCard
          label="Statut global"
          value={error ? 'Erreur' : allHealthy ? 'Sain' : 'Dégradé'}
          variant={error ? 'danger' : allHealthy ? 'success' : 'warning'}
          icon={<Heart size={20} />}
        />
        <StatCard
          label="Services"
          value={entries.length}
          detail={`${entries.filter(([, e]) => e.status === 'Healthy').length} sains`}
        />
        <StatCard
          label="Durée totale"
          value={health?.totalDuration ?? '-'}
        />
      </div>

      <h2 className="mb-3 text-lg font-semibold text-slate-800">Détails des services</h2>
      <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
        <table className="w-full text-sm">
          <thead className="bg-slate-50">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">Service</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">Statut</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">Durée</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">Description</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {entries.map(([name, entry]) => (
              <tr key={name} className="transition-colors hover:bg-slate-50">
                <td className="px-4 py-3 font-medium text-slate-900">{name}</td>
                <td className="px-4 py-3">
                  <Badge variant={entry.status === 'Healthy' ? 'success' : entry.status === 'Degraded' ? 'warning' : 'danger'}>
                    {entry.status}
                  </Badge>
                </td>
                <td className="px-4 py-3 text-slate-600">{entry.duration}</td>
                <td className="px-4 py-3 text-slate-500">{entry.description ?? '-'}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {entries.length === 0 && !error && (
          <div className="py-8 text-center text-slate-400">Aucun service health check configuré.</div>
        )}
        {error && (
          <div className="py-8 text-center text-red-400">Impossible de joindre les health checks.</div>
        )}
      </div>

      {lastUpdate && (
        <p className="mt-3 text-xs text-slate-500">
          Dernière mise à jour : {lastUpdate.toLocaleTimeString('fr-FR')}
        </p>
      )}
    </div>
  );
}
