import { useState } from 'react';
import { useSearchParams } from 'react-router';
import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { Lightbulb, Plus } from 'lucide-react';
import { propostasApi } from '../api/services';
import { AREAS, STATUS_PROPOSTA, type AreaConhecimento, type StatusProposta } from '../api/types';
import { AREA_INFO, BAIRROS_ZONA_SUL, STATUS_PROPOSTA_INFO } from '../lib/labels';
import { useAuth } from '../auth/AuthProvider';
import { useVotos } from '../hooks/useVotos';
import { PageHeader } from '../components/ui/PageHeader';
import { Select } from '../components/ui/Field';
import { Button, ButtonLink } from '../components/ui/Button';
import { PropostaCard } from '../components/PropostaCard';
import { ErrorState, Skeleton } from '../components/ui/Feedback';
import { EmptyState } from '../components/ui/EmptyState';
import { Pagination } from '../components/ui/Pagination';
import { NovaPropostaModal } from '../components/PropostaForm';
import { cn } from '../lib/cn';

export default function PropostasPage() {
  const [params, setParams] = useSearchParams();
  const { sessao } = useAuth();
  const votos = useVotos();
  const [criando, setCriando] = useState(false);

  const status = (params.get('status') as StatusProposta) || undefined;
  const area = (params.get('area') as AreaConhecimento) || undefined;
  const bairro = params.get('bairro') || undefined;
  const pagina = Number(params.get('pagina')) || 1;
  const filtro = { status, area, bairro, pagina, tamanho: 10, ordenarPorVotos: true };

  const q = useQuery({ queryKey: ['propostas', filtro], queryFn: () => propostasApi.listar(filtro), placeholderData: keepPreviousData });

  function atualizar(chave: string, valor?: string) {
    setParams((a) => {
      const n = new URLSearchParams(a);
      if (valor) n.set(chave, valor);
      else n.delete(chave);
      if (chave !== 'pagina') n.delete('pagina');
      return n;
    });
  }

  const ehCidadao = sessao?.perfil === 'Cidadao';

  return (
    <>
      <PageHeader
        selo="Participação cidadã"
        titulo="Propostas da comunidade"
        descricao="O que está faltando no seu bairro? Proponha, vote nas ideias dos vizinhos e acompanhe quando uma organização atender."
        acoes={
          ehCidadao ? (
            <Button tamanho="lg" variante="sol" icone={<Plus className="size-5" />} onClick={() => setCriando(true)}>
              Nova proposta
            </Button>
          ) : !sessao ? (
            <ButtonLink to="/entrar" state={{ de: '/propostas' }} tamanho="lg" variante="secundario">
              Entre para propor e votar
            </ButtonLink>
          ) : null
        }
      />
      <div className="mx-auto max-w-5xl px-4 py-10 sm:px-6">
        <div className="flex flex-wrap items-end gap-3">
          <div className="surface-2 flex rounded-2xl p-1" role="group" aria-label="Filtrar por status">
            {[undefined, ...STATUS_PROPOSTA].map((s) => (
              <button
                key={s ?? 'todas'}
                type="button"
                onClick={() => atualizar('status', s)}
                aria-pressed={status === s}
                className={cn('rounded-xl px-4 py-2 text-sm font-semibold transition', status === s ? 'bg-[var(--surface)] shadow' : 'muted hover:text-[var(--text)]')}
              >
                {s ? STATUS_PROPOSTA_INFO[s].rotulo : 'Todas'}
              </button>
            ))}
          </div>
          <Select rotulo="Área" className="min-w-44 flex-1 sm:flex-none" value={area ?? ''} onChange={(e) => atualizar('area', e.target.value)}>
            <option value="">Todas</option>
            {AREAS.map((a) => <option key={a} value={a}>{AREA_INFO[a].rotulo}</option>)}
          </Select>
          <Select rotulo="Bairro" className="min-w-44 flex-1 sm:flex-none" value={bairro ?? ''} onChange={(e) => atualizar('bairro', e.target.value)}>
            <option value="">Todos</option>
            {BAIRROS_ZONA_SUL.map((b) => <option key={b} value={b}>{b}</option>)}
          </Select>
        </div>

        <div className="mt-8 space-y-4">
          {q.isLoading ? (
            [0, 1, 2].map((i) => <Skeleton key={i} className="h-36" />)
          ) : q.isError ? (
            <ErrorState erro={q.error} onRetry={() => q.refetch()} />
          ) : q.data?.itens.length === 0 ? (
            <EmptyState
              icone={Lightbulb}
              titulo="Nenhuma proposta com esses filtros"
              descricao="Que tal ser a primeira pessoa a propor?"
              acao={ehCidadao && <Button onClick={() => setCriando(true)}>Criar proposta</Button>}
            />
          ) : (
            q.data?.itens.map((p, i) => (
              <PropostaCard
                key={p.id}
                proposta={p}
                indice={i}
                posicao={(pagina - 1) * 10 + i + 1}
                podeVotar={votos.podeVotar}
                jaVotou={votos.jaVotou(p.id)}
                votando={votos.votandoId === p.id}
                aoVotar={() => votos.votar(p.id)}
              />
            ))
          )}
        </div>
        {q.data && <Pagination pagina={q.data.pagina} totalPaginas={q.data.totalPaginas} aoMudar={(p) => atualizar('pagina', String(p))} />}
      </div>
      {ehCidadao && <NovaPropostaModal aberto={criando} aoFechar={() => setCriando(false)} />}
    </>
  );
}
