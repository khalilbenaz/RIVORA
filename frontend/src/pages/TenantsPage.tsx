import { useState } from 'react';
import { tenantsApi } from '../api/tenants';
import { useApi } from '../hooks/useApi';
import Badge from '../components/Badge';
import Spinner from '../components/Spinner';
import type { Tenant } from '../types';
import { Search, Plus, Trash2 } from 'lucide-react';
import { useToastStore } from '../store/toastStore';

export default function TenantsPage() {
  const [search, setSearch] = useState('');
  const { data: tenants, loading, refetch } = useApi<Tenant[]>(() => tenantsApi.getAll());
  const [deleting, setDeleting] = useState<string | null>(null);
  const addToast = useToastStore((s) => s.addToast);

  const filtered = (tenants ?? []).filter((t) => {
    if (!search) return true;
    const s = search.toLowerCase();
    return t.name.toLowerCase().includes(s) || t.identifier.toLowerCase().includes(s);
  });

  const handleDelete = async (id: string) => {
    if (!confirm('Supprimer ce tenant ?')) return;
    setDeleting(id);
    try {
      await tenantsApi.delete(id);
      await refetch();
      addToast({ message: 'Supprimé avec succès', type: 'success' });
    } catch {
      addToast({ message: 'Erreur lors de la suppression', type: 'error' });
    } finally {
      setDeleting(null);
    }
  };

  if (loading) return <Spinner />;

  return (
    <div>
      <div className="mb-6 flex flex-wrap items-center justify-between gap-4">
        <h1 className="text-2xl font-bold text-slate-900">Tenants</h1>
        <button aria-label="Ajouter un tenant" className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700">
          <Plus size={16} /> Ajouter
        </button>
      </div>

      <div className="relative mb-4">
        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Rechercher par nom ou identifiant..."
          aria-label="Rechercher des tenants"
          className="w-full max-w-md rounded-lg border border-slate-300 py-2 pl-9 pr-3 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
        />
      </div>

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
        {filtered.map((t) => (
          <div key={t.id} className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm transition hover:shadow-md">
            <div className="mb-3 flex items-start justify-between">
              <div>
                <h3 className="font-semibold text-slate-900">{t.name}</h3>
                <p className="text-xs text-slate-500">{t.identifier}</p>
              </div>
              <Badge variant={t.isActive ? 'success' : 'danger'}>
                {t.isActive ? 'Actif' : 'Inactif'}
              </Badge>
            </div>
            <div className="mb-3 text-xs text-slate-500">
              Créé le {new Date(t.createdAt).toLocaleDateString('fr-FR')}
            </div>
            <div className="flex justify-end">
              <button
                onClick={() => handleDelete(t.id)}
                disabled={deleting === t.id}
                className={`rounded p-1.5 text-slate-400 transition hover:bg-red-50 hover:text-red-600 ${deleting === t.id ? 'opacity-50 cursor-not-allowed' : ''}`}
              >
                <Trash2 size={15} />
              </button>
            </div>
          </div>
        ))}
      </div>

      {filtered.length === 0 && (
        <div className="mt-8 rounded-xl border border-dashed border-slate-300 py-12 text-center text-slate-400">
          Aucun tenant trouvé.
        </div>
      )}
    </div>
  );
}
