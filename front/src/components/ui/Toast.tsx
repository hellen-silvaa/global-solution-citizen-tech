import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react';
import { AnimatePresence, motion } from 'motion/react';
import { AlertTriangle, CheckCircle2, Info, X } from 'lucide-react';
import { cn } from '../../lib/cn';

type TipoToast = 'sucesso' | 'erro' | 'info';
interface Toast {
  id: number;
  tipo: TipoToast;
  titulo: string;
  descricao?: string;
}

interface ToastApi {
  sucesso: (titulo: string, descricao?: string) => void;
  erro: (titulo: string, descricao?: string) => void;
  info: (titulo: string, descricao?: string) => void;
}

const Contexto = createContext<ToastApi | null>(null);
let proximoId = 1;

const ICONES = { sucesso: CheckCircle2, erro: AlertTriangle, info: Info };
const CORES = {
  sucesso: 'text-emerald-600 dark:text-emerald-400',
  erro: 'text-rose-600 dark:text-rose-400',
  info: 'text-brand-600 dark:text-brand-300',
};

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const remover = useCallback((id: number) => setToasts((t) => t.filter((x) => x.id !== id)), []);

  const mostrar = useCallback(
    (tipo: TipoToast, titulo: string, descricao?: string) => {
      const id = proximoId++;
      setToasts((t) => [...t.slice(-3), { id, tipo, titulo, descricao }]);
      setTimeout(() => remover(id), tipo === 'erro' ? 6500 : 4500);
    },
    [remover],
  );

  const api = useMemo<ToastApi>(
    () => ({
      sucesso: (t, d) => mostrar('sucesso', t, d),
      erro: (t, d) => mostrar('erro', t, d),
      info: (t, d) => mostrar('info', t, d),
    }),
    [mostrar],
  );

  return (
    <Contexto.Provider value={api}>
      {children}
      <div
        aria-live="polite"
        role="status"
        className="pointer-events-none fixed inset-x-0 bottom-4 z-[100] flex flex-col items-center gap-2 px-4 sm:bottom-6 sm:items-end sm:px-6"
      >
        <AnimatePresence initial={false}>
          {toasts.map((t) => {
            const Icone = ICONES[t.tipo];
            return (
              <motion.div
                key={t.id}
                layout
                initial={{ opacity: 0, y: 24, scale: 0.96 }}
                animate={{ opacity: 1, y: 0, scale: 1 }}
                exit={{ opacity: 0, x: 40, transition: { duration: 0.18 } }}
                className="card pointer-events-auto flex w-full max-w-sm items-start gap-3 p-4 shadow-xl shadow-black/10"
              >
                <Icone className={cn('mt-0.5 size-5 shrink-0', CORES[t.tipo])} aria-hidden />
                <div className="min-w-0 flex-1">
                  <p className="text-sm font-semibold">{t.titulo}</p>
                  {t.descricao && <p className="muted mt-0.5 text-sm">{t.descricao}</p>}
                </div>
                <button
                  type="button"
                  onClick={() => remover(t.id)}
                  className="muted -m-1 rounded-lg p-1 hover:bg-black/5 dark:hover:bg-white/10"
                  aria-label="Fechar aviso"
                >
                  <X className="size-4" />
                </button>
              </motion.div>
            );
          })}
        </AnimatePresence>
      </div>
    </Contexto.Provider>
  );
}

export function useToast(): ToastApi {
  const ctx = useContext(Contexto);
  if (!ctx) throw new Error('useToast precisa estar dentro de <ToastProvider>');
  return ctx;
}
