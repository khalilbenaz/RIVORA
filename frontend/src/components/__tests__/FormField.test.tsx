import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import FormField from '../FormField';

describe('FormField', () => {
  const defaultProps = {
    label: 'Email',
    value: '',
    onChange: vi.fn(),
  };

  it('renders label text', () => {
    render(<FormField {...defaultProps} />);
    expect(screen.getByText('Email')).toBeInTheDocument();
  });

  it('renders input with correct type', () => {
    render(<FormField {...defaultProps} type="password" />);
    const input = document.querySelector('input[type="password"]');
    expect(input).toBeInTheDocument();
  });

  it('shows error message when error prop is provided', () => {
    render(<FormField {...defaultProps} error="Required field" />);
    expect(screen.getByText('Required field')).toBeInTheDocument();
  });

  it('input has red border class when error', () => {
    render(<FormField {...defaultProps} error="Invalid" />);
    const input = document.querySelector('input');
    expect(input?.className).toContain('border-red-400');
  });

  it('calls onChange when typing', () => {
    const onChange = vi.fn();
    render(<FormField {...defaultProps} onChange={onChange} />);
    const input = document.querySelector('input')!;
    fireEvent.change(input, { target: { value: 'test@example.com' } });
    expect(onChange).toHaveBeenCalledWith('test@example.com');
  });

  it('shows required asterisk when required', () => {
    render(<FormField {...defaultProps} required />);
    expect(screen.getByText('*')).toBeInTheDocument();
  });
});
