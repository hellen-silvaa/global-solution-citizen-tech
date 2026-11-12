import { useState } from 'react';
import { Link, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Check, GraduationCap, Users, X } from 'lucide-react';
import { inscricoesApi, oportunidadesApi } from '../../api/services';
import { STATUS_INSCRICAO, type Inscricao, type StatusInscricao } from '../../api/types';
import { STATUS_INSCRICAO_INFO } from '../../lib/labels';
import { formatarData, iniciais } from '../../lib/format';
import { cn } from '../../lib/cn';
import { PageHeader } from '../../components/ui/PageHeader';
import { Button } from '../../components/ui/Button';
import { StatusBadge } from '../../components/ui/Badge';
import { ErrorState, Skeleton } from '../../components/ui/Feedback';
import { EmptyState } from '../../components/ui/EmptyState';
import { ConfirmDialog } from '../../components/ui/Modal';
import { useToast } from '../../components/ui/Toast';
import { VagasBarra } from '../../components/OportunidadeCard';

type Acao = 'confirmar' | 'concluir' | 'cancelar';

const TEXTO_ACAO: Record<Acao, { feito: string; titulo: string }> = {
  confirmar: { feito: 'Inscrição confirmada', titulo: 'Confirmar' },
  concluir: { feito: 'Conclusão registrada 🎓', titulo: 'Concluir' },
  cancelar: { feito: 'Inscrição cancelada', titulo: 'Cancelar' },
};

export default function InscritosPage() {
  const id = Number(useParams().id);
  const toast = useToast();
  const queryClient = useQueryClient();
  const [filtro, setFiltro] = useState<StatusInscricao | 'Todas'>('Todas');
  const [cancelando, setCancelando] = useState<Inscricao | null>(null);

  const op = useQuery({ queryKey: ['oportunidade', id], queryFn: () => oportunidadesApi.obter(id) });
  const q = useQuery({ queryKey: ['inscritos', id], queryFn: () => oportunidadesApi.inscritos(id) });

  const acao = useMutation({
    mutationFn: async ({ inscricao, tipo }: { inscricao: Inscricao; tipo: Acao }) => {
      if (tipo === 'confirmar') await inscricoesApi.confirmar(inscricao.id);
      else if (tipo === 'concluir') await inscricoesApi.concluir(inscricao.id);
      else await inscricoesApi.cancelar(inscricao.id);
    },
    onSuccess: (_, { tipo, inscricao }) => {
      toast.sucesso(TEXTO_ACAO[tipo].feito, inscricao.cidadaoNome ?? undefined);
      setCancelando(null);
      void queryClient.invalidateQueries({ queryKey: ['inscritos', id] });
      void queryClient.invalidateQueries({ queryKey: ['oportunidade', id] });
      void queryClient.invalidateQueries({ queryKey: ['oportunidades'] });
      void queryClient.invalidateQueries({ queryKey: ['impacto'] });
    },
    onError: (e) => {
      toast.erro('Não foi possível concluir a ação', e.message);
      setCancelando(null);
    },
  });

  const lista = (q.data ?? []).filter((i) => filtro === 'Todas' || i.status === filtro);
  const contagem = (s: StatusInscricao) => q.data?.filter((i) => i.status === s).length ?? 0;
  const emAndamento = acao.isPending ? acao.variables : undefined;

  return (
    <>
      <PageHeader
        selo={<Link to="/organizacao" className="inline-flex items-center gap-1 hover:underline"><ArrowLeft className="size-4" /> Painel</Link>}
        titulo={op.data ? op.data.titulo : 'Inscritos'}
        descricao="Confirme as vagas, registre quem concluiu (isso entra no indicador de impacto) ou cancele inscrições."
      />
      <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6">
        {op.data && (
          <div className="card mb-6 p-5">
            <VagasBarra disponiveis={op.data.vagasDisponiveis} totais={op.data.vagasTotais} />
          </div>
        )}
        <div className="mb-5 flex flex-wrap gap-2" role="group" aria-label="Filtrar por status">
          {(['Todas', ...STATUS_INSCRICAO] as const).map((s) => (
            <button
              key={s}
              type="button"
              aria-pressed={filtro === s}
              onClick={() => setFiltro(s)}
              className={cn('rounded-full border px-4 py-1.5 text-sm font-semibold transition', filtro === s ? 'border-brand-600 bg-brand-600 text-white' : 'border-[var(--border)] bg-[var(--surface)] hover:border-brand-300')}
            >
              {s === 'Todas' ? `Todas (${q.data?.length ?? 0})` : `${STATUS_INSCRICAO_INFO[s].rotulo} (${contagem(s)})`}
            </button>
          ))}
        </div>

        {q.isLoading ? (
          <div className="space-y-3">{[0, 1, 2].map((i) => <Skeleton key={i} className="h-20" />)}</div>
        ) : q.isError ? (
          <ErrorState erro={q.error} onRetry={() => q.refetch()} />
        ) : lista.length === 0 ? (
          <EmptyState icone={Users} titulo={filtro === 'Todas' ? 'Ninguém se inscreveu ainda' : 'Nenhuma inscrição com este status'} />
        ) : (
          <ul className="space-y-3">
            {lista.map((i) => {
              const st = STATUS_INSCRICAO_INFO[i.status];
              const nome = i.cidadaoNome ?? `Cidadão #${i.cidadaoId}`;
              const ocupado = emAndamento?.inscricao.id === i.id;
              return (
                <li key={i.id} className="card flex flex-wrap items-center gap-4 p-4 sm:p-5">
                  <span className="grid size-11 place-items-center rounded-xl bg-gradient-to-br from-brand-500 to-violet-500 text-sm font-bold text-white">{iniciais(nome)}</span>
                  <div className="min-w-0 flex-1">
                    <p className="font-bold">{nome}</p>
                    <p className="muted text-sm">Inscrição em {formatarData(i.criadaEm)}</p>
                  </div>
                  <StatusBadge tom={st.tom}>{st.rotulo}</StatusBadge>
                  <div className="flex flex-wrap gap-2">
                    {i.status === 'Pendente' && (
                      <Button tamanho="sm" icone={<Check className="size-4" />} carregando={ocupado && emAndamento?.tipo === 'confirmar'} disabled={ocupado} onClick={() => acao.mutate({ inscricao: i, tipo: 'confirmar' })}>
                        Confirmar
                      </Button>
                    )}
                    {i.status === 'Confirmada' && (
                      <Button tamanho="sm" variante="sol" icone={<GraduationCap className="size-4" />} carregando={ocupado && emAndamento?.tipo === 'concluir'} disabled={ocupado} onClick={() => acao.mutate({ inscricao: i, tipo: 'concluir' })}>
                        Concluir
                      </Button>
                    )}
                    {(i.status === 'Pendente' || i.status === 'Confirmada') && (
                      <Button tamanho="sm" variante="fantasma" className="text-rose-600" icone={<X className="size-4" />} disabled={ocupado} onClick={() => setCancelando(i)}>
                        Cancelar
                      </Button>
                    )}
                  </div>
                </li>
              );
            })}
          </ul>
        )}
      </div>
      <ConfirmDialog
        aberto={!!cancelando}
        titulo="Cancelar esta inscrição?"
        descricao={<>A vaga de <strong>{cancelando?.cidadaoNome}</strong> volta a ficar disponível.</>}
        confirmar="Cancelar inscrição"
        perigo
        carregando={acao.isPending}
        aoConfirmar={() => cancelando && acao.mutate({ inscricao: cancelando, tipo: 'cancelar' })}
        aoFechar={() => setCancelando(null)}
      />
    </>
  );
}
