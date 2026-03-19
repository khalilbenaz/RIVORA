import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Plus,
  Download,
  Trash2,
  MoreVertical,
  Search,
  Clock,
  CheckCircle2,
  AlertCircle,
  Loader2,
} from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { projectTemplates, categoryLabels } from './templates';

// ---------------------------------------------------------------------------
// Mock data
// ---------------------------------------------------------------------------

interface Project {
  id: string;
  name: string;
  templateId: string | null;
  database: string;
  modulesCount: number;
  entitiesCount: number;
  status: 'ready' | 'generating' | 'error';
  createdAt: string;
}

const mockProjects: Project[] = [
  {
    id: 'proj-1',
    name: 'Acme SaaS Platform',
    templateId: 'saas-starter',
    database: 'postgresql',
    modulesCount: 12,
    entitiesCount: 3,
    status: 'ready',
    createdAt: new Date(Date.now() - 86400000 * 5).toISOString(),
  },
  {
    id: 'proj-2',
    name: 'ShopFront Store',
    templateId: 'ecommerce',
    database: 'postgresql',
    modulesCount: 11,
    entitiesCount: 4,
    status: 'generating',
    createdAt: new Date(Date.now() - 3600000).toISOString(),
  },
  {
    id: 'proj-3',
    name: 'Internal Dashboard',
    templateId: 'internal-tools',
    database: 'sqlserver',
    modulesCount: 9,
    entitiesCount: 3,
    status: 'error',
    createdAt: new Date(Date.now() - 86400000 * 2).toISOString(),
  },
];

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function getTemplateInfo(templateId: string | null) {
  if (!templateId) return { icon: '\u2795', name: 'Blank', category: null, color: 'bg-slate-500' };
  const tpl = projectTemplates.find((t) => t.id === templateId);
  return tpl
    ? { icon: tpl.icon, name: tpl.name, category: tpl.category, color: tpl.color }
    : { icon: '?', name: 'Unknown', category: null, color: 'bg-slate-500' };
}

const statusConfig = {
  ready: { icon: CheckCircle2, label: 'projects.statusReady', cls: 'text-emerald-400 bg-emerald-400/10' },
  generating: { icon: Loader2, label: 'projects.statusGenerating', cls: 'text-amber-400 bg-amber-400/10', spin: true },
  error: { icon: AlertCircle, label: 'projects.statusError', cls: 'text-red-400 bg-red-400/10' },
};

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

export default function ProjectListPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [projects, setProjects] = useState(mockProjects);
  const [search, setSearch] = useState('');
  const [menuOpen, setMenuOpen] = useState<string | null>(null);

  const filtered = projects.filter((p) =>
    p.name.toLowerCase().includes(search.toLowerCase()),
  );

  const deleteProject = (id: string) => {
    if (!confirm(t('projects.confirmDelete'))) return;
    setProjects((prev) => prev.filter((p) => p.id !== id));
    setMenuOpen(null);
  };

  return (
    <div className="min-h-screen p-6">
      {/* Header */}
      <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-white">{t('projects.title')}</h1>
          <p className="mt-1 text-sm text-slate-400">{t('projects.subtitle')}</p>
        </div>
        <button
          onClick={() => navigate('/admin/projects/new')}
          className="flex items-center gap-2 rounded-xl bg-gradient-to-r from-blue-600 to-violet-600 px-5 py-2.5 text-sm font-bold text-white shadow-lg shadow-blue-500/25 transition-all hover:shadow-blue-500/40"
        >
          <Plus size={18} />
          {t('projects.newProject')}
        </button>
      </div>

      {/* Search */}
      <div className="relative mb-6 max-w-md">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" size={16} />
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder={t('projects.searchPlaceholder')}
          className="w-full rounded-lg border border-slate-700 bg-slate-800 py-2.5 pl-9 pr-4 text-sm text-white placeholder-slate-500 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
        />
      </div>

      {/* Grid */}
      {filtered.length === 0 ? (
        <div className="rounded-xl border border-dashed border-slate-600 py-20 text-center">
          <p className="text-slate-400">{t('projects.noProjects')}</p>
          <button
            onClick={() => navigate('/admin/projects/new')}
            className="mt-3 text-sm font-medium text-blue-400 hover:text-blue-300"
          >
            {t('projects.createFirst')}
          </button>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {filtered.map((project) => {
            const tplInfo = getTemplateInfo(project.templateId);
            const status = statusConfig[project.status];
            const StatusIcon = status.icon;
            const isMenuOpen = menuOpen === project.id;

            return (
              <div
                key={project.id}
                className="group relative rounded-xl border border-slate-700 bg-slate-800/50 p-5 transition-all hover:border-slate-600"
              >
                {/* Top row */}
                <div className="mb-3 flex items-start justify-between">
                  <div className="flex items-center gap-3">
                    <span className="text-2xl">{tplInfo.icon}</span>
                    <div>
                      <h3 className="font-semibold text-white">{project.name}</h3>
                      <div className="flex items-center gap-2">
                        {tplInfo.category && (
                          <span
                            className={`rounded-full px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider text-white ${tplInfo.color}`}
                          >
                            {categoryLabels[tplInfo.category as keyof typeof categoryLabels]}
                          </span>
                        )}
                        <span className="text-xs text-slate-500">{tplInfo.name}</span>
                      </div>
                    </div>
                  </div>

                  {/* Menu */}
                  <div className="relative">
                    <button
                      onClick={() => setMenuOpen(isMenuOpen ? null : project.id)}
                      className="rounded p-1 text-slate-500 transition-colors hover:bg-slate-700 hover:text-white"
                    >
                      <MoreVertical size={16} />
                    </button>
                    {isMenuOpen && (
                      <>
                        <div className="fixed inset-0 z-10" onClick={() => setMenuOpen(null)} />
                        <div className="absolute right-0 z-20 mt-1 w-40 rounded-lg border border-slate-700 bg-slate-800 py-1 shadow-xl">
                          {project.status === 'ready' && (
                            <button className="flex w-full items-center gap-2 px-3 py-2 text-sm text-slate-300 hover:bg-slate-700">
                              <Download size={14} />
                              {t('projects.download')}
                            </button>
                          )}
                          <button
                            onClick={() => deleteProject(project.id)}
                            className="flex w-full items-center gap-2 px-3 py-2 text-sm text-red-400 hover:bg-slate-700"
                          >
                            <Trash2 size={14} />
                            {t('common.delete')}
                          </button>
                        </div>
                      </>
                    )}
                  </div>
                </div>

                {/* Info row */}
                <div className="mb-3 flex items-center gap-4 text-xs text-slate-500">
                  <span>{project.database}</span>
                  <span>{project.modulesCount} modules</span>
                  <span>{project.entitiesCount} entities</span>
                </div>

                {/* Footer */}
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-1.5 text-xs text-slate-500">
                    <Clock size={12} />
                    {new Date(project.createdAt).toLocaleDateString()}
                  </div>
                  <span
                    className={`flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium ${status.cls}`}
                  >
                    <StatusIcon size={12} className={(status as { spin?: boolean }).spin ? 'animate-spin' : ''} />
                    {t(status.label)}
                  </span>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
