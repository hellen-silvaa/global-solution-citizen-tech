import { afterEach, describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import LoginPage from './LoginPage';
import CadastroPage, { validarComum } from './CadastroPage';
import OportunidadesPage, { lerFiltro } from './OportunidadesPage';
import OportunidadeDetalhePage from './OportunidadeDetalhePage';
import PropostasPage from './PropostasPage';
import ImpactoPage from './ImpactoPage';
import InscritosPage from './organizacao/InscritosPage';
import { RequireAuth } from '../auth/RequireAuth';
import { lerSessao } from '../auth/AuthProvider';
import { logar, mockarApi, renderizar, sessaoFake } from '../../test/utils';
import { impacto, inscricao, oportunidade, pagina, proposta } from '../../test/fixtures';

afterEach(() => vi.unstubAllGlobals());

describe('sessão e rotas protegidas', () => {
  it('descarta sessão expirada', () => {
    localStorage.setItem('oportuniza.sessao', JSON.stringify(sessaoFake({ expiraEm: '2000-01-01T00:00:00Z' })));
    expect(lerSessao()).toBeNull();
    expect(localStorage.getItem('oportuniza.sessao')).toBeNull();
  });

  it('manda para o login quem não está logado', () => {
    renderizar(<RequireAuth><p>secreto</p></RequireAuth>, { rota: '/minha-area', caminho: '/minha-area', extras: [{ caminho: '/entrar', elemento: <p>tela de login</p> }] });
    expect(screen.getByText('tela de login')).toBeInTheDocument();
  });

  it('bloqueia perfil errado (como o Authorize(Roles) da API)', () => {
    logar({ perfil: 'Organizacao' });
    renderizar(<RequireAuth perfis={['Admin']}><p>secreto</p></RequireAuth>);
    expect(screen.queryByText('secreto')).not.toBeInTheDocument();
    expect(screen.getByText(/não é para o seu perfil/)).toBeInTheDocument();
  });

  it('libera o perfil certo', () => {
    logar({ perfil: 'Admin' });
    renderizar(<RequireAuth perfis={['Admin']}><p>secreto</p></RequireAuth>);
    expect(screen.getByText('secreto')).toBeInTheDocument();
  });
});

describe('LoginPage', () => {
  it('entra e leva o cidadão para a minha área', async () => {
    const chamadas = mockarApi([{ metodo: 'POST', url: /auth\/login/, corpo: sessaoFake() }]);
    renderizar(<LoginPage />, { rota: '/entrar', caminho: '/entrar', extras: [{ caminho: '/minha-area', elemento: <p>área do cidadão</p> }] });
    await userEvent.type(screen.getByLabelText('E-mail'), 'ana@exemplo.local');
    await userEvent.type(screen.getByLabelText('Senha'), 'Senha@123');
    await userEvent.click(screen.getByRole('button', { name: 'Entrar' }));
    expect(await screen.findByText('área do cidadão')).toBeInTheDocument();
    expect(chamadas[0].corpo).toEqual({ email: 'ana@exemplo.local', senha: 'Senha@123' });
    expect(lerSessao()?.nome).toBe('Ana Souza');
  });

  it('mostra erro genérico em 401 (não revela se o e-mail existe)', async () => {
    mockarApi([{ metodo: 'POST', url: /auth\/login/, status: 401, corpo: { mensagem: 'Credenciais inválidas' } }]);
    renderizar(<LoginPage />);
    await userEvent.type(screen.getByLabelText('E-mail'), 'x@x.com');
    await userEvent.type(screen.getByLabelText('Senha'), 'errada123');
    await userEvent.click(screen.getByRole('button', { name: 'Entrar' }));
    expect(await screen.findByRole('alert')).toHaveTextContent('E-mail ou senha incorretos.');
  });

  it('mostra o bloqueio por força bruta (429)', async () => {
    mockarApi([{ metodo: 'POST', url: /auth\/login/, status: 429, corpo: { mensagem: 'Conta bloqueada temporariamente.' } }]);
    renderizar(<LoginPage />);
    await userEvent.click(screen.getByRole('button', { name: /Cidadã/ }));
    await userEvent.click(screen.getByRole('button', { name: 'Entrar' }));
    expect(await screen.findByRole('alert')).toHaveTextContent('Conta bloqueada temporariamente.');
  });

  it('pede e-mail e senha', async () => {
    renderizar(<LoginPage />);
    await userEvent.click(screen.getByRole('button', { name: 'Entrar' }));
    expect(screen.getByRole('alert')).toHaveTextContent('Informe e-mail e senha.');
  });
});

describe('CadastroPage', () => {
  it('valida os campos comuns', () => {
    const erros = validarComum({ nome: '', email: 'x', senha: 'abc', confirmacao: 'abd', endereco: { cep: '1', logradouro: '', bairro: '', cidade: '', uf: 's' } });
    expect(Object.keys(erros).sort()).toEqual(['bairro', 'cep', 'cidade', 'confirmacao', 'email', 'logradouro', 'nome', 'senha', 'uf'].sort());
  });

  it('preenche o endereço pelo ViaCEP', async () => {
    mockarApi([{ url: /enderecos\/01001000/, corpo: { cep: '01001000', logradouro: 'Praça da Sé', bairro: 'Sé', cidade: 'São Paulo', uf: 'SP' } }]);
    renderizar(<CadastroPage />);
    await userEvent.type(screen.getByLabelText('CEP'), '01001000');
    await waitFor(() => expect(screen.getByLabelText('Rua / logradouro')).toHaveValue('Praça da Sé'));
    expect(screen.getByLabelText('Bairro')).toHaveValue('Sé');
    expect(screen.getByLabelText('UF')).toHaveValue('SP');
    expect(screen.getByText(/encontrado pelo ViaCEP/)).toBeInTheDocument();
  });

  it('mostra erros antes de enviar e troca para organização', async () => {
    const chamadas = mockarApi([]);
    renderizar(<CadastroPage />);
    await userEvent.click(screen.getByRole('button', { name: 'Criar minha conta' }));
    expect(screen.getByText('Escolha pelo menos uma área')).toBeInTheDocument();
    expect(screen.getByText('É preciso concordar para continuar')).toBeInTheDocument();
    expect(chamadas).toHaveLength(0);
    await userEvent.click(screen.getByRole('tab', { name: /organização/ }));
    expect(screen.getByLabelText('Nome da organização')).toBeInTheDocument();
    expect(await screen.findByLabelText('Tipo de organização')).toBeInTheDocument();
  });

  it('cadastra cidadão e já entra', async () => {
    const chamadas = mockarApi([
      { metodo: 'POST', url: /\/cidadaos$/, status: 201, corpo: { id: 99 } },
      { metodo: 'POST', url: /auth\/login/, corpo: sessaoFake({ usuarioId: 99, nome: 'Novo Cidadão' }) },
      { url: /enderecos\/05767000/, corpo: { cep: '05767000', logradouro: 'Rua Professora Nina Stocco', bairro: 'Capão Redondo', cidade: 'São Paulo', uf: 'SP' } },
    ]);
    renderizar(<CadastroPage />, { rota: '/cadastro', caminho: '/cadastro', extras: [{ caminho: '/minha-area', elemento: <p>bem-vindo</p> }] });
    await userEvent.type(screen.getByLabelText('Nome completo'), 'Novo Cidadão');
    await userEvent.type(screen.getByLabelText('E-mail'), 'novo@exemplo.local');
    await userEvent.type(screen.getByLabelText('Data de nascimento'), '2005-03-10');
    await userEvent.click(screen.getByRole('checkbox', { name: 'Tecnologia' }));
    await userEvent.type(screen.getByLabelText('CEP'), '05767000');
    await waitFor(() => expect(screen.getByLabelText('Cidade')).toHaveValue('São Paulo'));
    await userEvent.type(screen.getByLabelText('Senha'), 'Senha@123');
    await userEvent.type(screen.getByLabelText('Confirme a senha'), 'Senha@123');
    await userEvent.click(screen.getByRole('checkbox', { name: /LGPD/ }));
    await userEvent.click(screen.getByRole('button', { name: 'Criar minha conta' }));
    expect(await screen.findByText('bem-vindo')).toBeInTheDocument();
    const cadastro = chamadas.find((c) => c.url.endsWith('/cidadaos'))!;
    expect(cadastro.corpo).toMatchObject({ nome: 'Novo Cidadão', dataNascimento: '2005-03-10T00:00:00', areasInteresse: ['Tecnologia'], endereco: { uf: 'SP', cep: '05767-000', bairro: 'Capão Redondo', numero: null } });
  });

  it('mostra no campo o erro de validação que veio da API', async () => {
    mockarApi([{ metodo: 'POST', url: /\/organizacoes$/, status: 400, corpo: [{ campo: 'Email', mensagem: 'E-mail já cadastrado!' }] }]);
    renderizar(<CadastroPage />, { rota: '/cadastro?perfil=organizacao' });
    await userEvent.type(screen.getByLabelText('Nome da organização'), 'ONG Teste');
    await userEvent.type(screen.getByLabelText('E-mail'), 'ong@teste.local');
    await userEvent.type(screen.getByLabelText('Descrição'), 'Ajuda o bairro');
    await userEvent.type(screen.getByLabelText('CEP'), '05767-000');
    await userEvent.type(screen.getByLabelText('Rua / logradouro'), 'Rua A');
    await userEvent.type(screen.getByLabelText('Bairro'), 'Campo Limpo');
    await userEvent.type(screen.getByLabelText('Cidade'), 'São Paulo');
    await userEvent.type(screen.getByLabelText('UF'), 'SP');
    await userEvent.type(screen.getByLabelText('Senha'), 'Senha@123');
    await userEvent.type(screen.getByLabelText('Confirme a senha'), 'Senha@123');
    await userEvent.click(screen.getByRole('checkbox', { name: /LGPD/ }));
    await userEvent.click(screen.getByRole('button', { name: 'Criar minha conta' }));
    expect(await screen.findByText('E-mail já cadastrado!')).toBeInTheDocument();
  });
});

describe('OportunidadesPage', () => {
  it('lê os filtros da URL', () => {
    const f = lerFiltro(new URLSearchParams('area=Saude&gratuita=true&todas=true&pagina=3'));
    expect(f).toMatchObject({ area: 'Saude', gratuita: true, somenteComVagas: false, pagina: 3 });
    expect(lerFiltro(new URLSearchParams())).toMatchObject({ somenteComVagas: true, pagina: 1, gratuita: undefined });
  });

  it('lista resultados e aplica filtro de área na chamada à API', async () => {
    const chamadas = mockarApi([{ url: /\/oportunidades\?/, corpo: pagina([oportunidade()]) }]);
    renderizar(<OportunidadesPage />, { rota: '/oportunidades' });
    expect(await screen.findByText('Introdução à Programação Web')).toBeInTheDocument();
    expect(screen.getByText('1 oportunidade encontrada')).toBeInTheDocument();
    await userEvent.selectOptions(screen.getByLabelText('Área'), 'Saude');
    await waitFor(() => expect(chamadas.some((c) => c.url.includes('area=Saude'))).toBe(true));
    expect(chamadas[0].url).toContain('somenteComVagas=true');
    expect(screen.getByRole('button', { name: 'Limpar filtros' })).toBeInTheDocument();
  });

  it('mostra estado vazio', async () => {
    mockarApi([{ url: /\/oportunidades\?/, corpo: pagina([]) }]);
    renderizar(<OportunidadesPage />);
    expect(await screen.findByText('Nada por aqui ainda')).toBeInTheDocument();
  });

  it('mostra erro com opção de tentar de novo', async () => {
    mockarApi([{ url: /\/oportunidades\?/, status: 500, corpo: { mensagem: 'Erro interno' } }]);
    renderizar(<OportunidadesPage />);
    expect(await screen.findByText('Erro interno')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Tentar de novo/ })).toBeInTheDocument();
  });
});

describe('OportunidadeDetalhePage', () => {
  const rota = { rota: '/oportunidades/1', caminho: '/oportunidades/:id' };

  it('pede login para visitante', async () => {
    mockarApi([{ url: /\/oportunidades\/1$/, corpo: oportunidade() }]);
    renderizar(<OportunidadeDetalhePage />, rota);
    expect(await screen.findByRole('heading', { name: 'Introdução à Programação Web' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Entrar para se inscrever' })).toBeInTheDocument();
  });

  it('cidadão se inscreve', async () => {
    logar();
    const chamadas = mockarApi([
      { url: /\/oportunidades\/1$/, corpo: oportunidade() },
      { url: /\/cidadaos\/5\/inscricoes/, corpo: [] },
      { metodo: 'POST', url: /\/oportunidades\/1\/inscricoes/, status: 201, corpo: inscricao() },
    ]);
    renderizar(<OportunidadeDetalhePage />, rota);
    await userEvent.click(await screen.findByRole('button', { name: 'Quero me inscrever' }));
    expect(await screen.findByText('Inscrição feita!')).toBeInTheDocument();
    expect(chamadas.some((c) => c.metodo === 'POST')).toBe(true);
  });

  it('mostra que já está inscrito', async () => {
    logar();
    mockarApi([
      { url: /\/oportunidades\/1$/, corpo: oportunidade() },
      { url: /\/cidadaos\/5\/inscricoes/, corpo: [inscricao({ status: 'Confirmada' })] },
    ]);
    renderizar(<OportunidadeDetalhePage />, rota);
    expect(await screen.findByText('Você está inscrito')).toBeInTheDocument();
  });

  it('mostra a regra de negócio violada', async () => {
    logar();
    mockarApi([
      { url: /\/oportunidades\/1$/, corpo: oportunidade() },
      { url: /\/cidadaos\/5\/inscricoes/, corpo: [] },
      { metodo: 'POST', url: /inscricoes/, status: 400, corpo: { mensagem: 'Cidadão já possui 3 inscrições ativas!' } },
    ]);
    renderizar(<OportunidadeDetalhePage />, rota);
    await userEvent.click(await screen.findByRole('button', { name: 'Quero me inscrever' }));
    expect(await screen.findByText('Cidadão já possui 3 inscrições ativas!')).toBeInTheDocument();
  });

  it('organização dona vê a gestão e encerra inscrições', async () => {
    logar({ perfil: 'Organizacao', usuarioId: 2, nome: 'Instituto Recomeçar' });
    const chamadas = mockarApi([
      { url: /\/oportunidades\/1$/, corpo: oportunidade() },
      { metodo: 'PATCH', url: /encerrar/, status: 204 },
    ]);
    renderizar(<OportunidadeDetalhePage />, rota);
    await userEvent.click(await screen.findByRole('button', { name: 'Encerrar inscrições' }));
    await userEvent.click(screen.getByRole('button', { name: 'Encerrar' }));
    await waitFor(() => expect(chamadas.some((c) => c.metodo === 'PATCH')).toBe(true));
    expect(screen.getByRole('link', { name: 'Ver inscritos' })).toHaveAttribute('href', '/organizacao/oportunidades/1/inscritos');
  });

  it('outra organização não vê a gestão', async () => {
    logar({ perfil: 'Organizacao', usuarioId: 3 });
    mockarApi([{ url: /\/oportunidades\/1$/, corpo: oportunidade() }]);
    renderizar(<OportunidadeDetalhePage />, rota);
    await screen.findByRole('heading', { name: 'Introdução à Programação Web' });
    expect(screen.queryByText('Gestão')).not.toBeInTheDocument();
  });
});

describe('PropostasPage', () => {
  it('cidadão vota e o botão fica marcado', async () => {
    logar();
    mockarApi([
      { url: /\/propostas\?/, corpo: pagina([proposta()]) },
      { metodo: 'POST', url: /\/propostas\/1\/votos/, corpo: proposta({ totalVotos: 4 }) },
    ]);
    renderizar(<PropostasPage />);
    await userEvent.click(await screen.findByRole('button', { name: /Votar em Curso de Cuidador/ }));
    expect(await screen.findByText('Voto registrado!')).toBeInTheDocument();
    expect(await screen.findByRole('button', { name: /já votou/ })).toBeInTheDocument();
  });

  it('cria proposta pelo modal', async () => {
    logar();
    const chamadas = mockarApi([
      { url: /\/propostas\?/, corpo: pagina([]) },
      { metodo: 'POST', url: /\/propostas$/, status: 201, corpo: proposta({ id: 2 }) },
    ]);
    renderizar(<PropostasPage />);
    await userEvent.click(await screen.findByRole('button', { name: 'Nova proposta' }));
    await userEvent.type(screen.getByLabelText('Título'), 'Curso de eletricista');
    await userEvent.type(screen.getByLabelText('Por que é importante?'), 'Muita demanda');
    await userEvent.type(screen.getAllByLabelText('Bairro').at(-1)!, 'Campo Limpo');
    await userEvent.click(screen.getByRole('button', { name: 'Publicar proposta' }));
    expect(await screen.findByText('Proposta publicada!')).toBeInTheDocument();
    expect(chamadas.find((c) => c.metodo === 'POST')?.corpo).toMatchObject({ titulo: 'Curso de eletricista', bairro: 'Campo Limpo' });
  });

  it('visitante é convidado a entrar', async () => {
    mockarApi([{ url: /\/propostas\?/, corpo: pagina([proposta()]) }]);
    renderizar(<PropostasPage />);
    expect(await screen.findByRole('link', { name: 'Entre para propor e votar' })).toBeInTheDocument();
  });
});

describe('ImpactoPage', () => {
  it('mostra os indicadores da API', async () => {
    mockarApi([{ url: /indicadores\/impacto/, corpo: impacto }]);
    renderizar(<ImpactoPage />);
    expect(await screen.findByText('Taxa de conclusão')).toBeInTheDocument();
    expect(screen.getByText('Qualificações concluídas')).toBeInTheDocument();
    expect(screen.getByRole('region', { name: 'Inscrições por bairro' })).toBeInTheDocument();
    expect(screen.getByRole('region', { name: 'Demanda reprimida' })).toBeInTheDocument();
  });
});

describe('InscritosPage', () => {
  it('organização confirma e conclui inscrições', async () => {
    logar({ perfil: 'Organizacao', usuarioId: 2 });
    const chamadas = mockarApi([
      { url: /\/oportunidades\/1$/, corpo: oportunidade() },
      { url: /\/oportunidades\/1\/inscricoes$/, corpo: [inscricao(), inscricao({ id: 2, cidadaoNome: 'Bruno Lima', status: 'Confirmada' })] },
      { metodo: 'PATCH', url: /inscricoes\/1\/confirmar/, corpo: inscricao({ status: 'Confirmada' }) },
      { metodo: 'PATCH', url: /inscricoes\/2\/concluir/, corpo: inscricao({ id: 2, status: 'Concluida' }) },
    ]);
    renderizar(<InscritosPage />, { rota: '/organizacao/oportunidades/1/inscritos', caminho: '/organizacao/oportunidades/:id/inscritos' });
    await userEvent.click(await screen.findByRole('button', { name: 'Confirmar' }));
    await userEvent.click(screen.getByRole('button', { name: 'Concluir' }));
    await waitFor(() => expect(chamadas.filter((c) => c.metodo === 'PATCH').map((c) => c.url.split('/inscricoes/')[1])).toEqual(['1/confirmar', '2/concluir']));
    await userEvent.click(screen.getByRole('button', { name: /^Concluída/ }));
    expect(screen.getByText('Nenhuma inscrição com este status')).toBeInTheDocument();
  });
});
