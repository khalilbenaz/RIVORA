import { useState, useCallback, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  ArrowRight,
  Check,
  ChevronRight,
  Copy,
  Download,
  FolderTree,
  Layers,
  Minus,
  Plus,
  Sparkles,
  X,
} from 'lucide-react';
import { useTranslation } from 'react-i18next';
import {
  projectTemplates,
  categoryLabels,
  allModules,
  databaseOptions,
  fieldTypes,
  type ProjectTemplate,
  type TemplateEntity,
} from './templates';

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

interface WizardState {
  selectedTemplate: ProjectTemplate | null;
  isBlank: boolean;
  projectName: string;
  namespace: string;
  description: string;
  database: string;
  selectedModules: string[];
  entities: TemplateEntity[];
  enabledFlows: string[];
}

const STEPS = [
  'Template',
  'Configuration',
  'Modules',
  'Entities',
  'Flows',
  'Summary',
] as const;

function slugify(text: string): string {
  return text
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-|-$)/g, '');
}

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

export default function ProjectWizard() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [step, setStep] = useState(0);
  const [animDir, setAnimDir] = useState<'next' | 'prev'>('next');

  const [state, setState] = useState<WizardState>({
    selectedTemplate: null,
    isBlank: false,
    projectName: '',
    namespace: '',
    description: '',
    database: 'postgresql',
    selectedModules: [],
    entities: [],
    enabledFlows: [],
  });

  // ---- helpers ----
  const patch = useCallback(
    (partial: Partial<WizardState>) => setState((s) => ({ ...s, ...partial })),
    [],
  );

  const canProceed = useMemo(() => {
    switch (step) {
      case 0:
        return state.selectedTemplate !== null || state.isBlank;
      case 1:
        return state.projectName.trim().length > 0 && state.namespace.trim().length > 0;
      default:
        return true;
    }
  }, [step, state]);

  const goNext = () => {
    if (!canProceed) return;
    setAnimDir('next');
    setStep((s) => Math.min(s + 1, STEPS.length - 1));
  };
  const goBack = () => {
    setAnimDir('prev');
    setStep((s) => Math.max(s - 1, 0));
  };

  // ---- template selection ----
  const selectTemplate = (tpl: ProjectTemplate) => {
    patch({
      selectedTemplate: tpl,
      isBlank: false,
      database: tpl.database,
      selectedModules: [...tpl.modules],
      entities: tpl.entities.map((e) => ({
        name: e.name,
        fields: e.fields.map((f) => ({ ...f })),
      })),
      enabledFlows: [...tpl.flows],
    });
  };

  const selectBlank = () => {
    patch({
      selectedTemplate: null,
      isBlank: true,
      database: 'postgresql',
      selectedModules: ['jwt', 'health-checks'],
      entities: [],
      enabledFlows: [],
    });
  };

  // ---- module toggle ----
  const toggleModule = (id: string) => {
    setState((s) => ({
      ...s,
      selectedModules: s.selectedModules.includes(id)
        ? s.selectedModules.filter((m) => m !== id)
        : [...s.selectedModules, id],
    }));
  };

  // ---- entity helpers ----
  const addEntity = () => {
    setState((s) => ({
      ...s,
      entities: [
        ...s.entities,
        {
          name: 'NewEntity',
          fields: [{ name: 'Id', type: 'Guid', required: true }],
        },
      ],
    }));
  };

  const removeEntity = (idx: number) => {
    setState((s) => ({
      ...s,
      entities: s.entities.filter((_, i) => i !== idx),
    }));
  };

  const updateEntityName = (idx: number, name: string) => {
    setState((s) => {
      const entities = s.entities.map((e, i) =>
        i === idx ? { name, fields: [...e.fields] } : e,
      );
      return { ...s, entities };
    });
  };

  const addField = (entityIdx: number) => {
    setState((s) => {
      const entities = s.entities.map((e, i) =>
        i === entityIdx
          ? { name: e.name, fields: [...e.fields, { name: '', type: 'string', required: false }] }
          : e,
      );
      return { ...s, entities };
    });
  };

  const removeField = (entityIdx: number, fieldIdx: number) => {
    setState((s) => {
      const entities = s.entities.map((e, i) =>
        i === entityIdx
          ? { name: e.name, fields: e.fields.filter((_, fi) => fi !== fieldIdx) }
          : e,
      );
      return { ...s, entities };
    });
  };

  const updateField = (
    entityIdx: number,
    fieldIdx: number,
    key: 'name' | 'type' | 'required',
    value: string | boolean,
  ) => {
    setState((s) => {
      const entities = s.entities.map((e, i) =>
        i === entityIdx
          ? {
              name: e.name,
              fields: e.fields.map((f, fi) =>
                fi === fieldIdx ? { ...f, [key]: value } : f,
              ),
            }
          : e,
      );
      return { ...s, entities };
    });
  };

  // ---- flow toggle ----
  const toggleFlow = (flow: string) => {
    setState((s) => ({
      ...s,
      enabledFlows: s.enabledFlows.includes(flow)
        ? s.enabledFlows.filter((f) => f !== flow)
        : [...s.enabledFlows, flow],
    }));
  };

  // ---- generate helpers ----
  const buildCliCommand = () => {
    const parts = [`rvr new "${state.projectName}"`];
    parts.push(`--db ${state.database}`);
    if (state.selectedModules.length) {
      parts.push(`--modules ${state.selectedModules.join(',')}`);
    }
    return parts.join(' \\\n  ');
  };

  const buildProjectTree = (): string[] => {
    const ns = state.namespace || 'MyProject';
    const lines = [
      `${ns}/`,
      `  ${ns}.Api/`,
      `    Controllers/`,
      ...state.entities.map((e) => `      ${e.name}Controller.cs`),
      `    Program.cs`,
      `    appsettings.json`,
      `  ${ns}.Application/`,
      `    Services/`,
      ...state.entities.map((e) => `      ${e.name}Service.cs`),
      `    DTOs/`,
      ...state.entities.map((e) => `      ${e.name}Dto.cs`),
      `  ${ns}.Domain/`,
      `    Entities/`,
      ...state.entities.map((e) => `      ${e.name}.cs`),
      `  ${ns}.Infrastructure/`,
      `    Persistence/`,
      `      AppDbContext.cs`,
      ...state.entities.map((e) => `      ${e.name}Configuration.cs`),
      `    Migrations/`,
    ];
    if (state.selectedModules.includes('caching-redis')) lines.push(`    Caching/`);
    if (state.selectedModules.includes('email')) lines.push(`    Email/`);
    if (state.selectedModules.includes('webhooks')) lines.push(`    Webhooks/`);
    lines.push(`  ${ns}.sln`);
    return lines;
  };

  const [copied, setCopied] = useState(false);
  const copyCliCommand = () => {
    navigator.clipboard.writeText(buildCliCommand());
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const [generating, setGenerating] = useState(false);
  const handleGenerate = async () => {
    setGenerating(true);
    // Simulate API call
    await new Promise((r) => setTimeout(r, 1800));
    setGenerating(false);
    navigate('/admin/projects');
  };

  // ---------------------------------------------------------------------------
  // Render helpers
  // ---------------------------------------------------------------------------

  const renderProgressBar = () => (
    <div className="mb-8">
      <div className="flex items-center justify-between">
        {STEPS.map((label, i) => {
          const done = i < step;
          const active = i === step;
          return (
            <div key={label} className="flex flex-1 items-center">
              <div className="flex flex-col items-center">
                <div
                  className={`flex h-9 w-9 items-center justify-center rounded-full text-sm font-semibold transition-colors ${
                    done
                      ? 'bg-blue-500 text-white'
                      : active
                        ? 'bg-blue-600 text-white ring-4 ring-blue-600/30'
                        : 'bg-slate-700 text-slate-400'
                  }`}
                >
                  {done ? <Check size={16} /> : i + 1}
                </div>
                <span
                  className={`mt-1.5 text-xs font-medium ${
                    active ? 'text-blue-400' : done ? 'text-slate-300' : 'text-slate-500'
                  }`}
                >
                  {label}
                </span>
              </div>
              {i < STEPS.length - 1 && (
                <div
                  className={`mx-2 h-0.5 flex-1 rounded ${
                    i < step ? 'bg-blue-500' : 'bg-slate-700'
                  }`}
                />
              )}
            </div>
          );
        })}
      </div>
    </div>
  );

  // Step 1 -------------------------------------------------------------------
  const renderStep1 = () => (
    <div>
      <h2 className="mb-1 text-xl font-bold text-white">
        {t('projects.wizard.chooseTemplate')}
      </h2>
      <p className="mb-6 text-sm text-slate-400">
        {t('projects.wizard.chooseTemplateDesc')}
      </p>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {/* Blank */}
        <button
          onClick={selectBlank}
          className={`group relative rounded-xl border-2 p-5 text-left transition-all ${
            state.isBlank
              ? 'border-blue-500 bg-blue-500/10'
              : 'border-slate-700 bg-slate-800/50 hover:border-slate-600'
          }`}
        >
          <div className="mb-3 text-3xl">&#x2795;</div>
          <h3 className="font-semibold text-white">{t('projects.wizard.blankProject')}</h3>
          <p className="mt-1 text-xs text-slate-400">{t('projects.wizard.blankProjectDesc')}</p>
          {state.isBlank && (
            <div className="absolute right-3 top-3 rounded-full bg-blue-500 p-1">
              <Check size={14} className="text-white" />
            </div>
          )}
        </button>

        {/* Templates */}
        {projectTemplates.map((tpl) => {
          const selected = state.selectedTemplate?.id === tpl.id;
          return (
            <button
              key={tpl.id}
              onClick={() => selectTemplate(tpl)}
              className={`group relative rounded-xl border-2 p-5 text-left transition-all ${
                selected
                  ? 'border-blue-500 bg-blue-500/10'
                  : 'border-slate-700 bg-slate-800/50 hover:border-slate-600'
              }`}
            >
              <div className="mb-3 flex items-center gap-3">
                <span className="text-3xl">{tpl.icon}</span>
                <span
                  className={`rounded-full px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider text-white ${tpl.color}`}
                >
                  {categoryLabels[tpl.category]}
                </span>
              </div>
              <h3 className="font-semibold text-white">{tpl.name}</h3>
              <p className="mt-1 text-xs text-slate-400 line-clamp-2">{tpl.description}</p>
              <div className="mt-3 flex flex-wrap gap-1">
                {tpl.features.slice(0, 3).map((f) => (
                  <span key={f} className="rounded bg-slate-700/60 px-1.5 py-0.5 text-[10px] text-slate-300">
                    {f}
                  </span>
                ))}
              </div>
              <div className="mt-2 text-[10px] text-slate-500">
                {tpl.entities.length} entities &middot; {tpl.modules.length} modules &middot; {tpl.estimatedSetup}
              </div>
              {selected && (
                <div className="absolute right-3 top-3 rounded-full bg-blue-500 p-1">
                  <Check size={14} className="text-white" />
                </div>
              )}
            </button>
          );
        })}
      </div>
    </div>
  );

  // Step 2 -------------------------------------------------------------------
  const renderStep2 = () => (
    <div className="mx-auto max-w-2xl">
      <h2 className="mb-1 text-xl font-bold text-white">{t('projects.wizard.projectConfig')}</h2>
      <p className="mb-6 text-sm text-slate-400">{t('projects.wizard.projectConfigDesc')}</p>

      <div className="space-y-5">
        {/* Name */}
        <div>
          <label className="mb-1 block text-sm font-medium text-slate-300">
            {t('projects.wizard.projectName')} <span className="text-red-400">*</span>
          </label>
          <input
            type="text"
            value={state.projectName}
            onChange={(e) => {
              const name = e.target.value;
              patch({ projectName: name, namespace: slugify(name) });
            }}
            placeholder="My Awesome App"
            className="w-full rounded-lg border border-slate-600 bg-slate-800 px-4 py-2.5 text-white placeholder-slate-500 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>

        {/* Namespace */}
        <div>
          <label className="mb-1 block text-sm font-medium text-slate-300">
            {t('projects.wizard.namespace')} <span className="text-red-400">*</span>
          </label>
          <input
            type="text"
            value={state.namespace}
            onChange={(e) => patch({ namespace: e.target.value })}
            placeholder="my-awesome-app"
            className="w-full rounded-lg border border-slate-600 bg-slate-800 px-4 py-2.5 text-white placeholder-slate-500 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
          <p className="mt-1 text-xs text-slate-500">
            {t('projects.wizard.namespaceHint')}
          </p>
        </div>

        {/* Description */}
        <div>
          <label className="mb-1 block text-sm font-medium text-slate-300">
            {t('projects.wizard.description')}
          </label>
          <textarea
            value={state.description}
            onChange={(e) => patch({ description: e.target.value })}
            rows={3}
            placeholder={t('projects.wizard.descriptionPlaceholder')}
            className="w-full resize-none rounded-lg border border-slate-600 bg-slate-800 px-4 py-2.5 text-white placeholder-slate-500 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>

        {/* Database */}
        <div>
          <label className="mb-2 block text-sm font-medium text-slate-300">
            {t('projects.wizard.database')}
          </label>
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
            {databaseOptions.map((db) => {
              const active = state.database === db.id;
              return (
                <button
                  key={db.id}
                  onClick={() => patch({ database: db.id })}
                  className={`rounded-lg border-2 p-3 text-center transition-all ${
                    active
                      ? 'border-blue-500 bg-blue-500/10'
                      : 'border-slate-700 bg-slate-800/50 hover:border-slate-600'
                  }`}
                >
                  <div className="mb-1 text-2xl">{db.icon}</div>
                  <div className="text-sm font-medium text-white">{db.name}</div>
                  <div className="mt-0.5 text-[10px] text-slate-500">{db.description}</div>
                </button>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );

  // Step 3 -------------------------------------------------------------------
  const renderStep3 = () => {
    const moduleCount = state.selectedModules.length;
    return (
      <div>
        <div className="mb-6 flex items-center justify-between">
          <div>
            <h2 className="mb-1 text-xl font-bold text-white">{t('projects.wizard.modules')}</h2>
            <p className="text-sm text-slate-400">{t('projects.wizard.modulesDesc')}</p>
          </div>
          <span className="rounded-full bg-blue-500/20 px-3 py-1 text-sm font-semibold text-blue-400">
            {moduleCount} {t('projects.wizard.selected')}
          </span>
        </div>

        <div className="grid gap-6 md:grid-cols-2 xl:grid-cols-3">
          {Object.entries(allModules).map(([key, group]) => (
            <div key={key} className="rounded-xl border border-slate-700 bg-slate-800/50 p-4">
              <h3 className="mb-3 text-sm font-bold uppercase tracking-wider text-slate-400">
                {group.label}
              </h3>
              <div className="space-y-2">
                {group.modules.map((mod) => {
                  const checked = state.selectedModules.includes(mod.id);
                  return (
                    <label
                      key={mod.id}
                      className={`flex cursor-pointer items-center gap-3 rounded-lg px-3 py-2 transition-colors ${
                        checked ? 'bg-blue-500/10' : 'hover:bg-slate-700/50'
                      }`}
                    >
                      <input
                        type="checkbox"
                        checked={checked}
                        onChange={() => toggleModule(mod.id)}
                        className="h-4 w-4 rounded border-slate-500 bg-slate-700 text-blue-500 focus:ring-blue-500 focus:ring-offset-0"
                      />
                      <div className="flex-1">
                        <div className="text-sm font-medium text-white">{mod.name}</div>
                        <div className="text-xs text-slate-500">{mod.description}</div>
                      </div>
                    </label>
                  );
                })}
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  };

  // Step 4 -------------------------------------------------------------------
  const renderStep4 = () => (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h2 className="mb-1 text-xl font-bold text-white">{t('projects.wizard.entities')}</h2>
          <p className="text-sm text-slate-400">{t('projects.wizard.entitiesDesc')}</p>
        </div>
        <button
          onClick={addEntity}
          className="flex items-center gap-1.5 rounded-lg bg-blue-600 px-3 py-2 text-sm font-medium text-white transition-colors hover:bg-blue-700"
        >
          <Plus size={16} />
          {t('projects.wizard.addEntity')}
        </button>
      </div>

      {state.entities.length === 0 && (
        <div className="rounded-xl border border-dashed border-slate-600 py-16 text-center">
          <Layers className="mx-auto mb-3 text-slate-600" size={40} />
          <p className="text-slate-400">{t('projects.wizard.noEntities')}</p>
          <button
            onClick={addEntity}
            className="mt-3 text-sm font-medium text-blue-400 hover:text-blue-300"
          >
            {t('projects.wizard.addFirstEntity')}
          </button>
        </div>
      )}

      <div className="space-y-4">
        {state.entities.map((entity, eIdx) => (
          <div key={eIdx} className="rounded-xl border border-slate-700 bg-slate-800/50 p-4">
            <div className="mb-3 flex items-center gap-3">
              <input
                type="text"
                value={entity.name}
                onChange={(e) => updateEntityName(eIdx, e.target.value)}
                className="rounded-lg border border-slate-600 bg-slate-700 px-3 py-1.5 text-sm font-semibold text-white focus:border-blue-500 focus:outline-none"
              />
              <button
                onClick={() => removeEntity(eIdx)}
                className="ml-auto rounded p-1 text-slate-500 transition-colors hover:bg-red-500/20 hover:text-red-400"
              >
                <X size={16} />
              </button>
            </div>

            <table className="w-full text-sm">
              <thead>
                <tr className="text-left text-xs uppercase text-slate-500">
                  <th className="pb-2 pl-2">{t('projects.wizard.fieldName')}</th>
                  <th className="pb-2">{t('projects.wizard.fieldType')}</th>
                  <th className="pb-2 text-center">{t('projects.wizard.required')}</th>
                  <th className="pb-2 w-10"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-700/50">
                {entity.fields.map((field, fIdx) => (
                  <tr key={fIdx}>
                    <td className="py-1.5 pl-2 pr-2">
                      <input
                        type="text"
                        value={field.name}
                        onChange={(e) => updateField(eIdx, fIdx, 'name', e.target.value)}
                        className="w-full rounded border border-slate-600 bg-slate-700 px-2 py-1 text-white focus:border-blue-500 focus:outline-none"
                      />
                    </td>
                    <td className="py-1.5 pr-2">
                      <select
                        value={field.type}
                        onChange={(e) => updateField(eIdx, fIdx, 'type', e.target.value)}
                        className="w-full rounded border border-slate-600 bg-slate-700 px-2 py-1 text-white focus:border-blue-500 focus:outline-none"
                      >
                        {fieldTypes.map((ft) => (
                          <option key={ft} value={ft}>
                            {ft}
                          </option>
                        ))}
                      </select>
                    </td>
                    <td className="py-1.5 text-center">
                      <input
                        type="checkbox"
                        checked={field.required}
                        onChange={(e) => updateField(eIdx, fIdx, 'required', e.target.checked)}
                        className="h-4 w-4 rounded border-slate-500 bg-slate-700 text-blue-500 focus:ring-blue-500 focus:ring-offset-0"
                      />
                    </td>
                    <td className="py-1.5 text-center">
                      <button
                        onClick={() => removeField(eIdx, fIdx)}
                        className="rounded p-0.5 text-slate-500 hover:text-red-400"
                      >
                        <Minus size={14} />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            <button
              onClick={() => addField(eIdx)}
              className="mt-2 flex items-center gap-1 text-xs font-medium text-blue-400 hover:text-blue-300"
            >
              <Plus size={12} />
              {t('projects.wizard.addField')}
            </button>
          </div>
        ))}
      </div>
    </div>
  );

  // Step 5 -------------------------------------------------------------------
  const renderStep5 = () => {
    const availableFlows = state.selectedTemplate?.flows ?? [];
    return (
      <div className="mx-auto max-w-2xl">
        <h2 className="mb-1 text-xl font-bold text-white">{t('projects.wizard.flows')}</h2>
        <p className="mb-6 text-sm text-slate-400">{t('projects.wizard.flowsDesc')}</p>

        {availableFlows.length === 0 ? (
          <div className="rounded-xl border border-dashed border-slate-600 py-16 text-center">
            <p className="text-slate-400">{t('projects.wizard.noFlows')}</p>
          </div>
        ) : (
          <div className="space-y-3">
            {availableFlows.map((flow, i) => {
              const enabled = state.enabledFlows.includes(flow);
              return (
                <label
                  key={i}
                  className={`flex cursor-pointer items-start gap-3 rounded-xl border p-4 transition-all ${
                    enabled
                      ? 'border-blue-500 bg-blue-500/10'
                      : 'border-slate-700 bg-slate-800/50 hover:border-slate-600'
                  }`}
                >
                  <input
                    type="checkbox"
                    checked={enabled}
                    onChange={() => toggleFlow(flow)}
                    className="mt-0.5 h-4 w-4 rounded border-slate-500 bg-slate-700 text-blue-500 focus:ring-blue-500 focus:ring-offset-0"
                  />
                  <div className="flex-1">
                    <div className="flex items-center gap-2 text-sm text-white">
                      {flow.split(' -> ').map((segment, j, arr) => (
                        <span key={j} className="flex items-center gap-1">
                          <span className="rounded bg-slate-700 px-2 py-0.5 text-xs font-medium">
                            {segment}
                          </span>
                          {j < arr.length - 1 && (
                            <ChevronRight size={12} className="text-slate-500" />
                          )}
                        </span>
                      ))}
                    </div>
                  </div>
                </label>
              );
            })}
          </div>
        )}
      </div>
    );
  };

  // Step 6 -------------------------------------------------------------------
  const renderStep6 = () => {
    const tree = buildProjectTree();
    return (
      <div className="mx-auto max-w-3xl">
        <h2 className="mb-1 text-xl font-bold text-white">{t('projects.wizard.summary')}</h2>
        <p className="mb-6 text-sm text-slate-400">{t('projects.wizard.summaryDesc')}</p>

        <div className="grid gap-6 md:grid-cols-2">
          {/* Left column - recap */}
          <div className="space-y-4">
            {/* Project info */}
            <div className="rounded-xl border border-slate-700 bg-slate-800/50 p-4">
              <h3 className="mb-3 flex items-center gap-2 text-sm font-bold uppercase tracking-wider text-slate-400">
                {t('projects.wizard.projectInfo')}
              </h3>
              <dl className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <dt className="text-slate-400">{t('projects.wizard.projectName')}</dt>
                  <dd className="font-medium text-white">{state.projectName || '-'}</dd>
                </div>
                <div className="flex justify-between">
                  <dt className="text-slate-400">{t('projects.wizard.namespace')}</dt>
                  <dd className="font-mono text-xs text-blue-400">{state.namespace || '-'}</dd>
                </div>
                <div className="flex justify-between">
                  <dt className="text-slate-400">{t('projects.wizard.template')}</dt>
                  <dd className="font-medium text-white">
                    {state.selectedTemplate
                      ? `${state.selectedTemplate.icon} ${state.selectedTemplate.name}`
                      : t('projects.wizard.blankProject')}
                  </dd>
                </div>
                <div className="flex justify-between">
                  <dt className="text-slate-400">{t('projects.wizard.database')}</dt>
                  <dd className="font-medium text-white">
                    {databaseOptions.find((d) => d.id === state.database)?.name}
                  </dd>
                </div>
              </dl>
            </div>

            {/* Modules */}
            <div className="rounded-xl border border-slate-700 bg-slate-800/50 p-4">
              <h3 className="mb-3 flex items-center gap-2 text-sm font-bold uppercase tracking-wider text-slate-400">
                {t('projects.wizard.modules')}
                <span className="rounded-full bg-blue-500/20 px-2 py-0.5 text-xs text-blue-400">
                  {state.selectedModules.length}
                </span>
              </h3>
              <div className="flex flex-wrap gap-1.5">
                {state.selectedModules.map((id) => (
                  <span key={id} className="rounded-full bg-slate-700 px-2.5 py-0.5 text-xs text-slate-300">
                    {id}
                  </span>
                ))}
              </div>
            </div>

            {/* Entities */}
            <div className="rounded-xl border border-slate-700 bg-slate-800/50 p-4">
              <h3 className="mb-3 text-sm font-bold uppercase tracking-wider text-slate-400">
                {t('projects.wizard.entities')} ({state.entities.length})
              </h3>
              <div className="space-y-1">
                {state.entities.map((e, i) => (
                  <div key={i} className="flex items-center justify-between text-sm">
                    <span className="font-medium text-white">{e.name}</span>
                    <span className="text-xs text-slate-500">{e.fields.length} fields</span>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* Right column - tree + actions */}
          <div className="space-y-4">
            {/* Project tree */}
            <div className="rounded-xl border border-slate-700 bg-slate-800/50 p-4">
              <h3 className="mb-3 flex items-center gap-2 text-sm font-bold uppercase tracking-wider text-slate-400">
                <FolderTree size={14} />
                {t('projects.wizard.projectStructure')}
              </h3>
              <pre className="max-h-72 overflow-y-auto rounded-lg bg-slate-900 p-3 text-xs leading-relaxed text-slate-300">
                {tree.join('\n')}
              </pre>
            </div>

            {/* CLI command */}
            <div className="rounded-xl border border-slate-700 bg-slate-800/50 p-4">
              <h3 className="mb-3 text-sm font-bold uppercase tracking-wider text-slate-400">
                {t('projects.wizard.cliCommand')}
              </h3>
              <pre className="rounded-lg bg-slate-900 p-3 text-xs text-green-400">
                {buildCliCommand()}
              </pre>
              <button
                onClick={copyCliCommand}
                className="mt-2 flex items-center gap-1.5 text-xs text-slate-400 hover:text-white"
              >
                <Copy size={12} />
                {copied ? t('projects.wizard.copied') : t('projects.wizard.copyCommand')}
              </button>
            </div>
          </div>
        </div>

        {/* Action buttons */}
        <div className="mt-8 flex flex-wrap items-center justify-center gap-3">
          <button
            onClick={handleGenerate}
            disabled={generating}
            className="flex items-center gap-2 rounded-xl bg-gradient-to-r from-blue-600 to-violet-600 px-8 py-3 text-sm font-bold text-white shadow-lg shadow-blue-500/25 transition-all hover:shadow-blue-500/40 disabled:opacity-60"
          >
            {generating ? (
              <>
                <span className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
                {t('projects.wizard.generating')}
              </>
            ) : (
              <>
                <Sparkles size={18} />
                {t('projects.wizard.generate')}
              </>
            )}
          </button>
          <button
            onClick={copyCliCommand}
            className="flex items-center gap-2 rounded-xl border border-slate-600 bg-slate-800 px-6 py-3 text-sm font-medium text-slate-300 transition-colors hover:bg-slate-700"
          >
            <Copy size={16} />
            {t('projects.wizard.copyCommand')}
          </button>
          <button className="flex items-center gap-2 rounded-xl border border-slate-600 bg-slate-800 px-6 py-3 text-sm font-medium text-slate-300 transition-colors hover:bg-slate-700">
            <Download size={16} />
            {t('projects.wizard.downloadZip')}
          </button>
        </div>
      </div>
    );
  };

  // ---------------------------------------------------------------------------
  // Main render
  // ---------------------------------------------------------------------------

  const renderCurrentStep = () => {
    switch (step) {
      case 0: return renderStep1();
      case 1: return renderStep2();
      case 2: return renderStep3();
      case 3: return renderStep4();
      case 4: return renderStep5();
      case 5: return renderStep6();
      default: return renderStep1();
    }
  };

  return (
    <div className="min-h-screen p-6">
      {/* Header */}
      <div className="mb-6 flex items-center gap-3">
        <button
          onClick={() => navigate('/admin/projects')}
          className="rounded-lg p-2 text-slate-400 transition-colors hover:bg-slate-800 hover:text-white"
        >
          <ArrowLeft size={20} />
        </button>
        <div>
          <h1 className="text-2xl font-bold text-white">{t('projects.wizard.title')}</h1>
          <p className="text-sm text-slate-400">{t('projects.wizard.subtitle')}</p>
        </div>
      </div>

      {/* Progress */}
      {renderProgressBar()}

      {/* Step content */}
      <div
        key={step}
        className={`transition-all duration-300 ${
          animDir === 'next' ? 'animate-fade-in-right' : 'animate-fade-in-left'
        }`}
      >
        {renderCurrentStep()}
      </div>

      {/* Navigation */}
      {step < STEPS.length - 1 && (
        <div className="mt-8 flex items-center justify-between">
          <button
            onClick={goBack}
            disabled={step === 0}
            className="flex items-center gap-2 rounded-lg px-5 py-2.5 text-sm font-medium text-slate-400 transition-colors hover:bg-slate-800 hover:text-white disabled:opacity-30 disabled:hover:bg-transparent"
          >
            <ArrowLeft size={16} />
            {t('common.back')}
          </button>
          <button
            onClick={goNext}
            disabled={!canProceed}
            className="flex items-center gap-2 rounded-lg bg-blue-600 px-6 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-blue-700 disabled:opacity-40"
          >
            {t('projects.wizard.next')}
            <ArrowRight size={16} />
          </button>
        </div>
      )}

      {step === STEPS.length - 1 && (
        <div className="mt-8 flex justify-start">
          <button
            onClick={goBack}
            className="flex items-center gap-2 rounded-lg px-5 py-2.5 text-sm font-medium text-slate-400 transition-colors hover:bg-slate-800 hover:text-white"
          >
            <ArrowLeft size={16} />
            {t('common.back')}
          </button>
        </div>
      )}
    </div>
  );
}
