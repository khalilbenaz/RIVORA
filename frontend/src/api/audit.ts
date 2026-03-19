import api from './client';
import type { AuditLog } from '../types';

export const auditApi = {
  getAll: (params?: { page?: number; pageSize?: number }) =>
    api.get<AuditLog[]>('/audit', { params }),
};
