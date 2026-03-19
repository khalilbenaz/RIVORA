import { useState } from 'react';
import { ChevronDown, ChevronRight, CheckCircle2, XCircle, Loader2, Clock, SkipForward, AlertTriangle } from 'lucide-react';
import type { FlowRun, FlowRunStep } from '../../api/flows';
import { useTranslation } from 'react-i18next';

interface FlowRunHistoryProps {
  runs: FlowRun[];
}

const statusConfig = {
  running: { icon: Loader2, color: 'text-blue-500', bg: 'bg-blue-50', label: 'Running' },
  completed: { icon: CheckCircle2, color: 'text-green-500', bg: 'bg-green-50', label: 'Completed' },
  failed: { icon: XCircle, color: 'text-red-500', bg: 'bg-red-50', label: 'Failed' },
};

const stepStatusConfig: Record<FlowRunStep['status'], { icon: React.ElementType; color: string }> = {
  pending: { icon: Clock, color: 'text-slate-400' },
  running: { icon: Loader2, color: 'text-blue-500' },
  completed: { icon: CheckCircle2, color: 'text-green-500' },
  failed: { icon: XCircle, color: 'text-red-500' },
  skipped: { icon: SkipForward, color: 'text-slate-400' },
};

function formatDuration(ms?: number) {
  if (!ms) return '-';
  if (ms < 1000) return `${ms}ms`;
  return `${(ms / 1000).toFixed(1)}s`;
}

function formatTime(iso: string) {
  return new Date(iso).toLocaleString();
}

export default function FlowRunHistory({ runs }: FlowRunHistoryProps) {
  const { t } = useTranslation();
  const [expandedRunId, setExpandedRunId] = useState<string | null>(null);

  if (runs.length === 0) {
    return (
      <div className="px-4 py-6 text-center text-sm text-slate-400">
        {t('flows.noRuns')}
      </div>
    );
  }

  return (
    <div className="space-y-2">
      <h3 className="mb-3 text-sm font-semibold text-slate-700 dark:text-slate-200">{t('flows.runHistory')}</h3>
      {runs.map((run) => {
        const cfg = statusConfig[run.status];
        const StatusIcon = cfg.icon;
        const isExpanded = expandedRunId === run.id;
        const duration = run.completedAt
          ? new Date(run.completedAt).getTime() - new Date(run.startedAt).getTime()
          : undefined;

        return (
          <div key={run.id} className="rounded-lg border border-slate-200 bg-white dark:border-slate-700 dark:bg-slate-800">
            <button
              onClick={() => setExpandedRunId(isExpanded ? null : run.id)}
              className="flex w-full items-center gap-3 px-4 py-3 text-left"
            >
              {isExpanded ? <ChevronDown size={14} className="text-slate-400" /> : <ChevronRight size={14} className="text-slate-400" />}
              <span className={`flex items-center gap-1.5 rounded-full px-2 py-0.5 text-xs font-medium ${cfg.bg} ${cfg.color}`}>
                <StatusIcon size={12} className={run.status === 'running' ? 'animate-spin' : ''} />
                {cfg.label}
              </span>
              <span className="flex-1 text-xs text-slate-500 dark:text-slate-400">
                {formatTime(run.startedAt)}
              </span>
              {duration !== undefined && (
                <span className="text-xs text-slate-400">{formatDuration(duration)}</span>
              )}
            </button>

            {isExpanded && (
              <div className="border-t border-slate-200 px-4 py-3 dark:border-slate-700">
                {run.error && (
                  <div className="mb-3 flex items-start gap-2 rounded-lg bg-red-50 px-3 py-2 text-xs text-red-700 dark:bg-red-900/20 dark:text-red-400">
                    <AlertTriangle size={14} className="mt-0.5 shrink-0" />
                    {run.error}
                  </div>
                )}
                <div className="space-y-2">
                  {run.steps.map((step, idx) => {
                    const stepCfg = stepStatusConfig[step.status];
                    const StepIcon = stepCfg.icon;
                    return (
                      <div key={idx} className={`rounded-lg border px-3 py-2 ${step.status === 'failed' ? 'border-red-200 bg-red-50 dark:border-red-800 dark:bg-red-900/10' : 'border-slate-100 dark:border-slate-700'}`}>
                        <div className="flex items-center gap-2">
                          <StepIcon size={14} className={`${stepCfg.color} ${step.status === 'running' ? 'animate-spin' : ''}`} />
                          <span className="flex-1 text-sm font-medium text-slate-700 dark:text-slate-300">{step.nodeLabel}</span>
                          {step.duration !== undefined && (
                            <span className="text-xs text-slate-400">{formatDuration(step.duration)}</span>
                          )}
                        </div>
                        {step.error && (
                          <div className="mt-1 text-xs text-red-600 dark:text-red-400">{step.error}</div>
                        )}
                        {step.output && (
                          <div className="mt-1 truncate text-xs text-slate-500 dark:text-slate-400">
                            {t('flows.output')}: {step.output}
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}
