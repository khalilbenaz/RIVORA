import { useState } from 'react';
import Badge from '../components/Badge';
import StatCard from '../components/StatCard';
import Spinner from '../components/Spinner';
import Pagination from '../components/Pagination';
import FormField from '../components/FormField';
import TableSkeleton from '../components/TableSkeleton';
import { Users, ShoppingCart, Activity, AlertTriangle } from 'lucide-react';

type ToastType = 'success' | 'error' | 'warning' | 'info' | null;

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section className="mb-10">
      <h2 className="mb-4 text-lg font-semibold text-slate-800 border-b border-slate-200 pb-2">{title}</h2>
      {children}
    </section>
  );
}

const toastStyles: Record<string, string> = {
  success: 'bg-emerald-50 border-emerald-300 text-emerald-800',
  error: 'bg-red-50 border-red-300 text-red-800',
  warning: 'bg-amber-50 border-amber-300 text-amber-800',
  info: 'bg-blue-50 border-blue-300 text-blue-800',
};

export default function ComponentShowcase() {
  const [page, setPage] = useState(3);
  const [formValue, setFormValue] = useState('Hello');
  const [formError, setFormError] = useState('');
  const [formDisabled] = useState('Disabled value');
  const [toast, setToast] = useState<ToastType>(null);

  const showToast = (type: ToastType) => {
    setToast(type);
    setTimeout(() => setToast(null), 2500);
  };

  return (
    <div className="min-h-screen bg-slate-50 p-6 md:p-10">
      <div className="mx-auto max-w-5xl">
        <h1 className="mb-2 text-2xl font-bold text-slate-900">Component Showcase</h1>
        <p className="mb-8 text-sm text-slate-500">All UI components with their variants and states.</p>

        {/* Toast notification */}
        {toast && (
          <div className={`fixed top-6 right-6 z-50 rounded-lg border px-4 py-3 text-sm font-medium shadow-lg transition-all ${toastStyles[toast]}`}>
            {toast === 'success' && 'Operation completed successfully!'}
            {toast === 'error' && 'Something went wrong.'}
            {toast === 'warning' && 'Please check your input.'}
            {toast === 'info' && 'Here is some useful information.'}
          </div>
        )}

        {/* Badges */}
        <Section title="Badges">
          <div className="flex flex-wrap items-center gap-3">
            <Badge variant="success">Success</Badge>
            <Badge variant="danger">Danger</Badge>
            <Badge variant="warning">Warning</Badge>
            <Badge variant="info">Info</Badge>
            <Badge variant="neutral">Neutral</Badge>
          </div>
        </Section>

        {/* StatCards */}
        <Section title="StatCards">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <StatCard label="Total Users" value={1284} detail="+12% this month" icon={<Users size={18} />} />
            <StatCard label="Revenue" value="$48.2k" detail="+8.1% this month" icon={<ShoppingCart size={18} />} variant="success" />
            <StatCard label="Errors" value={23} detail="3 critical" icon={<AlertTriangle size={18} />} variant="danger" />
            <StatCard label="Uptime" value="99.7%" detail="Last 30 days" icon={<Activity size={18} />} variant="warning" />
          </div>
        </Section>

        {/* Spinner */}
        <Section title="Spinner">
          <div className="rounded-lg border border-slate-200 bg-white">
            <Spinner />
          </div>
        </Section>

        {/* Pagination */}
        <Section title="Pagination">
          <div className="rounded-lg border border-slate-200 bg-white">
            <Pagination total={100} page={page} pageSize={10} onPageChange={setPage} />
          </div>
        </Section>

        {/* FormField */}
        <Section title="FormField">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3 rounded-lg border border-slate-200 bg-white p-6">
            <FormField
              label="Normal Field"
              value={formValue}
              onChange={setFormValue}
              placeholder="Type something..."
            />
            <FormField
              label="With Error"
              value={formError}
              onChange={setFormError}
              error="This field is required"
              placeholder="Has an error"
            />
            <div className="opacity-50 pointer-events-none">
              <FormField
                label="Disabled"
                value={formDisabled}
                onChange={() => {}}
                placeholder="Cannot edit"
              />
            </div>
          </div>
        </Section>

        {/* TableSkeleton */}
        <Section title="TableSkeleton">
          <TableSkeleton columns={5} rows={3} />
        </Section>

        {/* Buttons */}
        <Section title="Buttons">
          <div className="flex flex-wrap items-center gap-3">
            <button className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-blue-700">
              Primary
            </button>
            <button className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm transition hover:bg-slate-50">
              Outline
            </button>
            <button className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-red-700">
              Danger
            </button>
            <button
              disabled
              className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow-sm opacity-50 cursor-not-allowed"
            >
              Disabled
            </button>
          </div>
        </Section>

        {/* Toast Preview */}
        <Section title="Toast Preview">
          <div className="flex flex-wrap items-center gap-3">
            <button
              onClick={() => showToast('success')}
              className="rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-emerald-700"
            >
              Success Toast
            </button>
            <button
              onClick={() => showToast('error')}
              className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-red-700"
            >
              Error Toast
            </button>
            <button
              onClick={() => showToast('warning')}
              className="rounded-lg bg-amber-500 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-amber-600"
            >
              Warning Toast
            </button>
            <button
              onClick={() => showToast('info')}
              className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-blue-700"
            >
              Info Toast
            </button>
          </div>
        </Section>
      </div>
    </div>
  );
}
