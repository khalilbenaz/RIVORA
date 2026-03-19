import api from './client';

export interface Note {
  id: string;
  title: string;
  content: string;
  color: 'yellow' | 'blue' | 'green' | 'pink' | 'purple';
  pinned: boolean;
  createdBy: string;
  createdAt: string;
  updatedAt: string;
}

export const notesApi = {
  getAll: () => api.get<Note[]>('/notes'),
  create: (data: Partial<Note>) => api.post<Note>('/notes', data),
  update: (id: string, data: Partial<Note>) => api.put<Note>(`/notes/${id}`, data),
  delete: (id: string) => api.delete(`/notes/${id}`),
  togglePin: (id: string) => api.patch(`/notes/${id}/pin`),
};
