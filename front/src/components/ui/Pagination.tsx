import { ChevronLeft, ChevronRight } from 'lucide-react';
import { Button } from './Button';

export function Pagination({ pagina, totalPaginas, aoMudar }: { pagina: number; totalPaginas: number; aoMudar: (p: number) => void }) {
  if (totalPaginas <= 1) return null;
  return (
    <nav className="flex items-center justify-center gap-3 pt-4" aria-label="Paginação">
      <Button
        variante="secundario"
        tamanho="sm"
        onClick={() => aoMudar(pagina - 1)}
        disabled={pagina <= 1}
        icone={<ChevronLeft className="size-4" />}
      >
        Anterior
      </Button>
      <span className="muted text-sm tabular-nums" aria-current="page">
        Página <strong className="text-[var(--text)]">{pagina}</strong> de {totalPaginas}
      </span>
      <Button variante="secundario" tamanho="sm" onClick={() => aoMudar(pagina + 1)} disabled={pagina >= totalPaginas}>
        Próxima
        <ChevronRight className="size-4" />
      </Button>
    </nav>
  );
}
