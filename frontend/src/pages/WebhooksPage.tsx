import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  webhooksApi,
  type WebhookSubscription,
  type IncomingWebhookConfig,
  type WebhookDeliveryLog,
  type WebhookRule,
  type WebhookCondition,
} from '../api/webhooks';
import { useApi } from '../hooks/useApi';
import Badge from '../components/Badge';
import TableSkeleton from '../components/TableSkeleton';
import {
  Trash2,
  Plus,
  X,
  ChevronDown,
  ChevronUp,
  RotateCcw,
  Check,
  AlertCircle,
  Play,
  Pencil,
  Zap,
} from 'lucide-react';

type TabId = 'outgoing' | 'incoming' | 'builder';

// ─── Outgoing Tab ────────────────────────────────────────────────────────────

function OutgoingTab() {
  const { t } = useTranslation();
  const { data: webhooks, loading, refetch } = useApi<WebhookSubscription[]>(() => webhooksApi.getAll());
  const [deleting, setDeleting] = useState<string | null>(null);
  const [toggling, setToggling] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [formData, setFormData] = useState({ eventType: '', callbackUrl: '', secret: '' });
  const [submitting, setSubmitting] = useState(false);

  const handleDelete = async (id: string) => {
    if (!confirm(t('webhooks.deleteConfirm'))) return;
    setDeleting(id);
    try {
      await webhooksApi.delete(id);
      await refetch();
    } finally {
      setDeleting(null);
    }
  };

  const handleToggle = async (id: string, isActive: boolean) => {
    setToggling(id);
    try {
      await webhooksApi.toggle(id, !isActive);
      await refetch();
    } finally {
      setToggling(null);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      await webhooksApi.create({
        eventType: formData.eventType,
        callbackUrl: formData.callbackUrl,
        secret: formData.secret || null,
      });
      setFormData({ eventType: '', callbackUrl: '', secret: '' });
      setShowForm(false);
      await refetch();
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <TableSkeleton columns={5} />;

  return (
    <div>
      <div className="mb-4 flex items-center justify-end">
        {!showForm && (
          <button
            onClick={() => setShowForm(true)}
            className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700"
          >
            <Plus size={16} /> {t('webhooks.addOutgoing')}
          </button>
        )}
      </div>

      {showForm && (
        <form
          onSubmit={(e) => void handleSubmit(e)}
          className="mb-6 rounded-xl border border-slate-200 bg-white p-4 shadow-sm"
        >
          <div className="mb-4 flex items-center justify-between">
            <h2 className="text-sm font-semibold text-slate-700">{t('webhooks.newOutgoing')}</h2>
            <button
              type="button"
              onClick={() => setShowForm(false)}
              className="rounded p-1 text-slate-400 transition hover:text-slate-600"
            >
              <X size={16} />
            </button>
          </div>
          <div className="grid gap-4 sm:grid-cols-3">
            <div>
              <label htmlFor="eventType" className="mb-1 block text-xs font-medium text-slate-600">
                {t('webhooks.eventType')}
              </label>
              <input
                id="eventType"
                type="text"
                required
                value={formData.eventType}
                onChange={(e) => setFormData((f) => ({ ...f, eventType: e.target.value }))}
                placeholder="e.g. order.created"
                className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
              />
            </div>
            <div>
              <label htmlFor="callbackUrl" className="mb-1 block text-xs font-medium text-slate-600">
                {t('webhooks.callbackUrl')}
              </label>
              <input
                id="callbackUrl"
                type="url"
                required
                value={formData.callbackUrl}
                onChange={(e) => setFormData((f) => ({ ...f, callbackUrl: e.target.value }))}
                placeholder="https://example.com/webhook"
                className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
              />
            </div>
            <div>
              <label htmlFor="secret" className="mb-1 block text-xs font-medium text-slate-600">
                {t('webhooks.secretOptional')}
              </label>
              <input
                id="secret"
                type="text"
                value={formData.secret}
                onChange={(e) => setFormData((f) => ({ ...f, secret: e.target.value }))}
                placeholder="whsec_..."
                className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
              />
            </div>
          </div>
          <div className="mt-4 flex gap-2">
            <button
              type="submit"
              disabled={submitting}
              className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700 disabled:opacity-50"
            >
              {submitting ? t('webhooks.adding') : t('webhooks.add')}
            </button>
            <button
              type="button"
              onClick={() => setShowForm(false)}
              className="rounded-lg border border-slate-300 px-4 py-2 text-sm text-slate-600 transition hover:bg-slate-100"
            >
              {t('common.cancel')}
            </button>
          </div>
        </form>
      )}

      <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
        <table className="w-full text-sm">
          <thead className="bg-slate-50">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">{t('webhooks.eventType')}</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">{t('webhooks.callbackUrl')}</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">{t('webhooks.active')}</th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">{t('webhooks.lastTriggered')}</th>
              <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">{t('common.actions')}</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {(webhooks ?? []).map((w) => (
              <tr key={w.id} className="transition-colors hover:bg-slate-50">
                <td className="px-4 py-3">
                  <Badge variant="info">{w.eventType}</Badge>
                </td>
                <td className="max-w-[250px] truncate px-4 py-3 text-slate-600" title={w.callbackUrl}>
                  <code className="rounded bg-slate-100 px-1.5 py-0.5 text-xs text-slate-600">{w.callbackUrl}</code>
                </td>
                <td className="px-4 py-3">
                  <button
                    onClick={() => void handleToggle(w.id, w.isActive)}
                    disabled={toggling === w.id}
                    className={`relative inline-flex h-5 w-9 shrink-0 cursor-pointer items-center rounded-full transition-colors ${
                      w.isActive ? 'bg-blue-600' : 'bg-slate-300'
                    } ${toggling === w.id ? 'opacity-50' : ''}`}
                    role="switch"
                    aria-checked={w.isActive}
                    aria-label={`Toggle webhook ${w.eventType}`}
                  >
                    <span
                      className={`inline-block h-3.5 w-3.5 transform rounded-full bg-white shadow transition-transform ${
                        w.isActive ? 'translate-x-[18px]' : 'translate-x-[3px]'
                      }`}
                    />
                  </button>
                </td>
                <td className="whitespace-nowrap px-4 py-3 text-slate-500">
                  {w.lastTriggeredAt
                    ? new Date(w.lastTriggeredAt).toLocaleString('fr-FR')
                    : <span className="text-slate-400">-</span>}
                </td>
                <td className="px-4 py-3 text-right">
                  <button
                    onClick={() => void handleDelete(w.id)}
                    disabled={deleting === w.id}
                    className={`rounded p-1.5 text-slate-400 transition hover:bg-red-50 hover:text-red-600 ${
                      deleting === w.id ? 'opacity-50 cursor-not-allowed' : ''
                    }`}
                    title={t('common.delete')}
                  >
                    <Trash2 size={15} />
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {(webhooks ?? []).length === 0 && (
          <div className="py-12 text-center text-slate-400">{t('webhooks.noOutgoing')}</div>
        )}
      </div>
    </div>
  );
}

// ─── Incoming Tab ────────────────────────────────────────────────────────────

function IncomingTab() {
  const { t } = useTranslation();
  const { data: configs, loading: loadingConfigs, refetch: refetchConfigs } = useApi<IncomingWebhookConfig[]>(() => webhooksApi.getIncomingConfigs());
  const { data: logs, loading: loadingLogs, refetch: refetchLogs } = useApi<WebhookDeliveryLog[]>(() => webhooksApi.getIncomingLogs());

  const [showForm, setShowForm] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [formData, setFormData] = useState({
    name: '',
    source: '',
    signatureHeader: '',
    secret: '',
    signatureAlgorithm: 'hmac-sha256',
  });
  const [expandedLogId, setExpandedLogId] = useState<string | null>(null);
  const [replaying, setReplaying] = useState<string | null>(null);
  const [deleting, setDeleting] = useState<string | null>(null);

  const handleCreateConfig = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      await webhooksApi.createIncomingConfig({
        name: formData.name,
        source: formData.source,
        signatureHeader: formData.signatureHeader || null,
        secret: formData.secret || null,
        signatureAlgorithm: formData.signatureAlgorithm,
      });
      setFormData({ name: '', source: '', signatureHeader: '', secret: '', signatureAlgorithm: 'hmac-sha256' });
      setShowForm(false);
      await refetchConfigs();
    } finally {
      setSubmitting(false);
    }
  };

  const handleDeleteConfig = async (id: string) => {
    if (!confirm(t('webhooks.deleteConfirm'))) return;
    setDeleting(id);
    try {
      await webhooksApi.deleteIncomingConfig(id);
      await refetchConfigs();
    } finally {
      setDeleting(null);
    }
  };

  const handleReplay = async (id: string) => {
    setReplaying(id);
    try {
      await webhooksApi.replayLog(id);
      await refetchLogs();
    } finally {
      setReplaying(null);
    }
  };

  const statusBadgeVariant = (status: WebhookDeliveryLog['status']): 'info' | 'success' | 'danger' | 'warning' => {
    switch (status) {
      case 'received': return 'info';
      case 'processing': return 'warning';
      case 'processed': return 'success';
      case 'failed': return 'danger';
    }
  };

  const tryPrettyJson = (str: string): string => {
    try {
      return JSON.stringify(JSON.parse(str), null, 2);
    } catch {
      return str;
    }
  };

  return (
    <div className="space-y-6">
      {/* Configs section */}
      <div>
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-lg font-semibold text-slate-800">{t('webhooks.incomingConfigs')}</h2>
          {!showForm && (
            <button
              onClick={() => setShowForm(true)}
              className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700"
            >
              <Plus size={16} /> {t('webhooks.addConfig')}
            </button>
          )}
        </div>

        {showForm && (
          <form
            onSubmit={(e) => void handleCreateConfig(e)}
            className="mb-6 rounded-xl border border-slate-200 bg-white p-4 shadow-sm"
          >
            <div className="mb-4 flex items-center justify-between">
              <h3 className="text-sm font-semibold text-slate-700">{t('webhooks.newConfig')}</h3>
              <button
                type="button"
                onClick={() => setShowForm(false)}
                className="rounded p-1 text-slate-400 transition hover:text-slate-600"
              >
                <X size={16} />
              </button>
            </div>
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              <div>
                <label className="mb-1 block text-xs font-medium text-slate-600">{t('common.name')}</label>
                <input
                  type="text"
                  required
                  value={formData.name}
                  onChange={(e) => setFormData((f) => ({ ...f, name: e.target.value }))}
                  placeholder="e.g. Stripe Webhooks"
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                />
              </div>
              <div>
                <label className="mb-1 block text-xs font-medium text-slate-600">{t('webhooks.source')}</label>
                <input
                  type="text"
                  required
                  value={formData.source}
                  onChange={(e) => setFormData((f) => ({ ...f, source: e.target.value }))}
                  placeholder="e.g. stripe"
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                />
              </div>
              <div>
                <label className="mb-1 block text-xs font-medium text-slate-600">{t('webhooks.signatureHeader')}</label>
                <input
                  type="text"
                  value={formData.signatureHeader}
                  onChange={(e) => setFormData((f) => ({ ...f, signatureHeader: e.target.value }))}
                  placeholder="e.g. Stripe-Signature"
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                />
              </div>
              <div>
                <label className="mb-1 block text-xs font-medium text-slate-600">{t('webhooks.secretOptional')}</label>
                <input
                  type="text"
                  value={formData.secret}
                  onChange={(e) => setFormData((f) => ({ ...f, secret: e.target.value }))}
                  placeholder="whsec_..."
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                />
              </div>
              <div>
                <label className="mb-1 block text-xs font-medium text-slate-600">{t('webhooks.algorithm')}</label>
                <select
                  value={formData.signatureAlgorithm}
                  onChange={(e) => setFormData((f) => ({ ...f, signatureAlgorithm: e.target.value }))}
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                >
                  <option value="hmac-sha256">HMAC-SHA256</option>
                  <option value="hmac-sha1">HMAC-SHA1</option>
                  <option value="hmac-sha512">HMAC-SHA512</option>
                </select>
              </div>
            </div>
            <div className="mt-4 flex gap-2">
              <button
                type="submit"
                disabled={submitting}
                className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700 disabled:opacity-50"
              >
                {submitting ? t('webhooks.adding') : t('webhooks.add')}
              </button>
              <button
                type="button"
                onClick={() => setShowForm(false)}
                className="rounded-lg border border-slate-300 px-4 py-2 text-sm text-slate-600 transition hover:bg-slate-100"
              >
                {t('common.cancel')}
              </button>
            </div>
          </form>
        )}

        {loadingConfigs ? (
          <TableSkeleton columns={5} rows={3} />
        ) : (
          <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
            <table className="w-full text-sm">
              <thead className="bg-slate-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">{t('common.name')}</th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">{t('webhooks.source')}</th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">{t('webhooks.algorithm')}</th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">{t('webhooks.active')}</th>
                  <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">{t('common.actions')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {(configs ?? []).map((c) => (
                  <tr key={c.id} className="transition-colors hover:bg-slate-50">
                    <td className="px-4 py-3 font-medium text-slate-800">{c.name}</td>
                    <td className="px-4 py-3">
                      <Badge variant="neutral">{c.source}</Badge>
                    </td>
                    <td className="px-4 py-3 text-slate-600">
                      <code className="rounded bg-slate-100 px-1.5 py-0.5 text-xs">{c.signatureAlgorithm}</code>
                    </td>
                    <td className="px-4 py-3">
                      <Badge variant={c.isActive ? 'success' : 'neutral'}>
                        {c.isActive ? t('webhooks.active') : t('webhooks.inactive')}
                      </Badge>
                    </td>
                    <td className="px-4 py-3 text-right">
                      <button
                        onClick={() => void handleDeleteConfig(c.id)}
                        disabled={deleting === c.id}
                        className={`rounded p-1.5 text-slate-400 transition hover:bg-red-50 hover:text-red-600 ${
                          deleting === c.id ? 'opacity-50 cursor-not-allowed' : ''
                        }`}
                        title={t('common.delete')}
                      >
                        <Trash2 size={15} />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {(configs ?? []).length === 0 && (
              <div className="py-12 text-center text-slate-400">{t('webhooks.noConfigs')}</div>
            )}
          </div>
        )}
      </div>

      {/* Delivery logs section */}
      <div>
        <h2 className="mb-4 text-lg font-semibold text-slate-800">{t('webhooks.deliveryLogs')}</h2>
        {loadingLogs ? (
          <TableSkeleton columns={5} rows={4} />
        ) : (
          <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
            <table className="w-full text-sm">
              <thead className="bg-slate-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">{t('webhooks.time')}</th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">{t('webhooks.source')}</th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">{t('webhooks.eventType')}</th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">{t('common.status')}</th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">{t('webhooks.signature')}</th>
                  <th className="w-8 px-4 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {(logs ?? []).map((log) => (
                  <LogRow
                    key={log.id}
                    log={log}
                    expanded={expandedLogId === log.id}
                    onToggle={() => setExpandedLogId(expandedLogId === log.id ? null : log.id)}
                    onReplay={() => void handleReplay(log.id)}
                    replaying={replaying === log.id}
                    tryPrettyJson={tryPrettyJson}
                    statusBadgeVariant={statusBadgeVariant}
                  />
                ))}
              </tbody>
            </table>
            {(logs ?? []).length === 0 && (
              <div className="py-12 text-center text-slate-400">{t('webhooks.noLogs')}</div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

function LogRow({
  log,
  expanded,
  onToggle,
  onReplay,
  replaying,
  tryPrettyJson,
  statusBadgeVariant,
}: {
  log: WebhookDeliveryLog;
  expanded: boolean;
  onToggle: () => void;
  onReplay: () => void;
  replaying: boolean;
  tryPrettyJson: (s: string) => string;
  statusBadgeVariant: (s: WebhookDeliveryLog['status']) => 'info' | 'success' | 'danger' | 'warning';
}) {
  const { t } = useTranslation();

  return (
    <>
      <tr
        className="cursor-pointer transition-colors hover:bg-slate-50"
        onClick={onToggle}
      >
        <td className="whitespace-nowrap px-4 py-3 text-slate-500">
          {new Date(log.receivedAt).toLocaleString('fr-FR')}
        </td>
        <td className="px-4 py-3">
          <Badge variant="neutral">{log.source}</Badge>
        </td>
        <td className="px-4 py-3 text-slate-600">
          {log.eventType ? <Badge variant="info">{log.eventType}</Badge> : <span className="text-slate-400">-</span>}
        </td>
        <td className="px-4 py-3">
          <Badge variant={statusBadgeVariant(log.status)}>{log.status}</Badge>
        </td>
        <td className="px-4 py-3">
          {log.signatureValid ? (
            <span className="inline-flex items-center gap-1 text-emerald-600">
              <Check size={14} /> {t('webhooks.valid')}
            </span>
          ) : (
            <span className="inline-flex items-center gap-1 text-red-500">
              <AlertCircle size={14} /> {t('webhooks.invalid')}
            </span>
          )}
        </td>
        <td className="px-4 py-3">
          {expanded ? <ChevronUp size={16} className="text-slate-400" /> : <ChevronDown size={16} className="text-slate-400" />}
        </td>
      </tr>
      {expanded && (
        <tr>
          <td colSpan={6} className="bg-slate-50 px-4 py-4">
            <div className="space-y-4">
              <div>
                <h4 className="mb-1 text-xs font-semibold uppercase tracking-wider text-slate-500">{t('webhooks.headers')}</h4>
                <pre className="max-h-48 overflow-auto rounded-lg bg-slate-900 p-3 text-xs text-emerald-300">
                  {tryPrettyJson(log.headers)}
                </pre>
              </div>
              <div>
                <h4 className="mb-1 text-xs font-semibold uppercase tracking-wider text-slate-500">{t('webhooks.payload')}</h4>
                <pre className="max-h-64 overflow-auto rounded-lg bg-slate-900 p-3 text-xs text-sky-300">
                  {tryPrettyJson(log.payload)}
                </pre>
              </div>
              {log.error && (
                <div>
                  <h4 className="mb-1 text-xs font-semibold uppercase tracking-wider text-red-500">{t('webhooks.error')}</h4>
                  <pre className="rounded-lg bg-red-50 p-3 text-xs text-red-700">{log.error}</pre>
                </div>
              )}
              <button
                onClick={(e) => { e.stopPropagation(); onReplay(); }}
                disabled={replaying}
                className="inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-3 py-1.5 text-sm font-medium text-slate-700 transition hover:bg-slate-100 disabled:opacity-50"
              >
                <RotateCcw size={14} className={replaying ? 'animate-spin' : ''} />
                {replaying ? t('webhooks.replaying') : t('webhooks.replay')}
              </button>
            </div>
          </td>
        </tr>
      )}
    </>
  );
}

// ─── Builder Tab ─────────────────────────────────────────────────────────────

const TRIGGER_EVENTS = [
  'product.created',
  'product.updated',
  'product.deleted',
  'user.created',
  'user.updated',
  'user.deleted',
  'tenant.created',
  'tenant.updated',
  'order.placed',
  'order.updated',
  'order.cancelled',
];

const CONDITION_OPERATORS: WebhookCondition['operator'][] = ['equals', 'contains', 'gt', 'lt', 'exists'];
const HTTP_METHODS = ['POST', 'PUT', 'PATCH'];

const emptyCondition = (): WebhookCondition => ({ field: '', operator: 'equals', value: '' });

interface RuleFormData {
  name: string;
  triggerEvent: string;
  conditions: WebhookCondition[];
  targetUrl: string;
  method: string;
  headers: { key: string; value: string }[];
  payloadTemplate: string;
  isActive: boolean;
}

const defaultRuleForm = (): RuleFormData => ({
  name: '',
  triggerEvent: TRIGGER_EVENTS[0] ?? 'product.created',
  conditions: [],
  targetUrl: '',
  method: 'POST',
  headers: [],
  payloadTemplate: '{\n  "event": "{{event.type}}",\n  "id": "{{event.id}}",\n  "data": {{event.data}}\n}',
  isActive: true,
});

function BuilderTab() {
  const { t } = useTranslation();
  const { data: rules, loading, refetch } = useApi<WebhookRule[]>(() => webhooksApi.getRules());
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formData, setFormData] = useState<RuleFormData>(defaultRuleForm());
  const [submitting, setSubmitting] = useState(false);
  const [deleting, setDeleting] = useState<string | null>(null);
  const [testing, setTesting] = useState<string | null>(null);
  const [testResult, setTestResult] = useState<{ ruleId: string; data: unknown } | null>(null);

  const openNewForm = () => {
    setEditingId(null);
    setFormData(defaultRuleForm());
    setShowForm(true);
  };

  const openEditForm = (rule: WebhookRule) => {
    setEditingId(rule.id);
    setFormData({
      name: rule.name,
      triggerEvent: rule.triggerEvent,
      conditions: rule.conditions.length > 0 ? rule.conditions : [],
      targetUrl: rule.targetUrl,
      method: rule.method,
      headers: Object.entries(rule.headers).map(([key, value]) => ({ key, value })),
      payloadTemplate: rule.payloadTemplate,
      isActive: rule.isActive,
    });
    setShowForm(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      const headersObj: Record<string, string> = {};
      formData.headers.forEach((h) => {
        if (h.key.trim()) headersObj[h.key.trim()] = h.value;
      });

      const payload: Partial<WebhookRule> = {
        name: formData.name,
        triggerEvent: formData.triggerEvent,
        conditions: formData.conditions.filter((c) => c.field.trim() !== ''),
        targetUrl: formData.targetUrl,
        method: formData.method,
        headers: headersObj,
        payloadTemplate: formData.payloadTemplate,
        isActive: formData.isActive,
      };

      if (editingId) {
        await webhooksApi.updateRule(editingId, payload);
      } else {
        await webhooksApi.createRule(payload);
      }
      setShowForm(false);
      setEditingId(null);
      setFormData(defaultRuleForm());
      await refetch();
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm(t('webhooks.deleteConfirm'))) return;
    setDeleting(id);
    try {
      await webhooksApi.deleteRule(id);
      await refetch();
    } finally {
      setDeleting(null);
    }
  };

  const handleTest = async (id: string) => {
    setTesting(id);
    setTestResult(null);
    try {
      const res = await webhooksApi.testRule(id);
      setTestResult({ ruleId: id, data: res.data });
    } catch (err) {
      setTestResult({ ruleId: id, data: { error: (err as Error).message } });
    } finally {
      setTesting(null);
    }
  };

  // Condition helpers
  const addCondition = () => {
    setFormData((f) => ({ ...f, conditions: [...f.conditions, emptyCondition()] }));
  };
  const removeCondition = (idx: number) => {
    setFormData((f) => ({ ...f, conditions: f.conditions.filter((_, i) => i !== idx) }));
  };
  const updateCondition = (idx: number, field: keyof WebhookCondition, val: string) => {
    setFormData((f) => ({
      ...f,
      conditions: f.conditions.map((c, i) => (i === idx ? { ...c, [field]: val } : c)),
    }));
  };

  // Header helpers
  const addHeader = () => {
    setFormData((f) => ({ ...f, headers: [...f.headers, { key: '', value: '' }] }));
  };
  const removeHeader = (idx: number) => {
    setFormData((f) => ({ ...f, headers: f.headers.filter((_, i) => i !== idx) }));
  };
  const updateHeader = (idx: number, field: 'key' | 'value', val: string) => {
    setFormData((f) => ({
      ...f,
      headers: f.headers.map((h, i) => (i === idx ? { ...h, [field]: val } : h)),
    }));
  };

  if (loading) return <TableSkeleton columns={4} rows={3} />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-end">
        {!showForm && (
          <button
            onClick={openNewForm}
            className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700"
          >
            <Plus size={16} /> {t('webhooks.newRule')}
          </button>
        )}
      </div>

      {/* Rule builder form */}
      {showForm && (
        <form
          onSubmit={(e) => void handleSubmit(e)}
          className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm"
        >
          <div className="mb-5 flex items-center justify-between">
            <h2 className="text-base font-semibold text-slate-800">
              {editingId ? t('webhooks.editRule') : t('webhooks.newRule')}
            </h2>
            <button
              type="button"
              onClick={() => { setShowForm(false); setEditingId(null); }}
              className="rounded p-1 text-slate-400 transition hover:text-slate-600"
            >
              <X size={16} />
            </button>
          </div>

          <div className="space-y-5">
            {/* Name + Trigger */}
            <div className="grid gap-4 sm:grid-cols-2">
              <div>
                <label className="mb-1 block text-xs font-medium text-slate-600">{t('webhooks.ruleName')}</label>
                <input
                  type="text"
                  required
                  value={formData.name}
                  onChange={(e) => setFormData((f) => ({ ...f, name: e.target.value }))}
                  placeholder={t('webhooks.ruleNamePlaceholder')}
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                />
              </div>
              <div>
                <label className="mb-1 block text-xs font-medium text-slate-600">{t('webhooks.triggerEvent')}</label>
                <select
                  value={formData.triggerEvent}
                  onChange={(e) => setFormData((f) => ({ ...f, triggerEvent: e.target.value }))}
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                >
                  {TRIGGER_EVENTS.map((evt) => (
                    <option key={evt} value={evt}>{evt}</option>
                  ))}
                </select>
              </div>
            </div>

            {/* Conditions builder */}
            <div>
              <div className="mb-2 flex items-center justify-between">
                <label className="text-xs font-medium text-slate-600">{t('webhooks.conditions')}</label>
                <button
                  type="button"
                  onClick={addCondition}
                  className="inline-flex items-center gap-1 text-xs font-medium text-blue-600 transition hover:text-blue-700"
                >
                  <Plus size={12} /> {t('webhooks.addCondition')}
                </button>
              </div>
              {formData.conditions.length === 0 && (
                <p className="text-xs text-slate-400 italic">{t('webhooks.noConditions')}</p>
              )}
              <div className="space-y-2">
                {formData.conditions.map((cond, idx) => (
                  <div key={idx} className="flex items-center gap-2">
                    <span className="text-xs font-medium text-slate-500">{t('webhooks.when')}</span>
                    <input
                      type="text"
                      value={cond.field}
                      onChange={(e) => updateCondition(idx, 'field', e.target.value)}
                      placeholder="e.g. data.price"
                      className="w-36 rounded-lg border border-slate-300 px-2 py-1.5 text-xs focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                    />
                    <select
                      value={cond.operator}
                      onChange={(e) => updateCondition(idx, 'operator', e.target.value)}
                      className="rounded-lg border border-slate-300 px-2 py-1.5 text-xs focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                    >
                      {CONDITION_OPERATORS.map((op) => (
                        <option key={op} value={op}>{op}</option>
                      ))}
                    </select>
                    {cond.operator !== 'exists' && (
                      <input
                        type="text"
                        value={cond.value}
                        onChange={(e) => updateCondition(idx, 'value', e.target.value)}
                        placeholder={t('webhooks.valuePlaceholder')}
                        className="w-36 rounded-lg border border-slate-300 px-2 py-1.5 text-xs focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                      />
                    )}
                    <button
                      type="button"
                      onClick={() => removeCondition(idx)}
                      className="rounded p-1 text-slate-400 transition hover:text-red-500"
                    >
                      <X size={14} />
                    </button>
                  </div>
                ))}
              </div>
            </div>

            {/* Target URL + Method */}
            <div className="grid gap-4 sm:grid-cols-3">
              <div className="sm:col-span-2">
                <label className="mb-1 block text-xs font-medium text-slate-600">{t('webhooks.targetUrl')}</label>
                <input
                  type="url"
                  required
                  value={formData.targetUrl}
                  onChange={(e) => setFormData((f) => ({ ...f, targetUrl: e.target.value }))}
                  placeholder="https://api.example.com/webhook"
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                />
              </div>
              <div>
                <label className="mb-1 block text-xs font-medium text-slate-600">{t('webhooks.httpMethod')}</label>
                <select
                  value={formData.method}
                  onChange={(e) => setFormData((f) => ({ ...f, method: e.target.value }))}
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                >
                  {HTTP_METHODS.map((m) => (
                    <option key={m} value={m}>{m}</option>
                  ))}
                </select>
              </div>
            </div>

            {/* Custom headers */}
            <div>
              <div className="mb-2 flex items-center justify-between">
                <label className="text-xs font-medium text-slate-600">{t('webhooks.customHeaders')}</label>
                <button
                  type="button"
                  onClick={addHeader}
                  className="inline-flex items-center gap-1 text-xs font-medium text-blue-600 transition hover:text-blue-700"
                >
                  <Plus size={12} /> {t('webhooks.addHeader')}
                </button>
              </div>
              {formData.headers.length === 0 && (
                <p className="text-xs text-slate-400 italic">{t('webhooks.noHeaders')}</p>
              )}
              <div className="space-y-2">
                {formData.headers.map((h, idx) => (
                  <div key={idx} className="flex items-center gap-2">
                    <input
                      type="text"
                      value={h.key}
                      onChange={(e) => updateHeader(idx, 'key', e.target.value)}
                      placeholder="Header-Name"
                      className="w-44 rounded-lg border border-slate-300 px-2 py-1.5 text-xs focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                    />
                    <span className="text-slate-400">:</span>
                    <input
                      type="text"
                      value={h.value}
                      onChange={(e) => updateHeader(idx, 'value', e.target.value)}
                      placeholder="value"
                      className="flex-1 rounded-lg border border-slate-300 px-2 py-1.5 text-xs focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                    />
                    <button
                      type="button"
                      onClick={() => removeHeader(idx)}
                      className="rounded p-1 text-slate-400 transition hover:text-red-500"
                    >
                      <X size={14} />
                    </button>
                  </div>
                ))}
              </div>
            </div>

            {/* Payload template */}
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-600">{t('webhooks.payloadTemplate')}</label>
              <p className="mb-2 text-xs text-slate-400">
                {t('webhooks.payloadHint')}
              </p>
              <textarea
                value={formData.payloadTemplate}
                onChange={(e) => setFormData((f) => ({ ...f, payloadTemplate: e.target.value }))}
                rows={8}
                className="w-full rounded-lg border border-slate-300 px-3 py-2 font-mono text-xs leading-relaxed focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
              />
            </div>

            {/* Active toggle */}
            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={() => setFormData((f) => ({ ...f, isActive: !f.isActive }))}
                className={`relative inline-flex h-5 w-9 shrink-0 cursor-pointer items-center rounded-full transition-colors ${
                  formData.isActive ? 'bg-blue-600' : 'bg-slate-300'
                }`}
                role="switch"
                aria-checked={formData.isActive}
              >
                <span
                  className={`inline-block h-3.5 w-3.5 transform rounded-full bg-white shadow transition-transform ${
                    formData.isActive ? 'translate-x-[18px]' : 'translate-x-[3px]'
                  }`}
                />
              </button>
              <span className="text-sm text-slate-600">
                {formData.isActive ? t('webhooks.active') : t('webhooks.inactive')}
              </span>
            </div>
          </div>

          {/* Actions */}
          <div className="mt-5 flex gap-2 border-t border-slate-100 pt-4">
            <button
              type="submit"
              disabled={submitting}
              className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700 disabled:opacity-50"
            >
              {submitting ? t('common.loading') : t('common.save')}
            </button>
            <button
              type="button"
              onClick={() => { setShowForm(false); setEditingId(null); }}
              className="rounded-lg border border-slate-300 px-4 py-2 text-sm text-slate-600 transition hover:bg-slate-100"
            >
              {t('common.cancel')}
            </button>
          </div>
        </form>
      )}

      {/* Rule cards */}
      {(rules ?? []).length === 0 && !showForm && (
        <div className="rounded-xl border border-dashed border-slate-300 py-16 text-center text-slate-400">
          <Zap size={32} className="mx-auto mb-3 text-slate-300" />
          <p>{t('webhooks.noRules')}</p>
        </div>
      )}

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {(rules ?? []).map((rule) => (
          <div
            key={rule.id}
            className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm transition hover:shadow-md"
          >
            <div className="mb-3 flex items-start justify-between">
              <h3 className="font-semibold text-slate-800">{rule.name}</h3>
              <Badge variant={rule.isActive ? 'success' : 'neutral'}>
                {rule.isActive ? t('webhooks.active') : t('webhooks.inactive')}
              </Badge>
            </div>

            <div className="mb-3 space-y-1.5">
              <div className="flex items-center gap-2">
                <span className="text-xs text-slate-500">{t('webhooks.trigger')}:</span>
                <Badge variant="info">{rule.triggerEvent}</Badge>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-xs text-slate-500">{t('webhooks.target')}:</span>
                <code className="truncate rounded bg-slate-100 px-1.5 py-0.5 text-xs text-slate-600">{rule.targetUrl}</code>
              </div>
              {rule.conditions.length > 0 && (
                <div className="flex items-center gap-2">
                  <span className="text-xs text-slate-500">{t('webhooks.conditions')}:</span>
                  <Badge variant="neutral">{rule.conditions.length}</Badge>
                </div>
              )}
            </div>

            <div className="flex gap-1.5 border-t border-slate-100 pt-3">
              <button
                onClick={() => openEditForm(rule)}
                className="inline-flex items-center gap-1 rounded-lg border border-slate-200 px-2.5 py-1.5 text-xs font-medium text-slate-600 transition hover:bg-slate-100"
                title={t('common.edit')}
              >
                <Pencil size={12} /> {t('common.edit')}
              </button>
              <button
                onClick={() => void handleTest(rule.id)}
                disabled={testing === rule.id}
                className="inline-flex items-center gap-1 rounded-lg border border-slate-200 px-2.5 py-1.5 text-xs font-medium text-blue-600 transition hover:bg-blue-50 disabled:opacity-50"
                title={t('webhooks.test')}
              >
                <Play size={12} /> {t('webhooks.test')}
              </button>
              <button
                onClick={() => void handleDelete(rule.id)}
                disabled={deleting === rule.id}
                className="inline-flex items-center gap-1 rounded-lg border border-slate-200 px-2.5 py-1.5 text-xs font-medium text-red-500 transition hover:bg-red-50 disabled:opacity-50"
                title={t('common.delete')}
              >
                <Trash2 size={12} />
              </button>
            </div>

            {/* Test result */}
            {testResult && testResult.ruleId === rule.id && (
              <div className="mt-3 rounded-lg bg-slate-900 p-3">
                <h4 className="mb-1 text-xs font-semibold text-slate-400">{t('webhooks.testResult')}</h4>
                <pre className="max-h-40 overflow-auto text-xs text-emerald-300">
                  {JSON.stringify(testResult.data, null, 2)}
                </pre>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}

// ─── Main Page ───────────────────────────────────────────────────────────────

export default function WebhooksPage() {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState<TabId>('outgoing');

  const tabs: { id: TabId; label: string }[] = [
    { id: 'outgoing', label: t('webhooks.tabOutgoing') },
    { id: 'incoming', label: t('webhooks.tabIncoming') },
    { id: 'builder', label: t('webhooks.tabBuilder') },
  ];

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold text-slate-900">Webhooks</h1>

      {/* Tab navigation */}
      <div className="mb-6 border-b border-slate-200">
        <nav className="-mb-px flex gap-6">
          {tabs.map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`whitespace-nowrap border-b-2 pb-3 text-sm font-medium transition ${
                activeTab === tab.id
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-slate-500 hover:border-slate-300 hover:text-slate-700'
              }`}
            >
              {tab.label}
            </button>
          ))}
        </nav>
      </div>

      {/* Tab content */}
      {activeTab === 'outgoing' && <OutgoingTab />}
      {activeTab === 'incoming' && <IncomingTab />}
      {activeTab === 'builder' && <BuilderTab />}
    </div>
  );
}
