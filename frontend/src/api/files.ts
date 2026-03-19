import api from './client';

export interface FileItem {
  id: string;
  name: string;
  size: number;
  mimeType: string;
  url: string;
  thumbnailUrl?: string;
  uploadedBy: string;
  uploadedAt: string;
  folder?: string;
}

export const filesApi = {
  getAll: (folder?: string) => api.get<FileItem[]>('/files', { params: { folder } }),
  upload: (file: File, folder?: string) => {
    const formData = new FormData();
    formData.append('file', file);
    if (folder) formData.append('folder', folder);
    return api.post<FileItem>('/files', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  delete: (id: string) => api.delete(`/files/${id}`),
};
