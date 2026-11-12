import { forwardRef, useId, type InputHTMLAttributes, type ReactNode, type SelectHTMLAttributes, type TextareaHTMLAttributes } from 'react';
import { ChevronDown } from 'lucide-react';
import { cn } from '../../lib/cn';

export const estiloCampo =
  'w-full rounded-xl border border-[var(--border)] bg-[var(--surface)] px-3.5 text-[15px] text-[var(--text)] ' +
  'placeholder:text-[var(--text-muted)]/70 transition-colors outline-none ' +
  'focus:border-brand-500 focus:ring-4 focus:ring-brand-500/15 disabled:opacity-60 ' +
  'aria-[invalid=true]:border-rose-500 aria-[invalid=true]:focus:ring-rose-500/15';

interface CampoBaseProps {
  rotulo: string;
  erro?: string;
  ajuda?: ReactNode;
  opcional?: boolean;
  className?: string;
  children: (props: { id: string; 'aria-invalid'?: boolean; 'aria-describedby'?: string }) => ReactNode;
}

/** Rótulo + controle + mensagem de erro/ajuda ligados por id (acessibilidade). */
export function Campo({ rotulo, erro, ajuda, opcional, className, children }: CampoBaseProps) {
  const id = useId();
  const idDescricao = erro || ajuda ? `${id}-desc` : undefined;
  return (
    <div className={cn('flex flex-col gap-1.5', className)}>
      <label htmlFor={id} className="text-sm font-semibold">
        {rotulo}
        {opcional && <span className="muted ml-1 font-normal">(opcional)</span>}
      </label>
      {children({ id, 'aria-invalid': erro ? true : undefined, 'aria-describedby': idDescricao })}
      {erro ? (
        <p id={idDescricao} className="text-sm font-medium text-rose-600 dark:text-rose-400" role="alert">
          {erro}
        </p>
      ) : ajuda ? (
        <div id={idDescricao} className="muted text-xs">
          {ajuda}
        </div>
      ) : null}
    </div>
  );
}

type InputProps = InputHTMLAttributes<HTMLInputElement> & { rotulo: string; erro?: string; ajuda?: ReactNode; opcional?: boolean; icone?: ReactNode };

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input(
  { rotulo, erro, ajuda, opcional, className, icone, ...resto },
  ref,
) {
  return (
    <Campo rotulo={rotulo} erro={erro} ajuda={ajuda} opcional={opcional} className={className}>
      {(a11y) => (
        <div className="relative">
          {icone && <span className="muted pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2">{icone}</span>}
          <input ref={ref} className={cn(estiloCampo, 'h-11', icone && 'pl-10')} {...a11y} {...resto} />
        </div>
      )}
    </Campo>
  );
});

type SelectProps = SelectHTMLAttributes<HTMLSelectElement> & { rotulo: string; erro?: string; ajuda?: ReactNode; opcional?: boolean };

export const Select = forwardRef<HTMLSelectElement, SelectProps>(function Select(
  { rotulo, erro, ajuda, opcional, className, children, ...resto },
  ref,
) {
  return (
    <Campo rotulo={rotulo} erro={erro} ajuda={ajuda} opcional={opcional} className={className}>
      {(a11y) => (
        <div className="relative">
          <select ref={ref} className={cn(estiloCampo, 'h-11 appearance-none pr-10')} {...a11y} {...resto}>
            {children}
          </select>
          <ChevronDown className="muted pointer-events-none absolute right-3 top-1/2 size-4 -translate-y-1/2" aria-hidden />
        </div>
      )}
    </Campo>
  );
});

type TextareaProps = TextareaHTMLAttributes<HTMLTextAreaElement> & { rotulo: string; erro?: string; ajuda?: ReactNode; opcional?: boolean };

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(function Textarea(
  { rotulo, erro, ajuda, opcional, className, maxLength, value, ...resto },
  ref,
) {
  const contador = maxLength && typeof value === 'string' ? `${value.length}/${maxLength}` : undefined;
  return (
    <Campo
      rotulo={rotulo}
      erro={erro}
      ajuda={ajuda ?? contador}
      opcional={opcional}
      className={className}
    >
      {(a11y) => (
        <textarea
          ref={ref}
          className={cn(estiloCampo, 'min-h-28 resize-y py-2.5 leading-relaxed')}
          maxLength={maxLength}
          value={value}
          {...a11y}
          {...resto}
        />
      )}
    </Campo>
  );
});
