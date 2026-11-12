import type { ReactElement } from 'react';
import { render } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { vi } from 'vitest';
import { AuthProvider } from '../src/auth/AuthProvider';
import { ToastProvider } from '../src/components/ui/Toast';
import type { Sessao } from '../src/api/types';

export function sessaoFake(parcial: Partial<Sessao> = {}): Sessao {
  return {
    token: 'token-teste',
    tipo: 'Bearer',
    expiraEm: new Date(Date.now() + 3_600_000).toISOString(),
    usuarioId: 5,
    nome: 'Ana Souza',
    perfil: 'Cidadao',
    ...parcial,
  };
}

export function logar(parcial: Partial<Sessao> = {}) {
  localStorage.setItem('oportuniza.sessao', JSON.stringify(sessaoFake(parcial)));
}

interface Opcoes {
  rota?: string;
  caminho?: string;
  extras?: { caminho: string; elemento: ReactElement }[];
}

/** Renderiza com Router, React Query, Toasts e sessão, como no App real. */
export function renderizar(ui: ReactElement, { rota = '/', caminho = '*', extras = [] }: Opcoes = {}) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <AuthProvider>
          <MemoryRouter initialEntries={[rota]}>
            <Routes>
              <Route path={caminho} element={ui} />
              {extras.map((e) => (
                <Route key={e.caminho} path={e.caminho} element={e.elemento} />
              ))}
            </Routes>
          </MemoryRouter>
        </AuthProvider>
      </ToastProvider>
    </QueryClientProvider>,
  );
}

type Rota = { metodo?: string; url: RegExp; status?: number; corpo?: unknown };

/** Substitui o fetch por respostas fixas por rota; guarda as chamadas feitas. */
export function mockarApi(rotas: Rota[]) {
  const chamadas: { metodo: string; url: string; corpo?: unknown; headers: Record<string, string> }[] = [];
  const fn = vi.fn(async (entrada: RequestInfo | URL, init?: RequestInit) => {
    const url = String(entrada);
    const metodo = init?.method ?? 'GET';
    chamadas.push({ metodo, url, corpo: init?.body ? JSON.parse(String(init.body)) : undefined, headers: (init?.headers ?? {}) as Record<string, string> });
    const rota = rotas.find((r) => (r.metodo ?? 'GET') === metodo && r.url.test(url));
    if (!rota) return new Response(JSON.stringify({ mensagem: `sem mock para ${metodo} ${url}` }), { status: 404 });
    const status = rota.status ?? 200;
    return new Response(status === 204 || rota.corpo === undefined ? null : JSON.stringify(rota.corpo), { status });
  });
  vi.stubGlobal('fetch', fn);
  return chamadas;
}
