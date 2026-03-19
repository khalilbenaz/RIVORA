import { useToastStore } from '../store/toastStore';
import type { Toast } from '../store/toastStore';
import { CheckCircle, XCircle, AlertTriangle, Info, X } from 'lucide-react';

const iconMap: Record<Toast['type'], typeof CheckCircle> = {
  success: CheckCircle,
  error: XCircle,
  warning: AlertTriangle,
  info: Info,
};

const borderColorMap: Record<Toast['type'], string> = {
  success: '#22c55e',
  error: '#ef4444',
  warning: '#f59e0b',
  info: '#3b82f6',
};

const iconColorMap: Record<Toast['type'], string> = {
  success: '#16a34a',
  error: '#dc2626',
  warning: '#d97706',
  info: '#2563eb',
};

export default function ToastContainer() {
  const toasts = useToastStore((s) => s.toasts);
  const removeToast = useToastStore((s) => s.removeToast);

  if (toasts.length === 0) return null;

  return (
    <div
      style={{
        position: 'fixed',
        bottom: 24,
        right: 24,
        zIndex: 9999,
        display: 'flex',
        flexDirection: 'column',
        gap: 12,
        pointerEvents: 'none',
      }}
    >
      {toasts.map((toast) => {
        const Icon = iconMap[toast.type];
        return (
          <div
            key={toast.id}
            style={{
              pointerEvents: 'auto',
              display: 'flex',
              alignItems: 'center',
              gap: 12,
              padding: '12px 16px',
              backgroundColor: '#ffffff',
              borderRadius: 8,
              boxShadow: '0 4px 12px rgba(0,0,0,0.12)',
              borderLeft: `4px solid ${borderColorMap[toast.type]}`,
              minWidth: 300,
              maxWidth: 420,
              animation: 'toast-slide-in 0.3s ease-out',
            }}
          >
            <Icon size={18} color={iconColorMap[toast.type]} style={{ flexShrink: 0 }} />
            <span style={{ flex: 1, fontSize: 14, color: '#1e293b' }}>{toast.message}</span>
            <button
              onClick={() => removeToast(toast.id)}
              style={{
                background: 'none',
                border: 'none',
                cursor: 'pointer',
                padding: 4,
                color: '#94a3b8',
                flexShrink: 0,
              }}
              aria-label="Fermer"
            >
              <X size={14} />
            </button>
          </div>
        );
      })}
      <style>{`
        @keyframes toast-slide-in {
          from {
            opacity: 0;
            transform: translateX(100%);
          }
          to {
            opacity: 1;
            transform: translateX(0);
          }
        }
      `}</style>
    </div>
  );
}
