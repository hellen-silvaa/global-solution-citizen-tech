import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router';
import type { Perfil } from '../api/types';
import { useAuth } from './AuthProvider';
import { EmptyState } from '../components/ui/EmptyState';
import { ShieldAlert } from 'lucide-react';
import { ButtonLink } from '../components/ui/Button';

/** Protege rotas por perfil, como o [Authorize(Roles = ...)] da API. */
export function RequireAuth({ perfis, children }: { perfis?: Perfil[]; children: ReactNode }) {
  const { sessao } = useAuth();
  const local = useLocation();

  if (!sessao) return <Navigate to="/entrar" replace state={{ de: local.pathname + local.search }} />;

  if (perfis && !perfis.includes(sessao.perfil)) {
    return (
      <div className="mx-auto max-w-xl px-4 py-24">
        <EmptyState
          icone={ShieldAlert}
          titulo="Esta área não é para o seu perfil"
          descricao="Você entrou com um perfil que não tem acesso a esta página."
          acao={<ButtonLink to="/">Voltar ao início</ButtonLink>}
        />
      </div>
    );
  }
  return <>{children}</>;
}
