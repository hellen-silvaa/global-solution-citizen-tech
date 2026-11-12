import { lazy, Suspense, type ReactNode } from 'react';
import { BrowserRouter, Route, Routes } from 'react-router';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MotionConfig } from 'motion/react';
import { AuthProvider } from './auth/AuthProvider';
import { RequireAuth } from './auth/RequireAuth';
import { ToastProvider } from './components/ui/Toast';
import { Layout } from './components/layout/Layout';
import { ApiError } from './api/client';
import { HomePage } from './pages/HomePage';

const OportunidadesPage = lazy(() => import('./pages/OportunidadesPage'));
const OportunidadeDetalhePage = lazy(() => import('./pages/OportunidadeDetalhePage'));
const PropostasPage = lazy(() => import('./pages/PropostasPage'));
const PropostaDetalhePage = lazy(() => import('./pages/PropostaDetalhePage'));
const OrganizacoesPage = lazy(() => import('./pages/OrganizacoesPage'));
const OrganizacaoDetalhePage = lazy(() => import('./pages/OrganizacaoDetalhePage'));
const ImpactoPage = lazy(() => import('./pages/ImpactoPage'));
const LoginPage = lazy(() => import('./pages/LoginPage'));
const CadastroPage = lazy(() => import('./pages/CadastroPage'));
const MinhaAreaPage = lazy(() => import('./pages/cidadao/MinhaAreaPage'));
const PainelOrganizacaoPage = lazy(() => import('./pages/organizacao/PainelOrganizacaoPage'));
const OportunidadeEdicaoPage = lazy(() => import('./pages/organizacao/OportunidadeEdicaoPage'));
const InscritosPage = lazy(() => import('./pages/organizacao/InscritosPage'));
const AdminPage = lazy(() => import('./pages/admin/AdminPage'));
const NotFoundPage = lazy(() => import('./pages/NotFoundPage'));

export function criarQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 30_000,
        refetchOnWindowFocus: false,
        // Não insiste em erros do cliente (4xx): só em falhas de rede/servidor.
        retry: (tentativas, erro) => tentativas < 2 && !(erro instanceof ApiError && erro.status >= 400 && erro.status < 500),
      },
    },
  });
}

const queryClient = criarQueryClient();

function Carregando() {
  return (
    <div className="grid min-h-[50vh] place-items-center" role="status" aria-label="Carregando página">
      <div className="size-10 animate-spin rounded-full border-4 border-brand-200 border-t-brand-600" />
    </div>
  );
}

const pagina = (el: ReactNode) => <Suspense fallback={<Carregando />}>{el}</Suspense>;

export function Rotas() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<HomePage />} />
        <Route path="oportunidades" element={pagina(<OportunidadesPage />)} />
        <Route path="oportunidades/:id" element={pagina(<OportunidadeDetalhePage />)} />
        <Route path="propostas" element={pagina(<PropostasPage />)} />
        <Route path="propostas/:id" element={pagina(<PropostaDetalhePage />)} />
        <Route path="organizacoes" element={pagina(<OrganizacoesPage />)} />
        <Route path="organizacoes/:id" element={pagina(<OrganizacaoDetalhePage />)} />
        <Route path="impacto" element={pagina(<ImpactoPage />)} />
        <Route path="entrar" element={pagina(<LoginPage />)} />
        <Route path="cadastro" element={pagina(<CadastroPage />)} />

        <Route path="minha-area" element={pagina(<RequireAuth perfis={['Cidadao']}><MinhaAreaPage /></RequireAuth>)} />

        <Route path="organizacao" element={pagina(<RequireAuth perfis={['Organizacao']}><PainelOrganizacaoPage /></RequireAuth>)} />
        <Route path="organizacao/oportunidades/nova" element={pagina(<RequireAuth perfis={['Organizacao']}><OportunidadeEdicaoPage /></RequireAuth>)} />
        <Route path="organizacao/oportunidades/:id/editar" element={pagina(<RequireAuth perfis={['Organizacao', 'Admin']}><OportunidadeEdicaoPage /></RequireAuth>)} />
        <Route path="organizacao/oportunidades/:id/inscritos" element={pagina(<RequireAuth perfis={['Organizacao', 'Admin']}><InscritosPage /></RequireAuth>)} />

        <Route path="admin" element={pagina(<RequireAuth perfis={['Admin']}><AdminPage /></RequireAuth>)} />

        <Route path="*" element={pagina(<NotFoundPage />)} />
      </Route>
    </Routes>
  );
}

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <MotionConfig reducedMotion="user">
        <ToastProvider>
          <AuthProvider>
            <BrowserRouter>
              <Rotas />
            </BrowserRouter>
          </AuthProvider>
        </ToastProvider>
      </MotionConfig>
    </QueryClientProvider>
  );
}
