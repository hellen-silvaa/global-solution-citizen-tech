import type { Inscricao, Oportunidade, Pagina, Proposta, RelatorioImpacto } from '../src/api/types';

export function oportunidade(parcial: Partial<Oportunidade> = {}): Oportunidade {
  return {
    id: 1,
    titulo: 'Introdução à Programação Web',
    descricao: 'HTML, CSS e JavaScript do zero.',
    tipo: 'Curso',
    modalidade: 'Presencial',
    area: 'Tecnologia',
    bairro: 'Capão Redondo',
    cargaHoraria: 40,
    vagasTotais: 30,
    vagasDisponiveis: 12,
    dataInicio: '2099-12-01T00:00:00',
    dataFim: '2099-12-20T00:00:00',
    gratuita: true,
    status: 'Aberta',
    organizacaoId: 2,
    organizacaoNome: 'Instituto Recomeçar',
    ...parcial,
  };
}

export function proposta(parcial: Partial<Proposta> = {}): Proposta {
  return {
    id: 1,
    titulo: 'Curso de Cuidador de Idosos',
    descricao: 'Muitas famílias do bairro precisam.',
    area: 'Saude',
    bairro: 'Jardim Ângela',
    status: 'Aberta',
    totalVotos: 3,
    autorId: 7,
    autorNome: 'Carla Mendes',
    oportunidadeGeradaId: null,
    criadaEm: '2026-11-05T00:00:00',
    ...parcial,
  };
}

export function inscricao(parcial: Partial<Inscricao> = {}): Inscricao {
  return {
    id: 1,
    cidadaoId: 5,
    cidadaoNome: 'Ana Souza',
    oportunidadeId: 1,
    oportunidadeTitulo: 'Introdução à Programação Web',
    status: 'Pendente',
    criadaEm: '2026-11-10T00:00:00',
    atualizadaEm: null,
    ...parcial,
  };
}

export function pagina<T>(itens: T[]): Pagina<T> {
  return { itens, pagina: 1, tamanhoPagina: 10, totalItens: itens.length, totalPaginas: 1 };
}

export const impacto: RelatorioImpacto = {
  totalInscricoes: 5,
  inscricoesConcluidas: 2,
  taxaConclusaoPercentual: 40,
  propostasAtendidas: 1,
  porBairro: [
    { bairro: 'Capão Redondo', oportunidadesAbertas: 2, vagasOfertadas: 53, inscricoesAtivas: 2, inscricoesConcluidas: 2, propostasAbertas: 0, propostasAtendidas: 1 },
    { bairro: 'Campo Limpo', oportunidadesAbertas: 2, vagasOfertadas: 34, inscricoesAtivas: 1, inscricoesConcluidas: 0, propostasAbertas: 1, propostasAtendidas: 0 },
  ],
  demandaReprimida: [{ area: 'Saude', oportunidadesAbertas: 0, propostasAbertas: 1, votosEmPropostasAbertas: 3 }],
};
