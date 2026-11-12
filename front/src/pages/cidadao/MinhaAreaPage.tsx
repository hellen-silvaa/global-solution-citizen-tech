import { useState, type FormEvent } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { motion } from 'motion/react';
import { Bookmark, CalendarCheck2, GraduationCap, Lightbulb, Plus, Sparkles, UserRound, XCircle } from 'lucide-react';
import { cidadaosApi, inscricoesApi, propostasApi } from '../../api/services';
import { ESCOLARIDADES, type AreaConhecimento, type Cidadao, type Endereco, type Escolaridade, type Inscricao } from '../../api/types';
import { useAuth } from '../../auth/AuthProvider';
import { useVotos } from '../../hooks/useVotos';
import { ESCOLARIDADE_ROTULO, STATUS_INSCRICAO_INFO } from '../../lib/labels';
import { formatarData, primeiroNome } from '../../lib/format';
import { cn } from '../../lib/cn';
import { PageHeader } from '../../components/ui/PageHeader';
import { OportunidadeCard } from '../../components/OportunidadeCard';
import { PropostaCard } from '../../components/PropostaCard';
import { CardsSkeleton, ErrorState, Skeleton } from '../../components/ui/Feedback';
import { EmptyState } from '../../components/ui/EmptyState';
import { StatusBadge } from '../../components/ui/Badge';
import { Button, ButtonLink } from '../../components/ui/Button';
import { ConfirmDialog } from '../../components/ui/Modal';
import { Input, Select } from '../../components/ui/Field';
import { AreasPicker } from '../../components/AreasPicker';
import { EnderecoFields } from '../../components/EnderecoFields';
import { NovaPropostaModal } from '../../components/PropostaForm';
import { useToast } from '../../components/ui/Toast';

const ABAS = [
  { id: 'recomendacoes', rotulo: 'Para você', icone: Sparkles },
  { id: 'inscricoes', rotulo: 'Minhas inscrições', icone: CalendarCheck2 },
  { id: 'propostas', rotulo: 'Minhas propostas', icone: Lightbulb },
  { id: 'perfil', rotulo: 'Meu perfil', icone: UserRound },
] as const;
type Aba = (typeof ABAS)[number]['id'];

function Recomendacoes({ id }: { id: number }) {
  const q = useQuery({ queryKey: ['recomendacoes', id], queryFn: () => cidadaosApi.recomendacoes(id, 9) });
  if (q.isLoading) return <CardsSkeleton quantidade={3} />;
  if (q.isError) return <ErrorState erro={q.error} onRetry={() => q.refetch()} />;
  if (!q.data?.length)
    return (
      <EmptyState
        icone={Sparkles}
        titulo="Sem recomendações no momento"
        descricao="Não há oportunidades abertas que combinem com você agora, ou você já se inscreveu nelas. Ajuste suas áreas de interesse no perfil ou explore todas."
        acao={<ButtonLink to="/oportunidades">Explorar oportunidades</ButtonLink>}
      />
    );
  return (
    <>
      <p className="muted mb-5 text-sm">
        Ordenadas por pontuação: interesse declarado <strong>+3</strong>, mesmo bairro <strong>+2</strong>, online <strong>+1</strong>, gratuita <strong>+1</strong>.
      </p>
      <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
        {q.data.map((r, i) => (
          <OportunidadeCard key={r.oportunidade.id} oportunidade={r.oportunidade} motivos={r.motivos} pontuacao={r.pontuacao} indice={i} />
        ))}
      </div>
    </>
  );
}

const ATIVAS = ['Pendente', 'Confirmada'];

function Inscricoes({ id }: { id: number }) {
  const q = useQuery({ queryKey: ['inscricoes', id], queryFn: () => cidadaosApi.inscricoes(id) });
  const [cancelando, setCancelando] = useState<Inscricao | null>(null);
  const toast = useToast();
  const queryClient = useQueryClient();

  const cancelar = useMutation({
    mutationFn: (inscricaoId: number) => inscricoesApi.cancelar(inscricaoId),
    onSuccess: () => {
      toast.sucesso('Inscrição cancelada', 'A vaga voltou para outra pessoa.');
      setCancelando(null);
      void queryClient.invalidateQueries({ queryKey: ['inscricoes'] });
      void queryClient.invalidateQueries({ queryKey: ['recomendacoes'] });
      void queryClient.invalidateQueries({ queryKey: ['oportunidades'] });
      void queryClient.invalidateQueries({ queryKey: ['oportunidade'] });
    },
    onError: (e) => {
      toast.erro('Não foi possível cancelar', e.message);
      setCancelando(null);
    },
  });

  if (q.isLoading) return <div className="space-y-3">{[0, 1, 2].map((i) => <Skeleton key={i} className="h-20" />)}</div>;
  if (q.isError) return <ErrorState erro={q.error} onRetry={() => q.refetch()} />;
  if (!q.data?.length)
    return <EmptyState icone={Bookmark} titulo="Você ainda não se inscreveu" descricao="Comece pelas recomendações feitas para você." acao={<ButtonLink to="/oportunidades">Ver oportunidades</ButtonLink>} />;

  const ativas = q.data.filter((i) => ATIVAS.includes(i.status)).length;
  const concluidas = q.data.filter((i) => i.status === 'Concluida').length;
  const ordenadas = [...q.data].sort((a, b) => b.criadaEm.localeCompare(a.criadaEm));

  return (
    <>
      <div className="mb-6 grid gap-4 sm:grid-cols-3">
        <div className="card p-5">
          <p className="muted text-sm">Inscrições ativas</p>
          <p className="mt-1 font-display text-3xl font-extrabold">{ativas}<span className="muted text-lg font-semibold">/3</span></p>
          <div className="mt-2 flex gap-1" aria-hidden>
            {[0, 1, 2].map((n) => <span key={n} className={cn('h-1.5 flex-1 rounded-full', n < ativas ? 'bg-brand-500' : 'bg-[var(--surface-2)]')} />)}
          </div>
        </div>
        <div className="card p-5">
          <p className="muted text-sm">Concluídas</p>
          <p className="mt-1 flex items-center gap-2 font-display text-3xl font-extrabold">{concluidas} <GraduationCap className="size-6 text-emerald-500" /></p>
        </div>
        <div className="card p-5">
          <p className="muted text-sm">Total</p>
          <p className="mt-1 font-display text-3xl font-extrabold">{q.data.length}</p>
        </div>
      </div>
      <ul className="space-y-3">
        {ordenadas.map((i, idx) => {
          const st = STATUS_INSCRICAO_INFO[i.status];
          return (
            <motion.li
              key={i.id}
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: idx * 0.04 }}
              className="card flex flex-wrap items-center gap-4 p-4 sm:p-5"
            >
              <div className="min-w-0 flex-1">
                <Link to={`/oportunidades/${i.oportunidadeId}`} className="font-bold hover:text-brand-600 dark:hover:text-brand-300">
                  {i.oportunidadeTitulo ?? `Oportunidade #${i.oportunidadeId}`}
                </Link>
                <p className="muted text-sm">Inscrição em {formatarData(i.criadaEm)}{i.atualizadaEm && i.atualizadaEm !== i.criadaEm ? ` · atualizada em ${formatarData(i.atualizadaEm)}` : ''}</p>
              </div>
              <StatusBadge tom={st.tom}>{st.rotulo}</StatusBadge>
              {ATIVAS.includes(i.status) && (
                <Button variante="fantasma" tamanho="sm" className="text-rose-600" icone={<XCircle className="size-4" />} onClick={() => setCancelando(i)}>
                  Cancelar
                </Button>
              )}
            </motion.li>
          );
        })}
      </ul>
      <ConfirmDialog
        aberto={!!cancelando}
        titulo="Cancelar inscrição?"
        descricao={<>Sua vaga em <strong>{cancelando?.oportunidadeTitulo}</strong> será liberada para outra pessoa. Só é possível cancelar antes do início.</>}
        confirmar="Cancelar inscrição"
        perigo
        carregando={cancelar.isPending}
        aoConfirmar={() => cancelando && cancelar.mutate(cancelando.id)}
        aoFechar={() => setCancelando(null)}
      />
    </>
  );
}

function MinhasPropostas({ id, bairro }: { id: number; bairro?: string }) {
  const votos = useVotos();
  const [criando, setCriando] = useState(false);
  // A API não filtra por autor: buscamos a primeira página grande e filtramos aqui.
  const q = useQuery({ queryKey: ['propostas', 'todas'], queryFn: () => propostasApi.listar({ tamanho: 50, ordenarPorVotos: true }) });
  const minhas = q.data?.itens.filter((p) => p.autorId === id) ?? [];
  return (
    <>
      <div className="mb-5 flex justify-end">
        <Button icone={<Plus className="size-4" />} onClick={() => setCriando(true)}>Nova proposta</Button>
      </div>
      {q.isLoading ? (
        <Skeleton className="h-36" />
      ) : q.isError ? (
        <ErrorState erro={q.error} onRetry={() => q.refetch()} />
      ) : minhas.length === 0 ? (
        <EmptyState icone={Lightbulb} titulo="Você ainda não fez propostas" descricao="Falta algum curso no seu bairro? Proponha e chame os vizinhos para votar." />
      ) : (
        <div className="space-y-4">
          {minhas.map((p, i) => (
            <PropostaCard key={p.id} proposta={p} indice={i} podeVotar={votos.podeVotar} jaVotou={votos.jaVotou(p.id)} votando={votos.votandoId === p.id} aoVotar={() => votos.votar(p.id)} />
          ))}
        </div>
      )}
      <NovaPropostaModal aberto={criando} aoFechar={() => setCriando(false)} bairroPadrao={bairro} />
    </>
  );
}

function Perfil({ cidadao }: { cidadao: Cidadao }) {
  const [nome, setNome] = useState(cidadao.nome);
  const [escolaridade, setEscolaridade] = useState<Escolaridade>(cidadao.escolaridade);
  const [areas, setAreas] = useState<AreaConhecimento[]>(cidadao.areasInteresse);
  const [endereco, setEndereco] = useState<Endereco>({ ...cidadao.endereco, numero: cidadao.endereco.numero ?? '' });
  const [desativando, setDesativando] = useState(false);
  const [erro, setErro] = useState<string>();
  const toast = useToast();
  const queryClient = useQueryClient();
  const { sair } = useAuth();
  const navegar = useNavigate();

  const salvar = useMutation({
    mutationFn: () => cidadaosApi.atualizar(cidadao.id, { nome: nome.trim(), escolaridade, areasInteresse: areas, endereco: { ...endereco, numero: endereco.numero || null } }),
    onSuccess: (c) => {
      queryClient.setQueryData(['cidadao', c.id], c);
      void queryClient.invalidateQueries({ queryKey: ['recomendacoes'] });
      toast.sucesso('Perfil atualizado', 'Suas recomendações foram recalculadas.');
    },
    onError: (e) => setErro(e.message),
  });

  const desativar = useMutation({
    mutationFn: () => cidadaosApi.desativar(cidadao.id),
    onSuccess: () => {
      toast.info('Conta desativada', 'Sentiremos sua falta.');
      sair();
      navegar('/');
    },
    onError: (e) => toast.erro('Não foi possível desativar', e.message),
  });

  function enviar(e: FormEvent) {
    e.preventDefault();
    setErro(undefined);
    if (!nome.trim()) return setErro('Informe o nome');
    if (areas.length === 0) return setErro('Escolha pelo menos uma área de interesse');
    salvar.mutate();
  }

  return (
    <div className="grid gap-6 lg:grid-cols-[1fr_20rem]">
      <form onSubmit={enviar} noValidate className="card space-y-6 p-6 sm:p-8">
        <div className="grid gap-4 sm:grid-cols-2">
          <Input rotulo="Nome" value={nome} maxLength={120} onChange={(e) => setNome(e.target.value)} />
          <Input rotulo="E-mail" value={cidadao.email} disabled ajuda="O e-mail não pode ser alterado" />
          <Select rotulo="Escolaridade" value={escolaridade} onChange={(e) => setEscolaridade(e.target.value as Escolaridade)}>
            {ESCOLARIDADES.map((x) => <option key={x} value={x}>{ESCOLARIDADE_ROTULO[x]}</option>)}
          </Select>
          <Input rotulo="Nascimento" value={formatarData(cidadao.dataNascimento)} disabled />
        </div>
        <AreasPicker valor={areas} aoMudar={setAreas} />
        <div>
          <h3 className="mb-3 text-sm font-bold">Endereço</h3>
          <EnderecoFields valor={endereco} aoMudar={setEndereco} />
        </div>
        {erro && <p role="alert" className="rounded-xl bg-rose-50 px-4 py-3 text-sm text-rose-700 dark:bg-rose-500/10 dark:text-rose-300">{erro}</p>}
        <div className="flex justify-end">
          <Button type="submit" carregando={salvar.isPending}>Salvar alterações</Button>
        </div>
      </form>
      <aside className="space-y-4">
        <div className="card p-6 text-sm">
          <p className="font-bold">Seus dados</p>
          <p className="muted mt-2 leading-relaxed">Usamos seu bairro e interesses só para recomendar oportunidades. Na lista do administrador não aparecem seu e-mail nem sua data de nascimento.</p>
          <p className="muted mt-3">Conta criada em {formatarData(cidadao.criadoEm)}.</p>
        </div>
        <div className="card border-rose-200 p-6 text-sm dark:border-rose-500/30">
          <p className="font-bold text-rose-700 dark:text-rose-300">Desativar conta</p>
          <p className="muted mt-2">Sua conta deixa de funcionar e você não poderá mais se inscrever.</p>
          <Button variante="secundario" tamanho="sm" className="mt-4 text-rose-600" onClick={() => setDesativando(true)}>Desativar minha conta</Button>
        </div>
      </aside>
      <ConfirmDialog
        aberto={desativando}
        titulo="Desativar sua conta?"
        descricao="Você será desconectado e não poderá mais entrar com este e-mail."
        confirmar="Desativar"
        perigo
        carregando={desativar.isPending}
        aoConfirmar={() => desativar.mutate()}
        aoFechar={() => setDesativando(false)}
      />
    </div>
  );
}

export default function MinhaAreaPage() {
  const { sessao } = useAuth();
  const id = sessao!.usuarioId;
  const [params, setParams] = useSearchParams();
  const aba = (ABAS.find((a) => a.id === params.get('aba'))?.id ?? 'recomendacoes') as Aba;
  const cidadao = useQuery({ queryKey: ['cidadao', id], queryFn: () => cidadaosApi.obter(id) });

  return (
    <>
      <PageHeader
        selo={cidadao.data ? `${cidadao.data.endereco.bairro} · ${ESCOLARIDADE_ROTULO[cidadao.data.escolaridade]}` : 'Minha área'}
        titulo={<>Olá, <span className="text-gradient">{primeiroNome(sessao!.nome)}</span> 👋</>}
        descricao="Aqui ficam suas recomendações, inscrições e propostas."
      />
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6">
        <div className="-mx-4 overflow-x-auto px-4 sm:mx-0 sm:px-0">
          <div role="tablist" aria-label="Seções da minha área" className="surface-2 inline-flex min-w-max gap-1 rounded-2xl p-1">
            {ABAS.map(({ id: a, rotulo, icone: Icone }) => (
              <button
                key={a}
                role="tab"
                id={`aba-${a}`}
                aria-selected={aba === a}
                aria-controls={`painel-${a}`}
                onClick={() => setParams(a === 'recomendacoes' ? {} : { aba: a }, { replace: true })}
                className={cn('relative flex items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-semibold transition', aba === a ? 'text-[var(--text)]' : 'muted hover:text-[var(--text)]')}
              >
                {aba === a && <motion.span layoutId="aba-cidadao" className="absolute inset-0 rounded-xl bg-[var(--surface)] shadow" />}
                <Icone className="relative size-4" aria-hidden />
                <span className="relative">{rotulo}</span>
              </button>
            ))}
          </div>
        </div>
        <section role="tabpanel" id={`painel-${aba}`} aria-labelledby={`aba-${aba}`} className="mt-8">
          {aba === 'recomendacoes' && <Recomendacoes id={id} />}
          {aba === 'inscricoes' && <Inscricoes id={id} />}
          {aba === 'propostas' && <MinhasPropostas id={id} bairro={cidadao.data?.endereco.bairro} />}
          {aba === 'perfil' &&
            (cidadao.isLoading ? <Skeleton className="h-96" /> : cidadao.isError || !cidadao.data ? <ErrorState erro={cidadao.error} onRetry={() => cidadao.refetch()} /> : <Perfil cidadao={cidadao.data} />)}
        </section>
      </div>
    </>
  );
}
