interface Props {
  children: React.ReactNode;
  variant?: 'success' | 'danger' | 'warning' | 'info' | 'neutral';
}

const styles = {
  success: 'bg-emerald-100 text-emerald-700',
  danger: 'bg-red-100 text-red-700',
  warning: 'bg-amber-100 text-amber-700',
  info: 'bg-blue-100 text-blue-700',
  neutral: 'bg-slate-100 text-slate-600',
};

export default function Badge({ children, variant = 'neutral' }: Props) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-semibold ${styles[variant]}`}
    >
      {children}
    </span>
  );
}
