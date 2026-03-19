import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import Badge from '../Badge';

describe('Badge', () => {
  it('renders children text', () => {
    render(<Badge>Active</Badge>);
    expect(screen.getByText('Active')).toBeInTheDocument();
  });

  it('applies neutral variant by default', () => {
    render(<Badge>Default</Badge>);
    const el = screen.getByText('Default');
    expect(el.className).toContain('bg-slate-100');
    expect(el.className).toContain('text-slate-600');
  });

  it('applies success variant classes (emerald)', () => {
    render(<Badge variant="success">OK</Badge>);
    const el = screen.getByText('OK');
    expect(el.className).toContain('bg-emerald-100');
    expect(el.className).toContain('text-emerald-700');
  });

  it('applies danger variant classes (red)', () => {
    render(<Badge variant="danger">Error</Badge>);
    const el = screen.getByText('Error');
    expect(el.className).toContain('bg-red-100');
    expect(el.className).toContain('text-red-700');
  });

  it('applies warning variant classes (amber)', () => {
    render(<Badge variant="warning">Warn</Badge>);
    const el = screen.getByText('Warn');
    expect(el.className).toContain('bg-amber-100');
    expect(el.className).toContain('text-amber-700');
  });

  it('applies info variant classes (blue)', () => {
    render(<Badge variant="info">Info</Badge>);
    const el = screen.getByText('Info');
    expect(el.className).toContain('bg-blue-100');
    expect(el.className).toContain('text-blue-700');
  });

  it('renders as a span with rounded-full class', () => {
    render(<Badge>Tag</Badge>);
    const el = screen.getByText('Tag');
    expect(el.tagName).toBe('SPAN');
    expect(el.className).toContain('rounded-full');
  });
});
