import { useState, type FormEvent } from 'react';
import { Link, useSearchParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { motion } from 'motion/react';
import { Briefcase, Building2, CalendarDays, HandHeart, Pencil, Plus, Users } from 'lucide-react';
import { oportunidadesApi, organizacoesApi, propostasApi } from '../../api/services';
import type { Endereco, Organizacao, Oportunidade } from '../../api/types';
import { useAuth } from '../../auth/AuthProvider';
import { AREA_INFO, STATUS_OPORTUNIDADE_INFO, TIPO_OPORTUNIDADE_ROTULO, TIPO_ORGANIZACAO_ROTULO } from '../../lib/labels';
import { formatarPeriodo } from '../../lib/format';
import { cn } from '../../lib/cn';
import { PageHeader } from '../../components/ui/PageHeader';
import { ButtonLink, Button } from '../../components/ui/Button';
import { ErrorState, Skeleton } from '../../components/ui/Feedback';
import { EmptyState } from '../../components/ui/EmptyState';
import { StatusBadge } from '../../components/ui/Badge';
import { AreaIcon } from '../../components/AreaIcon';
import { PropostaCard } from '../../components/PropostaCard';
import { Input, Textarea } from '../../components/ui/Field';
import { EnderecoFields } from '../../components/EnderecoFields';
import { useToast } from '../../components/ui/Toast';

const ABAS = [
  { id: 'oportunidades', rotulo: 'Oportunidades', icone: Briefcase },
  { id: 'propostas', rotulo: 'Pedidos da comunidade', icone: HandHeart },
  { id: 'perfil', rotulo: 'Perfil da organização', icone: Building2 },
] as const;

function LinhaOportunidade({ o, indice }: { o: Oportunidade; indice: number }) {
  const st = STATUS_OPORTUNIDADE_INFO[o.status];
  const ocupadas = o.vagasTotais - o.vagasDisponiveis;
  return (
    <motion.li initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: indice * 0.04 }} className="card flex flex-col gap-4 p-5 md:flex-row md:items-center">
      <div className="flex min-w-0 flex-1 items-center gap-4">
        <AreaIcon area={o.area} />
        <div className="min-w-0">
          <Link to={`/oportunidades/${o.id}`} className="block truncate font-bold hover:text-brand-600 dark:hover:text-brand-300">{o.titulo}</Link>
          <p className="muted flex flex-wrap items-center gap-x-3 text-sm">
            <span>{TIPO_OPORTUNIDADE_ROTULO[o.tipo]} · {AREA_INFO[o.area].rotulo}</span>
            <span className="flex items-center gap-1"><CalendarDays className="size-3.5" />{formatarPeriodo(o.dataInicio, o.dataFim)}</span>
          </p>
        </div>
      </div>
      <div className="flex flex-wrap items-center gap-3">
        <span className="text-sm font-semibold tabular-nums">{ocupadas}/{o.vagasTotais} <span className="muted font-normal">inscritos</span></span>
        <StatusBadge tom={st.tom}>{st.rotulo}</StatusBadge>
        <ButtonLink to={`/organizacao/oportunidades/${o.id}/inscritos`} variante="secundario" tamanho="sm" icone={<Users className="size-4" />}>Inscritos</ButtonLink>
        {o.status === 'Aberta' && (
          <ButtonLink to={`/organizacao/oportunidades/${o.id}/editar`} variante="fantasma" tamanho="sm" icone={<Pencil className="size-4" />} aria-label={`Editar ${o.titulo}`}>
            Editar
          </ButtonLink>
        )}
      </div>
    </motion.li>
  );
}

function MinhasOportunidades({ id }: { id: number }) {
  const q = useQuery({ queryKey: ['oportunidades', { organizacaoId: id, gestao: true }], queryFn: () => oportunidadesApi.buscar({ organizacaoId: id, tamanho: 50 }) });
  if (q.isLoading) return <div className="space-y-3">{[0, 1, 2].map((i) => <Skeleton key={i} className="h-24" />)}</div>;
  if (q.isError) return <ErrorState erro={q.error} onRetry={() => q.refetch()} />;
  const itens = q.data?.itens ?? [];
  const abertas = itens.filter((o) => o.status === 'Aberta');
  const inscritos = itens.reduce((s, o) => s + (o.vagasTotais - o.vagasDisponiveis), 0);
  const vagas = abertas.reduce((s, o) => s + o.vagasDisponiveis, 0);

  return (
    <>
      <div className="mb-6 grid gap-4 sm:grid-cols-3">
        {[
          ['Oportunidades abertas', abertas.length],
          ['Pessoas inscritas', inscritos],
          ['Vagas ainda livres', vagas],
        ].map(([r, v]) => (
          <div key={r} className="card p-5">
            <p className="muted text-sm">{r}</p>
            <p className="mt-1 font-display text-3xl font-extrabold tabular-nums">{v}</p>
          </div>
        ))}
      </div>
      {itens.length === 0 ? (
        <EmptyState icone={Briefcase} titulo="Publique sua primeira oportunidade" descricao="Cursos, oficinas, mentorias ou vagas de jovem aprendiz." acao={<ButtonLink to="/organizacao/oportunidades/nova">Nova oportunidade</ButtonLink>} />
      ) : (
        <ul className="space-y-3">{itens.map((o, i) => <LinhaOportunidade key={o.id} o={o} indice={i} />)}</ul>
      )}
    </>
  );
}

function PedidosComunidade() {
  const q = useQuery({ queryKey: ['propostas', 'abertas-org'], queryFn: () => propostasApi.listar({ status: 'Aberta', ordenarPorVotos: true, tamanho: 50 }) });
  if (q.isLoading) return <Skeleton className="h-40" />;
  if (q.isError) return <ErrorState erro={q.error} onRetry={() => q.refetch()} />;
  if (!q.data?.itens.length) return <EmptyState icone={HandHeart} titulo="Nenhum pedido em aberto" descricao="Quando a comunidade propuser algo, aparece aqui." />;
  return (
    <>
      <p className="muted mb-5 text-sm">Propostas ordenadas por votos. Abra uma e clique em <strong>Atender esta proposta</strong> para publicar a oportunidade que responde ao pedido.</p>
      <div className="space-y-4">{q.data.itens.map((p, i) => <PropostaCard key={p.id} proposta={p} indice={i} posicao={i + 1} />)}</div>
    </>
  );
}

function PerfilOrganizacao({ org }: { org: Organizacao }) {
  const [nome, setNome] = useState(org.nome);
  const [descricao, setDescricao] = useState(org.descricao);
  const [site, setSite] = useState(org.site ?? '');
  const [endereco, setEndereco] = useState<Endereco>({ ...org.endereco, numero: org.endereco.numero ?? '' });
  const [erro, setErro] = useState<string>();
  const toast = useToast();
  const queryClient = useQueryClient();

  const salvar = useMutation({
    mutationFn: () => organizacoesApi.atualizar(org.id, { nome: nome.trim(), descricao: descricao.trim(), site: site.trim() || null, endereco: { ...endereco, numero: endereco.numero || null } }),
    onSuccess: (o) => {
      queryClient.setQueryData(['organizacao', o.id], o);
      void queryClient.invalidateQueries({ queryKey: ['organizacoes'] });
      toast.sucesso('Perfil atualizado');
    },
    onError: (e) => setErro(e.message),
  });

  function enviar(e: FormEvent) {
    e.preventDefault();
    setErro(undefined);
    if (!nome.trim() || !descricao.trim()) return setErro('Nome e descrição são obrigatórios');
    if (site && !/^https?:\/\/\S+\.\S+/.test(site)) return setErro('O site precisa começar com http:// ou https://');
    salvar.mutate();
  }

  return (
    <form onSubmit={enviar} noValidate className="card max-w-3xl space-y-5 p-6 sm:p-8">
      <div className="grid gap-4 sm:grid-cols-2">
        <Input rotulo="Nome" value={nome} maxLength={120} onChange={(e) => setNome(e.target.value)} />
        <Input rotulo="Tipo" value={TIPO_ORGANIZACAO_ROTULO[org.tipo]} disabled />
      </div>
      <Textarea rotulo="Descrição" value={descricao} maxLength={500} onChange={(e) => setDescricao(e.target.value)} />
      <Input rotulo="Site" opcional type="url" value={site} maxLength={200} onChange={(e) => setSite(e.target.value)} placeholder="https://" />
      <div>
        <h3 className="mb-3 text-sm font-bold">Endereço</h3>
        <EnderecoFields valor={endereco} aoMudar={setEndereco} />
      </div>
      {erro && <p role="alert" className="rounded-xl bg-rose-50 px-4 py-3 text-sm text-rose-700 dark:bg-rose-500/10 dark:text-rose-300">{erro}</p>}
      <div className="flex justify-between gap-2">
        <ButtonLink to={`/organizacoes/${org.id}`} variante="fantasma">Ver página pública</ButtonLink>
        <Button type="submit" carregando={salvar.isPending}>Salvar</Button>
      </div>
    </form>
  );
}

export default function PainelOrganizacaoPage() {
  const { sessao } = useAuth();
  const id = sessao!.usuarioId;
  const [params, setParams] = useSearchParams();
  const aba = ABAS.find((a) => a.id === params.get('aba'))?.id ?? 'oportunidades';
  const org = useQuery({ queryKey: ['organizacao', id], queryFn: () => organizacoesApi.obter(id) });

  return (
    <>
      <PageHeader
        selo={org.data ? TIPO_ORGANIZACAO_ROTULO[org.data.tipo] : 'Painel da organização'}
        titulo={sessao!.nome}
        descricao="Publique oportunidades, acompanhe inscritos e responda aos pedidos da comunidade."
        acoes={<ButtonLink to="/organizacao/oportunidades/nova" tamanho="lg" variante="sol" icone={<Plus className="size-5" />}>Nova oportunidade</ButtonLink>}
      />
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6">
        <div className="-mx-4 overflow-x-auto px-4 sm:mx-0 sm:px-0">
          <div role="tablist" aria-label="Seções do painel" className="surface-2 inline-flex min-w-max gap-1 rounded-2xl p-1">
            {ABAS.map(({ id: a, rotulo, icone: Icone }) => (
              <button
                key={a}
                role="tab"
                aria-selected={aba === a}
                onClick={() => setParams(a === 'oportunidades' ? {} : { aba: a }, { replace: true })}
                className={cn('relative flex items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-semibold transition', aba === a ? '' : 'muted hover:text-[var(--text)]')}
              >
                {aba === a && <motion.span layoutId="aba-org" className="absolute inset-0 rounded-xl bg-[var(--surface)] shadow" />}
                <Icone className="relative size-4" aria-hidden />
                <span className="relative">{rotulo}</span>
              </button>
            ))}
          </div>
        </div>
        <section role="tabpanel" className="mt-8">
          {aba === 'oportunidades' && <MinhasOportunidades id={id} />}
          {aba === 'propostas' && <PedidosComunidade />}
          {aba === 'perfil' && (org.isLoading ? <Skeleton className="h-96" /> : org.data ? <PerfilOrganizacao org={org.data} /> : <ErrorState erro={org.error} />)}
        </section>
      </div>
    </>
  );
}
