import type { AreaConhecimento } from '../api/types';
import { AREA_INFO } from '../lib/labels';
import { cn } from '../lib/cn';

export function AreaIcon({ area, className = 'size-11' }: { area: AreaConhecimento; className?: string }) {
  const { icone: Icone, cor, rotulo } = AREA_INFO[area];
  return (
    <span
      className={cn('grid shrink-0 place-items-center rounded-2xl bg-gradient-to-br text-white shadow-md', cor, className)}
      title={rotulo}
    >
      <Icone className="size-[48%]" aria-hidden />
    </span>
  );
}
