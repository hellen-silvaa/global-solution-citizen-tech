import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { configurarAutenticacao } from '../api/client';
import { authApi } from '../api/services';
import type { Perfil, Sessao } from '../api/types';

const CHAVE = 'oportuniza.sessao';

export function lerSessao(agora: number = Date.now()): Sessao | null {
  try {
    const bruto = localStorage.getItem(CHAVE);
    if (!bruto) return null;
    const sessao = JSON.parse(bruto) as Sessao;
    if (!sessao.token || new Date(sessao.expiraEm).getTime() <= agora) {
      localStorage.removeItem(CHAVE);
      return null;
    }
    return sessao;
  } catch {
    return null;
  }
}

function gravarSessao(sessao: Sessao | null) {
  try {
    if (sessao) localStorage.setItem(CHAVE, JSON.stringify(sessao));
    else localStorage.removeItem(CHAVE);
  } catch {
    /* navegação privada: a sessão fica só em memória */
  }
}

interface AuthContexto {
  sessao: Sessao | null;
  perfil: Perfil | null;
  entrar: (email: string, senha: string) => Promise<Sessao>;
  sair: (motivo?: 'expirou') => void;
  sessaoExpirou: boolean;
}

const Contexto = createContext<AuthContexto | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [sessao, setSessao] = useState<Sessao | null>(() => lerSessao());
  const [sessaoExpirou, setSessaoExpirou] = useState(false);
  const queryClient = useQueryClient();

  const sair = useCallback(
    (motivo?: 'expirou') => {
      gravarSessao(null);
      setSessao(null);
      setSessaoExpirou(motivo === 'expirou');
      queryClient.clear();
    },
    [queryClient],
  );

  // O cliente HTTP lê o token daqui e avisa quando a API responde 401.
  useEffect(() => {
    configurarAutenticacao(
      () => sessao?.token ?? null,
      () => sair('expirou'),
    );
  }, [sessao, sair]);

  // Encerra a sessão sozinho quando o JWT expira.
  useEffect(() => {
    if (!sessao) return;
    const restante = new Date(sessao.expiraEm).getTime() - Date.now();
    const timer = window.setTimeout(() => sair('expirou'), Math.max(restante, 0));
    return () => window.clearTimeout(timer);
  }, [sessao, sair]);

  const entrar = useCallback(async (email: string, senha: string) => {
    const nova = await authApi.login(email.trim(), senha);
    gravarSessao(nova);
    configurarAutenticacao(() => nova.token, () => sair('expirou'));
    setSessao(nova);
    setSessaoExpirou(false);
    return nova;
  }, [sair]);

  const valor = useMemo(
    () => ({ sessao, perfil: sessao?.perfil ?? null, entrar, sair, sessaoExpirou }),
    [sessao, entrar, sair, sessaoExpirou],
  );

  return <Contexto.Provider value={valor}>{children}</Contexto.Provider>;
}

export function useAuth(): AuthContexto {
  const ctx = useContext(Contexto);
  if (!ctx) throw new Error('useAuth precisa estar dentro de <AuthProvider>');
  return ctx;
}

/** Página inicial de cada perfil depois do login. */
export function rotaInicial(perfil: Perfil): string {
  return perfil === 'Cidadao' ? '/minha-area' : perfil === 'Organizacao' ? '/organizacao' : '/admin';
}
