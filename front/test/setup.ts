import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/react';
import { afterEach } from 'vitest';

afterEach(() => {
  cleanup();
  localStorage.clear();
});

if (!window.matchMedia) {
  window.matchMedia = (query: string) =>
    ({
      matches: false,
      media: query,
      onchange: null,
      addListener: () => {},
      removeListener: () => {},
      addEventListener: () => {},
      removeEventListener: () => {},
      dispatchEvent: () => false,
    }) as MediaQueryList;
}
class IO {
  observe() {}
  unobserve() {}
  disconnect() {}
  takeRecords() { return []; }
  root = null; rootMargin = ''; thresholds = [];
}
class RO { observe() {} unobserve() {} disconnect() {} }
// jsdom não tem IntersectionObserver nem ResizeObserver (usados por motion e Recharts).
const w = window as unknown as Record<string, unknown>;
w.IntersectionObserver ??= IO;
w.ResizeObserver ??= RO;
window.scrollTo = () => {};
