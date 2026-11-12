import { useState } from 'react';
import { Link, useSearchParams } from 'react-router';
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { motion } from 'motion/react';
import { Archive, BarChart3, Building2, Eye, Lightbulb, ShieldCheck, UserX, Users } from 'lucide-react';
import { cidadaosApi, indicadoresApi, organizacoesApi, propostasApi } from '../../api/services';
import type { Proposta } from '../../api/types';
import { AREA_INFO, BAIRROS_ZONA_SUL, ESCOLARIDADE_ROTULO, STATUS_PROPOSTA_INFO, TIPO_ORGANIZACAO_ROTULO } from '../../lib/labels';
import { formatarData } from '../../lib/format';
import { cn } from '../../lib/cn';
import { PageHeader } from '../../components/ui/PageHeader';
import { Button, ButtonLink } from '../../components/ui/Button';
import { Select } from '../../components/ui/Field';
import { StatusBadge } from '../../components/ui/Badge';
import { ErrorState, Skeleton } from '../../components/ui/Feedback';
import { EmptyState } from '../../components/ui/EmptyState';
import { Pagination } from '../../components/ui/Pagination';
import { ConfirmDialog, Modal } from '../../components/ui/Modal';
import { useToast } from '../../components/ui/Toast';
import { GraficoDemandaReprimida, GraficoInscricoesPorBairro } from '../../components/ImpactCharts';

const ABAS = [
  { id: 'visao', rotulo: 'Visão geral', icone: BarChart3 },
  { id: 'cidadaos', rotulo: 'Cidadãos', icone: Users },
  { id: 'organizacoes', rotulo: 'Organizações', icone: Building2 },
  { id: 'propostas', rotulo: 'Propostas', icone: Lightbulb },
] as const;

const thClasse = 'muted px-4 py-3 text-left text-xs font-bold uppercase tracking-wider';
const tdClasse = 'px-4 py-3';

function VisaoGeral() {
  const q = useQuery({ queryKey: ['impacto'], queryFn: indicadoresApi.impacto });
  if (q.isLoading) return <Skeleton className="h-96" />;
  if (q.isError || !q.data) return <ErrorState erro={q.error} onRetry={() => q.refetch()} />;
  const d = q.data;
  return (
    <div className="space-y-6">
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {[
          ['Inscrições', d.totalInscricoes],
          ['Concluídas', d.inscricoesConcluidas],
          ['Taxa de conclusão', `${d.taxaConclusaoPercentual.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%`],
          ['Propostas atendidas', d.propostasAtendidas],
        ].map(([r, v]) => (
          <div key={r} className="card p-5">
            <p className="muted text-sm">{r}</p>
            <p className="mt-1 font-display text-3xl font-extrabold tabular-nums">{v}</p>
          </div>
        ))}
      </div>
      <div className="grid gap-6 lg:grid-cols-2">
        <GraficoInscricoesPorBairro dados={d.porBairro} />
        <GraficoDemandaReprimida dados={d.demandaReprimida} />
      </div>
    </div>
  );
}

function Cidadaos() {
  const [bairro, setBairro] = useState('');
  const [pagina, setPagina] = useState(1);
  const [detalheId, setDetalheId] = useState<number | null>(null);
  const [desativarId, setDesativarId] = useState<number | null>(null);
  const toast = useToast();
  const queryClient = useQueryClient();

  const q = useQuery({
    queryKey: ['admin', 'cidadaos', bairro, pagina],
    queryFn: () => cidadaosApi.listar({ bairro: bairro || undefined, pagina, tamanho: 10 }),
    placeholderData: keepPreviousData,
  });
  const detalhe = useQuery({ queryKey: ['cidadao', detalheId], queryFn: () => cidadaosApi.obter(detalheId!), enabled: detalheId !== null });

  const desativar = useMutation({
    mutationFn: (id: number) => cidadaosApi.desativar(id),
    onSuccess: () => {
      toast.sucesso('Cidadão desativado');
      setDesativarId(null);
      void queryClient.invalidateQueries({ queryKey: ['admin', 'cidadaos'] });
    },
    onError: (e) => {
      toast.erro('Não foi possível desativar', e.message);
      setDesativarId(null);
    },
  });

  return (
    <>
      <div className="mb-5 flex flex-wrap items-end justify-between gap-4">
        <Select rotulo="Bairro" className="w-56" value={bairro} onChange={(e) => { setBairro(e.target.value); setPagina(1); }}>
          <option value="">Todos os bairros</option>
          {BAIRROS_ZONA_SUL.map((b) => <option key={b}>{b}</option>)}
        </Select>
        <p className="muted flex items-center gap-1.5 text-xs"><ShieldCheck className="size-4 text-emerald-500" /> Lista sem e-mail e data de nascimento (minimização de dados, LGPD)</p>
      </div>
      {q.isLoading ? (
        <Skeleton className="h-72" />
      ) : q.isError ? (
        <ErrorState erro={q.error} onRetry={() => q.refetch()} />
      ) : !q.data?.itens.length ? (
        <EmptyState icone={Users} titulo="Nenhum cidadão encontrado" />
      ) : (
        <div className="card overflow-x-auto">
          <table className="w-full min-w-[640px] text-sm">
            <thead className="border-b border-[var(--border)]">
              <tr><th scope="col" className={thClasse}>Nome</th><th scope="col" className={thClasse}>Bairro</th><th scope="col" className={thClasse}>Escolaridade</th><th scope="col" className={thClasse}>Situação</th><th scope="col" className={cn(thClasse, 'text-right')}>Ações</th></tr>
            </thead>
            <tbody>
              {q.data.itens.map((c) => (
                <tr key={c.id} className="border-b border-[var(--border)] last:border-0">
                  <th scope="row" className={cn(tdClasse, 'text-left font-semibold')}>{c.nome}</th>
                  <td className={tdClasse}>{c.bairro}</td>
                  <td className={tdClasse}>{ESCOLARIDADE_ROTULO[c.escolaridade]}</td>
                  <td className={tdClasse}><StatusBadge tom={c.ativo ? 'green' : 'gray'}>{c.ativo ? 'Ativo' : 'Inativo'}</StatusBadge></td>
                  <td className={cn(tdClasse, 'text-right')}>
                    <div className="flex justify-end gap-1">
                      <Button variante="fantasma" tamanho="sm" icone={<Eye className="size-4" />} onClick={() => setDetalheId(c.id)} aria-label={`Ver ${c.nome}`}>Ver</Button>
                      {c.ativo && <Button variante="fantasma" tamanho="sm" className="text-rose-600" icone={<UserX className="size-4" />} onClick={() => setDesativarId(c.id)} aria-label={`Desativar ${c.nome}`}>Desativar</Button>}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      {q.data && <Pagination pagina={q.data.pagina} totalPaginas={q.data.totalPaginas} aoMudar={setPagina} />}

      <Modal aberto={detalheId !== null} aoFechar={() => setDetalheId(null)} titulo={detalhe.data?.nome ?? 'Cidadão'} descricao="Dados completos (acesso exclusivo do administrador)">
        {detalhe.isLoading ? <Skeleton className="h-40" /> : detalhe.data && (
          <dl className="grid gap-4 text-sm sm:grid-cols-2">
            {[
              ['E-mail', detalhe.data.email],
              ['Nascimento', formatarData(detalhe.data.dataNascimento)],
              ['Escolaridade', ESCOLARIDADE_ROTULO[detalhe.data.escolaridade]],
              ['Cadastro', formatarData(detalhe.data.criadoEm)],
              ['Endereço', `${detalhe.data.endereco.logradouro}${detalhe.data.endereco.numero ? ', ' + detalhe.data.endereco.numero : ''} · ${detalhe.data.endereco.bairro}, ${detalhe.data.endereco.cidade}/${detalhe.data.endereco.uf}`],
              ['Interesses', detalhe.data.areasInteresse.map((a) => AREA_INFO[a].rotulo).join(', ')],
            ].map(([k, v]) => (
              <div key={k}><dt className="muted text-xs font-bold uppercase tracking-wider">{k}</dt><dd className="mt-0.5 font-medium">{v}</dd></div>
            ))}
          </dl>
        )}
      </Modal>
      <ConfirmDialog aberto={desativarId !== null} titulo="Desativar cidadão?" descricao="A conta deixa de funcionar (exclusão lógica)." confirmar="Desativar" perigo carregando={desativar.isPending} aoConfirmar={() => desativarId && desativar.mutate(desativarId)} aoFechar={() => setDesativarId(null)} />
    </>
  );
}

function Organizacoes() {
  const [pagina, setPagina] = useState(1);
  const [desativarId, setDesativarId] = useState<number | null>(null);
  const toast = useToast();
  const queryClient = useQueryClient();
  const q = useQuery({ queryKey: ['organizacoes', 'admin', pagina], queryFn: () => organizacoesApi.listar({ pagina, tamanho: 10 }), placeholderData: keepPreviousData });
  const desativar = useMutation({
    mutationFn: (id: number) => organizacoesApi.desativar(id),
    onSuccess: () => {
      toast.sucesso('Organização desativada');
      setDesativarId(null);
      void queryClient.invalidateQueries({ queryKey: ['organizacoes'] });
    },
    onError: (e) => {
      toast.erro('Não foi possível desativar', e.message);
      setDesativarId(null);
    },
  });

  if (q.isLoading) return <Skeleton className="h-72" />;
  if (q.isError) return <ErrorState erro={q.error} onRetry={() => q.refetch()} />;
  return (
    <>
      <div className="card overflow-x-auto">
        <table className="w-full min-w-[640px] text-sm">
          <thead className="border-b border-[var(--border)]">
            <tr><th scope="col" className={thClasse}>Organização</th><th scope="col" className={thClasse}>Tipo</th><th scope="col" className={thClasse}>Bairro</th><th scope="col" className={thClasse}>Desde</th><th scope="col" className={cn(thClasse, 'text-right')}>Ações</th></tr>
          </thead>
          <tbody>
            {q.data?.itens.map((o) => (
              <tr key={o.id} className="border-b border-[var(--border)] last:border-0">
                <th scope="row" className={cn(tdClasse, 'text-left font-semibold')}><Link to={`/organizacoes/${o.id}`} className="hover:text-brand-600">{o.nome}</Link></th>
                <td className={tdClasse}>{TIPO_ORGANIZACAO_ROTULO[o.tipo]}</td>
                <td className={tdClasse}>{o.endereco.bairro}</td>
                <td className={tdClasse}>{formatarData(o.criadoEm)}</td>
                <td className={cn(tdClasse, 'text-right')}>
                  {o.ativo ? (
                    <Button variante="fantasma" tamanho="sm" className="text-rose-600" icone={<UserX className="size-4" />} onClick={() => setDesativarId(o.id)} aria-label={`Desativar ${o.nome}`}>Desativar</Button>
                  ) : (
                    <StatusBadge tom="gray">Inativa</StatusBadge>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {q.data && <Pagination pagina={q.data.pagina} totalPaginas={q.data.totalPaginas} aoMudar={setPagina} />}
      <ConfirmDialog aberto={desativarId !== null} titulo="Desativar organização?" descricao="Ela não poderá mais publicar oportunidades." confirmar="Desativar" perigo carregando={desativar.isPending} aoConfirmar={() => desativarId && desativar.mutate(desativarId)} aoFechar={() => setDesativarId(null)} />
    </>
  );
}

function Propostas() {
  const [arquivar, setArquivar] = useState<Proposta | null>(null);
  const toast = useToast();
  const queryClient = useQueryClient();
  const q = useQuery({ queryKey: ['propostas', 'admin'], queryFn: () => propostasApi.listar({ tamanho: 50, ordenarPorVotos: true }) });
  const mut = useMutation({
    mutationFn: (id: number) => propostasApi.arquivar(id),
    onSuccess: () => {
      toast.sucesso('Proposta arquivada');
      setArquivar(null);
      void queryClient.invalidateQueries({ queryKey: ['propostas'] });
    },
    onError: (e) => {
      toast.erro('Não foi possível arquivar', e.message);
      setArquivar(null);
    },
  });
  if (q.isLoading) return <Skeleton className="h-72" />;
  if (q.isError) return <ErrorState erro={q.error} onRetry={() => q.refetch()} />;
  return (
    <>
      <div className="card overflow-x-auto">
        <table className="w-full min-w-[680px] text-sm">
          <thead className="border-b border-[var(--border)]">
            <tr><th scope="col" className={thClasse}>Proposta</th><th scope="col" className={thClasse}>Área · bairro</th><th scope="col" className={cn(thClasse, 'text-right')}>Votos</th><th scope="col" className={thClasse}>Status</th><th scope="col" className={cn(thClasse, 'text-right')}>Ações</th></tr>
          </thead>
          <tbody>
            {q.data?.itens.map((p) => {
              const st = STATUS_PROPOSTA_INFO[p.status];
              return (
                <tr key={p.id} className="border-b border-[var(--border)] last:border-0">
                  <th scope="row" className={cn(tdClasse, 'text-left font-semibold')}><Link to={`/propostas/${p.id}`} className="hover:text-brand-600">{p.titulo}</Link><span className="muted block text-xs font-normal">por {p.autorNome}</span></th>
                  <td className={tdClasse}>{AREA_INFO[p.area].rotulo} · {p.bairro}</td>
                  <td className={cn(tdClasse, 'text-right font-bold tabular-nums')}>{p.totalVotos}</td>
                  <td className={tdClasse}><StatusBadge tom={st.tom}>{st.rotulo}</StatusBadge></td>
                  <td className={cn(tdClasse, 'text-right')}>
                    {p.status === 'Aberta' && <Button variante="fantasma" tamanho="sm" icone={<Archive className="size-4" />} onClick={() => setArquivar(p)} aria-label={`Arquivar ${p.titulo}`}>Arquivar</Button>}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
      <ConfirmDialog aberto={!!arquivar} titulo="Arquivar proposta?" descricao={<>“{arquivar?.titulo}” sai da votação.</>} confirmar="Arquivar" perigo carregando={mut.isPending} aoConfirmar={() => arquivar && mut.mutate(arquivar.id)} aoFechar={() => setArquivar(null)} />
    </>
  );
}

export default function AdminPage() {
  const [params, setParams] = useSearchParams();
  const aba = ABAS.find((a) => a.id === params.get('aba'))?.id ?? 'visao';
  return (
    <>
      <PageHeader
        selo="Administração"
        titulo="Painel administrativo"
        descricao="Acompanhe os indicadores e faça a moderação de cidadãos, organizações e propostas."
        acoes={<ButtonLink to="/impacto" variante="secundario" icone={<BarChart3 className="size-4" />}>Painel público</ButtonLink>}
      />
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6">
        <div className="-mx-4 overflow-x-auto px-4 sm:mx-0 sm:px-0">
          <div role="tablist" aria-label="Seções da administração" className="surface-2 inline-flex min-w-max gap-1 rounded-2xl p-1">
            {ABAS.map(({ id: a, rotulo, icone: Icone }) => (
              <button
                key={a}
                role="tab"
                aria-selected={aba === a}
                onClick={() => setParams(a === 'visao' ? {} : { aba: a }, { replace: true })}
                className={cn('relative flex items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-semibold transition', aba === a ? '' : 'muted hover:text-[var(--text)]')}
              >
                {aba === a && <motion.span layoutId="aba-admin" className="absolute inset-0 rounded-xl bg-[var(--surface)] shadow" />}
                <Icone className="relative size-4" aria-hidden />
                <span className="relative">{rotulo}</span>
              </button>
            ))}
          </div>
        </div>
        <section role="tabpanel" className="mt-8">
          {aba === 'visao' && <VisaoGeral />}
          {aba === 'cidadaos' && <Cidadaos />}
          {aba === 'organizacoes' && <Organizacoes />}
          {aba === 'propostas' && <Propostas />}
        </section>
      </div>
    </>
  );
}
