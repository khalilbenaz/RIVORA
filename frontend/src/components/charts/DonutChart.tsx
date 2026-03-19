interface DonutChartProps {
  data: { label: string; value: number; color: string }[];
  size?: number;
}

export default function DonutChart({ data, size = 180 }: DonutChartProps) {
  const total = data.reduce((sum, d) => sum + d.value, 0);
  if (total === 0) return null;

  const cx = size / 2;
  const cy = size / 2;
  const outerRadius = size / 2 - 4;
  const innerRadius = outerRadius * 0.6;

  let cumulativeAngle = -Math.PI / 2;

  const arcs = data.map((d) => {
    const angle = (d.value / total) * 2 * Math.PI;
    const startAngle = cumulativeAngle;
    const endAngle = cumulativeAngle + angle;
    cumulativeAngle = endAngle;

    const largeArc = angle > Math.PI ? 1 : 0;

    const x1Outer = cx + outerRadius * Math.cos(startAngle);
    const y1Outer = cy + outerRadius * Math.sin(startAngle);
    const x2Outer = cx + outerRadius * Math.cos(endAngle);
    const y2Outer = cy + outerRadius * Math.sin(endAngle);

    const x1Inner = cx + innerRadius * Math.cos(endAngle);
    const y1Inner = cy + innerRadius * Math.sin(endAngle);
    const x2Inner = cx + innerRadius * Math.cos(startAngle);
    const y2Inner = cy + innerRadius * Math.sin(startAngle);

    const path = [
      `M ${x1Outer} ${y1Outer}`,
      `A ${outerRadius} ${outerRadius} 0 ${largeArc} 1 ${x2Outer} ${y2Outer}`,
      `L ${x1Inner} ${y1Inner}`,
      `A ${innerRadius} ${innerRadius} 0 ${largeArc} 0 ${x2Inner} ${y2Inner}`,
      'Z',
    ].join(' ');

    return { ...d, path };
  });

  return (
    <div className="flex flex-col items-center gap-4">
      <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`}>
        {arcs.map((arc, i) => (
          <path key={i} d={arc.path} fill={arc.color} className="transition-opacity hover:opacity-80" />
        ))}
        <text
          x={cx}
          y={cy - 6}
          textAnchor="middle"
          className="fill-slate-800 text-lg font-bold"
          dominantBaseline="central"
        >
          {total.toLocaleString()}
        </text>
        <text
          x={cx}
          y={cy + 14}
          textAnchor="middle"
          className="fill-slate-500 text-[10px]"
          dominantBaseline="central"
        >
          Total
        </text>
      </svg>

      {/* Legend */}
      <div className="flex flex-wrap justify-center gap-x-4 gap-y-1">
        {data.map((d) => (
          <div key={d.label} className="flex items-center gap-1.5 text-xs text-slate-600">
            <span
              className="inline-block h-2.5 w-2.5 rounded-full"
              style={{ backgroundColor: d.color }}
            />
            {d.label} ({Math.round((d.value / total) * 100)}%)
          </div>
        ))}
      </div>
    </div>
  );
}
