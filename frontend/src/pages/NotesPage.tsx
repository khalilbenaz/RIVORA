import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pin, Trash2, Search, X } from 'lucide-react';
import type { Note } from '../api/notes';

type NoteColor = Note['color'];

const NOTE_COLORS: NoteColor[] = ['yellow', 'blue', 'green', 'pink', 'purple'];

const COLOR_BG: Record<NoteColor, string> = {
  yellow: 'bg-yellow-50 border-yellow-200 dark:bg-yellow-900/20 dark:border-yellow-800',
  blue: 'bg-blue-50 border-blue-200 dark:bg-blue-900/20 dark:border-blue-800',
  green: 'bg-green-50 border-green-200 dark:bg-green-900/20 dark:border-green-800',
  pink: 'bg-pink-50 border-pink-200 dark:bg-pink-900/20 dark:border-pink-800',
  purple: 'bg-purple-50 border-purple-200 dark:bg-purple-900/20 dark:border-purple-800',
};

const COLOR_DOT: Record<NoteColor, string> = {
  yellow: 'bg-yellow-400',
  blue: 'bg-blue-400',
  green: 'bg-green-400',
  pink: 'bg-pink-400',
  purple: 'bg-purple-400',
};

function generateMockNotes(): Note[] {
  const titles = [
    'Sprint goals', 'API refactoring ideas', 'Meeting notes', 'Bug tracker',
    'Design feedback', 'Deployment checklist', 'Tech debt items', 'Feature requests',
    'Performance metrics', 'Security audit notes',
  ];
  const contents = [
    'Complete the user authentication flow and add rate limiting to all public endpoints.',
    'Refactor the data layer to use repository pattern. Consider adding caching with Redis.',
    'Discussed new dashboard layout. Team prefers sidebar navigation over top nav.',
    'Fix the pagination issue on mobile. The scroll position resets when loading more items.',
    'Colors need more contrast for accessibility. Use WCAG AA standard minimum.',
    'Run migrations, update env vars, clear CDN cache, notify stakeholders.',
    'Replace legacy date library, upgrade to latest TypeScript, fix ESLint warnings.',
    'Users want dark mode, export to PDF, and keyboard shortcuts for power users.',
    'Page load time: 1.2s avg. Bundle size reduced by 15% after tree shaking.',
    'Review OAuth scopes, rotate API keys quarterly, add CSP headers.',
  ];
  return titles.map((title, i) => ({
    id: `note-${i}`,
    title,
    content: contents[i]!,
    color: NOTE_COLORS[i % NOTE_COLORS.length]!,
    pinned: i < 2,
    createdBy: 'admin',
    createdAt: new Date(Date.now() - i * 3600000 * 6).toISOString(),
    updatedAt: new Date(Date.now() - i * 3600000 * 3).toISOString(),
  }));
}

export default function NotesPage() {
  const { t } = useTranslation();
  const [notes, setNotes] = useState<Note[]>(generateMockNotes);
  const [search, setSearch] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);

  // Form state
  const [formTitle, setFormTitle] = useState('');
  const [formContent, setFormContent] = useState('');
  const [formColor, setFormColor] = useState<NoteColor>('yellow');

  const filtered = useMemo(() => {
    let result = [...notes];
    if (search) {
      const s = search.toLowerCase();
      result = result.filter(
        (n) => n.title.toLowerCase().includes(s) || n.content.toLowerCase().includes(s)
      );
    }
    // Pinned first
    result.sort((a, b) => (a.pinned === b.pinned ? 0 : a.pinned ? -1 : 1));
    return result;
  }, [notes, search]);

  const handleCreate = () => {
    if (!formTitle.trim()) return;
    const newNote: Note = {
      id: `note-${Date.now()}`,
      title: formTitle,
      content: formContent,
      color: formColor,
      pinned: false,
      createdBy: 'admin',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };
    setNotes((prev) => [newNote, ...prev]);
    resetForm();
  };

  const handleUpdate = (id: string) => {
    setNotes((prev) =>
      prev.map((n) =>
        n.id === id
          ? { ...n, title: formTitle, content: formContent, color: formColor, updatedAt: new Date().toISOString() }
          : n
      )
    );
    resetForm();
  };

  const handleDelete = (id: string) => {
    if (!confirm(t('notes.confirmDelete'))) return;
    setNotes((prev) => prev.filter((n) => n.id !== id));
    if (editingId === id) resetForm();
  };

  const handleTogglePin = (id: string) => {
    setNotes((prev) => prev.map((n) => (n.id === id ? { ...n, pinned: !n.pinned } : n)));
  };

  const startEdit = (note: Note) => {
    setEditingId(note.id);
    setFormTitle(note.title);
    setFormContent(note.content);
    setFormColor(note.color);
    setShowForm(true);
  };

  const resetForm = () => {
    setShowForm(false);
    setEditingId(null);
    setFormTitle('');
    setFormContent('');
    setFormColor('yellow');
  };

  const relativeTime = (dateStr: string) => {
    const diff = Date.now() - new Date(dateStr).getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 1) return t('notes.justNow');
    if (mins < 60) return `${mins}m`;
    const hours = Math.floor(mins / 60);
    if (hours < 24) return `${hours}h`;
    const days = Math.floor(hours / 24);
    return `${days}d`;
  };

  return (
    <div>
      {/* Header */}
      <div className="mb-6 flex flex-wrap items-center justify-between gap-4">
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">{t('notes.title')}</h1>
        <button
          onClick={() => { resetForm(); setShowForm(true); }}
          className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700"
        >
          <Plus size={16} /> {t('notes.newNote')}
        </button>
      </div>

      {/* Search */}
      <div className="relative mb-6">
        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder={t('notes.searchPlaceholder')}
          className="w-full max-w-md rounded-lg border border-slate-300 py-2 pl-9 pr-3 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-slate-600 dark:bg-slate-700 dark:text-white"
        />
      </div>

      {/* Inline editor */}
      {showForm && (
        <div className="mb-6 rounded-xl border border-slate-200 bg-white p-5 shadow-sm dark:border-slate-700 dark:bg-slate-800">
          <div className="mb-4 flex items-center justify-between">
            <h3 className="font-semibold text-slate-800 dark:text-white">
              {editingId ? t('notes.editNote') : t('notes.newNote')}
            </h3>
            <button onClick={resetForm} className="text-slate-400 hover:text-slate-600"><X size={18} /></button>
          </div>
          <div className="space-y-3">
            <input
              type="text"
              placeholder={t('notes.titlePlaceholder')}
              value={formTitle}
              onChange={(e) => setFormTitle(e.target.value)}
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm font-semibold focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-slate-600 dark:bg-slate-700 dark:text-white"
            />
            <textarea
              placeholder={t('notes.contentPlaceholder')}
              value={formContent}
              onChange={(e) => setFormContent(e.target.value)}
              rows={4}
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-slate-600 dark:bg-slate-700 dark:text-white"
            />
            <div className="flex items-center gap-3">
              <span className="text-xs text-slate-500 dark:text-slate-400">{t('notes.color')}:</span>
              {NOTE_COLORS.map((c) => (
                <button
                  key={c}
                  onClick={() => setFormColor(c)}
                  className={`h-7 w-7 rounded-full ${COLOR_DOT[c]} transition ${formColor === c ? 'ring-2 ring-offset-2 ring-slate-400' : 'opacity-60 hover:opacity-100'}`}
                />
              ))}
            </div>
          </div>
          <div className="mt-4 flex gap-2">
            <button
              onClick={() => (editingId ? handleUpdate(editingId) : handleCreate())}
              disabled={!formTitle.trim()}
              className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700 disabled:opacity-50"
            >
              {editingId ? t('common.save') : t('common.create')}
            </button>
            <button onClick={resetForm} className="rounded-lg border border-slate-300 px-4 py-2 text-sm text-slate-600 transition hover:bg-slate-100 dark:border-slate-600 dark:text-slate-300">
              {t('common.cancel')}
            </button>
          </div>
        </div>
      )}

      {/* Masonry grid */}
      {filtered.length === 0 ? (
        <div className="py-12 text-center text-slate-400">{t('notes.noNotes')}</div>
      ) : (
        <div className="columns-1 gap-4 sm:columns-2 lg:columns-3">
          {filtered.map((note) => (
            <div
              key={note.id}
              className={`mb-4 break-inside-avoid rounded-xl border p-4 shadow-sm transition hover:shadow-md ${COLOR_BG[note.color]} cursor-pointer`}
              onClick={() => startEdit(note)}
            >
              <div className="mb-2 flex items-start justify-between">
                <h3 className="font-bold text-slate-800 dark:text-slate-100">{note.title}</h3>
                <div className="flex items-center gap-1">
                  <button
                    onClick={(e) => { e.stopPropagation(); handleTogglePin(note.id); }}
                    className={`rounded p-1 transition ${note.pinned ? 'text-amber-500' : 'text-slate-400 hover:text-amber-500'}`}
                    title={note.pinned ? t('notes.unpin') : t('notes.pin')}
                  >
                    <Pin size={14} className={note.pinned ? 'fill-current' : ''} />
                  </button>
                  <button
                    onClick={(e) => { e.stopPropagation(); handleDelete(note.id); }}
                    className="rounded p-1 text-slate-400 transition hover:text-red-500"
                    title={t('common.delete')}
                  >
                    <Trash2 size={14} />
                  </button>
                </div>
              </div>
              <p className="line-clamp-4 text-sm text-slate-600 dark:text-slate-300">{note.content}</p>
              <div className="mt-3 text-xs text-slate-400">
                {relativeTime(note.updatedAt)} {t('notes.ago')}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
