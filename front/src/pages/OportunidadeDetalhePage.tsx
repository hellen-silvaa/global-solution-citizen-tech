import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { motion } from 'motion/react';
import confettiCores from '../lib/confetti';
import {
  ArrowLeft,
  Ban,
  Building2,
  CalendarDays,
  CheckCircle2,
  Clock,
  Lock,
  MapPin,
  Pencil,
  ShieldCheck,
  Users,
  Wifi,
} from 'lucide-react';
import { cidadaosApi, oportunidadesApi } from '../api/services';
import { useAuth } from '../auth/AuthProvider';
import { AREA_INFO, MODALIDADE_ROTULO, STATUS_OPORTUNIDADE_INFO, TIPO_OPORTUNIDADE_ROTULO } from '../lib/labels';
import { formatarData, rotuloPrazo } from '../lib/format';
import { AreaIcon } from '../components/AreaIcon';
import { Badge, StatusBadge } from '../components/ui/Badge';
import { Button, ButtonLink } from '../components/ui/Button';
import { ErrorState, Skeleton } from '../components/ui/Feedback';
import { ConfirmDialog } from '../components/ui/Modal';
import { useToast } from '../components/ui/Toast';
import { VagasBarra } from '../components/OportunidadeCard';

export default function OportunidadeDetalhePage() {
  const id = Number(useParams().id);
  const { sessao } = useAuth();
  const navegar = useNavigate();
  const toast = useToast();
  const queryClient = useQueryClient();
  const [confirmar, setConfirmar] = useState<'encerrar' | 'cancelar' | null>(null);
  const [comemorar, setComemorar] = useState(false);

  const q = useQuery({ queryKey: ['oportunidade', id], queryFn: () => oportunidadesApi.obter(id) });
  const ehCidadao = sessao?.perfil === 'Cidadao';
  const minhas = useQuery({
    queryKey: ['inscricoes', sessao?.usuarioId],
    queryFn: () => cidadaosApi.inscricoes(sessao!.usuarioId),
    enabled: ehCidadao,
  });

  const inscricaoAtual = minhas.data?.find((i) => i.oportunidadeId === id && (i.status === 'Pendente' || i.status === 'Confirmada'));
  const concluiu = minhas.data?.some((i) => i.oportunidadeId === id && i.status === 'Concluida');

  const inscrever = useMutation({
    mutationFn: () => oportunidadesApi.inscrever(id),
    onSuccess: () => {
      setComemorar(true);
      toast.sucesso('Inscrição feita!', 'A organização vai confirmar sua vaga. Acompanhe em "Minha área".');
      void queryClient.invalidateQueries({ queryKey: ['oportunidade', id] });
      void queryClient.invalidateQueries({ queryKey: ['inscricoes'] });
      void queryClient.invalidateQueries({ queryKey: ['recomendacoes'] });
      void queryClient.invalidateQueries({ queryKey: ['oportunidades'] });
      void queryClient.invalidateQueries({ queryKey: ['impacto'] });
      setTimeout(() => setComemorar(false), 2600);
    },
    onError: (e) => toast.erro('Não foi possível se inscrever', e.message),
  });

  const acaoGestao = useMutation({
    mutationFn: (acao: 'encerrar' | 'cancelar') => (acao === 'encerrar' ? oportunidadesApi.encerrar(id) : oportunidadesApi.cancelar(id)),
    onSuccess: (_, acao) => {
      toast.sucesso(acao === 'encerrar' ? 'Inscrições encerradas' : 'Oportunidade cancelada');
      setConfirmar(null);
      void queryClient.invalidateQueries({ queryKey: ['oportunidade', id] });
      void queryClient.invalidateQueries({ queryKey: ['oportunidades'] });
    },
    onError: (e) => {
      toast.erro('Não foi possível concluir', e.message);
      setConfirmar(null);
    },
  });

  if (q.isLoading) {
    return (
      <div className="mx-auto max-w-5xl space-y-4 px-4 py-14 sm:px-6">
        <Skeleton className="h-8 w-40" />
        <Skeleton className="h-14 w-3/4" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }
  if (q.isError || !q.data) {
    return (
      <div className="mx-auto max-w-xl px-4 py-24">
        <ErrorState erro={q.error} />
      </div>
    );
  }

  const o = q.data;
  const status = STATUS_OPORTUNIDADE_INFO[o.status];
  const ehDona = sessao?.perfil === 'Organizacao' && sessao.usuarioId === o.organizacaoId;
  const podeGerir = ehDona || sessao?.perfil === 'Admin';
  const jaComecou = new Date(o.dataInicio) <= new Date();
  const podeInscrever = o.status === 'Aberta' && o.vagasDisponiveis > 0 && !jaComecou;

  const infos = [
    { icone: o.modalidade === 'Online' ? Wifi : MapPin, rotulo: 'Onde', valor: o.modalidade === 'Online' ? 'Online' : `${o.bairro} · ${MODALIDADE_ROTULO[o.modalidade]}` },
    { icone: CalendarDays, rotulo: 'Início', valor: formatarData(o.dataInicio) },
    { icone: CalendarDays, rotulo: 'Término', valor: formatarData(o.dataFim) },
    { icone: Clock, rotulo: 'Carga horária', valor: `${o.cargaHoraria} horas` },
    { icone: Users, rotulo: 'Vagas', valor: `${o.vagasTotais} no total` },
  ];

  function blocoInscricao() {
    if (!sessao) {
      return (
        <>
          <ButtonLink to="/entrar" state={{ de: `/oportunidades/${id}` }} tamanho="lg" className="w-full">
            Entrar para se inscrever
          </ButtonLink>
          <p className="muted text-center text-sm">
            Ainda não tem conta? <Link to="/cadastro" className="font-semibold text-brand-600 hover:underline dark:text-brand-300">Cadastre-se grátis</Link>
          </p>
        </>
      );
    }
    if (!ehCidadao) {
      return <p className="muted text-sm">Inscrições são feitas por cidadãos. Você entrou como {sessao.perfil === 'Admin' ? 'administrador' : 'organização'}.</p>;
    }
    if (inscricaoAtual) {
      return (
        <div className="rounded-2xl bg-emerald-50 p-4 text-emerald-800 dark:bg-emerald-500/10 dark:text-emerald-300">
          <p className="flex items-center gap-2 font-bold"><CheckCircle2 className="size-5" /> Você está inscrito</p>
          <p className="mt-1 text-sm">Status: {inscricaoAtual.status === 'Pendente' ? 'aguardando confirmação' : 'vaga confirmada'}.</p>
          <Link to="/minha-area?aba=inscricoes" className="mt-2 inline-block text-sm font-semibold underline">Ver minhas inscrições</Link>
        </div>
      );
    }
    if (concluiu) {
      return <p className="rounded-2xl bg-emerald-50 p-4 font-semibold text-emerald-800 dark:bg-emerald-500/10 dark:text-emerald-300">🎓 Você já concluiu esta oportunidade. Parabéns!</p>;
    }
    return (
      <>
        <Button tamanho="lg" className="w-full" variante="sol" disabled={!podeInscrever} carregando={inscrever.isPending} onClick={() => inscrever.mutate()}>
          {podeInscrever ? 'Quero me inscrever' : o.status !== 'Aberta' ? 'Inscrições fechadas' : o.vagasDisponiveis === 0 ? 'Vagas esgotadas' : 'Prazo encerrado'}
        </Button>
        <p className="muted flex items-start gap-1.5 text-xs">
          <ShieldCheck className="mt-0.5 size-3.5 shrink-0" aria-hidden />
          Para distribuir melhor as vagas gratuitas, cada pessoa pode ter até 3 inscrições ativas.
        </p>
      </>
    );
  }

  return (
    <div className="relative">
      {comemorar && <Confete />}
      <div className={`h-56 bg-gradient-to-br ${AREA_INFO[o.area].cor} relative overflow-hidden`}>
        <div className="grid-bg absolute inset-0 opacity-40 invert" aria-hidden />
        <div className="absolute inset-0 bg-gradient-to-t from-[var(--bg)] via-transparent" aria-hidden />
      </div>
      <div className="relative mx-auto -mt-40 max-w-6xl px-4 sm:px-6">
        <button onClick={() => navegar(-1)} className="mb-5 inline-flex items-center gap-1.5 rounded-xl bg-white/80 px-3 py-1.5 text-sm font-semibold text-slate-800 backdrop-blur hover:bg-white">
          <ArrowLeft className="size-4" /> Voltar
        </button>
        <div className="grid gap-6 lg:grid-cols-[1fr_22rem]">
          <motion.article initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} className="card p-6 sm:p-8">
            <div className="flex flex-wrap items-center gap-3">
              <AreaIcon area={o.area} className="size-14" />
              <div>
                <p className="text-sm font-bold uppercase tracking-wider text-brand-600 dark:text-brand-300">{TIPO_OPORTUNIDADE_ROTULO[o.tipo]} · {AREA_INFO[o.area].rotulo}</p>
                <div className="mt-1 flex flex-wrap gap-2">
                  <StatusBadge tom={status.tom}>{status.rotulo}</StatusBadge>
                  {o.gratuita && <Badge tom="green">Gratuita</Badge>}
                </div>
              </div>
            </div>
            <h1 className="mt-6 text-3xl font-extrabold sm:text-4xl">{o.titulo}</h1>
            <Link to={`/organizacoes/${o.organizacaoId}`} className="muted mt-2 inline-flex items-center gap-1.5 font-semibold hover:text-brand-600">
              <Building2 className="size-4" /> {o.organizacaoNome}
            </Link>
            <h2 className="mt-8 text-lg font-bold">Sobre</h2>
            <p className="muted mt-2 whitespace-pre-line text-base leading-relaxed">{o.descricao}</p>

            <dl className="mt-8 grid gap-3 sm:grid-cols-2">
              {infos.map(({ icone: Icone, rotulo, valor }) => (
                <div key={rotulo} className="surface-2 flex items-center gap-3 rounded-2xl p-4">
                  <Icone className="size-5 text-brand-500" aria-hidden />
                  <div>
                    <dt className="muted text-xs font-semibold uppercase tracking-wider">{rotulo}</dt>
                    <dd className="font-semibold">{valor}</dd>
                  </div>
                </div>
              ))}
            </dl>
          </motion.article>

          <motion.aside initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.1 }} className="space-y-4 lg:sticky lg:top-24 lg:self-start">
            <div className="card space-y-4 p-6">
              {o.status === 'Aberta' && <VagasBarra disponiveis={o.vagasDisponiveis} totais={o.vagasTotais} />}
              {o.status === 'Aberta' && <p className="text-sm font-semibold">{rotuloPrazo(o.dataInicio)}</p>}
              {blocoInscricao()}
            </div>
            {podeGerir && (
              <div className="card space-y-3 p-6">
                <p className="flex items-center gap-2 font-bold"><Lock className="size-4 text-brand-500" /> Gestão</p>
                <ButtonLink to={`/organizacao/oportunidades/${o.id}/inscritos`} variante="secundario" className="w-full" icone={<Users className="size-4" />}>
                  Ver inscritos
                </ButtonLink>
                {o.status === 'Aberta' && (
                  <>
                    <ButtonLink to={`/organizacao/oportunidades/${o.id}/editar`} variante="secundario" className="w-full" icone={<Pencil className="size-4" />}>
                      Editar
                    </ButtonLink>
                    <Button variante="secundario" className="w-full" onClick={() => setConfirmar('encerrar')} icone={<Lock className="size-4" />}>
                      Encerrar inscrições
                    </Button>
                  </>
                )}
                {o.status !== 'Cancelada' && (
                  <Button variante="fantasma" className="w-full text-rose-600" onClick={() => setConfirmar('cancelar')} icone={<Ban className="size-4" />}>
                    Cancelar oportunidade
                  </Button>
                )}
              </div>
            )}
          </motion.aside>
        </div>
      </div>
      <ConfirmDialog
        aberto={confirmar !== null}
        titulo={confirmar === 'encerrar' ? 'Encerrar inscrições?' : 'Cancelar oportunidade?'}
        descricao={
          confirmar === 'encerrar'
            ? 'Ninguém mais poderá se inscrever. Quem já está inscrito continua.'
            : 'A oportunidade deixa de acontecer. Esta ação não pode ser desfeita.'
        }
        confirmar={confirmar === 'encerrar' ? 'Encerrar' : 'Cancelar oportunidade'}
        perigo={confirmar === 'cancelar'}
        carregando={acaoGestao.isPending}
        aoConfirmar={() => confirmar && acaoGestao.mutate(confirmar)}
        aoFechar={() => setConfirmar(null)}
      />
    </div>
  );
}

function Confete() {
  return (
    <div className="pointer-events-none fixed inset-0 z-[80] overflow-hidden" aria-hidden>
      {Array.from({ length: 60 }, (_, i) => (
        <motion.span
          key={i}
          className="absolute top-0 block h-3 w-2 rounded-sm"
          style={{ left: `${(i * 37) % 100}%`, background: confettiCores[i % confettiCores.length] }}
          initial={{ y: -40, rotate: 0, opacity: 1 }}
          animate={{ y: '105vh', rotate: 360 + ((i * 53) % 360), opacity: [1, 1, 0] }}
          transition={{ duration: 1.8 + ((i * 7) % 10) / 10, delay: ((i * 13) % 10) / 20, ease: 'easeIn' }}
        />
      ))}
    </div>
  );
}
