import { useState, useRef, useCallback, useEffect } from 'react';
import { Plus, Minus, Trash2 } from 'lucide-react';
import type { Flow, FlowNode, FlowConnection, NodeType } from '../../api/flows';
import { nodeTypeConfigs } from './nodeTypes';
import { useTranslation } from 'react-i18next';

interface FlowCanvasProps {
  flow: Flow;
  onChange: (flow: Flow) => void;
}

let idCounter = Date.now();
function generateId() {
  return `n_${++idCounter}`;
}

export default function FlowCanvas({ flow, onChange }: FlowCanvasProps) {
  const { t } = useTranslation();
  const canvasRef = useRef<HTMLDivElement>(null);
  const [zoom, setZoom] = useState(1);
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);
  const [draggingNodeId, setDraggingNodeId] = useState<string | null>(null);
  const [dragOffset, setDragOffset] = useState({ x: 0, y: 0 });
  const [connectingFrom, setConnectingFrom] = useState<string | null>(null);

  const selectedNode = flow.nodes.find((n) => n.id === selectedNodeId) ?? null;

  // --- Node dragging ---
  const handleNodeMouseDown = (e: React.MouseEvent, nodeId: string) => {
    if ((e.target as HTMLElement).closest('[data-connector]')) return;
    e.stopPropagation();
    const node = flow.nodes.find((n) => n.id === nodeId);
    if (!node) return;
    setDraggingNodeId(nodeId);
    setDragOffset({ x: e.clientX - node.x * zoom, y: e.clientY - node.y * zoom });
  };

  const handleMouseMove = useCallback(
    (e: MouseEvent) => {
      if (!draggingNodeId) return;
      const newX = (e.clientX - dragOffset.x) / zoom;
      const newY = (e.clientY - dragOffset.y) / zoom;
      onChange({
        ...flow,
        nodes: flow.nodes.map((n) => (n.id === draggingNodeId ? { ...n, x: Math.max(0, newX), y: Math.max(0, newY) } : n)),
      });
    },
    [draggingNodeId, dragOffset, zoom, flow, onChange],
  );

  const handleMouseUp = useCallback(() => {
    setDraggingNodeId(null);
  }, []);

  useEffect(() => {
    window.addEventListener('mousemove', handleMouseMove);
    window.addEventListener('mouseup', handleMouseUp);
    return () => {
      window.removeEventListener('mousemove', handleMouseMove);
      window.removeEventListener('mouseup', handleMouseUp);
    };
  }, [handleMouseMove, handleMouseUp]);

  // --- Add node ---
  const addNode = (type: NodeType, x?: number, y?: number) => {
    const cfg = nodeTypeConfigs[type];
    const newNode: FlowNode = {
      id: generateId(),
      type,
      label: cfg.label,
      config: {},
      x: x ?? 200 + flow.nodes.length * 60,
      y: y ?? 200 + flow.nodes.length * 40,
    };
    onChange({ ...flow, nodes: [...flow.nodes, newNode] });
    setSelectedNodeId(newNode.id);
  };

  // --- Delete node ---
  const deleteNode = (id: string) => {
    onChange({
      ...flow,
      nodes: flow.nodes.filter((n) => n.id !== id),
      connections: flow.connections.filter((c) => c.fromNodeId !== id && c.toNodeId !== id),
    });
    if (selectedNodeId === id) setSelectedNodeId(null);
  };

  // --- Connector clicks ---
  const handleOutputClick = (nodeId: string) => {
    if (connectingFrom && connectingFrom !== nodeId) {
      // complete the connection
      const exists = flow.connections.some((c) => c.fromNodeId === connectingFrom && c.toNodeId === nodeId);
      if (!exists) {
        const conn: FlowConnection = { id: generateId(), fromNodeId: connectingFrom, toNodeId: nodeId };
        onChange({ ...flow, connections: [...flow.connections, conn] });
      }
      setConnectingFrom(null);
    } else {
      setConnectingFrom(nodeId);
    }
  };

  const handleInputClick = (nodeId: string) => {
    if (connectingFrom && connectingFrom !== nodeId) {
      const exists = flow.connections.some((c) => c.fromNodeId === connectingFrom && c.toNodeId === nodeId);
      if (!exists) {
        const conn: FlowConnection = { id: generateId(), fromNodeId: connectingFrom, toNodeId: nodeId };
        onChange({ ...flow, connections: [...flow.connections, conn] });
      }
      setConnectingFrom(null);
    }
  };

  const deleteConnection = (connId: string) => {
    onChange({ ...flow, connections: flow.connections.filter((c) => c.id !== connId) });
  };

  // --- Update node config ---
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

  // --- Drop handler for palette ---
  const handleCanvasDrop = (e: React.DragEvent) => {
    e.preventDefault();
    const type = e.dataTransfer.getData('nodeType') as NodeType;
    if (!type || !nodeTypeConfigs[type]) return;
    const rect = canvasRef.current?.getBoundingClientRect();
    if (!rect) return;
    const x = (e.clientX - rect.left) / zoom;
    const y = (e.clientY - rect.top) / zoom;
    addNode(type, x, y);
  };

  const handleCanvasDragOver = (e: React.DragEvent) => {
    e.preventDefault();
  };

  // --- Connection SVG paths ---
  const NODE_W = 200;
  const NODE_H = 80;

  function getConnectionPath(conn: FlowConnection) {
    const from = flow.nodes.find((n) => n.id === conn.fromNodeId);
    const to = flow.nodes.find((n) => n.id === conn.toNodeId);
    if (!from || !to) return '';
    const x1 = from.x + NODE_W;
    const y1 = from.y + NODE_H / 2;
    const x2 = to.x;
    const y2 = to.y + NODE_H / 2;
    return `M ${x1},${y1} C ${x1 + 80},${y1} ${x2 - 80},${y2} ${x2},${y2}`;
  }

  return (
    <div className="flex flex-1 overflow-hidden">
      {/* Left: Node palette */}
      <div className="w-48 shrink-0 border-r border-slate-200 bg-white p-3 dark:border-slate-700 dark:bg-slate-800">
        <h3 className="mb-3 text-xs font-semibold uppercase tracking-wider text-slate-500 dark:text-slate-400">
          {t('flows.nodeTypes')}
        </h3>
        <div className="space-y-1.5">
          {Object.values(nodeTypeConfigs).map((cfg) => {
            const Icon = cfg.icon;
            return (
              <div
                key={cfg.type}
                draggable
                onDragStart={(e) => e.dataTransfer.setData('nodeType', cfg.type)}
                onClick={() => addNode(cfg.type)}
                className={`flex cursor-grab items-center gap-2 rounded-lg border px-3 py-2 text-sm font-medium transition-colors hover:shadow-sm ${cfg.color} ${cfg.borderColor} border dark:bg-opacity-20`}
              >
                <Icon size={15} />
                {cfg.label}
              </div>
            );
          })}
        </div>
      </div>

      {/* Center: Canvas */}
      <div className="relative flex-1 overflow-auto bg-slate-50 dark:bg-slate-900">
        {/* Zoom controls */}
        <div className="absolute right-3 top-3 z-20 flex items-center gap-1 rounded-lg border border-slate-200 bg-white px-1 py-1 shadow-sm dark:border-slate-700 dark:bg-slate-800">
          <button onClick={() => setZoom((z) => Math.max(0.25, z - 0.1))} className="rounded p-1 hover:bg-slate-100 dark:hover:bg-slate-700">
            <Minus size={14} />
          </button>
          <span className="min-w-[40px] text-center text-xs text-slate-600 dark:text-slate-400">{Math.round(zoom * 100)}%</span>
          <button onClick={() => setZoom((z) => Math.min(2, z + 0.1))} className="rounded p-1 hover:bg-slate-100 dark:hover:bg-slate-700">
            <Plus size={14} />
          </button>
        </div>

        {connectingFrom && (
          <div className="absolute left-3 top-3 z-20 rounded-lg bg-blue-600 px-3 py-1.5 text-xs font-medium text-white shadow">
            {t('flows.clickTargetNode')}
            <button onClick={() => setConnectingFrom(null)} className="ml-2 underline">
              {t('common.cancel')}
            </button>
          </div>
        )}

        <div
          ref={canvasRef}
          onDrop={handleCanvasDrop}
          onDragOver={handleCanvasDragOver}
          onClick={() => { setSelectedNodeId(null); setConnectingFrom(null); }}
          className="relative"
          style={{
            minHeight: 700,
            minWidth: 1200,
            transform: `scale(${zoom})`,
            transformOrigin: 'top left',
            backgroundImage: 'radial-gradient(circle, #cbd5e1 1px, transparent 1px)',
            backgroundSize: '24px 24px',
          }}
        >
          {/* SVG connections */}
          <svg className="pointer-events-none absolute inset-0" style={{ width: '100%', height: '100%', overflow: 'visible' }}>
            {flow.connections.map((conn) => (
              <g key={conn.id}>
                <path
                  d={getConnectionPath(conn)}
                  fill="none"
                  stroke="#94a3b8"
                  strokeWidth={2}
                  className="pointer-events-auto cursor-pointer transition-colors hover:stroke-red-400"
                  onClick={(e) => { e.stopPropagation(); deleteConnection(conn.id); }}
                />
                <path
                  d={getConnectionPath(conn)}
                  fill="none"
                  stroke="transparent"
                  strokeWidth={12}
                  className="pointer-events-auto cursor-pointer"
                  onClick={(e) => { e.stopPropagation(); deleteConnection(conn.id); }}
                />
              </g>
            ))}
          </svg>

          {/* Nodes */}
          {flow.nodes.map((node) => {
            const cfg = nodeTypeConfigs[node.type];
            const Icon = cfg.icon;
            const isSelected = selectedNodeId === node.id;
            const isConnectSource = connectingFrom === node.id;

            return (
              <div
                key={node.id}
                onMouseDown={(e) => handleNodeMouseDown(e, node.id)}
                onClick={(e) => { e.stopPropagation(); setSelectedNodeId(node.id); }}
                className={`absolute select-none rounded-xl border-2 shadow-md transition-shadow ${cfg.color} ${cfg.borderColor} ${isSelected ? 'ring-2 ring-blue-500 ring-offset-2' : ''} ${isConnectSource ? 'ring-2 ring-green-500' : ''} cursor-grab dark:bg-opacity-20`}
                style={{ left: node.x, top: node.y, width: NODE_W }}
              >
                {/* Header */}
                <div className="flex items-center gap-2 border-b border-slate-200/50 px-3 py-2">
                  <Icon size={16} className="shrink-0 text-slate-700 dark:text-slate-300" />
                  <span className="truncate text-sm font-semibold text-slate-800 dark:text-slate-200">{node.label}</span>
                </div>
                {/* Config preview */}
                <div className="px-3 py-1.5">
                  {Object.entries(node.config).filter(([, v]) => v).slice(0, 2).map(([k, v]) => (
                    <div key={k} className="truncate text-xs text-slate-500 dark:text-slate-400">
                      <span className="font-medium">{k}:</span> {String(v).slice(0, 25)}
                    </div>
                  ))}
                  {Object.keys(node.config).filter((k) => node.config[k]).length === 0 && (
                    <div className="text-xs italic text-slate-400">{t('flows.noConfig')}</div>
                  )}
                </div>
                {/* Input connector (left) */}
                <div
                  data-connector="input"
                  onClick={(e) => { e.stopPropagation(); handleInputClick(node.id); }}
                  className="absolute -left-2.5 top-1/2 h-5 w-5 -translate-y-1/2 cursor-crosshair rounded-full border-2 border-slate-400 bg-white transition-colors hover:border-blue-500 hover:bg-blue-50 dark:bg-slate-700"
                />
                {/* Output connector (right) */}
                <div
                  data-connector="output"
                  onClick={(e) => { e.stopPropagation(); handleOutputClick(node.id); }}
                  className="absolute -right-2.5 top-1/2 h-5 w-5 -translate-y-1/2 cursor-crosshair rounded-full border-2 border-slate-400 bg-white transition-colors hover:border-green-500 hover:bg-green-50 dark:bg-slate-700"
                />
              </div>
            );
          })}
        </div>
      </div>

      {/* Right: Config panel */}
      {selectedNode && (
        <div className="w-72 shrink-0 overflow-y-auto border-l border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-800">
          <div className="mb-4 flex items-center justify-between">
            <h3 className="text-sm font-semibold text-slate-700 dark:text-slate-200">{t('flows.nodeConfig')}</h3>
            <button
              onClick={() => deleteNode(selectedNode.id)}
              className="rounded p-1.5 text-red-500 transition-colors hover:bg-red-50 dark:hover:bg-red-900/20"
              title={t('common.delete')}
            >
              <Trash2 size={16} />
            </button>
          </div>

          {/* Label */}
          <label className="mb-3 block">
            <span className="mb-1 block text-xs font-medium text-slate-500 dark:text-slate-400">{t('flows.label')}</span>
            <input
              value={selectedNode.label}
              onChange={(e) => updateNodeLabel(selectedNode.id, e.target.value)}
              className="w-full rounded-lg border border-slate-300 px-3 py-1.5 text-sm dark:border-slate-600 dark:bg-slate-700 dark:text-slate-200"
            />
          </label>

          {/* Type badge */}
          <div className="mb-3">
            <span className={`inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium ${nodeTypeConfigs[selectedNode.type].color} ${nodeTypeConfigs[selectedNode.type].borderColor} border`}>
              {nodeTypeConfigs[selectedNode.type].label}
            </span>
          </div>

          {/* Config fields */}
          {nodeTypeConfigs[selectedNode.type].configFields.map((field) => (
            <label key={field.key} className="mb-3 block">
              <span className="mb-1 block text-xs font-medium text-slate-500 dark:text-slate-400">{field.label}</span>
              {field.type === 'select' ? (
                <select
                  value={selectedNode.config[field.key] ?? ''}
                  onChange={(e) => updateNodeConfig(selectedNode.id, field.key, e.target.value)}
                  className="w-full rounded-lg border border-slate-300 px-3 py-1.5 text-sm dark:border-slate-600 dark:bg-slate-700 dark:text-slate-200"
                >
                  <option value="">-- Select --</option>
                  {field.options?.map((o) => (
                    <option key={o} value={o}>{o}</option>
                  ))}
                </select>
              ) : field.type === 'textarea' ? (
                <textarea
                  value={selectedNode.config[field.key] ?? ''}
                  onChange={(e) => updateNodeConfig(selectedNode.id, field.key, e.target.value)}
                  rows={3}
                  className="w-full rounded-lg border border-slate-300 px-3 py-1.5 text-sm dark:border-slate-600 dark:bg-slate-700 dark:text-slate-200"
                />
              ) : (
                <input
                  type={field.type}
                  value={selectedNode.config[field.key] ?? ''}
                  onChange={(e) => updateNodeConfig(selectedNode.id, field.key, e.target.value)}
                  className="w-full rounded-lg border border-slate-300 px-3 py-1.5 text-sm dark:border-slate-600 dark:bg-slate-700 dark:text-slate-200"
                />
              )}
            </label>
          ))}

          {/* Position info */}
          <div className="mt-4 border-t border-slate-200 pt-3 text-xs text-slate-400 dark:border-slate-700">
            x: {Math.round(selectedNode.x)}, y: {Math.round(selectedNode.y)}
          </div>
        </div>
      )}
    </div>
  );
}
