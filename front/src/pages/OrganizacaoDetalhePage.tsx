import { Link, useParams } from 'react-router';
import { useQuery } from '@tanstack/react-query';
import { ArrowLeft, Globe, GraduationCap, Mail, MapPin } from 'lucide-react';
import { oportunidadesApi, organizacoesApi } from '../api/services';
import { GRADIENTE_ORGANIZACAO, TIPO_ORGANIZACAO_ROTULO } from '../lib/labels';
import { formatarData, iniciais } from '../lib/format';
import { Badge } from '../components/ui/Badge';
import { CardsSkeleton, ErrorState, Skeleton } from '../components/ui/Feedback';
import { OportunidadeCard } from '../components/OportunidadeCard';
import { EmptyState } from '../components/ui/EmptyState';

/** Só aceita links http(s) para o site (a API também valida). */
function siteSeguro(site?: string | null): string | null {
  if (!site) return null;
  try {
    const url = new URL(site);
    return url.protocol === 'http:' || url.protocol === 'https:' ? url.toString() : null;
  } catch {
    return null;
  }
}

export default function OrganizacaoDetalhePage() {
  const id = Number(useParams().id);
  const q = useQuery({ queryKey: ['organizacao', id], queryFn: () => organizacoesApi.obter(id) });
  const ops = useQuery({ queryKey: ['oportunidades', { organizacaoId: id }], queryFn: () => oportunidadesApi.buscar({ organizacaoId: id, tamanho: 50 }) });

  if (q.isLoading) return <div className="mx-auto max-w-5xl px-4 py-16"><Skeleton className="h-60" /></div>;
  if (q.isError || !q.data) return <div className="mx-auto max-w-xl px-4 py-24"><ErrorState erro={q.error} /></div>;
  const o = q.data;
  const site = siteSeguro(o.site);

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6">
      <Link to="/organizacoes" className="muted mb-6 inline-flex items-center gap-1.5 text-sm font-semibold hover:text-brand-600">
        <ArrowLeft className="size-4" /> Organizações
      </Link>
      <section className="card relative overflow-hidden p-6 sm:p-10">
        <div className={`pointer-events-none absolute -right-20 -top-20 size-72 rounded-full bg-gradient-to-br ${GRADIENTE_ORGANIZACAO[o.tipo]} opacity-20 blur-3xl`} aria-hidden />
        <div className="relative flex flex-col gap-6 sm:flex-row sm:items-center">
          <span className={`grid size-24 shrink-0 place-items-center rounded-3xl bg-gradient-to-br ${GRADIENTE_ORGANIZACAO[o.tipo]} font-display text-3xl font-extrabold text-white shadow-xl`}>
            {iniciais(o.nome)}
          </span>
          <div>
            <Badge tom="brand">{TIPO_ORGANIZACAO_ROTULO[o.tipo]}</Badge>
            <h1 className="mt-2 text-3xl font-extrabold sm:text-4xl">{o.nome}</h1>
            <p className="muted mt-1 text-sm">Na rede desde {formatarData(o.criadoEm)}</p>
          </div>
        </div>
        <p className="relative mt-6 max-w-3xl text-lg leading-relaxed">{o.descricao}</p>
        <ul className="muted relative mt-6 flex flex-wrap gap-x-6 gap-y-2 text-sm">
          <li className="flex items-center gap-1.5"><MapPin className="size-4" /> {o.endereco.logradouro}{o.endereco.numero ? `, ${o.endereco.numero}` : ''} · {o.endereco.bairro}, {o.endereco.cidade}/{o.endereco.uf}</li>
          <li className="flex items-center gap-1.5"><Mail className="size-4" /> <a className="hover:underline" href={`mailto:${o.email}`}>{o.email}</a></li>
          {site && (
            <li className="flex items-center gap-1.5"><Globe className="size-4" /> <a className="font-semibold text-brand-600 hover:underline dark:text-brand-300" href={site} target="_blank" rel="noopener noreferrer nofollow">{new URL(site).host}</a></li>
          )}
        </ul>
      </section>

      <h2 className="mt-14 text-2xl font-extrabold">Oportunidades publicadas</h2>
      <div className="mt-6">
        {ops.isLoading ? (
          <CardsSkeleton quantidade={3} />
        ) : ops.data?.itens.length === 0 ? (
          <EmptyState icone={GraduationCap} titulo="Nenhuma oportunidade publicada ainda" />
        ) : (
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {ops.data?.itens.map((op, i) => <OportunidadeCard key={op.id} oportunidade={op} indice={i} />)}
          </div>
        )}
      </div>
    </div>
  );
}
