import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, Search, Zap, ToggleLeft, ToggleRight, Trash2, Clock, Hash } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import type { Flow } from '../../api/flows';

// Mock flows for demo
function createMockFlows(): Flow[] {
  return [
    {
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
    {
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
    {
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
  ];
}

export default function FlowListPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [flows, setFlows] = useState<Flow[]>(createMockFlows);
  const [search, setSearch] = useState('');
  const [deleteConfirmId, setDeleteConfirmId] = useState<string | null>(null);

  const filteredFlows = flows.filter((f) =>
    f.name.toLowerCase().includes(search.toLowerCase()),
  );

  const toggleFlow = (id: string) => {
    setFlows((prev) =>
      prev.map((f) => (f.id === id ? { ...f, isActive: !f.isActive } : f)),
    );
  };

  const deleteFlow = (id: string) => {
    setFlows((prev) => prev.filter((f) => f.id !== id));
    setDeleteConfirmId(null);
  };

  function timeAgo(iso?: string) {
    if (!iso) return '-';
    const diff = Date.now() - new Date(iso).getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 60) return `${mins}m ago`;
    const hrs = Math.floor(mins / 60);
    if (hrs < 24) return `${hrs}h ago`;
    return `${Math.floor(hrs / 24)}d ago`;
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-800 dark:text-slate-100">{t('flows.title')}</h1>
          <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">{t('flows.subtitle')}</p>
        </div>
        <button
          onClick={() => navigate('/admin/flows/new')}
          className="flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2.5 text-sm font-medium text-white shadow-sm transition-colors hover:bg-blue-700"
        >
          <Plus size={16} />
          {t('flows.newFlow')}
        </button>
      </div>

      {/* Search */}
      <div className="relative max-w-md">
        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder={t('flows.searchFlows')}
          className="w-full rounded-lg border border-slate-300 py-2 pl-9 pr-3 text-sm dark:border-slate-600 dark:bg-slate-800 dark:text-slate-200"
        />
      </div>

      {/* Grid */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
        {filteredFlows.map((flow) => (
          <div
            key={flow.id}
            className="group relative rounded-xl border border-slate-200 bg-white p-5 shadow-sm transition-shadow hover:shadow-md dark:border-slate-700 dark:bg-slate-800"
          >
            {/* Active badge */}
            <div className="mb-3 flex items-center justify-between">
              <span
                className={`inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium ${
                  flow.isActive
                    ? 'bg-green-50 text-green-700 dark:bg-green-900/20 dark:text-green-400'
                    : 'bg-slate-100 text-slate-500 dark:bg-slate-700 dark:text-slate-400'
                }`}
              >
                <span className={`h-1.5 w-1.5 rounded-full ${flow.isActive ? 'bg-green-500' : 'bg-slate-400'}`} />
                {flow.isActive ? t('flows.active') : t('flows.inactive')}
              </span>
              <span className="rounded-full bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-600 dark:bg-slate-700 dark:text-slate-400">
                {flow.triggerType}
              </span>
            </div>

            {/* Name & description */}
            <h3
              onClick={() => navigate(`/admin/flows/${flow.id}`)}
              className="cursor-pointer text-base font-semibold text-slate-800 transition-colors hover:text-blue-600 dark:text-slate-200 dark:hover:text-blue-400"
            >
              {flow.name}
            </h3>
            {flow.description && (
              <p className="mt-1 line-clamp-2 text-sm text-slate-500 dark:text-slate-400">{flow.description}</p>
            )}

            {/* Stats */}
            <div className="mt-4 flex items-center gap-4 text-xs text-slate-500 dark:text-slate-400">
              <span className="flex items-center gap-1">
                <Zap size={12} />
                {flow.nodes.length} {t('flows.nodes')}
              </span>
              <span className="flex items-center gap-1">
                <Hash size={12} />
                {flow.runCount} {t('flows.runs')}
              </span>
              <span className="flex items-center gap-1">
                <Clock size={12} />
                {timeAgo(flow.lastRunAt)}
              </span>
            </div>

            {/* Actions */}
            <div className="mt-4 flex items-center gap-2 border-t border-slate-100 pt-3 dark:border-slate-700">
              <button
                onClick={() => toggleFlow(flow.id)}
                className="flex items-center gap-1 text-xs text-slate-500 transition-colors hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-300"
              >
                {flow.isActive ? <ToggleRight size={16} className="text-green-500" /> : <ToggleLeft size={16} />}
                {flow.isActive ? t('flows.deactivate') : t('flows.activate')}
              </button>
              <div className="flex-1" />
              {deleteConfirmId === flow.id ? (
                <div className="flex items-center gap-2">
                  <span className="text-xs text-red-500">{t('flows.confirmDelete')}</span>
                  <button onClick={() => deleteFlow(flow.id)} className="rounded bg-red-600 px-2 py-1 text-xs text-white hover:bg-red-700">
                    {t('common.yes')}
                  </button>
                  <button onClick={() => setDeleteConfirmId(null)} className="rounded bg-slate-200 px-2 py-1 text-xs dark:bg-slate-600 dark:text-slate-200">
                    {t('common.no')}
                  </button>
                </div>
              ) : (
                <button
                  onClick={() => setDeleteConfirmId(flow.id)}
                  className="flex items-center gap-1 text-xs text-red-400 transition-colors hover:text-red-600"
                >
                  <Trash2 size={14} />
                  {t('common.delete')}
                </button>
              )}
            </div>
          </div>
        ))}
      </div>

      {filteredFlows.length === 0 && (
        <div className="py-12 text-center text-sm text-slate-400">
          {t('flows.noFlows')}
        </div>
      )}
    </div>
  );
}
