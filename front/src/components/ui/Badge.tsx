import type { ReactNode } from 'react';
import { cn } from '../../lib/cn';

export type Tom = 'brand' | 'green' | 'amber' | 'red' | 'gray' | 'sky' | 'sun';

const TONS: Record<Tom, string> = {
  brand: 'bg-brand-100 text-brand-800 dark:bg-brand-500/15 dark:text-brand-200',
  green: 'bg-emerald-100 text-emerald-800 dark:bg-emerald-500/15 dark:text-emerald-300',
  amber: 'bg-amber-100 text-amber-900 dark:bg-amber-500/15 dark:text-amber-300',
  red: 'bg-rose-100 text-rose-800 dark:bg-rose-500/15 dark:text-rose-300',
  gray: 'bg-slate-100 text-slate-700 dark:bg-white/10 dark:text-slate-300',
  sky: 'bg-sky-100 text-sky-800 dark:bg-sky-500/15 dark:text-sky-300',
  sun: 'bg-orange-100 text-orange-800 dark:bg-orange-500/15 dark:text-orange-300',
};

export function Badge({ tom = 'gray', icone, children, className }: { tom?: Tom; icone?: ReactNode; children: ReactNode; className?: string }) {
  return (
    <span className={cn('inline-flex items-center gap-1 rounded-full px-2.5 py-1 text-xs font-semibold', TONS[tom], className)}>
      {icone}
      {children}
    </span>
  );
}

/** Status sempre com ponto + texto (nunca só cor). */
export function StatusBadge({ tom, children }: { tom: Tom; children: ReactNode }) {
  return (
    <Badge tom={tom}>
      <span className="size-1.5 rounded-full bg-current" aria-hidden />
      {children}
    </Badge>
  );
}
