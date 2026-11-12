import { useEffect } from 'react';
import { Outlet, useLocation } from 'react-router';
import { Header } from './Header';
import { Footer } from './Footer';
import { useAuth } from '../../auth/AuthProvider';
import { useToast } from '../ui/Toast';

export function Layout() {
  const { pathname } = useLocation();
  const { sessaoExpirou } = useAuth();
  const toast = useToast();

  useEffect(() => {
    window.scrollTo({ top: 0 });
  }, [pathname]);

  useEffect(() => {
    if (sessaoExpirou) toast.info('Sua sessão terminou', 'Entre de novo para continuar.');
  }, [sessaoExpirou, toast]);

  return (
    <div className="flex min-h-dvh flex-col">
      <a
        href="#conteudo"
        className="sr-only z-[200] rounded-xl bg-brand-600 px-4 py-2 font-semibold text-white focus:not-sr-only focus:fixed focus:left-4 focus:top-4"
      >
        Pular para o conteúdo
      </a>
      <Header />
      <main id="conteudo" className="flex-1" tabIndex={-1}>
        <Outlet />
      </main>
      <Footer />
    </div>
  );
}
