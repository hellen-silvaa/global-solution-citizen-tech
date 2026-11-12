import { useQuery } from '@tanstack/react-query';
import { motion } from 'motion/react';
import { BadgeCheck, GraduationCap, HandHeart, RefreshCw, Users } from 'lucide-react';
import { indicadoresApi } from '../api/services';
import { PageHeader } from '../components/ui/PageHeader';
import { AnimatedNumber } from '../components/ui/AnimatedNumber';
import { ErrorState, Skeleton } from '../components/ui/Feedback';
import { GraficoDemandaReprimida, GraficoInscricoesPorBairro, GraficoVagasPorBairro } from '../components/ImpactCharts';
import { Button } from '../components/ui/Button';

/** Anel de progresso para a taxa de conclusão (um único número: não precisa de gráfico completo). */
function AnelConclusao({ pct }: { pct: number }) {
  const r = 52;
  const c = 2 * Math.PI * r;
  return (
    <svg viewBox="0 0 120 120" className="size-28 -rotate-90" aria-hidden>
      <circle cx="60" cy="60" r={r} fill="none" stroke="var(--surface-2)" strokeWidth="12" />
      <motion.circle
        cx="60"
        cy="60"
        r={r}
        fill="none"
        stroke="url(#anel)"
        strokeWidth="12"
        strokeLinecap="round"
        strokeDasharray={c}
        initial={{ strokeDashoffset: c }}
        whileInView={{ strokeDashoffset: c - (c * Math.min(pct, 100)) / 100 }}
        viewport={{ once: true }}
        transition={{ duration: 1.4, ease: [0.16, 1, 0.3, 1] }}
      />
      <defs>
        <linearGradient id="anel" x1="0" y1="0" x2="1" y2="1">
          <stop offset="0" stopColor="#6366f1" />
          <stop offset="1" stopColor="#f97316" />
        </linearGradient>
      </defs>
    </svg>
  );
}

export default function ImpactoPage() {
  const q = useQuery({ queryKey: ['impacto'], queryFn: indicadoresApi.impacto, refetchInterval: 60_000 });

  return (
    <>
      <PageHeader
        selo="Transparência"
        titulo="Painel de impacto"
        descricao="O indicador público do Oportuniza: quantas pessoas concluíram uma qualificação, em que bairro, e o que a comunidade ainda está pedindo."
        acoes={
          <Button variante="secundario" onClick={() => q.refetch()} carregando={q.isFetching} icone={<RefreshCw className="size-4" />}>
            Atualizar
          </Button>
        }
      />
      <div className="mx-auto max-w-7xl space-y-6 px-4 py-10 sm:px-6">
        {q.isLoading ? (
          <div className="grid gap-4 md:grid-cols-4">{[0, 1, 2, 3].map((i) => <Skeleton key={i} className="h-36" />)}</div>
        ) : q.isError || !q.data ? (
          <ErrorState erro={q.error} onRetry={() => q.refetch()} />
        ) : (
          <>
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
              <div className="card flex items-center gap-5 p-6 sm:col-span-2 lg:col-span-1 lg:row-span-1">
                <div className="relative">
                  <AnelConclusao pct={q.data.taxaConclusaoPercentual} />
                  <span className="absolute inset-0 grid place-items-center font-display text-2xl font-extrabold">
                    <AnimatedNumber valor={q.data.taxaConclusaoPercentual} sufixo="%" />
                  </span>
                </div>
                <div>
                  <p className="font-bold">Taxa de conclusão</p>
                  <p className="muted text-sm">das inscrições viraram qualificação concluída</p>
                </div>
              </div>
              {[
                { rotulo: 'Inscrições no total', valor: q.data.totalInscricoes, icone: Users, cor: 'text-brand-500' },
                { rotulo: 'Qualificações concluídas', valor: q.data.inscricoesConcluidas, icone: GraduationCap, cor: 'text-emerald-500' },
                { rotulo: 'Propostas da comunidade atendidas', valor: q.data.propostasAtendidas, icone: HandHeart, cor: 'text-sun-500' },
              ].map(({ rotulo, valor, icone: Icone, cor }, i) => (
                <motion.div
                  key={rotulo}
                  initial={{ opacity: 0, y: 14 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: 0.05 * i }}
                  className="card p-6"
                >
                  <Icone className={`size-6 ${cor}`} aria-hidden />
                  <p className="mt-4 font-display text-5xl font-extrabold">
                    <AnimatedNumber valor={valor} />
                  </p>
                  <p className="muted mt-1 text-sm">{rotulo}</p>
                </motion.div>
              ))}
            </div>

            <div className="grid gap-6 lg:grid-cols-[1.4fr_1fr]">
              <GraficoInscricoesPorBairro dados={q.data.porBairro} />
              <GraficoVagasPorBairro dados={q.data.porBairro} />
            </div>
            <GraficoDemandaReprimida dados={q.data.demandaReprimida} />

            <div className="card flex items-start gap-4 p-6">
              <BadgeCheck className="mt-0.5 size-6 shrink-0 text-brand-500" aria-hidden />
              <div className="text-sm leading-relaxed">
                <p className="font-bold">Como calculamos</p>
                <p className="muted mt-1">
                  Taxa de conclusão = inscrições concluídas ÷ total de inscrições. Demanda reprimida soma os votos em propostas abertas por área,
                  comparando com as oportunidades abertas na mesma área. Os números vêm direto da API (<code>GET /indicadores/impacto</code>) e
                  se atualizam a cada minuto.
                </p>
              </div>
            </div>
          </>
        )}
      </div>
    </>
  );
}
