import { motion } from 'motion/react';
import { Compass } from 'lucide-react';
import { ButtonLink } from '../components/ui/Button';

export default function NotFoundPage() {
  return (
    <div className="mx-auto flex max-w-xl flex-col items-center px-4 py-28 text-center">
      <motion.div initial={{ rotate: -30, scale: 0.6, opacity: 0 }} animate={{ rotate: 0, scale: 1, opacity: 1 }} transition={{ type: 'spring', damping: 12 }}>
        <Compass className="size-20 text-brand-500" aria-hidden />
      </motion.div>
      <p className="mt-6 font-display text-7xl font-extrabold text-gradient">404</p>
      <h1 className="mt-2 text-2xl font-bold">Esta página não existe</h1>
      <p className="muted mt-2">Mas muitas oportunidades existem. Que tal procurar uma?</p>
      <div className="mt-8 flex gap-3">
        <ButtonLink to="/">Início</ButtonLink>
        <ButtonLink to="/oportunidades" variante="secundario">Ver oportunidades</ButtonLink>
      </div>
    </div>
  );
}
