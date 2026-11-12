import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { motion } from 'motion/react';
import { Archive, ArrowLeft, ArrowUpRight, ChevronUp, HandHeart, MapPin } from 'lucide-react';
import { propostasApi } from '../api/services';
import { useAuth } from '../auth/AuthProvider';
import { useVotos } from '../hooks/useVotos';
import { AREA_INFO, STATUS_PROPOSTA_INFO } from '../lib/labels';
import { formatarData } from '../lib/format';
import { AreaIcon } from '../components/AreaIcon';
import { StatusBadge } from '../components/ui/Badge';
import { Button, ButtonLink } from '../components/ui/Button';
import { ErrorState, Skeleton } from '../components/ui/Feedback';
import { ConfirmDialog, Modal } from '../components/ui/Modal';
import { useToast } from '../components/ui/Toast';
import { OportunidadeForm, paraCriacao, valoresIniciais } from '../components/OportunidadeForm';

export default function PropostaDetalhePage() {
  const id = Number(useParams().id);
  const { sessao } = useAuth();
  const votos = useVotos();
  const navegar = useNavigate();
  const toast = useToast();
  const queryClient = useQueryClient();
  const [atendendo, setAtendendo] = useState(false);
  const [arquivando, setArquivando] = useState(false);

  const q = useQuery({ queryKey: ['proposta', id], queryFn: () => propostasApi.obter(id) });

  const arquivar = useMutation({
    mutationFn: () => propostasApi.arquivar(id),
    onSuccess: () => {
      toast.sucesso('Proposta arquivada');
      setArquivando(false);
      void queryClient.invalidateQueries({ queryKey: ['proposta', id] });
      void queryClient.invalidateQueries({ queryKey: ['propostas'] });
    },
    onError: (e) => {
      toast.erro('Não foi possível arquivar', e.message);
      setArquivando(false);
    },
  });

  if (q.isLoading) return <div className="mx-auto max-w-3xl px-4 py-16"><Skeleton className="h-80" /></div>;
  if (q.isError || !q.data) return <div className="mx-auto max-w-xl px-4 py-24"><ErrorState erro={q.error} /></div>;

  const p = q.data;
  const status = STATUS_PROPOSTA_INFO[p.status];
  const aberta = p.status === 'Aberta';
  const podeArquivar = aberta && (sessao?.perfil === 'Admin' || (sessao?.perfil === 'Cidadao' && sessao.usuarioId === p.autorId));
  const podeAtender = aberta && sessao?.perfil === 'Organizacao';

  return (
    <div className="mx-auto max-w-3xl px-4 py-10 sm:px-6">
      <Link to="/propostas" className="muted mb-6 inline-flex items-center gap-1.5 text-sm font-semibold hover:text-brand-600">
        <ArrowLeft className="size-4" /> Todas as propostas
      </Link>
      <motion.article initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} className="card overflow-hidden">
        <div className={`bg-gradient-to-br ${AREA_INFO[p.area].cor} p-6 text-white sm:p-8`}>
          <div className="flex items-center gap-3">
            <span className="rounded-2xl bg-white/20 p-1 backdrop-blur">
              <AreaIcon area={p.area} className="size-11 shadow-none" />
            </span>
            <div>
              <p className="text-sm font-bold uppercase tracking-wider opacity-90">{AREA_INFO[p.area].rotulo}</p>
              <p className="flex items-center gap-1 text-sm"><MapPin className="size-4" /> {p.bairro}</p>
            </div>
          </div>
          <h1 className="mt-5 text-3xl font-extrabold sm:text-4xl">{p.titulo}</h1>
        </div>
        <div className="p-6 sm:p-8">
          <div className="flex flex-wrap items-center gap-3">
            <StatusBadge tom={status.tom}>{status.rotulo}</StatusBadge>
            <span className="muted text-sm">Proposta por <strong>{p.autorNome}</strong> em {formatarData(p.criadaEm)}</span>
          </div>
          <p className="mt-6 whitespace-pre-line text-lg leading-relaxed">{p.descricao}</p>

          <div className="surface-2 mt-8 flex flex-wrap items-center gap-5 rounded-2xl p-5">
            <div className="text-center">
              <motion.p key={p.totalVotos} initial={{ scale: 1.5 }} animate={{ scale: 1 }} className="font-display text-5xl font-extrabold text-brand-600 dark:text-brand-300">{p.totalVotos}</motion.p>
              <p className="muted text-sm font-semibold">{p.totalVotos === 1 ? 'voto' : 'votos'}</p>
            </div>
            <div className="flex-1">
              {p.status === 'Atendida' && p.oportunidadeGeradaId ? (
                <div>
                  <p className="font-bold text-emerald-700 dark:text-emerald-300">🎉 Uma organização atendeu este pedido!</p>
                  <ButtonLink to={`/oportunidades/${p.oportunidadeGeradaId}`} variante="secundario" tamanho="sm" className="mt-2">
                    Ver a oportunidade criada <ArrowUpRight className="size-4" />
                  </ButtonLink>
                </div>
              ) : aberta ? (
                votos.podeVotar ? (
                  <Button
                    tamanho="lg"
                    onClick={() => votos.votar(p.id)}
                    disabled={votos.jaVotou(p.id)}
                    carregando={votos.votandoId === p.id}
                    icone={<ChevronUp className="size-5" />}
                  >
                    {votos.jaVotou(p.id) ? 'Você já votou' : 'Apoiar com meu voto'}
                  </Button>
                ) : !sessao ? (
                  <ButtonLink to="/entrar" state={{ de: `/propostas/${p.id}` }}>Entre para votar</ButtonLink>
                ) : (
                  <p className="muted text-sm">Só cidadãos votam. Organizações podem atender a proposta.</p>
                )
              ) : (
                <p className="muted text-sm">Esta proposta não recebe mais votos.</p>
              )}
            </div>
          </div>

          {(podeAtender || podeArquivar) && (
            <div className="mt-6 flex flex-wrap gap-2 border-t border-[var(--border)] pt-6">
              {podeAtender && (
                <Button variante="sol" icone={<HandHeart className="size-5" />} onClick={() => setAtendendo(true)}>
                  Atender esta proposta
                </Button>
              )}
              {podeArquivar && (
                <Button variante="secundario" icone={<Archive className="size-4" />} onClick={() => setArquivando(true)}>
                  Arquivar
                </Button>
              )}
            </div>
          )}
        </div>
      </motion.article>

      {podeAtender && (
        <Modal
          aberto={atendendo}
          aoFechar={() => setAtendendo(false)}
          largura="lg"
          titulo="Atender proposta"
          descricao="Publique uma oportunidade que responde ao pedido da comunidade. A proposta fica marcada como atendida."
        >
          <OportunidadeForm
            inicial={valoresIniciais(undefined, { titulo: p.titulo, descricao: p.descricao, area: p.area, bairro: p.bairro })}
            textoEnviar="Publicar e atender"
            aoCancelar={() => setAtendendo(false)}
            aoEnviar={async (v) => {
              const o = await propostasApi.atender(p.id, paraCriacao(v));
              toast.sucesso('Proposta atendida!', 'A oportunidade foi publicada e a comunidade já pode se inscrever.');
              void queryClient.invalidateQueries();
              setAtendendo(false);
              navegar(`/oportunidades/${o.id}`);
            }}
          />
        </Modal>
      )}
      <ConfirmDialog
        aberto={arquivando}
        titulo="Arquivar proposta?"
        descricao="Ela sai da votação e não pode ser reaberta."
        confirmar="Arquivar"
        perigo
        carregando={arquivar.isPending}
        aoConfirmar={() => arquivar.mutate()}
        aoFechar={() => setArquivando(false)}
      />
    </div>
  );
}
