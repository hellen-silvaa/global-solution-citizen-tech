import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router';
import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { FilterX, Search, SearchX, SlidersHorizontal } from 'lucide-react';
import { oportunidadesApi } from '../api/services';
import { AREAS, MODALIDADES, TIPOS_OPORTUNIDADE, type AreaConhecimento, type FiltroOportunidades, type Modalidade, type TipoOportunidade } from '../api/types';
import { AREA_INFO, BAIRROS_ZONA_SUL, MODALIDADE_ROTULO, TIPO_OPORTUNIDADE_ROTULO } from '../lib/labels';
import { plural } from '../lib/format';
import { useDebounce } from '../hooks/useDebounce';
import { PageHeader } from '../components/ui/PageHeader';
import { Input, Select } from '../components/ui/Field';
import { OportunidadeCard } from '../components/OportunidadeCard';
import { CardsSkeleton, ErrorState } from '../components/ui/Feedback';
import { EmptyState } from '../components/ui/EmptyState';
import { Pagination } from '../components/ui/Pagination';
import { Button } from '../components/ui/Button';
import { cn } from '../lib/cn';

const TAMANHO = 9;

export function lerFiltro(params: URLSearchParams): FiltroOportunidades {
  const gratuita = params.get('gratuita');
  return {
    texto: params.get('texto') ?? undefined,
    area: (params.get('area') as AreaConhecimento) || undefined,
    tipo: (params.get('tipo') as TipoOportunidade) || undefined,
    modalidade: (params.get('modalidade') as Modalidade) || undefined,
    bairro: params.get('bairro') ?? undefined,
    gratuita: gratuita === 'true' ? true : undefined,
    somenteComVagas: params.get('todas') !== 'true',
    pagina: Number(params.get('pagina')) || 1,
    tamanho: TAMANHO,
  };
}

export default function OportunidadesPage() {
  const [params, setParams] = useSearchParams();
  const filtro = lerFiltro(params);
  const [texto, setTexto] = useState(filtro.texto ?? '');
  const textoAtrasado = useDebounce(texto);
  const [filtrosMovel, setFiltrosMovel] = useState(false);

  function atualizar(chave: string, valor: string | undefined) {
    setParams(
      (atual) => {
        const novo = new URLSearchParams(atual);
        if (valor) novo.set(chave, valor);
        else novo.delete(chave);
        if (chave !== 'pagina') novo.delete('pagina');
        return novo;
      },
      { replace: chave !== 'pagina' },
    );
  }

  useEffect(() => {
    if ((filtro.texto ?? '') !== textoAtrasado) atualizar('texto', textoAtrasado || undefined);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [textoAtrasado]);

  const q = useQuery({
    queryKey: ['oportunidades', filtro],
    queryFn: () => oportunidadesApi.buscar(filtro),
    placeholderData: keepPreviousData,
  });

  const filtrosAtivos = ['area', 'tipo', 'modalidade', 'bairro', 'gratuita', 'texto', 'todas'].filter((k) => params.has(k)).length;

  return (
    <>
      <PageHeader
        selo="Explorar"
        titulo="Oportunidades de qualificação"
        descricao="Filtre por área, bairro, modalidade e tipo. Tudo de organizações da sua região."
      />
      <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6">
        <div className="flex flex-col gap-3 sm:flex-row">
          <div className="flex-1">
            <Input
              rotulo="Buscar"
              className="[&>label]:sr-only"
              type="search"
              placeholder="Buscar por título ou descrição (ex.: programação, currículo...)"
              icone={<Search className="size-4" />}
              value={texto}
              onChange={(e) => setTexto(e.target.value)}
            />
          </div>
          <Button
            variante="secundario"
            className="sm:hidden"
            onClick={() => setFiltrosMovel((v) => !v)}
            aria-expanded={filtrosMovel}
            icone={<SlidersHorizontal className="size-4" />}
          >
            Filtros {filtrosAtivos > 0 && `(${filtrosAtivos})`}
          </Button>
        </div>

        <div className={cn('mt-4 grid gap-3 sm:grid sm:grid-cols-2 lg:grid-cols-5', filtrosMovel ? 'grid' : 'hidden')}>
          <Select rotulo="Área" value={filtro.area ?? ''} onChange={(e) => atualizar('area', e.target.value)}>
            <option value="">Todas as áreas</option>
            {AREAS.map((a) => (
              <option key={a} value={a}>{AREA_INFO[a].rotulo}</option>
            ))}
          </Select>
          <Select rotulo="Tipo" value={filtro.tipo ?? ''} onChange={(e) => atualizar('tipo', e.target.value)}>
            <option value="">Todos os tipos</option>
            {TIPOS_OPORTUNIDADE.map((t) => (
              <option key={t} value={t}>{TIPO_OPORTUNIDADE_ROTULO[t]}</option>
            ))}
          </Select>
          <Select rotulo="Modalidade" value={filtro.modalidade ?? ''} onChange={(e) => atualizar('modalidade', e.target.value)}>
            <option value="">Qualquer modalidade</option>
            {MODALIDADES.map((m) => (
              <option key={m} value={m}>{MODALIDADE_ROTULO[m]}</option>
            ))}
          </Select>
          <Select rotulo="Bairro" value={filtro.bairro ?? ''} onChange={(e) => atualizar('bairro', e.target.value)}>
            <option value="">Todos os bairros</option>
            {BAIRROS_ZONA_SUL.map((b) => (
              <option key={b} value={b}>{b}</option>
            ))}
            {filtro.bairro && !BAIRROS_ZONA_SUL.includes(filtro.bairro as (typeof BAIRROS_ZONA_SUL)[number]) && (
              <option value={filtro.bairro}>{filtro.bairro}</option>
            )}
          </Select>
          <fieldset className="flex flex-col gap-1.5">
            <legend className="mb-1.5 text-sm font-semibold">Mostrar</legend>
            <div className="flex h-11 items-center gap-4 text-sm">
              <label className="flex cursor-pointer items-center gap-2">
                <input type="checkbox" className="size-4 accent-brand-600" checked={!!filtro.gratuita} onChange={(e) => atualizar('gratuita', e.target.checked ? 'true' : undefined)} />
                Só gratuitas
              </label>
              <label className="flex cursor-pointer items-center gap-2">
                <input type="checkbox" className="size-4 accent-brand-600" checked={!filtro.somenteComVagas} onChange={(e) => atualizar('todas', e.target.checked ? 'true' : undefined)} />
                Incluir encerradas
              </label>
            </div>
          </fieldset>
        </div>

        <div className="mt-8 flex min-h-9 flex-wrap items-center justify-between gap-3">
          <p className="muted text-sm" aria-live="polite">
            {q.data ? <><strong className="text-[var(--text)]">{plural(q.data.totalItens, 'oportunidade encontrada', 'oportunidades encontradas')}</strong></> : 'Buscando...'}
          </p>
          {filtrosAtivos > 0 && (
            <Button
              variante="fantasma"
              tamanho="sm"
              icone={<FilterX className="size-4" />}
              onClick={() => {
                setTexto('');
                setParams({}, { replace: true });
              }}
            >
              Limpar filtros
            </Button>
          )}
        </div>

        <div className={cn('mt-4 transition-opacity', q.isPlaceholderData && 'opacity-60')}>
          {q.isLoading ? (
            <CardsSkeleton />
          ) : q.isError ? (
            <ErrorState erro={q.error} onRetry={() => q.refetch()} />
          ) : q.data && q.data.itens.length === 0 ? (
            <EmptyState
              icone={SearchX}
              titulo="Nada por aqui ainda"
              descricao="Tente outros filtros. Se o que você procura não existe no seu bairro, crie uma proposta para a comunidade votar."
            />
          ) : (
            <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
              {q.data?.itens.map((o, i) => <OportunidadeCard key={o.id} oportunidade={o} indice={i} />)}
            </div>
          )}
        </div>

        {q.data && <Pagination pagina={q.data.pagina} totalPaginas={q.data.totalPaginas} aoMudar={(p) => atualizar('pagina', String(p))} />}
      </div>
    </>
  );
}
