import { request } from './client';
import type {
  AtualizacaoCidadao,
  AtualizacaoOportunidade,
  AtualizacaoOrganizacao,
  CadastroCidadao,
  CadastroOrganizacao,
  Cidadao,
  CidadaoListagem,
  CriacaoOportunidade,
  CriacaoProposta,
  Endereco,
  FiltroOportunidades,
  FiltroPropostas,
  Inscricao,
  Oportunidade,
  Organizacao,
  Pagina,
  Proposta,
  Recomendacao,
  RelatorioImpacto,
  Sessao,
  StatusInscricao,
  TipoOrganizacao,
} from './types';

// Um objeto por recurso REST, espelhando os controllers da API.

export const authApi = {
  login: (email: string, senha: string) => request<Sessao>('/auth/login', { method: 'POST', body: { email, senha } }),
};

export const cidadaosApi = {
  cadastrar: (dados: CadastroCidadao) => request<Cidadao>('/cidadaos', { method: 'POST', body: dados }),
  listar: (filtro: { bairro?: string; pagina?: number; tamanho?: number } = {}) =>
    request<Pagina<CidadaoListagem>>('/cidadaos', { query: filtro }),
  obter: (id: number) => request<Cidadao>(`/cidadaos/${id}`),
  atualizar: (id: number, dados: AtualizacaoCidadao) => request<Cidadao>(`/cidadaos/${id}`, { method: 'PUT', body: dados }),
  desativar: (id: number) => request<void>(`/cidadaos/${id}`, { method: 'DELETE' }),
  inscricoes: (id: number, status?: StatusInscricao) =>
    request<Inscricao[]>(`/cidadaos/${id}/inscricoes`, { query: { status } }),
  recomendacoes: (id: number, limite = 6) =>
    request<Recomendacao[]>(`/cidadaos/${id}/recomendacoes`, { query: { limite } }),
};

export const organizacoesApi = {
  cadastrar: (dados: CadastroOrganizacao) => request<Organizacao>('/organizacoes', { method: 'POST', body: dados }),
  listar: (filtro: { tipo?: TipoOrganizacao; pagina?: number; tamanho?: number } = {}) =>
    request<Pagina<Organizacao>>('/organizacoes', { query: filtro }),
  obter: (id: number) => request<Organizacao>(`/organizacoes/${id}`),
  atualizar: (id: number, dados: AtualizacaoOrganizacao) =>
    request<Organizacao>(`/organizacoes/${id}`, { method: 'PUT', body: dados }),
  desativar: (id: number) => request<void>(`/organizacoes/${id}`, { method: 'DELETE' }),
};

export const oportunidadesApi = {
  buscar: (filtro: FiltroOportunidades = {}) =>
    request<Pagina<Oportunidade>>('/oportunidades', { query: { ...filtro } }),
  obter: (id: number) => request<Oportunidade>(`/oportunidades/${id}`),
  criar: (dados: CriacaoOportunidade) => request<Oportunidade>('/oportunidades', { method: 'POST', body: dados }),
  atualizar: (id: number, dados: AtualizacaoOportunidade) =>
    request<Oportunidade>(`/oportunidades/${id}`, { method: 'PUT', body: dados }),
  encerrar: (id: number) => request<void>(`/oportunidades/${id}/encerrar`, { method: 'PATCH' }),
  cancelar: (id: number) => request<void>(`/oportunidades/${id}/cancelar`, { method: 'PATCH' }),
  inscrever: (id: number) => request<Inscricao>(`/oportunidades/${id}/inscricoes`, { method: 'POST' }),
  inscritos: (id: number) => request<Inscricao[]>(`/oportunidades/${id}/inscricoes`),
};

export const inscricoesApi = {
  obter: (id: number) => request<Inscricao>(`/inscricoes/${id}`),
  confirmar: (id: number) => request<Inscricao>(`/inscricoes/${id}/confirmar`, { method: 'PATCH' }),
  concluir: (id: number) => request<Inscricao>(`/inscricoes/${id}/concluir`, { method: 'PATCH' }),
  cancelar: (id: number) => request<void>(`/inscricoes/${id}/cancelar`, { method: 'PATCH' }),
};

export const propostasApi = {
  listar: (filtro: FiltroPropostas = {}) => request<Pagina<Proposta>>('/propostas', { query: { ...filtro } }),
  obter: (id: number) => request<Proposta>(`/propostas/${id}`),
  criar: (dados: CriacaoProposta) => request<Proposta>('/propostas', { method: 'POST', body: dados }),
  votar: (id: number) => request<Proposta>(`/propostas/${id}/votos`, { method: 'POST' }),
  atender: (id: number, dados: CriacaoOportunidade) =>
    request<Oportunidade>(`/propostas/${id}/atender`, { method: 'POST', body: dados }),
  arquivar: (id: number) => request<void>(`/propostas/${id}/arquivar`, { method: 'PATCH' }),
};

export const indicadoresApi = {
  impacto: () => request<RelatorioImpacto>('/indicadores/impacto'),
};

export const enderecosApi = {
  consultar: (cep: string) => request<Endereco>(`/enderecos/${cep.replace(/\D/g, '')}`),
};
