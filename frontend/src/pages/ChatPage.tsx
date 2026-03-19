import { useState, useEffect, useRef, useCallback } from 'react';
import { Send, Search, MessageCircle, ArrowLeft, Loader2 } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { chatApi, type ChatRoom, type ChatMessage } from '../api/chat';
import { useSignalR } from '../hooks/useSignalR';
import { useAuthStore } from '../store/authStore';

function formatTime(dateStr: string) {
  const d = new Date(dateStr);
  const now = new Date();
  const diffDays = Math.floor((now.getTime() - d.getTime()) / (1000 * 60 * 60 * 24));
  if (diffDays === 0) return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  if (diffDays === 1) return 'Yesterday';
  if (diffDays < 7) return d.toLocaleDateString([], { weekday: 'short' });
  return d.toLocaleDateString([], { month: 'short', day: 'numeric' });
}

// ─── Room List Item ───────────────────────────────────────────
function RoomItem({
  room,
  active,
  onClick,
}: {
  room: ChatRoom;
  active: boolean;
  onClick: () => void;
}) {
  return (
    <button
      onClick={onClick}
      className={`flex w-full items-center gap-3 rounded-lg px-3 py-3 text-left transition-colors ${
        active ? 'bg-blue-50 dark:bg-blue-900/30' : 'hover:bg-slate-50 dark:hover:bg-slate-800'
      }`}
    >
      <div className="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-violet-500 text-sm font-bold text-white">
        {room.name.charAt(0).toUpperCase()}
      </div>
      <div className="min-w-0 flex-1">
        <div className="flex items-center justify-between">
          <span className="truncate text-sm font-medium text-slate-900 dark:text-slate-100">
            {room.name}
          </span>
          {room.lastMessage && (
            <span className="ml-2 flex-shrink-0 text-xs text-slate-400">
              {formatTime(room.lastMessage.createdAt)}
            </span>
          )}
        </div>
        <div className="flex items-center justify-between">
          <p className="truncate text-xs text-slate-500">
            {room.lastMessage?.content ?? 'No messages yet'}
          </p>
          {room.unreadCount > 0 && (
            <span className="ml-2 flex h-5 min-w-[20px] flex-shrink-0 items-center justify-center rounded-full bg-blue-600 px-1.5 text-[10px] font-bold text-white">
              {room.unreadCount > 99 ? '99+' : room.unreadCount}
            </span>
          )}
        </div>
      </div>
    </button>
  );
}

// ─── Message Skeleton ─────────────────────────────────────────
function MessageSkeleton() {
  return (
    <div className="space-y-4 p-4">
      {[...Array(6)].map((_, i) => (
        <div key={i} className={`flex ${i % 2 === 0 ? 'justify-start' : 'justify-end'}`}>
          <div
            className={`h-10 animate-pulse rounded-2xl ${
              i % 2 === 0 ? 'bg-slate-200 dark:bg-slate-700' : 'bg-blue-200 dark:bg-blue-800'
            }`}
            style={{ width: `${120 + Math.random() * 180}px` }}
          />
        </div>
      ))}
    </div>
  );
}

// ─── Message Bubble ───────────────────────────────────────────
function MessageBubble({ message, isOwn }: { message: ChatMessage; isOwn: boolean }) {
  return (
    <div className={`flex ${isOwn ? 'justify-end' : 'justify-start'}`}>
      <div className={`max-w-[75%] space-y-1`}>
        {!isOwn && (
          <p className="px-1 text-xs font-medium text-slate-500">{message.senderName}</p>
        )}
        <div
          className={`rounded-2xl px-4 py-2.5 text-sm ${
            isOwn
              ? 'rounded-br-md bg-blue-600 text-white'
              : 'rounded-bl-md bg-slate-100 text-slate-900 dark:bg-slate-700 dark:text-slate-100'
          }`}
        >
          {message.content}
        </div>
        <p className={`px-1 text-[10px] text-slate-400 ${isOwn ? 'text-right' : 'text-left'}`}>
          {formatTime(message.createdAt)}
        </p>
      </div>
    </div>
  );
}

// ─── Chat Page ────────────────────────────────────────────────
export default function ChatPage() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const currentUserId = user?.id ?? '';

  const [rooms, setRooms] = useState<ChatRoom[]>([]);
  const [selectedRoomId, setSelectedRoomId] = useState<string | null>(null);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [loadingRooms, setLoadingRooms] = useState(true);
  const [loadingMessages, setLoadingMessages] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [newMessage, setNewMessage] = useState('');
  const [sending, setSending] = useState(false);

  const messagesEndRef = useRef<HTMLDivElement>(null);
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const { notifications } = useSignalR();

  // Load rooms
  useEffect(() => {
    setLoadingRooms(true);
    chatApi
      .getRooms()
      .then((res) => setRooms(res.data))
      .catch(() => {})
      .finally(() => setLoadingRooms(false));
  }, []);

  // Load messages when room changes
  useEffect(() => {
    if (!selectedRoomId) return;
    setLoadingMessages(true);
    setMessages([]);
    chatApi
      .getMessages(selectedRoomId)
      .then((res) => setMessages(res.data))
      .catch(() => {})
      .finally(() => setLoadingMessages(false));

    chatApi.markAsRead(selectedRoomId).catch(() => {});
    setRooms((prev) =>
      prev.map((r) => (r.id === selectedRoomId ? { ...r, unreadCount: 0 } : r))
    );
  }, [selectedRoomId]);

  // Listen for real-time chat messages via SignalR
  useEffect(() => {
    const latest = notifications[0];
    if (!latest || latest.type !== 'chat.message') return;

    const incoming = latest.data as ChatMessage;
    if (!incoming?.roomId) return;

    // Add to current chat if viewing that room
    if (incoming.roomId === selectedRoomId) {
      setMessages((prev) => {
        if (prev.some((m) => m.id === incoming.id)) return prev;
        return [...prev, incoming];
      });
    }

    // Update room list
    setRooms((prev) =>
      prev.map((r) =>
        r.id === incoming.roomId
          ? {
              ...r,
              lastMessage: incoming,
              unreadCount: r.id === selectedRoomId ? 0 : r.unreadCount + 1,
            }
          : r
      )
    );
  }, [notifications, selectedRoomId]);

  // Auto-scroll
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  // Send message
  const handleSend = useCallback(async () => {
    const content = newMessage.trim();
    if (!content || !selectedRoomId || sending) return;

    setSending(true);
    setNewMessage('');

    try {
      const res = await chatApi.sendMessage(selectedRoomId, content);
      setMessages((prev) => [...prev, res.data]);
      setRooms((prev) =>
        prev.map((r) => (r.id === selectedRoomId ? { ...r, lastMessage: res.data } : r))
      );
    } catch {
      setNewMessage(content); // Restore on failure
    } finally {
      setSending(false);
      textareaRef.current?.focus();
    }
  }, [newMessage, selectedRoomId, sending]);

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const filteredRooms = rooms.filter((r) =>
    r.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const selectedRoom = rooms.find((r) => r.id === selectedRoomId);

  // Mobile: show either list or thread
  const showMobileChat = selectedRoomId !== null;

  return (
    <div className="flex h-[calc(100vh-4rem)] overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-900">
      {/* ── Left panel: Room list ── */}
      <div
        className={`w-full flex-shrink-0 border-r border-slate-200 dark:border-slate-700 md:w-80 md:block ${
          showMobileChat ? 'hidden' : 'block'
        }`}
      >
        <div className="border-b border-slate-200 p-4 dark:border-slate-700">
          <h2 className="mb-3 text-lg font-bold text-slate-900 dark:text-white">
            {t('nav.chat')}
          </h2>
          <div className="relative">
            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
            <input
              type="text"
              placeholder={t('common.search')}
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full rounded-lg border border-slate-200 bg-slate-50 py-2 pl-9 pr-3 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
            />
          </div>
        </div>

        <div className="overflow-y-auto p-2" style={{ height: 'calc(100% - 110px)' }}>
          {loadingRooms ? (
            <div className="space-y-2 p-2">
              {[...Array(5)].map((_, i) => (
                <div key={i} className="flex items-center gap-3 rounded-lg p-3">
                  <div className="h-10 w-10 animate-pulse rounded-full bg-slate-200 dark:bg-slate-700" />
                  <div className="flex-1 space-y-2">
                    <div className="h-3 w-24 animate-pulse rounded bg-slate-200 dark:bg-slate-700" />
                    <div className="h-2 w-36 animate-pulse rounded bg-slate-200 dark:bg-slate-700" />
                  </div>
                </div>
              ))}
            </div>
          ) : filteredRooms.length === 0 ? (
            <div className="py-12 text-center text-sm text-slate-400">
              {t('common.noData')}
            </div>
          ) : (
            filteredRooms.map((room) => (
              <RoomItem
                key={room.id}
                room={room}
                active={room.id === selectedRoomId}
                onClick={() => setSelectedRoomId(room.id)}
              />
            ))
          )}
        </div>
      </div>

      {/* ── Right panel: Messages ── */}
      <div
        className={`flex flex-1 flex-col md:flex ${
          showMobileChat ? 'flex' : 'hidden md:flex'
        }`}
      >
        {selectedRoom ? (
          <>
            {/* Header */}
            <div className="flex items-center gap-3 border-b border-slate-200 px-4 py-3 dark:border-slate-700">
              <button
                onClick={() => setSelectedRoomId(null)}
                className="rounded p-1 text-slate-400 hover:text-slate-600 md:hidden"
              >
                <ArrowLeft size={20} />
              </button>
              <div className="flex h-9 w-9 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-violet-500 text-sm font-bold text-white">
                {selectedRoom.name.charAt(0).toUpperCase()}
              </div>
              <div>
                <h3 className="text-sm font-semibold text-slate-900 dark:text-white">
                  {selectedRoom.name}
                </h3>
                <p className="text-xs text-slate-400">
                  {selectedRoom.participants.length} participants
                </p>
              </div>
            </div>

            {/* Messages */}
            <div className="flex-1 overflow-y-auto p-4">
              {loadingMessages ? (
                <MessageSkeleton />
              ) : messages.length === 0 ? (
                <div className="flex h-full flex-col items-center justify-center text-slate-400">
                  <MessageCircle size={40} className="mb-3 opacity-40" />
                  <p className="text-sm">No messages yet. Start the conversation!</p>
                </div>
              ) : (
                <div className="space-y-3">
                  {messages.map((msg) => (
                    <MessageBubble
                      key={msg.id}
                      message={msg}
                      isOwn={msg.senderId === currentUserId}
                    />
                  ))}
                  <div ref={messagesEndRef} />
                </div>
              )}
            </div>

            {/* Input */}
            <div className="border-t border-slate-200 p-3 dark:border-slate-700">
              <div className="flex items-end gap-2">
                <textarea
                  ref={textareaRef}
                  value={newMessage}
                  onChange={(e) => setNewMessage(e.target.value)}
                  onKeyDown={handleKeyDown}
                  placeholder="Type a message..."
                  rows={1}
                  className="max-h-32 flex-1 resize-none rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
                />
                <button
                  onClick={handleSend}
                  disabled={!newMessage.trim() || sending}
                  className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-600 text-white transition hover:bg-blue-700 disabled:opacity-40"
                >
                  {sending ? <Loader2 size={18} className="animate-spin" /> : <Send size={18} />}
                </button>
              </div>
            </div>
          </>
        ) : (
          /* Empty state */
          <div className="flex flex-1 flex-col items-center justify-center text-slate-400">
            <MessageCircle size={56} className="mb-4 opacity-30" />
            <p className="text-lg font-medium">Select a conversation</p>
            <p className="text-sm">Choose a room from the sidebar to start chatting.</p>
          </div>
        )}
      </div>
    </div>
  );
}
