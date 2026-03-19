import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import Pagination from '../Pagination';

describe('Pagination', () => {
  it('shows correct range text', () => {
    render(<Pagination total={50} page={1} pageSize={10} onPageChange={() => {}} />);
    expect(screen.getByText(/1–10 sur 50/)).toBeInTheDocument();
  });

  it('shows "Aucun résultat" when total is 0', () => {
    render(<Pagination total={0} page={1} pageSize={10} onPageChange={() => {}} />);
    expect(screen.getByText('Aucun résultat')).toBeInTheDocument();
  });

  it('shows correct page indicator', () => {
    render(<Pagination total={50} page={2} pageSize={10} onPageChange={() => {}} />);
    expect(screen.getByText('Page 2 / 5')).toBeInTheDocument();
  });

  it('disables previous button on first page', () => {
    render(<Pagination total={50} page={1} pageSize={10} onPageChange={() => {}} />);
    const prevBtn = screen.getByLabelText('Page précédente');
    expect(prevBtn).toBeDisabled();
  });

  it('disables next button on last page', () => {
    render(<Pagination total={50} page={5} pageSize={10} onPageChange={() => {}} />);
    const nextBtn = screen.getByLabelText('Page suivante');
    expect(nextBtn).toBeDisabled();
  });

  it('enables both buttons on middle pages', () => {
    render(<Pagination total={50} page={3} pageSize={10} onPageChange={() => {}} />);
    expect(screen.getByLabelText('Page précédente')).not.toBeDisabled();
    expect(screen.getByLabelText('Page suivante')).not.toBeDisabled();
  });

  it('calls onPageChange with page-1 when clicking previous', () => {
    const handler = vi.fn();
    render(<Pagination total={50} page={3} pageSize={10} onPageChange={handler} />);
    fireEvent.click(screen.getByLabelText('Page précédente'));
    expect(handler).toHaveBeenCalledWith(2);
  });

  it('calls onPageChange with page+1 when clicking next', () => {
    const handler = vi.fn();
    render(<Pagination total={50} page={3} pageSize={10} onPageChange={handler} />);
    fireEvent.click(screen.getByLabelText('Page suivante'));
    expect(handler).toHaveBeenCalledWith(4);
  });

  it('shows correct end range on last page with partial results', () => {
    render(<Pagination total={23} page={3} pageSize={10} onPageChange={() => {}} />);
    expect(screen.getByText(/21–23 sur 23/)).toBeInTheDocument();
  });
});
