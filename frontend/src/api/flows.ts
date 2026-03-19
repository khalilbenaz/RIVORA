import api from './client';

export type NodeType = 'trigger' | 'condition' | 'action' | 'transform' | 'delay' | 'webhook' | 'email' | 'log';

export interface FlowNode {
  id: string;
  type: NodeType;
  label: string;
  config: Record<string, string>;
  x: number;
  y: number;
}

export interface FlowConnection {
  id: string;
  fromNodeId: string;
  toNodeId: string;
  label?: string;
}

export interface Flow {
  id: string;
  name: string;
  description?: string;
  nodes: FlowNode[];
  connections: FlowConnection[];
  isActive: boolean;
  triggerType: string;
  lastRunAt?: string;
  runCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface FlowRun {
  id: string;
  flowId: string;
  status: 'running' | 'completed' | 'failed';
  startedAt: string;
  completedAt?: string;
  steps: FlowRunStep[];
  error?: string;
}

export interface FlowRunStep {
  nodeId: string;
  nodeLabel: string;
  status: 'pending' | 'running' | 'completed' | 'failed' | 'skipped';
  input?: string;
  output?: string;
  duration?: number;
  error?: string;
}

export const flowsApi = {
  getAll: () => api.get<Flow[]>('/flows'),
  getById: (id: string) => api.get<Flow>(`/flows/${id}`),
  create: (data: Partial<Flow>) => api.post<Flow>('/flows', data),
  update: (id: string, data: Partial<Flow>) => api.put<Flow>(`/flows/${id}`, data),
  delete: (id: string) => api.delete(`/flows/${id}`),
  toggle: (id: string, isActive: boolean) => api.patch(`/flows/${id}`, { isActive }),
  run: (id: string) => api.post<FlowRun>(`/flows/${id}/run`),
  getRuns: (id: string) => api.get<FlowRun[]>(`/flows/${id}/runs`),
};
