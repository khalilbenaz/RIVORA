import { useState } from 'react';
import { Plus, ChevronUp, ChevronDown, Trash2, ArrowDown } from 'lucide-react';
import type { Flow, FlowNode, NodeType } from '../../api/flows';
import { nodeTypeConfigs } from './nodeTypes';
import { useTranslation } from 'react-i18next';

interface FlowPipelineProps {
  flow: Flow;
  onChange: (flow: Flow) => void;
}

let idCounter = Date.now();
function generateId() {
  return `p_${++idCounter}`;
}

export default function FlowPipeline({ flow, onChange }: FlowPipelineProps) {
  const { t } = useTranslation();
  const [editingNodeId, setEditingNodeId] = useState<string | null>(null);
  const [showAddMenuAt, setShowAddMenuAt] = useState<number | null>(null);

  // Order nodes by connections (topological) or fallback to array order
  const orderedNodes = getOrderedNodes(flow);

  function getOrderedNodes(f: Flow): FlowNode[] {
    if (f.nodes.length === 0) return [];
    // Simple: find root (node that is never a target), then follow connections
    const targetIds = new Set(f.connections.map((c) => c.toNodeId));
    const roots = f.nodes.filter((n) => !targetIds.has(n.id));
    const ordered: FlowNode[] = [];
    const visited = new Set<string>();
    const queue = roots.length > 0 ? [roots[0]] : [f.nodes[0]];

    while (queue.length > 0) {
      const node = queue.shift()!;
      if (visited.has(node.id)) continue;
      visited.add(node.id);
      ordered.push(node);
      const nextConns = f.connections.filter((c) => c.fromNodeId === node.id);
      for (const c of nextConns) {
        const next = f.nodes.find((n) => n.id === c.toNodeId);
        if (next && !visited.has(next.id)) queue.push(next);
      }
    }
    // Add any nodes not yet visited
    for (const n of f.nodes) {
      if (!visited.has(n.id)) ordered.push(n);
    }
    return ordered;
  }

  const addNodeAt = (index: number, type: NodeType) => {
    const cfg = nodeTypeConfigs[type];
    const newNode: FlowNode = {
      id: generateId(),
      type,
      label: cfg.label,
      config: {},
      x: 100 + index * 60,
      y: 100 + index * 120,
    };

    const newNodes = [...flow.nodes, newNode];
    let newConns = [...flow.connections];

    // Wire into pipeline
    if (orderedNodes.length > 0) {
      if (index > 0 && index <= orderedNodes.length) {
        const prevNode = orderedNodes[index - 1]!;
        const nextNode = index < orderedNodes.length ? orderedNodes[index]! : null;
        if (nextNode) {
          newConns = newConns.filter((c) => !(c.fromNodeId === prevNode.id && c.toNodeId === nextNode.id));
          newConns.push({ id: generateId(), fromNodeId: newNode.id, toNodeId: nextNode.id });
        }
        newConns.push({ id: generateId(), fromNodeId: prevNode.id, toNodeId: newNode.id });
      } else if (index === 0 && orderedNodes.length > 0) {
        newConns.push({ id: generateId(), fromNodeId: newNode.id, toNodeId: orderedNodes[0]!.id });
      }
    }

    onChange({ ...flow, nodes: newNodes, connections: newConns });
    setShowAddMenuAt(null);
    setEditingNodeId(newNode.id);
  };

  const deleteNode = (nodeId: string) => {
    const idx = orderedNodes.findIndex((n) => n.id === nodeId);
    const filtered = flow.connections.filter((c) => c.fromNodeId !== nodeId && c.toNodeId !== nodeId);
    // Re-wire: connect prev to next
    const newConns = idx > 0 && idx < orderedNodes.length - 1
      ? [...filtered, { id: generateId(), fromNodeId: orderedNodes[idx - 1]!.id, toNodeId: orderedNodes[idx + 1]!.id }]
      : filtered;
    onChange({
      ...flow,
      nodes: flow.nodes.filter((n) => n.id !== nodeId),
      connections: newConns,
    });
    if (editingNodeId === nodeId) setEditingNodeId(null);
  };

  const moveNode = (nodeId: string, direction: -1 | 1) => {
    const idx = orderedNodes.findIndex((n) => n.id === nodeId);
    const newIdx = idx + direction;
    if (newIdx < 0 || newIdx >= orderedNodes.length) return;

    // Swap in ordered list and rebuild connections
    const newOrder = [...orderedNodes];
    const a = newOrder[idx]!;
    const b = newOrder[newIdx]!;
    newOrder[idx] = b;
    newOrder[newIdx] = a;

    const newConns: Flow['connections'] = [];
    for (let i = 0; i < newOrder.length - 1; i++) {
      newConns.push({ id: generateId(), fromNodeId: newOrder[i]!.id, toNodeId: newOrder[i + 1]!.id });
    }
    onChange({ ...flow, connections: newConns });
  };

  const updateNodeConfig = (nodeId: string, key: string, value: string) => {
    onChange({
      ...flow,
      nodes: flow.nodes.map((n) => (n.id === nodeId ? { ...n, config: { ...n.config, [key]: value } } : n)),
    });
  };

  const updateNodeLabel = (nodeId: string, label: string) => {
    onChange({
      ...flow,
      nodes: flow.nodes.map((n) => (n.id === nodeId ? { ...n, label } : n)),
    });
  };

  const AddMenu = ({ index }: { index: number }) => (
    <div className="absolute left-1/2 z-10 mt-1 -translate-x-1/2 rounded-xl border border-slate-200 bg-white p-2 shadow-xl dark:border-slate-700 dark:bg-slate-800">
      <div className="grid grid-cols-4 gap-1">
        {Object.values(nodeTypeConfigs).map((cfg) => {
          const Icon = cfg.icon;
          return (
            <button
              key={cfg.type}
              onClick={() => addNodeAt(index, cfg.type)}
              className={`flex flex-col items-center gap-1 rounded-lg px-2 py-2 text-xs transition-colors hover:shadow ${cfg.color} ${cfg.borderColor} border`}
            >
              <Icon size={14} />
              {cfg.label}
            </button>
          );
        })}
      </div>
    </div>
  );

  return (
    <div className="mx-auto max-w-2xl px-6 py-8">
      {/* Add at top */}
      <div className="relative mb-2 flex justify-center">
        <button
          onClick={() => setShowAddMenuAt(showAddMenuAt === 0 ? null : 0)}
          className="flex h-8 w-8 items-center justify-center rounded-full border-2 border-dashed border-slate-300 text-slate-400 transition-colors hover:border-blue-400 hover:text-blue-500 dark:border-slate-600 dark:hover:border-blue-400"
        >
          <Plus size={16} />
        </button>
        {showAddMenuAt === 0 && <AddMenu index={0} />}
      </div>

      {orderedNodes.map((node, idx) => {
        const cfg = nodeTypeConfigs[node.type];
        const Icon = cfg.icon;
        const isEditing = editingNodeId === node.id;

        return (
          <div key={node.id}>
            {/* Step card */}
            <div
              className={`relative rounded-xl border-2 transition-all ${cfg.borderColor} ${isEditing ? 'ring-2 ring-blue-500 ring-offset-2' : ''} bg-white shadow-sm dark:bg-slate-800`}
            >
              <div
                className="flex cursor-pointer items-center gap-3 px-4 py-3"
                onClick={() => setEditingNodeId(isEditing ? null : node.id)}
              >
                {/* Step number */}
                <div className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-sm font-bold text-white ${cfg.borderColor.replace('border-', 'bg-')}`}>
                  {idx + 1}
                </div>
                <div className={`rounded-lg p-1.5 ${cfg.color}`}>
                  <Icon size={16} />
                </div>
                <div className="flex-1">
                  <div className="text-sm font-semibold text-slate-800 dark:text-slate-200">{node.label}</div>
                  <div className="text-xs text-slate-500">
                    {Object.entries(node.config).filter(([, v]) => v).slice(0, 2).map(([k, v]) => `${k}: ${String(v).slice(0, 20)}`).join(' | ') || t('flows.noConfig')}
                  </div>
                </div>
                <div className="flex items-center gap-1">
                  <button onClick={(e) => { e.stopPropagation(); moveNode(node.id, -1); }} disabled={idx === 0} className="rounded p-1 text-slate-400 hover:text-slate-600 disabled:opacity-30 dark:hover:text-slate-300">
                    <ChevronUp size={14} />
                  </button>
                  <button onClick={(e) => { e.stopPropagation(); moveNode(node.id, 1); }} disabled={idx === orderedNodes.length - 1} className="rounded p-1 text-slate-400 hover:text-slate-600 disabled:opacity-30 dark:hover:text-slate-300">
                    <ChevronDown size={14} />
                  </button>
                  <button onClick={(e) => { e.stopPropagation(); deleteNode(node.id); }} className="rounded p-1 text-red-400 hover:text-red-600">
                    <Trash2 size={14} />
                  </button>
                </div>
              </div>

              {/* Inline config editor */}
              {isEditing && (
                <div className="border-t border-slate-200 px-4 py-3 dark:border-slate-700">
                  <label className="mb-2 block">
                    <span className="mb-1 block text-xs font-medium text-slate-500">{t('flows.label')}</span>
                    <input
                      value={node.label}
                      onChange={(e) => updateNodeLabel(node.id, e.target.value)}
                      className="w-full rounded-lg border border-slate-300 px-3 py-1.5 text-sm dark:border-slate-600 dark:bg-slate-700 dark:text-slate-200"
                    />
                  </label>
                  {cfg.configFields.map((field) => (
                    <label key={field.key} className="mb-2 block">
                      <span className="mb-1 block text-xs font-medium text-slate-500">{field.label}</span>
                      {field.type === 'select' ? (
                        <select
                          value={node.config[field.key] ?? ''}
                          onChange={(e) => updateNodeConfig(node.id, field.key, e.target.value)}
                          className="w-full rounded-lg border border-slate-300 px-3 py-1.5 text-sm dark:border-slate-600 dark:bg-slate-700 dark:text-slate-200"
                        >
                          <option value="">-- Select --</option>
                          {field.options?.map((o) => <option key={o} value={o}>{o}</option>)}
                        </select>
                      ) : field.type === 'textarea' ? (
                        <textarea
                          value={node.config[field.key] ?? ''}
                          onChange={(e) => updateNodeConfig(node.id, field.key, e.target.value)}
                          rows={3}
                          className="w-full rounded-lg border border-slate-300 px-3 py-1.5 text-sm dark:border-slate-600 dark:bg-slate-700 dark:text-slate-200"
                        />
                      ) : (
                        <input
                          type={field.type}
                          value={node.config[field.key] ?? ''}
                          onChange={(e) => updateNodeConfig(node.id, field.key, e.target.value)}
                          className="w-full rounded-lg border border-slate-300 px-3 py-1.5 text-sm dark:border-slate-600 dark:bg-slate-700 dark:text-slate-200"
                        />
                      )}
                    </label>
                  ))}
                </div>
              )}
            </div>

            {/* Arrow + Add button between steps */}
            {idx < orderedNodes.length - 1 && (
              <div className="relative flex flex-col items-center py-1">
                <ArrowDown size={18} className="text-slate-300 dark:text-slate-600" />
                <button
                  onClick={() => setShowAddMenuAt(showAddMenuAt === idx + 1 ? null : idx + 1)}
                  className="flex h-6 w-6 items-center justify-center rounded-full border-2 border-dashed border-slate-300 text-slate-400 transition-colors hover:border-blue-400 hover:text-blue-500 dark:border-slate-600 dark:hover:border-blue-400"
                >
                  <Plus size={12} />
                </button>
                {showAddMenuAt === idx + 1 && <AddMenu index={idx + 1} />}
              </div>
            )}
          </div>
        );
      })}

      {/* Add at bottom */}
      {orderedNodes.length > 0 && (
        <div className="relative mt-2 flex flex-col items-center">
          <ArrowDown size={18} className="text-slate-300 dark:text-slate-600" />
          <button
            onClick={() => setShowAddMenuAt(showAddMenuAt === orderedNodes.length ? null : orderedNodes.length)}
            className="flex h-8 w-8 items-center justify-center rounded-full border-2 border-dashed border-slate-300 text-slate-400 transition-colors hover:border-blue-400 hover:text-blue-500 dark:border-slate-600 dark:hover:border-blue-400"
          >
            <Plus size={16} />
          </button>
          {showAddMenuAt === orderedNodes.length && <AddMenu index={orderedNodes.length} />}
        </div>
      )}

      {orderedNodes.length === 0 && (
        <div className="mt-8 text-center text-slate-400">
          <p className="mb-2 text-sm">{t('flows.emptyPipeline')}</p>
          <p className="text-xs">{t('flows.clickPlusToAdd')}</p>
        </div>
      )}
    </div>
  );
}
