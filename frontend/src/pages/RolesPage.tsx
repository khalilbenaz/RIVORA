import { Shield, Plus } from 'lucide-react';
import Badge from '../components/Badge';

const defaultRoles = [
  { name: 'Admin', description: 'Accès complet au système', permissions: ['users.*', 'products.*', 'tenants.*', 'audit.read', 'settings.*'], color: 'danger' as const },
  { name: 'Manager', description: 'Gestion des utilisateurs et produits', permissions: ['users.read', 'users.write', 'products.*', 'audit.read'], color: 'warning' as const },
  { name: 'Viewer', description: 'Lecture seule', permissions: ['users.read', 'products.read', 'audit.read'], color: 'info' as const },
];

export default function RolesPage() {
  return (
    <div>
      <div className="mb-6 flex flex-wrap items-center justify-between gap-4">
        <h1 className="text-2xl font-bold text-slate-900">Rôles & Permissions</h1>
        <button className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700">
          <Plus size={16} /> Nouveau rôle
        </button>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
        {defaultRoles.map((role) => (
          <div key={role.name} className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm transition hover:shadow-md">
            <div className="mb-3 flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-slate-100">
                <Shield size={20} className="text-slate-600" />
              </div>
              <div>
                <h3 className="font-semibold text-slate-900">{role.name}</h3>
                <p className="text-xs text-slate-500">{role.description}</p>
              </div>
            </div>
            <div className="flex flex-wrap gap-1.5">
              {role.permissions.map((p) => (
                <Badge key={p} variant={role.color}>{p}</Badge>
              ))}
            </div>
          </div>
        ))}
      </div>

      <div className="mt-8 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <h2 className="mb-4 text-lg font-semibold text-slate-800">Matrice des permissions</h2>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-200">
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase text-slate-500">Permission</th>
                {defaultRoles.map((r) => (
                  <th key={r.name} className="px-4 py-3 text-center text-xs font-semibold uppercase text-slate-500">{r.name}</th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {['users.read', 'users.write', 'products.read', 'products.write', 'tenants.read', 'tenants.write', 'audit.read', 'settings.read', 'settings.write'].map((perm) => (
                <tr key={perm} className="hover:bg-slate-50">
                  <td className="px-4 py-2.5">
                    <code className="rounded bg-slate-100 px-1.5 py-0.5 text-xs">{perm}</code>
                  </td>
                  {defaultRoles.map((r) => {
                    const has = r.permissions.some((p) => {
                      if (p === perm) return true;
                      const [mod] = perm.split('.');
                      return p === `${mod}.*`;
                    });
                    return (
                      <td key={r.name} className="px-4 py-2.5 text-center">
                        {has ? (
                          <span className="text-emerald-500">&#10003;</span>
                        ) : (
                          <span className="text-slate-300">&#10005;</span>
                        )}
                      </td>
                    );
                  })}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
