import { useState } from 'react';
import { productsApi } from '../api/products';
import { useApi } from '../hooks/useApi';
import Badge from '../components/Badge';
import TableSkeleton from '../components/TableSkeleton';
import type { Product } from '../types';
import { Search, Plus, Trash2, Download } from 'lucide-react';
import { exportToCsv } from '../utils/exportCsv';
import { useToastStore } from '../store/toastStore';

export default function ProductsPage() {
  const [search, setSearch] = useState('');
  const { data: products, loading, refetch } = useApi<Product[]>(() => productsApi.getAll());
  const [deleting, setDeleting] = useState<string | null>(null);
  const addToast = useToastStore((s) => s.addToast);

  const filtered = (products ?? []).filter((p) => {
    if (!search) return true;
    const s = search.toLowerCase();
    return (
      p.name.toLowerCase().includes(s) ||
      (p.sku?.toLowerCase().includes(s) ?? false) ||
      (p.category?.toLowerCase().includes(s) ?? false)
    );
  });

  const handleDelete = async (id: string) => {
    if (!confirm('Supprimer ce produit ?')) return;
    setDeleting(id);
    try {
      await productsApi.delete(id);
      await refetch();
      addToast({ message: 'Supprimé avec succès', type: 'success' });
    } catch {
      addToast({ message: 'Erreur lors de la suppression', type: 'error' });
    } finally {
      setDeleting(null);
    }
  };

  if (loading) return <TableSkeleton columns={7} />;

  return (
    <div>
      <div className="mb-6 flex flex-wrap items-center justify-between gap-4">
        <h1 className="text-2xl font-bold text-slate-900">Produits</h1>
        <div className="flex items-center gap-2">
          <button
            onClick={() =>
              exportToCsv(filtered, 'produits', [
                { key: 'name', label: 'Produit' },
                { key: 'sku', label: 'SKU' },
                { key: 'category', label: 'Catégorie' },
                { key: 'price', label: 'Prix' },
                { key: 'stock', label: 'Stock' },
                { key: 'isActive', label: 'Actif' },
              ])
            }
            className="inline-flex items-center gap-2 rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-100"
            title="Exporter en CSV"
          >
            <Download size={16} /> Export CSV
          </button>
          <button aria-label="Ajouter un produit" className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700">
            <Plus size={16} /> Ajouter
          </button>
        </div>
      </div>

      <div className="relative mb-4">
        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Rechercher par nom, SKU, catégorie..."
          aria-label="Rechercher des produits"
          className="w-full max-w-md rounded-lg border border-slate-300 py-2 pl-9 pr-3 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
        />
        <span role="status" className="ml-3 text-xs text-slate-500">{filtered.length} résultat(s)</span>
      </div>

      <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
        <table className="w-full text-sm">
          <thead className="bg-slate-50">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">Produit</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">SKU</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">Catégorie</th>
              <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">Prix</th>
              <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">Stock</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">Statut</th>
              <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {filtered.map((p) => (
              <tr key={p.id} className="transition-colors hover:bg-slate-50">
                <td className="px-4 py-3 font-medium text-slate-900">{p.name}</td>
                <td className="px-4 py-3">
                  <code className="rounded bg-slate-100 px-1.5 py-0.5 text-xs text-slate-600">{p.sku}</code>
                </td>
                <td className="px-4 py-3 text-slate-600">{p.category ?? '-'}</td>
                <td className="px-4 py-3 text-right tabular-nums text-slate-700">
                  {p.price.toLocaleString('fr-FR', { style: 'currency', currency: 'EUR' })}
                </td>
                <td className="px-4 py-3 text-right">
                  <Badge variant={p.stock === 0 ? 'danger' : p.stock < 10 ? 'warning' : 'success'}>
                    {p.stock}
                  </Badge>
                </td>
                <td className="px-4 py-3">
                  <Badge variant={p.isActive ? 'success' : 'danger'}>
                    {p.isActive ? 'Actif' : 'Inactif'}
                  </Badge>
                </td>
                <td className="px-4 py-3 text-right">
                  <button
                    onClick={() => handleDelete(p.id)}
                    disabled={deleting === p.id}
                    className={`rounded p-1.5 text-slate-400 transition hover:bg-red-50 hover:text-red-600 ${deleting === p.id ? 'opacity-50 cursor-not-allowed' : ''}`}
                  >
                    <Trash2 size={15} />
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {filtered.length === 0 && (
          <div className="py-12 text-center text-slate-400">Aucun produit trouvé.</div>
        )}
      </div>
    </div>
  );
}
