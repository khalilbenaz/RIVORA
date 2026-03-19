interface LineChartProps {
  data: { label: string; value: number }[];
  height?: number;
  color?: string;
}

export default function LineChart({ data, height = 220, color = '#8b5cf6' }: LineChartProps) {
  if (data.length === 0) return null;

  const maxValue = Math.max(...data.map((d) => d.value), 1);
  const minValue = Math.min(...data.map((d) => d.value), 0);
  const range = maxValue - minValue || 1;
  const padding = { top: 20, right: 16, bottom: 36, left: 16 };
  const chartWidth = data.length * 28 + padding.left + padding.right;
  const chartHeight = height - padding.top - padding.bottom;

  const points = data.map((d, i) => {
    const x = padding.left + (i / (data.length - 1)) * (chartWidth - padding.left - padding.right);
    const y = padding.top + chartHeight - ((d.value - minValue) / range) * chartHeight;
    return { x, y, ...d };
  });

  const polyline = points.map((p) => `${p.x},${p.y}`).join(' ');

  // Area fill path
  const first = points[0]!;
  const last = points[points.length - 1]!;
  const areaPath = `M ${first.x},${padding.top + chartHeight} ${points.map((p) => `L ${p.x},${p.y}`).join(' ')} L ${last.x},${padding.top + chartHeight} Z`;

  // Show every Nth label to avoid overcrowding
  const labelInterval = Math.max(1, Math.floor(data.length / 7));

  return (
    <svg width="100%" height={height} viewBox={`0 0 ${chartWidth} ${height}`} preserveAspectRatio="xMidYMid meet">
      {/* Grid lines */}
      {[0, 0.25, 0.5, 0.75, 1].map((ratio) => {
        const y = padding.top + chartHeight * (1 - ratio);
        return (
          <line
            key={ratio}
            x1={padding.left}
            y1={y}
            x2={chartWidth - padding.right}
            y2={y}
            stroke="#e2e8f0"
            strokeDasharray={ratio === 0 ? undefined : '4 4'}
          />
        );
      })}

      {/* Area fill */}
      <path d={areaPath} fill={color} opacity={0.08} />

      {/* Line */}
      <polyline
        points={polyline}
        fill="none"
        stroke={color}
        strokeWidth={2.5}
        strokeLinecap="round"
        strokeLinejoin="round"
      />

      {/* Dots */}
      {points.map((p, i) => (
        <g key={i}>
          <circle cx={p.x} cy={p.y} r={3} fill="white" stroke={color} strokeWidth={2} />
          {i % labelInterval === 0 && (
            <text
              x={p.x}
              y={height - 8}
              textAnchor="middle"
              className="fill-slate-500 text-[9px]"
            >
              {p.label}
            </text>
          )}
        </g>
      ))}
    </svg>
  );
}
