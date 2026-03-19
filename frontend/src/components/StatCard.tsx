import type { ReactNode } from 'react';

interface Props {
  label: string;
  value: string | number;
  detail?: string;
  icon?: ReactNode;
  variant?: 'default' | 'success' | 'danger' | 'warning';
}

const variants = {
  default: 'border-slate-200',
  success: 'border-emerald-200 bg-emerald-50/50',
  danger: 'border-red-200 bg-red-50/50',
  warning: 'border-amber-200 bg-amber-50/50',
};

export default function StatCard({ label, value, detail, icon, variant = 'default' }: Props) {
  return (
    <div
      className={`rounded-xl border bg-white p-5 shadow-sm transition-shadow hover:shadow-md ${variants[variant]}`}
    >
      <div className="flex items-center justify-between">
        <p className="text-xs font-semibold uppercase tracking-wider text-slate-500">{label}</p>
        {icon && <div className="text-slate-400">{icon}</div>}
      </div>
      <p className="mt-1 text-3xl font-bold text-slate-900 tabular-nums">{value}</p>
      {detail && <p className="mt-0.5 text-xs text-slate-500">{detail}</p>}
    </div>
  );
}
