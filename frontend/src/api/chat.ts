import api from './client';

export interface ChatRoom {
  id: string;
  name: string;
  type: 'direct' | 'group';
  participants: string[];
  lastMessage?: ChatMessage;
  unreadCount: number;
  createdAt: string;
}

export interface ChatMessage {
  id: string;
  roomId: string;
  senderId: string;
  senderName: string;
  content: string;
  type: 'text' | 'image' | 'file';
  attachmentUrl?: string;
  createdAt: string;
  readBy: string[];
}

export const chatApi = {
  getRooms: () => api.get<ChatRoom[]>('/chat/rooms'),
  getMessages: (roomId: string, before?: string) =>
    api.get<ChatMessage[]>(`/chat/rooms/${roomId}/messages`, { params: { before } }),
  sendMessage: (roomId: string, content: string, type?: string) =>
    api.post<ChatMessage>(`/chat/rooms/${roomId}/messages`, { content, type: type ?? 'text' }),
  createRoom: (data: { name: string; type: string; participantIds: string[] }) =>
    api.post<ChatRoom>('/chat/rooms', data),
  markAsRead: (roomId: string) => api.post(`/chat/rooms/${roomId}/read`),
};
