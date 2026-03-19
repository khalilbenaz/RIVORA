interface Props {
  label: string;
  type?: string;
  value: string;
  onChange: (value: string) => void;
  error?: string;
  required?: boolean;
  placeholder?: string;
}

export default function FormField({ label, type = 'text', value, onChange, error, required, placeholder }: Props) {
  return (
    <div>
      <label className="mb-1 block text-sm font-medium text-slate-700">
        {label}
        {required && <span className="ml-0.5 text-red-500">*</span>}
      </label>
      <input
        type={type}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        required={required}
        placeholder={placeholder}
        className={`w-full rounded-lg border px-3 py-2.5 text-sm focus:outline-none focus:ring-2 ${
          error
            ? 'border-red-400 focus:border-red-500 focus:ring-red-500/20'
            : 'border-slate-300 focus:border-blue-500 focus:ring-blue-500/20'
        }`}
      />
      {error && <p className="mt-1 text-xs text-red-600">{error}</p>}
    </div>
  );
}
