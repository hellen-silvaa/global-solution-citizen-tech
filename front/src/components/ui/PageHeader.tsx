import type { ReactNode } from 'react';
import { motion } from 'motion/react';

export function PageHeader({ selo, titulo, descricao, acoes }: { selo?: ReactNode; titulo: ReactNode; descricao?: ReactNode; acoes?: ReactNode }) {
  return (
    <div className="relative overflow-hidden border-b border-[var(--border)] bg-[var(--surface)]">
      <div className="grid-bg pointer-events-none absolute inset-0 opacity-70" aria-hidden />
      <div className="pointer-events-none absolute -right-24 -top-24 size-72 rounded-full bg-brand-400/20 blur-3xl" aria-hidden />
      <div className="pointer-events-none absolute -left-20 bottom-0 size-56 rounded-full bg-sun-400/15 blur-3xl" aria-hidden />
      <motion.div
        initial={{ opacity: 0, y: 12 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.4 }}
        className="relative mx-auto flex max-w-7xl flex-col gap-5 px-4 py-10 sm:px-6 md:flex-row md:items-end md:justify-between lg:py-14"
      >
        <div className="max-w-2xl">
          {selo && <div className="mb-3 text-sm font-semibold text-brand-600 dark:text-brand-300">{selo}</div>}
          <h1 className="text-3xl font-extrabold sm:text-4xl">{titulo}</h1>
          {descricao && <p className="muted mt-3 text-base leading-relaxed sm:text-lg">{descricao}</p>}
        </div>
        {acoes && <div className="flex flex-wrap gap-2">{acoes}</div>}
      </motion.div>
    </div>
  );
}
