import { useState, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Save, Play, ArrowLeft, ToggleLeft, ToggleRight, LayoutGrid, List, ChevronDown, ChevronUp } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import type { Flow, FlowRun } from '../api/flows';
import FlowCanvas from './flows/FlowCanvas';
import FlowPipeline from './flows/FlowPipeline';
import FlowRunHistory from './flows/FlowRunHistory';

// Mock data for existing flows
function getMockFlow(id: string): Flow {
  const flows: Record<string, Flow> = {
    'flow-1': {
      id: 'flow-1',
      name: 'New User Onboarding',
      description: 'Send welcome email and create default workspace when a user signs up',
      triggerType: 'user.created',
      isActive: true,
      runCount: 142,
      lastRunAt: new Date(Date.now() - 3600000).toISOString(),
      createdAt: new Date(Date.now() - 86400000 * 30).toISOString(),
      updatedAt: new Date(Date.now() - 3600000).toISOString(),
      nodes: [
        { id: 'n1', type: 'trigger', label: 'User Created', config: { event: 'user.created' }, x: 50, y: 200 },
        { id: 'n2', type: 'condition', label: 'Has Email?', config: { field: 'email', operator: 'exists', value: '' }, x: 300, y: 200 },
        { id: 'n3', type: 'email', label: 'Welcome Email', config: { to: '{{user.email}}', subject: 'Welcome!', template: 'welcome' }, x: 550, y: 150 },
        { id: 'n4', type: 'action', label: 'Create Workspace', config: { action: 'create_record', target: 'workspaces' }, x: 550, y: 300 },
        { id: 'n5', type: 'log', label: 'Log Success', config: { message: 'Onboarding complete', level: 'info' }, x: 800, y: 200 },
      ],
      connections: [
        { id: 'c1', fromNodeId: 'n1', toNodeId: 'n2' },
        { id: 'c2', fromNodeId: 'n2', toNodeId: 'n3', label: 'yes' },
        { id: 'c3', fromNodeId: 'n2', toNodeId: 'n4', label: 'yes' },
        { id: 'c4', fromNodeId: 'n3', toNodeId: 'n5' },
        { id: 'c5', fromNodeId: 'n4', toNodeId: 'n5' },
      ],
    },
    'flow-2': {
      id: 'flow-2',
      name: 'Order Notification Pipeline',
      description: 'Notify team via webhook and send confirmation email when an order is placed',
      triggerType: 'order.placed',
      isActive: true,
      runCount: 89,
      lastRunAt: new Date(Date.now() - 7200000).toISOString(),
      createdAt: new Date(Date.now() - 86400000 * 14).toISOString(),
      updatedAt: new Date(Date.now() - 7200000).toISOString(),
      nodes: [
        { id: 'n1', type: 'trigger', label: 'Order Placed', config: { event: 'order.placed' }, x: 50, y: 200 },
        { id: 'n2', type: 'transform', label: 'Format Data', config: { mapping: '{"orderId": "{{id}}", "amount": "{{total}}"}' }, x: 300, y: 200 },
        { id: 'n3', type: 'webhook', label: 'Notify Slack', config: { url: 'https://hooks.slack.com/...', method: 'POST' }, x: 550, y: 120 },
        { id: 'n4', type: 'email', label: 'Order Confirmation', config: { to: '{{customer.email}}', subject: 'Order confirmed', template: 'invoice' }, x: 550, y: 300 },
      ],
      connections: [
        { id: 'c1', fromNodeId: 'n1', toNodeId: 'n2' },
        { id: 'c2', fromNodeId: 'n2', toNodeId: 'n3' },
        { id: 'c3', fromNodeId: 'n2', toNodeId: 'n4' },
      ],
    },
    'flow-3': {
      id: 'flow-3',
      name: 'Product Sync',
      description: 'Sync product updates to external inventory system',
      triggerType: 'product.updated',
      isActive: false,
      runCount: 34,
      lastRunAt: new Date(Date.now() - 86400000 * 3).toISOString(),
      createdAt: new Date(Date.now() - 86400000 * 60).toISOString(),
      updatedAt: new Date(Date.now() - 86400000 * 3).toISOString(),
      nodes: [
        { id: 'n1', type: 'trigger', label: 'Product Updated', config: { event: 'product.updated' }, x: 50, y: 200 },
        { id: 'n2', type: 'delay', label: 'Debounce', config: { duration: '5' }, x: 300, y: 200 },
        { id: 'n3', type: 'webhook', label: 'Sync to Inventory', config: { url: 'https://inventory.api/sync', method: 'PUT' }, x: 550, y: 200 },
      ],
      connections: [
        { id: 'c1', fromNodeId: 'n1', toNodeId: 'n2' },
        { id: 'c2', fromNodeId: 'n2', toNodeId: 'n3' },
      ],
    },
  };
  return flows[id] || createEmptyFlow();
}

function createEmptyFlow(): Flow {
  return {
    id: `flow-${Date.now()}`,
    name: 'Untitled Flow',
    description: '',
    triggerType: '',
    isActive: false,
    runCount: 0,
    nodes: [],
    connections: [],
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  };
}

function getMockRuns(): FlowRun[] {
  return [
    {
      id: 'run-1',
      flowId: 'flow-1',
      status: 'completed',
      startedAt: new Date(Date.now() - 3600000).toISOString(),
      completedAt: new Date(Date.now() - 3598000).toISOString(),
      steps: [
        { nodeId: 'n1', nodeLabel: 'User Created', status: 'completed', duration: 12, output: '{"userId": "u_123"}' },
        { nodeId: 'n2', nodeLabel: 'Has Email?', status: 'completed', duration: 3, output: 'true' },
        { nodeId: 'n3', nodeLabel: 'Welcome Email', status: 'completed', duration: 450, output: 'Sent to user@example.com' },
        { nodeId: 'n4', nodeLabel: 'Create Workspace', status: 'completed', duration: 120, output: '{"workspaceId": "ws_456"}' },
        { nodeId: 'n5', nodeLabel: 'Log Success', status: 'completed', duration: 5 },
      ],
    },
    {
      id: 'run-2',
      flowId: 'flow-1',
      status: 'failed',
      startedAt: new Date(Date.now() - 7200000).toISOString(),
      completedAt: new Date(Date.now() - 7199000).toISOString(),
      error: 'Email service timeout',
      steps: [
        { nodeId: 'n1', nodeLabel: 'User Created', status: 'completed', duration: 10 },
        { nodeId: 'n2', nodeLabel: 'Has Email?', status: 'completed', duration: 2 },
        { nodeId: 'n3', nodeLabel: 'Welcome Email', status: 'failed', duration: 5000, error: 'SMTP connection timeout after 5000ms' },
        { nodeId: 'n4', nodeLabel: 'Create Workspace', status: 'completed', duration: 115 },
        { nodeId: 'n5', nodeLabel: 'Log Success', status: 'skipped' },
      ],
    },
    {
      id: 'run-3',
      flowId: 'flow-1',
      status: 'completed',
      startedAt: new Date(Date.now() - 14400000).toISOString(),
      completedAt: new Date(Date.now() - 14398500).toISOString(),
      steps: [
        { nodeId: 'n1', nodeLabel: 'User Created', status: 'completed', duration: 15 },
        { nodeId: 'n2', nodeLabel: 'Has Email?', status: 'completed', duration: 4 },
        { nodeId: 'n3', nodeLabel: 'Welcome Email', status: 'completed', duration: 380 },
        { nodeId: 'n4', nodeLabel: 'Create Workspace', status: 'completed', duration: 95 },
        { nodeId: 'n5', nodeLabel: 'Log Success', status: 'completed', duration: 3 },
      ],
    },
  ];
}

export default function FlowEditorPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isNew = !id || id === 'new';

  const [flow, setFlow] = useState<Flow>(() => (isNew ? createEmptyFlow() : getMockFlow(id!)));
  const [viewMode, setViewMode] = useState<'canvas' | 'pipeline'>('canvas');
  const [showHistory, setShowHistory] = useState(false);
  const [runs] = useState<FlowRun[]>(getMockRuns);
  const [saving, setSaving] = useState(false);
  const [running, setRunning] = useState(false);

  const handleFlowChange = useCallback((updated: Flow) => {
    setFlow(updated);
  }, []);

  const handleSave = async () => {
    setSaving(true);
    // Simulate API call
    await new Promise((r) => setTimeout(r, 500));
    setFlow((f) => ({ ...f, updatedAt: new Date().toISOString() }));
    setSaving(false);
  };

  const handleRun = async () => {
    setRunning(true);
    await new Promise((r) => setTimeout(r, 1000));
    setRunning(false);
    setFlow((f) => ({ ...f, runCount: f.runCount + 1, lastRunAt: new Date().toISOString() }));
  };

  const toggleActive = () => {
    setFlow((f) => ({ ...f, isActive: !f.isActive }));
  };

  return (
    <div className="flex h-[calc(100vh-4rem)] flex-col">
      {/* Toolbar */}
      <div className="flex items-center gap-3 border-b border-slate-200 bg-white px-4 py-2.5 dark:border-slate-700 dark:bg-slate-800">
        <button
          onClick={() => navigate('/admin/flows')}
          className="rounded-lg p-1.5 text-slate-500 transition-colors hover:bg-slate-100 dark:hover:bg-slate-700"
        >
          <ArrowLeft size={18} />
        </button>

        {/* Flow name */}
        <input
          value={flow.name}
          onChange={(e) => setFlow((f) => ({ ...f, name: e.target.value }))}
          className="min-w-0 flex-1 rounded-lg border border-transparent bg-transparent px-2 py-1 text-base font-semibold text-slate-800 transition-colors hover:border-slate-300 focus:border-blue-400 focus:outline-none dark:text-slate-200 dark:hover:border-slate-600"
          placeholder={t('flows.flowName')}
        />

        {/* View toggle */}
        <div className="flex rounded-lg border border-slate-200 dark:border-slate-600">
          <button
            onClick={() => setViewMode('canvas')}
            className={`flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium transition-colors ${viewMode === 'canvas' ? 'bg-blue-600 text-white' : 'text-slate-500 hover:bg-slate-50 dark:hover:bg-slate-700'}`}
            style={{ borderRadius: '0.5rem 0 0 0.5rem' }}
          >
            <LayoutGrid size={14} />
            {t('flows.visual')}
          </button>
          <button
            onClick={() => setViewMode('pipeline')}
            className={`flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium transition-colors ${viewMode === 'pipeline' ? 'bg-blue-600 text-white' : 'text-slate-500 hover:bg-slate-50 dark:hover:bg-slate-700'}`}
            style={{ borderRadius: '0 0.5rem 0.5rem 0' }}
          >
            <List size={14} />
            {t('flows.pipeline')}
          </button>
        </div>

        {/* Active toggle */}
        <button
          onClick={toggleActive}
          className={`flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-xs font-medium transition-colors ${flow.isActive ? 'bg-green-50 text-green-700 dark:bg-green-900/20 dark:text-green-400' : 'bg-slate-100 text-slate-500 dark:bg-slate-700 dark:text-slate-400'}`}
        >
          {flow.isActive ? <ToggleRight size={16} className="text-green-500" /> : <ToggleLeft size={16} />}
          {flow.isActive ? t('flows.active') : t('flows.inactive')}
        </button>

        {/* Save */}
        <button
          onClick={handleSave}
          disabled={saving}
          className="flex items-center gap-1.5 rounded-lg bg-blue-600 px-4 py-1.5 text-sm font-medium text-white shadow-sm transition-colors hover:bg-blue-700 disabled:opacity-50"
        >
          <Save size={14} />
          {saving ? t('common.loading') : t('common.save')}
        </button>

        {/* Run */}
        <button
          onClick={handleRun}
          disabled={running || flow.nodes.length === 0}
          className="flex items-center gap-1.5 rounded-lg bg-emerald-600 px-4 py-1.5 text-sm font-medium text-white shadow-sm transition-colors hover:bg-emerald-700 disabled:opacity-50"
        >
          <Play size={14} />
          {running ? t('flows.running') : t('flows.run')}
        </button>

        {/* History toggle */}
        <button
          onClick={() => setShowHistory((h) => !h)}
          className="flex items-center gap-1 rounded-lg border border-slate-200 px-3 py-1.5 text-xs font-medium text-slate-500 transition-colors hover:bg-slate-50 dark:border-slate-600 dark:hover:bg-slate-700"
        >
          {t('flows.history')}
          {showHistory ? <ChevronDown size={12} /> : <ChevronUp size={12} />}
        </button>
      </div>

      {/* Editor area */}
      <div className="flex flex-1 overflow-hidden">
        <div className="flex flex-1 flex-col overflow-hidden">
          {viewMode === 'canvas' ? (
            <FlowCanvas flow={flow} onChange={handleFlowChange} />
          ) : (
            <div className="flex-1 overflow-auto">
              <FlowPipeline flow={flow} onChange={handleFlowChange} />
            </div>
          )}
        </div>

        {/* Run history sidebar */}
        {showHistory && (
          <div className="w-80 shrink-0 overflow-y-auto border-l border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-800">
            <FlowRunHistory runs={runs} />
          </div>
        )}
      </div>
    </div>
  );
}
