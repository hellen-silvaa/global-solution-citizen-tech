import { useEffect, useId, useRef, type ReactNode } from 'react';
import { createPortal } from 'react-dom';
import { AnimatePresence, motion } from 'motion/react';
import { X } from 'lucide-react';
import { cn } from '../../lib/cn';
import { Button } from './Button';

interface ModalProps {
  aberto: boolean;
  aoFechar: () => void;
  titulo: string;
  descricao?: ReactNode;
  children?: ReactNode;
  rodape?: ReactNode;
  largura?: 'sm' | 'md' | 'lg';
}

/** Diálogo acessível: foco preso, Esc fecha, foco volta para quem abriu. */
export function Modal({ aberto, aoFechar, titulo, descricao, children, rodape, largura = 'md' }: ModalProps) {
  const id = useId();
  const painel = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!aberto) return;
    const anterior = document.activeElement as HTMLElement | null;
    const overflow = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    const foco = window.setTimeout(() => {
      const primeiro = painel.current?.querySelector<HTMLElement>('input, select, textarea, button:not([data-fechar])');
      (primeiro ?? painel.current)?.focus();
    }, 30);

    function teclado(e: KeyboardEvent) {
      if (e.key === 'Escape') aoFechar();
      if (e.key !== 'Tab' || !painel.current) return;
      const focaveis = painel.current.querySelectorAll<HTMLElement>(
        'a[href], button:not([disabled]), input:not([disabled]), select, textarea, [tabindex]:not([tabindex="-1"])',
      );
      if (focaveis.length === 0) return;
      const primeiro = focaveis[0];
      const ultimo = focaveis[focaveis.length - 1];
      if (e.shiftKey && document.activeElement === primeiro) {
        e.preventDefault();
        ultimo.focus();
      } else if (!e.shiftKey && document.activeElement === ultimo) {
        e.preventDefault();
        primeiro.focus();
      }
    }
    document.addEventListener('keydown', teclado);
    return () => {
      window.clearTimeout(foco);
      document.removeEventListener('keydown', teclado);
      document.body.style.overflow = overflow;
      anterior?.focus?.();
    };
  }, [aberto, aoFechar]);

  return createPortal(
    <AnimatePresence>
      {aberto && (
        <div className="fixed inset-0 z-[90] flex items-end justify-center p-0 sm:items-center sm:p-6">
          <motion.div
            className="absolute inset-0 bg-slate-950/50 backdrop-blur-sm"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={aoFechar}
          />
          <motion.div
            ref={painel}
            role="dialog"
            aria-modal="true"
            aria-labelledby={`${id}-titulo`}
            aria-describedby={descricao ? `${id}-desc` : undefined}
            tabIndex={-1}
            initial={{ opacity: 0, y: 40, scale: 0.98 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            exit={{ opacity: 0, y: 30, scale: 0.98, transition: { duration: 0.15 } }}
            transition={{ type: 'spring', damping: 28, stiffness: 340 }}
            className={cn(
              'card relative flex max-h-[92dvh] w-full flex-col overflow-hidden rounded-b-none shadow-2xl sm:rounded-b-[1.25rem]',
              largura === 'sm' ? 'sm:max-w-md' : largura === 'md' ? 'sm:max-w-xl' : 'sm:max-w-3xl',
            )}
          >
            <div className="flex items-start justify-between gap-4 border-b border-[var(--border)] px-6 py-5">
              <div>
                <h2 id={`${id}-titulo`} className="text-xl font-bold">
                  {titulo}
                </h2>
                {descricao && (
                  <div id={`${id}-desc`} className="muted mt-1 text-sm">
                    {descricao}
                  </div>
                )}
              </div>
              <button
                type="button"
                data-fechar
                onClick={aoFechar}
                className="muted -m-1.5 rounded-xl p-1.5 hover:bg-black/5 dark:hover:bg-white/10"
                aria-label="Fechar"
              >
                <X className="size-5" />
              </button>
            </div>
            {children && <div className="overflow-y-auto px-6 py-5">{children}</div>}
            {rodape && <div className="flex flex-wrap justify-end gap-2 border-t border-[var(--border)] px-6 py-4">{rodape}</div>}
          </motion.div>
        </div>
      )}
    </AnimatePresence>,
    document.body,
  );
}

interface ConfirmProps {
  aberto: boolean;
  titulo: string;
  descricao: ReactNode;
  confirmar: string;
  perigo?: boolean;
  carregando?: boolean;
  aoConfirmar: () => void;
  aoFechar: () => void;
}

export function ConfirmDialog({ aberto, titulo, descricao, confirmar, perigo, carregando, aoConfirmar, aoFechar }: ConfirmProps) {
  return (
    <Modal
      aberto={aberto}
      aoFechar={aoFechar}
      titulo={titulo}
      descricao={descricao}
      largura="sm"
      rodape={
        <>
          <Button variante="secundario" onClick={aoFechar}>
            Voltar
          </Button>
          <Button variante={perigo ? 'perigo' : 'primario'} onClick={aoConfirmar} carregando={carregando}>
            {confirmar}
          </Button>
        </>
      }
    />
  );
}
