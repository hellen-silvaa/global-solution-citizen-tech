import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router';
import { motion, useScroll, useTransform } from 'motion/react';
import {
  ArrowRight,
  BadgeCheck,
  Building2,
  GraduationCap,
  HandHeart,
  MapPin,
  Search,
  Sparkles,
  Users,
  Vote,
} from 'lucide-react';
import { indicadoresApi, oportunidadesApi, organizacoesApi, propostasApi } from '../api/services';
import { AREAS } from '../api/types';
import { AREA_INFO, BAIRROS_ZONA_SUL, TIPO_OPORTUNIDADE_ROTULO } from '../lib/labels';
import { formatarDataCurta } from '../lib/format';
import { useAuth, rotaInicial } from '../auth/AuthProvider';
import { AnimatedNumber } from '../components/ui/AnimatedNumber';
import { ButtonLink } from '../components/ui/Button';
import { OportunidadeCard } from '../components/OportunidadeCard';
import { AreaIcon } from '../components/AreaIcon';
import { Skeleton } from '../components/ui/Feedback';
import { useRef } from 'react';

const ODS = [
  { n: 4, nome: 'Educação de qualidade', meta: 'Meta 4.4', texto: 'Mais jovens e adultos com habilidades técnicas e profissionais, com qualificação gratuita perto de casa.', cor: 'bg-ods-4' },
  { n: 8, nome: 'Trabalho decente e crescimento econômico', meta: 'Meta 8.6', texto: 'Menos jovens sem emprego, estudo ou formação, com destaque para as vagas de jovem aprendiz.', cor: 'bg-ods-8' },
  { n: 10, nome: 'Redução das desigualdades', meta: 'Meta 10.2', texto: 'A recomendação prioriza o gratuito, o online e o próprio bairro, e limita inscrições para distribuir as vagas.', cor: 'bg-ods-10' },
  { n: 11, nome: 'Cidades e comunidades sustentáveis', meta: 'Meta 11.3', texto: 'Planejamento participativo: o morador propõe e vota no que o seu bairro precisa.', cor: 'bg-ods-11' },
];

const PASSOS = [
  { icone: Building2, titulo: 'Organizações publicam', texto: 'ONGs, escolas técnicas, empresas e poder público divulgam cursos, oficinas, mentorias e vagas de jovem aprendiz.' },
  { icone: Sparkles, titulo: 'Você recebe recomendações', texto: 'Cruzamos seus interesses e seu bairro e explicamos o porquê de cada sugestão. Sem algoritmo caixa-preta.' },
  { icone: Vote, titulo: 'A comunidade decide', texto: 'Faltou um curso no bairro? Proponha, junte votos dos vizinhos e uma organização pode atender o pedido.' },
];

function Hero() {
  const { sessao } = useAuth();
  const ref = useRef<HTMLElement>(null);
  const { scrollYProgress } = useScroll({ target: ref, offset: ['start start', 'end start'] });
  const y = useTransform(scrollYProgress, [0, 1], [0, 120]);

  const destaques = useQuery({
    queryKey: ['oportunidades', 'destaques'],
    queryFn: () => oportunidadesApi.buscar({ somenteComVagas: true, tamanho: 3 }),
  });

  return (
    <section ref={ref} className="relative -mt-16 overflow-hidden pt-16">
      <div className="grid-bg pointer-events-none absolute inset-0" aria-hidden />
      <div className="pointer-events-none absolute -left-32 top-10 size-[28rem] animate-blob rounded-full bg-brand-500/25 blur-3xl" aria-hidden />
      <div className="pointer-events-none absolute -right-20 top-40 size-[24rem] animate-blob rounded-full bg-sun-400/25 blur-3xl [animation-delay:-6s]" aria-hidden />
      <div className="pointer-events-none absolute bottom-0 left-1/3 size-72 animate-blob rounded-full bg-fuchsia-400/20 blur-3xl [animation-delay:-12s]" aria-hidden />

      <div className="relative mx-auto grid max-w-7xl items-center gap-12 px-4 pb-20 pt-12 sm:px-6 lg:grid-cols-[1.1fr_1fr] lg:pb-28 lg:pt-20">
        <div>
          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            className="inline-flex items-center gap-2 rounded-full border border-brand-200 bg-white/70 px-3 py-1.5 text-sm font-semibold text-brand-700 backdrop-blur dark:border-brand-500/30 dark:bg-brand-500/10 dark:text-brand-200"
          >
            <span className="relative flex size-2">
              <span className="absolute inline-flex size-full animate-ping rounded-full bg-emerald-400 opacity-75" />
              <span className="relative inline-flex size-2 rounded-full bg-emerald-500" />
            </span>
            CitizenTech da Zona Sul de São Paulo
          </motion.div>

          <motion.h1
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.08, duration: 0.6, ease: [0.16, 1, 0.3, 1] }}
            className="mt-6 text-5xl font-extrabold leading-[1.02] sm:text-6xl lg:text-7xl"
          >
            Qualificação gratuita <span className="text-gradient">perto de você.</span>
          </motion.h1>

          <motion.p
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.18, duration: 0.6 }}
            className="muted mt-6 max-w-xl text-lg leading-relaxed sm:text-xl"
          >
            Cursos, oficinas, mentorias e vagas de jovem aprendiz de ONGs, escolas técnicas, empresas e do poder público,
            reunidos em um só lugar, com recomendações feitas para o seu bairro.
          </motion.p>

          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.28, duration: 0.6 }}
            className="mt-9 flex flex-wrap gap-3"
          >
            <ButtonLink to="/oportunidades" tamanho="lg" icone={<Search className="size-5" />}>
              Encontrar oportunidades
            </ButtonLink>
            {sessao ? (
              <ButtonLink to={rotaInicial(sessao.perfil)} tamanho="lg" variante="secundario">
                Ir para minha área <ArrowRight className="size-5" />
              </ButtonLink>
            ) : (
              <ButtonLink to="/cadastro" tamanho="lg" variante="secundario">
                Criar conta grátis <ArrowRight className="size-5" />
              </ButtonLink>
            )}
          </motion.div>

          <motion.ul
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ delay: 0.5 }}
            className="muted mt-8 flex flex-wrap gap-x-6 gap-y-2 text-sm"
          >
            {BAIRROS_ZONA_SUL.map((b) => (
              <li key={b} className="flex items-center gap-1.5">
                <MapPin className="size-4 text-sun-500" aria-hidden /> {b}
              </li>
            ))}
          </motion.ul>
        </div>

        {/* Cartões flutuando com oportunidades reais da API */}
        <motion.div style={{ y }} className="relative hidden h-[30rem] lg:block" aria-hidden>
          {destaques.isLoading
            ? [0, 1, 2].map((i) => <Skeleton key={i} className="absolute h-40 w-80" />)
            : destaques.data?.itens.slice(0, 3).map((o, i) => {
                const pos = ['left-0 top-4 rotate-[-4deg]', 'right-0 top-36 rotate-[3deg]', 'left-10 bottom-0 rotate-[-1deg]'][i];
                return (
                  <motion.div
                    key={o.id}
                    initial={{ opacity: 0, y: 40, scale: 0.9 }}
                    animate={{ opacity: 1, y: 0, scale: 1 }}
                    transition={{ delay: 0.3 + i * 0.15, type: 'spring', damping: 18 }}
                    className={`absolute w-80 ${pos}`}
                  >
                    <div className="card animate-float p-5 shadow-2xl shadow-brand-900/10 dark:shadow-black/40" style={{ animationDelay: `${i * -2.3}s` }}>
                      <div className="flex items-center gap-3">
                        <AreaIcon area={o.area} />
                        <div className="min-w-0">
                          <p className="text-xs font-bold uppercase tracking-wider text-brand-600 dark:text-brand-300">{TIPO_OPORTUNIDADE_ROTULO[o.tipo]}</p>
                          <p className="truncate font-bold">{o.titulo}</p>
                        </div>
                      </div>
                      <div className="muted mt-4 flex items-center justify-between text-xs">
                        <span className="flex items-center gap-1"><MapPin className="size-3.5" />{o.modalidade === 'Online' ? 'Online' : o.bairro}</span>
                        <span>{formatarDataCurta(o.dataInicio)}</span>
                        <span className="rounded-full bg-emerald-100 px-2 py-0.5 font-bold text-emerald-700 dark:bg-emerald-500/15 dark:text-emerald-300">
                          {o.vagasDisponiveis} vagas
                        </span>
                      </div>
                    </div>
                  </motion.div>
                );
              })}
          <motion.div
            initial={{ opacity: 0, scale: 0.6 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ delay: 0.9, type: 'spring' }}
            className="absolute right-6 top-0 flex items-center gap-2 rounded-2xl bg-gradient-to-r from-sun-500 to-rose-500 px-4 py-2.5 text-sm font-bold text-white shadow-xl"
          >
            <Sparkles className="size-4" /> Feito para o seu bairro
          </motion.div>
          <motion.div
            initial={{ opacity: 0, scale: 0.6 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ delay: 1.05, type: 'spring' }}
            className="card absolute bottom-24 right-2 flex items-center gap-2 px-4 py-2.5 text-sm font-bold shadow-xl"
          >
            <BadgeCheck className="size-5 text-emerald-500" /> 100% gratuito para o cidadão
          </motion.div>
        </motion.div>
      </div>
    </section>
  );
}

function Indicadores() {
  const impacto = useQuery({ queryKey: ['impacto'], queryFn: indicadoresApi.impacto });
  const oportunidades = useQuery({ queryKey: ['oportunidades', 'contagem'], queryFn: () => oportunidadesApi.buscar({ somenteComVagas: true, tamanho: 1 }) });
  const organizacoes = useQuery({ queryKey: ['organizacoes', 'contagem'], queryFn: () => organizacoesApi.listar({ tamanho: 1 }) });

  const vagas = impacto.data?.porBairro.reduce((s, b) => s + b.vagasOfertadas, 0) ?? 0;
  const itens = [
    { rotulo: 'oportunidades abertas', valor: oportunidades.data?.totalItens, icone: GraduationCap },
    { rotulo: 'vagas gratuitas ofertadas', valor: vagas, icone: Users, pronto: impacto.isSuccess },
    { rotulo: 'organizações parceiras', valor: organizacoes.data?.totalItens, icone: Building2 },
    { rotulo: 'taxa de conclusão', valor: impacto.data?.taxaConclusaoPercentual, icone: BadgeCheck, sufixo: '%', casas: 0 },
    { rotulo: 'propostas da comunidade atendidas', valor: impacto.data?.propostasAtendidas, icone: HandHeart },
  ];

  return (
    <section aria-labelledby="titulo-indicadores" className="relative mx-auto max-w-7xl px-4 sm:px-6">
      <h2 id="titulo-indicadores" className="sr-only">Indicadores ao vivo</h2>
      <div className="card relative overflow-hidden p-2 shadow-xl shadow-brand-900/5">
        <div className="grid grid-cols-2 gap-2 md:grid-cols-5">
          {itens.map(({ rotulo, valor, icone: Icone, sufixo, casas }, i) => (
            <motion.div
              key={rotulo}
              initial={{ opacity: 0, y: 12 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ delay: i * 0.07 }}
              className={`rounded-2xl p-5 ${i === 4 ? 'col-span-2 md:col-span-1' : ''} ${i % 2 === 0 ? 'bg-brand-50/70 dark:bg-brand-500/5' : ''}`}
            >
              <Icone className="size-5 text-brand-500" aria-hidden />
              <div className="mt-3 font-display text-4xl font-extrabold">
                {valor === undefined ? <Skeleton className="h-10 w-16" /> : <AnimatedNumber valor={valor} sufixo={sufixo} casas={casas} />}
              </div>
              <p className="muted mt-1 text-sm leading-snug">{rotulo}</p>
            </motion.div>
          ))}
        </div>
        <p className="muted flex items-center gap-1.5 px-5 pb-3 pt-1 text-xs">
          <span className="size-1.5 rounded-full bg-emerald-500" aria-hidden /> Dados ao vivo da API ·{' '}
          <Link to="/impacto" className="font-semibold text-brand-600 hover:underline dark:text-brand-300">
            ver o painel de impacto completo
          </Link>
        </p>
      </div>
    </section>
  );
}

function ComoFunciona() {
  return (
    <section className="mx-auto max-w-7xl px-4 pt-28 sm:px-6" aria-labelledby="titulo-como">
      <div className="mx-auto max-w-2xl text-center">
        <p className="font-semibold text-brand-600 dark:text-brand-300">Como funciona</p>
        <h2 id="titulo-como" className="mt-2 text-4xl font-extrabold sm:text-5xl">
          Três caminhos, <span className="text-gradient">uma rede.</span>
        </h2>
        <p className="muted mt-4 text-lg">As oportunidades existem, mas estão espalhadas. O Oportuniza junta tudo e faz chegar a quem precisa.</p>
      </div>
      <div className="mt-14 grid gap-5 md:grid-cols-3">
        {PASSOS.map(({ icone: Icone, titulo, texto }, i) => (
          <motion.div
            key={titulo}
            initial={{ opacity: 0, y: 24 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ delay: i * 0.12, duration: 0.5 }}
            className="card group relative overflow-hidden p-7"
          >
            <span className="absolute -right-3 -top-6 font-display text-[7rem] font-extrabold leading-none text-brand-500/[0.07]" aria-hidden>
              {i + 1}
            </span>
            <div className="grid size-14 place-items-center rounded-2xl bg-gradient-to-br from-brand-500 to-violet-500 text-white shadow-lg shadow-brand-500/30 transition-transform duration-300 group-hover:-rotate-6 group-hover:scale-110">
              <Icone className="size-7" aria-hidden />
            </div>
            <h3 className="mt-6 text-xl font-bold">{titulo}</h3>
            <p className="muted mt-2 leading-relaxed">{texto}</p>
          </motion.div>
        ))}
      </div>
    </section>
  );
}

function Areas() {
  const lista = [...AREAS, ...AREAS];
  return (
    <section className="pt-24" aria-label="Áreas de conhecimento">
      <div className="relative overflow-hidden py-2 [mask-image:linear-gradient(to_right,transparent,black_10%,black_90%,transparent)]">
        <div className="flex w-max animate-marquee gap-3 hover:[animation-play-state:paused]">
          {lista.map((a, i) => (
            <Link
              key={`${a}-${i}`}
              to={`/oportunidades?area=${a}`}
              tabIndex={i >= AREAS.length ? -1 : undefined}
              aria-hidden={i >= AREAS.length || undefined}
              className="card flex items-center gap-3 rounded-full py-2 pl-2 pr-5 font-semibold transition hover:border-brand-300 hover:shadow-md"
            >
              <AreaIcon area={a} className="size-9 rounded-full" />
              {AREA_INFO[a].rotulo}
            </Link>
          ))}
        </div>
      </div>
    </section>
  );
}

function Destaques() {
  const q = useQuery({
    queryKey: ['oportunidades', 'home'],
    queryFn: () => oportunidadesApi.buscar({ somenteComVagas: true, tamanho: 6 }),
  });
  if (q.isError || (q.data && q.data.itens.length === 0)) return null;
  return (
    <section className="mx-auto max-w-7xl px-4 pt-24 sm:px-6" aria-labelledby="titulo-destaques">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="font-semibold text-brand-600 dark:text-brand-300">Inscrições abertas</p>
          <h2 id="titulo-destaques" className="mt-2 text-4xl font-extrabold">Começam em breve</h2>
        </div>
        <ButtonLink to="/oportunidades" variante="secundario">
          Ver todas <ArrowRight className="size-4" />
        </ButtonLink>
      </div>
      <div className="mt-10 grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
        {q.isLoading
          ? Array.from({ length: 3 }, (_, i) => <Skeleton key={i} className="h-72" />)
          : q.data?.itens.map((o, i) => <OportunidadeCard key={o.id} oportunidade={o} indice={i} />)}
      </div>
    </section>
  );
}

function PropostasEmAlta() {
  const q = useQuery({
    queryKey: ['propostas', 'home'],
    queryFn: () => propostasApi.listar({ status: 'Aberta', ordenarPorVotos: true, tamanho: 3 }),
  });
  return (
    <section className="mx-auto max-w-7xl px-4 pt-28 sm:px-6" aria-labelledby="titulo-propostas">
      <div className="relative overflow-hidden rounded-[2rem] bg-gradient-to-br from-brand-700 via-brand-600 to-violet-600 p-8 text-white shadow-2xl shadow-brand-700/30 sm:p-12">
        <div className="pointer-events-none absolute -right-20 -top-20 size-80 rounded-full bg-sun-400/30 blur-3xl" aria-hidden />
        <div className="pointer-events-none absolute -bottom-24 left-10 size-72 rounded-full bg-fuchsia-400/30 blur-3xl" aria-hidden />
        <div className="relative grid gap-10 lg:grid-cols-[1fr_1.2fr] lg:items-center">
          <div>
            <p className="font-semibold text-brand-100">Participação cidadã</p>
            <h2 id="titulo-propostas" className="mt-2 text-4xl font-extrabold sm:text-5xl">O bairro pede, a rede responde.</h2>
            <p className="mt-4 text-lg leading-relaxed text-brand-50/90">
              Qualquer morador pode propor uma qualificação que falta. As mais votadas viram oportunidades de verdade quando uma organização decide atender.
            </p>
            <div className="mt-8 flex flex-wrap gap-3">
              <Link to="/propostas" className="inline-flex h-12 items-center gap-2 rounded-xl bg-white px-6 font-semibold text-brand-700 shadow-lg transition hover:scale-[1.02]">
                <Vote className="size-5" /> Ver e votar
              </Link>
            </div>
          </div>
          <ol className="space-y-3">
            {q.isLoading &&
              [0, 1, 2].map((i) => <li key={i} className="h-20 animate-pulse rounded-2xl bg-white/10" />)}
            {q.data?.itens.length === 0 && <li className="rounded-2xl bg-white/10 p-5">Nenhuma proposta em votação agora. Seja a primeira pessoa a propor!</li>}
            {q.data?.itens.map((p, i) => (
              <motion.li
                key={p.id}
                initial={{ opacity: 0, x: 30 }}
                whileInView={{ opacity: 1, x: 0 }}
                viewport={{ once: true }}
                transition={{ delay: i * 0.1 }}
              >
                <Link to={`/propostas/${p.id}`} className="flex items-center gap-4 rounded-2xl bg-white/10 p-4 backdrop-blur transition hover:bg-white/20">
                  <span className="grid size-14 shrink-0 place-items-center rounded-xl bg-white text-brand-700">
                    <span className="text-center leading-none">
                      <span className="block font-display text-xl font-extrabold">{p.totalVotos}</span>
                      <span className="text-[10px] font-bold uppercase">votos</span>
                    </span>
                  </span>
                  <span className="min-w-0">
                    <span className="block truncate font-bold">{p.titulo}</span>
                    <span className="block text-sm text-brand-100">
                      {AREA_INFO[p.area].rotulo} · {p.bairro}
                    </span>
                  </span>
                  <ArrowRight className="ml-auto size-5 shrink-0 opacity-70" aria-hidden />
                </Link>
              </motion.li>
            ))}
          </ol>
        </div>
      </div>
    </section>
  );
}

function Ods() {
  return (
    <section className="mx-auto max-w-7xl px-4 pt-28 sm:px-6" aria-labelledby="titulo-ods">
      <div className="max-w-2xl">
        <p className="font-semibold text-brand-600 dark:text-brand-300">Agenda 2030 da ONU</p>
        <h2 id="titulo-ods" className="mt-2 text-4xl font-extrabold">Conectado a 4 Objetivos de Desenvolvimento Sustentável</h2>
      </div>
      <div className="mt-10 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
        {ODS.map((o, i) => (
          <motion.article
            key={o.n}
            initial={{ opacity: 0, y: 24 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ delay: i * 0.08 }}
            whileHover={{ y: -6 }}
            className="card overflow-hidden"
          >
            <div className={`${o.cor} flex items-end gap-3 p-5 text-white`}>
              <span className="font-display text-6xl font-extrabold leading-none">{o.n}</span>
              <span className="pb-1 text-sm font-bold uppercase leading-tight">{o.nome}</span>
            </div>
            <div className="p-5">
              <p className="text-xs font-bold uppercase tracking-wider text-brand-600 dark:text-brand-300">{o.meta}</p>
              <p className="muted mt-2 text-sm leading-relaxed">{o.texto}</p>
            </div>
          </motion.article>
        ))}
      </div>
    </section>
  );
}

function ChamadaFinal() {
  const { sessao } = useAuth();
  if (sessao) return null;
  return (
    <section className="mx-auto max-w-7xl px-4 pt-28 sm:px-6">
      <div className="card relative overflow-hidden p-10 text-center sm:p-16">
        <div className="grid-bg pointer-events-none absolute inset-0" aria-hidden />
        <div className="relative mx-auto max-w-2xl">
          <h2 className="text-4xl font-extrabold sm:text-5xl">
            Sua próxima oportunidade <span className="text-gradient">pode estar na sua rua.</span>
          </h2>
          <p className="muted mt-4 text-lg">Crie sua conta em menos de 2 minutos. É de graça e sempre vai ser.</p>
          <div className="mt-8 flex flex-wrap justify-center gap-3">
            <ButtonLink to="/cadastro" tamanho="lg" variante="sol">
              Sou cidadão: quero me qualificar
            </ButtonLink>
            <ButtonLink to="/cadastro?perfil=organizacao" tamanho="lg" variante="secundario">
              Sou organização: quero publicar
            </ButtonLink>
          </div>
        </div>
      </div>
    </section>
  );
}

export function HomePage() {
  return (
    <>
      <Hero />
      <Indicadores />
      <ComoFunciona />
      <Areas />
      <Destaques />
      <PropostasEmAlta />
      <Ods />
      <ChamadaFinal />
    </>
  );
}
