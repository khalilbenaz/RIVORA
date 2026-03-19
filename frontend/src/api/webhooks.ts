import api from './client';

// Outgoing webhooks (existing)
export interface WebhookSubscription {
  id: string;
  eventType: string;
  callbackUrl: string;
  secret: string | null;
  isActive: boolean;
  headers: string[];
  createdAt: string;
  lastTriggeredAt: string | null;
}

// Incoming webhooks
export interface IncomingWebhookConfig {
  id: string;
  name: string;
  source: string;
  signatureHeader: string | null;
  secret: string | null;
  signatureAlgorithm: string;
  isActive: boolean;
  createdAt: string;
}

export interface WebhookDeliveryLog {
  id: string;
  configId: string;
  source: string;
  httpMethod: string;
  eventType: string | null;
  headers: string;
  payload: string;
  statusCode: number | null;
  signatureValid: boolean;
  error: string | null;
  status: 'received' | 'processing' | 'processed' | 'failed';
  receivedAt: string;
  processedAt: string | null;
}

// Webhook rules (builder)
export interface WebhookRule {
  id: string;
  name: string;
  triggerEvent: string;
  conditions: WebhookCondition[];
  targetUrl: string;
  method: string;
  headers: Record<string, string>;
  payloadTemplate: string;
  isActive: boolean;
  createdAt: string;
}

export interface WebhookCondition {
  field: string;
  operator: 'equals' | 'contains' | 'gt' | 'lt' | 'exists';
  value: string;
}

export const webhooksApi = {
  // Outgoing
  getAll: () => api.get<WebhookSubscription[]>('/webhooks'),
  create: (data: Partial<WebhookSubscription>) => api.post<WebhookSubscription>('/webhooks', data),
  delete: (id: string) => api.delete(`/webhooks/${id}`),
  toggle: (id: string, isActive: boolean) => api.patch(`/webhooks/${id}`, { isActive }),

  // Incoming
  getIncomingConfigs: () => api.get<IncomingWebhookConfig[]>('/webhooks/incoming/configs'),
  createIncomingConfig: (data: Partial<IncomingWebhookConfig>) => api.post<IncomingWebhookConfig>('/webhooks/incoming/configs', data),
  deleteIncomingConfig: (id: string) => api.delete(`/webhooks/incoming/configs/${id}`),
  getIncomingLogs: () => api.get<WebhookDeliveryLog[]>('/webhooks/incoming/logs'),
  getIncomingLog: (id: string) => api.get<WebhookDeliveryLog>(`/webhooks/incoming/logs/${id}`),
  replayLog: (id: string) => api.post(`/webhooks/incoming/logs/${id}/replay`),

  // Rules (builder)
  getRules: () => api.get<WebhookRule[]>('/webhooks/rules'),
  createRule: (data: Partial<WebhookRule>) => api.post<WebhookRule>('/webhooks/rules', data),
  updateRule: (id: string, data: Partial<WebhookRule>) => api.put<WebhookRule>(`/webhooks/rules/${id}`, data),
  deleteRule: (id: string) => api.delete(`/webhooks/rules/${id}`),
  testRule: (id: string) => api.post(`/webhooks/rules/${id}/test`),
};
