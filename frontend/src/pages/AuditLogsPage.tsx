import { useState } from 'react';
import { auditApi } from '../api/audit';
import { useApi } from '../hooks/useApi';
import Badge from '../components/Badge';
import Spinner from '../components/Spinner';
import type { AuditLog } from '../types';
import { Search, RefreshCw, Download } from 'lucide-react';
import { exportToCsv } from '../utils/exportCsv';
import Pagination from '../components/Pagination';

export default function AuditLogsPage() {
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [page, setPage] = useState(1);
  const pageSize = 50;
  const { data: logs, loading, refetch } = useApi<AuditLog[]>(() => auditApi.getAll());

  const filtered = (logs ?? []).filter((l) => {
    if (statusFilter === 'success' && (l.httpStatusCode ?? 0) >= 400) return false;
    if (statusFilter === 'error' && (l.httpStatusCode ?? 0) < 400) return false;
    if (!search) return true;
    const s = search.toLowerCase();
    return (
      (l.url?.toLowerCase().includes(s) ?? false) ||
      (l.httpMethod?.toLowerCase().includes(s) ?? false)
    );
  });

  if (loading) return <Spinner />;

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900">Audit Logs</h1>
        <div className="flex items-center gap-2">
          <button
            onClick={() =>
              exportToCsv(filtered, 'audit_logs', [
                { key: 'executionDate', label: 'Date' },
                { key: 'httpMethod', label: 'Méthode' },
                { key: 'url', label: 'URL' },
                { key: 'httpStatusCode', label: 'Status' },
                { key: 'ipAddress', label: 'IP' },
                { key: 'executionTime', label: 'Durée (ms)' },
                { key: 'exceptionMessage', label: 'Exception' },
              ])
            }
            className="inline-flex items-center gap-2 rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-600 transition hover:bg-slate-100"
            title="Exporter en CSV"
          >
            <Download size={14} /> Export CSV
          </button>
          <button
            onClick={() => void refetch()}
            className="inline-flex items-center gap-2 rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-600 transition hover:bg-slate-100"
          >
            <RefreshCw size={14} /> Rafraîchir
          </button>
        </div>
      </div>

      <div className="mb-4 flex flex-wrap items-center gap-3">
        <div className="relative">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            type="text"
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            placeholder="Filtrer par URL ou méthode..."
            className="w-80 rounded-lg border border-slate-300 py-2 pl-9 pr-3 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
          aria-label="Filtrer par statut"
          className="rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
        >
          <option value="">Tous les statuts</option>
          <option value="success">Succès (2xx)</option>
          <option value="error">Erreurs (4xx/5xx)</option>
        </select>
        <span className="text-xs text-slate-500">{filtered.length} résultat(s)</span>
      </div>

      <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
        <table className="w-full text-sm">
          <caption className="sr-only">Liste des logs d'audit</caption>
          <thead className="bg-slate-50">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">Date</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">Méthode</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">URL</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">Status</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">IP</th>
              <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">Durée</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">Exception</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {filtered.slice((page - 1) * pageSize, page * pageSize).map((l) => (
              <tr
                key={l.id}
                className={`transition-colors hover:bg-slate-50 ${(l.httpStatusCode ?? 0) >= 400 ? 'bg-red-50/50' : ''}`}
              >
                <td className="whitespace-nowrap px-4 py-2.5 text-slate-600">
                  {new Date(l.executionDate).toLocaleString('fr-FR')}
                </td>
                <td className="px-4 py-2.5"><Badge variant="info">{l.httpMethod}</Badge></td>
                <td className="max-w-[200px] truncate px-4 py-2.5 text-slate-600" title={l.url ?? ''}>
                  {l.url}
                </td>
                <td className="px-4 py-2.5">
                  <Badge variant={(l.httpStatusCode ?? 0) >= 400 ? 'danger' : (l.httpStatusCode ?? 0) >= 300 ? 'warning' : 'success'}>
                    {l.httpStatusCode}
                  </Badge>
                </td>
                <td className="px-4 py-2.5 text-slate-500">{l.ipAddress}</td>
                <td className="px-4 py-2.5 text-right tabular-nums text-slate-600">{l.executionTime} ms</td>
                <td className="max-w-[180px] truncate px-4 py-2.5 text-slate-500" title={l.exceptionMessage ?? ''}>
                  {l.exceptionMessage ?? '-'}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {filtered.length === 0 && (
          <div className="py-12 text-center text-slate-400">Aucun log d'audit trouvé.</div>
        )}
        <Pagination
          total={filtered.length}
          page={page}
          pageSize={pageSize}
          onPageChange={setPage}
        />
      </div>
    </div>
  );
}
