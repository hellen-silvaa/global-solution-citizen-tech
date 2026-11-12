import { AlertTriangle, RefreshCw } from 'lucide-react';
import { cn } from '../../lib/cn';
import { mensagemDeErro } from '../../api/client';
import { Button } from './Button';

export function Skeleton({ className }: { className?: string }) {
  return <div className={cn('animate-pulse rounded-xl bg-[var(--surface-2)]', className)} aria-hidden />;
}

export function CardsSkeleton({ quantidade = 6 }: { quantidade?: number }) {
  return (
    <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3" aria-label="Carregando" role="status">
      {Array.from({ length: quantidade }, (_, i) => (
        <div key={i} className="card space-y-4 p-5">
          <div className="flex items-center gap-3">
            <Skeleton className="size-11 rounded-2xl" />
            <Skeleton className="h-4 w-24" />
          </div>
          <Skeleton className="h-5 w-4/5" />
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-4 w-2/3" />
          <Skeleton className="h-2 w-full" />
        </div>
      ))}
    </div>
  );
}

export function ErrorState({ erro, onRetry }: { erro: unknown; onRetry?: () => void }) {
  return (
    <div role="alert" className="card flex flex-col items-center gap-3 border-rose-200 px-6 py-10 text-center dark:border-rose-500/30">
      <div className="grid size-12 place-items-center rounded-2xl bg-rose-100 text-rose-600 dark:bg-rose-500/15 dark:text-rose-300">
        <AlertTriangle className="size-6" aria-hidden />
      </div>
      <p className="font-semibold">Não foi possível carregar</p>
      <p className="muted max-w-md text-sm">{mensagemDeErro(erro)}</p>
      {onRetry && (
        <Button variante="secundario" tamanho="sm" onClick={onRetry} icone={<RefreshCw className="size-4" />}>
          Tentar de novo
        </Button>
      )}
    </div>
  );
}
