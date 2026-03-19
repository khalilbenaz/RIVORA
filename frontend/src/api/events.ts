import api from './client';

export interface CalendarEvent {
  id: string;
  title: string;
  description?: string;
  startDate: string;
  endDate?: string;
  allDay: boolean;
  color: string;
  createdBy: string;
  createdAt: string;
}

export const eventsApi = {
  getAll: (month?: number, year?: number) =>
    api.get<CalendarEvent[]>('/events', { params: { month, year } }),
  create: (data: Partial<CalendarEvent>) => api.post<CalendarEvent>('/events', data),
  update: (id: string, data: Partial<CalendarEvent>) => api.put<CalendarEvent>(`/events/${id}`, data),
  delete: (id: string) => api.delete(`/events/${id}`),
};
