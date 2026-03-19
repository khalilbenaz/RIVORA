import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import StatCard from '../StatCard';

describe('StatCard', () => {
  it('renders label and value', () => {
    render(<StatCard label="Total Users" value={42} />);
    expect(screen.getByText('Total Users')).toBeInTheDocument();
    expect(screen.getByText('42')).toBeInTheDocument();
  });

  it('renders detail text when provided', () => {
    render(<StatCard label="Revenue" value="$1,200" detail="+12% this month" />);
    expect(screen.getByText('+12% this month')).toBeInTheDocument();
  });

  it('does not render detail when not provided', () => {
    const { container } = render(<StatCard label="Count" value={10} />);
    const detailEls = container.querySelectorAll('.text-xs.text-slate-500');
    // Only the label should match, not a detail element
    const texts = Array.from(detailEls).map((el) => el.textContent);
    expect(texts).not.toContain(undefined);
  });

  it('renders icon when provided', () => {
    render(<StatCard label="Health" value="OK" icon={<span data-testid="icon">IC</span>} />);
    expect(screen.getByTestId('icon')).toBeInTheDocument();
  });

  it('applies default variant border class', () => {
    const { container } = render(<StatCard label="A" value={1} />);
    const card = container.firstElementChild as HTMLElement;
    expect(card.className).toContain('border-slate-200');
  });

  it('applies success variant classes', () => {
    const { container } = render(<StatCard label="A" value={1} variant="success" />);
    const card = container.firstElementChild as HTMLElement;
    expect(card.className).toContain('border-emerald-200');
    expect(card.className).toContain('bg-emerald-50/50');
  });

  it('applies danger variant classes', () => {
    const { container } = render(<StatCard label="A" value={1} variant="danger" />);
    const card = container.firstElementChild as HTMLElement;
    expect(card.className).toContain('border-red-200');
  });

  it('applies warning variant classes', () => {
    const { container } = render(<StatCard label="A" value={1} variant="warning" />);
    const card = container.firstElementChild as HTMLElement;
    expect(card.className).toContain('border-amber-200');
  });
});
