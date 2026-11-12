// Tipos espelhados do contrato OpenAPI da API Oportuniza (docs/openapi.json).

export const AREAS = [
  'Tecnologia',
  'Administracao',
  'Saude',
  'Gastronomia',
  'ConstrucaoCivil',
  'Idiomas',
  'Beleza',
  'Logistica',
  'Artes',
  'Empreendedorismo',
] as const;
export type AreaConhecimento = (typeof AREAS)[number];

export const ESCOLARIDADES = [
  'FundamentalIncompleto',
  'FundamentalCompleto',
  'MedioIncompleto',
  'MedioCompleto',
  'Tecnico',
  'SuperiorIncompleto',
  'SuperiorCompleto',
] as const;
export type Escolaridade = (typeof ESCOLARIDADES)[number];

export const MODALIDADES = ['Presencial', 'Online', 'Hibrida'] as const;
export type Modalidade = (typeof MODALIDADES)[number];

export const TIPOS_OPORTUNIDADE = ['Curso', 'Oficina', 'Mentoria', 'JovemAprendiz'] as const;
export type TipoOportunidade = (typeof TIPOS_OPORTUNIDADE)[number];

export const TIPOS_ORGANIZACAO = ['Ong', 'EscolaTecnica', 'Empresa', 'PoderPublico', 'ColetivoComunitario'] as const;
export type TipoOrganizacao = (typeof TIPOS_ORGANIZACAO)[number];

export const STATUS_INSCRICAO = ['Pendente', 'Confirmada', 'Concluida', 'Cancelada'] as const;
export type StatusInscricao = (typeof STATUS_INSCRICAO)[number];

export const STATUS_PROPOSTA = ['Aberta', 'Atendida', 'Arquivada'] as const;
export type StatusProposta = (typeof STATUS_PROPOSTA)[number];

export type StatusOportunidade = 'Aberta' | 'Encerrada' | 'Cancelada';
export type Perfil = 'Admin' | 'Cidadao' | 'Organizacao';

export interface Pagina<T> {
  itens: T[];
  pagina: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
}

export interface Endereco {
  cep: string;
  logradouro: string;
  numero?: string | null;
  bairro: string;
  cidade: string;
  uf: string;
}

export interface Sessao {
  token: string;
  tipo: string;
  expiraEm: string;
  usuarioId: number;
  nome: string;
  perfil: Perfil;
}

export interface Cidadao {
  id: number;
  nome: string;
  email: string;
  dataNascimento: string;
  escolaridade: Escolaridade;
  areasInteresse: AreaConhecimento[];
  endereco: Endereco;
  ativo: boolean;
  criadoEm: string;
}

export interface CidadaoListagem {
  id: number;
  nome: string;
  bairro: string;
  escolaridade: Escolaridade;
  ativo: boolean;
}

export interface CadastroCidadao {
  nome: string;
  email: string;
  senha: string;
  dataNascimento: string;
  escolaridade: Escolaridade;
  areasInteresse: AreaConhecimento[];
  endereco: Endereco;
}

export type AtualizacaoCidadao = Pick<CadastroCidadao, 'nome' | 'escolaridade' | 'areasInteresse' | 'endereco'>;

export interface Organizacao {
  id: number;
  nome: string;
  email: string;
  tipo: TipoOrganizacao;
  descricao: string;
  site?: string | null;
  endereco: Endereco;
  ativo: boolean;
  criadoEm: string;
}

export interface CadastroOrganizacao {
  nome: string;
  email: string;
  senha: string;
  tipo: TipoOrganizacao;
  descricao: string;
  site?: string | null;
  endereco: Endereco;
}

export type AtualizacaoOrganizacao = Pick<CadastroOrganizacao, 'nome' | 'descricao' | 'site' | 'endereco'>;

export interface Oportunidade {
  id: number;
  titulo: string;
  descricao: string;
  tipo: TipoOportunidade;
  modalidade: Modalidade;
  area: AreaConhecimento;
  bairro: string;
  cargaHoraria: number;
  vagasTotais: number;
  vagasDisponiveis: number;
  dataInicio: string;
  dataFim: string;
  gratuita: boolean;
  status: StatusOportunidade;
  organizacaoId: number;
  organizacaoNome: string;
}

export interface CriacaoOportunidade {
  titulo: string;
  descricao: string;
  tipo: TipoOportunidade;
  modalidade: Modalidade;
  area: AreaConhecimento;
  bairro: string;
  cargaHoraria: number;
  vagas: number;
  dataInicio: string;
  dataFim: string;
  gratuita: boolean;
}

export type AtualizacaoOportunidade = Omit<CriacaoOportunidade, 'tipo' | 'gratuita'>;

export interface FiltroOportunidades {
  area?: AreaConhecimento;
  bairro?: string;
  modalidade?: Modalidade;
  tipo?: TipoOportunidade;
  gratuita?: boolean;
  organizacaoId?: number;
  texto?: string;
  somenteComVagas?: boolean;
  pagina?: number;
  tamanho?: number;
}

export interface Inscricao {
  id: number;
  cidadaoId: number;
  cidadaoNome?: string | null;
  oportunidadeId: number;
  oportunidadeTitulo?: string | null;
  status: StatusInscricao;
  criadaEm: string;
  atualizadaEm?: string | null;
}

export interface Recomendacao {
  oportunidade: Oportunidade;
  pontuacao: number;
  motivos: string[];
}

export interface Proposta {
  id: number;
  titulo: string;
  descricao: string;
  area: AreaConhecimento;
  bairro: string;
  status: StatusProposta;
  totalVotos: number;
  autorId: number;
  autorNome: string;
  oportunidadeGeradaId?: number | null;
  criadaEm: string;
}

export interface CriacaoProposta {
  titulo: string;
  descricao: string;
  area: AreaConhecimento;
  bairro: string;
}

export interface FiltroPropostas {
  bairro?: string;
  area?: AreaConhecimento;
  status?: StatusProposta;
  ordenarPorVotos?: boolean;
  pagina?: number;
  tamanho?: number;
}

export interface IndicadorBairro {
  bairro: string;
  oportunidadesAbertas: number;
  vagasOfertadas: number;
  inscricoesAtivas: number;
  inscricoesConcluidas: number;
  propostasAbertas: number;
  propostasAtendidas: number;
}

export interface IndicadorArea {
  area: AreaConhecimento;
  oportunidadesAbertas: number;
  propostasAbertas: number;
  votosEmPropostasAbertas: number;
}

export interface RelatorioImpacto {
  totalInscricoes: number;
  inscricoesConcluidas: number;
  taxaConclusaoPercentual: number;
  propostasAtendidas: number;
  porBairro: IndicadorBairro[];
  demandaReprimida: IndicadorArea[];
}

export interface ErroValidacao {
  campo: string;
  mensagem: string;
}
