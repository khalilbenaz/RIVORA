export default function Spinner({ className = '' }: { className?: string }) {
  return (
    <div className={`flex items-center justify-center py-12 ${className}`}>
      <div className="h-8 w-8 animate-spin rounded-full border-3 border-slate-200 border-t-blue-500" />
    </div>
  );
}
