import { useState, type ReactNode } from 'react';
import { Bar, BarChart, CartesianGrid, LabelList, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { BarChart3, Table2 } from 'lucide-react';
import type { IndicadorArea, IndicadorBairro } from '../api/types';
import { AREA_INFO } from '../lib/labels';
import { cn } from '../lib/cn';

// Paleta categórica validada (azul / laranja) para daltonismo nos modos claro e escuro:
// as cores vêm de --series-1 e --series-2 (index.css).
const EIXO = { fontSize: 12, fill: 'var(--text-muted)' };

interface LinhaTooltip {
  name?: string | number;
  value?: number | string;
  color?: string;
}

function CaixaTooltip({ active, payload, label }: { active?: boolean; payload?: LinhaTooltip[]; label?: string | number }) {
  if (!active || !payload?.length) return null;
  return (
    <div className="card min-w-40 px-3.5 py-2.5 text-sm shadow-xl">
      <p className="mb-1.5 font-semibold">{label}</p>
      {payload.map((p) => (
        <p key={String(p.name)} className="flex items-center gap-2">
          <span className="size-2.5 rounded-sm" style={{ background: p.color }} aria-hidden />
          <span className="muted">{p.name}</span>
          <span className="ml-auto font-semibold tabular-nums">{p.value}</span>
        </p>
      ))}
    </div>
  );
}

function Legenda({ itens }: { itens: { cor: string; rotulo: string }[] }) {
  return (
    <ul className="flex flex-wrap gap-4 text-xs">
      {itens.map((i) => (
        <li key={i.rotulo} className="flex items-center gap-1.5">
          <span className="size-3 rounded-sm" style={{ background: i.cor }} aria-hidden />
          {i.rotulo}
        </li>
      ))}
    </ul>
  );
}

interface PainelProps {
  titulo: string;
  descricao: string;
  legenda?: ReactNode;
  grafico: ReactNode;
  tabela: ReactNode;
}

/** Cartão de gráfico com alternância para tabela (alternativa acessível). */
export function PainelGrafico({ titulo, descricao, legenda, grafico, tabela }: PainelProps) {
  const [modo, setModo] = useState<'grafico' | 'tabela'>('grafico');
  return (
    <section className="card p-5 sm:p-6" aria-label={titulo}>
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h3 className="text-lg font-bold">{titulo}</h3>
          <p className="muted mt-0.5 text-sm">{descricao}</p>
        </div>
        <div className="surface-2 flex rounded-xl p-1" role="group" aria-label="Forma de visualização">
          {(['grafico', 'tabela'] as const).map((m) => (
            <button
              key={m}
              type="button"
              onClick={() => setModo(m)}
              aria-pressed={modo === m}
              className={cn(
                'flex items-center gap-1.5 rounded-lg px-2.5 py-1.5 text-xs font-semibold transition',
                modo === m ? 'bg-[var(--surface)] shadow-sm' : 'muted hover:text-[var(--text)]',
              )}
            >
              {m === 'grafico' ? <BarChart3 className="size-3.5" aria-hidden /> : <Table2 className="size-3.5" aria-hidden />}
              {m === 'grafico' ? 'Gráfico' : 'Tabela'}
            </button>
          ))}
        </div>
      </div>
      {legenda && modo === 'grafico' && <div className="mt-4">{legenda}</div>}
      <div className="mt-4">{modo === 'grafico' ? grafico : tabela}</div>
    </section>
  );
}

function Tabela({ cabecalho, linhas }: { cabecalho: string[]; linhas: (string | number)[][] }) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full text-left text-sm">
        <thead>
          <tr className="border-b border-[var(--border)]">
            {cabecalho.map((c, i) => (
              <th key={c} scope="col" className={cn('muted py-2 pr-4 font-semibold', i > 0 && 'text-right')}>
                {c}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {linhas.map((l) => (
            <tr key={String(l[0])} className="border-b border-[var(--border)] last:border-0">
              {l.map((c, i) =>
                i === 0 ? (
                  <th key={i} scope="row" className="py-2.5 pr-4 font-medium">
                    {c}
                  </th>
                ) : (
                  <td key={i} className="py-2.5 pr-4 text-right tabular-nums">
                    {c}
                  </td>
                ),
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function GraficoInscricoesPorBairro({ dados }: { dados: IndicadorBairro[] }) {
  return (
    <PainelGrafico
      titulo="Inscrições por bairro"
      descricao="Quem está estudando agora e quem já concluiu, em cada bairro."
      legenda={
        <Legenda
          itens={[
            { cor: 'var(--series-1)', rotulo: 'Inscrições ativas' },
            { cor: 'var(--series-2)', rotulo: 'Concluídas' },
          ]}
        />
      }
      grafico={
        <div className="h-72">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={dados} barGap={2} barCategoryGap="28%" margin={{ top: 20, right: 4, left: -20, bottom: 0 }}>
              <CartesianGrid vertical={false} stroke="var(--chart-grid)" />
              <XAxis dataKey="bairro" interval={0} tick={EIXO} axisLine={false} tickLine={false} />
              <YAxis allowDecimals={false} tick={EIXO} axisLine={false} tickLine={false} />
              <Tooltip content={<CaixaTooltip />} cursor={{ fill: 'var(--surface-2)' }} />
              <Bar dataKey="inscricoesAtivas" name="Inscrições ativas" fill="var(--series-1)" radius={[4, 4, 0, 0]} maxBarSize={44}>
                <LabelList dataKey="inscricoesAtivas" position="top" style={{ fontSize: 12, fill: 'var(--text-muted)' }} />
              </Bar>
              <Bar dataKey="inscricoesConcluidas" name="Concluídas" fill="var(--series-2)" radius={[4, 4, 0, 0]} maxBarSize={44}>
                <LabelList dataKey="inscricoesConcluidas" position="top" style={{ fontSize: 12, fill: 'var(--text-muted)' }} />
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>
      }
      tabela={
        <Tabela
          cabecalho={['Bairro', 'Ativas', 'Concluídas', 'Oportunidades abertas', 'Propostas atendidas']}
          linhas={dados.map((d) => [d.bairro, d.inscricoesAtivas, d.inscricoesConcluidas, d.oportunidadesAbertas, d.propostasAtendidas])}
        />
      }
    />
  );
}

export function GraficoVagasPorBairro({ dados }: { dados: IndicadorBairro[] }) {
  const ordenados = [...dados].sort((a, b) => b.vagasOfertadas - a.vagasOfertadas);
  return (
    <PainelGrafico
      titulo="Vagas ofertadas por bairro"
      descricao="Total de vagas em oportunidades abertas."
      grafico={
        <div style={{ height: Math.max(ordenados.length * 52, 160) }}>
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={ordenados} layout="vertical" margin={{ top: 0, right: 40, left: 8, bottom: 0 }} barCategoryGap="30%">
              <CartesianGrid horizontal={false} stroke="var(--chart-grid)" />
              <XAxis type="number" allowDecimals={false} tick={EIXO} axisLine={false} tickLine={false} />
              <YAxis type="category" dataKey="bairro" tick={EIXO} axisLine={false} tickLine={false} width={110} />
              <Tooltip content={<CaixaTooltip />} cursor={{ fill: 'var(--surface-2)' }} />
              <Bar dataKey="vagasOfertadas" name="Vagas" fill="var(--series-1)" radius={[0, 4, 4, 0]} maxBarSize={28}>
                <LabelList dataKey="vagasOfertadas" position="right" style={{ fontSize: 12, fill: 'var(--text)' }} />
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>
      }
      tabela={<Tabela cabecalho={['Bairro', 'Vagas ofertadas']} linhas={ordenados.map((d) => [d.bairro, d.vagasOfertadas])} />}
    />
  );
}

export function GraficoDemandaReprimida({ dados }: { dados: IndicadorArea[] }) {
  const linhas = [...dados]
    .sort((a, b) => b.votosEmPropostasAbertas - a.votosEmPropostasAbertas)
    .map((d) => ({ ...d, rotulo: AREA_INFO[d.area].rotulo }));
  return (
    <PainelGrafico
      titulo="Demanda reprimida"
      descricao="Áreas que a comunidade pediu (votos em propostas abertas) e ainda não têm oferta suficiente."
      grafico={
        linhas.length === 0 ? (
          <p className="muted py-10 text-center text-sm">Nenhuma demanda reprimida: toda área pedida já tem oferta. 🎉</p>
        ) : (
          <div style={{ height: Math.max(linhas.length * 52, 160) }}>
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={linhas} layout="vertical" margin={{ top: 0, right: 40, left: 8, bottom: 0 }} barCategoryGap="30%">
                <CartesianGrid horizontal={false} stroke="var(--chart-grid)" />
                <XAxis type="number" allowDecimals={false} tick={EIXO} axisLine={false} tickLine={false} />
                <YAxis type="category" dataKey="rotulo" tick={EIXO} axisLine={false} tickLine={false} width={120} />
                <Tooltip content={<CaixaTooltip />} cursor={{ fill: 'var(--surface-2)' }} />
                <Bar dataKey="votosEmPropostasAbertas" name="Votos" fill="var(--series-2)" radius={[0, 4, 4, 0]} maxBarSize={28}>
                  <LabelList dataKey="votosEmPropostasAbertas" position="right" style={{ fontSize: 12, fill: 'var(--text)' }} />
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </div>
        )
      }
      tabela={
        <Tabela
          cabecalho={['Área', 'Votos', 'Propostas abertas', 'Oportunidades abertas']}
          linhas={linhas.map((d) => [d.rotulo, d.votosEmPropostasAbertas, d.propostasAbertas, d.oportunidadesAbertas])}
        />
      }
    />
  );
}
