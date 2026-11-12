import {
  Briefcase,
  ChefHat,
  Code2,
  HardHat,
  HeartPulse,
  Languages,
  Lightbulb,
  Palette,
  Scissors,
  Truck,
  type LucideIcon,
} from 'lucide-react';
import type {
  AreaConhecimento,
  Escolaridade,
  Modalidade,
  Perfil,
  StatusInscricao,
  StatusOportunidade,
  StatusProposta,
  TipoOportunidade,
  TipoOrganizacao,
} from '../api/types';

export const AREA_INFO: Record<AreaConhecimento, { rotulo: string; icone: LucideIcon; cor: string }> = {
  Tecnologia: { rotulo: 'Tecnologia', icone: Code2, cor: 'from-indigo-500 to-sky-500' },
  Administracao: { rotulo: 'Administração', icone: Briefcase, cor: 'from-slate-600 to-indigo-500' },
  Saude: { rotulo: 'Saúde', icone: HeartPulse, cor: 'from-rose-500 to-pink-500' },
  Gastronomia: { rotulo: 'Gastronomia', icone: ChefHat, cor: 'from-orange-500 to-amber-400' },
  ConstrucaoCivil: { rotulo: 'Construção civil', icone: HardHat, cor: 'from-amber-500 to-yellow-400' },
  Idiomas: { rotulo: 'Idiomas', icone: Languages, cor: 'from-teal-500 to-emerald-400' },
  Beleza: { rotulo: 'Beleza', icone: Scissors, cor: 'from-fuchsia-500 to-pink-400' },
  Logistica: { rotulo: 'Logística', icone: Truck, cor: 'from-cyan-600 to-blue-500' },
  Artes: { rotulo: 'Artes', icone: Palette, cor: 'from-violet-500 to-purple-400' },
  Empreendedorismo: { rotulo: 'Empreendedorismo', icone: Lightbulb, cor: 'from-emerald-500 to-lime-400' },
};

export const ESCOLARIDADE_ROTULO: Record<Escolaridade, string> = {
  FundamentalIncompleto: 'Fundamental incompleto',
  FundamentalCompleto: 'Fundamental completo',
  MedioIncompleto: 'Médio incompleto',
  MedioCompleto: 'Médio completo',
  Tecnico: 'Técnico',
  SuperiorIncompleto: 'Superior incompleto',
  SuperiorCompleto: 'Superior completo',
};

export const MODALIDADE_ROTULO: Record<Modalidade, string> = {
  Presencial: 'Presencial',
  Online: 'Online',
  Hibrida: 'Híbrida',
};

export const TIPO_OPORTUNIDADE_ROTULO: Record<TipoOportunidade, string> = {
  Curso: 'Curso',
  Oficina: 'Oficina',
  Mentoria: 'Mentoria',
  JovemAprendiz: 'Jovem aprendiz',
};

export const TIPO_ORGANIZACAO_ROTULO: Record<TipoOrganizacao, string> = {
  Ong: 'ONG',
  EscolaTecnica: 'Escola técnica',
  Empresa: 'Empresa',
  PoderPublico: 'Poder público',
  ColetivoComunitario: 'Coletivo comunitário',
};

export const PERFIL_ROTULO: Record<Perfil, string> = {
  Admin: 'Administração',
  Cidadao: 'Cidadão',
  Organizacao: 'Organização',
};

type Tom = 'brand' | 'green' | 'amber' | 'red' | 'gray' | 'sky';

export const STATUS_INSCRICAO_INFO: Record<StatusInscricao, { rotulo: string; tom: Tom }> = {
  Pendente: { rotulo: 'Pendente', tom: 'amber' },
  Confirmada: { rotulo: 'Confirmada', tom: 'sky' },
  Concluida: { rotulo: 'Concluída', tom: 'green' },
  Cancelada: { rotulo: 'Cancelada', tom: 'gray' },
};

export const STATUS_OPORTUNIDADE_INFO: Record<StatusOportunidade, { rotulo: string; tom: Tom }> = {
  Aberta: { rotulo: 'Inscrições abertas', tom: 'green' },
  Encerrada: { rotulo: 'Inscrições encerradas', tom: 'amber' },
  Cancelada: { rotulo: 'Cancelada', tom: 'red' },
};

export const STATUS_PROPOSTA_INFO: Record<StatusProposta, { rotulo: string; tom: Tom }> = {
  Aberta: { rotulo: 'Em votação', tom: 'brand' },
  Atendida: { rotulo: 'Atendida', tom: 'green' },
  Arquivada: { rotulo: 'Arquivada', tom: 'gray' },
};

/** Troca nomes de enum que aparecem em textos da API ("Administracao") pelo rótulo com acento. */
export function humanizarTexto(texto: string): string {
  return texto.replace(/\b(Administracao|Saude|ConstrucaoCivil|Logistica)\b/g, (m) => AREA_INFO[m as AreaConhecimento].rotulo);
}

export const BAIRROS_ZONA_SUL = ['Capão Redondo', 'Campo Limpo', 'Jardim Ângela'] as const;

export const GRADIENTE_ORGANIZACAO: Record<TipoOrganizacao, string> = {
  Ong: 'from-rose-500 to-orange-400',
  EscolaTecnica: 'from-brand-500 to-sky-500',
  Empresa: 'from-slate-700 to-slate-500',
  PoderPublico: 'from-emerald-600 to-teal-500',
  ColetivoComunitario: 'from-fuchsia-500 to-violet-500',
};
