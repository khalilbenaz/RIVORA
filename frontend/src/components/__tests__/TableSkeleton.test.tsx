import { render } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import TableSkeleton from '../TableSkeleton';

describe('TableSkeleton', () => {
  it('renders correct number of columns', () => {
    const { container } = render(<TableSkeleton columns={4} />);
    const headerCells = container.querySelectorAll('th');
    expect(headerCells).toHaveLength(4);
  });

  it('renders correct number of rows (default 5)', () => {
    const { container } = render(<TableSkeleton columns={3} />);
    const bodyRows = container.querySelectorAll('tbody tr');
    expect(bodyRows).toHaveLength(5);
  });

  it('renders custom rows count', () => {
    const { container } = render(<TableSkeleton columns={3} rows={8} />);
    const bodyRows = container.querySelectorAll('tbody tr');
    expect(bodyRows).toHaveLength(8);
  });

  it('has animate-pulse class for animation', () => {
    const { container } = render(<TableSkeleton columns={2} rows={1} />);
    const pulseElements = container.querySelectorAll('.animate-pulse');
    expect(pulseElements.length).toBeGreaterThan(0);
  });
});
