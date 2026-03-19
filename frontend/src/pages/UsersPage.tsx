import { useState } from 'react';
import { usersApi } from '../api/users';
import { useApi } from '../hooks/useApi';
import Badge from '../components/Badge';
import TableSkeleton from '../components/TableSkeleton';
import type { User } from '../types';
import { Search, UserPlus, Trash2, Download } from 'lucide-react';
import { exportToCsv } from '../utils/exportCsv';
import { useToastStore } from '../store/toastStore';

export default function UsersPage() {
  const [search, setSearch] = useState('');
  const { data: users, loading, refetch } = useApi<User[]>(() => usersApi.getAll());
  const [deleting, setDeleting] = useState<string | null>(null);
  const addToast = useToastStore((s) => s.addToast);

  const filtered = (users ?? []).filter((u) => {
    if (!search) return true;
    const s = search.toLowerCase();
    return (
      u.userName.toLowerCase().includes(s) ||
      u.email.toLowerCase().includes(s) ||
      (u.firstName?.toLowerCase().includes(s) ?? false) ||
      (u.lastName?.toLowerCase().includes(s) ?? false)
    );
  });

  const handleDelete = async (id: string) => {
    if (!confirm('Supprimer cet utilisateur ?')) return;
    setDeleting(id);
    try {
      await usersApi.delete(id);
      await refetch();
      addToast({ message: 'Supprimé avec succès', type: 'success' });
    } catch {
      addToast({ message: 'Erreur lors de la suppression', type: 'error' });
    } finally {
      setDeleting(null);
    }
  };

  if (loading) return <TableSkeleton columns={6} />;

  return (
    <div>
      <div className="mb-6 flex flex-wrap items-center justify-between gap-4">
        <h1 className="text-2xl font-bold text-slate-900">Utilisateurs</h1>
        <div className="flex items-center gap-2">
          <button
            onClick={() =>
              exportToCsv(filtered, 'utilisateurs', [
                { key: 'userName', label: 'Utilisateur' },
                { key: 'email', label: 'Email' },
                { key: 'firstName', label: 'Prénom' },
                { key: 'lastName', label: 'Nom' },
                { key: 'isActive', label: 'Actif' },
                { key: 'createdAt', label: 'Créé le' },
              ])
            }
            className="inline-flex items-center gap-2 rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-100"
            title="Exporter en CSV"
          >
            <Download size={16} /> Export CSV
          </button>
          <a
            href="/users/new"
            aria-label="Ajouter un utilisateur"
            className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700"
          >
            <UserPlus size={16} /> Ajouter
          </a>
        </div>
      </div>

      <div className="relative mb-4">
        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Rechercher par nom, email..."
          aria-label="Rechercher des utilisateurs"
          className="w-full max-w-md rounded-lg border border-slate-300 py-2 pl-9 pr-3 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
        />
        <span className="ml-3 text-xs text-slate-500">{filtered.length} résultat(s)</span>
      </div>

      <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm dark:bg-slate-800 dark:border-slate-700">
        <table className="w-full text-sm">
          <thead className="bg-slate-50">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">Utilisateur</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">Email</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">Statut</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">2FA</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">Créé le</th>
              <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {filtered.map((u) => (
              <tr key={u.id} className="transition-colors hover:bg-slate-50">
                <td className="px-4 py-3">
                  <div className="font-medium text-slate-900">{u.userName}</div>
                  {(u.firstName || u.lastName) && (
                    <div className="text-xs text-slate-500">{u.firstName} {u.lastName}</div>
                  )}
                </td>
                <td className="px-4 py-3 text-slate-600">{u.email}</td>
                <td className="px-4 py-3">
                  <Badge variant={u.isActive ? 'success' : 'danger'}>
                    {u.isActive ? 'Actif' : 'Inactif'}
                  </Badge>
                </td>
                <td className="px-4 py-3">
                  {u.twoFactorEnabled ? <Badge variant="info">Activé</Badge> : <span className="text-slate-400">-</span>}
                </td>
                <td className="whitespace-nowrap px-4 py-3 text-slate-500">
                  {new Date(u.createdAt).toLocaleDateString('fr-FR')}
                </td>
                <td className="px-4 py-3 text-right">
                  <button
                    onClick={() => handleDelete(u.id)}
                    disabled={deleting === u.id}
                    className={`rounded p-1.5 text-slate-400 transition hover:bg-red-50 hover:text-red-600 ${deleting === u.id ? 'opacity-50 cursor-not-allowed' : ''}`}
                    title="Supprimer"
                  >
                    <Trash2 size={15} />
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {filtered.length === 0 && (
          <div className="py-12 text-center text-slate-400">Aucun utilisateur trouvé.</div>
        )}
      </div>
    </div>
  );
}
