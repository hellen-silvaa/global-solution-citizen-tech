import { Link } from 'react-router';
import { motion } from 'motion/react';
import { CalendarDays, Clock, MapPin, Sparkles, Wifi } from 'lucide-react';
import type { Oportunidade } from '../api/types';
import { AREA_INFO, MODALIDADE_ROTULO, STATUS_OPORTUNIDADE_INFO, TIPO_OPORTUNIDADE_ROTULO, humanizarTexto } from '../lib/labels';
import { formatarPeriodo, rotuloPrazo } from '../lib/format';
import { AreaIcon } from './AreaIcon';
import { Badge, StatusBadge } from './ui/Badge';
import { cn } from '../lib/cn';

export function VagasBarra({ disponiveis, totais }: { disponiveis: number; totais: number }) {
  const ocupadas = totais - disponiveis;
  const pct = totais > 0 ? Math.round((ocupadas / totais) * 100) : 0;
  const quaseLotado = disponiveis > 0 && disponiveis / totais <= 0.2;
  return (
    <div>
      <div className="flex items-baseline justify-between text-xs">
        <span className={cn('font-semibold', disponiveis === 0 ? 'text-rose-600 dark:text-rose-400' : quaseLotado ? 'text-orange-600 dark:text-orange-400' : '')}>
          {disponiveis === 0 ? 'Vagas esgotadas' : `${disponiveis} ${disponiveis === 1 ? 'vaga restante' : 'vagas restantes'}`}
        </span>
        <span className="muted tabular-nums">
          {ocupadas}/{totais} inscritos
        </span>
      </div>
      <div
        className="mt-1.5 h-2 overflow-hidden rounded-full bg-[var(--surface-2)]"
        role="progressbar"
        aria-valuemin={0}
        aria-valuemax={totais}
        aria-valuenow={ocupadas}
        aria-label="Vagas ocupadas"
      >
        <motion.div
          className={cn('h-full rounded-full', quaseLotado || disponiveis === 0 ? 'bg-gradient-to-r from-sun-400 to-rose-500' : 'bg-gradient-to-r from-brand-500 to-violet-500')}
          initial={{ width: 0 }}
          whileInView={{ width: `${Math.max(pct, 3)}%` }}
          viewport={{ once: true }}
          transition={{ duration: 0.9, ease: [0.16, 1, 0.3, 1] }}
        />
      </div>
    </div>
  );
}

interface Props {
  oportunidade: Oportunidade;
  motivos?: string[];
  pontuacao?: number;
  indice?: number;
}

export function OportunidadeCard({ oportunidade: o, motivos, pontuacao, indice = 0 }: Props) {
  const status = STATUS_OPORTUNIDADE_INFO[o.status];
  return (
    <motion.article
      initial={{ opacity: 0, y: 18 }}
      whileInView={{ opacity: 1, y: 0 }}
      viewport={{ once: true, margin: '-30px' }}
      transition={{ duration: 0.45, delay: Math.min(indice, 8) * 0.05 }}
      whileHover={{ y: -4 }}
      className="card group relative flex h-full flex-col p-5 transition-shadow hover:shadow-xl hover:shadow-brand-900/5 dark:hover:shadow-black/30"
    >
      <div className="flex items-start justify-between gap-3">
        <div className="flex items-center gap-3">
          <AreaIcon area={o.area} />
          <div>
            <p className="text-xs font-bold uppercase tracking-wider text-brand-600 dark:text-brand-300">{TIPO_OPORTUNIDADE_ROTULO[o.tipo]}</p>
            <p className="muted text-sm">{AREA_INFO[o.area].rotulo}</p>
          </div>
        </div>
        {o.gratuita && <Badge tom="green">Gratuita</Badge>}
      </div>

      <h3 className="mt-4 text-lg font-bold leading-snug">
        <Link to={`/oportunidades/${o.id}`} className="after:absolute after:inset-0 after:rounded-[1.25rem] focus-visible:outline-none">
          {o.titulo}
        </Link>
      </h3>
      <p className="muted mt-1 text-sm">{o.organizacaoNome}</p>

      <ul className="muted mt-4 grid grid-cols-2 gap-x-3 gap-y-2 text-[13px]">
        <li className="flex items-center gap-1.5">
          {o.modalidade === 'Online' ? <Wifi className="size-4 shrink-0" aria-hidden /> : <MapPin className="size-4 shrink-0" aria-hidden />}
          <span className="truncate">{o.modalidade === 'Online' ? 'Online' : o.bairro}</span>
        </li>
        <li className="flex items-center gap-1.5">
          <Clock className="size-4 shrink-0" aria-hidden />
          {o.cargaHoraria}h · {MODALIDADE_ROTULO[o.modalidade]}
        </li>
        <li className="col-span-2 flex items-center gap-1.5">
          <CalendarDays className="size-4 shrink-0" aria-hidden />
          {formatarPeriodo(o.dataInicio, o.dataFim)}
        </li>
      </ul>

      {motivos && motivos.length > 0 && (
        <div className="mt-4 rounded-2xl bg-gradient-to-br from-brand-50 to-orange-50 p-3 dark:from-brand-500/10 dark:to-orange-500/5">
          <p className="flex items-center gap-1.5 text-xs font-bold text-brand-700 dark:text-brand-200">
            <Sparkles className="size-3.5" aria-hidden /> Por que recomendamos
            {pontuacao !== undefined && <span className="ml-auto rounded-full bg-white/70 px-2 py-0.5 tabular-nums dark:bg-white/10">{pontuacao} pts</span>}
          </p>
          <ul className="mt-1.5 space-y-1 text-[13px]">
            {motivos.map((m) => (
              <li key={m} className="flex gap-1.5">
                <span className="text-brand-500" aria-hidden>✓</span>
                {humanizarTexto(m)}
              </li>
            ))}
          </ul>
        </div>
      )}

      <div className="mt-auto space-y-3 pt-5">
        {o.status === 'Aberta' ? (
          <VagasBarra disponiveis={o.vagasDisponiveis} totais={o.vagasTotais} />
        ) : (
          <StatusBadge tom={status.tom}>{status.rotulo}</StatusBadge>
        )}
        {o.status === 'Aberta' && <p className="muted text-xs">{rotuloPrazo(o.dataInicio)}</p>}
      </div>
    </motion.article>
  );
}
