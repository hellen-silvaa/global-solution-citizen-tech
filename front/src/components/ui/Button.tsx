import { forwardRef, type ButtonHTMLAttributes, type ReactNode } from 'react';
import { Link, type LinkProps } from 'react-router';
import { Loader2 } from 'lucide-react';
import { cn } from '../../lib/cn';

type Variante = 'primario' | 'secundario' | 'fantasma' | 'perigo' | 'sol';
type Tamanho = 'sm' | 'md' | 'lg';

const BASE =
  'inline-flex items-center justify-center gap-2 rounded-xl font-semibold transition-all duration-200 select-none ' +
  'disabled:pointer-events-none disabled:opacity-55 active:scale-[0.97] whitespace-nowrap';

const VARIANTES: Record<Variante, string> = {
  primario:
    'bg-brand-600 text-white shadow-lg shadow-brand-600/25 hover:bg-brand-700 hover:shadow-brand-600/40 dark:bg-brand-500 dark:hover:bg-brand-400 dark:text-white',
  sol: 'bg-gradient-to-r from-sun-500 to-rose-500 text-white shadow-lg shadow-sun-500/30 hover:brightness-110',
  secundario:
    'border border-[var(--border)] bg-[var(--surface)] text-[var(--text)] hover:border-brand-300 hover:bg-brand-50 dark:hover:bg-brand-950/60 dark:hover:border-brand-700',
  fantasma: 'text-[var(--text)] hover:bg-black/5 dark:hover:bg-white/10',
  perigo: 'bg-rose-600 text-white shadow-lg shadow-rose-600/20 hover:bg-rose-700',
};

const TAMANHOS: Record<Tamanho, string> = {
  sm: 'h-9 px-3.5 text-sm',
  md: 'h-11 px-5 text-sm',
  lg: 'h-13 px-7 text-base',
};

export function estiloBotao(variante: Variante = 'primario', tamanho: Tamanho = 'md', extra?: string) {
  return cn(BASE, VARIANTES[variante], TAMANHOS[tamanho], extra);
}

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variante?: Variante;
  tamanho?: Tamanho;
  carregando?: boolean;
  icone?: ReactNode;
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(function Button(
  { variante = 'primario', tamanho = 'md', carregando, icone, className, children, disabled, type = 'button', ...resto },
  ref,
) {
  return (
    <button
      ref={ref}
      type={type}
      className={estiloBotao(variante, tamanho, className)}
      disabled={disabled || carregando}
      aria-busy={carregando || undefined}
      {...resto}
    >
      {carregando ? <Loader2 className="size-4 animate-spin" aria-hidden /> : icone}
      {children}
    </button>
  );
});

interface ButtonLinkProps extends LinkProps {
  variante?: Variante;
  tamanho?: Tamanho;
  icone?: ReactNode;
}

export function ButtonLink({ variante = 'primario', tamanho = 'md', icone, className, children, ...resto }: ButtonLinkProps) {
  return (
    <Link className={estiloBotao(variante, tamanho, typeof className === 'string' ? className : undefined)} {...resto}>
      {icone}
      {children}
    </Link>
  );
}
