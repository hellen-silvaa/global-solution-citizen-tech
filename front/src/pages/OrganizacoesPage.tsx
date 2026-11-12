import { Link, useSearchParams } from 'react-router';
import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { motion } from 'motion/react';
import { Building2, Globe, MapPin } from 'lucide-react';
import { organizacoesApi } from '../api/services';
import { TIPOS_ORGANIZACAO, type TipoOrganizacao } from '../api/types';
import { GRADIENTE_ORGANIZACAO as GRADIENTES, TIPO_ORGANIZACAO_ROTULO } from '../lib/labels';
import { iniciais } from '../lib/format';
import { PageHeader } from '../components/ui/PageHeader';
import { Badge } from '../components/ui/Badge';
import { CardsSkeleton, ErrorState } from '../components/ui/Feedback';
import { EmptyState } from '../components/ui/EmptyState';
import { Pagination } from '../components/ui/Pagination';
import { cn } from '../lib/cn';

export default function OrganizacoesPage() {
  const [params, setParams] = useSearchParams();
  const tipo = (params.get('tipo') as TipoOrganizacao) || undefined;
  const pagina = Number(params.get('pagina')) || 1;
  const q = useQuery({
    queryKey: ['organizacoes', tipo, pagina],
    queryFn: () => organizacoesApi.listar({ tipo, pagina, tamanho: 12 }),
    placeholderData: keepPreviousData,
  });

  return (
    <>
      <PageHeader
        selo="Rede de parceiros"
        titulo="Organizações que fazem acontecer"
        descricao="ONGs, escolas técnicas, empresas, coletivos e poder público oferecendo qualificação gratuita na Zona Sul."
      />
      <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6">
        <div className="flex flex-wrap gap-2" role="group" aria-label="Filtrar por tipo">
          {[undefined, ...TIPOS_ORGANIZACAO].map((t) => (
            <button
              key={t ?? 'todas'}
              type="button"
              aria-pressed={tipo === t}
              onClick={() => setParams(t ? { tipo: t } : {})}
              className={cn(
                'rounded-full border px-4 py-2 text-sm font-semibold transition',
                tipo === t ? 'border-brand-600 bg-brand-600 text-white' : 'border-[var(--border)] bg-[var(--surface)] hover:border-brand-300',
              )}
            >
              {t ? TIPO_ORGANIZACAO_ROTULO[t] : 'Todas'}
            </button>
          ))}
        </div>
        <div className="mt-8">
          {q.isLoading ? (
            <CardsSkeleton />
          ) : q.isError ? (
            <ErrorState erro={q.error} onRetry={() => q.refetch()} />
          ) : q.data?.itens.length === 0 ? (
            <EmptyState icone={Building2} titulo="Nenhuma organização deste tipo ainda" />
          ) : (
            <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
              {q.data?.itens.map((o, i) => (
                <motion.div
                  key={o.id}
                  initial={{ opacity: 0, y: 16 }}
                  whileInView={{ opacity: 1, y: 0 }}
                  viewport={{ once: true }}
                  transition={{ delay: i * 0.05 }}
                  whileHover={{ y: -4 }}
                >
                  <Link to={`/organizacoes/${o.id}`} className="card flex h-full flex-col p-6 transition-shadow hover:shadow-xl">
                    <div className="flex items-center gap-4">
                      <span className={`grid size-14 place-items-center rounded-2xl bg-gradient-to-br ${GRADIENTES[o.tipo]} font-display text-lg font-extrabold text-white shadow-lg`}>
                        {iniciais(o.nome)}
                      </span>
                      <div className="min-w-0">
                        <h2 className="truncate text-lg font-bold">{o.nome}</h2>
                        <Badge tom="brand">{TIPO_ORGANIZACAO_ROTULO[o.tipo]}</Badge>
                      </div>
                    </div>
                    <p className="muted mt-4 line-clamp-3 flex-1 text-sm leading-relaxed">{o.descricao}</p>
                    <div className="muted mt-4 flex flex-wrap gap-x-4 gap-y-1 text-xs">
                      <span className="flex items-center gap-1"><MapPin className="size-3.5" /> {o.endereco.bairro}</span>
                      {o.site && <span className="flex items-center gap-1"><Globe className="size-3.5" /> site</span>}
                    </div>
                  </Link>
                </motion.div>
              ))}
            </div>
          )}
        </div>
        {q.data && <Pagination pagina={q.data.pagina} totalPaginas={q.data.totalPaginas} aoMudar={(p) => setParams({ ...(tipo ? { tipo } : {}), pagina: String(p) })} />}
      </div>
    </>
  );
}
