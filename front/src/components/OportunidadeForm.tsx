import { useState, type FormEvent } from 'react';
import {
  AREAS,
  MODALIDADES,
  TIPOS_OPORTUNIDADE,
  type AreaConhecimento,
  type CriacaoOportunidade,
  type Modalidade,
  type Oportunidade,
  type TipoOportunidade,
} from '../api/types';
import { AREA_INFO, MODALIDADE_ROTULO, TIPO_OPORTUNIDADE_ROTULO } from '../lib/labels';
import { dataParaApi, dataParaInput } from '../lib/format';
import { ApiError } from '../api/client';
import { Input, Select, Textarea } from './ui/Field';
import { Button } from './ui/Button';

export interface ValoresOportunidade {
  titulo: string;
  descricao: string;
  tipo: TipoOportunidade;
  modalidade: Modalidade;
  area: AreaConhecimento;
  bairro: string;
  cargaHoraria: string;
  vagas: string;
  dataInicio: string;
  dataFim: string;
  gratuita: boolean;
}

function hojeMais(dias: number) {
  const d = new Date();
  d.setDate(d.getDate() + dias);
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

export function valoresIniciais(o?: Oportunidade, parcial?: Partial<ValoresOportunidade>): ValoresOportunidade {
  if (o) {
    return {
      titulo: o.titulo,
      descricao: o.descricao,
      tipo: o.tipo,
      modalidade: o.modalidade,
      area: o.area,
      bairro: o.bairro,
      cargaHoraria: String(o.cargaHoraria),
      vagas: String(o.vagasTotais),
      dataInicio: dataParaInput(o.dataInicio),
      dataFim: dataParaInput(o.dataFim),
      gratuita: o.gratuita,
    };
  }
  return {
    titulo: '',
    descricao: '',
    tipo: 'Curso',
    modalidade: 'Presencial',
    area: 'Tecnologia',
    bairro: '',
    cargaHoraria: '20',
    vagas: '20',
    dataInicio: hojeMais(14),
    dataFim: hojeMais(44),
    gratuita: true,
    ...parcial,
  };
}

export function validarOportunidade(v: ValoresOportunidade): Partial<Record<keyof ValoresOportunidade, string>> {
  const erros: Partial<Record<keyof ValoresOportunidade, string>> = {};
  if (!v.titulo.trim()) erros.titulo = 'Título é obrigatório';
  if (!v.descricao.trim()) erros.descricao = 'Descrição é obrigatória';
  if (!v.bairro.trim()) erros.bairro = 'Bairro é obrigatório';
  const carga = Number(v.cargaHoraria);
  if (!Number.isInteger(carga) || carga < 1 || carga > 2000) erros.cargaHoraria = 'Entre 1 e 2000 horas';
  const vagas = Number(v.vagas);
  if (!Number.isInteger(vagas) || vagas < 1 || vagas > 1000) erros.vagas = 'Entre 1 e 1000 vagas';
  if (!v.dataInicio) erros.dataInicio = 'Informe a data de início';
  if (!v.dataFim) erros.dataFim = 'Informe a data de término';
  else if (v.dataInicio && v.dataFim < v.dataInicio) erros.dataFim = 'O término não pode ser antes do início';
  return erros;
}

export function paraCriacao(v: ValoresOportunidade): CriacaoOportunidade {
  return {
    titulo: v.titulo.trim(),
    descricao: v.descricao.trim(),
    tipo: v.tipo,
    modalidade: v.modalidade,
    area: v.area,
    bairro: v.bairro.trim(),
    cargaHoraria: Number(v.cargaHoraria),
    vagas: Number(v.vagas),
    dataInicio: dataParaApi(v.dataInicio),
    dataFim: dataParaApi(v.dataFim),
    gratuita: v.gratuita,
  };
}

interface Props {
  inicial: ValoresOportunidade;
  edicao?: boolean;
  textoEnviar: string;
  aoEnviar: (v: ValoresOportunidade) => Promise<void>;
  aoCancelar?: () => void;
}

export function OportunidadeForm({ inicial, edicao, textoEnviar, aoEnviar, aoCancelar }: Props) {
  const [v, setV] = useState(inicial);
  const [erros, setErros] = useState<Partial<Record<keyof ValoresOportunidade, string>>>({});
  const [erroGeral, setErroGeral] = useState<string>();
  const [enviando, setEnviando] = useState(false);

  const mudar = <K extends keyof ValoresOportunidade>(k: K, valor: ValoresOportunidade[K]) => setV((a) => ({ ...a, [k]: valor }));

  async function enviar(e: FormEvent) {
    e.preventDefault();
    const encontrados = validarOportunidade(v);
    setErros(encontrados);
    setErroGeral(undefined);
    if (Object.keys(encontrados).length > 0) return;
    setEnviando(true);
    try {
      await aoEnviar(v);
    } catch (err) {
      setErroGeral(err instanceof ApiError || err instanceof Error ? err.message : 'Não foi possível salvar.');
    } finally {
      setEnviando(false);
    }
  }

  return (
    <form onSubmit={enviar} noValidate className="space-y-5">
      <Input rotulo="Título" value={v.titulo} maxLength={120} erro={erros.titulo} onChange={(e) => mudar('titulo', e.target.value)} required />
      <Textarea
        rotulo="Descrição"
        value={v.descricao}
        maxLength={1000}
        erro={erros.descricao}
        onChange={(e) => mudar('descricao', e.target.value)}
        placeholder="O que a pessoa vai aprender, pré-requisitos, local..."
        required
      />
      <div className="grid gap-4 sm:grid-cols-3">
        <Select rotulo="Tipo" value={v.tipo} disabled={edicao} onChange={(e) => mudar('tipo', e.target.value as TipoOportunidade)}>
          {TIPOS_OPORTUNIDADE.map((t) => (
            <option key={t} value={t}>
              {TIPO_OPORTUNIDADE_ROTULO[t]}
            </option>
          ))}
        </Select>
        <Select rotulo="Área" value={v.area} onChange={(e) => mudar('area', e.target.value as AreaConhecimento)}>
          {AREAS.map((a) => (
            <option key={a} value={a}>
              {AREA_INFO[a].rotulo}
            </option>
          ))}
        </Select>
        <Select rotulo="Modalidade" value={v.modalidade} onChange={(e) => mudar('modalidade', e.target.value as Modalidade)}>
          {MODALIDADES.map((m) => (
            <option key={m} value={m}>
              {MODALIDADE_ROTULO[m]}
            </option>
          ))}
        </Select>
      </div>
      <div className="grid gap-4 sm:grid-cols-3">
        <Input rotulo="Bairro" value={v.bairro} maxLength={80} erro={erros.bairro} onChange={(e) => mudar('bairro', e.target.value)} placeholder="Ex.: Capão Redondo" required />
        <Input rotulo="Carga horária (h)" type="number" min={1} max={2000} value={v.cargaHoraria} erro={erros.cargaHoraria} onChange={(e) => mudar('cargaHoraria', e.target.value)} required />
        <Input rotulo="Vagas" type="number" min={1} max={1000} value={v.vagas} erro={erros.vagas} onChange={(e) => mudar('vagas', e.target.value)} required />
      </div>
      <div className="grid gap-4 sm:grid-cols-2">
        <Input rotulo="Início" type="date" value={v.dataInicio} erro={erros.dataInicio} onChange={(e) => mudar('dataInicio', e.target.value)} required />
        <Input rotulo="Término" type="date" value={v.dataFim} min={v.dataInicio} erro={erros.dataFim} onChange={(e) => mudar('dataFim', e.target.value)} required />
      </div>
      {!edicao && (
        <label className="flex cursor-pointer items-center gap-3 rounded-xl border border-[var(--border)] p-4">
          <input type="checkbox" className="size-5 accent-brand-600" checked={v.gratuita} onChange={(e) => mudar('gratuita', e.target.checked)} />
          <span>
            <span className="block text-sm font-semibold">Oportunidade gratuita</span>
            <span className="muted block text-xs">Oportunidades gratuitas ganham prioridade nas recomendações.</span>
          </span>
        </label>
      )}
      {erroGeral && (
        <p role="alert" className="rounded-xl bg-rose-50 px-4 py-3 text-sm font-medium text-rose-700 dark:bg-rose-500/10 dark:text-rose-300">
          {erroGeral}
        </p>
      )}
      <div className="flex justify-end gap-2 pt-2">
        {aoCancelar && (
          <Button variante="secundario" onClick={aoCancelar}>
            Cancelar
          </Button>
        )}
        <Button type="submit" carregando={enviando}>
          {textoEnviar}
        </Button>
      </div>
    </form>
  );
}
