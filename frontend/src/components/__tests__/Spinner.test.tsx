import { render } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import Spinner from '../Spinner';

describe('Spinner', () => {
  it('renders a spinner element with animate-spin class', () => {
    const { container } = render(<Spinner />);
    const spinner = container.querySelector('.animate-spin');
    expect(spinner).toBeInTheDocument();
  });

  it('renders the outer wrapper with flex centering', () => {
    const { container } = render(<Spinner />);
    const wrapper = container.firstElementChild as HTMLElement;
    expect(wrapper.className).toContain('flex');
    expect(wrapper.className).toContain('items-center');
    expect(wrapper.className).toContain('justify-center');
  });

  it('applies additional className when provided', () => {
    const { container } = render(<Spinner className="mt-8" />);
    const wrapper = container.firstElementChild as HTMLElement;
    expect(wrapper.className).toContain('mt-8');
  });
});
