import { Users, Package, Building2, ScrollText } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import StatCard from '../components/StatCard';
import Badge from '../components/Badge';
import Spinner from '../components/Spinner';
import { useApi } from '../hooks/useApi';
import { usersApi } from '../api/users';
import { productsApi } from '../api/products';
import { auditApi } from '../api/audit';
import { tenantsApi } from '../api/tenants';
import type { User, Product, AuditLog, Tenant } from '../types';

export default function Dashboard() {
  const { t } = useTranslation();
  const { data: users, loading: lu } = useApi<User[]>(() => usersApi.getAll());
  const { data: products, loading: lp } = useApi<Product[]>(() => productsApi.getAll());
  const { data: tenants, loading: lt } = useApi<Tenant[]>(() => tenantsApi.getAll());
  const { data: logs, loading: ll } = useApi<AuditLog[]>(() => auditApi.getAll());

  const loading = lu || lp || lt || ll;

  if (loading) return <Spinner />;

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold text-slate-900">{t('dashboard.title')}</h1>

      <div className="mb-8 grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard label={t('dashboard.users')} value={users?.length ?? 0} icon={<Users size={20} />} />
        <StatCard label={t('dashboard.products')} value={products?.length ?? 0} icon={<Package size={20} />} />
        <StatCard label={t('dashboard.tenants')} value={tenants?.length ?? 0} icon={<Building2 size={20} />} />
        <StatCard label={t('dashboard.auditLogs')} value={logs?.length ?? 0} icon={<ScrollText size={20} />} />
      </div>

      <h2 className="mb-3 text-lg font-semibold text-slate-800">{t('dashboard.recentAuditLogs')}</h2>
      <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
        <table className="w-full text-sm">
          <thead className="bg-slate-50">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">{t('dashboard.date')}</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">{t('dashboard.method')}</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">{t('dashboard.url')}</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">{t('dashboard.status')}</th>
              <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">{t('dashboard.duration')}</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {(logs ?? []).slice(0, 10).map((log) => (
              <tr key={log.id} className="transition-colors hover:bg-slate-50">
                <td className="whitespace-nowrap px-4 py-2.5 text-slate-600">
                  {new Date(log.executionDate).toLocaleString('fr-FR')}
                </td>
                <td className="px-4 py-2.5">
                  <Badge variant="info">{log.httpMethod}</Badge>
                </td>
                <td className="max-w-xs truncate px-4 py-2.5 text-slate-600" title={log.url ?? ''}>
                  {log.url}
                </td>
                <td className="px-4 py-2.5">
                  <Badge variant={(log.httpStatusCode ?? 0) >= 400 ? 'danger' : 'success'}>
                    {log.httpStatusCode}
                  </Badge>
                </td>
                <td className="px-4 py-2.5 text-right tabular-nums text-slate-600">
                  {log.executionTime} ms
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {(!logs || logs.length === 0) && (
          <div className="py-8 text-center text-slate-400">{t('dashboard.noAuditLogs')}</div>
        )}
      </div>
    </div>
  );
}
