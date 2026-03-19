import api from './client';
import type { Tenant } from '../types';

export const tenantsApi = {
  getAll: () => api.get<Tenant[]>('/tenants'),
  getById: (id: string) => api.get<Tenant>(`/tenants/${id}`),
  create: (data: Partial<Tenant>) => api.post<Tenant>('/tenants', data),
  update: (id: string, data: Partial<Tenant>) => api.put<Tenant>(`/tenants/${id}`, data),
  delete: (id: string) => api.delete(`/tenants/${id}`),
};
