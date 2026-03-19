interface BarChartProps {
  data: { label: string; value: number }[];
  height?: number;
  color?: string;
}

export default function BarChart({ data, height = 220, color = '#3b82f6' }: BarChartProps) {
  const maxValue = Math.max(...data.map((d) => d.value), 1);
  const padding = { top: 24, right: 12, bottom: 36, left: 12 };
  const chartHeight = height - padding.top - padding.bottom;

  return (
    <svg width="100%" height={height} viewBox={`0 0 ${data.length * 64 + padding.left + padding.right} ${height}`} preserveAspectRatio="xMidYMid meet">
      {/* Grid lines */}
      {[0, 0.25, 0.5, 0.75, 1].map((ratio) => {
        const y = padding.top + chartHeight * (1 - ratio);
        return (
          <line
            key={ratio}
            x1={padding.left}
            y1={y}
            x2={data.length * 64 + padding.left}
            y2={y}
            stroke="#e2e8f0"
            strokeDasharray={ratio === 0 ? undefined : '4 4'}
          />
        );
      })}

      {data.map((d, i) => {
        const barWidth = 36;
        const gap = 64;
        const x = padding.left + i * gap + (gap - barWidth) / 2;
        const barHeight = (d.value / maxValue) * chartHeight;
        const y = padding.top + chartHeight - barHeight;

        return (
          <g key={d.label}>
            <rect
              x={x}
              y={y}
              width={barWidth}
              height={barHeight}
              fill={color}
              rx={4}
              className="transition-all duration-200"
              opacity={0.85}
            />
            <text
              x={x + barWidth / 2}
              y={y - 6}
              textAnchor="middle"
              className="fill-slate-600 text-[10px] font-medium"
            >
              {d.value.toLocaleString()}
            </text>
            <text
              x={x + barWidth / 2}
              y={height - 8}
              textAnchor="middle"
              className="fill-slate-500 text-[10px]"
            >
              {d.label}
            </text>
          </g>
        );
      })}
    </svg>
  );
}
