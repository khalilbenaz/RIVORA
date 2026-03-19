import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { useToastStore } from '../toastStore';

describe('toastStore', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    useToastStore.setState({ toasts: [] });
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('has empty toasts array initially', () => {
    const state = useToastStore.getState();
    expect(state.toasts).toEqual([]);
  });

  it('addToast adds a toast with generated id', () => {
    useToastStore.getState().addToast({ message: 'Hello', type: 'success' });

    const state = useToastStore.getState();
    expect(state.toasts).toHaveLength(1);
    const first = state.toasts[0]!;
    expect(first.message).toBe('Hello');
    expect(first.id).toBeDefined();
    expect(typeof first.id).toBe('string');
  });

  it('removeToast removes the correct toast', () => {
    useToastStore.getState().addToast({ message: 'First', type: 'info' });
    useToastStore.getState().addToast({ message: 'Second', type: 'error' });

    const toasts = useToastStore.getState().toasts;
    expect(toasts).toHaveLength(2);

    useToastStore.getState().removeToast(toasts[0]!.id);

    const remaining = useToastStore.getState().toasts;
    expect(remaining).toHaveLength(1);
    expect(remaining[0]!.message).toBe('Second');
  });

  it('multiple toasts can coexist', () => {
    useToastStore.getState().addToast({ message: 'A', type: 'success' });
    useToastStore.getState().addToast({ message: 'B', type: 'error' });
    useToastStore.getState().addToast({ message: 'C', type: 'warning' });

    const state = useToastStore.getState();
    expect(state.toasts).toHaveLength(3);
    expect(state.toasts.map((t) => t.message)).toEqual(['A', 'B', 'C']);
  });

  it('preserves toast type (success, error, warning, info)', () => {
    useToastStore.getState().addToast({ message: 's', type: 'success' });
    useToastStore.getState().addToast({ message: 'e', type: 'error' });
    useToastStore.getState().addToast({ message: 'w', type: 'warning' });
    useToastStore.getState().addToast({ message: 'i', type: 'info' });

    const types = useToastStore.getState().toasts.map((t) => t.type);
    expect(types).toEqual(['success', 'error', 'warning', 'info']);
  });
});
