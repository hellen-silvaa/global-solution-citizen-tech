import { useEffect, useState } from 'react';
import { NavLink, useLocation, useNavigate } from 'react-router';
import { AnimatePresence, motion } from 'motion/react';
import { LogOut, Menu, Moon, Sun, X } from 'lucide-react';
import { useAuth, rotaInicial } from '../../auth/AuthProvider';
import { useTheme } from '../../hooks/useTheme';
import { iniciais, primeiroNome } from '../../lib/format';
import { PERFIL_ROTULO } from '../../lib/labels';
import { cn } from '../../lib/cn';
import { Logo } from './Logo';
import { ButtonLink } from '../ui/Button';
import type { Perfil } from '../../api/types';

interface Item {
  para: string;
  rotulo: string;
}

const PUBLICOS: Item[] = [
  { para: '/oportunidades', rotulo: 'Oportunidades' },
  { para: '/propostas', rotulo: 'Propostas' },
  { para: '/organizacoes', rotulo: 'Organizações' },
  { para: '/impacto', rotulo: 'Impacto' },
];

const POR_PERFIL: Record<Perfil, Item> = {
  Cidadao: { para: '/minha-area', rotulo: 'Minha área' },
  Organizacao: { para: '/organizacao', rotulo: 'Painel da organização' },
  Admin: { para: '/admin', rotulo: 'Administração' },
};

function ThemeToggle() {
  const { tema, alternar } = useTheme();
  const escuro = tema === 'dark';
  return (
    <button
      type="button"
      onClick={alternar}
      className="grid size-10 place-items-center rounded-xl border border-[var(--border)] bg-[var(--surface)] transition hover:border-brand-300"
      aria-label={escuro ? 'Ativar modo claro' : 'Ativar modo escuro'}
      title={escuro ? 'Modo claro' : 'Modo escuro'}
    >
      <AnimatePresence mode="wait" initial={false}>
        <motion.span
          key={tema}
          initial={{ rotate: -90, opacity: 0 }}
          animate={{ rotate: 0, opacity: 1 }}
          exit={{ rotate: 90, opacity: 0 }}
          transition={{ duration: 0.2 }}
        >
          {escuro ? <Sun className="size-[18px] text-amber-300" /> : <Moon className="size-[18px] text-brand-700" />}
        </motion.span>
      </AnimatePresence>
    </button>
  );
}

function linkClasse({ isActive }: { isActive: boolean }) {
  return cn(
    'relative rounded-xl px-3 py-2 text-sm font-semibold transition-colors',
    isActive ? 'text-brand-700 dark:text-brand-200' : 'muted hover:text-[var(--text)]',
  );
}

export function Header() {
  const { sessao, sair } = useAuth();
  const navegar = useNavigate();
  const local = useLocation();
  const [menuAberto, setMenuAberto] = useState(false);
  const [rolou, setRolou] = useState(false);

  useEffect(() => setMenuAberto(false), [local.pathname]);
  useEffect(() => {
    const aoRolar = () => setRolou(window.scrollY > 8);
    aoRolar();
    window.addEventListener('scroll', aoRolar, { passive: true });
    return () => window.removeEventListener('scroll', aoRolar);
  }, []);

  const itens = sessao ? [POR_PERFIL[sessao.perfil], ...PUBLICOS] : PUBLICOS;

  function fazerLogout() {
    sair();
    navegar('/');
  }

  return (
    <header
      className={cn(
        'sticky top-0 z-50 transition-all duration-300',
        rolou ? 'border-b border-[var(--border)] bg-[var(--bg)]/80 backdrop-blur-xl' : 'bg-transparent',
      )}
    >
      <div className="mx-auto flex h-16 max-w-7xl items-center justify-between gap-4 px-4 sm:px-6">
        <Logo />

        <nav className="hidden items-center gap-1 lg:flex" aria-label="Principal">
          {itens.map((item) => (
            <NavLink key={item.para} to={item.para} className={linkClasse}>
              {({ isActive }) => (
                <>
                  {item.rotulo}
                  {isActive && (
                    <motion.span
                      layoutId="nav-ativo"
                      className="absolute inset-0 -z-10 rounded-xl bg-brand-100 dark:bg-brand-500/15"
                      transition={{ type: 'spring', damping: 30, stiffness: 380 }}
                    />
                  )}
                </>
              )}
            </NavLink>
          ))}
        </nav>

        <div className="flex items-center gap-2">
          <ThemeToggle />
          {sessao ? (
            <div className="hidden items-center gap-2 lg:flex">
              <NavLink
                to={rotaInicial(sessao.perfil)}
                className="flex items-center gap-2 rounded-xl py-1 pl-1 pr-3 hover:bg-black/5 dark:hover:bg-white/10"
              >
                <span className="grid size-8 place-items-center rounded-lg bg-gradient-to-br from-brand-500 to-sun-500 text-xs font-bold text-white">
                  {iniciais(sessao.nome)}
                </span>
                <span className="text-left leading-tight">
                  <span className="block text-sm font-semibold">{primeiroNome(sessao.nome)}</span>
                  <span className="muted block text-[11px]">{PERFIL_ROTULO[sessao.perfil]}</span>
                </span>
              </NavLink>
              <button
                type="button"
                onClick={fazerLogout}
                className="muted grid size-10 place-items-center rounded-xl hover:bg-black/5 hover:text-rose-600 dark:hover:bg-white/10"
                aria-label="Sair"
                title="Sair"
              >
                <LogOut className="size-[18px]" />
              </button>
            </div>
          ) : (
            <div className="hidden items-center gap-2 sm:flex">
              <ButtonLink to="/entrar" variante="fantasma" tamanho="sm">
                Entrar
              </ButtonLink>
              <ButtonLink to="/cadastro" tamanho="sm">
                Criar conta
              </ButtonLink>
            </div>
          )}
          <button
            type="button"
            className="grid size-10 place-items-center rounded-xl border border-[var(--border)] bg-[var(--surface)] lg:hidden"
            onClick={() => setMenuAberto((v) => !v)}
            aria-expanded={menuAberto}
            aria-controls="menu-movel"
            aria-label={menuAberto ? 'Fechar menu' : 'Abrir menu'}
          >
            {menuAberto ? <X className="size-5" /> : <Menu className="size-5" />}
          </button>
        </div>
      </div>

      <AnimatePresence>
        {menuAberto && (
          <motion.nav
            id="menu-movel"
            aria-label="Menu"
            initial={{ height: 0, opacity: 0 }}
            animate={{ height: 'auto', opacity: 1 }}
            exit={{ height: 0, opacity: 0 }}
            className="overflow-hidden border-b border-[var(--border)] bg-[var(--surface)] lg:hidden"
          >
            <div className="flex flex-col gap-1 px-4 py-4">
              {itens.map((item) => (
                <NavLink key={item.para} to={item.para} className={({ isActive }) => cn(linkClasse({ isActive }), 'py-3', isActive && 'bg-brand-100 dark:bg-brand-500/15')}>
                  {item.rotulo}
                </NavLink>
              ))}
              <div className="mt-3 flex gap-2 border-t border-[var(--border)] pt-4">
                {sessao ? (
                  <button type="button" onClick={fazerLogout} className="flex items-center gap-2 px-3 py-2 text-sm font-semibold text-rose-600">
                    <LogOut className="size-4" /> Sair ({primeiroNome(sessao.nome)})
                  </button>
                ) : (
                  <>
                    <ButtonLink to="/entrar" variante="secundario" className="flex-1">
                      Entrar
                    </ButtonLink>
                    <ButtonLink to="/cadastro" className="flex-1">
                      Criar conta
                    </ButtonLink>
                  </>
                )}
              </div>
            </div>
          </motion.nav>
        )}
      </AnimatePresence>
    </header>
  );
}
