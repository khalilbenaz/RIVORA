import api from './client';
import type { Product } from '../types';

export const productsApi = {
  getAll: () => api.get<Product[]>('/products'),
  getById: (id: string) => api.get<Product>(`/products/${id}`),
  create: (data: Partial<Product>) => api.post<Product>('/products', data),
  update: (id: string, data: Partial<Product>) => api.put<Product>(`/products/${id}`, data),
  delete: (id: string) => api.delete(`/products/${id}`),
  search: (term: string) => api.get<Product[]>(`/products/search?term=${encodeURIComponent(term)}`),
};
