import api from './client';

export type TaskPriority = 'low' | 'medium' | 'high' | 'urgent';
export type TaskStatus = 'backlog' | 'todo' | 'in_progress' | 'review' | 'done';

export interface KanbanTask {
  id: string;
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  assignee?: string;
  labels: string[];
  dueDate?: string;
  createdAt: string;
  order: number;
}

export const tasksApi = {
  getAll: () => api.get<KanbanTask[]>('/tasks'),
  create: (data: Partial<KanbanTask>) => api.post<KanbanTask>('/tasks', data),
  update: (id: string, data: Partial<KanbanTask>) => api.put<KanbanTask>(`/tasks/${id}`, data),
  delete: (id: string) => api.delete(`/tasks/${id}`),
  move: (id: string, status: TaskStatus, order: number) => api.patch(`/tasks/${id}/move`, { status, order }),
};
