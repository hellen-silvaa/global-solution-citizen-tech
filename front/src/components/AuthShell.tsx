import type { ReactNode } from 'react';
import { motion } from 'motion/react';
import { CheckCircle2 } from 'lucide-react';
import { LogoMarca } from './layout/Logo';

const VANTAGENS = [
  'Cursos, oficinas e mentorias gratuitas perto de casa',
  'Recomendações explicadas pelo seu interesse e bairro',
  'Vote no que o seu bairro precisa',
];

/** Tela dividida usada no login e no cadastro. */
export function AuthShell({ titulo, subtitulo, children }: { titulo: string; subtitulo: ReactNode; children: ReactNode }) {
  return (
    <div className="mx-auto grid max-w-7xl gap-10 px-4 py-10 sm:px-6 lg:grid-cols-[1fr_1.1fr] lg:py-16">
      <motion.aside
        initial={{ opacity: 0, x: -20 }}
        animate={{ opacity: 1, x: 0 }}
        className="relative hidden overflow-hidden rounded-[2rem] bg-gradient-to-br from-brand-700 via-violet-600 to-sun-500 p-10 text-white lg:flex lg:flex-col"
      >
        <div className="grid-bg absolute inset-0 opacity-30 invert" aria-hidden />
        <div className="absolute -bottom-24 -right-24 size-96 animate-blob rounded-full bg-white/15 blur-3xl" aria-hidden />
        <LogoMarca className="relative size-14" />
        <h2 className="relative mt-auto text-4xl font-extrabold leading-tight">Oportunidade de qualificação para todo mundo.</h2>
        <ul className="relative mt-8 space-y-3">
          {VANTAGENS.map((v, i) => (
            <motion.li
              key={v}
              initial={{ opacity: 0, x: -10 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.2 + i * 0.1 }}
              className="flex items-center gap-3 text-lg"
            >
              <CheckCircle2 className="size-6 shrink-0 text-sun-300" aria-hidden /> {v}
            </motion.li>
          ))}
        </ul>
      </motion.aside>
      <motion.section initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} className="mx-auto w-full max-w-xl">
        <h1 className="text-3xl font-extrabold sm:text-4xl">{titulo}</h1>
        <div className="muted mt-2">{subtitulo}</div>
        <div className="mt-8">{children}</div>
      </motion.section>
    </div>
  );
}
