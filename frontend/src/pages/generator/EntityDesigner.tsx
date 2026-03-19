import { useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Plus,
  Trash2,
  Copy,
  Download,
  Database,
  Code2,
  Eye,
} from 'lucide-react';
import type { EntityDefinition, EntityField, FieldType, GeneratedCode } from './types';
import { generateAll } from './codeGenerator';

const FIELD_TYPES: FieldType[] = ['string', 'int', 'decimal', 'bool', 'DateTime', 'Guid', 'enum', 'relation'];

const RELATION_TYPES = ['one-to-one', 'one-to-many', 'many-to-many'] as const;

function newField(): EntityField {
  return {
    id: crypto.randomUUID(),
    name: '',
    type: 'string',
    required: false,
    isSearchable: false,
    isFilterable: false,
    showInList: true,
    showInForm: true,
  };
}

function newEntity(): EntityDefinition {
  return {
    id: crypto.randomUUID(),
    name: '',
    pluralName: '',
    description: '',
    fields: [],
    hasAudit: true,
    hasSoftDelete: false,
    hasTenantId: false,
    apiPrefix: '',
  };
}

// ─── Code Preview Tab ───────────────────────────────────────────────
function CodeTab({ gen }: { gen: GeneratedCode }) {
  const [copied, setCopied] = useState(false);

  const handleCopy = () => {
    navigator.clipboard.writeText(gen.code);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className="relative">
      <button
        onClick={handleCopy}
        className="absolute right-2 top-2 rounded bg-slate-600 px-2 py-1 text-xs text-white hover:bg-slate-500"
      >
        {copied ? 'Copied!' : <Copy size={14} />}
      </button>
      <pre className="max-h-[500px] overflow-auto rounded-lg bg-slate-900 p-4 text-sm text-green-300">
        <code>{gen.code}</code>
      </pre>
    </div>
  );
}

// ─── Entity Card (UML-style) ────────────────────────────────────────
function EntityCard({ entity, small }: { entity: EntityDefinition; small?: boolean }) {
  if (!entity.name) return null;
  const textSize = small ? 'text-[10px]' : 'text-xs';
  return (
    <div className={`inline-block rounded border border-slate-600 bg-slate-800 ${small ? 'min-w-[120px]' : 'min-w-[200px]'}`}>
      <div className={`border-b border-slate-600 bg-blue-600/30 px-3 py-1.5 text-center font-bold text-blue-300 ${small ? 'text-xs' : 'text-sm'}`}>
        {entity.name}
      </div>
      <div className={`px-3 py-1.5 ${textSize} space-y-0.5 text-slate-300`}>
        <div className="text-slate-500">+ Id : Guid</div>
        {entity.fields.map(f => (
          <div key={f.id}>
            <span className="text-slate-500">{f.required ? '+' : '-'}</span>{' '}
            {f.name || '???'} : <span className="text-violet-300">{f.type}</span>
          </div>
        ))}
        {entity.hasAudit && <div className="text-slate-500 italic">+ CreatedAt, UpdatedAt...</div>}
        {entity.hasSoftDelete && <div className="text-slate-500 italic">+ IsDeleted</div>}
        {entity.hasTenantId && <div className="text-slate-500 italic">+ TenantId</div>}
      </div>
    </div>
  );
}

// ─── Entity Relationship Diagram (SVG) ─────────────────────────────
function ERDiagram({ entities }: { entities: EntityDefinition[] }) {
  const validEntities = entities.filter(e => e.name);
  if (validEntities.length === 0) {
    return (
      <div className="flex h-48 items-center justify-center text-slate-500">
        Add entities to see the ER diagram
      </div>
    );
  }

  const boxW = 180;
  const boxPadding = 40;
  const fieldH = 18;

  const positions = validEntities.map((_, i) => ({
    x: 20 + i * (boxW + boxPadding),
    y: 20,
  }));

  const entityBoxH = (e: EntityDefinition) => 30 + e.fields.length * fieldH + 10;

  // Find relations
  const relations: { from: number; to: number; label: string }[] = [];
  validEntities.forEach((e, i) => {
    e.fields.filter(f => f.type === 'relation' && f.relationTarget).forEach(f => {
      const targetIdx = validEntities.findIndex(t => t.name === f.relationTarget);
      if (targetIdx >= 0) {
        relations.push({ from: i, to: targetIdx, label: f.relationType || '1:N' });
      }
    });
  });

  const totalW = Math.max(600, positions.length * (boxW + boxPadding) + 40);
  const maxH = Math.max(200, ...validEntities.map(entityBoxH)) + 60;

  return (
    <svg width="100%" height={maxH} viewBox={`0 0 ${totalW} ${maxH}`} className="rounded-lg bg-slate-950/50">
      {/* Relation lines */}
      {relations.map((r, ri) => {
        const fromPos = positions[r.from]!;
        const toPos = positions[r.to]!;
        const fromEntity = validEntities[r.from]!;
        const toEntity = validEntities[r.to]!;
        const fromH = entityBoxH(fromEntity);
        const x1 = fromPos.x + boxW;
        const y1 = fromPos.y + fromH / 2;
        const x2 = toPos.x;
        const y2 = toPos.y + entityBoxH(toEntity) / 2;
        const mx = (x1 + x2) / 2;
        return (
          <g key={ri}>
            <path
              d={`M ${x1} ${y1} C ${mx} ${y1}, ${mx} ${y2}, ${x2} ${y2}`}
              fill="none"
              stroke="#60a5fa"
              strokeWidth={2}
              markerEnd="url(#arrow)"
            />
            <text x={mx} y={Math.min(y1, y2) - 6} textAnchor="middle" fill="#94a3b8" fontSize={10}>
              {r.label}
            </text>
          </g>
        );
      })}

      {/* Arrow marker */}
      <defs>
        <marker id="arrow" viewBox="0 0 10 10" refX={10} refY={5} markerWidth={6} markerHeight={6} orient="auto">
          <path d="M 0 0 L 10 5 L 0 10 z" fill="#60a5fa" />
        </marker>
      </defs>

      {/* Entity boxes */}
      {validEntities.map((e, i) => {
        const pos = positions[i]!;
        const h = entityBoxH(e);
        return (
          <g key={e.id}>
            <rect x={pos.x} y={pos.y} width={boxW} height={h} rx={6} fill="#1e293b" stroke="#475569" strokeWidth={1} />
            <rect x={pos.x} y={pos.y} width={boxW} height={26} rx={6} fill="#1e40af" opacity={0.4} />
            <rect x={pos.x} y={pos.y + 20} width={boxW} height={6} fill="#1e293b" />
            <text x={pos.x + boxW / 2} y={pos.y + 18} textAnchor="middle" fill="#93c5fd" fontWeight="bold" fontSize={12}>
              {e.name}
            </text>
            <line x1={pos.x} y1={pos.y + 26} x2={pos.x + boxW} y2={pos.y + 26} stroke="#475569" />
            {e.fields.map((f, fi) => (
              <text key={f.id} x={pos.x + 8} y={pos.y + 42 + fi * fieldH} fill="#cbd5e1" fontSize={10}>
                {f.name || '???'}: {f.type}
              </text>
            ))}
          </g>
        );
      })}
    </svg>
  );
}

// ─── Main Component ─────────────────────────────────────────────────
export default function EntityDesigner() {
  const { t } = useTranslation();
  const [entities, setEntities] = useState<EntityDefinition[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState(0);
  const [showPreview, setShowPreview] = useState(false);

  const selected = entities.find(e => e.id === selectedId) || null;

  const updateEntity = useCallback((id: string, patch: Partial<EntityDefinition>) => {
    setEntities(prev => prev.map(e => e.id === id ? { ...e, ...patch } : e));
  }, []);

  const addEntity = () => {
    const e = newEntity();
    setEntities(prev => [...prev, e]);
    setSelectedId(e.id);
  };

  const removeEntity = (id: string) => {
    setEntities(prev => prev.filter(e => e.id !== id));
    if (selectedId === id) setSelectedId(null);
  };

  const addField = () => {
    if (!selected) return;
    updateEntity(selected.id, { fields: [...selected.fields, newField()] });
  };

  const updateField = (fieldId: string, patch: Partial<EntityField>) => {
    if (!selected) return;
    updateEntity(selected.id, {
      fields: selected.fields.map(f => f.id === fieldId ? { ...f, ...patch } : f),
    });
  };

  const removeField = (fieldId: string) => {
    if (!selected) return;
    updateEntity(selected.id, {
      fields: selected.fields.filter(f => f.id !== fieldId),
    });
  };

  // Auto-generate plural / prefix
  const handleNameChange = (name: string) => {
    if (!selected) return;
    const plural = name.endsWith('y') ? name.slice(0, -1) + 'ies'
      : name.endsWith('s') || name.endsWith('x') || name.endsWith('ch') || name.endsWith('sh') ? name + 'es'
      : name + 's';
    updateEntity(selected.id, {
      name,
      pluralName: plural,
      apiPrefix: plural.toLowerCase(),
    });
  };

  // Code generation
  const generatedFiles = selected ? generateAll(selected) : [];
  const tabLabels = generatedFiles.map(g => `${g.layer}: ${g.fileName}`);

  // Download ZIP (simple concatenation since we don't have JSZip)
  const handleDownloadAll = () => {
    if (generatedFiles.length === 0) return;
    // Download each file individually using data URIs
    generatedFiles.forEach(g => {
      const blob = new Blob([g.code], { type: 'text/plain' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = g.fileName;
      a.click();
      URL.revokeObjectURL(url);
    });
  };

  return (
    <div className="flex h-full flex-col overflow-hidden">
      {/* Header */}
      <div className="flex items-center justify-between border-b border-slate-700 bg-slate-800/50 px-6 py-3">
        <div className="flex items-center gap-3">
          <Code2 className="text-blue-400" size={22} />
          <h1 className="text-xl font-bold text-white">{t('generator.title')}</h1>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={() => setShowPreview(!showPreview)}
            className="flex items-center gap-1.5 rounded-lg bg-slate-700 px-3 py-1.5 text-sm text-slate-300 hover:bg-slate-600"
          >
            <Eye size={14} />
            {showPreview ? t('generator.hideErd') : t('generator.showErd')}
          </button>
          <button
            onClick={handleDownloadAll}
            disabled={generatedFiles.length === 0}
            className="flex items-center gap-1.5 rounded-lg bg-blue-600 px-3 py-1.5 text-sm text-white hover:bg-blue-500 disabled:opacity-40"
          >
            <Download size={14} />
            {t('generator.generateAll')}
          </button>
        </div>
      </div>

      <div className="flex flex-1 overflow-hidden">
        {/* ─── Left: Entity list ──────────────────────────── */}
        <div className="w-56 flex-shrink-0 overflow-y-auto border-r border-slate-700 bg-slate-800/30 p-3">
          <button
            onClick={addEntity}
            className="mb-3 flex w-full items-center justify-center gap-1.5 rounded-lg border border-dashed border-slate-600 py-2 text-sm text-slate-400 hover:border-blue-500 hover:text-blue-400"
          >
            <Plus size={14} />
            {t('generator.addEntity')}
          </button>

          {entities.map(e => (
            <div
              key={e.id}
              onClick={() => setSelectedId(e.id)}
              className={`group mb-1 flex cursor-pointer items-center justify-between rounded-lg px-3 py-2 text-sm transition-colors ${
                selectedId === e.id
                  ? 'bg-blue-600/20 text-blue-300'
                  : 'text-slate-400 hover:bg-slate-700/50 hover:text-slate-200'
              }`}
            >
              <div className="flex items-center gap-2">
                <Database size={14} />
                <span>{e.name || 'Unnamed'}</span>
              </div>
              <button
                onClick={(ev) => { ev.stopPropagation(); removeEntity(e.id); }}
                className="hidden text-red-400 hover:text-red-300 group-hover:block"
              >
                <Trash2 size={14} />
              </button>
            </div>
          ))}

          {entities.length === 0 && (
            <p className="mt-4 text-center text-xs text-slate-500">
              {t('generator.noEntities')}
            </p>
          )}
        </div>

        {/* ─── Center: Field editor ──────────────────────── */}
        <div className="flex-1 overflow-y-auto p-4">
          {!selected ? (
            <div className="flex h-full items-center justify-center text-slate-500">
              <div className="text-center">
                <Database size={48} className="mx-auto mb-3 opacity-30" />
                <p>{t('generator.selectEntity')}</p>
              </div>
            </div>
          ) : (
            <div className="space-y-4">
              {/* Entity metadata */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="mb-1 block text-xs text-slate-400">{t('generator.entityName')}</label>
                  <input
                    value={selected.name}
                    onChange={e => handleNameChange(e.target.value)}
                    placeholder="e.g. Product"
                    className="w-full rounded-lg border border-slate-600 bg-slate-800 px-3 py-2 text-sm text-white placeholder-slate-500 focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div>
                  <label className="mb-1 block text-xs text-slate-400">{t('generator.pluralName')}</label>
                  <input
                    value={selected.pluralName}
                    onChange={e => updateEntity(selected.id, { pluralName: e.target.value })}
                    placeholder="e.g. Products"
                    className="w-full rounded-lg border border-slate-600 bg-slate-800 px-3 py-2 text-sm text-white placeholder-slate-500 focus:border-blue-500 focus:outline-none"
                  />
                </div>
              </div>

              <div>
                <label className="mb-1 block text-xs text-slate-400">{t('generator.description')}</label>
                <textarea
                  value={selected.description || ''}
                  onChange={e => updateEntity(selected.id, { description: e.target.value })}
                  rows={2}
                  className="w-full rounded-lg border border-slate-600 bg-slate-800 px-3 py-2 text-sm text-white placeholder-slate-500 focus:border-blue-500 focus:outline-none"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="mb-1 block text-xs text-slate-400">{t('generator.apiPrefix')}</label>
                  <input
                    value={selected.apiPrefix}
                    onChange={e => updateEntity(selected.id, { apiPrefix: e.target.value })}
                    className="w-full rounded-lg border border-slate-600 bg-slate-800 px-3 py-2 text-sm text-white placeholder-slate-500 focus:border-blue-500 focus:outline-none"
                  />
                </div>
                <div className="flex items-end gap-4 pb-2">
                  <label className="flex items-center gap-1.5 text-sm text-slate-300">
                    <input
                      type="checkbox"
                      checked={selected.hasAudit}
                      onChange={e => updateEntity(selected.id, { hasAudit: e.target.checked })}
                      className="rounded border-slate-600"
                    />
                    Audit
                  </label>
                  <label className="flex items-center gap-1.5 text-sm text-slate-300">
                    <input
                      type="checkbox"
                      checked={selected.hasSoftDelete}
                      onChange={e => updateEntity(selected.id, { hasSoftDelete: e.target.checked })}
                      className="rounded border-slate-600"
                    />
                    Soft Delete
                  </label>
                  <label className="flex items-center gap-1.5 text-sm text-slate-300">
                    <input
                      type="checkbox"
                      checked={selected.hasTenantId}
                      onChange={e => updateEntity(selected.id, { hasTenantId: e.target.checked })}
                      className="rounded border-slate-600"
                    />
                    Tenant
                  </label>
                </div>
              </div>

              {/* Field table */}
              <div>
                <div className="mb-2 flex items-center justify-between">
                  <h3 className="text-sm font-semibold text-white">{t('generator.fields')}</h3>
                  <button
                    onClick={addField}
                    className="flex items-center gap-1 rounded bg-blue-600/20 px-2 py-1 text-xs text-blue-400 hover:bg-blue-600/30"
                  >
                    <Plus size={12} />
                    {t('generator.addField')}
                  </button>
                </div>

                <div className="overflow-x-auto rounded-lg border border-slate-700">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b border-slate-700 bg-slate-800/80 text-left text-xs text-slate-400">
                        <th className="px-2 py-2">{t('generator.fieldName')}</th>
                        <th className="px-2 py-2">{t('generator.fieldType')}</th>
                        <th className="px-2 py-2 text-center">Req</th>
                        <th className="px-2 py-2 text-center">Search</th>
                        <th className="px-2 py-2 text-center">Filter</th>
                        <th className="px-2 py-2 text-center">List</th>
                        <th className="px-2 py-2 text-center">Form</th>
                        <th className="px-2 py-2"></th>
                      </tr>
                    </thead>
                    <tbody>
                      {selected.fields.map(f => (
                        <tr key={f.id} className="border-b border-slate-700/50">
                          <td className="px-2 py-1.5">
                            <input
                              value={f.name}
                              onChange={e => updateField(f.id, { name: e.target.value })}
                              placeholder="fieldName"
                              className="w-full rounded border border-slate-600 bg-slate-800 px-2 py-1 text-xs text-white focus:border-blue-500 focus:outline-none"
                            />
                          </td>
                          <td className="px-2 py-1.5">
                            <select
                              value={f.type}
                              onChange={e => updateField(f.id, { type: e.target.value as FieldType })}
                              className="w-full rounded border border-slate-600 bg-slate-800 px-2 py-1 text-xs text-white focus:border-blue-500 focus:outline-none"
                            >
                              {FIELD_TYPES.map(ft => (
                                <option key={ft} value={ft}>{ft}</option>
                              ))}
                            </select>
                          </td>
                          <td className="px-2 py-1.5 text-center">
                            <input type="checkbox" checked={f.required} onChange={e => updateField(f.id, { required: e.target.checked })} />
                          </td>
                          <td className="px-2 py-1.5 text-center">
                            <input type="checkbox" checked={f.isSearchable} onChange={e => updateField(f.id, { isSearchable: e.target.checked })} />
                          </td>
                          <td className="px-2 py-1.5 text-center">
                            <input type="checkbox" checked={f.isFilterable} onChange={e => updateField(f.id, { isFilterable: e.target.checked })} />
                          </td>
                          <td className="px-2 py-1.5 text-center">
                            <input type="checkbox" checked={f.showInList} onChange={e => updateField(f.id, { showInList: e.target.checked })} />
                          </td>
                          <td className="px-2 py-1.5 text-center">
                            <input type="checkbox" checked={f.showInForm} onChange={e => updateField(f.id, { showInForm: e.target.checked })} />
                          </td>
                          <td className="px-2 py-1.5 text-center">
                            <button onClick={() => removeField(f.id)} className="text-red-400 hover:text-red-300">
                              <Trash2 size={14} />
                            </button>
                          </td>
                        </tr>
                      ))}
                      {selected.fields.length === 0 && (
                        <tr>
                          <td colSpan={8} className="py-4 text-center text-xs text-slate-500">
                            {t('generator.noFields')}
                          </td>
                        </tr>
                      )}
                    </tbody>
                  </table>
                </div>

                {/* Enum / relation extra inputs */}
                {selected.fields.filter(f => f.type === 'enum').map(f => (
                  <div key={f.id} className="mt-2 rounded-lg bg-slate-800/50 p-2">
                    <label className="mb-1 block text-xs text-violet-300">
                      {f.name || '???'} - Enum values (comma separated)
                    </label>
                    <input
                      value={(f.enumValues || []).join(', ')}
                      onChange={e => updateField(f.id, { enumValues: e.target.value.split(',').map(v => v.trim()).filter(Boolean) })}
                      placeholder="Active, Inactive, Pending"
                      className="w-full rounded border border-slate-600 bg-slate-800 px-2 py-1 text-xs text-white focus:border-blue-500 focus:outline-none"
                    />
                  </div>
                ))}

                {selected.fields.filter(f => f.type === 'relation').map(f => (
                  <div key={f.id} className="mt-2 flex gap-2 rounded-lg bg-slate-800/50 p-2">
                    <div className="flex-1">
                      <label className="mb-1 block text-xs text-blue-300">{f.name || '???'} - Target Entity</label>
                      <select
                        value={f.relationTarget || ''}
                        onChange={e => updateField(f.id, { relationTarget: e.target.value })}
                        className="w-full rounded border border-slate-600 bg-slate-800 px-2 py-1 text-xs text-white focus:border-blue-500 focus:outline-none"
                      >
                        <option value="">Select...</option>
                        {entities.filter(e => e.id !== selected.id && e.name).map(e => (
                          <option key={e.id} value={e.name}>{e.name}</option>
                        ))}
                      </select>
                    </div>
                    <div className="flex-1">
                      <label className="mb-1 block text-xs text-blue-300">Relation Type</label>
                      <select
                        value={f.relationType || 'one-to-many'}
                        onChange={e => updateField(f.id, { relationType: e.target.value as EntityField['relationType'] })}
                        className="w-full rounded border border-slate-600 bg-slate-800 px-2 py-1 text-xs text-white focus:border-blue-500 focus:outline-none"
                      >
                        {RELATION_TYPES.map(rt => (
                          <option key={rt} value={rt}>{rt}</option>
                        ))}
                      </select>
                    </div>
                  </div>
                ))}
              </div>

              {/* Visual entity card preview */}
              {selected.name && (
                <div>
                  <h3 className="mb-2 text-sm font-semibold text-white">{t('generator.preview')}</h3>
                  <EntityCard entity={selected} />
                </div>
              )}
            </div>
          )}
        </div>

        {/* ─── Right: Code preview ───────────────────────── */}
        <div className="w-[420px] flex-shrink-0 overflow-y-auto border-l border-slate-700 bg-slate-800/20 p-3">
          {generatedFiles.length === 0 ? (
            <div className="flex h-full items-center justify-center text-slate-500">
              <div className="text-center">
                <Code2 size={48} className="mx-auto mb-3 opacity-30" />
                <p className="text-sm">{t('generator.noCode')}</p>
              </div>
            </div>
          ) : (
            <>
              {/* Tabs */}
              <div className="mb-3 flex flex-wrap gap-1">
                {tabLabels.map((label, i) => (
                  <button
                    key={i}
                    onClick={() => setActiveTab(i)}
                    className={`rounded px-2 py-1 text-[10px] font-medium transition-colors ${
                      activeTab === i
                        ? 'bg-blue-600 text-white'
                        : 'bg-slate-700 text-slate-400 hover:bg-slate-600'
                    }`}
                  >
                    {label}
                  </button>
                ))}
              </div>

              {generatedFiles[activeTab] && (
                <CodeTab gen={generatedFiles[activeTab]} />
              )}
            </>
          )}
        </div>
      </div>

      {/* ─── Bottom: ER Diagram ─────────────────────────── */}
      {showPreview && (
        <div className="border-t border-slate-700 bg-slate-800/30 p-3">
          <h3 className="mb-2 text-sm font-semibold text-white">{t('generator.erDiagram')}</h3>
          <ERDiagram entities={entities} />
        </div>
      )}
    </div>
  );
}
