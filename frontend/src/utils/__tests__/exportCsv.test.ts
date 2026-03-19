import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { exportToCsv } from '../exportCsv';

describe('exportToCsv', () => {
  let clickSpy: ReturnType<typeof vi.fn>;
  const OriginalBlob = globalThis.Blob;
  let lastBlobContent: string;

  beforeEach(() => {
    clickSpy = vi.fn();
    lastBlobContent = '';

    vi.spyOn(document, 'createElement').mockReturnValue({
      href: '',
      download: '',
      click: clickSpy,
    } as unknown as HTMLAnchorElement);

    URL.createObjectURL = vi.fn().mockReturnValue('blob:mock');
    URL.revokeObjectURL = vi.fn();

    // Replace Blob to capture content without recursion
    globalThis.Blob = class MockBlob {
      constructor(parts?: BlobPart[]) {
        if (parts) {
          lastBlobContent = (parts as string[]).join('');
        }
      }
    } as unknown as typeof Blob;
  });

  afterEach(() => {
    globalThis.Blob = OriginalBlob;
    vi.restoreAllMocks();
  });

  it('generates CSV with correct header and rows', () => {
    const data = [
      { name: 'Alice', age: '30' },
      { name: 'Bob', age: '25' },
    ];

    exportToCsv(data, 'test');

    expect(lastBlobContent).toContain('name,age');
    expect(lastBlobContent).toContain('Alice,30');
    expect(lastBlobContent).toContain('Bob,25');
  });

  it('sanitizes formula injection for values starting with =, +, -, @', () => {
    const data = [
      { val: '=SUM(A1)' },
      { val: '+cmd' },
      { val: '-danger' },
      { val: '@import' },
    ];

    exportToCsv(data, 'test');

    const lines = lastBlobContent.replace('\uFEFF', '').split('\n');
    expect(lines[1]).toBe("'=SUM(A1)");
    expect(lines[2]).toBe("'+cmd");
    expect(lines[3]).toBe("'-danger");
    expect(lines[4]).toBe("'@import");
  });

  it('escapes commas and quotes in values', () => {
    const data = [{ val: 'hello, world' }, { val: 'say "hi"' }];

    exportToCsv(data, 'test');

    const lines = lastBlobContent.replace('\uFEFF', '').split('\n');
    expect(lines[1]).toBe('"hello, world"');
    expect(lines[2]).toBe('"say ""hi"""');
  });

  it('returns early without crashing for empty data', () => {
    expect(() => exportToCsv([], 'empty')).not.toThrow();
    expect(clickSpy).not.toHaveBeenCalled();
  });

  it('includes BOM prefix in output', () => {
    const data = [{ a: '1' }];

    exportToCsv(data, 'test');

    expect(lastBlobContent.startsWith('\uFEFF')).toBe(true);
  });
});
