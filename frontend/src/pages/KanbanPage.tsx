import { useState, useRef, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Plus,
  X,
  Calendar,
  AlertCircle,
} from 'lucide-react';
import type { KanbanTask, TaskStatus, TaskPriority } from '../api/tasks';

// ─── Constants ──────────────────────────────────────────────────────
const COLUMNS: { id: TaskStatus; labelKey: string; color: string }[] = [
  { id: 'backlog', labelKey: 'kanban.backlog', color: 'bg-slate-500' },
  { id: 'todo', labelKey: 'kanban.todo', color: 'bg-blue-500' },
  { id: 'in_progress', labelKey: 'kanban.inProgress', color: 'bg-amber-500' },
  { id: 'review', labelKey: 'kanban.review', color: 'bg-purple-500' },
  { id: 'done', labelKey: 'kanban.done', color: 'bg-green-500' },
];

const PRIORITY_CONFIG: Record<TaskPriority, { bg: string; text: string; label: string }> = {
  urgent: { bg: 'bg-red-500/20', text: 'text-red-400', label: 'Urgent' },
  high: { bg: 'bg-orange-500/20', text: 'text-orange-400', label: 'High' },
  medium: { bg: 'bg-blue-500/20', text: 'text-blue-400', label: 'Medium' },
  low: { bg: 'bg-slate-500/20', text: 'text-slate-400', label: 'Low' },
};

const LABEL_COLORS = [
  'bg-blue-500/20 text-blue-300',
  'bg-green-500/20 text-green-300',
  'bg-purple-500/20 text-purple-300',
  'bg-pink-500/20 text-pink-300',
  'bg-amber-500/20 text-amber-300',
  'bg-teal-500/20 text-teal-300',
];

function labelColor(label: string): string {
  let hash = 0;
  for (let i = 0; i < label.length; i++) hash = label.charCodeAt(i) + ((hash << 5) - hash);
  return LABEL_COLORS[Math.abs(hash) % LABEL_COLORS.length] ?? LABEL_COLORS[0]!;
}

function isOverdue(dueDate?: string): boolean {
  if (!dueDate) return false;
  return new Date(dueDate) < new Date();
}

function initials(name: string): string {
  return name.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2);
}

// ─── Mock Data ──────────────────────────────────────────────────────
const MOCK_TASKS: KanbanTask[] = [
  { id: '1', title: 'Set up project structure', description: 'Initialize the monorepo with proper folder structure and tooling', status: 'done', priority: 'high', assignee: 'Alice Martin', labels: ['setup', 'infra'], createdAt: '2026-03-10T10:00:00Z', order: 0 },
  { id: '2', title: 'Design database schema', description: 'Create the initial ERD and migration scripts for core entities', status: 'done', priority: 'high', assignee: 'Bob Chen', labels: ['backend', 'database'], createdAt: '2026-03-10T11:00:00Z', order: 1 },
  { id: '3', title: 'Implement authentication', description: 'JWT-based auth with refresh tokens and role-based access', status: 'review', priority: 'urgent', assignee: 'Alice Martin', labels: ['backend', 'security'], dueDate: '2026-03-18T00:00:00Z', createdAt: '2026-03-11T09:00:00Z', order: 0 },
  { id: '4', title: 'Create landing page', description: 'Hero section, features grid, pricing and footer', status: 'review', priority: 'medium', assignee: 'Carol Davis', labels: ['frontend', 'design'], createdAt: '2026-03-12T08:00:00Z', order: 1 },
  { id: '5', title: 'Build dashboard layout', description: 'Sidebar navigation, header with user menu, responsive design', status: 'in_progress', priority: 'high', assignee: 'Carol Davis', labels: ['frontend'], dueDate: '2026-03-22T00:00:00Z', createdAt: '2026-03-13T10:00:00Z', order: 0 },
  { id: '6', title: 'Set up CI/CD pipeline', description: 'GitHub Actions for build, test, and deploy to staging', status: 'in_progress', priority: 'medium', assignee: 'Dave Wilson', labels: ['devops', 'infra'], createdAt: '2026-03-13T14:00:00Z', order: 1 },
  { id: '7', title: 'Write unit tests for services', description: 'Cover business logic with xUnit tests and mocks', status: 'todo', priority: 'medium', assignee: 'Bob Chen', labels: ['backend', 'testing'], dueDate: '2026-03-25T00:00:00Z', createdAt: '2026-03-14T09:00:00Z', order: 0 },
  { id: '8', title: 'Implement file upload', description: 'S3-compatible storage with presigned URLs and progress tracking', status: 'todo', priority: 'low', assignee: 'Dave Wilson', labels: ['backend', 'feature'], createdAt: '2026-03-14T11:00:00Z', order: 1 },
  { id: '9', title: 'Add multi-tenancy support', description: 'Tenant isolation at database level with middleware', status: 'todo', priority: 'high', labels: ['backend', 'architecture'], createdAt: '2026-03-15T08:00:00Z', order: 2 },
  { id: '10', title: 'Design email templates', description: 'Transactional email templates for welcome, reset password, notifications', status: 'backlog', priority: 'low', labels: ['design'], createdAt: '2026-03-15T10:00:00Z', order: 0 },
  { id: '11', title: 'Implement webhooks system', description: 'Outgoing webhooks with retry logic and delivery logs', status: 'backlog', priority: 'medium', labels: ['backend', 'feature'], createdAt: '2026-03-16T09:00:00Z', order: 1 },
  { id: '12', title: 'Add dark mode support', description: 'Theme toggle with system preference detection and persistence', status: 'backlog', priority: 'low', assignee: 'Carol Davis', labels: ['frontend', 'ux'], createdAt: '2026-03-16T14:00:00Z', order: 2 },
];

// ─── Task Card ──────────────────────────────────────────────────────
function TaskCard({
  task,
  onDragStart,
  onClick,
}: {
  task: KanbanTask;
  onDragStart: (e: React.DragEvent, task: KanbanTask) => void;
  onClick: (task: KanbanTask) => void;
}) {
  const p = PRIORITY_CONFIG[task.priority];
  const overdue = isOverdue(task.dueDate);

  return (
    <div
      draggable
      onDragStart={e => onDragStart(e, task)}
      onClick={() => onClick(task)}
      className="group cursor-pointer rounded-lg border border-slate-700 bg-slate-800 p-3 shadow-sm transition-all hover:border-slate-500 hover:shadow-md active:opacity-70 active:shadow-lg"
    >
      {/* Labels */}
      {task.labels.length > 0 && (
        <div className="mb-2 flex flex-wrap gap-1">
          {task.labels.map(l => (
            <span key={l} className={`rounded-full px-2 py-0.5 text-[10px] font-medium ${labelColor(l)}`}>
              {l}
            </span>
          ))}
        </div>
      )}

      {/* Title */}
      <h4 className="mb-1 text-sm font-semibold text-white">{task.title}</h4>

      {/* Description */}
      {task.description && (
        <p className="mb-2 line-clamp-2 text-xs text-slate-400">{task.description}</p>
      )}

      {/* Footer */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          {/* Priority badge */}
          <span className={`rounded px-1.5 py-0.5 text-[10px] font-medium ${p.bg} ${p.text}`}>
            {p.label}
          </span>

          {/* Due date */}
          {task.dueDate && (
            <span className={`flex items-center gap-1 text-[10px] ${overdue ? 'text-red-400' : 'text-slate-500'}`}>
              <Calendar size={10} />
              {new Date(task.dueDate).toLocaleDateString()}
              {overdue && <AlertCircle size={10} />}
            </span>
          )}
        </div>

        {/* Assignee */}
        {task.assignee && (
          <div
            className="flex h-6 w-6 items-center justify-center rounded-full bg-blue-600/30 text-[10px] font-bold text-blue-300"
            title={task.assignee}
          >
            {initials(task.assignee)}
          </div>
        )}
      </div>
    </div>
  );
}

// ─── Inline Add Form ────────────────────────────────────────────────
function InlineAddForm({
  onAdd,
  onCancel,
}: {
  onAdd: (title: string, description: string, priority: TaskPriority) => void;
  onCancel: () => void;
}) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState<TaskPriority>('medium');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) return;
    onAdd(title.trim(), description.trim(), priority);
    setTitle('');
    setDescription('');
    setPriority('medium');
  };

  return (
    <form onSubmit={handleSubmit} className="rounded-lg border border-slate-600 bg-slate-800 p-2">
      <input
        value={title}
        onChange={e => setTitle(e.target.value)}
        placeholder="Task title..."
        autoFocus
        className="mb-1.5 w-full rounded border border-slate-600 bg-slate-700 px-2 py-1.5 text-sm text-white placeholder-slate-500 focus:border-blue-500 focus:outline-none"
      />
      <textarea
        value={description}
        onChange={e => setDescription(e.target.value)}
        placeholder="Description (optional)"
        rows={2}
        className="mb-1.5 w-full rounded border border-slate-600 bg-slate-700 px-2 py-1.5 text-xs text-white placeholder-slate-500 focus:border-blue-500 focus:outline-none"
      />
      <div className="flex items-center justify-between">
        <select
          value={priority}
          onChange={e => setPriority(e.target.value as TaskPriority)}
          className="rounded border border-slate-600 bg-slate-700 px-2 py-1 text-xs text-white focus:outline-none"
        >
          <option value="low">Low</option>
          <option value="medium">Medium</option>
          <option value="high">High</option>
          <option value="urgent">Urgent</option>
        </select>
        <div className="flex gap-1">
          <button type="button" onClick={onCancel} className="rounded px-2 py-1 text-xs text-slate-400 hover:text-white">
            Cancel
          </button>
          <button type="submit" className="rounded bg-blue-600 px-3 py-1 text-xs text-white hover:bg-blue-500">
            Add
          </button>
        </div>
      </div>
    </form>
  );
}

// ─── Detail Panel ───────────────────────────────────────────────────
function DetailPanel({
  task,
  onUpdate,
  onClose,
  onDelete,
}: {
  task: KanbanTask;
  onUpdate: (id: string, patch: Partial<KanbanTask>) => void;
  onClose: () => void;
  onDelete: (id: string) => void;
}) {
  const { t } = useTranslation();
  const [title, setTitle] = useState(task.title);
  const [description, setDescription] = useState(task.description || '');
  const [priority, setPriority] = useState(task.priority);
  const [assignee, setAssignee] = useState(task.assignee || '');
  const [dueDate, setDueDate] = useState(task.dueDate?.slice(0, 10) || '');
  const [labelsText, setLabelsText] = useState(task.labels.join(', '));

  const handleSave = () => {
    onUpdate(task.id, {
      title,
      description: description || undefined,
      priority,
      assignee: assignee || undefined,
      dueDate: dueDate ? new Date(dueDate).toISOString() : undefined,
      labels: labelsText.split(',').map(l => l.trim()).filter(Boolean),
    });
    onClose();
  };

  return (
    <div className="fixed inset-y-0 right-0 z-50 flex w-96 flex-col border-l border-slate-700 bg-slate-900 shadow-2xl">
      {/* Header */}
      <div className="flex items-center justify-between border-b border-slate-700 px-4 py-3">
        <h3 className="text-sm font-semibold text-white">{t('kanban.taskDetail')}</h3>
        <button onClick={onClose} className="text-slate-400 hover:text-white">
          <X size={18} />
        </button>
      </div>

      {/* Form */}
      <div className="flex-1 overflow-y-auto p-4 space-y-3">
        <div>
          <label className="mb-1 block text-xs text-slate-400">{t('kanban.taskTitle')}</label>
          <input
            value={title}
            onChange={e => setTitle(e.target.value)}
            className="w-full rounded-lg border border-slate-600 bg-slate-800 px-3 py-2 text-sm text-white focus:border-blue-500 focus:outline-none"
          />
        </div>

        <div>
          <label className="mb-1 block text-xs text-slate-400">{t('kanban.taskDescription')}</label>
          <textarea
            value={description}
            onChange={e => setDescription(e.target.value)}
            rows={4}
            className="w-full rounded-lg border border-slate-600 bg-slate-800 px-3 py-2 text-sm text-white focus:border-blue-500 focus:outline-none"
          />
        </div>

        <div>
          <label className="mb-1 block text-xs text-slate-400">{t('kanban.priority')}</label>
          <select
            value={priority}
            onChange={e => setPriority(e.target.value as TaskPriority)}
            className="w-full rounded-lg border border-slate-600 bg-slate-800 px-3 py-2 text-sm text-white focus:border-blue-500 focus:outline-none"
          >
            <option value="low">Low</option>
            <option value="medium">Medium</option>
            <option value="high">High</option>
            <option value="urgent">Urgent</option>
          </select>
        </div>

        <div>
          <label className="mb-1 block text-xs text-slate-400">{t('kanban.assignee')}</label>
          <input
            value={assignee}
            onChange={e => setAssignee(e.target.value)}
            placeholder="Name..."
            className="w-full rounded-lg border border-slate-600 bg-slate-800 px-3 py-2 text-sm text-white focus:border-blue-500 focus:outline-none"
          />
        </div>

        <div>
          <label className="mb-1 block text-xs text-slate-400">{t('kanban.dueDate')}</label>
          <input
            type="date"
            value={dueDate}
            onChange={e => setDueDate(e.target.value)}
            className="w-full rounded-lg border border-slate-600 bg-slate-800 px-3 py-2 text-sm text-white focus:border-blue-500 focus:outline-none"
          />
        </div>

        <div>
          <label className="mb-1 block text-xs text-slate-400">{t('kanban.labels')} (comma separated)</label>
          <input
            value={labelsText}
            onChange={e => setLabelsText(e.target.value)}
            placeholder="bug, frontend, urgent"
            className="w-full rounded-lg border border-slate-600 bg-slate-800 px-3 py-2 text-sm text-white focus:border-blue-500 focus:outline-none"
          />
        </div>

        <div className="pt-2 text-xs text-slate-500">
          {t('kanban.status')}: <span className="text-slate-300">{task.status.replace('_', ' ')}</span>
          <br />
          {t('kanban.created')}: <span className="text-slate-300">{new Date(task.createdAt).toLocaleDateString()}</span>
        </div>
      </div>

      {/* Actions */}
      <div className="border-t border-slate-700 px-4 py-3 flex justify-between">
        <button
          onClick={() => { onDelete(task.id); onClose(); }}
          className="rounded-lg px-3 py-1.5 text-sm text-red-400 hover:bg-red-500/10"
        >
          {t('common.delete')}
        </button>
        <div className="flex gap-2">
          <button onClick={onClose} className="rounded-lg px-3 py-1.5 text-sm text-slate-400 hover:text-white">
            {t('common.cancel')}
          </button>
          <button onClick={handleSave} className="rounded-lg bg-blue-600 px-4 py-1.5 text-sm text-white hover:bg-blue-500">
            {t('common.save')}
          </button>
        </div>
      </div>
    </div>
  );
}

// ─── Main Kanban Page ───────────────────────────────────────────────
export default function KanbanPage() {
  const { t } = useTranslation();
  const [tasks, setTasks] = useState<KanbanTask[]>(MOCK_TASKS);
  const [addingIn, setAddingIn] = useState<TaskStatus | null>(null);
  const [detailTask, setDetailTask] = useState<KanbanTask | null>(null);
  const [dragOverCol, setDragOverCol] = useState<TaskStatus | null>(null);
  const dragRef = useRef<KanbanTask | null>(null);

  const tasksByStatus = useCallback(
    (status: TaskStatus) => tasks.filter(t => t.status === status).sort((a, b) => a.order - b.order),
    [tasks],
  );

  const handleDragStart = (_e: React.DragEvent, task: KanbanTask) => {
    dragRef.current = task;
  };

  const handleDragOver = (e: React.DragEvent, status: TaskStatus) => {
    e.preventDefault();
    setDragOverCol(status);
  };

  const handleDragLeave = () => {
    setDragOverCol(null);
  };

  const handleDrop = (e: React.DragEvent, status: TaskStatus) => {
    e.preventDefault();
    setDragOverCol(null);
    const task = dragRef.current;
    if (!task || task.status === status) return;

    setTasks(prev => {
      const colTasks = prev.filter(t => t.status === status);
      const newOrder = colTasks.length;
      return prev.map(t =>
        t.id === task.id ? { ...t, status, order: newOrder } : t,
      );
    });
    dragRef.current = null;
  };

  const handleAdd = (status: TaskStatus, title: string, description: string, priority: TaskPriority) => {
    const colTasks = tasks.filter(t => t.status === status);
    const newTask: KanbanTask = {
      id: crypto.randomUUID(),
      title,
      description: description || undefined,
      status,
      priority,
      labels: [],
      createdAt: new Date().toISOString(),
      order: colTasks.length,
    };
    setTasks(prev => [...prev, newTask]);
    setAddingIn(null);
  };

  const handleUpdate = (id: string, patch: Partial<KanbanTask>) => {
    setTasks(prev => prev.map(t => t.id === id ? { ...t, ...patch } : t));
  };

  const handleDelete = (id: string) => {
    setTasks(prev => prev.filter(t => t.id !== id));
  };

  return (
    <div className="flex h-full flex-col overflow-hidden">
      {/* Header */}
      <div className="flex items-center justify-between border-b border-slate-700 bg-slate-800/50 px-6 py-3">
        <h1 className="text-xl font-bold text-white">{t('kanban.title')}</h1>
        <div className="text-sm text-slate-400">
          {tasks.length} {t('kanban.totalTasks')}
        </div>
      </div>

      {/* Board */}
      <div className="flex flex-1 gap-3 overflow-x-auto p-4">
        {COLUMNS.map(col => {
          const colTasks = tasksByStatus(col.id);
          const isDragOver = dragOverCol === col.id;

          return (
            <div
              key={col.id}
              className={`flex w-72 flex-shrink-0 flex-col rounded-xl border transition-colors ${
                isDragOver
                  ? 'border-blue-500 bg-blue-500/5'
                  : 'border-slate-700/50 bg-slate-800/30'
              }`}
              onDragOver={e => handleDragOver(e, col.id)}
              onDragLeave={handleDragLeave}
              onDrop={e => handleDrop(e, col.id)}
            >
              {/* Column header */}
              <div className="flex items-center justify-between px-3 py-2.5">
                <div className="flex items-center gap-2">
                  <div className={`h-2.5 w-2.5 rounded-full ${col.color}`} />
                  <span className="text-sm font-semibold text-white">{t(col.labelKey)}</span>
                  <span className="rounded-full bg-slate-700 px-1.5 py-0.5 text-[10px] text-slate-400">
                    {colTasks.length}
                  </span>
                </div>
              </div>

              {/* Cards */}
              <div className="flex-1 space-y-2 overflow-y-auto px-2 pb-2">
                {colTasks.map(task => (
                  <TaskCard
                    key={task.id}
                    task={task}
                    onDragStart={handleDragStart}
                    onClick={setDetailTask}
                  />
                ))}

                {/* Inline add */}
                {addingIn === col.id ? (
                  <InlineAddForm
                    onAdd={(title, desc, pri) => handleAdd(col.id, title, desc, pri)}
                    onCancel={() => setAddingIn(null)}
                  />
                ) : (
                  <button
                    onClick={() => setAddingIn(col.id)}
                    className="flex w-full items-center justify-center gap-1 rounded-lg border border-dashed border-slate-700 py-2 text-xs text-slate-500 hover:border-slate-500 hover:text-slate-300"
                  >
                    <Plus size={14} />
                    {t('kanban.addTask')}
                  </button>
                )}
              </div>
            </div>
          );
        })}
      </div>

      {/* Detail panel */}
      {detailTask && (
        <>
          <div className="fixed inset-0 z-40 bg-black/30" onClick={() => setDetailTask(null)} />
          <DetailPanel
            task={detailTask}
            onUpdate={handleUpdate}
            onClose={() => setDetailTask(null)}
            onDelete={handleDelete}
          />
        </>
      )}
    </div>
  );
}
