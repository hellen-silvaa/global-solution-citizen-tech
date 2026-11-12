import { Check, X } from 'lucide-react';
import { avaliarSenha } from '../lib/senha';
import { cn } from '../lib/cn';

const CORES = ['bg-rose-500', 'bg-rose-500', 'bg-amber-500', 'bg-emerald-500', 'bg-emerald-600'];

export function PasswordStrength({ senha }: { senha: string }) {
  const forca = avaliarSenha(senha);
  if (!senha) return null;
  return (
    <div className="space-y-2" aria-live="polite">
      <div className="flex items-center gap-2">
        <div className="flex flex-1 gap-1" aria-hidden>
          {[1, 2, 3, 4].map((n) => (
            <span
              key={n}
              className={cn('h-1.5 flex-1 rounded-full transition-colors duration-300', forca.pontos >= n ? CORES[forca.pontos] : 'bg-[var(--surface-2)]')}
            />
          ))}
        </div>
        <span className="text-xs font-semibold">Senha {forca.rotulo.toLowerCase()}</span>
      </div>
      <ul className="grid gap-1 text-xs sm:grid-cols-2">
        {forca.requisitos.map((r) => (
          <li key={r.texto} className={cn('flex items-center gap-1.5', r.ok ? 'text-emerald-700 dark:text-emerald-400' : 'muted')}>
            {r.ok ? <Check className="size-3.5" aria-hidden /> : <X className="size-3.5" aria-hidden />}
            {r.texto}
            <span className="sr-only">{r.ok ? '(atendido)' : '(pendente)'}</span>
          </li>
        ))}
      </ul>
    </div>
  );
}
