// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function exportToCsv<T extends Record<string, any>>(
  data: T[],
  filename: string,
  columns?: { key: keyof T; label: string }[]
) {
  if (data.length === 0) return;

  const cols = columns ?? Object.keys(data[0]!).map((k) => ({ key: k as keyof T, label: String(k) }));

  const header = cols.map((c) => escapeCsv(c.label)).join(',');
  const rows = data.map((row) =>
    cols.map((c) => escapeCsv(String(row[c.key] ?? ''))).join(',')
  );

  const csv = [header, ...rows].join('\n');
  const blob = new Blob(['\uFEFF' + csv], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = `${filename}_${new Date().toISOString().slice(0, 10)}.csv`;
  link.click();
  URL.revokeObjectURL(url);
}

function escapeCsv(value: string): string {
  // Sanitize formula injection
  if (value.length > 0 && '=+-@\t\r'.includes(value[0]!)) {
    value = "'" + value;
  }
  if (value.includes('"') || value.includes(',') || value.includes('\n')) {
    return `"${value.replace(/"/g, '""')}"`;
  }
  return value;
}
