import type { ReactNode } from 'react';
import type { LucideIcon } from 'lucide-react';

export function EmptyState({ icone: Icone, titulo, descricao, acao }: { icone: LucideIcon; titulo: string; descricao?: ReactNode; acao?: ReactNode }) {
  return (
    <div className="card flex flex-col items-center gap-3 px-6 py-12 text-center">
      <div className="grid size-14 place-items-center rounded-2xl bg-brand-100 text-brand-600 dark:bg-brand-500/15 dark:text-brand-300">
        <Icone className="size-7" aria-hidden />
      </div>
      <h3 className="text-lg font-bold">{titulo}</h3>
      {descricao && <p className="muted max-w-md text-sm leading-relaxed">{descricao}</p>}
      {acao && <div className="mt-2">{acao}</div>}
    </div>
  );
}
