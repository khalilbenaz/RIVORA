import { describe, it, expect } from 'vitest';
import { formatFileSize } from '../formatFileSize';

describe('formatFileSize', () => {
  it('returns "0 B" for 0 bytes', () => {
    expect(formatFileSize(0)).toBe('0 B');
  });

  it('returns "1.0 KB" for 1024 bytes', () => {
    expect(formatFileSize(1024)).toBe('1.0 KB');
  });

  it('returns "1.0 MB" for 1048576 bytes', () => {
    expect(formatFileSize(1048576)).toBe('1.0 MB');
  });

  it('returns "1.0 GB" for 1073741824 bytes', () => {
    expect(formatFileSize(1073741824)).toBe('1.0 GB');
  });

  it('returns "512.0 B" for 512 bytes', () => {
    expect(formatFileSize(512)).toBe('512.0 B');
  });

  it('returns "1.5 KB" for 1500 bytes', () => {
    expect(formatFileSize(1500)).toBe('1.5 KB');
  });
});
