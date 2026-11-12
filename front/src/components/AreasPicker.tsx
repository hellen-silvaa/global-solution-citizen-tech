import { AREAS, type AreaConhecimento } from '../api/types';
import { AREA_INFO } from '../lib/labels';
import { cn } from '../lib/cn';

/** Seleção múltipla de áreas de interesse em "chips" (checkboxes acessíveis). */
export function AreasPicker({ valor, aoMudar, erro }: { valor: AreaConhecimento[]; aoMudar: (v: AreaConhecimento[]) => void; erro?: string }) {
  function alternar(area: AreaConhecimento) {
    aoMudar(valor.includes(area) ? valor.filter((a) => a !== area) : [...valor, area]);
  }
  return (
    <fieldset>
      <legend className="text-sm font-semibold">Áreas de interesse</legend>
      <p className="muted mt-0.5 text-xs">Escolha uma ou mais: usamos isso para recomendar oportunidades.</p>
      <div className="mt-3 flex flex-wrap gap-2">
        {AREAS.map((area) => {
          const { rotulo, icone: Icone } = AREA_INFO[area];
          const ativo = valor.includes(area);
          return (
            <label
              key={area}
              className={cn(
                'flex cursor-pointer items-center gap-1.5 rounded-full border px-3 py-1.5 text-sm font-medium transition-all has-[:focus-visible]:ring-4 has-[:focus-visible]:ring-brand-500/30',
                ativo
                  ? 'border-brand-500 bg-brand-600 text-white shadow-md shadow-brand-600/20'
                  : 'border-[var(--border)] bg-[var(--surface)] hover:border-brand-300',
              )}
            >
              <input type="checkbox" className="sr-only" checked={ativo} onChange={() => alternar(area)} />
              <Icone className="size-4" aria-hidden />
              {rotulo}
            </label>
          );
        })}
      </div>
      {erro && (
        <p className="mt-2 text-sm font-medium text-rose-600 dark:text-rose-400" role="alert">
          {erro}
        </p>
      )}
    </fieldset>
  );
}
