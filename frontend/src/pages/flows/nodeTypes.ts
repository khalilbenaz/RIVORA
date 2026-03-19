import { Zap, GitBranch, Play, ArrowRightLeft, Clock, Globe, Mail, FileText } from 'lucide-react';
import type { NodeType } from '../../api/flows';

export interface NodeTypeConfig {
  type: NodeType;
  label: string;
  icon: React.ElementType;
  color: string;
  borderColor: string;
  configFields: { key: string; label: string; type: 'text' | 'select' | 'textarea' | 'number'; options?: string[] }[];
}

export const nodeTypeConfigs: Record<NodeType, NodeTypeConfig> = {
  trigger: {
    type: 'trigger', label: 'Trigger', icon: Zap, color: 'bg-amber-100', borderColor: 'border-amber-400',
    configFields: [
      { key: 'event', label: 'Event', type: 'select', options: ['product.created', 'product.updated', 'user.created', 'user.deleted', 'order.placed', 'webhook.received', 'schedule.cron'] },
      { key: 'filter', label: 'Filter', type: 'text' },
    ]
  },
  condition: {
    type: 'condition', label: 'Condition', icon: GitBranch, color: 'bg-violet-100', borderColor: 'border-violet-400',
    configFields: [
      { key: 'field', label: 'Field', type: 'text' },
      { key: 'operator', label: 'Operator', type: 'select', options: ['equals', 'not_equals', 'contains', 'gt', 'lt', 'exists'] },
      { key: 'value', label: 'Value', type: 'text' },
    ]
  },
  action: {
    type: 'action', label: 'Action', icon: Play, color: 'bg-emerald-100', borderColor: 'border-emerald-400',
    configFields: [
      { key: 'action', label: 'Action', type: 'select', options: ['create_record', 'update_record', 'delete_record', 'call_api', 'run_query'] },
      { key: 'target', label: 'Target', type: 'text' },
      { key: 'data', label: 'Data', type: 'textarea' },
    ]
  },
  transform: {
    type: 'transform', label: 'Transform', icon: ArrowRightLeft, color: 'bg-blue-100', borderColor: 'border-blue-400',
    configFields: [
      { key: 'mapping', label: 'Mapping (JSON)', type: 'textarea' },
    ]
  },
  delay: {
    type: 'delay', label: 'Delay', icon: Clock, color: 'bg-slate-100', borderColor: 'border-slate-400',
    configFields: [
      { key: 'duration', label: 'Duration (seconds)', type: 'number' },
    ]
  },
  webhook: {
    type: 'webhook', label: 'Webhook', icon: Globe, color: 'bg-cyan-100', borderColor: 'border-cyan-400',
    configFields: [
      { key: 'url', label: 'URL', type: 'text' },
      { key: 'method', label: 'Method', type: 'select', options: ['POST', 'PUT', 'PATCH', 'GET'] },
      { key: 'headers', label: 'Headers (JSON)', type: 'textarea' },
      { key: 'body', label: 'Body template', type: 'textarea' },
    ]
  },
  email: {
    type: 'email', label: 'Email', icon: Mail, color: 'bg-pink-100', borderColor: 'border-pink-400',
    configFields: [
      { key: 'to', label: 'To', type: 'text' },
      { key: 'subject', label: 'Subject', type: 'text' },
      { key: 'template', label: 'Template', type: 'select', options: ['welcome', 'password-reset', 'email-verification', 'invoice', 'custom'] },
      { key: 'body', label: 'Body (if custom)', type: 'textarea' },
    ]
  },
  log: {
    type: 'log', label: 'Log', icon: FileText, color: 'bg-gray-100', borderColor: 'border-gray-400',
    configFields: [
      { key: 'message', label: 'Message', type: 'text' },
      { key: 'level', label: 'Level', type: 'select', options: ['info', 'warning', 'error'] },
    ]
  },
};
