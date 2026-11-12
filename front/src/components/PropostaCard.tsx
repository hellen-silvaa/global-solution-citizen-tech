import { Link } from 'react-router';
import { motion } from 'motion/react';
import { ArrowUpRight, ChevronUp, MapPin } from 'lucide-react';
import type { Proposta } from '../api/types';
import { AREA_INFO, STATUS_PROPOSTA_INFO } from '../lib/labels';
import { formatarData } from '../lib/format';
import { AreaIcon } from './AreaIcon';
import { StatusBadge } from './ui/Badge';
import { cn } from '../lib/cn';

interface Props {
  proposta: Proposta;
  posicao?: number;
  podeVotar?: boolean;
  jaVotou?: boolean;
  votando?: boolean;
  aoVotar?: () => void;
  indice?: number;
}

export function PropostaCard({ proposta: p, posicao, podeVotar, jaVotou, votando, aoVotar, indice = 0 }: Props) {
  const status = STATUS_PROPOSTA_INFO[p.status];
  const aberta = p.status === 'Aberta';
  return (
    <motion.article
      initial={{ opacity: 0, y: 14 }}
      whileInView={{ opacity: 1, y: 0 }}
      viewport={{ once: true }}
      transition={{ duration: 0.4, delay: Math.min(indice, 8) * 0.05 }}
      className="card flex gap-4 p-5"
    >
      <div className="flex flex-col items-center gap-1">
        <motion.button
          type="button"
          whileTap={{ scale: 0.85 }}
          onClick={aoVotar}
          disabled={!aberta || !podeVotar || jaVotou || votando}
          aria-label={jaVotou ? `Você já votou em ${p.titulo}` : `Votar em ${p.titulo}`}
          aria-pressed={jaVotou}
          title={!aberta ? 'Só propostas em votação recebem voto' : !podeVotar ? 'Entre como cidadão para votar' : jaVotou ? 'Você já votou' : 'Votar'}
          className={cn(
            'flex w-16 flex-col items-center rounded-2xl border-2 py-2 transition-colors',
            jaVotou
              ? 'border-brand-500 bg-brand-500 text-white'
              : aberta && podeVotar
                ? 'border-brand-200 text-brand-700 hover:border-brand-500 hover:bg-brand-50 dark:border-brand-500/40 dark:text-brand-200 dark:hover:bg-brand-500/10'
                : 'cursor-not-allowed border-[var(--border)] text-[var(--text-muted)]',
          )}
        >
          <ChevronUp className={cn('size-5', votando && 'animate-bounce')} aria-hidden />
          <motion.span key={p.totalVotos} initial={{ scale: 1.6 }} animate={{ scale: 1 }} className="font-display text-xl font-extrabold tabular-nums">
            {p.totalVotos}
          </motion.span>
          <span className="text-[10px] font-semibold uppercase tracking-wider">{p.totalVotos === 1 ? 'voto' : 'votos'}</span>
        </motion.button>
        {posicao !== undefined && <span className="muted text-xs font-bold">#{posicao}</span>}
      </div>

      <div className="min-w-0 flex-1">
        <div className="flex flex-wrap items-center gap-2">
          <StatusBadge tom={status.tom}>{status.rotulo}</StatusBadge>
          <span className="muted flex items-center gap-1 text-xs">
            <MapPin className="size-3.5" aria-hidden />
            {p.bairro}
          </span>
        </div>
        <h3 className="mt-2 text-lg font-bold leading-snug">
          <Link to={`/propostas/${p.id}`} className="hover:text-brand-600 dark:hover:text-brand-300">
            {p.titulo}
          </Link>
        </h3>
        <p className="muted mt-1 line-clamp-2 text-sm leading-relaxed">{p.descricao}</p>
        <div className="mt-3 flex flex-wrap items-center gap-x-4 gap-y-2 text-xs">
          <span className="flex items-center gap-1.5">
            <AreaIcon area={p.area} className="size-6 rounded-lg" />
            {AREA_INFO[p.area].rotulo}
          </span>
          <span className="muted">
            por {p.autorNome} · {formatarData(p.criadaEm)}
          </span>
          {p.oportunidadeGeradaId && (
            <Link
              to={`/oportunidades/${p.oportunidadeGeradaId}`}
              className="flex items-center gap-1 font-semibold text-emerald-700 hover:underline dark:text-emerald-300"
            >
              Ver oportunidade criada <ArrowUpRight className="size-3.5" aria-hidden />
            </Link>
          )}
        </div>
      </div>
    </motion.article>
  );
}
