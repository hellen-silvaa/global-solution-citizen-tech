import { afterEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import { QueryClientProvider } from '@tanstack/react-query';
import { Rotas, criarQueryClient } from '../App';
import { AuthProvider } from '../auth/AuthProvider';
import { ToastProvider } from '../components/ui/Toast';
import { Pagination } from '../components/ui/Pagination';
import { logar, mockarApi, renderizar } from '../../test/utils';
import { impacto, inscricao, oportunidade, pagina, proposta } from '../../test/fixtures';
import type { Cidadao, Organizacao } from '../api/types';

afterEach(() => vi.unstubAllGlobals());

const endereco = { cep: '05767-000', logradouro: 'Rua A', numero: '10', bairro: 'Capão Redondo', cidade: 'São Paulo', uf: 'SP' };
const cidadao: Cidadao = { id: 5, nome: 'Ana Souza', email: 'ana@exemplo.local', dataNascimento: '2004-05-10T00:00:00', escolaridade: 'MedioCompleto', areasInteresse: ['Tecnologia'], endereco, ativo: true, criadoEm: '2026-10-01T00:00:00' };
const org: Organizacao = { id: 2, nome: 'Instituto Recomeçar', email: 'contato@recomecar.local', tipo: 'Ong', descricao: 'ONG da Zona Sul', site: 'https://recomecar.org', endereco, ativo: true, criadoEm: '2026-09-01T00:00:00' };

/** Renderiza o app inteiro (rotas reais, lazy loading, layout) em uma URL. */
function app(rota: string) {
  return render(
    <QueryClientProvider client={criarQueryClient()}>
      <ToastProvider>
        <AuthProvider>
          <MemoryRouter initialEntries={[rota]}>
            <Rotas />
          </MemoryRouter>
        </AuthProvider>
      </ToastProvider>
    </QueryClientProvider>,
  );
}

const rotasPublicas = [
  { url: /\/indicadores\/impacto/, corpo: impacto },
  { url: /\/oportunidades\?/, corpo: pagina([oportunidade(), oportunidade({ id: 2, titulo: 'Oficina de Currículo', area: 'Administracao' })]) },
  { url: /\/organizacoes\?/, corpo: pagina([org]) },
  { url: /\/propostas\?/, corpo: pagina([proposta()]) },
];

describe('app completo', () => {
  it('home mostra hero, indicadores ao vivo, ODS e propostas', async () => {
    mockarApi(rotasPublicas);
    app('/');
    expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Qualificação gratuita');
    expect(await screen.findByText('vagas gratuitas ofertadas')).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: /4 Objetivos de Desenvolvimento Sustentável/ })).toBeInTheDocument();
    expect(await screen.findAllByText('Curso de Cuidador de Idosos')).not.toHaveLength(0);
    expect(screen.getByRole('link', { name: 'Pular para o conteúdo' })).toBeInTheDocument();
  });

  it('menu do visitante e troca de tema', async () => {
    mockarApi(rotasPublicas);
    app('/');
    const nav = screen.getByRole('navigation', { name: 'Principal' });
    expect(within(nav).getByRole('link', { name: 'Oportunidades' })).toBeInTheDocument();
    await userEvent.click(screen.getByRole('button', { name: 'Ativar modo escuro' }));
    expect(document.documentElement).toHaveClass('dark');
    expect(localStorage.getItem('oportuniza.tema')).toBe('dark');
    await userEvent.click(screen.getByRole('button', { name: 'Ativar modo claro' }));
    expect(document.documentElement).not.toHaveClass('dark');
    await userEvent.click(screen.getByRole('button', { name: 'Abrir menu' }));
    expect(screen.getByRole('navigation', { name: 'Menu' })).toBeInTheDocument();
  });

  it('usuário logado vê o atalho do perfil e pode sair', async () => {
    logar({ perfil: 'Admin', nome: 'Admin Oportuniza' });
    mockarApi(rotasPublicas);
    app('/');
    const nav = screen.getByRole('navigation', { name: 'Principal' });
    expect(within(nav).getByRole('link', { name: 'Administração' })).toHaveAttribute('href', '/admin');
    await userEvent.click(screen.getByRole('button', { name: 'Sair' }));
    expect(localStorage.getItem('oportuniza.sessao')).toBeNull();
    expect(screen.getAllByRole('link', { name: 'Entrar' }).length).toBeGreaterThan(0);
  });

  it('rota inexistente mostra 404', async () => {
    mockarApi([]);
    app('/nao-existe');
    expect(await screen.findByText('Esta página não existe')).toBeInTheDocument();
  });

  it('lista e detalha organizações', async () => {
    mockarApi([...rotasPublicas, { url: /\/organizacoes\/2$/, corpo: org }]);
    app('/organizacoes');
    await userEvent.click(await screen.findByRole('link', { name: /Instituto Recomeçar/ }));
    expect(await screen.findByRole('heading', { name: 'Instituto Recomeçar', level: 1 })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'recomecar.org' })).toHaveAttribute('rel', 'noopener noreferrer nofollow');
  });

  it('não renderiza site com javascript:', async () => {
    mockarApi([...rotasPublicas, { url: /\/organizacoes\/2$/, corpo: { ...org, site: 'javascript:alert(1)' } }]);
    app('/organizacoes/2');
    await screen.findByRole('heading', { name: 'Instituto Recomeçar', level: 1 });
    expect(screen.queryByRole('link', { name: /alert/ })).not.toBeInTheDocument();
  });
});

describe('minha área do cidadão', () => {
  const base = [
    { url: /\/cidadaos\/5$/, corpo: cidadao },
    { url: /\/cidadaos\/5\/recomendacoes/, corpo: [{ oportunidade: oportunidade(), pontuacao: 6, motivos: ['É gratuita'] }] },
    { url: /\/cidadaos\/5\/inscricoes/, corpo: [inscricao(), inscricao({ id: 2, status: 'Concluida', oportunidadeTitulo: 'Informática Básica' })] },
    { url: /\/propostas\?/, corpo: pagina([proposta({ autorId: 5, titulo: 'Minha proposta' }), proposta({ id: 2, titulo: 'De outra pessoa' })]) },
  ];

  it('mostra recomendações com motivos', async () => {
    logar();
    mockarApi(base);
    app('/minha-area');
    expect(await screen.findByText('Por que recomendamos')).toBeInTheDocument();
    expect(screen.getByText('6 pts')).toBeInTheDocument();
  });

  it('lista inscrições e cancela uma', async () => {
    logar();
    const chamadas = mockarApi([...base, { metodo: 'PATCH', url: /inscricoes\/1\/cancelar/, status: 204 }]);
    app('/minha-area?aba=inscricoes');
    expect(await screen.findByText('Informática Básica')).toBeInTheDocument();
    await userEvent.click(screen.getByRole('button', { name: 'Cancelar' }));
    await userEvent.click(screen.getByRole('button', { name: 'Cancelar inscrição' }));
    await waitFor(() => expect(chamadas.some((c) => c.metodo === 'PATCH')).toBe(true));
  });

  it('mostra só as propostas da própria pessoa', async () => {
    logar();
    mockarApi(base);
    app('/minha-area?aba=propostas');
    expect(await screen.findByText('Minha proposta')).toBeInTheDocument();
    expect(screen.queryByText('De outra pessoa')).not.toBeInTheDocument();
  });

  it('edita o perfil', async () => {
    logar();
    const chamadas = mockarApi([...base, { metodo: 'PUT', url: /\/cidadaos\/5$/, corpo: { ...cidadao, nome: 'Ana S.' } }]);
    app('/minha-area?aba=perfil');
    const nome = await screen.findByLabelText('Nome');
    await userEvent.clear(nome);
    await userEvent.type(nome, 'Ana S.');
    await userEvent.click(screen.getByRole('checkbox', { name: 'Saúde' }));
    await userEvent.click(screen.getByRole('button', { name: 'Salvar alterações' }));
    expect(await screen.findByText('Perfil atualizado')).toBeInTheDocument();
    expect(chamadas.find((c) => c.metodo === 'PUT')?.corpo).toMatchObject({ nome: 'Ana S.', areasInteresse: ['Tecnologia', 'Saude'] });
  });
});

describe('painel da organização', () => {
  it('lista oportunidades com totais e pedidos da comunidade', async () => {
    logar({ perfil: 'Organizacao', usuarioId: 2, nome: 'Instituto Recomeçar' });
    mockarApi([{ url: /\/organizacoes\/2$/, corpo: org }, ...rotasPublicas]);
    app('/organizacao');
    expect(await screen.findByText('Oportunidades abertas')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Editar Introdução à Programação Web' })).toHaveAttribute('href', '/organizacao/oportunidades/1/editar');
    await userEvent.click(screen.getByRole('tab', { name: /Pedidos da comunidade/ }));
    expect(await screen.findByText('Curso de Cuidador de Idosos')).toBeInTheDocument();
    await userEvent.click(screen.getByRole('tab', { name: /Perfil da organização/ }));
    expect(await screen.findByDisplayValue('ONG da Zona Sul')).toBeInTheDocument();
  });

  it('publica nova oportunidade', async () => {
    logar({ perfil: 'Organizacao', usuarioId: 2 });
    const chamadas = mockarApi([
      { metodo: 'POST', url: /\/oportunidades$/, status: 201, corpo: oportunidade({ id: 50, titulo: 'Curso novo' }) },
      { url: /\/oportunidades\/50$/, corpo: oportunidade({ id: 50, titulo: 'Curso novo' }) },
    ]);
    app('/organizacao/oportunidades/nova');
    await userEvent.type(await screen.findByLabelText('Título'), 'Curso novo');
    await userEvent.type(screen.getByLabelText('Descrição'), 'Descrição do curso');
    await userEvent.type(screen.getByLabelText('Bairro'), 'Campo Limpo');
    await userEvent.click(screen.getByRole('button', { name: 'Publicar oportunidade' }));
    expect(await screen.findByRole('heading', { name: 'Curso novo' })).toBeInTheDocument();
    expect(chamadas.find((c) => c.metodo === 'POST')?.corpo).toMatchObject({ titulo: 'Curso novo', tipo: 'Curso', gratuita: true });
  });

  it('edita oportunidade sem mandar tipo nem gratuita', async () => {
    logar({ perfil: 'Organizacao', usuarioId: 2 });
    const chamadas = mockarApi([
      { url: /\/oportunidades\/1$/, corpo: oportunidade() },
      { metodo: 'PUT', url: /\/oportunidades\/1$/, corpo: oportunidade({ vagasTotais: 40 }) },
    ]);
    app('/organizacao/oportunidades/1/editar');
    const vagas = await screen.findByLabelText('Vagas');
    await userEvent.clear(vagas);
    await userEvent.type(vagas, '40');
    await userEvent.click(screen.getByRole('button', { name: 'Salvar alterações' }));
    await waitFor(() => expect(chamadas.some((c) => c.metodo === 'PUT')).toBe(true));
    const corpo = chamadas.find((c) => c.metodo === 'PUT')!.corpo as Record<string, unknown>;
    expect(corpo.vagas).toBe(40);
    expect(corpo).not.toHaveProperty('tipo');
    expect(corpo).not.toHaveProperty('gratuita');
  });

  it('atende uma proposta e vai para a oportunidade criada', async () => {
    logar({ perfil: 'Organizacao', usuarioId: 2 });
    const chamadas = mockarApi([
      { url: /\/propostas\/1$/, corpo: proposta() },
      { metodo: 'POST', url: /\/propostas\/1\/atender/, status: 201, corpo: oportunidade({ id: 77, titulo: 'Curso de Cuidador de Idosos' }) },
      { url: /\/oportunidades\/77$/, corpo: oportunidade({ id: 77, titulo: 'Curso de Cuidador de Idosos' }) },
    ]);
    app('/propostas/1');
    await userEvent.click(await screen.findByRole('button', { name: 'Atender esta proposta' }));
    expect(screen.getByLabelText('Título')).toHaveValue('Curso de Cuidador de Idosos');
    await userEvent.click(screen.getByRole('button', { name: 'Publicar e atender' }));
    expect(await screen.findByText('Proposta atendida!')).toBeInTheDocument();
    expect(chamadas.find((c) => c.url.includes('atender'))?.corpo).toMatchObject({ area: 'Saude', bairro: 'Jardim Ângela' });
  });
});

describe('administração', () => {
  it('visão geral, cidadãos (sem e-mail na lista), organizações e propostas', async () => {
    logar({ perfil: 'Admin', usuarioId: 1, nome: 'Admin' });
    const chamadas = mockarApi([
      ...rotasPublicas,
      { url: /\/cidadaos\?/, corpo: pagina([{ id: 5, nome: 'Ana Souza', bairro: 'Capão Redondo', escolaridade: 'MedioCompleto', ativo: true }]) },
      { url: /\/cidadaos\/5$/, corpo: cidadao },
      { metodo: 'DELETE', url: /\/cidadaos\/5$/, status: 204 },
      { metodo: 'PATCH', url: /\/propostas\/1\/arquivar/, status: 204 },
    ]);
    app('/admin');
    expect(await screen.findByText('Taxa de conclusão')).toBeInTheDocument();

    await userEvent.click(screen.getByRole('tab', { name: /Cidadãos/ }));
    expect(await screen.findByRole('rowheader', { name: 'Ana Souza' })).toBeInTheDocument();
    expect(screen.queryByText('ana@exemplo.local')).not.toBeInTheDocument();
    await userEvent.click(screen.getByRole('button', { name: 'Ver Ana Souza' }));
    expect(await screen.findByText('ana@exemplo.local')).toBeInTheDocument();
    await userEvent.click(screen.getByRole('button', { name: 'Fechar' }));
    await userEvent.click(screen.getByRole('button', { name: 'Desativar Ana Souza' }));
    await userEvent.click(screen.getByRole('button', { name: 'Desativar' }));
    await waitFor(() => expect(chamadas.some((c) => c.metodo === 'DELETE')).toBe(true));

    await userEvent.click(screen.getByRole('tab', { name: /Organizações/ }));
    expect(await screen.findByRole('link', { name: 'Instituto Recomeçar' })).toBeInTheDocument();

    await userEvent.click(screen.getByRole('tab', { name: /Propostas/ }));
    await userEvent.click(await screen.findByRole('button', { name: /Arquivar Curso de Cuidador/ }));
    await userEvent.click(screen.getByRole('button', { name: 'Arquivar' }));
    await waitFor(() => expect(chamadas.some((c) => c.url.includes('arquivar'))).toBe(true));
  });
});

describe('Pagination', () => {
  it('navega e desabilita nas pontas', async () => {
    const aoMudar = vi.fn();
    renderizar(<Pagination pagina={1} totalPaginas={3} aoMudar={aoMudar} />);
    expect(screen.getByRole('button', { name: /Anterior/ })).toBeDisabled();
    await userEvent.click(screen.getByRole('button', { name: /Próxima/ }));
    expect(aoMudar).toHaveBeenCalledWith(2);
  });

  it('some com uma página só', () => {
    renderizar(<Pagination pagina={1} totalPaginas={1} aoMudar={() => {}} />);
    expect(screen.queryByRole('navigation', { name: 'Paginação' })).not.toBeInTheDocument();
  });
});
