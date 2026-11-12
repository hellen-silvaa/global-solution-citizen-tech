import { describe, expect, it, vi } from 'vitest';
import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { useState } from 'react';
import { PasswordStrength } from './PasswordStrength';
import { AreasPicker } from './AreasPicker';
import { OportunidadeCard, VagasBarra } from './OportunidadeCard';
import { PropostaCard } from './PropostaCard';
import { OportunidadeForm, paraCriacao, validarOportunidade, valoresIniciais } from './OportunidadeForm';
import { GraficoDemandaReprimida, GraficoInscricoesPorBairro } from './ImpactCharts';
import type { AreaConhecimento } from '../api/types';
import { renderizar } from '../../test/utils';
import { impacto, oportunidade, proposta } from '../../test/fixtures';

describe('PasswordStrength', () => {
  it('não mostra nada sem senha', () => {
    renderizar(<PasswordStrength senha="" />);
    expect(screen.queryByText(/De 8 a 64/)).not.toBeInTheDocument();
  });

  it('lista requisitos atendidos e pendentes', () => {
    renderizar(<PasswordStrength senha="abcdefgh" />);
    expect(screen.getByText('Pelo menos um número').parentElement).toHaveTextContent('(pendente)');
    expect(screen.getByText('De 8 a 64 caracteres').parentElement).toHaveTextContent('(atendido)');
  });
});

describe('AreasPicker', () => {
  function Controlado() {
    const [v, setV] = useState<AreaConhecimento[]>([]);
    return <AreasPicker valor={v} aoMudar={setV} />;
  }
  it('marca e desmarca áreas', async () => {
    renderizar(<Controlado />);
    const saude = screen.getByRole('checkbox', { name: 'Saúde' });
    await userEvent.click(saude);
    expect(saude).toBeChecked();
    await userEvent.click(saude);
    expect(saude).not.toBeChecked();
  });
});

describe('OportunidadeCard', () => {
  it('mostra título, organização, vagas e motivos da recomendação', () => {
    renderizar(<OportunidadeCard oportunidade={oportunidade()} motivos={['Combina com seu interesse em Administracao']} pontuacao={6} />);
    expect(screen.getByRole('link', { name: 'Introdução à Programação Web' })).toHaveAttribute('href', '/oportunidades/1');
    expect(screen.getByText('Instituto Recomeçar')).toBeInTheDocument();
    expect(screen.getByText('12 vagas restantes')).toBeInTheDocument();
    expect(screen.getByText('Combina com seu interesse em Administração')).toBeInTheDocument();
    expect(screen.getByText('6 pts')).toBeInTheDocument();
    expect(screen.getByText('Gratuita')).toBeInTheDocument();
  });

  it('mostra o status quando não está aberta', () => {
    renderizar(<OportunidadeCard oportunidade={oportunidade({ status: 'Cancelada', gratuita: false, modalidade: 'Online' })} />);
    expect(screen.getByText('Cancelada')).toBeInTheDocument();
    expect(screen.getByText('Online')).toBeInTheDocument();
    expect(screen.queryByText('Gratuita')).not.toBeInTheDocument();
  });

  it('VagasBarra indica esgotado e última vaga', () => {
    const primeiro = renderizar(<VagasBarra disponiveis={0} totais={10} />);
    expect(screen.getByText('Vagas esgotadas')).toBeInTheDocument();
    primeiro.unmount();
    renderizar(<VagasBarra disponiveis={1} totais={10} />);
    expect(screen.getByText('1 vaga restante')).toBeInTheDocument();
    expect(screen.getByRole('progressbar')).toHaveAttribute('aria-valuenow', '9');
  });
});

describe('PropostaCard', () => {
  it('vota ao clicar quando pode', async () => {
    const aoVotar = vi.fn();
    renderizar(<PropostaCard proposta={proposta()} podeVotar aoVotar={aoVotar} posicao={1} />);
    await userEvent.click(screen.getByRole('button', { name: /Votar em Curso de Cuidador/ }));
    expect(aoVotar).toHaveBeenCalled();
    expect(screen.getByText('#1')).toBeInTheDocument();
  });

  it('bloqueia o voto se já votou ou se não pode', () => {
    const primeiro = renderizar(<PropostaCard proposta={proposta()} podeVotar jaVotou />);
    expect(screen.getByRole('button', { name: /já votou/ })).toBeDisabled();
    primeiro.unmount();
    renderizar(<PropostaCard proposta={proposta()} />);
    expect(screen.getByRole('button', { name: /Votar/ })).toBeDisabled();
  });

  it('linka para a oportunidade gerada quando atendida', () => {
    renderizar(<PropostaCard proposta={proposta({ status: 'Atendida', oportunidadeGeradaId: 4 })} />);
    expect(screen.getByRole('link', { name: /Ver oportunidade criada/ })).toHaveAttribute('href', '/oportunidades/4');
    expect(screen.getByText('Atendida')).toBeInTheDocument();
  });
});

describe('OportunidadeForm', () => {
  it('valida as mesmas regras da API', () => {
    const v = valoresIniciais(undefined, { titulo: '', cargaHoraria: '0', vagas: '1001', dataInicio: '2026-12-10', dataFim: '2026-12-01' });
    const erros = validarOportunidade(v);
    expect(erros.titulo).toBeTruthy();
    expect(erros.bairro).toBeTruthy();
    expect(erros.cargaHoraria).toBeTruthy();
    expect(erros.vagas).toBeTruthy();
    expect(erros.dataFim).toMatch(/antes do início/);
  });

  it('converte para o formato da API', () => {
    const dados = paraCriacao(valoresIniciais(undefined, { titulo: ' Oficina ', bairro: 'Campo Limpo', dataInicio: '2026-12-01', dataFim: '2026-12-02' }));
    expect(dados).toMatchObject({ titulo: 'Oficina', bairro: 'Campo Limpo', cargaHoraria: 20, vagas: 20, dataInicio: '2026-12-01T00:00:00', gratuita: true });
  });

  it('preenche a partir de uma oportunidade existente', () => {
    const v = valoresIniciais(oportunidade());
    expect(v).toMatchObject({ titulo: 'Introdução à Programação Web', vagas: '30', dataInicio: '2099-12-01' });
  });

  it('mostra erros e não envia com dados inválidos', async () => {
    const aoEnviar = vi.fn();
    renderizar(<OportunidadeForm inicial={valoresIniciais()} textoEnviar="Publicar" aoEnviar={aoEnviar} />);
    await userEvent.click(screen.getByRole('button', { name: 'Publicar' }));
    expect(aoEnviar).not.toHaveBeenCalled();
    expect(screen.getByText('Título é obrigatório')).toBeInTheDocument();
  });

  it('envia e mostra o erro devolvido pela API', async () => {
    const aoEnviar = vi.fn().mockRejectedValue(new Error('Data de início deve ser futura!'));
    renderizar(<OportunidadeForm inicial={valoresIniciais(undefined, { titulo: 'T', descricao: 'D', bairro: 'B' })} textoEnviar="Publicar" aoEnviar={aoEnviar} />);
    await userEvent.click(screen.getByRole('button', { name: 'Publicar' }));
    expect(aoEnviar).toHaveBeenCalledOnce();
    expect(await screen.findByRole('alert')).toHaveTextContent('Data de início deve ser futura!');
  });
});

describe('gráficos de impacto', () => {
  it('alternam para tabela acessível', async () => {
    renderizar(<GraficoInscricoesPorBairro dados={impacto.porBairro} />);
    await userEvent.click(screen.getByRole('button', { name: 'Tabela' }));
    const tabela = screen.getByRole('table');
    expect(within(tabela).getByRole('rowheader', { name: 'Capão Redondo' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Tabela' })).toHaveAttribute('aria-pressed', 'true');
  });

  it('demanda reprimida vazia mostra mensagem', () => {
    renderizar(<GraficoDemandaReprimida dados={[]} />);
    expect(screen.getByText(/Nenhuma demanda reprimida/)).toBeInTheDocument();
  });
});
